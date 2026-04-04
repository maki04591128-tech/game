namespace RougelikeGame.Core.Systems;

/// <summary>
/// 派閥関係の定義
/// </summary>
public record FactionRelation(MonsterRace Race1, MonsterRace Race2, float Hostility);

/// <summary>
/// ダンジョン内派閥・生態系システム - 種族間の敵対/同盟関係
/// </summary>
public static class DungeonFactionSystem
{
    private static readonly List<FactionRelation> Relations = new()
    {
        // 敵対関係（Hostility > 0.5 = 敵対）
        new(MonsterRace.Beast, MonsterRace.Undead, 0.8f),
        new(MonsterRace.Beast, MonsterRace.Construct, 0.3f),
        new(MonsterRace.Humanoid, MonsterRace.Demon, 0.7f),
        new(MonsterRace.Humanoid, MonsterRace.Undead, 0.6f),
        new(MonsterRace.Dragon, MonsterRace.Demon, 0.5f),
        new(MonsterRace.Spirit, MonsterRace.Demon, 0.9f),
        new(MonsterRace.Plant, MonsterRace.Construct, 0.2f),

        // 同盟・中立関係（Hostility < 0.3 = 同盟的）
        new(MonsterRace.Beast, MonsterRace.Plant, 0.1f),
        new(MonsterRace.Undead, MonsterRace.Demon, 0.2f),
        new(MonsterRace.Undead, MonsterRace.Spirit, 0.3f),
        new(MonsterRace.Insect, MonsterRace.Plant, 0.15f),
    };

    /// <summary>2種族間の敵意レベルを取得（0.0=友好〜1.0=完全敵対）</summary>
    public static float GetHostility(MonsterRace race1, MonsterRace race2)
    {
        if (race1 == race2) return 0f; // 同種族は友好

        var relation = Relations.FirstOrDefault(r =>
            (r.Race1 == race1 && r.Race2 == race2) ||
            (r.Race1 == race2 && r.Race2 == race1));

        return relation?.Hostility ?? 0.4f; // デフォルト = 中立やや警戒
    }

    /// <summary>2種族が敵対関係にあるか</summary>
    public static bool AreHostile(MonsterRace race1, MonsterRace race2)
    {
        return GetHostility(race1, race2) >= 0.5f;
    }

    /// <summary>2種族が同盟的関係にあるか</summary>
    public static bool AreAllied(MonsterRace race1, MonsterRace race2)
    {
        return GetHostility(race1, race2) <= 0.3f;
    }

    /// <summary>指定種族と敵対する種族一覧を取得</summary>
    public static IReadOnlyList<MonsterRace> GetHostileRaces(MonsterRace race)
    {
        return Enum.GetValues<MonsterRace>()
            .Where(r => r != race && AreHostile(race, r))
            .ToList();
    }

    /// <summary>全派閥関係を取得</summary>
    public static IReadOnlyList<FactionRelation> GetAllRelations() => Relations;
}
