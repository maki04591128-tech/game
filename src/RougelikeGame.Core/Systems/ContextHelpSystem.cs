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
        RegisterTopic("move_basic", HelpCategory.Movement, "基本移動", "テンキーまたはViキーで8方向に移動できます", "hjklyubn", 10);
        RegisterTopic("move_wait", HelpCategory.Movement, "待機", ".キーでその場で1ターン待機します", ".", 5);
        RegisterTopic("combat_attack", HelpCategory.Combat, "近接攻撃", "敵に隣接して移動キーを押すと攻撃します", null, 10);
        RegisterTopic("combat_magic", HelpCategory.Combat, "魔法", "Mキーで魔法メニューを開きます", "M", 8);
        RegisterTopic("inventory_open", HelpCategory.Inventory, "インベントリ", "Iキーでインベントリを開きます", "I", 10);
        RegisterTopic("survival_hunger", HelpCategory.Survival, "空腹度", "時間経過で空腹になります。食料を携帯しましょう", null, 8);
        RegisterTopic("survival_sanity", HelpCategory.Survival, "正気度", "死に戻りで正気度が減少します。0になると廃人化", null, 9);
    }
}
