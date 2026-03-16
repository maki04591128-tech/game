# 実装計画書 Ver.prt.0.1（プロトタイプ完成）

**目標**: 全ゲームシステムの実装完了・プロトタイプとして遊べる状態
**達成条件**: 本計画書の全タスク完了時点
**状態**: ✅ 完了（Phase 1-6 全タスク完了）

---

## 1. 概要

Ver.prt.0.1 は全てのゲームシステムを実装し、プロトタイプとして一通り遊べる状態を目指す。
基盤構築（Phase 1-3）は完了済み。ゲームプレイ拡張（Phase 4 残り）、
コンテンツのシステム基盤（Phase 5 システム部分）、リリース準備基盤（Phase 6.1-6.4）を
全て完了させることでマイルストーンを達成する。

> **注**: テキストコンテンツ（ストーリー、NPC会話内容、クエスト詳細、フレーバーテキスト等）は
> Ver.α で取り組むため、本バージョンではシステム・仕組みの実装に集中する。

---

## 2. Phase 1: 基盤構築 ✅

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 1.1 | ソリューション・プロジェクト構成作成 | ✅ | Core/Engine/Data/Gui/App の5プロジェクト |
| 1.2 | エンティティ基底クラス（Character） | ✅ | BaseStats/EffectiveStats、HP/MP/SP管理 |
| 1.3 | 値オブジェクト（Position, Stats, Damage） | ✅ | Stats: 9基本ステータス+派生計算プロパティ |
| 1.4 | 列挙型・インターフェース定義 | ✅ | Element, DamageType, EquipmentSlot 等 |
| 1.5 | テスト基盤構築（xUnit） | ✅ | Core.Tests + Gui.Tests |
| 1.6 | UTF-8エンコーディング対応 | ✅ | コンソール・WPF両対応 |

---

## 3. Phase 2: コアシステム ✅

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 2.1 | Player / Enemy エンティティ | ✅ | レベル、経験値、正気度、飢餓度、装備、インベントリ |
| 2.2 | 敵AI（状態遷移） | ✅ | Idle/Patrol/Alert/Combat/Flee の5状態 |
| 2.3 | 戦闘システム（CombatSystem） | ✅ | 物理/魔法ダメージ計算、命中/回避/会心 |
| 2.4 | 属性システム（ElementSystem） | ✅ | 属性相性倍率 |
| 2.5 | 状態異常システム（StatusEffect） | ✅ | 毒/麻痺/睡眠等、スタック、ティック処理 |
| 2.6 | アイテムシステム（武器/防具/盾/消耗品） | ✅ | Equipment装備管理、ItemFactory生成 |
| 2.7 | マップ生成（BSP法） | ✅ | DungeonGenerator + FixDiagonalOnlyTiles後処理 |
| 2.8 | 視野計算（FOV） | ✅ | DungeonMap.ComputeFov |
| 2.9 | 魔法言語データ基盤 | ✅ | RuneWordDatabase、SpellParser |
| 2.10 | ゲーム定数管理（GameConstants） | ✅ | 飢餓間隔、救出回数、初期値等 |

---

## 4. Phase 3: GUI・操作 ✅

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 3.1 | WPFメインウィンドウ（MainWindow） | ✅ | Canvas描画、ステータスバー、メッセージログ |
| 3.2 | GameController（入力→ロジック→描画） | ✅ | 全GameAction処理、イベント駆動 |
| 3.3 | GameRenderer（マップ描画） | ✅ | タイル・エンティティ・FOV描画 |
| 3.4 | キーボード入力（WASD/矢印） | ✅ | 8方向移動（同時押し斜め移動対応） |
| 3.5 | インベントリウィンドウ | ✅ | モーダルダイアログ、装備/使用/数字キー選択 |
| 3.6 | コンソール版デモ（InteractiveGameDemo） | ✅ | デバッグ・テスト用 |

---

## 5. Phase 4: ゲームプレイ拡張 ✅

### 5.1 完了済みタスク

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 4.1 | ゲーム内時間システム（GameTime） | ✅ | 日本語暦（12ヶ月30日）、60ターン=1分 |
| 4.2 | 敵アクティブ範囲制限 | ✅ | チェビシェフ距離10タイル |
| 4.3 | ターン効果処理（ProcessTurnEffects） | ✅ | 飢餓減少、HP/SP自然回復、状態異常ティック |
| 4.4 | 食事システム（満腹度回復） | ✅ | Food.Use → ModifyHunger(+nutrition) |
| 4.5 | プレイヤーイベント通知 | ✅ | レベルアップ、飢餓段階、正気度段階 |
| 4.6 | 死亡・救出システム | ✅ | DeathCause追跡、救出回数制限（最大3回） |
| 4.7 | UI拡張（ステータスバー色分け） | ✅ | Lv/EXP/正気度/満腹度の色変え表示 |
| 4.8 | ミニマップ | ✅ | 右上オーバーレイ、スケール計算描画、Mキー切り替え |
| 4.9 | 敵ドロップ処理 | ✅ | 基本25%+階層×2%、ItemFactory.GenerateRandomItem |
| 4.10 | 自動探索 | ✅ | BFS経路探索、Tabキー、DispatcherTimer(80ms)、停止条件 |
| 4.11 | ステータス画面（StatusWindow） | ✅ | 基本9ステータス、戦闘パラメータ、装備、状態異常表示、Cキー |
| 4.12 | メッセージログ改善 | ✅ | ログ履歴閲覧ウィンドウ、フィルタリング、Lキー |
| 4.13 | セーブ・ロード機能 | ✅ | JSON形式、SaveData/SaveManager、F5/F9 |
| 4.14 | 階段上昇・ダンジョン帰還 | ✅ | 1階で地上帰還、2階以上は上階層移動、Shift+< |

### 5.2 完了済みタスク（追加分）

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 4.15 | キャラクター作成画面 | ✅ | 種族（10種）・職業（10種）・素性（10種）定義、Race/CharacterClass/Background列挙型、RaceDefinition/ClassDefinition/BackgroundDefinition（StatModifierボーナス/HP/MPボーナス/特性/経験値倍率/正気度減少倍率/初期スキル/初期ゴールド）、Player.Create(name, race, class, background)オーバーロード、SaveData対応 |
| 4.16 | ターン制限システム | ✅ | 31,536,000ターン（1年）制限、素性別背景（スタンピード/戦争/疫病等）、素性別フラグによる延長・撤廃、残りターン表示 |
| 4.17 | 死に戻りシステム完全版 | ✅ | HandlePlayerDeath拡張（TransferData生成→ExecuteRebirth）、知識引き継ぎ（魔法言語/スキル/宗教）、肉体リセット（Lv1/初期装備/初期ステータス）、正気度0知識喪失、廃人化救済（正気度20回復/知識喪失）、真のゲームオーバー（救済回数0）、TotalDeaths累計、セーブ/ロード対応 |
| 4.18 | ダンジョン階層拡張 | ✅ | 最大30階層、階段配置（StairsUp/Down）、階層間移動（TryDescend/AscendStairs）、1層帰還、階層別敵配置（6段階深度別）、3階毎ステータス補正、ボスフロア（5階毎）敵増加+通知、EnemyFactory階層補正オーバーロード |
| 4.19 | トラップシステム | ✅ | TrapType列挙（8種: 毒/落とし穴/テレポート/矢/警報/火炎/睡眠/混乱）、TrapDefinition（ダメージ/状態異常/階層補正/発見難易度/解除難易度）、PER依存発見・DEX依存解除判定、DungeonGenerator連携、GameControllerトリガー処理 |
| 4.20 | ドア・隠し通路 | ✅ | ドア開閉（TryMove内DoorClosed→DoorOpen変換、CloseDoorアクション）、施錠ドア（IsLocked/LockDifficulty、DEX+d20判定ピッキング）、隠し通路（SecretDoor配置：壁の両側通行可能箇所、Searchアクション：PER+d20>=15で発見）、DungeonGenerator階層依存施錠率・隠し通路数、Fキー探索/Xキードア閉じ |
| 4.21 | 投擲・射撃システム | ✅ | 遠距離攻撃（TryRangedAttack：装備武器Range内最近敵自動選択、HasLineOfSight射線判定）、投擲（TryThrowItem：WeaponType.Thrown消費+地面ドロップ、STR依存投擲距離）、FindNearestEnemyInRange/GetDistance（チェビシェフ距離）、Rキー射撃/Tキー投擲 |
| 4.22 | アイテム鑑定システム | ✅ | 装備品/巻物未鑑定生成、識別の巻物で自動鑑定（HandleIdentifyEffect）、装着時自動鑑定+呪い警告、呪い/祝福付与（階層依存確率）、GetDisplayName未識別表示 |
| 4.23 | インベントリ重量システム | ✅ | STRベース最大重量（50+STR×5）、拾得時重量チェック、超過時移動コスト1.5倍ペナルティ、重量UI表示（色分け）、Initialize/Load時のMaxWeight更新 |
| 4.24 | 通貨・経済基盤 | ✅ | ゴールド通貨システム、筒得・消費・表示、敵撃破時ドロップ、セーブ/ロード対応 |
| 4.25 | 行動コスト差分の適用 | ✅ | TurnCosts定数の実戦接続（移動1、攻撃3、魔法5-100等） |
| 4.26 | 難易度システム | ✅ | Easy/Normal/Hard/Nightmare/Ironmanの5段階、パラメータ変動、ターン制限倍率連動 |

---

## 6. Phase 5: コンテンツシステム基盤 ✅

> **方針**: ここではシステム・仕組みの実装のみ行う。
> テキストコンテンツ（NPC会話内容、クエスト詳細、ストーリー、フレーバーテキスト等）は Ver.α で実装する。

### 6.1 キャラクターシステム

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 5.1 | 種族システム | ✅ | 10種族の初期ステータス・種族特性・制約のデータ定義＋ゲーム接続。RacialTraitSystemをPlayer/CombatSystem/GameControllerに統合。種族特性（食糧免除・毒免疫・罠感知・浮遊・狂戦士・物理耐性・魔法ボーナス・マナ吸収等）が実際に機能。ExpMultiplier・SanityLossMultiplierも接続済。 |
| 5.2 | 職業（クラス）システム | ✅ | 10職業の装備適性・GrowthSystemによる種族/職業別成長曲線。Player.LevelUpがGrowthSystemを呼び出し、ステータス成長・HP/MP成長を実行。装備翔循チェック（ClassEquipmentSystem）をGameController・CombatSystemに統合。 |
| 5.3 | 素性システム | ✅ | BackgroundClearSystem作成。フラグ追跡（bool/int）・クリア条件判定・セーブ/ロード対応。GameControllerに統合（敵撃破・dungeon_clearフラグ）。テスト87件追加、全465テスト合格。 |
| 5.4 | スキルシステム | ✅ | SkillSystemをGameControllerに統合。UseSkillアクション・TryUseFirstReadySkill・ApplySkillEffect（Combat/Magic/Support/Exploration各カテゴリ）実装。クールダウンTickをProcessTurnEffectsに追加。セーブ/ロード対応（GetCooldownState/RestoreCooldownState）。SkillSystemTests 68件追加、全533テスト合格。 |
| 5.5 | 成長曲線・レベルアップ強化 | ✅ | GrowthSystemによる種族/職業別成長曲線はタスク5.1-5.2で実装済み（Player.LevelUp→GrowthSystem.CalculateLevelUpBonus、HP/MP成長にGrowthRate適用）。 |

### 6.2 魔法言語システム

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 5.6 | 魔法言語 詠唱UI | ✅ | SpellCastingSystemをGameControllerに統合。StartCasting/CastSpell/CancelCastingアクション追加。AddRuneWord/RemoveLastRuneWord/GetSpellPreviewメソッド実装。IsCastingプロパティ、OnCastingStarted/OnCastingEnded/OnSpellPreviewUpdatedイベント追加。 |
| 5.7 | 魔法言語 語彙入手システム | ✅ | VocabularyAcquisitionSystemをGameControllerに統合。LearnRuneWord（碑文）、LearnFromBook（古代の書）、LearnRandomRuneWord（ランダム）実装。理解度成長ロジックは既存のPlayer.ImproveWordMasteryで対応済み。IRandomProvider対応。 |
| 5.8 | 魔法言語 効果実体化 | ✅ | SpellEffectResolverをGameControllerに統合。ApplySpellEffectでDamage/Heal/Purify/Buff/Control/Detect/Unlock等16種の効果タイプを処理。属性マッピング、30語+の組み合わせ→ダメージ/回復/制御等の実際ゲーム効果生成。MagicLanguageSystemTests 79件追加、全612テスト合格。 |

### 6.3 宗教システム

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 5.9 | 宗教コンテンツ基盤 | ✅ | ReligionDatabase: 6宗教×6段階恩恵・禁忌・スキル、宗教間関係テーブル(敵対/友好/同盟/中立)、背教者の呪い定義(5宗教)、死に戻り効果(6宗教)。ReligionBenefitType 23種、ReligionTabooType 15種。FaithRank 6段階(Believer/Devout/Blessed/Priest/Champion/Saint)。宗教別階級名(GetRankTitle)。GameConstants 宗教定数11個。75テスト作成。 |
| 5.10 | 宗教施設・NPC基盤 | ✅ | ReligionSystem: JoinReligion(入信+スキル付与)、ConvertReligion(改宗+背教者の呪い+正気度ペナルティ)、LeaveReligion(脱退+呪い)。GameController統合: TryJoinReligion/TryLeaveReligion/TryPray/CheckTabooViolation/GetReligionStatus/OnDefeatHostileFollower。GameAction追加(Pray/JoinReligion/LeaveReligion)。Save/Load完全対応(宗教履歴・呪い状態含む)。 |
| 5.11 | 信仰度システム | ✅ | 信仰度0-100 (FaithCap制御、再入信時-20)。Pray(1日1回+2)。ViolateTaboo(禁忌別ペナルティ)。ProcessDailyTick(祈りリセット、信仰度自然減少10日毎-1、呪い経過)。CalculateDeathTransferFaith(通常80%、死神信仰Priest90%/Champion95%/Saint100%)。GetRebirthEffect(6宗教別効果)。戦闘統合(経験値ボーナス、属性禁忌チェック)。Player拡張(PreviousReligion/PreviousReligions/FaithCap/ApostasyCurse)。TransferData宗教履歴引き継ぎ。 |

### 6.4 マップ・世界システム

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 5.12 | マップ階層 国（全体マップ） | ✅ | WorldMapSystem: 6領地(Capital/Forest/Mountain/Coast/Southern/Frontier)、TerritoryDefinition(位置・隣接・レベル制限・説明)、TravelTo(隣接チェック+レベルチェック+ターン消費)、VisitedTerritories管理。LocationType enum(Town/Village/Facility/ReligiousSite/Field/Dungeon)。LocationDefinition record(40+ロケーション定義)。GetAdjacentTerritories。84テスト作成。 |
| 5.13 | マップ階層 領地（フィールドマップ） | ✅ | LocationDefinition.GetByTerritory/GetDungeonsByTerritory: 領地別ロケーション一覧取得。IsOnSurface状態管理(地上/ダンジョン切替)。SetTerritory(セーブ/ロード対応)。各領地に固有のロケーション配置(王都=城/ギルド/商店街/神殿、森林=森の都/エルフの集落等)。 |
| 5.14 | 地上拠点・街 | ✅ | TownSystem: RestAtInn(HP/MP/SP全回復+ターン消費)、RemoveCurseAtChurch(呪い解除100G)、IsFacilityAvailable(領地別施設チェック)。FacilityDefinition(12施設)。GameController統合: TryEnterTown/TryLeaveTown/TryUseInn/TryVisitChurch。銀行システム(DepositGold/WithdrawGold/BankBalance+SetBankBalance)。 |
| 5.15 | ショップシステム | ✅ | ShopSystem: InitializeShop(施設×領地×レベル別商品生成)、Buy(カリスマ値引+領地価格倍率)、Sell(売却40%基本価格)。GetTerritoryPriceMultiplier(Capital=1.0/Coast=0.9/Frontier=1.5等)。CalculateCharismaDiscount(上限20%)。レベル別商品追加(Lv5鉄の盾/Lv10鋼の剣/Lv15ミスリル短剣等)。領地別特産品(Mountain=ミスリル/Forest=薬草)。ClearShopInventory。 |
| 5.16 | ダンジョン特殊フロア | ✅ | SpecialFloorSystem: DetermineFloorType(IRandomProvider対応、ボス5F毎)、GetEnemySpawnMultiplier(Shop=0/RestPoint=0/TreasureVault=1.5/Arena=2.0)、GetSpecialFloorDescription。7種フロア(Normal/Shop/TreasureVault/BossRoom/RestPoint/Arena/Event)。GameController統合: DetermineSpecialFloorType/GetSpecialFloorDescription。 |
| 5.17 | 領地間移動イベント | ✅ | RandomEventSystem: RollTravelEvent(IRandomProvider対応、30%発生率)、10種イベント(BanditAttack/MonsterEncounter/Merchant/Treasure/Trap/Weather/Ruins/MysteriousItem/Traveler/RestPoint)。解決メソッド(ResolveByForce/ResolveByNegotiation/ResolveByEvasion+ResolveRestPoint/ResolveTrap/ResolveRuins/ResolveMysteriousItem)。SaveData拡張(CurrentTerritory/VisitedTerritories/IsOnSurface/BankBalance)。 |

### 6.5 NPC・クエストシステム基盤

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 5.18 | NPC基盤 | ✅ | NpcDefinition: 16NPC定義(6領地配置)、GetByTerritory/GetById/GetAll静的メソッド。NpcSystem: 好感度(0-100、5段階)、MeetNpc初対面、ModifyAffinity(Clamp)、周回引き継ぎ80%(CreateTransferData/ApplyTransferData)、GetAllStates/RestoreStates(Save/Load)。NpcStateSaveData型。GameController統合: _npcSystem/_dialogueSystem フィールド、TryTalkToNpc/GetNpcAffinity/GetNpcAffinityRank/GetNpcsInCurrentTerritory。66テスト作成。 |
| 5.19 | 会話システム | ✅ | DialogueNode record(Id/SpeakerName/Text/Choices/NextNodeId/ConditionFlag)。DialogueChoice record(Text/NextNodeId/AffinityChange/RequiredFlag)。DialogueSystem: RegisterNode/RegisterNodes、StartDialogue(条件フラグチェック)、SelectChoice(好感度変更+条件チェック)、Advance(線形進行)、EndDialogue。フラグ管理(SetFlag/HasFlag/GetAllFlags/RestoreFlags)。DialogueResult record。GameController統合: TryAdvanceDialogue/TrySelectDialogueChoice/EndDialogue/SetDialogueFlag、OnShowDialogue event。SaveData.DialogueFlags。 |
| 5.20 | クエストシステム基盤 | ✅ | QuestDefinition record(6種QuestType対応)。QuestObjective record(IsComplete判定)。QuestReward record(Gold/Exp/Items/GuildPoints/FaithPoints)。QuestSystem: RegisterQuest/RegisterQuests、AcceptQuest(レベル・ランク・重複チェック)、UpdateObjective(自動完了判定)、TurnInQuest(報酬付与)、GetActiveQuests/GetAvailableQuests/CompletedQuestIds。RestoreState/CreateActiveQuestsSaveData(Save/Load)。QuestDatabase: 11クエスト定義(Copper3/Iron3/Silver2/Gold2/Platinum1)、GetByRank/GetById。SaveData: ActiveQuests/CompletedQuests。GameController統合: TryAcceptQuest/TryTurnInQuest/UpdateQuestObjective/GetActiveQuests/GetAvailableQuests、OnQuestUpdated event。 |
| 5.21 | 冒険者ギルドシステム | ✅ | GuildSystem: Register(Copper開始)、AddPoints(自動ランクアップ判定)、GetPointsForNextRank。7段階ランク(Copper→Iron→Silver→Gold→Platinum→Mythril→Adamantine、閾値: 100/400/1000/2500/5000/10000)。GetRankName(日本語名)。IsRegistered property。RestoreState(Save/Load)。GameController統合: TryRegisterGuild/AddGuildPoints/GetGuildRank/GetGuildPoints/GetPointsForNextRank/IsGuildRegistered、OnGuildRankUp event。GameAction追加(TalkToNpc/AcceptQuest/TurnInQuest/RegisterGuild/ViewQuestLog/AdvanceDialogue)。ProcessInput 7新ケース。CreateSaveData/LoadSaveData完全対応(NPC/Quest/Guild/Dialogue)。 |

### 6.6 敵・アイテム拡張

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 5.24 | 敵バリエーション追加 | ✅ | 領土別敵15種＋ボス6種追加（計29種）、GetEnemiesForTerritory/GetBossForTerritory |
| 5.25 | 敵AI強化 | ✅ | RangedBehavior/BerserkerBehavior/SummonerBehavior追加、ボスにBerserker付与 |
| 5.26 | アイテムバリエーション | ✅ | 武器5/防具3/アクセ3/薬9/食料3/巻物7追加（計52種）|
| 5.27 | 装備強化・合成 | ✅ | CraftingSystem（レシピ10/強化+10/エンチャント）、CraftingInventory |
| 5.28 | ランダムイベント | ✅ | MonsterHouse/CursedRoom/BlessedRoom/HiddenShop/MaterialDeposit、領土別イベントプール |

### 6.7 バランス・チュートリアル

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 5.29 | バランス調整 | ✅ | BalanceConfig（階層/ランク別倍率、推奨レベル、ドロップ率、ゴールド/経験値/ショップ倍率）、DropTableSystem（14テーブル、GenerateLoot、GetScaledStats） |
| 5.30 | チュートリアル | ✅ | TutorialSystem拡張（18ステップ、18トリガー、OnTrigger自動完了、Priority付きステップ、Reset）、GameController統合 |

---

## 7. Phase 6: リリース準備基盤 ✅

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| 6.1 | タイトル画面 | ✅ | TitleWindow.xaml/.cs: ゲームタイトル「Abyss Reborn — 深淵より蘇りし者」、ニューゲーム/コンティニュー（セーブ存在時のみ有効）/設定/終了。キーボードナビゲーション（↑↓選択、Enter決定、Esc終了）。ダークファンタジー風デザイン。App.xaml.csでタイトル→メインウィンドウ起動制御。TitleAction列挙型。 |
| 6.2 | 設定画面（キー設定、音量等） | ✅ | SettingsWindow.xaml/.cs: マスター/BGM/SE音量スライダー（0-100%）、フォントサイズ（10-24）。キーバインド一覧表示（参照用）。保存/キャンセル/初期値リセット。GameSettings（Core）: JSON永続化（LocalApplicationData/RougelikeGame/settings.json）、Validate/Clone/EffectiveBgmVolume/EffectiveSeVolume。 |
| 6.3 | BGM/SE実装 | ✅ | IAudioManager/AudioManager（WPF MediaPlayer）/SilentAudioManager。BGMループ再生、SE複数同時再生（最大8）、同一SE連続再生抑制（50ms間隔）。BgmIds（10種）/SeIds（16種）定数定義。音量制御（Master×BGM/SE乗算）。場面別BGM切り替え（タイトル/探索/戦闘/ボス/町/ショップ/ゲームオーバー）。リソースフォルダ構成（Resources/BGM/, Resources/SE/）。 |
| 6.4 | パフォーマンス最適化 | ✅ | ObjectPool\<T\>汎用クラス（ConcurrentBag、初期確保/最大サイズ/リセットアクション）。GameRenderer描画最適化: Rectangle/TextBlockプーリング導入（毎フレームのCanvas.Children.Clear()+再生成を廃止、Visibility切り替えによる再利用）。描画範囲計算は既存実装で大規模マップ対応済み。 |

---

## 8. 操作キー一覧

### 8.1 実装済み

| キー | 操作 |
|------|------|
| W/A/S/D / 矢印キー | 移動（同時押しで斜め） |
| Space | 待機 |
| G | アイテム拾う |
| I | インベントリ |
| C | ステータス画面 |
| Tab | 自動探索 |
| M | ミニマップ表示/非表示 |
| Shift+. | 階段を降りる |
| Shift+, | 階段を上る |
| L | メッセージログ履歴 |
| F5 | セーブ |
| F9 | ロード |
| Esc | ダイアログ閉じる |
| Q | ゲーム終了 |

### 8.2 本バージョンで実装予定

| キー | 操作 | 対象タスク |
|------|------|-----------|
| N | キャラクター作成 | 4.15 |
| T | 投擲 | 4.21 |
| R | 射撃 | 4.21 |
| F | ドア操作（開閉） | 4.20 |
| S+数字 | スキル使用 | 5.4 |
| / | 鑑定 | 4.22 |
| E | 詠唱（魔法言語入力） | 5.6 |
| P | 祈り（宗教アクション） | 5.9 |

---

## 9. 解決済みバグ

| ID | 概要 | 状態 |
|----|------|------|
| BUG-001 | Food.Use()が満腹度を減少させていた（方向逆転） | ✅ 修正済み |
| BUG-002 | StatusEffect名前空間エラー（Entities→Core） | ✅ 修正済み |
| BUG-003 | コンソール出力の日本語文字化け | ✅ UTF-8設定で対応 |

---

## 10. ドキュメント整備

### 10.1 ドキュメントブラッシュアップ（第1回）

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| D.1 | 00_ドキュメント概要 新規作成 | ✅ | 全18ドキュメントの索引・関連性 |
| D.2 | 01_企画書 マップサイズ修正 | ✅ | 200×200→80×50 |
| D.3 | 02_設計書 テンプレ残り修正 | ✅ | 実装準拠に更新 |
| D.4 | 06_戦闘設計書 HP/MP/SP計算式修正 | ✅ | 設計値→実装値 |
| D.5 | 11_クラス設計書 Damage定義修正 | ✅ | 名前空間修正 |
| D.6 | 13_GUIシステム設計書 新規作成 | ✅ | 描画エンジン、色設計、入力処理 |
| D.7 | 14_マップシステム設計書 新規作成 | ✅ | BSP生成、タイル、FOV、自動探索 |
| D.8 | 15_BGM・SE設計書 新規作成 | ✅ | 音響方針、BGM/SE一覧 |
| D.9 | 16_演出設計書 新規作成 | ✅ | 実装済み演出、将来計画 |
| D.10 | 17_デバッグ・テスト設計書 新規作成 | ✅ | テスト戦略、デバッグ手法 |
| D.11 | 18_設定資料 新規作成 | ✅ | 世界観、歴史、暦、ストーリー |
| D.12 | 全既存ドキュメントに実装状況注記 | ✅ | 04〜12全設計書に追加 |

### 10.2 ドキュメントブラッシュアップ（第2回）

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| D.13 | 01_企画書 HTMLテンプレコメント除去 | ✅ | 10箇所 |
| D.14 | 01_企画書 世界観・ストーリー充実 | ✅ | ヴァルディア王国追加 |
| D.15 | 05_キャラクター設計書 種族・職業数確認 | ✅ | 10種族・10職業 |
| D.16 | 06_戦闘システム設計書 行動コスト表修正 | ✅ | 2カラム形式 |
| D.17 | 00_ドキュメント概要 用語集拡充 | ✅ | 8→18項目 |
| D.18 | 10_プロジェクト構造設計書 ディレクトリ修正 | ✅ | サブディレクトリ追加 |
| D.19 | 02_設計書 StatusWindow表示値修正 | ✅ | 実装値に修正 |
| D.20 | 13_GUIシステム設計書 色設計照合 | ✅ | 実装と一致 |

### 10.3 フォルダ再構成

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| D.21 | 企画設計書フォルダ作成・移動 | ✅ | 17ドキュメント移動 |
| D.22 | 計画書フォルダ作成 | ✅ | Ver.prt/Ver.α/Ver.β/Ver.0.1~0.9/Ver.1~1.9 |
| D.23 | 00_ドキュメント概要 パス更新 | ✅ | 全パス更新 |

### 10.4 ドキュメントブラッシュアップ（第3回）

> Phase 4 全タスク完了（4.15-4.26）に伴い、実装状況の記述を最新化。

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| D.24 | 00_ドキュメント概要 Ver.prt.0.1ステータス更新 | ✅ | 🔄→Phase 4 ✅、用語集に6語追加（CharacterCreation/TransferData/RaceDefinition/ClassDefinition/BackgroundDefinition/ExecuteRebirth） |
| D.25 | 02_設計書 セーブデータ・クラス図・クラス一覧更新 | ✅ | Race/CharacterClass/Background/TotalDeaths/TransferData追加、CharacterCreation追加、キャラクター作成画面を部分実装表記 |
| D.26 | 05_キャラクター設計書 実装状況全面更新 | ✅ | 種族/職業/素性定義✅、Player統合✅、SaveData✅、GUI画面⬜ |
| D.27 | 06_戦闘システム設計書 実装状況更新 | ✅ | 難易度✅、通貨✅、行動コスト✅、クラス別成長⚠️ |
| D.28 | 07_死に戻り設計書 TransferData実装状況更新 | ✅ | TransferData⬜→✅、注記にHandlePlayerDeath/ExecuteRebirth詳細追加 |
| D.29 | 09_宗教システム設計書 引き継ぎ状況更新 | ✅ | 死に戻り引き継ぎ⬜→✅ |
| D.30 | 10_プロジェクト構造設計書 構造・テスト数更新 | ✅ | Systems/追加、テスト数378件、SaveManager.cs追加 |
| D.31 | 11_クラス設計書 実装状況更新 | ✅ | Player Race/Class/Background✅、TransferData✅、CharacterCreation新規追加 |
| D.32 | 12_ターン消費システム設計書 行動コスト更新 | ✅ | 状況別移動倍率⚠️→✅ |
| D.33 | 17_デバッグ・テスト設計書 テスト数・ファイル更新 | ✅ | 224→378件、CharacterCreationTests.cs追加 |
| D.34 | 実装計画書Ver.prt.md Phase 4ステータス更新 | ✅ | Phase 4 🔄→✅ |
| D.35 | 実装計画書Ver.prt.0.1.md ブラッシュアップ記録追加 | ✅ | 本セクション |

### 10.5 ドキュメントブラッシュアップ（第4回）

> Phase 5 全タスク完了（5.1-5.30）に伴い、実装状況の記述を最新化。
> テスト総数: 1081件（Core 934 + Gui 147）

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| D.36 | 実装計画書Ver.prt.0.1.md Phase 5ステータス更新 | ✅ | Phase 5 ⬜→✅、全体ステータスにPhase 5完了表記 |
| D.37 | 11_クラス設計書 新規クラス追加 | ✅ | BalanceConfig/DropTableSystem/CraftingSystem/CraftingInventory/TutorialSystem拡張/新AI行動3種 |
| D.38 | 10_プロジェクト構造設計書 構造・テスト数更新 | ✅ | 新規ファイル追加、テスト数897→1081件 |
| D.39 | 00_ドキュメント概要 ステータス更新 | ✅ | Phase 5完了、テスト数更新 |
| D.40 | 14_マップシステム設計書 領地別敵・イベント更新 | ✅ | テリトリー敵プール、DropTableSystem、ランダムイベント拡張 |
| D.41 | 06_戦闘システム設計書 AI・バランス更新 | ✅ | 新AI行動(Ranged/Berserker/Summoner)、BalanceConfig統合 |
| D.42 | 05_キャラクター設計書 クラフト・チュートリアル更新 | ✅ | CraftingSystem、TutorialSystem参照追加 |

### 10.6 ドキュメントブラッシュアップ（第5回）

> Phase 6 全タスク完了（6.1-6.4）に伴い、実装状況の記述を最新化。
> テスト総数: 1102件（Core 955 + Gui 147）

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| D.43 | 実装計画書Ver.prt.0.1.md Phase 6ステータス更新 | ✅ | Phase 6 ⬜→✅、全体ステータス✅完了 |
| D.44 | 11_クラス設計書 Phase 6追加クラス | ✅ | TitleWindow/SettingsWindow/GameSettings/IAudioManager/AudioManager/SilentAudioManager/ObjectPool |
| D.45 | 10_プロジェクト構造設計書 構造・テスト数更新 | ✅ | Audio/ディレクトリ追加、テスト数1081→1102件 |
| D.46 | 00_ドキュメント概要 ステータス更新 | ✅ | 実装計画書Ver.prt.0.1 ✅完了、用語集追加 |
| D.47 | 13_GUIシステム設計書 タイトル・設定・Audio更新 | ✅ | TitleWindow/SettingsWindow/AudioManager追加、将来計画→実装済みに更新 |
| D.48 | 15_BGM・SE設計書 実装状況更新 | ✅ | 未実装→AudioManager実装済み、技術選定確定、ファイル構成更新、AudioIds一覧追加 |

### 10.7 ドキュメントブラッシュアップ（第6回）

> デバッグマップアリーナ拡張（15x10→32x24）、GUIオートテスト37件＋システム検証テスト37件追加に伴う全ドキュメント最新化。
> テスト総数: 1,176件（Core 955 + Gui 221）

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| D.49 | 17_デバッグ・テスト設計書 全面更新 | ✅ | テスト数378→1,176件、Core.Tests 19ファイル詳細、Gui.Tests 5ファイル詳細、GuiAutomation/SystemVerification詳細セクション追加、デバッグアリーナ仕様追加、BUG-004/005追加 |
| D.50 | 19_GUIオートテスト説明書 デバッグマップ仕様更新 | ✅ | 15x10→32x24、敵1→5体、アイテム1→13個、デバッグタイル4個、特殊地形追加 |
| D.51 | 14_マップシステム設計書 タイル表更新 | ✅ | 表示文字7箇所修正（Lava≋/Pit^/PillarO/Altar_/Fountain{/Chest□/Grass"）、デバッグタイル4種追加、IsLocked/LockDifficultyプロパティ追加 |
| D.52 | 10_プロジェクト構造設計書 テスト数更新 | ✅ | 1102→1,176件、Gui.Tests 147→221件（内訳追記） |
| D.53 | 00_ドキュメント概要 #19追加 | ✅ | ドキュメント一覧/フォルダ構成/関連図/品質運用系セクションに19_GUIオートテスト説明書追加 |
| D.54 | 04_領地設計書 実装状況更新 | ✅ | 「全て設計段階」→Phase 5実装済み項目を反映（WorldMap/Location/Enemy/NPC/Shop等） |
| D.55 | 09_宗教システム設計書 実装状況更新 | ✅ | Phase 5 ReligionSystem実装反映（6宗教定義/信仰段階/恩恵禁忌） |
| D.56 | 18_設定資料 実装状況更新 | ✅ | NpcQuestSystem/WorldMapSystem実装済み反映 |
| D.57 | 13_GUIシステム設計書 タイル色テーブル更新 | ✅ | Altar/Fountain/Chest/デバッグタイル4種の色定義追加 |

### 10.8 Phase 5 GUI反映実装

> Phase 5で実装済みのバックエンドシステムのうち、GUIに未反映だった全システムをWPFダイアログとして実装。
> デバッグルームの敵AI初期値をOFFに変更。テスト総数: 1,139件（Core 955 + Gui 184）

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| G.1 | TownWindow 新規作成 | ✅ | 街施設メニュー（宿屋/教会/銀行/鍛冶屋/ギルド/神殿/図書館/闘技場/各種ショップ）、銀行預入・引出、ダンジョン入場、ショップ連携 |
| G.2 | TravelEventWindow 新規作成 | ✅ | 移動イベントダイアログ（強行/交渉/回避の3選択肢、イベントタイプ別ボタンラベル・結果表示） |
| G.3 | CharacterCreationWindow 新規作成 | ✅ | キャラクター作成（種族10/職業10/素性10選択、名前入力、ステータスプレビュー計算） |
| G.4 | DifficultySelectWindow 新規作成 | ✅ | 難易度選択（Easy/Normal/Hard/Nightmare/Ironman、詳細説明付き） |
| G.5 | WorldMapWindow 拡張 | ✅ | 3列レイアウト化（隣接領地/ロケーション・ダンジョン/施設）、地上/ダンジョン状態表示、街入場ボタン、LocationDefinitionからのデータ取得、全文日本語化 |
| G.6 | MainWindow.xaml.cs 拡張 | ✅ | OnTerritoryChanged購読、Bキー街入場、WorldMap結果のTownWindow処理、UpdateDisplayに領地名・地上/ダンジョン表示追加 |
| G.7 | MainWindow.xaml 拡張 | ✅ | ステータスバーに領地名(TerritoryText)・地上/ダンジョン状態(SurfaceStatusText)追加、操作説明テキスト全キーバインド反映 |
| G.8 | TitleWindow キャラ作成フロー | ✅ | NewGame → DifficultySelectWindow → CharacterCreationWindow → パラメータ保持してMainWindow起動 |
| G.9 | App.xaml.cs パラメータ受け渡し | ✅ | TitleWindowからのキャラ作成・難易度パラメータをMainWindowコンストラクタに中継 |
| G.10 | GameController.Initialize オーバーロード | ✅ | Initialize(name, race, class, background, difficulty) 追加、Player.Create(name, race, class, background)呼出 |
| G.11 | デバッグルーム敵AI初期OFF | ✅ | _debugAIActive = false に変更 |
| G.12 | ゴールドテスト修正 | ✅ | Adventurer素性の初期ゴールド100Gに合わせてテスト期待値を更新 |
