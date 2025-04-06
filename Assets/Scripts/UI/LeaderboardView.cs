using System.Collections.Generic;
using curry.Utilities;
using UnityEngine;

namespace curry.leaderboard
{
    public class LeaderboardView : MonoBehaviour
    {
        [SerializeField]
        private Transform m_CellParent;

        private List<LeaderboardCell> m_BoardCellList = new();

        public void Setup(List<SteamLeaderboardData> dataList, LeaderboardCell cellPrefab)
        {
            // データリストが無効ならエラー
            if (dataList == null)
            {
                DebugLogWrapper.LogAssertion("ランキングデータリストがnull");
                return;
            }

            // 足りないセルを新規作成
            if (m_BoardCellList.Count < dataList.Count)
            {
                if (cellPrefab == null)
                {
                    DebugLogWrapper.LogAssertion("セルのプレハブが設定されていない。");
                    return;
                }

                // 必要な数だけ新しいセルを作成
                for (int i = m_BoardCellList.Count; i < dataList.Count; i++)
                {
                    LeaderboardCell newCell = Instantiate(cellPrefab, m_CellParent);
                    m_BoardCellList.Add(newCell); // キャッシュに追加
                }
            }

            // 既存のセルを再利用または非表示に
            for (int i = 0; i < m_BoardCellList.Count; i++)
            {
                var cell = m_BoardCellList[i];
                var isExist = i < dataList.Count;

                UnityUtility.SetActive(cell, isExist);

                if (isExist)
                {
                    // データがある場合はセルを設定して表示
                    cell.Setup(dataList[i]);
                }
            }
        }
    }
}
