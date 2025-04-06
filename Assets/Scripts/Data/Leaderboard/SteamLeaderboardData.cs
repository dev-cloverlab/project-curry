using UnityEngine;

public struct SteamLeaderboardData
{
    /// 順位
    public int Rank => m_Rank;
    private readonly int m_Rank;

    /// SteamユーザーID
    public ulong SteamUserId => m_SteamUserId;
    private readonly ulong m_SteamUserId;

    /// Steamユーザー名
    public string SteamUserName => m_SteamUserName;
    private readonly string m_SteamUserName;

    /// スコア
    public int Score => m_Score;
    private readonly int m_Score;

    public SteamLeaderboardData(int rank, ulong steamUserId, string steamUserName, int score)
    {
        m_Rank = rank;
        m_SteamUserId = steamUserId;
        m_SteamUserName = steamUserName;
        m_Score = score;
    }
}
