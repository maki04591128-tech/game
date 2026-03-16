using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// 熟練度データ
/// </summary>
public class ProficiencyData
{
    /// <summary>カテゴリ</summary>
    public ProficiencyCategory Category { get; }

    /// <summary>現在レベル (0～100)</summary>
    public int Level { get; set; }

    /// <summary>現在の経験値</summary>
    public int CurrentExp { get; set; }

    public ProficiencyData(ProficiencyCategory category)
    {
        Category = category;
        Level = 0;
        CurrentExp = 0;
    }

    /// <summary>次レベルに必要な経験値（指数関数: 100 * 1.15^Level）</summary>
    public int GetRequiredExp() => (int)(100 * Math.Pow(1.15, Level));
}

/// <summary>
/// レベルアップイベント引数
/// </summary>
public record ProficiencyLevelUpEventArgs(
    ProficiencyCategory Category,
    int OldLevel,
    int NewLevel);

/// <summary>
/// 熟練度システム - 12カテゴリの熟練度を管理
/// </summary>
public class ProficiencySystem
{
    private readonly Dictionary<ProficiencyCategory, ProficiencyData> _proficiencies = new();

    /// <summary>レベルアップ時に発火するイベント</summary>
    public event Action<ProficiencyLevelUpEventArgs>? OnLevelUp;

    private const int MaxLevel = 100;
    private const double DamageBonusPerLevel = 0.005; // 0.5%

    public ProficiencySystem()
    {
        foreach (ProficiencyCategory category in Enum.GetValues<ProficiencyCategory>())
        {
            _proficiencies[category] = new ProficiencyData(category);
        }
    }

    /// <summary>経験値を獲得しレベルアップ判定を行う</summary>
    public void GainExperience(ProficiencyCategory category, int amount)
    {
        if (amount <= 0) return;
        var data = _proficiencies[category];
        if (data.Level >= MaxLevel) return;

        data.CurrentExp += amount;

        while (data.Level < MaxLevel && data.CurrentExp >= data.GetRequiredExp())
        {
            data.CurrentExp -= data.GetRequiredExp();
            int oldLevel = data.Level;
            data.Level++;
            OnLevelUp?.Invoke(new ProficiencyLevelUpEventArgs(category, oldLevel, data.Level));
        }

        // レベル上限到達時は余剰経験値をリセット
        if (data.Level >= MaxLevel)
        {
            data.CurrentExp = 0;
        }
    }

    /// <summary>指定カテゴリの現在レベルを取得</summary>
    public int GetLevel(ProficiencyCategory category) => _proficiencies[category].Level;

    /// <summary>全カテゴリの熟練度データを取得</summary>
    public IReadOnlyDictionary<ProficiencyCategory, ProficiencyData> GetAllProficiencies() => _proficiencies;

    /// <summary>熟練度によるダメージボーナスを計算（Level * 0.5%）</summary>
    public double GetBonusDamage(ProficiencyCategory category, int baseDamage)
    {
        int level = _proficiencies[category].Level;
        return baseDamage * level * DamageBonusPerLevel;
    }

    /// <summary>鍛冶レベルによる品質ボーナスを計算</summary>
    public double GetBonusCraftQuality(int smithingLevel) => smithingLevel * 0.01;

    /// <summary>武器種から対応する熟練度カテゴリを返す</summary>
    public static ProficiencyCategory GetWeaponProficiencyCategory(WeaponType weaponType) => weaponType switch
    {
        WeaponType.Sword => ProficiencyCategory.Swordsmanship,
        WeaponType.Greatsword => ProficiencyCategory.Swordsmanship,
        WeaponType.Dagger => ProficiencyCategory.Swordsmanship,
        WeaponType.Spear => ProficiencyCategory.Spearmanship,
        WeaponType.Bow => ProficiencyCategory.Archery,
        WeaponType.Crossbow => ProficiencyCategory.Archery,
        WeaponType.Thrown => ProficiencyCategory.Archery,
        WeaponType.Unarmed => ProficiencyCategory.MartialArts,
        WeaponType.Fist => ProficiencyCategory.MartialArts,
        WeaponType.Staff => ProficiencyCategory.Sorcery,
        // 斧系は独立カテゴリがないため剣術に含める（近接斬撃武器として共通）
        WeaponType.Axe => ProficiencyCategory.Swordsmanship,
        WeaponType.Greataxe => ProficiencyCategory.Swordsmanship,
        WeaponType.Hammer => ProficiencyCategory.MartialArts,
        WeaponType.Whip => ProficiencyCategory.MartialArts,
        _ => ProficiencyCategory.MartialArts
    };

    /// <summary>未使用カテゴリの熟練度を緩やかに低下させる</summary>
    public void DecayUnusedProficiencies(HashSet<ProficiencyCategory> usedThisTurn)
    {
        foreach (var (category, data) in _proficiencies)
        {
            if (usedThisTurn.Contains(category)) continue;
            if (data.CurrentExp <= 0) continue;

            // 未使用カテゴリは経験値を1ずつ減少
            data.CurrentExp = Math.Max(0, data.CurrentExp - 1);
        }
    }
}
