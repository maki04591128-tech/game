namespace RougelikeGame.Core.Systems;

/// <summary>
/// マルチクラス・転職システム - サブクラス選択と上位職解放
/// </summary>
public static class MultiClassSystem
{
    /// <summary>転職要件</summary>
    public record ClassChangeRequirement(
        CharacterClass FromClass,
        CharacterClass ToClass,
        ClassTier Tier,
        int RequiredLevel,
        string QuestFlag
    );

    private static readonly List<ClassChangeRequirement> Requirements = new()
    {
        new(CharacterClass.Fighter, CharacterClass.Knight, ClassTier.Advanced, 20, "knight_trial"),
        new(CharacterClass.Mage, CharacterClass.Necromancer, ClassTier.Advanced, 20, "necro_trial"),
        new(CharacterClass.Thief, CharacterClass.Ranger, ClassTier.Advanced, 20, "ranger_trial"),
        new(CharacterClass.Cleric, CharacterClass.Monk, ClassTier.Advanced, 20, "monk_trial"),
        new(CharacterClass.Bard, CharacterClass.Alchemist, ClassTier.Advanced, 20, "alchemist_trial"),
        // DR-1: Master職への昇進条件
        new(CharacterClass.Knight, CharacterClass.Fighter, ClassTier.Master, 40, "master_knight_trial"),
        new(CharacterClass.Necromancer, CharacterClass.Mage, ClassTier.Master, 40, "master_necro_trial"),
        new(CharacterClass.Ranger, CharacterClass.Thief, ClassTier.Master, 40, "master_ranger_trial"),
        new(CharacterClass.Monk, CharacterClass.Cleric, ClassTier.Master, 40, "master_monk_trial"),
        new(CharacterClass.Alchemist, CharacterClass.Bard, ClassTier.Master, 40, "master_alchemist_trial"),
    };

    /// <summary>転職可能か判定</summary>
    public static bool CanClassChange(CharacterClass current, CharacterClass target, int level, HashSet<string> completedQuests)
    {
        var req = Requirements.FirstOrDefault(r => r.FromClass == current && r.ToClass == target);
        if (req == null) return false;
        return level >= req.RequiredLevel && completedQuests.Contains(req.QuestFlag);
    }

    /// <summary>転職要件を取得</summary>
    public static ClassChangeRequirement? GetRequirement(CharacterClass from, CharacterClass to)
    {
        return Requirements.FirstOrDefault(r => r.FromClass == from && r.ToClass == to);
    }

    /// <summary>利用可能な転職先を取得</summary>
    public static IReadOnlyList<ClassChangeRequirement> GetAvailableChanges(CharacterClass current)
    {
        return Requirements.Where(r => r.FromClass == current).ToList();
    }

    /// <summary>クラス段階名を取得</summary>
    public static string GetTierName(ClassTier tier) => tier switch
    {
        ClassTier.Base => "基本職",
        ClassTier.Advanced => "上位職",
        ClassTier.Master => "最上位職",
        _ => "不明"
    };

    /// <summary>サブクラス経験値倍率（50%のスキルツリー取得）</summary>
    public static float GetSubclassExpRate() => 0.5f;
}
