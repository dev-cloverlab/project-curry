#if STEAMWORKS_NET
using Steamworks;
#endif
using UnityEngine.Events;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Debug = UnityEngine.Debug;

public static class SteamController
{
    // スコアランキング用
    public const string kLeaderboardScoreRankingName = "curry_leaderboard_6";

    // SteamAPI初期化処理
#region SteamManagerInitialize

    /// <summary>
    /// SteamManagerの初期化
    /// </summary>
    public static async UniTask SteamInit(UnityAction onSuccess = null,
        UnityAction onFailure = null,
        UnityAction onAlreadyInitialized = null)
    {

#if STEAMWORKS_NET

        if (!SteamManager.Initialized)
        {
            if (SteamAPI.Init())
            {
                await UserInit();
                Log("SteamAPI Init 成功");
                onSuccess?.Invoke();
            }
            else
            {
                LogError("SteamAPI Init エラー");
                onFailure?.Invoke();
            }
        }
        else
        {
            LogWarining("SteamAPI Init 既に初期化済み");
            onAlreadyInitialized?.Invoke();
        }
#else

        onSuccess?.Invoke();

#endif // STEAMWORKS_NET

    }

    private static async UniTask UserInit()
    {

#if STEAMWORKS_NET

        while (!SteamUser.BLoggedOn())
        {
            SteamAPI.RunCallbacks();
            await UniTask.Yield();
        }

#endif // STEAMWORKS_NET

    }


    /// <summary>
    /// SteamManagerが初期化完了しているか、完了していなければエラーが出ます。
    /// </summary>
    /// <returns> true エラー </returns>
    private static bool IsInitErrorWithMessage()
    {

#if STEAMWORKS_NET

        if (!SteamManager.Initialized)
        {
            LogAssertion("SteamManager初期化が終わっていません。SteamAPI.Init()を先に呼んでください。");
            return true;
        }

        return false;

#else

        return false;

#endif // STEAMWORKS_NET

    }

#endregion SteamManagerInitialize

    // リーダーボード機能
#region Leaderboards

    /// <summary>
    /// リーダーボードを最新の状態に更新
    /// </summary>
    /// <param name="leaderboardName">該当のリーダーボード名</param>
    /// <param name="onSuccess">成功時のコールバック</param>
    /// <param name="onFailure">失敗時のコールバック</param>
    public static void InitializeLeaderboards(string leaderboardName, UnityAction onSuccess = null, UnityAction onFailure = null)
    {

#if STEAMWORKS_NET

        if (IsInitErrorWithMessage()) return;

        if (!SteamManager.Initialized)
        {
            LogAssertion("SteamManager初期化が終わっていません。SteamAPI.Init()を先に呼んでください。");
            return;
        }

        ExecuteLeaderboard(leaderboardName, (leaderboard) =>
        {
            Log($"<color=cyan> リーダーボード取得成功 </color>\n" +
                $"リーダーボードの名前 : {SteamUserStats.GetLeaderboardName(leaderboard)}\n" +
                $"リーダーボードに登録されている数 : {SteamUserStats.GetLeaderboardEntryCount(leaderboard)}\n" +
                $"リーダーボードの並べ替え方法 : {SteamUserStats.GetLeaderboardSortMethod(leaderboard)}\n" +
                $"リーダーボードの表示タイプ : {SteamUserStats.GetLeaderboardDisplayType(leaderboard)}\n"
                );
        },
        onSuccess,
        onFailure);

#else

        onSuccess?.Invoke();

#endif // STEAMWORKS_NET

    }

    /// <summary>
    /// リーダーボードスコア送信
    /// ※ 10分で10回までしか呼べないので開発環境でチェックのために何度も呼ぶ場合は気を付けてください。
    /// </summary>
    /// <param name="leaderboardName">該当のリーダーボード名</param>
    /// <param name="score">スコア</param>
    /// <param name="scoreDetail">オプション：このスコアのロック解除に関する詳細を含む配列。</param>
    /// <param name="scoreDetailCount">scoreDetails内の要素の数。 k_cLeaderboardDetailsMaxを超えてはなりません。</param>
    /// <note> https://partner.steamgames.com/doc/api/ISteamUserStats#UploadLeaderboardScore </note>
    /// <param name="onSuccess">成功時のコールバック</param>
    /// <param name="onFailure">失敗時のコールバック</param>
    public static void UploadLeaderboard(string leaderboardName,
        int score,
        int[] scoreDetail = null,
        int scoreDetailCount = 0,
        UnityAction onSuccess = null,
        UnityAction onFailure = null)
    {

#if STEAMWORKS_NET

        if (IsInitErrorWithMessage()) return;

        ExecuteLeaderboard(leaderboardName, (leaderboard) =>
        {
            var call = SteamUserStats.UploadLeaderboardScore(
                leaderboard,
                ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
                score,
                scoreDetail,
                scoreDetailCount
            );

            Log($"<color=green>スコア送信</color>\n" +
                $"リーダーボードの名前 : {leaderboardName}\n" +
                $"スコア : {score}\n" +
                $"スコア詳細数 : {scoreDetailCount}");

            if (scoreDetail?.Length > 0)
            {
                for (int i = 0; i < scoreDetail.Length; i++)
                {
                    Log($"スコア詳細{i} : {scoreDetail[i]}");
                }
            }

            CallResult<LeaderboardScoreUploaded_t>.Create().Set(call, (result, failure) => OnUploadScore(result, failure, onSuccess, onFailure));
        });

#else

        onSuccess?.Invoke();

#endif // STEAMWORKS_NET

    }

#if STEAMWORKS_NET

    private static void OnUploadScore(LeaderboardScoreUploaded_t result,
        bool failure,
        UnityAction onSuccess = null,
        UnityAction onFailure = null)
    {


        if (failure || result.m_bSuccess != 1)
        {
            LogError("スコア送信に失敗しました");
            onFailure?.Invoke();
            return;
        }

        Log("<color=cyan>スコア送信に成功しました</color>");
        onSuccess?.Invoke();
    }

    /// <summary>
    /// リーダーボード処理の実行
    /// </summary>
    private static void ExecuteLeaderboard(string leaderBoardName,
        UnityAction<SteamLeaderboard_t> action,
        UnityAction onSuccess = null,
        UnityAction onFailure = null)
    {


        CallResult<LeaderboardFindResult_t>.Create().Set(SteamUserStats.FindLeaderboard(leaderBoardName), (result, failure) =>
        {
            if (failure || result.m_bLeaderboardFound <= 0)
            {
                LogAssertion($"リーダーボードが見つかりません {leaderBoardName}");
                onFailure?.Invoke();
                return;
            }

            var leaderboard = result.m_hSteamLeaderboard;

            action?.Invoke(leaderboard);

            onSuccess?.Invoke();
        });
    }

#endif // STEAMWORKS_NET

#endregion Leaderboards

    // ログ出力
    // ※ ログはUnityEditorか、DevelopmentBuildのアプリでしか出ないようにしています
#region Log

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    private static void Log(string message)
    {
        Debug.Log(message);
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    private static void LogWarining(string message)
    {
        Debug.LogWarning(message);
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    private static void LogError(string message)
    {
        Debug.LogError(message);
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    private static void LogAssertion(string message)
    {
        Debug.LogAssertion(message);
    }

#endregion Log
}
