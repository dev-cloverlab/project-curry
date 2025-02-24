using curry.Common;
using UnityEngine;
using curry.UserData;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

/// <summary>
/// セーブデータ管理
/// </summary>
public class SaveLoadManager : SingletonMonoBehaviour<SaveLoadManager>
{
    private const float kDisplayMinimunTime = 0.5f; /// 画面表示最低時間(秒)

    /// <summary>
    /// 表示タイプ
    /// </summary>
    public enum DisplayKind
    {
        None,  // 定義なし
        Save,  // セーブ
        Load,  // ロード
    }

    private ISaveSystem m_System = null;          // セーブ処理実体

    private bool        m_IsProcessing  = false;  // 処理中か？
    private DisplayKind m_DisplayKind = DisplayKind.None;            // 現在の表示モード
    private bool        m_IsForceDisplay= false;  // 強制表示中か？

    [Header("セーブObject"),SerializeField]
    private GameObject      m_SaveObject;
    [Header("ロードObject"),SerializeField]
    private GameObject      m_LoadObject;

    /// <summary>
    /// Instanceの生成
    /// </summary>
    public new static void Create()
    {
        if (IsInstance())
        {
            return;
        }

        // 必要なUIも合わせて作成
        var prefab = HoldAssetLoadManager.Instance.LoadPrefab(AssetPath.kSaveLoadManagerPrefab);
        var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        instance.name = "SaveLoadManager";
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private new void Awake()
    {
        base.Awake();

        if (!IsThisInstance())
        {
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        m_System = new PcSaveSystem();
#elif UNITY_SWITCH
        m_System = new SwitchSaveSystem();
#endif
        DontDestroyOnLoad();

        DebugLogWrapper.Log("[SaveLoadManager]初期化完了");
    }

    private void Update()
    {
        m_System?.Update();
    }

    /// <summary>
    /// 画面表示OFFにする
    /// </summary>
    /// <param name="sec"></param>
    /// <returns></returns>
    private async UniTask DisableDisplayAsync( float sec )
    {
        // セーブにかかった時間が短ければウェイトを設ける
        if( kDisplayMinimunTime > sec ) {
            float waitTime = kDisplayMinimunTime - sec;
            await UniTask.WaitForSeconds( waitTime, true );
        }
        SetDisplay( DisplayKind.None );
    }


    /// <summary>
    /// 表示制御
    /// </summary>
    /// <param name="kind"></param>
    private void SetDisplay( DisplayKind kind )
    {
        if( m_IsForceDisplay ) {
            return; // 強制表示中なので制御しない
        }

        switch( kind )
        {
        case DisplayKind.Load:
            m_LoadObject.SetActive( true );
            m_SaveObject.SetActive( false );
            break;
        case DisplayKind.Save:
            m_LoadObject.SetActive( false );
            m_SaveObject.SetActive( true );
            break;
        default:
            m_LoadObject.SetActive( false );
            m_SaveObject.SetActive( false );
            break;
        }
        m_DisplayKind = kind;
    }

    /// <summary>
    /// 解放
    /// </summary>
    protected override void OnDestroy()
    {
        if( null != m_System ) {
            m_System.Release();
        }
        m_System = null;

        base.OnDestroy();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public async UniTask InitializeAsync()
    {
        if( null == m_System ) {
            DebugLogWrapper.Log("[SaveLoadManager]初期化されていないため処理しない");
            return;
        }
        if( IsExist() ) {
            await Load();
        }
    }

    /// <summary>
    /// セーブが存在するか？
    /// </summary>
    /// <returns></returns>
    public bool IsExist()
    {
        if( null == m_System ) {
            DebugLogWrapper.LogError("[SaveLoadManager.Save]Instanceがありません");
        }
        return m_System.IsExistFile();
    }

    /// <summary>
    /// セーブ
    /// </summary>
    /// <returns>true : 処理成功</returns>
    public async UniTask<bool> Save()
    {
        if( null == m_System ) {
            DebugLogWrapper.LogError("[SaveLoadManager.Save]Instanceがありません");
        }
        if( m_IsProcessing || m_System.IsProcessing ) {
            return false;
        }

        m_IsProcessing = true;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        SetDisplay( DisplayKind.Save );
        bool result = await m_System.Save<SaveData>( UserDataManager.Instance.Data );

        sw.Stop();
        await DisableDisplayAsync( (float)sw.Elapsed.TotalSeconds );

        m_IsProcessing = false;
        return result;
    }

    /// <summary>
    /// ロード
    /// </summary>
    /// <returns>true : 処理成功</returns>
    public async UniTask<bool> Load()
    {
        if( null == m_System ) {
            DebugLogWrapper.LogError("[SaveLoadManager.Load]Instanceがありません");
        }

        if( m_IsProcessing || m_System.IsProcessing ) {
            return false;
        }
        if( !m_System.IsExistFile() )
        {
            DebugLogWrapper.LogWarning("[SaveLoadManager.Load]ファイルがありません");
            return false;
        }

        m_IsProcessing = true;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        SetDisplay( DisplayKind.Load );
        SaveData load = null;
        try {
            // ファイル読み込みは別Treadで処理
            //await using (UniTask.ReturnToMainThread())
            //{
                //await UniTask.SwitchToThreadPool();
                load = m_System.Read<SaveData>();
            //}

        } catch( System.Exception e ) {

            Debug.LogException(e);
            DebugLogWrapper.LogError( $"{e.Message}" );
        }

        if( null != load ) {
            UserDataManager.Instance.Set( load );
        }

        sw.Stop();
        await DisableDisplayAsync( (float)sw.Elapsed.TotalSeconds );

        m_IsProcessing = false;
        return true;
    }

    /// <summary>
    /// 処理中の表示を強制的に設定にする
    /// 処理の実行にかかわらず表示をさせたい場合に使用します
    /// この関数で表示を有効にした場合は必ずDisableFourceDisplayで無効化してください
    /// </summary>
    /// <param name="kind">表示種類</param>
    public void EnableFourceDisplay( DisplayKind kind )
    {
        SetDisplay( kind );
        m_IsForceDisplay = true;
    }

    /// <summary>
    /// 処理中の表示を強制的に非表示にする
    /// EnableFourceDisplayで強制表示をした場合は必ずこの関数で非表示にしてください
    /// </summary>
    /// <param name="kind">表示種類</param>
    public void DisableFourceDisplay()
    {
        m_IsForceDisplay = false;
        SetDisplay( DisplayKind.None );
    }
}
