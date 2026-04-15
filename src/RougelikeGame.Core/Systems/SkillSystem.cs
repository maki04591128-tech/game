namespace RougelikeGame.Core.Systems;

/// <summary>
/// スキル定義
/// </summary>
public record SkillDefinition(
    string Id,
    string Name,
    string Description,
    SkillCategory Category,
    SkillTarget Target,
    int ManaCost,
    int SpCost,
    int Cooldown,
    int LevelRequired,
    CharacterClass? ClassRequired,
    string? PrerequisiteSkillId,
    double BasePower,
    Element Element = Element.None,
    int TurnCost = 3)
{
    /// <summary>スキルがクールダウン中でないか判定</summary>
    public bool IsReady(int currentCooldown) => currentCooldown <= 0;
}

/// <summary>
/// スキルツリーノード
/// </summary>
public record SkillTreeNode(
    string SkillId,
    int Tier,
    string[] Prerequisites)
{
    /// <summary>習得可能か判定</summary>
    public bool CanLearn(HashSet<string> learnedSkills, int playerLevel, CharacterClass playerClass)
    {
        var skill = SkillDatabase.GetById(SkillId);
        if (skill == null) return false;
        if (playerLevel < skill.LevelRequired) return false;
        if (skill.ClassRequired.HasValue && skill.ClassRequired.Value != playerClass) return false;
        return Prerequisites.All(p => learnedSkills.Contains(p));
    }
}

/// <summary>
/// スキルの実行時情報
/// </summary>
public class ActiveSkill
{
    public string SkillId { get; }
    public int CurrentCooldown { get; set; }

    public ActiveSkill(string skillId)
    {
        SkillId = skillId;
        CurrentCooldown = 0;
    }

    public void Use(int cooldown)
    {
        CurrentCooldown = cooldown;
    }

    public void TickCooldown()
    {
        if (CurrentCooldown > 0)
            CurrentCooldown--;
    }

    public bool IsReady => CurrentCooldown <= 0;
}

/// <summary>
/// スキルデータベース
/// </summary>
public static class SkillDatabase
{
    private static readonly Dictionary<string, SkillDefinition> _skills = new();
    private static readonly Dictionary<CharacterClass, List<SkillTreeNode>> _skillTrees = new();

    static SkillDatabase()
    {
        InitializeCombatSkills();
        InitializeMagicSkills();
        InitializeSupportSkills();
        InitializePassiveSkills();
        InitializeSkillTrees();
    }

    private static void InitializeCombatSkills()
    {
        // 戦士系
        Add(new("strong_strike", "強打", "力を込めた一撃。通常の1.5倍ダメージ", SkillCategory.Combat, SkillTarget.SingleEnemy, 0, 10, 3, 1, CharacterClass.Fighter, null, 1.5));
        Add(new("shield_bash", "盾打ち", "盾で殴りつけスタンさせる", SkillCategory.Combat, SkillTarget.SingleEnemy, 0, 15, 5, 3, CharacterClass.Knight, null, 1.0));
        Add(new("weapon_mastery", "武器習熟", "装備武器のダメージ+20%（パッシブ）", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 1, CharacterClass.Fighter, null, 0.20));
        Add(new("shield_block", "盾防御", "次の攻撃のダメージを50%軽減", SkillCategory.Combat, SkillTarget.Self, 0, 10, 4, 1, CharacterClass.Knight, null, 0.50));
        Add(new("provoke", "挑発", "周囲の敵を引き付ける", SkillCategory.Combat, SkillTarget.AllEnemies, 0, 20, 8, 5, CharacterClass.Knight, "shield_block", 1.0));
        Add(new("whirlwind", "旋風斬", "周囲の全敵に攻撃", SkillCategory.Combat, SkillTarget.AllEnemies, 0, 25, 6, 8, CharacterClass.Fighter, "strong_strike", 1.2));

        // 盗賊系
        Add(new("lockpick", "解錠", "鍵をこじ開ける。DEXで成功率上昇", SkillCategory.Exploration, SkillTarget.Self, 0, 5, 0, 1, CharacterClass.Thief, null, 0.0));
        Add(new("sneak", "忍び足", "敵に発見されにくくなる", SkillCategory.Support, SkillTarget.Self, 0, 10, 0, 1, CharacterClass.Thief, null, 0.0));
        Add(new("backstab", "バックスタブ", "隠密状態から3倍ダメージ", SkillCategory.Combat, SkillTarget.SingleEnemy, 0, 15, 5, 5, CharacterClass.Thief, "sneak", 3.0));
        Add(new("disarm_trap", "罠解除", "発見済みの罠を解除する", SkillCategory.Exploration, SkillTarget.Self, 0, 5, 0, 3, CharacterClass.Thief, "lockpick", 0.0));

        // 狩人系
        Add(new("precise_shot", "精密射撃", "必中の遠距離攻撃", SkillCategory.Combat, SkillTarget.SingleEnemy, 0, 15, 4, 1, CharacterClass.Ranger, null, 1.3));
        Add(new("tracking", "追跡", "敵の位置を把握する", SkillCategory.Exploration, SkillTarget.Self, 0, 10, 10, 1, CharacterClass.Ranger, null, 0.0));
        Add(new("multi_shot", "連射", "複数の敵に同時射撃", SkillCategory.Combat, SkillTarget.AllEnemies, 0, 25, 6, 7, CharacterClass.Ranger, "precise_shot", 0.8));

        // 修道士系
        Add(new("ki_strike", "気功", "MPではなく内気で攻撃。VIT依存ダメージ", SkillCategory.Combat, SkillTarget.SingleEnemy, 0, 15, 3, 1, CharacterClass.Monk, null, 1.2));
        Add(new("combo_strike", "連打", "3連続攻撃（各0.6倍ダメージ）", SkillCategory.Combat, SkillTarget.SingleEnemy, 0, 20, 4, 1, CharacterClass.Monk, null, 0.6, TurnCost: 5));
        Add(new("meditation", "瞑想", "HP・MP・SPを少量回復", SkillCategory.Support, SkillTarget.Self, 0, 0, 10, 5, CharacterClass.Monk, "ki_strike", 0.15, TurnCost: 10));

        // 上位戦技（Tier 3）
        Add(new("blade_dance", "刃の舞", "5連続斬撃。各0.5倍ダメージで合計2.5倍。回避不能の剣舞", SkillCategory.Combat, SkillTarget.SingleEnemy, 0, 35, 8, 12, CharacterClass.Fighter, "whirlwind", 0.5, TurnCost: 6));
        Add(new("counter_stance", "反撃の構え", "3ターンの間、受けた近接攻撃に自動反撃する構えを取る", SkillCategory.Combat, SkillTarget.Self, 0, 20, 10, 10, CharacterClass.Knight, "provoke", 1.2));
        Add(new("assassinate", "暗殺", "HP30%以下の敵を即死させる。失敗時も3倍ダメージ", SkillCategory.Combat, SkillTarget.SingleEnemy, 0, 30, 10, 12, CharacterClass.Thief, "backstab", 3.0, Element.Dark));
        Add(new("rain_of_arrows", "矢の雨", "空を覆う矢の豪雨。全敵に1.8倍ダメージ", SkillCategory.Combat, SkillTarget.AllEnemies, 0, 35, 8, 12, CharacterClass.Ranger, "multi_shot", 1.8));
        Add(new("iron_fist", "鉄拳", "防御力無視の渾身の一撃。装甲を貫通する", SkillCategory.Combat, SkillTarget.SingleEnemy, 0, 30, 6, 12, CharacterClass.Monk, "combo_strike", 2.5));
        Add(new("dual_wield", "二刀流", "二刀流戦闘を習得。通常攻撃時に追加攻撃が発生する", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 8, CharacterClass.Fighter, "weapon_mastery", 0.30));
        Add(new("parry", "受け流し", "次の攻撃を無効化し、隙を突いて反撃する", SkillCategory.Combat, SkillTarget.Self, 0, 15, 5, 6, CharacterClass.Knight, "shield_block", 1.5));
        Add(new("shadow_step", "影渡り", "影を伝って敵の背後に瞬間移動し、次の攻撃のダメージ2倍", SkillCategory.Combat, SkillTarget.SingleEnemy, 5, 15, 4, 7, CharacterClass.Thief, "sneak", 2.0, Element.Dark));
        Add(new("eagle_eye", "鷹の目", "クリティカル率+15%。弱点を見抜く狩人の眼力", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 10, CharacterClass.Ranger, "precise_shot", 0.15));
        Add(new("inner_peace", "内なる平穏", "毎ターンHPを最大値の3%回復する精神統一の境地", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 10, CharacterClass.Monk, "meditation", 0.03));
    }

    private static void InitializeMagicSkills()
    {
        // 魔術師系
        Add(new("mana_focus", "魔力集中", "次の魔法の威力+50%", SkillCategory.Magic, SkillTarget.Self, 5, 0, 5, 1, CharacterClass.Mage, null, 1.5));
        Add(new("basic_magic", "基礎魔法", "基本的な魔法攻撃", SkillCategory.Magic, SkillTarget.SingleEnemy, 8, 0, 0, 1, CharacterClass.Mage, null, 1.0, Element.None));
        Add(new("fireball", "ファイアボール", "範囲火属性攻撃", SkillCategory.Magic, SkillTarget.Area, 15, 0, 4, 5, CharacterClass.Mage, "basic_magic", 1.5, Element.Fire));
        Add(new("ice_storm", "アイスストーム", "範囲氷属性攻撃", SkillCategory.Magic, SkillTarget.Area, 18, 0, 5, 8, CharacterClass.Mage, "basic_magic", 1.4, Element.Ice));
        Add(new("arcane_shield", "魔法障壁", "魔法防御力を一時的に大幅上昇", SkillCategory.Support, SkillTarget.Self, 12, 0, 8, 6, CharacterClass.Mage, "mana_focus", 0.50));

        // 僧侶系
        Add(new("heal", "回復術", "HPを回復する", SkillCategory.Magic, SkillTarget.Self, 10, 0, 0, 1, CharacterClass.Cleric, null, 1.0, Element.Light));
        Add(new("purify", "浄化", "状態異常を1つ解除", SkillCategory.Support, SkillTarget.Self, 8, 0, 3, 1, CharacterClass.Cleric, null, 1.0));
        Add(new("holy_light", "聖光", "アンデッドに大ダメージ", SkillCategory.Magic, SkillTarget.SingleEnemy, 12, 0, 4, 5, CharacterClass.Cleric, "heal", 2.0, Element.Holy));
        Add(new("blessing", "祝福", "一定時間全ステータス微増", SkillCategory.Support, SkillTarget.Self, 15, 0, 10, 7, CharacterClass.Cleric, "purify", 0.10));

        // 死霊術師系
        Add(new("summon_undead", "死霊召喚", "骸骨の味方を召喚", SkillCategory.Magic, SkillTarget.Self, 20, 0, 8, 1, CharacterClass.Necromancer, null, 1.0, Element.Dark));
        Add(new("life_drain", "生命吸収", "敵のHPを吸収する", SkillCategory.Magic, SkillTarget.SingleEnemy, 12, 0, 3, 1, CharacterClass.Necromancer, null, 1.0, Element.Dark));
        Add(new("curse", "呪詛", "敵の全ステータスを低下", SkillCategory.Magic, SkillTarget.SingleEnemy, 15, 0, 6, 5, CharacterClass.Necromancer, "life_drain", 0.20, Element.Curse));

        // 上位魔法（Tier 3）
        Add(new("meteor", "メテオ", "灼熱の隕石を降らせ全敵を焼き尽くす。最強の火属性魔法", SkillCategory.Magic, SkillTarget.AllEnemies, 45, 0, 10, 15, CharacterClass.Mage, "fireball", 3.0, Element.Fire, TurnCost: 5));
        Add(new("blizzard", "ブリザード", "絶対零度の吹雪が全敵を凍結させる。凍結した敵は2ターン行動不能", SkillCategory.Magic, SkillTarget.AllEnemies, 40, 0, 10, 15, CharacterClass.Mage, "ice_storm", 2.5, Element.Ice, TurnCost: 5));
        Add(new("thunder_god", "雷神", "天より降り注ぐ神雷。単体に壊滅的な雷撃を叩き込む", SkillCategory.Magic, SkillTarget.SingleEnemy, 35, 0, 6, 12, CharacterClass.Mage, "basic_magic", 3.5, Element.Lightning));
        Add(new("resurrection", "蘇生", "死の淵から生命を引き戻す奇跡。HP50%で復活", SkillCategory.Magic, SkillTarget.Self, 50, 0, 20, 15, CharacterClass.Cleric, "blessing", 0.50, Element.Holy, TurnCost: 8));
        Add(new("mass_heal", "全体回復", "聖なる光が味方全体を包み、傷を癒す大治癒の奇跡", SkillCategory.Magic, SkillTarget.AllAllies, 35, 0, 8, 12, CharacterClass.Cleric, "heal", 1.5, Element.Light));
        Add(new("army_of_dead", "死者の軍勢", "大地から無数の死霊を呼び起こし、亡者の軍団を従える", SkillCategory.Magic, SkillTarget.Self, 50, 0, 15, 15, CharacterClass.Necromancer, "summon_undead", 2.0, Element.Dark, TurnCost: 8));
        Add(new("soul_cage", "魂の牢獄", "敵の魂を捕縛し、死亡時の復活を封じる呪縛", SkillCategory.Magic, SkillTarget.SingleEnemy, 25, 0, 8, 10, CharacterClass.Necromancer, "curse", 1.0, Element.Curse));
        Add(new("arcane_explosion", "魔力爆発", "自身を中心に蓄積魔力を一気に解放。周囲全体に大ダメージ", SkillCategory.Magic, SkillTarget.AllEnemies, 30, 0, 7, 10, CharacterClass.Mage, "mana_focus", 2.0));
        Add(new("divine_judgment", "神罰", "天からの聖なる裁きで敵を打ち、その生命力で自身を癒す", SkillCategory.Magic, SkillTarget.SingleEnemy, 30, 0, 6, 10, CharacterClass.Cleric, "holy_light", 2.0, Element.Holy));
        Add(new("death_fog", "死の霧", "猛毒の瘴気を広範囲に展開。毎ターン毒ダメージを与え続ける", SkillCategory.Magic, SkillTarget.AllEnemies, 30, 0, 8, 10, CharacterClass.Necromancer, "life_drain", 0.8, Element.Poison, TurnCost: 4));
    }

    private static void InitializeSupportSkills()
    {
        // 吟遊詩人系
        Add(new("inspire_song", "鼓舞の歌", "味方全体の攻撃力を一時的に上昇", SkillCategory.Support, SkillTarget.AllAllies, 8, 0, 6, 1, CharacterClass.Bard, null, 0.20));
        Add(new("knowledge_collect", "知識収集", "アイテムを自動鑑定する", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 1, CharacterClass.Bard, null, 0.0));
        Add(new("lullaby", "子守唄", "周囲の敵を眠らせる", SkillCategory.Support, SkillTarget.AllEnemies, 12, 0, 8, 5, CharacterClass.Bard, "inspire_song", 1.0));
        Add(new("charm", "魅了の旋律", "敵1体を一時的に味方にする", SkillCategory.Support, SkillTarget.SingleEnemy, 18, 0, 10, 8, CharacterClass.Bard, "lullaby", 1.0));

        // 錬金術師系
        Add(new("brew", "調合", "素材からポーションを作成", SkillCategory.Crafting, SkillTarget.Self, 5, 0, 0, 1, CharacterClass.Alchemist, null, 0.0, TurnCost: 10));
        Add(new("identify", "鑑定", "未鑑定アイテムを鑑定する", SkillCategory.Support, SkillTarget.Self, 3, 0, 0, 1, CharacterClass.Alchemist, null, 0.0));
        Add(new("enchant", "付与", "武器に一時的な属性を付与", SkillCategory.Crafting, SkillTarget.Self, 15, 0, 10, 5, CharacterClass.Alchemist, "brew", 0.0, TurnCost: 15));
        Add(new("transmute", "変成", "素材を別の素材に変換", SkillCategory.Crafting, SkillTarget.Self, 10, 0, 5, 7, CharacterClass.Alchemist, "brew", 0.0, TurnCost: 20));

        // 上位支援スキル（Tier 3）
        Add(new("war_cry", "鬨の声", "味方全体の攻撃力上昇＋敵全体に恐怖付与。戦場を支配する咆哮", SkillCategory.Support, SkillTarget.AllAllies, 20, 0, 10, 12, CharacterClass.Bard, "charm", 0.30));
        Add(new("song_of_peace", "平和の歌", "5ターンの間、全ての戦闘行為を封じる禁断の旋律", SkillCategory.Support, SkillTarget.AllEnemies, 25, 0, 15, 10, CharacterClass.Bard, "lullaby", 0.0, TurnCost: 4));
        Add(new("philosopher_stone", "賢者の石", "あらゆる素材を望みの物質に変換する究極の錬金術", SkillCategory.Crafting, SkillTarget.Self, 30, 0, 20, 15, CharacterClass.Alchemist, "transmute", 0.0, TurnCost: 25));
        Add(new("elixir", "エリクサー", "万病を癒し活力を与える究極の霊薬を調合する", SkillCategory.Crafting, SkillTarget.Self, 20, 0, 12, 10, CharacterClass.Alchemist, "brew", 1.0, TurnCost: 15));
        Add(new("smoke_bomb", "煙幕", "煙幕を張り確実に戦闘から離脱する。逃走成功率100%", SkillCategory.Support, SkillTarget.Self, 0, 20, 6, 5, CharacterClass.Thief, "sneak", 0.0));
        Add(new("battle_plan", "戦術指揮", "味方全体の防御力を上昇させる的確な戦術指示", SkillCategory.Support, SkillTarget.AllAllies, 0, 25, 8, 8, CharacterClass.Knight, "provoke", 0.25));
    }

    private static void InitializePassiveSkills()
    {
        // 共通パッシブ（クラス不問）
        Add(new("hp_boost", "体力強化", "最大HP+10%", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 5, null, null, 0.10));
        Add(new("mp_boost", "魔力強化", "最大MP+10%", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 5, null, null, 0.10));
        Add(new("poison_resist", "毒耐性", "毒ダメージ半減", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 8, null, null, 0.50));
        Add(new("critical_eye", "鋭い眼", "クリティカル率+5%", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 10, null, null, 0.05));
        Add(new("treasure_sense", "宝探しの勘", "隠しアイテム発見率上昇", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 7, null, null, 0.20));

        // CQ-2: 宗教スキル（信仰段階に応じて解放）
        // 光の神殿
        Add(new("holy_light", "聖なる光", "光属性で範囲ダメージ＋アンデッド特攻", SkillCategory.Magic, SkillTarget.AllEnemies, 25, 0, 3, 1, null, null, 1.5, Element.Holy));
        Add(new("purify", "浄化", "全状態異常を解除", SkillCategory.Support, SkillTarget.Self, 15, 0, 5, 1, null, null, 1.0));
        Add(new("divine_protection", "神の加護", "一定ターン被ダメージ30%減", SkillCategory.Support, SkillTarget.Self, 30, 0, 8, 1, null, null, 0.30));
        Add(new("divine_miracle", "神の奇跡", "HP全回復＋全状態異常解除", SkillCategory.Support, SkillTarget.Self, 60, 0, 15, 1, null, null, 1.0));
        // 闇の教団
        Add(new("dark_pact", "闇の契約", "HP消費で攻撃力大幅上昇", SkillCategory.Support, SkillTarget.Self, 0, 0, 5, 1, null, null, 0.50));
        Add(new("shadow_strike", "影の一撃", "闇属性の高威力単体攻撃", SkillCategory.Combat, SkillTarget.SingleEnemy, 20, 0, 2, 1, null, null, 2.0, Element.Dark));
        Add(new("summon_undead", "アンデッド召喚", "アンデッドの味方を召喚", SkillCategory.Magic, SkillTarget.Self, 40, 0, 10, 1, null, null, 1.0));
        // 自然崇拝
        Add(new("nature_heal", "自然の癒し", "周囲の自然力でHP回復", SkillCategory.Support, SkillTarget.Self, 20, 0, 4, 1, null, null, 0.40));
        Add(new("beast_summon", "獣召喚", "野生の獣を味方として召喚", SkillCategory.Magic, SkillTarget.Self, 35, 0, 8, 1, null, null, 1.0));
        Add(new("shapeshift", "変身", "一時的に獣の姿に変身し攻撃力上昇", SkillCategory.Support, SkillTarget.Self, 25, 0, 6, 1, null, null, 0.50));
        Add(new("world_tree_protection", "世界樹の加護", "全属性耐性上昇", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 1, null, null, 0.25));
        // 死の信仰
        Add(new("death_premonition", "死の予兆", "敵の残りHPを可視化", SkillCategory.Support, SkillTarget.Self, 10, 0, 3, 1, null, null, 0.0));
        Add(new("soul_harvest", "魂の収穫", "敵撃破時にMP回復", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 1, null, null, 0.15));
        Add(new("guide_of_dead", "死者の導き", "アンデッド系敵が非敵対化", SkillCategory.Support, SkillTarget.Self, 30, 0, 10, 1, null, null, 0.0));
        Add(new("death_sentence", "死の宣告", "3ターン後に対象即死（ボス耐性あり）", SkillCategory.Magic, SkillTarget.SingleEnemy, 50, 0, 12, 1, null, null, 1.0, Element.Dark));
        // 混沌の教団
        Add(new("chaos_wave", "混沌の波動", "ランダム属性の範囲攻撃", SkillCategory.Magic, SkillTarget.AllEnemies, 30, 0, 4, 1, null, null, 1.3));
        Add(new("reality_warp", "現実歪曲", "ランダムな有益効果を自分に付与", SkillCategory.Support, SkillTarget.Self, 25, 0, 6, 1, null, null, 0.0));
        Add(new("mutation_release", "突然変異解放", "全ステータスランダム変動", SkillCategory.Support, SkillTarget.Self, 40, 0, 10, 1, null, null, 0.0));
        Add(new("chaos_vortex", "混沌の渦", "敵全体にランダム状態異常", SkillCategory.Magic, SkillTarget.AllEnemies, 50, 0, 8, 1, null, null, 0.8));

        // 上位パッシブ・隠しスキル（クラス不問）
        Add(new("death_defiance", "死の抗い", "致死ダメージを受けた際、フロアにつき1回だけHP1で生存する", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 12, null, null, 1.0));
        Add(new("mana_shield", "魔力障壁", "HPダメージの30%をMPで肩代わりする。MPが尽きると無効", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 10, null, null, 0.30));
        Add(new("berserker_rage", "狂戦士の怒り", "HPが減少するほど攻撃力が上昇。HP10%以下で攻撃力2倍", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 8, null, null, 2.0));
        Add(new("fortune_blessing", "幸運の祝福", "敵撃破時のアイテムドロップ率が25%上昇する", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 7, null, null, 0.25));
        Add(new("scholar_insight", "学者の洞察", "戦闘で得られる経験値が20%増加する知識の蓄積", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 5, null, null, 0.20));
        Add(new("survival_instinct", "生存本能", "HP20%以下の時、敵の攻撃を25%の確率で自動回避する", SkillCategory.Passive, SkillTarget.Self, 0, 0, 0, 10, null, null, 0.25));
    }

    private static void InitializeSkillTrees()
    {
        // 戦士スキルツリー
        _skillTrees[CharacterClass.Fighter] = new()
        {
            new("strong_strike", 1, Array.Empty<string>()),
            new("weapon_mastery", 1, Array.Empty<string>()),
            new("whirlwind", 2, new[] { "strong_strike" }),
            new("hp_boost", 2, Array.Empty<string>()),
            new("critical_eye", 3, new[] { "weapon_mastery" }),
            new("blade_dance", 3, new[] { "whirlwind" }),
            new("dual_wield", 3, new[] { "weapon_mastery" })
        };

        _skillTrees[CharacterClass.Knight] = new()
        {
            new("shield_block", 1, Array.Empty<string>()),
            new("shield_bash", 1, Array.Empty<string>()),
            new("provoke", 2, new[] { "shield_block" }),
            new("hp_boost", 2, Array.Empty<string>()),
            new("counter_stance", 3, new[] { "provoke" }),
            new("parry", 3, new[] { "shield_block" }),
            new("battle_plan", 3, new[] { "provoke" })
        };

        _skillTrees[CharacterClass.Thief] = new()
        {
            new("lockpick", 1, Array.Empty<string>()),
            new("sneak", 1, Array.Empty<string>()),
            new("backstab", 2, new[] { "sneak" }),
            new("disarm_trap", 2, new[] { "lockpick" }),
            new("treasure_sense", 3, Array.Empty<string>()),
            new("assassinate", 3, new[] { "backstab" }),
            new("shadow_step", 3, new[] { "sneak" }),
            new("smoke_bomb", 3, new[] { "sneak" })
        };

        _skillTrees[CharacterClass.Ranger] = new()
        {
            new("precise_shot", 1, Array.Empty<string>()),
            new("tracking", 1, Array.Empty<string>()),
            new("multi_shot", 2, new[] { "precise_shot" }),
            new("critical_eye", 3, Array.Empty<string>()),
            new("rain_of_arrows", 3, new[] { "multi_shot" }),
            new("eagle_eye", 3, new[] { "precise_shot" })
        };

        _skillTrees[CharacterClass.Mage] = new()
        {
            new("mana_focus", 1, Array.Empty<string>()),
            new("basic_magic", 1, Array.Empty<string>()),
            new("fireball", 2, new[] { "basic_magic" }),
            new("ice_storm", 2, new[] { "basic_magic" }),
            new("arcane_shield", 2, new[] { "mana_focus" }),
            new("mp_boost", 3, Array.Empty<string>()),
            new("meteor", 3, new[] { "fireball" }),
            new("blizzard", 3, new[] { "ice_storm" }),
            new("thunder_god", 3, new[] { "basic_magic" }),
            new("arcane_explosion", 3, new[] { "mana_focus" })
        };

        _skillTrees[CharacterClass.Cleric] = new()
        {
            new("heal", 1, Array.Empty<string>()),
            new("purify", 1, Array.Empty<string>()),
            new("holy_light", 2, new[] { "heal" }),
            new("blessing", 2, new[] { "purify" }),
            new("hp_boost", 3, Array.Empty<string>()),
            new("resurrection", 3, new[] { "blessing" }),
            new("mass_heal", 3, new[] { "heal" }),
            new("divine_judgment", 3, new[] { "holy_light" })
        };

        _skillTrees[CharacterClass.Monk] = new()
        {
            new("ki_strike", 1, Array.Empty<string>()),
            new("combo_strike", 1, Array.Empty<string>()),
            new("meditation", 2, new[] { "ki_strike" }),
            new("hp_boost", 2, Array.Empty<string>()),
            new("critical_eye", 3, Array.Empty<string>()),
            new("iron_fist", 3, new[] { "combo_strike" }),
            new("inner_peace", 3, new[] { "meditation" })
        };

        _skillTrees[CharacterClass.Bard] = new()
        {
            new("inspire_song", 1, Array.Empty<string>()),
            new("knowledge_collect", 1, Array.Empty<string>()),
            new("lullaby", 2, new[] { "inspire_song" }),
            new("charm", 3, new[] { "lullaby" }),
            new("war_cry", 3, new[] { "charm" }),
            new("song_of_peace", 3, new[] { "lullaby" })
        };

        _skillTrees[CharacterClass.Alchemist] = new()
        {
            new("brew", 1, Array.Empty<string>()),
            new("identify", 1, Array.Empty<string>()),
            new("enchant", 2, new[] { "brew" }),
            new("transmute", 2, new[] { "brew" }),
            new("poison_resist", 3, Array.Empty<string>()),
            new("philosopher_stone", 3, new[] { "transmute" }),
            new("elixir", 3, new[] { "brew" })
        };

        _skillTrees[CharacterClass.Necromancer] = new()
        {
            new("summon_undead", 1, Array.Empty<string>()),
            new("life_drain", 1, Array.Empty<string>()),
            new("curse", 2, new[] { "life_drain" }),
            new("mp_boost", 2, Array.Empty<string>()),
            new("army_of_dead", 3, new[] { "summon_undead" }),
            new("soul_cage", 3, new[] { "curse" }),
            new("death_fog", 3, new[] { "life_drain" })
        };
    }

    private static void Add(SkillDefinition skill)
    {
        _skills[skill.Id] = skill;
    }

    public static SkillDefinition? GetById(string id) =>
        _skills.TryGetValue(id, out var skill) ? skill : null;

    public static IEnumerable<SkillDefinition> GetByCategory(SkillCategory category) =>
        _skills.Values.Where(s => s.Category == category);

    public static IEnumerable<SkillDefinition> GetByClass(CharacterClass cls) =>
        _skills.Values.Where(s => s.ClassRequired == cls);

    public static IReadOnlyList<SkillTreeNode> GetSkillTree(CharacterClass cls) =>
        _skillTrees.TryGetValue(cls, out var tree) ? tree : Array.Empty<SkillTreeNode>();

    public static IEnumerable<SkillDefinition> GetAll() => _skills.Values;

    public static int Count => _skills.Count;
}

/// <summary>
/// スキルシステム - スキルの使用・習得・クールダウン管理
/// </summary>
public class SkillSystem
{
    private readonly Dictionary<string, ActiveSkill> _activeSkills = new();

    /// <summary>スキルを登録（習得時に呼ぶ）</summary>
    public void RegisterSkill(string skillId)
    {
        if (!_activeSkills.ContainsKey(skillId))
        {
            _activeSkills[skillId] = new ActiveSkill(skillId);
        }
    }

    /// <summary>スキルが使用可能か判定</summary>
    public bool CanUse(string skillId, int currentMp, int currentSp)
    {
        var skill = SkillDatabase.GetById(skillId);
        if (skill == null) return false;
        if (!_activeSkills.TryGetValue(skillId, out var active)) return false;
        if (!active.IsReady) return false;
        if (currentMp < skill.ManaCost) return false;
        if (currentSp < skill.SpCost) return false;
        return true;
    }

    /// <summary>スキルを使用してクールダウンを開始</summary>
    public SkillUseResult Use(string skillId, int currentMp, int currentSp)
    {
        var skill = SkillDatabase.GetById(skillId);
        if (skill == null) return new SkillUseResult(false, "不明なスキル");
        if (!_activeSkills.TryGetValue(skillId, out var active))
            return new SkillUseResult(false, "スキル未習得");
        if (!active.IsReady)
            return new SkillUseResult(false, $"クールダウン中（残り{active.CurrentCooldown}ターン）");
        if (currentMp < skill.ManaCost)
            return new SkillUseResult(false, "MPが足りない");
        if (currentSp < skill.SpCost)
            return new SkillUseResult(false, "SPが足りない");

        active.Use(skill.Cooldown);
        return new SkillUseResult(true, $"{skill.Name}を使用した！", skill.ManaCost, skill.SpCost, skill.TurnCost);
    }

    /// <summary>全スキルのクールダウンを1ターン進める</summary>
    public void TickCooldowns()
    {
        foreach (var active in _activeSkills.Values)
        {
            active.TickCooldown();
        }
    }

    /// <summary>スキルの現在のクールダウンを取得</summary>
    public int GetCooldown(string skillId) =>
        _activeSkills.TryGetValue(skillId, out var active) ? active.CurrentCooldown : 0;

    /// <summary>習得可能なスキルを取得</summary>
    public IEnumerable<SkillTreeNode> GetLearnableSkills(CharacterClass cls, HashSet<string> learnedSkills, int level) =>
        SkillDatabase.GetSkillTree(cls)
            .Where(n => !learnedSkills.Contains(n.SkillId) && n.CanLearn(learnedSkills, level, cls));

    /// <summary>全スキルのクールダウン状態を取得（セーブ用）</summary>
    public Dictionary<string, int> GetCooldownState()
    {
        var state = new Dictionary<string, int>();
        foreach (var (id, active) in _activeSkills)
        {
            if (active.CurrentCooldown > 0)
                state[id] = active.CurrentCooldown;
        }
        return state;
    }

    /// <summary>クールダウン状態を復元（ロード用）</summary>
    public void RestoreCooldownState(Dictionary<string, int> state)
    {
        foreach (var (id, cooldown) in state)
        {
            if (_activeSkills.TryGetValue(id, out var active))
            {
                active.CurrentCooldown = cooldown;
            }
        }
    }

    /// <summary>AR-4: 習得済みパッシブスキルの累計ボーナスを取得（カテゴリ名で検索）</summary>
    public double GetPassiveBonus(string skillId)
    {
        if (!_activeSkills.ContainsKey(skillId)) return 0;
        var skill = SkillDatabase.GetById(skillId);
        return skill?.Category == SkillCategory.Passive ? skill.BasePower : 0;
    }

    /// <summary>習得済みパッシブスキルのボーナス合計（カテゴリフィルタ）</summary>
    public double GetTotalPassiveBonus(SkillCategory category)
    {
        double total = 0;
        foreach (var id in _activeSkills.Keys)
        {
            var skill = SkillDatabase.GetById(id);
            if (skill?.Category == SkillCategory.Passive)
                total += skill.BasePower;
        }
        return total;
    }
}

/// <summary>
/// スキル使用結果
/// </summary>
public record SkillUseResult(
    bool Success,
    string Message,
    int MpCost = 0,
    int SpCost = 0,
    int TurnCost = 0);
