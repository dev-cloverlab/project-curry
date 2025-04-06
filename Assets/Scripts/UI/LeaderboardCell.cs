using curry.Utilities;
using TMPro;
using UnityEngine;

namespace curry.leaderboard
{
    public class LeaderboardCell : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_RankText;

        [SerializeField]
        private TextMeshProUGUI m_UserNameText;

        [SerializeField]
        private TextMeshProUGUI m_ScoreText;

        public void Setup(SteamLeaderboardData data)
        {
            m_RankText.SetText($"{data.Rank}");
            m_UserNameText.SetText($"{data.SteamUserName}");
            m_ScoreText.SetText($"{CurryUtility.GetScoreText(data.Score)}");
        }
    }
}
