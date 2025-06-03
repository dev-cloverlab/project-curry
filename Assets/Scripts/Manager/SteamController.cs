#if STEAMWORKS_NET
using Steamworks;
#endif
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Debug = UnityEngine.Debug;


/*
 * SteamApi操作用クラス
 * 呼び出し側がSTEAMWORKS名前空間の有無を意識せずに使えるようにしています
 * STEAMWORKS名前空間がDefineSymbolに定義されていない場合、どの関数も成功扱いですぐに処理をすぐ終わらせます
 * その際ランキング等は空のデータを返します
 */
public static class SteamController
{
    /// <summary>
    /// リーダーボード表示タイプ
    /// </summary>
    public enum LeaderboardRequestType
    {
        /// トップランキング
        Global,
        /// 自分周辺ランキング
        AroundUser,
        /// フレンドのランキング
        Friends,
    }

    private static string GetString(this LeaderboardRequestType leaderboardRequestType)
    {
        return leaderboardRequestType switch
        {
            LeaderboardRequestType.Global => "Global",
            LeaderboardRequestType.AroundUser => "AroundUser",
            LeaderboardRequestType.Friends => "Friends",
            _ => string.Empty
        };
    }

    /// <summary>
    /// ログ出力をやめたい場合はここを弄ってください
    /// </summary>
    public static bool kEnableLog = false;

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

        try
        {
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
                LogWarning("SteamAPI Init 既に初期化済み");
                onAlreadyInitialized?.Invoke();
            }
        }
        catch (Exception e)
        {
            // エラーは強制表示
            Debug.LogException(e);
            onFailure?.Invoke();
        }

#else

        onSuccess?.Invoke();

#endif // STEAMWORKS_NET

    }

    private static async UniTask UserInit()
    {

#if STEAMWORKS_NET

        try
        {
            while (!SteamUser.BLoggedOn())
            {
                SteamAPI.RunCallbacks();
                await UniTask.Yield();
            }
        }
        catch (Exception e)
        {
            // エラーは強制表示
            Debug.LogException(e);
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

        try
        {
            if (!SteamManager.Initialized)
            {
                LogAssertion("SteamManager初期化が終わっていません。SteamAPI.Init()を先に呼んでください。");
                return true;
            }
        }
        catch (Exception e)
        {
            // エラーは強制表示
            Debug.LogException(e);
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
    public static async UniTask InitializeLeaderboards(string leaderboardName, UnityAction onSuccess = null, UnityAction onFailure = null)
    {

#if STEAMWORKS_NET

        try
        {
            if (IsInitErrorWithMessage())
            {
                onFailure?.Invoke();
                return;
            }

            if (!SteamManager.Initialized)
            {
                onFailure?.Invoke();
                LogAssertion("SteamManager初期化が終わっていません。SteamAPI.Init()を先に呼んでください。");
                return;
            }

            var isEnd = false;

            ExecuteLeaderboard(leaderboardName, (result) =>
                {
                    var leaderboard = result.m_hSteamLeaderboard;

                    Log($"<color=cyan>リーダーボード取得成功</color> " +
                        $"リーダーボードの名前 : {SteamUserStats.GetLeaderboardName(leaderboard)}     " +
                        $"リーダーボードに登録されている数 : {SteamUserStats.GetLeaderboardEntryCount(leaderboard)}     " +
                        $"リーダーボードの並べ替え方法 : {SteamUserStats.GetLeaderboardSortMethod(leaderboard)}     " +
                        $"リーダーボードの表示タイプ : {SteamUserStats.GetLeaderboardDisplayType(leaderboard)}"
                    );

                },
                () =>
                {
                    onSuccess?.Invoke();
                    isEnd = true;
                },
                () =>
                {
                    onFailure?.Invoke();
                    isEnd = true;
                });

            await UniTask.WaitWhile(() => !isEnd);
        }
        catch (Exception e)
        {
            // エラーは強制表示
            Debug.LogException(e);
            onFailure?.Invoke();
        }

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
    /// <param name="scoreDetailCount">scoreDetails内の要素の数。 k_cLeaderboardDetailsMaxを超えてはなりません。https://partner.steamgames.com/doc/api/ISteamUserStats#UploadLeaderboardScore </param>
    /// <param name="onSuccess">成功時のコールバック</param>
    /// <param name="onFailure">失敗時のコールバック</param>
    public static async UniTask UploadLeaderboardScore(string leaderboardName,
        int score,
        int[] scoreDetail = null,
        int scoreDetailCount = 0,
        UnityAction onSuccess = null,
        UnityAction onFailure = null)
    {

#if STEAMWORKS_NET

        try
        {
            if (IsInitErrorWithMessage())
            {
                onFailure?.Invoke();
                return;
            }

            var isEnd = false;

            ExecuteLeaderboard(leaderboardName, (result) =>
                {
                    var call = SteamUserStats.UploadLeaderboardScore(
                        result.m_hSteamLeaderboard,
                        ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
                        score,
                        scoreDetail,
                        scoreDetailCount
                    );

                    Log($"<color=green>スコア送信</color> " +
                        $"リーダーボードの名前 : {leaderboardName}     " +
                        $"スコア : {score}     " +
                        $"スコア詳細数 : {scoreDetailCount}");

                    if (scoreDetail?.Length > 0)
                    {
                        for (int i = 0; i < scoreDetail.Length; i++)
                        {
                            Log($"スコア詳細{i} : {scoreDetail[i]}");
                        }
                    }

                    CallResult<LeaderboardScoreUploaded_t>.Create().Set(call, (result, failure) =>
                    {
                        OnUploadScore(result, failure,
                            () =>
                            {
                                onSuccess?.Invoke();
                                isEnd = true;
                            }, () =>
                            {
                                onFailure?.Invoke();
                                isEnd = true;
                            });
                    });
                },
                onFailure: () =>
                {
                    onFailure?.Invoke();
                    isEnd = true;
                });

            await UniTask.WaitWhile(() => !isEnd);
        }
        catch (Exception e)
        {
            // エラーは強制表示
            Debug.LogException(e);
            onFailure?.Invoke();
        }

#else

        onSuccess?.Invoke();

#endif // STEAMWORKS_NET

    }


#if STEAMWORKS_NET

    /// <summary>
    /// スコア送信結果
    /// </summary>
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

#endif // STEAMWORKS_NET

    /// <summary>
    /// ランキング一覧を取得
    /// </summary>
    /// <note>
    /// APIには一定時間中の取得上限があるので、開くたびに取得しなおし続けると通信が失敗することがあります
    /// 結果は呼び出し側でキャッシュしておき、一定時間経過までAPIを呼び出さないようにしてください
    /// </note>
    /// <param name="leaderboardName">リーダーボード名</param>
    /// <param name="rangeStartIdx">取得する順位の一番上(インデクスなので0スタート)</param>
    /// <param name="rangeEndIdx">取得する順位の一番下(インデクスなので0スタート)</param>
    /// <param name="requestType">トップか自分周辺かフレンドかなどのタイプ https://partner.steamgames.com/doc/api/ISteamUserStats#ELeaderboardDataRequest </param>
    /// <param name="onSuccess"></param>
    /// <param name="onFailure"></param>
    /// <returns>ランキングデータ</returns>
    public static async UniTask<List<SteamLeaderboardData>> GetRankingList(string leaderboardName,
        int rangeStartIdx,
        int rangeEndIdx,
        LeaderboardRequestType requestType,
        UnityAction onSuccess = null,
        UnityAction onFailure = null)
    {

#if STEAMWORKS_NET

        try
        {
            if (IsInitErrorWithMessage())
            {
                onFailure?.Invoke();
                return new List<SteamLeaderboardData>();
            }

            List<SteamLeaderboardData> list = null;

            var isEnd = false;

            ELeaderboardDataRequest eRequestType;

            switch (requestType)
            {
                default:
                case LeaderboardRequestType.Global:
                    eRequestType = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;
                    break;
                case LeaderboardRequestType.AroundUser:
                    eRequestType = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser;
                    break;
                case LeaderboardRequestType.Friends:
                    eRequestType = ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends;
                    break;
            }

            ExecuteLeaderboard(leaderboardName, (result) =>
                {
                    var call = SteamUserStats.DownloadLeaderboardEntries(
                        result.m_hSteamLeaderboard,
                        eRequestType,
                        rangeStartIdx,
                        rangeEndIdx
                    );


                    CallResult<LeaderboardScoresDownloaded_t>.Create().Set(call, (result, failure) =>
                        OnReceivedRanking(result, failure, requestType, (rankingResult) => { list = rankingResult; },
                            () =>
                            {
                                onSuccess?.Invoke();
                                isEnd = true;
                            },
                            () =>
                            {
                                onFailure?.Invoke();
                                isEnd = true;
                            }));
                },
                onFailure: () =>
                {
                    onFailure?.Invoke();
                    isEnd = true;
                });

            await UniTask.WaitWhile(() => !isEnd);

            return list ?? new List<SteamLeaderboardData>();
        }
        catch (Exception e)
        {
            // エラーは強制表示
            Debug.LogException(e);
            onFailure?.Invoke();
            return new List<SteamLeaderboardData>();
        }

#else

        onSuccess?.Invoke();
        return new List<SteamLeaderboardData>();

#endif // STEAMWORKS_NET
    }

#if STEAMWORKS_NET

    /// <summary>
    /// リーダーボード取得結果
    /// </summary>
    private static void OnReceivedRanking(LeaderboardScoresDownloaded_t result,
        bool failure,
        LeaderboardRequestType requestType,
        UnityAction<List<SteamLeaderboardData>> setResultAction,
        UnityAction onSuccess = null,
        UnityAction onFailure = null)
    {
        try
        {
            if (failure)
            {
                LogError("ランキング取得に失敗しました");
                setResultAction?.Invoke(new List<SteamLeaderboardData>());
                onFailure?.Invoke();
                return;
            }

            var list = new List<SteamLeaderboardData>();

            Log($"<color=cyan>:{requestType.GetString()}:ランキング取得に成功しました</color>");
            for (var i = 0; i < result.m_cEntryCount; i++)
            {
                // FIXME pDetailsとcDetailsMax引数の扱いがよくわかってません…(スコアのみであれば不要なのでこのまま使えます)
                SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, out var leaderboardEntry, new int[0], 0);

                var rank = leaderboardEntry.m_nGlobalRank;
                var steamUserId = leaderboardEntry.m_steamIDUser.m_SteamID;
                var steamUserName = SteamFriends.GetFriendPersonaName(leaderboardEntry.m_steamIDUser);
                var score = leaderboardEntry.m_nScore;

                list.Add(new SteamLeaderboardData(rank, steamUserId, steamUserName, score));

                Log($"順位 : {rank}     " +
                    $"SteamユーザーID : {steamUserId}     " +
                    $"Steamユーザー名 : {steamUserName}     " +
                    $"スコア : {score}     "
                );
            }

            setResultAction?.Invoke(list);
            onSuccess?.Invoke();
        }
        catch (Exception e)
        {
            // エラーは強制表示
            Debug.LogException(e);
            onFailure?.Invoke();
        }
    }

#endif // STEAMWORKS_NET

#if STEAMWORKS_NET

    /// <summary>
    /// リーダーボード処理の実行
    /// </summary>
    private static void ExecuteLeaderboard(string leaderBoardName,
        UnityAction<LeaderboardFindResult_t> action,
        UnityAction onSuccess = null,
        UnityAction onFailure = null)
    {
        try
        {
            CallResult<LeaderboardFindResult_t>.Create().Set(SteamUserStats.FindLeaderboard(leaderBoardName), (result, failure) =>
            {
                if (failure || result.m_bLeaderboardFound <= 0)
                {
                    LogAssertion($"リーダーボードが見つかりません {leaderBoardName}");
                    onFailure?.Invoke();
                    return;
                }

                action?.Invoke(result);

                onSuccess?.Invoke();
            });
        }
        catch (Exception e)
        {
            // エラーは強制表示
            Debug.LogException(e);
            onFailure?.Invoke();
        }
    }

#endif // STEAMWORKS_NET

#endregion Leaderboards

    // ログ出力   ※ログはUnityEditorか、DevelopmentBuildのアプリでしか出ないようにしています
#region Log

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    private static void Log(string message)
    {
        if (!kEnableLog)
        {
            return;
        }

        Debug.Log(message);
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    private static void LogWarning(string message)
    {
        if (!kEnableLog)
        {
            return;
        }

        Debug.LogWarning(message);
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    private static void LogError(string message)
    {
        if (!kEnableLog)
        {
            return;
        }

        Debug.LogError(message);
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    private static void LogAssertion(string message)
    {
        if (!kEnableLog)
        {
            return;
        }

        Debug.LogAssertion(message);
    }

#endregion Log
}