using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// 合成レシピ定義
/// </summary>
public record CraftingRecipe(
    string RecipeId,
    string Name,
    string Description,
    List<CraftingMaterial> Materials,
    string ResultItemId,
    int RequiredLevel = 1,
    int CraftingCost = 0);

/// <summary>
/// 合成に必要な素材
/// </summary>
public record CraftingMaterial(string ItemId, int Quantity);

/// <summary>
/// 合成結果
/// </summary>
public record CraftingResult(bool Success, string Message, Item? ResultItem = null);

/// <summary>
/// 強化結果
/// </summary>
public record EnhancementResult(bool Success, string Message, int NewLevel = 0);

/// <summary>
/// 付与結果
/// </summary>
public record EnchantmentResult(bool Success, string Message, Element Element = Element.None);

/// <summary>
/// 合成・強化・付与システム
/// </summary>
public class CraftingSystem
{
    private readonly Dictionary<string, CraftingRecipe> _recipes = new();

    public CraftingSystem()
    {
        RegisterDefaultRecipes();
    }

    #region Recipe Management

    /// <summary>
    /// レシピを登録
    /// </summary>
    public void RegisterRecipe(CraftingRecipe recipe)
    {
        _recipes[recipe.RecipeId] = recipe;
    }

    /// <summary>
    /// レシピを取得
    /// </summary>
    public CraftingRecipe? GetRecipe(string recipeId)
    {
        return _recipes.GetValueOrDefault(recipeId);
    }

    /// <summary>
    /// 全レシピを取得
    /// </summary>
    public IReadOnlyList<CraftingRecipe> GetAllRecipes()
    {
        return _recipes.Values.ToList();
    }

    /// <summary>
    /// プレイヤーのレベルで利用可能なレシピを取得
    /// </summary>
    public IReadOnlyList<CraftingRecipe> GetAvailableRecipes(int playerLevel)
    {
        return _recipes.Values.Where(r => playerLevel >= r.RequiredLevel).ToList();
    }

    #endregion

    #region Crafting

    /// <summary>
    /// 合成可能か判定
    /// </summary>
    public bool CanCraft(string recipeId, Player player, CraftingInventory inventory)
    {
        if (!_recipes.TryGetValue(recipeId, out var recipe))
            return false;

        if (player.Level < recipe.RequiredLevel)
            return false;

        if (player.Gold < recipe.CraftingCost)
            return false;

        foreach (var material in recipe.Materials)
        {
            int count = inventory.CountItem(material.ItemId);
            if (count < material.Quantity)
                return false;
        }

        return true;
    }

    /// <summary>
    /// アイテムを合成する
    /// </summary>
    public CraftingResult Craft(string recipeId, Player player, CraftingInventory inventory)
    {
        if (!_recipes.TryGetValue(recipeId, out var recipe))
            return new CraftingResult(false, "レシピが見つからない");

        if (!CanCraft(recipeId, player, inventory))
            return new CraftingResult(false, "合成条件を満たしていない");

        // EL-1/EL-3: アイテム生成を先に確認（素材消費前にロールバック不要に）
        var resultItem = ItemDefinitions.Create(recipe.ResultItemId);
        if (resultItem == null)
            return new CraftingResult(false, "結果アイテムの生成に失敗した");

        // EL-3: ゴールドを先に消費チェック
        if (recipe.CraftingCost > 0)
        {
            if (!player.SpendGold(recipe.CraftingCost))
                return new CraftingResult(false, "ゴールドが足りない");
        }

        // CY-1/EL-2: 素材を消費（戻り値チェック）
        foreach (var material in recipe.Materials)
        {
            if (!inventory.RemoveItem(material.ItemId, material.Quantity))
                return new CraftingResult(false, $"素材{material.ItemId}が不足している");
        }

        return new CraftingResult(true, $"{recipe.Name}に成功！{resultItem.Name}を入手した！", resultItem);
    }

    #endregion

    #region Enhancement

    /// <summary>
    /// 装備強化の成功率を計算
    /// </summary>
    public static int CalculateEnhanceSuccessRate(int currentLevel)
    {
        return currentLevel switch
        {
            0 => 100,
            1 => 95,
            2 => 90,
            3 => 80,
            4 => 70,
            5 => 55,
            6 => 40,
            7 => 25,
            8 => 15,
            9 => 10,
            _ => 5
        };
    }

    /// <summary>
    /// 装備を強化する
    /// </summary>
    public EnhancementResult EnhanceEquipment(EquipmentItem equipment, Player player, IRandomProvider random, int cost = 100)
    {
        if (equipment.EnhancementLevel >= 10)
            return new EnhancementResult(false, "これ以上強化できない");

        if (player.Gold < cost)
            return new EnhancementResult(false, "ゴールドが足りない");

        player.SpendGold(cost);

        int successRate = CalculateEnhanceSuccessRate(equipment.EnhancementLevel);
        bool success = random.Next(100) < successRate;

        if (success)
        {
            equipment.EnhancementLevel++;
            return new EnhancementResult(true,
                $"{equipment.Name}の強化に成功！ +{equipment.EnhancementLevel}になった！",
                equipment.EnhancementLevel);
        }

        // 失敗時: +7以上で強化値が下がるリスク
        if (equipment.EnhancementLevel >= 7 && random.Next(100) < 30)
        {
            equipment.EnhancementLevel = Math.Max(0, equipment.EnhancementLevel - 1);
            return new EnhancementResult(false,
                $"強化に失敗！{equipment.Name}の強化値が下がった！ +{equipment.EnhancementLevel}",
                equipment.EnhancementLevel);
        }

        return new EnhancementResult(false, "強化に失敗した…");
    }

    #endregion

    #region Enchantment

    /// <summary>
    /// 武器に属性を付与する
    /// </summary>
    public EnchantmentResult EnchantWeapon(Weapon weapon, Element element, Player player, IRandomProvider random, int cost = 200)
    {
        if (element == Element.None)
            return new EnchantmentResult(false, "付与する属性を指定してください");

        if (weapon.Element != Element.None)
            return new EnchantmentResult(false, "既に属性が付与されている");

        if (player.Gold < cost)
            return new EnchantmentResult(false, "ゴールドが足りない");

        player.SpendGold(cost);

        int successRate = 70;
        if (random.Next(100) < successRate)
        {
            // Weapon.Elementはinit-only。新しいWeaponレコードを返す必要があるが、
            // テスト容易性のため成功メッセージと属性を返す
            return new EnchantmentResult(true,
                $"{weapon.Name}に{GetElementName(element)}属性を付与した！",
                element);
        }

        return new EnchantmentResult(false, "属性付与に失敗した…");
    }

    private static string GetElementName(Element element) => element switch
    {
        Element.Fire => "炎",
        Element.Water => "水",
        Element.Earth => "地",
        Element.Wind => "風",
        Element.Light => "光",
        Element.Dark => "闇",
        Element.Lightning => "雷",
        Element.Ice => "氷",
        Element.Poison => "毒",
        Element.Holy => "聖",
        Element.Curse => "呪",
        _ => "無"
    };

    #endregion

    #region Default Recipes

    private void RegisterDefaultRecipes()
    {
        // 武器レシピ
        RegisterRecipe(new CraftingRecipe(
            "recipe_iron_sword", "鉄の剣の鍛造", "鉄素材から剣を鍛造する",
            new List<CraftingMaterial>
            {
                new("material_iron_ore", 3),
                new("material_wood", 1)
            },
            "weapon_iron_sword", RequiredLevel: 1, CraftingCost: 50));

        RegisterRecipe(new CraftingRecipe(
            "recipe_steel_sword", "鋼の剣の鍛造", "高品質な鋼の剣を鍛造する",
            new List<CraftingMaterial>
            {
                new("material_iron_ore", 5),
                new("material_coal", 2)
            },
            "weapon_steel_sword", RequiredLevel: 5, CraftingCost: 150));

        RegisterRecipe(new CraftingRecipe(
            "recipe_battle_axe", "戦斧の鍛造", "強力な戦斧を鍛造する",
            new List<CraftingMaterial>
            {
                new("material_iron_ore", 4),
                new("material_wood", 2)
            },
            "weapon_battle_axe", RequiredLevel: 5, CraftingCost: 120));

        // 防具レシピ
        RegisterRecipe(new CraftingRecipe(
            "recipe_chainmail", "鎖帷子の鍛造", "鎖帷子を鍛造する",
            new List<CraftingMaterial>
            {
                new("material_iron_ore", 6),
            },
            "armor_chainmail", RequiredLevel: 3, CraftingCost: 100));

        RegisterRecipe(new CraftingRecipe(
            "recipe_leather_armor", "革鎧の製作", "革素材から鎧を作る",
            new List<CraftingMaterial>
            {
                new("material_leather", 4),
            },
            "armor_leather", RequiredLevel: 1, CraftingCost: 30));

        // ポーションレシピ
        RegisterRecipe(new CraftingRecipe(
            "recipe_healing_potion", "回復薬の調合", "薬草から回復薬を調合する",
            new List<CraftingMaterial>
            {
                new("material_herb", 3),
            },
            "potion_healing", RequiredLevel: 3, CraftingCost: 20));

        RegisterRecipe(new CraftingRecipe(
            "recipe_antidote", "解毒剤の調合", "薬草から解毒剤を調合する",
            new List<CraftingMaterial>
            {
                new("material_herb", 2),
            },
            "potion_antidote", RequiredLevel: 1, CraftingCost: 10));

        RegisterRecipe(new CraftingRecipe(
            "recipe_super_healing", "超回復薬の調合", "貴重な薬草から超回復薬を調合する",
            new List<CraftingMaterial>
            {
                new("material_herb", 5),
                new("material_magical_essence", 1)
            },
            "potion_healing_super", RequiredLevel: 8, CraftingCost: 80));

        // 食料レシピ
        RegisterRecipe(new CraftingRecipe(
            "recipe_cooked_meat", "焼き肉の調理", "生肉を焼く",
            new List<CraftingMaterial>
            {
                new("material_raw_meat", 1),
            },
            "food_cooked_meat", RequiredLevel: 1, CraftingCost: 0));

        RegisterRecipe(new CraftingRecipe(
            "recipe_emergency_ration", "非常食の製作", "保存食を作る",
            new List<CraftingMaterial>
            {
                new("material_raw_meat", 2),
                new("material_herb", 1)
            },
            "food_emergency_ration", RequiredLevel: 3, CraftingCost: 15));
    }

    #endregion
}

/// <summary>
/// インベントリ拡張（合成用）
/// </summary>
public class CraftingInventory
{
    private readonly Dictionary<string, int> _items = new();

    /// <summary>アイテムを追加</summary>
    public void AddItem(string itemId, int quantity = 1)
    {
        if (_items.ContainsKey(itemId))
            _items[itemId] += quantity;
        else
            _items[itemId] = quantity;
    }

    /// <summary>アイテムを削除</summary>
    public bool RemoveItem(string itemId, int quantity = 1)
    {
        if (!_items.ContainsKey(itemId) || _items[itemId] < quantity)
            return false;

        _items[itemId] -= quantity;
        if (_items[itemId] <= 0)
            _items.Remove(itemId);
        return true;
    }

    /// <summary>アイテム数を取得</summary>
    public int CountItem(string itemId)
    {
        return _items.GetValueOrDefault(itemId, 0);
    }

    /// <summary>全アイテムを取得</summary>
    public IReadOnlyDictionary<string, int> GetAllItems() => _items;

    /// <summary>アイテムが含まれているか</summary>
    public bool HasItem(string itemId, int quantity = 1)
    {
        return CountItem(itemId) >= quantity;
    }
}
