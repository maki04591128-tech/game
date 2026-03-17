namespace RougelikeGame.Core.Systems;

/// <summary>
/// 拠点作成システム - 施設の建設・防衛管理・効果発動
/// </summary>
public class BaseConstructionSystem
{
    /// <summary>施設定義</summary>
    public record FacilityDefinition(
        FacilityCategory Category,
        string Name,
        int MaterialCost,
        int BuildTurns,
        string Effect
    );

    /// <summary>施設効果</summary>
    public record FacilityBonus(
        float HpRecoveryMultiplier,
        float CraftingSuccessBonus,
        int ExtraStorageSlots,
        int FoodProductionPerDay,
        int DefenseBonus,
        int ExtraCompanionSlots
    );

    private static readonly Dictionary<FacilityCategory, FacilityDefinition> Facilities = new()
    {
        [FacilityCategory.Camp] = new(FacilityCategory.Camp, "キャンプ", 10, 5, "基本休息が可能"),
        [FacilityCategory.Workbench] = new(FacilityCategory.Workbench, "作業台", 30, 15, "アイテム加工が可能"),
        [FacilityCategory.Smithy] = new(FacilityCategory.Smithy, "鍛冶場", 80, 30, "武器防具の作成・修理"),
        [FacilityCategory.Storage] = new(FacilityCategory.Storage, "倉庫", 50, 20, "アイテム保管容量+50"),
        [FacilityCategory.Farm] = new(FacilityCategory.Farm, "畑", 40, 25, "食料を自動生産"),
        [FacilityCategory.Barricade] = new(FacilityCategory.Barricade, "防壁", 60, 20, "防衛力+30"),
        [FacilityCategory.Barracks] = new(FacilityCategory.Barracks, "宿舎", 70, 25, "仲間収容数+2"),
    };

    private readonly HashSet<FacilityCategory> _built = new();

    /// <summary>建設済み施設</summary>
    public IReadOnlyCollection<FacilityCategory> BuiltFacilities => _built;

    /// <summary>施設定義を取得</summary>
    public static FacilityDefinition? GetDefinition(FacilityCategory category)
    {
        return Facilities.TryGetValue(category, out var d) ? d : null;
    }

    /// <summary>建設可能か判定</summary>
    public bool CanBuild(FacilityCategory category, int materials)
    {
        if (_built.Contains(category)) return false;
        var def = GetDefinition(category);
        return def != null && materials >= def.MaterialCost;
    }

    /// <summary>施設を建設</summary>
    public bool Build(FacilityCategory category, int materials)
    {
        if (!CanBuild(category, materials)) return false;
        _built.Add(category);
        return true;
    }

    /// <summary>施設が建設済みか確認</summary>
    public bool HasFacility(FacilityCategory category) => _built.Contains(category);

    /// <summary>防衛力を計算</summary>
    public int CalculateDefenseRating()
    {
        int defense = 0;
        if (_built.Contains(FacilityCategory.Barricade)) defense += 30;
        if (_built.Contains(FacilityCategory.Barracks)) defense += 10;
        if (_built.Contains(FacilityCategory.Camp)) defense += 5;
        return defense;
    }

    /// <summary>全施設効果を集計</summary>
    public FacilityBonus GetTotalBonus()
    {
        float hpRecovery = 1.0f;
        float craftingBonus = 0f;
        int storage = 0;
        int food = 0;
        int defense = 0;
        int companion = 0;

        if (_built.Contains(FacilityCategory.Camp)) hpRecovery += 0.25f;
        if (_built.Contains(FacilityCategory.Barracks)) { hpRecovery += 0.5f; companion += 2; }
        if (_built.Contains(FacilityCategory.Workbench)) craftingBonus += 0.1f;
        if (_built.Contains(FacilityCategory.Smithy)) craftingBonus += 0.2f;
        if (_built.Contains(FacilityCategory.Storage)) storage += 50;
        if (_built.Contains(FacilityCategory.Farm)) food += 3;
        if (_built.Contains(FacilityCategory.Barricade)) defense += 30;

        return new FacilityBonus(hpRecovery, craftingBonus, storage, food, defense, companion);
    }

    /// <summary>休息時のHP回復ボーナス倍率を取得</summary>
    public float GetRestHpRecoveryMultiplier()
    {
        return GetTotalBonus().HpRecoveryMultiplier;
    }

    /// <summary>製作時の成功確率ボーナスを取得</summary>
    public float GetCraftingSuccessBonus()
    {
        return GetTotalBonus().CraftingSuccessBonus;
    }

    /// <summary>追加保管枠を取得</summary>
    public int GetExtraStorageSlots()
    {
        return GetTotalBonus().ExtraStorageSlots;
    }

    /// <summary>毎日の食料自動生産量を取得</summary>
    public int GetDailyFoodProduction()
    {
        return GetTotalBonus().FoodProductionPerDay;
    }

    /// <summary>追加仲間収容枠を取得</summary>
    public int GetExtraCompanionSlots()
    {
        return GetTotalBonus().ExtraCompanionSlots;
    }
}
