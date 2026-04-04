using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace RougelikeGame.Gui;

/// <summary>
/// キーバインドアクション種別（GameActionとUI専用アクションを統合）
/// </summary>
public enum KeyBindAction
{
    // 移動系
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,

    // 基本操作
    Wait,
    Pickup,
    UseStairs,
    AscendStairs,
    AutoExplore,
    Search,
    CloseDoor,
    RangedAttack,
    ThrowItem,
    StartCasting,
    Pray,

    // 画面系
    OpenInventory,
    OpenStatus,
    OpenMessageLog,
    OpenSkillTree,
    OpenWorldMap,
    OpenEncyclopedia,
    OpenCompanion,
    OpenDeathLog,
    OpenQuestLog,
    OpenReligion,
    OpenCooking,
    OpenVocabulary,

    // システム
    Save,
    Load,
    Quit,
    ToggleMinimap,
    CycleCombatStance,
    EnterTown,

    // スキルスロット
    SkillSlot1,
    SkillSlot2,
    SkillSlot3,
    SkillSlot4,
    SkillSlot5,
    SkillSlot6,
}

/// <summary>
/// キーバインドのキー＋修飾キーの組み合わせ
/// </summary>
public class KeyBinding
{
    public Key Key { get; set; }
    public ModifierKeys Modifiers { get; set; }

    public KeyBinding() { }

    public KeyBinding(Key key, ModifierKeys modifiers = ModifierKeys.None)
    {
        Key = key;
        Modifiers = modifiers;
    }

    public override string ToString()
    {
        if (Modifiers == ModifierKeys.None)
            return KeyToDisplayString(Key);
        var parts = new List<string>();
        if (Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        parts.Add(KeyToDisplayString(Key));
        return string.Join("+", parts);
    }

    private static string KeyToDisplayString(Key key) => key switch
    {
        Key.OemPeriod => ".",
        Key.OemComma => ",",
        Key.Space => "Space",
        Key.Tab => "Tab",
        Key.Escape => "Esc",
        Key.D1 => "1",
        Key.D2 => "2",
        Key.D3 => "3",
        Key.D4 => "4",
        Key.D5 => "5",
        Key.D6 => "6",
        _ => key.ToString()
    };
}

/// <summary>
/// キーバインド設定の永続化・管理クラス
/// </summary>
public class KeyBindingSettings
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RougelikeGame");

    private static readonly string KeyBindingsFilePath = Path.Combine(
        SettingsDirectory, "keybindings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// アクション→キーバインドのマッピング
    /// </summary>
    public Dictionary<KeyBindAction, KeyBinding> Bindings { get; set; } = new();

    /// <summary>
    /// デフォルトキーバインドを作成
    /// </summary>
    public static KeyBindingSettings CreateDefault()
    {
        var settings = new KeyBindingSettings();
        settings.Bindings = GetDefaultBindings();
        return settings;
    }

    /// <summary>
    /// デフォルトキーバインド辞書
    /// </summary>
    public static Dictionary<KeyBindAction, KeyBinding> GetDefaultBindings() => new()
    {
        // 移動
        [KeyBindAction.MoveUp] = new KeyBinding(Key.W),
        [KeyBindAction.MoveDown] = new KeyBinding(Key.S),
        [KeyBindAction.MoveLeft] = new KeyBinding(Key.A),
        [KeyBindAction.MoveRight] = new KeyBinding(Key.D),

        // 基本操作
        [KeyBindAction.Wait] = new KeyBinding(Key.Space),
        [KeyBindAction.Pickup] = new KeyBinding(Key.G),
        [KeyBindAction.UseStairs] = new KeyBinding(Key.OemPeriod, ModifierKeys.Shift),
        [KeyBindAction.AscendStairs] = new KeyBinding(Key.OemComma, ModifierKeys.Shift),
        [KeyBindAction.AutoExplore] = new KeyBinding(Key.Tab),
        [KeyBindAction.Search] = new KeyBinding(Key.F),
        [KeyBindAction.CloseDoor] = new KeyBinding(Key.X),
        [KeyBindAction.RangedAttack] = new KeyBinding(Key.R),
        [KeyBindAction.ThrowItem] = new KeyBinding(Key.T),
        [KeyBindAction.StartCasting] = new KeyBinding(Key.V),
        [KeyBindAction.Pray] = new KeyBinding(Key.P),
        [KeyBindAction.EnterTown] = new KeyBinding(Key.T, ModifierKeys.Shift),

        // 画面系
        [KeyBindAction.OpenInventory] = new KeyBinding(Key.I),
        [KeyBindAction.OpenStatus] = new KeyBinding(Key.C),
        [KeyBindAction.OpenMessageLog] = new KeyBinding(Key.L),
        [KeyBindAction.OpenSkillTree] = new KeyBinding(Key.E),
        [KeyBindAction.OpenWorldMap] = new KeyBinding(Key.J),
        [KeyBindAction.OpenEncyclopedia] = new KeyBinding(Key.Y),
        [KeyBindAction.OpenCompanion] = new KeyBinding(Key.U),
        [KeyBindAction.OpenDeathLog] = new KeyBinding(Key.Z),
        [KeyBindAction.OpenQuestLog] = new KeyBinding(Key.K),
        [KeyBindAction.OpenReligion] = new KeyBinding(Key.O),
        [KeyBindAction.OpenVocabulary] = new KeyBinding(Key.B),

        // システム
        [KeyBindAction.Save] = new KeyBinding(Key.F5),
        [KeyBindAction.Load] = new KeyBinding(Key.F9),
        [KeyBindAction.ToggleMinimap] = new KeyBinding(Key.M),
        [KeyBindAction.CycleCombatStance] = new KeyBinding(Key.N),

        // スキルスロット
        [KeyBindAction.SkillSlot1] = new KeyBinding(Key.D1),
        [KeyBindAction.SkillSlot2] = new KeyBinding(Key.D2),
        [KeyBindAction.SkillSlot3] = new KeyBinding(Key.D3),
        [KeyBindAction.SkillSlot4] = new KeyBinding(Key.D4),
        [KeyBindAction.SkillSlot5] = new KeyBinding(Key.D5),
        [KeyBindAction.SkillSlot6] = new KeyBinding(Key.D6),
    };

    /// <summary>
    /// アクションの日本語表示名
    /// </summary>
    public static string GetActionDisplayName(KeyBindAction action) => action switch
    {
        KeyBindAction.MoveUp => "上に移動",
        KeyBindAction.MoveDown => "下に移動",
        KeyBindAction.MoveLeft => "左に移動",
        KeyBindAction.MoveRight => "右に移動",
        KeyBindAction.Wait => "待機",
        KeyBindAction.Pickup => "アイテム拾う",
        KeyBindAction.UseStairs => "階段を降りる",
        KeyBindAction.AscendStairs => "階段を上る",
        KeyBindAction.AutoExplore => "自動探索",
        KeyBindAction.Search => "探索/ドア操作",
        KeyBindAction.CloseDoor => "ドアを閉じる",
        KeyBindAction.RangedAttack => "遠距離攻撃",
        KeyBindAction.ThrowItem => "投げる",
        KeyBindAction.StartCasting => "詠唱開始",
        KeyBindAction.Pray => "祈る",
        KeyBindAction.EnterTown => "町に入る",
        KeyBindAction.OpenInventory => "インベントリ",
        KeyBindAction.OpenStatus => "ステータス",
        KeyBindAction.OpenMessageLog => "メッセージログ",
        KeyBindAction.OpenSkillTree => "スキルツリー",
        KeyBindAction.OpenWorldMap => "ワールドマップ",
        KeyBindAction.OpenEncyclopedia => "図鑑",
        KeyBindAction.OpenCompanion => "仲間",
        KeyBindAction.OpenDeathLog => "死亡ログ",
        KeyBindAction.OpenQuestLog => "クエストログ",
        KeyBindAction.OpenReligion => "宗教",
        KeyBindAction.OpenCooking => "料理",
        KeyBindAction.OpenVocabulary => "ルーン語一覧",
        KeyBindAction.Save => "セーブ",
        KeyBindAction.Load => "ロード",
        KeyBindAction.Quit => "終了",
        KeyBindAction.ToggleMinimap => "ミニマップ切替",
        KeyBindAction.CycleCombatStance => "スタンス切替",
        KeyBindAction.SkillSlot1 => "スキルスロット1",
        KeyBindAction.SkillSlot2 => "スキルスロット2",
        KeyBindAction.SkillSlot3 => "スキルスロット3",
        KeyBindAction.SkillSlot4 => "スキルスロット4",
        KeyBindAction.SkillSlot5 => "スキルスロット5",
        KeyBindAction.SkillSlot6 => "スキルスロット6",
        _ => action.ToString()
    };

    /// <summary>
    /// 指定キー＋修飾キーに対応するアクションを検索
    /// </summary>
    public KeyBindAction? FindAction(Key key, ModifierKeys modifiers)
    {
        foreach (var kvp in Bindings)
        {
            if (kvp.Value.Key == key && kvp.Value.Modifiers == modifiers)
                return kvp.Key;
        }
        return null;
    }

    /// <summary>
    /// 設定をJSONファイルに保存
    /// </summary>
    public void Save()
    {
        Directory.CreateDirectory(SettingsDirectory);
        var json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(KeyBindingsFilePath, json);
    }

    /// <summary>
    /// 設定をJSONファイルから読み込む（ファイルがなければデフォルト）
    /// </summary>
    public static KeyBindingSettings Load()
    {
        if (!File.Exists(KeyBindingsFilePath))
            return CreateDefault();

        try
        {
            var json = File.ReadAllText(KeyBindingsFilePath);
            var settings = JsonSerializer.Deserialize<KeyBindingSettings>(json, JsonOptions);
            if (settings == null) return CreateDefault();

            // デフォルトに存在するが設定ファイルにないアクションを補完
            var defaults = GetDefaultBindings();
            foreach (var kvp in defaults)
            {
                if (!settings.Bindings.ContainsKey(kvp.Key))
                    settings.Bindings[kvp.Key] = kvp.Value;
            }
            return settings;
        }
        catch
        {
            return CreateDefault();
        }
    }

    /// <summary>
    /// 設定のコピーを作成
    /// </summary>
    public KeyBindingSettings Clone()
    {
        var clone = new KeyBindingSettings();
        foreach (var kvp in Bindings)
        {
            clone.Bindings[kvp.Key] = new KeyBinding(kvp.Value.Key, kvp.Value.Modifiers);
        }
        return clone;
    }
}
