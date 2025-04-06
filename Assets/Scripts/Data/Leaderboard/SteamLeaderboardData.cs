using UnityEngine;

public struct SteamLeaderboardData
{
    /// 順位
    public int Rank => m_Rank;
    private int m_Rank;

    /// SteamユーザーID
    public ulong SteamUserId => m_SteamUserId;
    private ulong m_SteamUserId;

    /// Steamユーザー名
    public string SteamUserName => m_SteamUserName;
    private string m_SteamUserName;

    /// スコア
    public int Score => m_Score;
    private int m_Score;

    public SteamLeaderboardData(int rank, ulong steamUserId, string steamUserName, int score)
    {
        m_Rank = rank;
        m_SteamUserId = steamUserId;
        m_SteamUserName = steamUserName;
        m_Score = score;
    }

#if DEBUG_MODE

    public void SetRank(int rank) => m_Rank = rank;
    public void SetScore(int score) => m_Score = score;

#endif
}
