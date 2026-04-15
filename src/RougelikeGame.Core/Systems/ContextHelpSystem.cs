namespace RougelikeGame.Core.Systems;

/// <summary>
/// コンテキストヘルプ・チュートリアルシステム - 段階的チュートリアルと状況別ヘルプ
/// 参考: Cogmind、DCSS（ヒントモード）
/// </summary>
public class ContextHelpSystem
{
    /// <summary>ヘルプトピック</summary>
    public record HelpTopic(
        string TopicId,
        HelpCategory Category,
        string Title,
        string Content,
        string? KeyBind,
        int Priority
    );

    /// <summary>チュートリアルステップ</summary>
    public record TutorialStep(
        int StepNumber,
        string Title,
        string Instruction,
        string TriggerCondition,
        bool IsCompleted
    );

    private readonly Dictionary<string, HelpTopic> _topics = new();
    private readonly List<TutorialStep> _tutorialSteps = new();
    private int _currentStep;
    private bool _tutorialEnabled = true;

    /// <summary>全ヘルプトピック</summary>
    public IReadOnlyDictionary<string, HelpTopic> Topics => _topics;

    /// <summary>チュートリアルステップ一覧</summary>
    public IReadOnlyList<TutorialStep> TutorialSteps => _tutorialSteps;

    /// <summary>現在のチュートリアルステップ番号</summary>
    public int CurrentStep => _currentStep;

    /// <summary>チュートリアル有効状態</summary>
    public bool TutorialEnabled => _tutorialEnabled;

    /// <summary>チュートリアルの有効/無効切り替え</summary>
    public void SetTutorialEnabled(bool enabled) => _tutorialEnabled = enabled;

    /// <summary>ヘルプトピックを登録</summary>
    public void RegisterTopic(string id, HelpCategory category, string title, string content,
        string? keyBind = null, int priority = 0)
    {
        _topics[id] = new HelpTopic(id, category, title, content, keyBind, priority);
    }

    /// <summary>チュートリアルステップを追加</summary>
    public void AddTutorialStep(string title, string instruction, string triggerCondition)
    {
        int stepNumber = _tutorialSteps.Count + 1;
        _tutorialSteps.Add(new TutorialStep(stepNumber, title, instruction, triggerCondition, false));
    }

    /// <summary>カテゴリ別ヘルプトピック取得</summary>
    public IReadOnlyList<HelpTopic> GetTopicsByCategory(HelpCategory category)
    {
        return _topics.Values
            .Where(t => t.Category == category)
            .OrderByDescending(t => t.Priority)
            .ToList();
    }

    /// <summary>キーバインドに対応するヘルプを取得</summary>
    public HelpTopic? GetHelpForKey(string key)
    {
        return _topics.Values.FirstOrDefault(t => t.KeyBind == key);
    }

    /// <summary>現在のチュートリアルステップの内容を取得</summary>
    public TutorialStep? GetCurrentTutorial()
    {
        if (!_tutorialEnabled) return null;
        if (_currentStep >= _tutorialSteps.Count) return null;
        return _tutorialSteps[_currentStep];
    }

    /// <summary>現在のチュートリアルステップを完了</summary>
    public TutorialStep? CompleteTutorialStep()
    {
        if (_currentStep >= _tutorialSteps.Count) return null;
        _tutorialSteps[_currentStep] = _tutorialSteps[_currentStep] with { IsCompleted = true };
        var completed = _tutorialSteps[_currentStep];
        _currentStep++;
        return completed;
    }

    /// <summary>チュートリアル進行率（0.0〜1.0）</summary>
    public float TutorialProgress => _tutorialSteps.Count == 0 ? 0f :
        (float)_tutorialSteps.Count(s => s.IsCompleted) / _tutorialSteps.Count;

    /// <summary>状況に応じたヘルプ候補を取得（優先度順）</summary>
    public IReadOnlyList<HelpTopic> GetContextualHelp(string context)
    {
        return _topics.Values
            .Where(t => t.Content.Contains(context, StringComparison.OrdinalIgnoreCase) ||
                        t.Title.Contains(context, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(t => t.Priority)
            .Take(5)
            .ToList();
    }

    /// <summary>ヘルプカテゴリ名を取得</summary>
    public static string GetCategoryName(HelpCategory category) => category switch
    {
        HelpCategory.Movement => "移動",
        HelpCategory.Combat => "戦闘",
        HelpCategory.Inventory => "インベントリ",
        HelpCategory.Magic => "魔法",
        HelpCategory.Crafting => "クラフト",
        HelpCategory.Survival => "サバイバル",
        HelpCategory.Advanced => "上級テクニック",
        _ => "その他"
    };

    /// <summary>デフォルトのヘルプトピックを一括登録</summary>
    public void RegisterDefaultTopics()
    {
        // 移動
        RegisterTopic("move_basic", HelpCategory.Movement, "基本移動", "WASD/矢印キーで4方向に移動できます。QE/ZCキーで斜め移動。テンキーやViキー(hjklyubn)も使用可能です", "WASD", 10);
        RegisterTopic("move_wait", HelpCategory.Movement, "待機", ".キーでその場で1ターン待機します。HPやSPが少しずつ回復します", ".", 5);
        RegisterTopic("move_auto", HelpCategory.Movement, "自動探索", "Oキーで自動探索を開始します。敵を発見するか、アイテムを見つけると停止します", "O", 7);
        RegisterTopic("move_stairs", HelpCategory.Movement, "階段", ">キーで下り階段を降り、<キーで上り階段を上ります。ダンジョンを深く探索しましょう", "><", 8);

        // 戦闘
        RegisterTopic("combat_attack", HelpCategory.Combat, "近接攻撃", "敵に隣接して移動キーを押すと攻撃します。武器の種類によってダメージが変わります", null, 10);
        RegisterTopic("combat_ranged", HelpCategory.Combat, "遠距離攻撃", "Rキーで弓や投擲武器による遠距離攻撃ができます。射程内の敵を狙えます", "R", 8);
        RegisterTopic("combat_magic", HelpCategory.Combat, "魔法", "Mキーで魔法メニューを開きます。ルーン語の組み合わせで様々な魔法を使えます", "M", 8);
        RegisterTopic("combat_skill", HelpCategory.Combat, "スキル", "1-6キーでスキルスロットに登録した戦闘スキルを使用できます", "1-6", 7);
        RegisterTopic("combat_throw", HelpCategory.Combat, "投擲", "投擲可能なアイテムを敵に投げつけることができます", null, 5);

        // インベントリ
        RegisterTopic("inventory_open", HelpCategory.Inventory, "インベントリ", "Iキーでインベントリを開きます。アイテムの使用・装備・ドロップができます", "I", 10);
        RegisterTopic("inventory_pickup", HelpCategory.Inventory, "アイテム拾得", "Gキーまたは,キーで足元のアイテムを拾えます", "G", 8);
        RegisterTopic("inventory_equip", HelpCategory.Inventory, "装備変更", "インベントリから装備品を選択して装着できます。必要レベルやステータス要件に注意", null, 7);
        RegisterTopic("inventory_drop", HelpCategory.Inventory, "アイテムドロップ", "Dキーでアイテムを床に置けます。重量オーバー時に活用しましょう", "D", 5);

        // 魔法
        RegisterTopic("magic_rune", HelpCategory.Magic, "ルーン語", "古代の書を読むことでルーン語を習得できます。図書館で新しい単語を学びましょう", null, 8);
        RegisterTopic("magic_spell", HelpCategory.Magic, "魔法詠唱", "効果語＋対象語＋修飾語の組み合わせで魔法を構成します。理解度が高いほど成功率が上がります", null, 7);

        // クラフト
        RegisterTopic("crafting_basic", HelpCategory.Crafting, "合成", "素材を組み合わせて新しいアイテムを作成できます。鍛冶屋で装備の強化も可能です", null, 8);
        RegisterTopic("crafting_cooking", HelpCategory.Crafting, "料理", "食材を組み合わせて料理を作れます。料理は通常の食料より高い回復効果があります", null, 6);

        // サバイバル
        RegisterTopic("survival_hunger", HelpCategory.Survival, "空腹度", "時間経過で空腹になります。食料を常に携帯し、定期的に食事を取りましょう。飢餓状態では毎ターンダメージを受けます", null, 9);
        RegisterTopic("survival_thirst", HelpCategory.Survival, "渇き", "時間経過で喉が渇きます。水や飲料を携帯しましょう。脱水状態では行動コストが増加します", null, 8);
        RegisterTopic("survival_sanity", HelpCategory.Survival, "正気度", "死に戻りで正気度が減少します。0になると廃人化し、救済回数を消費します。宿屋で休息すると少し回復します", null, 9);
        RegisterTopic("survival_fatigue", HelpCategory.Survival, "疲労", "行動するほど疲労が溜まります。重度の疲労では攻撃やスキルが使えなくなります。宿屋で休息しましょう", null, 7);
        RegisterTopic("survival_hygiene", HelpCategory.Survival, "衛生", "長時間の探索で衛生度が低下します。病気のリスクが高まるので注意しましょう", null, 5);

        // 上級テクニック
        RegisterTopic("adv_rebirth", HelpCategory.Advanced, "死に戻り", "死亡すると知識を引き継いで転生します。ルーン語やスキルの知識は保持されますが、レベルと装備はリセットされます", null, 9);
        RegisterTopic("adv_religion", HelpCategory.Advanced, "宗教", "神殿で宗教に入信できます。信仰度を上げると恩恵が得られますが、禁忌を犯すと罰を受けます", null, 7);
        RegisterTopic("adv_guild", HelpCategory.Advanced, "冒険者ギルド", "ギルドに登録してクエストを受注できます。ランクが上がるとより報酬の高い依頼が解禁されます", null, 6);
        RegisterTopic("adv_companion", HelpCategory.Advanced, "仲間", "一部のNPCを仲間として連れて行けます。仲間は自動で戦闘に参加し、AIモードを変更できます", null, 5);
    }
}
