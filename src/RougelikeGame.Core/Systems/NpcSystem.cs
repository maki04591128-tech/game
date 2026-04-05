namespace RougelikeGame.Core.Systems;

/// <summary>
/// NPC定義
/// </summary>
public record NpcDefinition(
    string Id,
    string Name,
    string Description,
    NpcType Type,
    TerritoryId Location,
    string[] DialogueIds,
    int InitialAffinity = 50)
{
    /// <summary>好感度段階名を取得</summary>
    public static string GetAffinityRank(int affinity) => affinity switch
    {
        >= 90 => "親友",
        >= 70 => "友好",
        >= 50 => "普通",
        >= 30 => "警戒",
        _ => "敵意"
    };

    /// <summary>全NPC定義</summary>
    private static readonly List<NpcDefinition> AllNpcs = new()
    {
        // 王都領
        new("npc_guild_master", "ギルドマスター・レオン", "冒険者ギルドの長。厳格だが面倒見がいい",
            NpcType.GuildMaster, TerritoryId.Capital, new[] { "dlg_leon_intro", "dlg_leon_quest" }, 50),
        new("npc_capital_shopkeeper", "商人マルコ", "王都の商店街で最大の店を営む商人",
            NpcType.Shopkeeper, TerritoryId.Capital, new[] { "dlg_marco_intro" }, 50),
        new("npc_capital_priest", "大司祭アルベール", "王都大聖堂の主。信仰に篤い老人",
            NpcType.Priest, TerritoryId.Capital, new[] { "dlg_albert_intro" }, 50),
        new("npc_capital_sage", "宮廷魔術師メルヴィン", "王に仕える魔術師。魔法言語に詳しい",
            NpcType.MagicShopkeeper, TerritoryId.Capital, new[] { "dlg_mervin_intro" }, 40),  // A-2
        // 森林領
        new("npc_forest_elder", "長老エルウェン", "エルフの集落の長老。森の知恵を持つ",
            NpcType.Sage, TerritoryId.Forest, new[] { "dlg_elwen_intro" }, 40),
        new("npc_forest_herbalist", "薬草師リーナ", "森の薬草に詳しい若い女性",
            NpcType.Alchemist, TerritoryId.Forest, new[] { "dlg_leena_intro" }, 60),
        new("npc_forest_ranger", "巡回士ガルド", "森林を守る巡回士。獣や魔物の情報に詳しい",
            NpcType.QuestGiver, TerritoryId.Forest, new[] { "dlg_gard_intro" }, 50),
        // 山岳領
        new("npc_mountain_smith", "鍛冶師ドワル", "山岳の鍛冶工房の親方。ミスリル加工の名人",
            NpcType.Blacksmith, TerritoryId.Mountain, new[] { "dlg_dwal_intro" }, 40),
        new("npc_mountain_miner", "坑夫長ブロック", "採掘隊を率いるドワーフ",
            NpcType.QuestGiver, TerritoryId.Mountain, new[] { "dlg_brock_intro" }, 50),
        // 沿岸領
        new("npc_coast_captain", "船長カリーナ", "港町の交易船の船長。世界各地の情報を持つ",
            NpcType.Wanderer, TerritoryId.Coast, new[] { "dlg_carina_intro" }, 50),
        new("npc_coast_innkeeper", "宿屋の女将ミラ", "港町の宿屋を営む気さくな女将",
            NpcType.Innkeeper, TerritoryId.Coast, new[] { "dlg_mira_intro" }, 60),
        new("npc_coast_fisherman", "漁師の老人トーマス", "海の魔物について語る老漁師",
            NpcType.Villager, TerritoryId.Coast, new[] { "dlg_thomas_intro" }, 50),
        // 南部領
        new("npc_southern_merchant", "行商人ハッサン", "砂漠を渡る行商人。珍しい品物を扱う",
            NpcType.BlackMarketDealer, TerritoryId.Southern, new[] { "dlg_hassan_intro" }, 50),  // A-3
        new("npc_southern_mystic", "占い師サーラ", "砂漠の神殿に住む神秘的な女性",
            NpcType.Sage, TerritoryId.Southern, new[] { "dlg_sara_intro" }, 30),
        // 辺境領
        new("npc_frontier_veteran", "老兵ヴォルフ", "辺境で魔物と戦い続ける老練の戦士",
            NpcType.QuestGiver, TerritoryId.Frontier, new[] { "dlg_wolf_intro" }, 40),
        new("npc_frontier_hermit", "隠者イゴール", "辺境の洞窟に住む謎の隠者",
            NpcType.Sage, TerritoryId.Frontier, new[] { "dlg_igor_intro" }, 20),
    };

    /// <summary>領地のNPC一覧を取得</summary>
    public static IReadOnlyList<NpcDefinition> GetByTerritory(TerritoryId territory)
        => AllNpcs.Where(n => n.Location == territory).ToList();

    /// <summary>全NPC一覧を取得</summary>
    public static IReadOnlyList<NpcDefinition> GetAll() => AllNpcs;

    /// <summary>IDでNPCを取得</summary>
    public static NpcDefinition? GetById(string id)
        => AllNpcs.FirstOrDefault(n => n.Id == id);
}

/// <summary>
/// NPCシステム - NPC配置・好感度・周回引き継ぎ管理
/// </summary>
public class NpcSystem
{
    private readonly Dictionary<string, NpcState> _npcStates = new();

    /// <summary>NPCの状態</summary>
    public class NpcState
    {
        public string NpcId { get; init; } = "";
        public int Affinity { get; set; } = 50;
        public HashSet<string> CompletedDialogues { get; } = new();
        public bool HasMet { get; set; }
    }

    /// <summary>NPCの状態を取得（初回は初期化）</summary>
    public NpcState GetNpcState(string npcId)
    {
        if (!_npcStates.TryGetValue(npcId, out var state))
        {
            state = new NpcState { NpcId = npcId };
            _npcStates[npcId] = state;
        }
        return state;
    }

    /// <summary>好感度を変更</summary>
    public void ModifyAffinity(string npcId, int amount)
    {
        var state = GetNpcState(npcId);
        state.Affinity = Math.Clamp(state.Affinity + amount, 0, 100);
    }

    /// <summary>NPCと初対面の処理</summary>
    public void MeetNpc(string npcId)
    {
        var state = GetNpcState(npcId);
        state.HasMet = true;
    }

    /// <summary>周回引き継ぎ - 好感度を80%引き継ぎ</summary>
    public Dictionary<string, int> CreateTransferData()
    {
        var data = new Dictionary<string, int>();
        foreach (var (id, state) in _npcStates)
        {
            data[id] = (int)(state.Affinity * 0.8);
        }
        return data;
    }

    /// <summary>引き継ぎデータを適用</summary>
    public void ApplyTransferData(Dictionary<string, int> data)
    {
        foreach (var (id, affinity) in data)
        {
            var state = GetNpcState(id);
            state.Affinity = affinity;
        }
    }

    /// <summary>
    /// 全NPC状態をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、NPC好感度・出会い記録は全て消失する。
    /// </summary>
    public void Reset()
    {
        _npcStates.Clear();
    }

    /// <summary>全NPC状態を取得（セーブ用）</summary>
    public IReadOnlyDictionary<string, NpcState> GetAllStates() => _npcStates;

    /// <summary>NPC状態を復元（ロード用）</summary>
    public void RestoreStates(Dictionary<string, NpcStateSaveData> data)
    {
        _npcStates.Clear();
        foreach (var (id, saveData) in data)
        {
            _npcStates[id] = new NpcState
            {
                NpcId = id,
                Affinity = saveData.Affinity,
                HasMet = saveData.HasMet
            };
            foreach (var dlg in saveData.CompletedDialogues)
            {
                _npcStates[id].CompletedDialogues.Add(dlg);
            }
        }
    }
}

/// <summary>
/// 会話ノード
/// </summary>
public record DialogueNode(
    string Id,
    string SpeakerName,
    string Text,
    DialogueChoice[]? Choices = null,
    string? NextNodeId = null,
    string? ConditionFlag = null)
{
    /// <summary>選択肢がある会話か</summary>
    public bool HasChoices => Choices != null && Choices.Length > 0;
}

/// <summary>
/// 会話選択肢
/// </summary>
public record DialogueChoice(
    string Text,
    string NextNodeId,
    int AffinityChange = 0,
    string? RequiredFlag = null);

/// <summary>
/// 会話システム - テキスト会話ウィンドウ・選択肢分岐管理
/// </summary>
public class DialogueSystem
{
    private readonly Dictionary<string, DialogueNode> _nodes = new();
    private readonly HashSet<string> _flags = new();

    /// <summary>現在の会話ノード</summary>
    public DialogueNode? CurrentNode { get; private set; }

    /// <summary>会話中かどうか</summary>
    public bool IsInDialogue => CurrentNode != null;

    /// <summary>会話ノードを登録</summary>
    public void RegisterNode(DialogueNode node)
    {
        _nodes[node.Id] = node;
    }

    /// <summary>複数の会話ノードを登録</summary>
    public void RegisterNodes(IEnumerable<DialogueNode> nodes)
    {
        foreach (var node in nodes)
        {
            _nodes[node.Id] = node;
        }
    }

    /// <summary>会話を開始</summary>
    public DialogueNode? StartDialogue(string nodeId)
    {
        if (!_nodes.TryGetValue(nodeId, out var node)) return null;

        // 条件フラグチェック
        if (node.ConditionFlag != null && !_flags.Contains(node.ConditionFlag))
            return null;

        CurrentNode = node;
        return node;
    }

    /// <summary>選択肢を選ぶ</summary>
    public DialogueResult? SelectChoice(int choiceIndex)
    {
        if (CurrentNode == null || !CurrentNode.HasChoices) return null;
        if (choiceIndex < 0 || choiceIndex >= CurrentNode.Choices!.Length) return null;

        var choice = CurrentNode.Choices[choiceIndex];

        // 条件フラグチェック
        if (choice.RequiredFlag != null && !_flags.Contains(choice.RequiredFlag))
            return new DialogueResult(false, "条件を満たしていない", null, 0);

        // 次のノードへ
        if (_nodes.TryGetValue(choice.NextNodeId, out var nextNode))
        {
            CurrentNode = nextNode;
            return new DialogueResult(true, nextNode.Text, nextNode, choice.AffinityChange);
        }

        // 終了
        CurrentNode = null;
        return new DialogueResult(true, "会話が終了した", null, choice.AffinityChange);
    }

    /// <summary>次のノードに進む（選択肢なしの場合）</summary>
    public DialogueNode? Advance()
    {
        if (CurrentNode == null) return null;
        if (CurrentNode.HasChoices) return CurrentNode; // 選択肢がある場合は進めない

        if (CurrentNode.NextNodeId != null && _nodes.TryGetValue(CurrentNode.NextNodeId, out var next))
        {
            CurrentNode = next;
            return next;
        }

        // 会話終了
        CurrentNode = null;
        return null;
    }

    /// <summary>会話を終了</summary>
    public void EndDialogue()
    {
        CurrentNode = null;
    }

    /// <summary>フラグを設定</summary>
    public void SetFlag(string flag) => _flags.Add(flag);

    /// <summary>フラグを確認</summary>
    public bool HasFlag(string flag) => _flags.Contains(flag);

    /// <summary>全フラグを取得（セーブ用）</summary>
    public IReadOnlySet<string> GetAllFlags() => _flags;

    /// <summary>フラグを復元（ロード用）</summary>
    public void RestoreFlags(IEnumerable<string> flags)
    {
        _flags.Clear();
        foreach (var flag in flags) _flags.Add(flag);
    }

    /// <summary>
    /// 全状態をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、会話フラグ・会話状態は全て消失する。
    /// 会話ノード定義は保持される（マスターデータ）。
    /// </summary>
    public void Reset()
    {
        _flags.Clear();
        CurrentNode = null;
    }
}

/// <summary>
/// 会話結果
/// </summary>
public record DialogueResult(
    bool Success,
    string Message,
    DialogueNode? NextNode,
    int AffinityChange);

/// <summary>
/// クエスト定義
/// </summary>
public record QuestDefinition(
    string Id,
    string Name,
    string Description,
    QuestType Type,
    string GiverNpcId,
    int RequiredLevel,
    GuildRank RequiredGuildRank,
    QuestObjective[] Objectives,
    QuestReward Reward);

/// <summary>
/// クエスト目標
/// </summary>
public record QuestObjective(
    string Description,
    string TargetId,
    int TargetCount,
    int CurrentCount = 0)
{
    /// <summary>目標達成済みか</summary>
    public bool IsComplete => CurrentCount >= TargetCount;
}

/// <summary>
/// クエスト報酬
/// </summary>
public record QuestReward(
    int Gold = 0,
    int Experience = 0,
    string[]? ItemIds = null,
    int GuildPoints = 0,
    int FaithPoints = 0);

/// <summary>
/// クエストシステム - クエスト進行フラグ管理・受注/完了/報酬
/// </summary>
public class QuestSystem
{
    private readonly Dictionary<string, QuestDefinition> _questDefinitions = new();
    private readonly Dictionary<string, QuestProgress> _activeQuests = new();
    private readonly HashSet<string> _completedQuests = new();

    /// <summary>クエスト進行状態</summary>
    public class QuestProgress
    {
        public string QuestId { get; init; } = "";
        public QuestState State { get; set; } = QuestState.Active;
        public List<QuestObjective> Objectives { get; init; } = new();
        public bool IsComplete => Objectives.All(o => o.IsComplete);
    }

    /// <summary>クエストを登録</summary>
    public void RegisterQuest(QuestDefinition quest)
    {
        _questDefinitions[quest.Id] = quest;
    }

    /// <summary>クエストを受注</summary>
    public QuestActionResult AcceptQuest(string questId, int playerLevel, GuildRank playerRank)
    {
        if (!_questDefinitions.TryGetValue(questId, out var quest))
            return new QuestActionResult(false, "不明なクエスト");

        if (_activeQuests.ContainsKey(questId))
            return new QuestActionResult(false, "既に受注済み");

        if (_completedQuests.Contains(questId))
            return new QuestActionResult(false, "既にクリア済み");

        if (playerLevel < quest.RequiredLevel)
            return new QuestActionResult(false, $"レベルが足りない（必要Lv.{quest.RequiredLevel}）");

        if (playerRank < quest.RequiredGuildRank)
            return new QuestActionResult(false, $"ギルドランクが足りない");

        _activeQuests[questId] = new QuestProgress
        {
            QuestId = questId,
            Objectives = quest.Objectives.ToList()
        };

        return new QuestActionResult(true, $"クエスト「{quest.Name}」を受注した");
    }

    /// <summary>クエスト目標を進行</summary>
    public void UpdateObjective(string targetId, int count = 1)
    {
        foreach (var progress in _activeQuests.Values.Where(p => p.State == QuestState.Active))
        {
            for (int i = 0; i < progress.Objectives.Count; i++)
            {
                var obj = progress.Objectives[i];
                if (obj.TargetId == targetId && !obj.IsComplete)
                {
                    progress.Objectives[i] = obj with { CurrentCount = obj.CurrentCount + count };
                }
            }

            if (progress.IsComplete)
            {
                progress.State = QuestState.Completed;
            }
        }
    }

    /// <summary>クエスト報酬を受け取る</summary>
    public QuestRewardResult TurnInQuest(string questId, Entities.Player player)
    {
        if (!_activeQuests.TryGetValue(questId, out var progress))
            return new QuestRewardResult(false, "このクエストは受注していない");

        if (progress.State != QuestState.Completed)
            return new QuestRewardResult(false, "クエストがまだ完了していない");

        if (!_questDefinitions.TryGetValue(questId, out var quest))
            return new QuestRewardResult(false, "不明なクエスト");

        // 報酬付与
        if (quest.Reward.Gold > 0) player.AddGold(quest.Reward.Gold);
        if (quest.Reward.Experience > 0) player.GainExperience(quest.Reward.Experience);

        // CQ-1: アイテム報酬の付与
        if (quest.Reward.ItemIds != null)
        {
            foreach (var itemId in quest.Reward.ItemIds)
            {
                var item = Items.ItemDefinitions.Create(itemId);
                if (item != null) ((Entities.Inventory)player.Inventory).Add(item);
            }
        }

        progress.State = QuestState.TurnedIn;
        _completedQuests.Add(questId);
        _activeQuests.Remove(questId);

        return new QuestRewardResult(true,
            $"クエスト「{quest.Name}」を完了！報酬: {quest.Reward.Gold}G, {quest.Reward.Experience}EXP",
            quest.Reward);
    }

    /// <summary>アクティブなクエスト一覧を取得</summary>
    public IReadOnlyList<(QuestDefinition Quest, QuestProgress Progress)> GetActiveQuests()
    {
        return _activeQuests.Values
            .Where(p => _questDefinitions.ContainsKey(p.QuestId))
            .Select(p => (_questDefinitions[p.QuestId], p))
            .ToList();
    }

    /// <summary>完了済みクエスト数を取得</summary>
    public int CompletedQuestCount => _completedQuests.Count;

    /// <summary>クエスト定義を取得</summary>
    public QuestDefinition? GetQuestDefinition(string questId)
        => _questDefinitions.TryGetValue(questId, out var quest) ? quest : null;

    /// <summary>完了済みクエストID一覧を取得</summary>
    public IReadOnlySet<string> CompletedQuestIds => _completedQuests;

    /// <summary>受注可能なクエスト一覧を取得（レベル・ランク・未受注チェック）</summary>
    public IReadOnlyList<QuestDefinition> GetAvailableQuests(int playerLevel, GuildRank playerRank)
    {
        return _questDefinitions.Values
            .Where(q => playerLevel >= q.RequiredLevel
                && playerRank >= q.RequiredGuildRank
                && !_activeQuests.ContainsKey(q.Id)
                && !_completedQuests.Contains(q.Id))
            .ToList();
    }

    /// <summary>複数のクエストを一括登録</summary>
    public void RegisterQuests(IEnumerable<QuestDefinition> quests)
    {
        foreach (var quest in quests)
            _questDefinitions[quest.Id] = quest;
    }

    /// <summary>クエスト状態を復元（ロード用）</summary>
    public void RestoreState(List<QuestProgressSaveData> activeQuests, List<string> completedQuests)
    {
        _activeQuests.Clear();
        _completedQuests.Clear();
        foreach (var completed in completedQuests)
            _completedQuests.Add(completed);
        foreach (var active in activeQuests)
        {
            _activeQuests[active.QuestId] = new QuestProgress
            {
                QuestId = active.QuestId,
                State = Enum.TryParse<QuestState>(active.State, out var state) ? state : QuestState.Active,
                Objectives = active.Objectives.Select(o =>
                    new QuestObjective(o.Description, o.TargetId, o.TargetCount, o.CurrentCount)).ToList()
            };
        }
    }

    /// <summary>アクティブクエストのセーブデータを生成</summary>
    public List<QuestProgressSaveData> CreateActiveQuestsSaveData()
    {
        return _activeQuests.Values.Select(p => new QuestProgressSaveData
        {
            QuestId = p.QuestId,
            State = p.State.ToString(),
            Objectives = p.Objectives.Select(o => new QuestObjectiveSaveData
            {
                Description = o.Description,
                TargetId = o.TargetId,
                TargetCount = o.TargetCount,
                CurrentCount = o.CurrentCount
            }).ToList()
        }).ToList();
    }

    /// <summary>メインクエスト定義を一括登録</summary>
    public void RegisterMainQuest()
    {
        var mainQuest = new QuestDefinition(
            Id: "main_quest_abyss",
            Name: "深淵の探索",
            Description: "ダンジョン最深部に潜む深淵の王を討伐せよ",
            Type: QuestType.Main,
            GiverNpcId: "npc_guild_master",
            RequiredLevel: 1,
            RequiredGuildRank: GuildRank.None,
            Objectives: new[]
            {
                new QuestObjective("5階ボス「キングスライム」を撃破", "floor_boss_5", 1),
                new QuestObjective("10階ボス「ゴブリンキング」を撃破", "floor_boss_10", 1),
                new QuestObjective("15階ボス「スケルトンロード」を撃破", "floor_boss_15", 1),
                new QuestObjective("20階ボス「ダークエルフ将軍」を撃破", "floor_boss_20", 1),
                new QuestObjective("25階ボス「炎竜ヴァルグレス」を撃破", "floor_boss_25", 1),
                new QuestObjective("30階ボス「深淵の王」を撃破", "floor_boss_30", 1),
            },
            Reward: new QuestReward(Gold: 10000, Experience: 5000)
        );
        RegisterQuest(mainQuest);
    }

    /// <summary>敵撃破時のクエスト目標自動更新</summary>
    public IReadOnlyList<string> UpdateKillObjective(string enemyTypeId)
    {
        var messages = new List<string>();
        UpdateObjective(enemyTypeId);

        foreach (var progress in _activeQuests.Values.Where(p => p.State == QuestState.Completed))
        {
            var quest = _questDefinitions.GetValueOrDefault(progress.QuestId);
            if (quest != null)
            {
                messages.Add($"📋 クエスト「{quest.Name}」の全目標を達成！報告可能になった");
            }
        }

        // ボス撃破チェックポイントメッセージ
        foreach (var progress in _activeQuests.Values.Where(p => p.State == QuestState.Active))
        {
            foreach (var obj in progress.Objectives)
            {
                if (obj.TargetId == enemyTypeId && obj.IsComplete)
                {
                    messages.Add($"📋 クエスト目標達成: {obj.Description}");
                }
            }
        }

        return messages;
    }

    /// <summary>フロア到達時のクエスト目標自動更新</summary>
    public IReadOnlyList<string> UpdateExploreObjective(int floor)
    {
        var messages = new List<string>();
        UpdateObjective($"floor_{floor}");
        return messages;
    }

    /// <summary>アイテム取得時のクエスト目標自動更新</summary>
    public IReadOnlyList<string> UpdateCollectObjective(string itemId)
    {
        var messages = new List<string>();
        UpdateObjective(itemId);
        return messages;
    }

    /// <summary>完了済みクエストの自動報酬付与</summary>
    public QuestRewardResult? AutoTurnInIfComplete(string questId, Entities.Player player)
    {
        if (!_activeQuests.TryGetValue(questId, out var progress)) return null;
        if (progress.State != QuestState.Completed) return null;
        return TurnInQuest(questId, player);
    }

    /// <summary>メインクエストが完了しているか</summary>
    public bool IsMainQuestComplete => _completedQuests.Contains("main_quest_abyss");

    /// <summary>
    /// 全クエスト進行をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、クエスト進行は全て消失する。
    /// クエスト定義（マスターデータ）は保持される。
    /// </summary>
    public void Reset()
    {
        _activeQuests.Clear();
        _completedQuests.Clear();
    }
}/// <summary>
/// クエストアクション結果
/// </summary>
public record QuestActionResult(bool Success, string Message);

/// <summary>
/// クエスト報酬結果
/// </summary>
public record QuestRewardResult(bool Success, string Message, QuestReward? Reward = null);

/// <summary>
/// 冒険者ギルドシステム - ギルドランク・依頼掲示板・報酬
/// </summary>
public class GuildSystem
{
    /// <summary>現在のギルドランク</summary>
    public GuildRank CurrentRank { get; private set; } = GuildRank.None;

    /// <summary>累計ギルドポイント</summary>
    public int GuildPoints { get; private set; }

    /// <summary>ギルドに登録</summary>
    public GuildActionResult Register()
    {
        if (CurrentRank != GuildRank.None)
            return new GuildActionResult(false, "既にギルドに登録済み");

        CurrentRank = GuildRank.Copper;
        GuildPoints = 0;
        return new GuildActionResult(true, "冒険者ギルドに登録した！ランク: 銅");
    }

    /// <summary>ギルドポイントを加算</summary>
    public GuildActionResult AddPoints(int points)
    {
        if (CurrentRank == GuildRank.None)
            return new GuildActionResult(false, "ギルド未登録");

        GuildPoints += points;

        // ランクアップ判定
        var newRank = CalculateRank(GuildPoints);
        if (newRank > CurrentRank)
        {
            var oldRank = CurrentRank;
            CurrentRank = newRank;
            return new GuildActionResult(true,
                $"ギルドランクが{GetRankName(oldRank)}から{GetRankName(newRank)}に昇格した！");
        }

        return new GuildActionResult(true, $"ギルドポイント+{points}（累計: {GuildPoints}）");
    }

    /// <summary>ランクアップに必要なポイント</summary>
    public int GetPointsForNextRank()
    {
        int required = GetRequiredPoints(CurrentRank + 1);
        return Math.Max(0, required - GuildPoints);
    }

    /// <summary>ギルドランク名を取得</summary>
    public static string GetRankName(GuildRank rank) => rank switch
    {
        GuildRank.Copper => "銅",
        GuildRank.Iron => "鉄",
        GuildRank.Silver => "銀",
        GuildRank.Gold => "金",
        GuildRank.Platinum => "白金",
        GuildRank.Mythril => "ミスリル",
        GuildRank.Adamantine => "アダマンタイト",
        _ => "未登録"
    };

    private static GuildRank CalculateRank(int points) => points switch
    {
        >= 10000 => GuildRank.Adamantine,
        >= 5000 => GuildRank.Mythril,
        >= 2500 => GuildRank.Platinum,
        >= 1000 => GuildRank.Gold,
        >= 400 => GuildRank.Silver,
        >= 100 => GuildRank.Iron,
        >= 0 => GuildRank.Copper,
        _ => GuildRank.None
    };

    private static int GetRequiredPoints(GuildRank rank) => rank switch
    {
        GuildRank.Iron => 100,
        GuildRank.Silver => 400,
        GuildRank.Gold => 1000,
        GuildRank.Platinum => 2500,
        GuildRank.Mythril => 5000,
        GuildRank.Adamantine => 10000,
        _ => 0
    };

    /// <summary>ギルド状態を復元（ロード用）</summary>
    public void RestoreState(GuildRank rank, int points)
    {
        CurrentRank = rank;
        GuildPoints = points;
    }

    /// <summary>登録されているかどうか</summary>
    public bool IsRegistered => CurrentRank != GuildRank.None;

    /// <summary>
    /// 全状態をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、ギルド登録・ポイントは全て消失する。
    /// </summary>
    public void Reset()
    {
        CurrentRank = GuildRank.None;
        GuildPoints = 0;
    }
}

/// <summary>
/// ギルドアクション結果
/// </summary>
public record GuildActionResult(bool Success, string Message);

/// <summary>
/// NPC状態のセーブデータ
/// </summary>
public class NpcStateSaveData
{
    public int Affinity { get; set; } = 50;
    public bool HasMet { get; set; }
    public List<string> CompletedDialogues { get; set; } = new();
}

/// <summary>
/// クエスト進行のセーブデータ
/// </summary>
public class QuestProgressSaveData
{
    public string QuestId { get; set; } = "";
    public string State { get; set; } = "Active";
    public List<QuestObjectiveSaveData> Objectives { get; set; } = new();
}

/// <summary>
/// クエスト目標のセーブデータ
/// </summary>
public class QuestObjectiveSaveData
{
    public string Description { get; set; } = "";
    public string TargetId { get; set; } = "";
    public int TargetCount { get; set; }
    public int CurrentCount { get; set; }
}

/// <summary>
/// 定義済みクエストデータベース
/// </summary>
public static class QuestDatabase
{
    /// <summary>全クエスト定義</summary>
    public static IReadOnlyList<QuestDefinition> AllQuests { get; } = new List<QuestDefinition>
    {
        // 銅ランク クエスト
        new("quest_rat_hunt", "地下倉庫のネズミ退治", "王都の地下倉庫に出没するネズミを退治してほしい",
            QuestType.Kill, "npc_guild_master", RequiredLevel: 1, RequiredGuildRank: GuildRank.Copper,
            Objectives: new[] { new QuestObjective("ネズミを5匹倒す", "enemy_rat", 5) },
            Reward: new QuestReward(Gold: 100, Experience: 50, GuildPoints: 10)),

        new("quest_herb_collect", "薬草の採集", "森で薬草を集めてきてほしい",
            QuestType.Collect, "npc_forest_herbalist", RequiredLevel: 1, RequiredGuildRank: GuildRank.Copper,
            Objectives: new[] { new QuestObjective("薬草を3つ集める", "material_herb", 3) },
            Reward: new QuestReward(Gold: 80, Experience: 30, GuildPoints: 8)),

        new("quest_forest_patrol", "森林巡回", "森林の安全を確認するため巡回してほしい",
            QuestType.Explore, "npc_forest_ranger", RequiredLevel: 3, RequiredGuildRank: GuildRank.Copper,
            Objectives: new[] { new QuestObjective("森林ダンジョン3階に到達する", "floor_forest_3", 1) },
            Reward: new QuestReward(Gold: 150, Experience: 80, GuildPoints: 15)),

        // 鉄ランク クエスト
        new("quest_bandit_clear", "山賊団の掃討", "山道に出没する山賊団を壊滅させてほしい",
            QuestType.Kill, "npc_guild_master", RequiredLevel: 5, RequiredGuildRank: GuildRank.Iron,
            Objectives: new[] { new QuestObjective("山賊を10人倒す", "enemy_bandit", 10) },
            Reward: new QuestReward(Gold: 300, Experience: 150, GuildPoints: 25)),

        new("quest_ore_delivery", "鉱石の配送", "山岳領の鉱石を王都に届けてほしい",
            QuestType.Deliver, "npc_mountain_miner", RequiredLevel: 5, RequiredGuildRank: GuildRank.Iron,
            Objectives: new[] { new QuestObjective("鉱石を王都のマルコに届ける", "deliver_ore_marco", 1) },
            Reward: new QuestReward(Gold: 250, Experience: 100, GuildPoints: 20)),

        new("quest_coast_monster", "海岸の魔物調査", "海岸に出現する魔物を調査してほしい",
            QuestType.Explore, "npc_coast_captain", RequiredLevel: 8, RequiredGuildRank: GuildRank.Iron,
            Objectives: new[] { new QuestObjective("海岸ダンジョン5階に到達する", "floor_coast_5", 1) },
            Reward: new QuestReward(Gold: 400, Experience: 200, GuildPoints: 30)),

        // 銀ランク クエスト
        new("quest_elder_talk", "長老への伝言", "エルフの長老に重要な伝言を届けてほしい",
            QuestType.Talk, "npc_guild_master", RequiredLevel: 10, RequiredGuildRank: GuildRank.Silver,
            Objectives: new[] { new QuestObjective("長老エルウェンに話しかける", "talk_elwen", 1) },
            Reward: new QuestReward(Gold: 500, Experience: 300, GuildPoints: 40)),

        new("quest_mine_boss", "鉱山のボス討伐", "鉱山の奥に潜むボスモンスターを討伐してほしい",
            QuestType.Kill, "npc_mountain_smith", RequiredLevel: 12, RequiredGuildRank: GuildRank.Silver,
            Objectives: new[] { new QuestObjective("鉱山のボスを倒す", "boss_mine_golem", 1) },
            Reward: new QuestReward(Gold: 800, Experience: 500, GuildPoints: 50)),

        // 金ランク クエスト
        new("quest_frontier_escort", "辺境への護衛依頼", "商人を辺境領まで護衛してほしい",
            QuestType.Escort, "npc_southern_merchant", RequiredLevel: 15, RequiredGuildRank: GuildRank.Gold,
            Objectives: new[] { new QuestObjective("ハッサンを辺境領まで護衛する", "escort_hassan", 1) },
            Reward: new QuestReward(Gold: 1500, Experience: 800, GuildPoints: 80)),

        new("quest_frontier_threat", "辺境の脅威排除", "辺境に出没する強大な魔物を排除してほしい",
            QuestType.Kill, "npc_frontier_veteran", RequiredLevel: 18, RequiredGuildRank: GuildRank.Gold,
            Objectives: new[] { new QuestObjective("辺境のボスを倒す", "boss_frontier_demon", 1) },
            Reward: new QuestReward(Gold: 2000, Experience: 1200, GuildPoints: 100)),

        // 白金ランク クエスト
        new("quest_ancient_ruins", "古代遺跡の調査", "南部に眠る古代遺跡を調査してほしい",
            QuestType.Explore, "npc_southern_mystic", RequiredLevel: 20, RequiredGuildRank: GuildRank.Platinum,
            Objectives: new[] { new QuestObjective("古代遺跡の最深部に到達する", "floor_ruins_10", 1) },
            Reward: new QuestReward(Gold: 3000, Experience: 2000, GuildPoints: 150)),
    };

    /// <summary>ギルドランクで受注可能なクエストを取得</summary>
    public static IReadOnlyList<QuestDefinition> GetByRank(GuildRank rank)
        => AllQuests.Where(q => q.RequiredGuildRank <= rank).ToList();

    /// <summary>IDでクエストを取得</summary>
    public static QuestDefinition? GetById(string id)
        => AllQuests.FirstOrDefault(q => q.Id == id);
}
