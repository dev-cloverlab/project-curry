using System.Collections.Generic;
using System.Linq;
using curry.Sound;
using curry.UI;
using curry.Utilities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace curry.leaderboard
{
    public class Leaderboard : UIBase
    {
        [SerializeField]
        private LeaderboardView m_GlobalView;

        [SerializeField]
        private LeaderboardView m_AroundUserView;

        [SerializeField]
        private LeaderboardView m_FriendsView;

        [SerializeField]
        private LeaderboardCell m_CellPrefab;

        [SerializeField]
        private Toggle m_GlobalToggle;

        private SteamController.LeaderboardRequestType m_CurrentType;

        public void Setup(List<SteamLeaderboardData> globalDataList,
            List<SteamLeaderboardData> aroundUserDataList,
            List<SteamLeaderboardData> friendsDataList)
        {


            #if DEBUG_MODE

            if (false)
            {
                var first = globalDataList.FirstOrDefault();
                first.SetRank(10);
                first.SetScore(100);
                globalDataList.Clear();
                globalDataList.Add(first);
                first = aroundUserDataList.FirstOrDefault();
                first.SetRank(10);
                first.SetScore(100);
                aroundUserDataList.Clear();
                aroundUserDataList.Add(first);
                first = friendsDataList.FirstOrDefault();
                first.SetRank(10);
                first.SetScore(100);
                friendsDataList.Clear();
                friendsDataList.Add(first);


                globalDataList.Add(new SteamLeaderboardData(1, 2, "GlobalAAAAAAAAA", 9000));
                globalDataList.Add(new SteamLeaderboardData(2, 2, "GlobalB", 8908));
                globalDataList.Add(new SteamLeaderboardData(3, 2, "GlobalCCC", 8589));
                globalDataList.Add(new SteamLeaderboardData(4, 2, "GlobalDDDDD", 7123));
                globalDataList.Add(new SteamLeaderboardData(5, 2, "Glo", 5012));
                globalDataList.Add(new SteamLeaderboardData(6, 2, "G", 2057));
                globalDataList.Add(new SteamLeaderboardData(7, 2, "Globa", 800));
                globalDataList.Add(new SteamLeaderboardData(8, 2, "GlobalAabdas", 700));
                globalDataList.Add(new SteamLeaderboardData(9, 2, "Globalrhweahare", 600));
                globalDataList.Add(new SteamLeaderboardData(11, 2, "GlobalAAAAAdds", 98));
                globalDataList.Add(new SteamLeaderboardData(12, 2, "GlobalAAAAAAAAA", 97));
                globalDataList.Add(new SteamLeaderboardData(13, 2, "GlobalAAAAAAAAA", 96));
                globalDataList.Add(new SteamLeaderboardData(14, 2, "GlobalAAAAAAAAA", 95));
                globalDataList.Add(new SteamLeaderboardData(15, 2, "GlobalAAAAAAAAA", 94));
                globalDataList.Add(new SteamLeaderboardData(16, 2, "GlobalAAAAAAAAA", 93));
                globalDataList = globalDataList.OrderBy(x => x.Rank).ToList();

                aroundUserDataList.Add(new SteamLeaderboardData(1, 2, "AroundAAAA", 9000));
                aroundUserDataList.Add(new SteamLeaderboardData(2, 2, "AroundB", 8908));
                aroundUserDataList.Add(new SteamLeaderboardData(3, 2, "AroundCCC", 8589));
                aroundUserDataList.Add(new SteamLeaderboardData(4, 2, "AroundDDDDD", 7123));
                aroundUserDataList.Add(new SteamLeaderboardData(5, 2, "Around", 5012));
                aroundUserDataList.Add(new SteamLeaderboardData(6, 2, "Aroun", 2057));
                aroundUserDataList.Add(new SteamLeaderboardData(7, 2, "Aro", 800));
                aroundUserDataList.Add(new SteamLeaderboardData(8, 2, "Aroundbdas", 700));
                aroundUserDataList.Add(new SteamLeaderboardData(9, 2, "Aroundhweahare", 600));
                aroundUserDataList.Add(new SteamLeaderboardData(11, 2, "AroundAAAAAdds", 98));
                aroundUserDataList.Add(new SteamLeaderboardData(12, 2, "AroundAAAAAAAAA", 97));
                aroundUserDataList.Add(new SteamLeaderboardData(13, 2, "AroundAAAAAAAAA", 96));
                aroundUserDataList.Add(new SteamLeaderboardData(14, 2, "AroundAAAAAAAAA", 95));
                aroundUserDataList.Add(new SteamLeaderboardData(15, 2, "AroundAAAAAAAAA", 94));
                aroundUserDataList.Add(new SteamLeaderboardData(16, 2, "AroundAAAAAAAAA", 93));
                aroundUserDataList = aroundUserDataList.OrderBy(x => x.Rank).ToList();

                friendsDataList.Add(new SteamLeaderboardData(3, 2, "FriendCCC", 8589));
                friendsDataList.Add(new SteamLeaderboardData(8, 2, "FriendAabdas", 700));
                friendsDataList.Add(new SteamLeaderboardData(13, 2, "FriendlAAAAAAAAA", 96));
                friendsDataList.Add(new SteamLeaderboardData(15, 2, "FriendAAAAAAAAA", 94));
                friendsDataList = friendsDataList.OrderBy(x => x.Rank).ToList();
            }

            #endif



            m_GlobalView.Setup(globalDataList, m_CellPrefab);
            m_AroundUserView.Setup(aroundUserDataList, m_CellPrefab);
            m_FriendsView.Setup(friendsDataList, m_CellPrefab);

            m_CurrentType = SteamController.LeaderboardRequestType.Global;
            m_GlobalToggle.SetIsOnWithoutNotify(true);

            UnityUtility.SetActive(m_GlobalView, true);
            UnityUtility.SetActive(m_AroundUserView, false);
            UnityUtility.SetActive(m_FriendsView, false);
        }

        private void ChangeView(SteamController.LeaderboardRequestType leaderboardType)
        {
            if (m_CurrentType == leaderboardType)
            {
                return;
            }

            SEPlayer.PlaySelectSE();
            m_CurrentType = leaderboardType;

            SetupViews();
        }

        private void SetupViews()
        {
            UnityUtility.SetActive(m_GlobalView, m_CurrentType == SteamController.LeaderboardRequestType.Global);
            UnityUtility.SetActive(m_AroundUserView, m_CurrentType == SteamController.LeaderboardRequestType.AroundUser);
            UnityUtility.SetActive(m_FriendsView, m_CurrentType == SteamController.LeaderboardRequestType.Friends);
        }

        private void Update()
        {
            if (!IsActivate)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

#region Unity Inspector

        public void GlobalToggle(bool isOn)
        {
            if (isOn)
            {
                ChangeView(SteamController.LeaderboardRequestType.Global);
            }
        }

        public void AroundUserToggle(bool isOn)
        {
            if (isOn)
            {
                ChangeView(SteamController.LeaderboardRequestType.AroundUser);
            }
        }

        public void FriendsToggle(bool isOn)
        {
            if (isOn)
            {
                ChangeView(SteamController.LeaderboardRequestType.Friends);
            }
        }

        public void Close()
        {
            SEPlayer.PlaySelectSE();
            Activate(false);
            CloseAnimation();
        }

#endregion Unity Inspector

    }
}
