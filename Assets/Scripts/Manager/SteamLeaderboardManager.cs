using System.Collections.Generic;
using System.Linq;
using curry.Common;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace curry.Ranking
{
    public class CurryLeaderBoardManager : SingletonMonoBehaviour<CurryLeaderBoardManager>
    {
        // スコアランキング用
        public const string kLeaderboardScoreRankingName = "curry_leaderboard_7";

        private const int kGlobalRangeStart = 0;
        private const int kGlobalRangeEnd = 20;

        private const int kAroundRangeStart = -10;
        private const int kAroundRangeEnd = 10;

        private List<SteamLeaderboardData> m_GlobalDataList = new List<SteamLeaderboardData>();
        public List<SteamLeaderboardData> GlobalDataList => m_GlobalDataList;

        private List<SteamLeaderboardData> m_GlobalAroundUserDataList = new List<SteamLeaderboardData>();
        public List<SteamLeaderboardData> GlobalAroundUserDataList => m_GlobalAroundUserDataList;

        private List<SteamLeaderboardData> m_FriendDataList = new List<SteamLeaderboardData>();
        public List<SteamLeaderboardData> FriendDataList => m_FriendDataList;

        private SteamLeaderboardData m_MyData = new();
        public SteamLeaderboardData MyData => m_MyData;

        private float CachedTime { get; set; } = float.MinValue;
        // 3分後に再取得可能
        private const float NextCacheTimeDelay = 180f;

        protected new void Awake()
        {
            base.Awake();

            if (!IsThisInstance())
            {
                return;
            }

            DontDestroyOnLoad();
        }

        // 初期化
        public async UniTask Init()
        {
            // リーダーボード初期化
            await SteamController.InitializeLeaderboards(kLeaderboardScoreRankingName);
            await SetupLeaderboardDataList(true);
        }

        // ランキングデータ取得
        public async UniTask SetupLeaderboardDataList(bool force = false)
        {
            // 前回取得から一定時間経過していなければスキップ
            if (!force && !IsElapsed())
            {
                return;
            }

#if STEAMWORKS_NET

            if (!SteamManager.Initialized)
            {
                Debug.LogWarning("Steamが初期化されていないため、ランキングデータを取得できません");
                return;
            }

#endif // STEAMWORKS_NET


            CachedTime = Time.time;

            try
            {
                var globalTask = SteamController.GetRankingList(kLeaderboardScoreRankingName, kGlobalRangeStart, kGlobalRangeEnd, SteamController.LeaderboardRequestType.Global);
                var aroundTask = SteamController.GetRankingList(kLeaderboardScoreRankingName, kAroundRangeStart, kAroundRangeEnd, SteamController.LeaderboardRequestType.AroundUser);
                var friendTask = SteamController.GetRankingList(kLeaderboardScoreRankingName, 0, 0, SteamController.LeaderboardRequestType.Friends);

                var (globalResults, aroundResults, friendResults) = await UniTask.WhenAll(globalTask, aroundTask, friendTask);

                m_GlobalDataList = globalResults.OrderBy(x => x.Rank).ToList();
                m_GlobalAroundUserDataList = aroundResults.OrderBy(x => x.Rank).ToList();
                m_FriendDataList = friendResults.OrderBy(x => x.Rank).ToList();

                m_MyData = FindMyData(m_GlobalAroundUserDataList);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ランキングデータ取得に失敗: {e.Message}");
                CachedTime = Time.time - NextCacheTimeDelay - 5f; // 少し早めに再試行できるようにする
            }
        }

        public async UniTask UploadScore(int score)
        {
            var myScore = MyData.Score;
            if (score <= myScore)
            {
                return;
            }

            try
            {
                await SteamController.UploadLeaderboardScore(kLeaderboardScoreRankingName, score);
                await SetupLeaderboardDataList(true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"スコア更新に失敗: {e.Message}");
            }
        }

        private SteamLeaderboardData FindMyData(List<SteamLeaderboardData> dataList)
        {
            try
            {
                var mySteamId = SteamUser.GetSteamID();
                var data = dataList.FirstOrDefault(data => data.SteamUserId == mySteamId.m_SteamID);

                // データが見つからない場合のフォールバック
                if (data.Equals(default(SteamLeaderboardData)))
                {
                    return new SteamLeaderboardData(0, mySteamId.m_SteamID, SteamFriends.GetPersonaName(), 0);
                }

                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"自分のデータ取得に失敗: {e.Message}");
                return new SteamLeaderboardData(); // デフォルト値を返す
            }
        }

        private bool IsElapsed()
        {
            var now = Time.time;

            // 一定時間後に再取得可能
            return now >= CachedTime + NextCacheTimeDelay;
        }
    }
}
