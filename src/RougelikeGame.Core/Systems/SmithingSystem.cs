namespace RougelikeGame.Core.Systems;

/// <summary>
/// 鍛冶システム - 装備強化・修理・合成
/// </summary>
public class SmithingSystem
{
    /// <summary>装備を強化する</summary>
    public SmithingResult Enhance(Entities.Player player, string itemName, int currentEnhance, int maxEnhance = 10)
    {
        if (currentEnhance >= maxEnhance)
            return new SmithingResult(false, "これ以上強化できない");

        int cost = CalculateEnhanceCost(currentEnhance);
        if (!player.SpendGold(cost))
            return new SmithingResult(false, $"ゴールドが足りない（必要: {cost}G）");

        int newEnhance = currentEnhance + 1;
        return new SmithingResult(true,
            $"{itemName}を+{newEnhance}に強化した！（{cost}G）",
            newEnhance);
    }

    /// <summary>装備を修理する</summary>
    public SmithingResult Repair(Entities.Player player, string itemName, int durabilityLost)
    {
        if (durabilityLost <= 0)
            return new SmithingResult(false, "修理の必要がない");

        int cost = (int)Math.Min((long)durabilityLost * 5, int.MaxValue);
        if (!player.SpendGold(cost))
            return new SmithingResult(false, $"ゴールドが足りない（必要: {cost}G）");

        return new SmithingResult(true, $"{itemName}を修理した（{cost}G）");
    }

    /// <summary>素材を合成して新アイテムを作成</summary>
    public SmithingResult Synthesize(Entities.Player player, string[] materialIds, string resultItemId, int cost)
    {
        if (!player.SpendGold(cost))
            return new SmithingResult(false, $"ゴールドが足りない（必要: {cost}G）");

        return new SmithingResult(true,
            $"素材を合成して新しいアイテムを作成した（{cost}G）",
            ResultItemId: resultItemId);
    }

    /// <summary>エンチャント（属性付与）</summary>
    public SmithingResult Enchant(Entities.Player player, string itemName, Element element, int cost)
    {
        if (!player.SpendGold(cost))
            return new SmithingResult(false, $"ゴールドが足りない（必要: {cost}G）");

        string elementName = element switch
        {
            Element.Fire => "炎",
            Element.Ice => "氷",
            Element.Lightning => "雷",
            Element.Holy => "聖",
            Element.Dark => "闇",
            _ => element.ToString()
        };

        return new SmithingResult(true,
            $"{itemName}に{elementName}属性を付与した（{cost}G）");
    }

    /// <summary>強化コストを計算</summary>
    public static int CalculateEnhanceCost(int currentEnhance) =>
        (int)Math.Min((long)100 * (currentEnhance + 1) * (currentEnhance + 1), int.MaxValue);
}

/// <summary>
/// 鍛冶結果
/// </summary>
public record SmithingResult(
    bool Success,
    string Message,
    int NewEnhanceLevel = 0,
    string? ResultItemId = null);

/// <summary>
/// チュートリアルシステム - 初回プレイ時の操作ガイド
/// </summary>
public class TutorialSystem
{
    private readonly HashSet<string> _completedSteps = new();
    private readonly HashSet<TutorialTrigger> _triggeredEvents = new();
    private readonly List<TutorialStep> _steps;

    /// <summary>チュートリアルが有効か</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>全ステップが完了済みか</summary>
    public bool IsComplete => _completedSteps.Count >= _steps.Count;

    /// <summary>完了数</summary>
    public int CompletedCount => _completedSteps.Count;

    /// <summary>全ステップ数</summary>
    public int TotalSteps => _steps.Count;

    public TutorialSystem()
    {
        _steps = new List<TutorialStep>
        {
            new("move", "移動", "WASD キーまたは矢印キーで移動できます。\n斜め移動は QE/ZC キーで行えます。", TutorialTrigger.GameStart, Priority: 100),
            new("attack", "攻撃", "敵の隣に移動すると自動で攻撃します。\n遠距離武器を装備していれば R キーで射撃できます。", TutorialTrigger.FirstEnemySight, Priority: 80),
            new("inventory", "インベントリ", "I キーでインベントリを開けます。\nアイテムを選んで使用・装備できます。", TutorialTrigger.FirstItemPickup, Priority: 70),
            new("stairs", "階段", "階段の上で > キーで降りる、< キーで上がれます。\nダンジョンを深く探索しましょう。", TutorialTrigger.FirstStairs, Priority: 65),
            new("magic", "魔法", "ルーン語を組み合わせて魔法を詠唱できます。\n効果語は必須、対象語・修飾語は任意です。", TutorialTrigger.FirstMagicWord, Priority: 45),
            new("death", "死に戻り", "HPが0になると死に戻りが発生します。\n知識（ルーン語・スキル）は引き継がれますが、装備やレベルはリセットされます。", TutorialTrigger.FirstDeath, Priority: 85),
            new("shop", "ショップ", "街のショップでアイテムを売買できます。\nカリスマが高いほど有利な価格で取引できます。", TutorialTrigger.FirstShopVisit, Priority: 30),
            new("religion", "宗教", "神殿で宗教に入信できます。\n信仰度を上げると恩恵を得られますが、禁忌を犯すと信仰度が下がります。", TutorialTrigger.FirstTempleVisit, Priority: 15),
            new("quest", "クエスト", "冒険者ギルドでクエストを受注できます。\nクエストを完了すると報酬とギルドポイントが得られます。", TutorialTrigger.FirstGuildVisit, Priority: 20),
            new("skill", "スキル", "レベルアップ時にスキルツリーからスキルを習得できます。\n職業によって習得可能なスキルが異なります。", TutorialTrigger.FirstLevelUp, Priority: 50),
            new("equip", "装備変更", "武器や防具を装備して戦闘力を上げましょう。装備品には必要レベルやステータス要件があります。", TutorialTrigger.FirstEquipChange, Priority: 55),
            new("potion", "ポーション", "インベントリからポーションを選択して使用できます。回復薬は戦闘中の回復に重要です。", TutorialTrigger.FirstPotionUse, Priority: 60),
            new("crafting", "合成", "素材を集めて新しいアイテムを合成できます。レシピは冒険を進めるほど増えていきます。", TutorialTrigger.FirstCrafting, Priority: 35),
            new("spell_cast", "魔法詠唱", "魔法を詠唱しました！単語の組み合わせで効果が変わります。理解度が高いほど成功率が上がります。", TutorialTrigger.FirstSpellCast, Priority: 40),
            new("npc_talk", "NPC会話", "NPCに話しかけました。NPCは情報やクエストを提供してくれます。定期的に話しかけると友好度が上がります。", TutorialTrigger.FirstNpcTalk, Priority: 25),
            new("boss", "ボス戦", "強力なボスが出現しました！ボスは通常の敵より遥かに強力です。十分な準備をしてから挑みましょう。", TutorialTrigger.FirstBossEncounter, Priority: 75),
            new("floor5", "中層到達", "5階に到達しました。敵が強くなります。装備を見直し、ポーションを十分に持っているか確認しましょう。", TutorialTrigger.ReachFloor5, Priority: 10),
            new("floor10", "深層への入口", "10階に到達しました。ここからは本格的な挑戦です。合成や強化を活用して万全の準備を整えましょう。", TutorialTrigger.ReachFloor10, Priority: 5)
        };
    }

    /// <summary>トリガーに対応するチュートリアルを取得</summary>
    public TutorialStep? GetStepForTrigger(TutorialTrigger trigger)
    {
        if (!IsEnabled) return null;

        return _steps
            .Where(s => s.Trigger == trigger && !_completedSteps.Contains(s.Id))
            .OrderByDescending(s => s.Priority)
            .FirstOrDefault();
    }

    /// <summary>トリガーイベントを発火し、該当ステップを自動完了して返す</summary>
    public TutorialStep? OnTrigger(TutorialTrigger trigger)
    {
        if (!IsEnabled) return null;
        if (_triggeredEvents.Contains(trigger)) return null;

        _triggeredEvents.Add(trigger);
        var step = GetStepForTrigger(trigger);
        if (step != null)
        {
            _completedSteps.Add(step.Id);
        }
        return step;
    }

    /// <summary>ステップを完了済みにする</summary>
    public void CompleteStep(string stepId)
    {
        _completedSteps.Add(stepId);
    }

    /// <summary>ステップが完了済みか判定</summary>
    public bool IsStepCompleted(string stepId)
    {
        return _completedSteps.Contains(stepId);
    }

    /// <summary>完了済みステップIDを取得（セーブ用）</summary>
    public IReadOnlySet<string> GetCompletedSteps() => _completedSteps;

    /// <summary>全ステップを取得</summary>
    public IReadOnlyList<TutorialStep> GetAllSteps() => _steps;

    /// <summary>完了済みステップを復元（ロード用）</summary>
    public void RestoreCompletedSteps(IEnumerable<string> stepIds)
    {
        _completedSteps.Clear();
        _triggeredEvents.Clear();
        foreach (var id in stepIds) _completedSteps.Add(id);
    }

    /// <summary>チュートリアル進捗をリセット</summary>
    public void Reset()
    {
        _completedSteps.Clear();
        _triggeredEvents.Clear();
    }

    /// <summary>進行度を取得（0.0-1.0）</summary>
    public double GetProgress() =>
        _steps.Count > 0 ? (double)_completedSteps.Count / _steps.Count : 1.0;
}

/// <summary>
/// チュートリアルステップ
/// </summary>
public record TutorialStep(
    string Id,
    string Title,
    string Message,
    TutorialTrigger Trigger,
    int Priority = 0);

/// <summary>
/// チュートリアルトリガー
/// </summary>
public enum TutorialTrigger
{
    GameStart,
    FirstEnemySight,
    FirstItemPickup,
    FirstStairs,
    FirstMagicWord,
    FirstDeath,
    FirstShopVisit,
    FirstTempleVisit,
    FirstGuildVisit,
    FirstLevelUp,
    FirstEquipChange,
    FirstPotionUse,
    FirstCrafting,
    FirstSpellCast,
    FirstNpcTalk,
    FirstBossEncounter,
    ReachFloor5,
    ReachFloor10
}
