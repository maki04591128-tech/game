# 実装計画書 Ver.prt.0.24 — ゲームプレイ違和感・不整合の洗い出しと修正

**ステータス**: 🔍 洗い出し中（修正はユーザー判断後に実施）
**目的**: ゲーム内で各システムの要素に触れる際に違和感がある不整合を網羅的に調査し、修正対象を確定する
**前提**: 全90システム統合済み（Ver.prt.0.23完了時点）、Core.Tests 5,539件全合格

---

## 調査方法

以下のカテゴリ別にソースコード全体を監査し、名称・説明・実際の挙動の不一致を洗い出した。

---

## カテゴリA: 商人・ショップシステム（9件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| A-1 | `NpcType.Alchemist`（薬師）がEnum定義済みだが、どのNPCにも割り当てられていない | 高 | `Enums.cs` | |
| A-2 | `NpcType.MagicShopkeeper`（魔法商人）がEnum定義済みだが、どのNPCにも割り当てられていない | 高 | `Enums.cs` | |
| A-3 | `NpcType.BlackMarketDealer`（闇商人）がEnum定義済みだが、どのNPCにも割り当てられていない | 高 | `Enums.cs` | |
| A-4 | 「薬草師リーナ」のNpcTypeが汎用`Shopkeeper`。説明は「森の薬草に詳しい若い女性」であり`Alchemist`型が適切 | 高 | `NpcSystem.cs:41` | |
| A-5 | 「行商人ハッサン」の品揃えが説明と不一致。「珍しい品物を扱う」と記載されるが実際は他の雑貨店と同じ汎用品（価格30%増のみ） | 中 | `NpcSystem.cs:58`, `WorldMapSystem.cs` | |
| A-6 | マナポーションが雑貨店と魔法店の両方で販売されており差別化なし | 中 | `WorldMapSystem.cs:548,625` | |
| A-7 | 識別の巻物が雑貨店と魔法店の両方で販売。魔法店専売であるべき | 中 | `WorldMapSystem.cs:553,627` | |
| A-8 | 山岳武器店のアイテムID=`weapon_greatsword`（大剣）だが、表示名は「ミスリルダガー」（短剣）。価格800Gは大剣価格帯 | 高 | `WorldMapSystem.cs:589` | |
| A-9 | NPC型（NpcType）と施設型（FacilityType）の間にマッピングが存在しない。商人NPCがどの店を運営するか紐付けなし | 設計課題 | `NpcSystem.cs`, `WorldMapSystem.cs` | |

---

## カテゴリB: アイテム・消耗品システム（13件）

### B-a: 巻物の未実装（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| B-1 | `ScrollType.Freeze`（凍結の巻物）— ファクトリで生成可能だが`Scroll.Use()`にハンドラなし。「何も起こらなかった」と表示 | 致命的 | `Consumables.cs:274-318`, `ItemFactory.cs:750-762` | |
| B-2 | `ScrollType.Sanctuary`（聖域の巻物）— 同上。「周囲に結界を張り、敵を退ける」と記載されるが実装なし | 致命的 | `Consumables.cs:274-318`, `ItemFactory.cs:801-812` | |
| B-3 | `ScrollType.Summon`（召喚の巻物）— Enum定義のみ。ファクトリメソッドもハンドラもなし | 高 | `Consumables.cs:350` | |
| B-4 | `ScrollType.Return`（帰還の巻物）— Enum定義のみ。ハンドラなし | 高 | `Consumables.cs` | |

### B-b: ポーションの未実装（8件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| B-5 | `PotionType.StrengthBoost`（筋力増強薬）— ファクトリで生成可能だが`Potion.Use()`にハンドラなし | 致命的 | `ItemFactory.cs:511-535`, `Consumables.cs:47-101` | |
| B-6 | `PotionType.AgilityBoost`（敏捷増強薬）— 同上 | 致命的 | `ItemFactory.cs:511-535`, `Consumables.cs:47-101` | |
| B-7 | `PotionType.IntelligenceBoost`（知力増強薬）— Enum定義のみ。ファクトリもハンドラもなし | 高 | `Consumables.cs:109-147` | |
| B-8 | `PotionType.Invisibility`（透明化薬）— ファクトリで生成可能だがUse()ハンドラなし | 致命的 | `Consumables.cs:47-101` | |
| B-9 | `PotionType.FireResistance`（耐火薬）— ファクトリで生成可能だがUse()ハンドラなし | 致命的 | `Consumables.cs:47-101` | |
| B-10 | `PotionType.ColdResistance`（耐寒薬）— ファクトリで生成可能だがUse()ハンドラなし | 致命的 | `Consumables.cs:47-101` | |
| B-11 | `PotionType.Poison`（毒薬）— Enum定義のみ。ファクトリもハンドラもなし | 高 | `Consumables.cs:109-147` | |
| B-12 | `PotionType.Confusion`（混乱薬）— Enum定義のみ。ファクトリもハンドラもなし | 高 | `Consumables.cs:109-147` | |

### B-c: 装備・スキル関連（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| B-13 | 「鼓舞の歌」スキルのターゲットが`SkillTarget.Self`（自分のみ）。名称「歌」は範囲効果を暗示するが自己バフのみ | 中 | `SkillSystem.cs:141` | |

---

## カテゴリC: 装備カテゴリ・ステータス効果（6件）

### C-a: 装備カテゴリの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| C-1 | `WeaponType.Spear`（槍）の`EquipmentCategory`が`Sword`にマッピング。EquipmentCategoryにSpearが存在しない | 中 | `Equipment.cs:164-175` | |
| C-2 | `WeaponType.Whip`（鞭）の`EquipmentCategory`が`Sword`にマッピング。EquipmentCategoryにWhipが存在しない | 中 | `Equipment.cs:164-175` | |

### C-b: ステータス効果の未実装（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| C-3 | `StatusEffect.Charm`（魅了）— Enum定義のみ。適用ロジック・効果処理なし | 高 | `Enums.cs:232` | |
| C-4 | `StatusEffect.Madness`（狂気）— 同上 | 高 | `Enums.cs:233` | |
| C-5 | `StatusEffect.Petrification`（石化）— 同上 | 高 | `Enums.cs:234` | |
| C-6 | `StatusEffect.InstantDeath`（即死）— 同上 | 高 | `Enums.cs:235` | |

---

## カテゴリD: 敵・ダンジョンシステム（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| D-1 | ダンジョンボスのテーマ不一致 — 王都カタコンベ（アンデッドテーマ）のボスがキングスライム、海岸沈没船のボスがスケルトンロード等 | 中 | `EnemyFactory.cs:790-829` | |
| D-2 | 人型敵のテリトリー不整合 — オーク（Humanoid）が山岳、ダークエルフ（Humanoid）が南方に配置。テリトリーテーマと不一致 | 中 | `EnemyFactory.cs:296-339` | |
| D-3 | スケルトンが王都と海岸で重複。海岸は水棲テーマだが陸上のスケルトンが配置 | 低 | `EnemyFactory.cs:856-859` | |
| D-4 | 森の精霊（Common）のステータスがElite相当（MND=15、平均9.3）だが経験値25は最低値 | 中 | `EnemyFactory.cs:405-417` | |
| D-5 | 巨大蟹（Common）のRES=18がElite級。防御力がトレント（Elite）を超えるが経験値25のまま | 中 | `EnemyFactory.cs:494-506` | |

---

## カテゴリE: NPC対話・テキストの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| E-1 | 宿屋主人が「料理する」選択肢を提供。宿屋の役割（休息・宿泊）と矛盾。料理人NPCまたは厨房施設が適切 | 中 | `GameController.cs:5169` | |
| E-2 | 訓練師が「転職する」選択肢を提供。転職はギルド受付の機能であり重複。訓練師の口上にも転職の言及なし | 中 | `GameController.cs:5179` | |
| E-3 | 一般商人が「裏の商品を見る」「密輸を依頼する」を直接提供。正規商人と闇商人の役割が混在 | 中 | `GameController.cs:5149-5150` | |

---

## カテゴリF: 宗教システムの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| F-1 | 「無神論」（Atheism）が信仰ポイント制の恩恵体系を持つ。無神論は信仰を否定する思想であり、信仰ポイントで強化される仕組みは矛盾 | 中 | `ReligionSystem.cs:328-345` | |
| F-2 | 全宗教の祈り効果が同一（信仰度+2のみ）。各神殿の特色が祈り効果に反映されていない | 中 | `ReligionSystem.cs:550-566` | |
| F-3 | 「混沌神カオス」の名称が「Chaos God Chaos」の直訳で個性不足。恩恵も「ランダムバフ」のみで具体性に欠ける | 低 | `ReligionSystem.cs:307` | |

---

## カテゴリG: テリトリー・ワールドマップの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| G-1 | 海岸テリトリーの説明に「漁村が点在する」と記載されるが、FacilityTypeに釣り・漁業関連施設が存在しない | 中 | `WorldMapSystem.cs:156-159` | |
| G-2 | 森林テリトリーに「薬師の村」サブロケーションと薬草師NPCが存在するが、薬草・錬金術関連施設がない | 中 | `WorldMapSystem.cs:52,149` | |
| G-3 | 南方テリトリーの説明が「荒野と古代遺跡」だが、実際のサブロケーションに「雪原の村」「狩人の集落」が含まれ、テーマが不一致 | 中 | `WorldMapSystem.cs:83-91,161` | |

---

## カテゴリH: ペットシステムの不整合（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| H-1 | キャット（Cat）ペットの能力が「幸運（ドロップ率UP）」。猫と幸運・ドロップ率の関連性が薄く、「夜間視力」「身軽さ」等が適切 | 低 | `PetSystem.cs:46` | |

---

## カテゴリI: ArmorType不整合（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| I-1 | 鉄の兜・鉄のブーツに`ArmorType.Plate`（板金鎧）が設定。兜・靴にはLeatherやChainmailが適切 | 低 | `ItemFactory.cs:343-394` | |

---

## カテゴリJ: 経験値・レベルカーブの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| J-1 | 経験値カーブが指数関数`100×1.5^(Lv-1)`で急勾配すぎる。Lv25で168万exp必要、Lv30で1278万exp必要。最強ボスexp=2000なので Lv25到達にボス841体撃破が必要。**実質的な最大レベルがLv18-20** | 致命的 | `Player.cs:389-390`, `GameConstants.cs` | |
| J-2 | 推奨レベル関数`GetRecommendedLevel()`がフロア20でLv35、フロア30でLv45を返すが、J-1により到達不可能。フロア15以降の推奨レベルが全て非現実的 | 高 | `GameConstants.cs:208-216` | |
| J-3 | 敵の経験値が固定値でフロア深度によるスケーリングなし。ゴブリンはフロア1でもフロア30でも常に15exp。深層の敵を倒すインセンティブが低い | 中 | `EnemyFactory.cs:252-783` | |

---

## カテゴリK: 戦闘バランスの不整合（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| K-1 | 魔法防御の係数が0.3で物理防御の0.5より大幅に低い。魔法攻撃INT×3に対し魔法防御MND×2×0.3=MND×0.6で、攻撃対防御の比率が5.2:1（物理は2.9:1）。**魔法が極端に強すぎる** | 高 | `DamageCalculator.cs:73-82` | |
| K-2 | 魔法攻撃はクリティカルヒットが不可能（ハードコードでfalse）。物理は1.5倍クリティカルが存在。物理と魔法の期待ダメージ差がさらに広がる場面がある | 中 | `CombatSystem.cs:269` | |
| K-3 | DEXのクリティカル率への寄与（0.3%/pt）がLUK（0.5%/pt）の60%しかない。攻撃系ステータスのDEXが幸運系のLUKより劣るのは不自然 | 中 | `DamageCalculator.cs:168-182` | |
| K-4 | 物理防御の係数0.5により、VIT投資の効率がSTR投資の約1/3。防御特化ビルドが成立しにくい | 中 | `DamageCalculator.cs:26-35` | |
| K-5 | ボスHP倍率がハードコード（3.0x～6.0x）で計算式ベースでない。深度スケーリング式(GetDepthStatMultiplier)との整合性なし | 中 | `GameConstants.cs:219-228` | |

---

## カテゴリL: ショップ経済の不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| L-1 | 買値倍率1.5、売値倍率0.3で**売買往復損失が80%**。100Gの武器を150Gで購入→30Gで売却。装備更新のたびにゴールドが大量消滅し、経済が回らない | 高 | `GameConstants.cs:330-338` | |
| L-2 | ポーション回復量が固定値（小30HP/中75HP/大150HP）でレベルスケーリングなし。Lv20（HP170）時点で小ポーションは17.6%回復、Lv30（HP300）では10%回復。**高レベルでは実質的に回復アイテムが機能しない** | 高 | `ItemFactory.cs:440-497` | |

---

## カテゴリM: セーブデータの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| M-1 | `Thirst`（渇き）・`Fatigue`（疲労）・`Hygiene`（衛生）がSaveDataに含まれていない。ロード時にこれらの値が初期値にリセットされる | 致命的 | `SaveData.cs`, `MultiSlotSaveSystem.cs` | |
| M-2 | コンパニオン・ペットデータがSaveDataに含まれていない。セーブ→ロードで仲間全員が消失する | 致命的 | `SaveData.cs`, `CompanionSystem.cs` | |
| M-3 | `DaysSinceLastPrayer`・`HasPrayedToday`がSaveDataに含まれていない。祈りの日次スケジュールがロード時にリセットされる | 中 | `SaveData.cs`, `Player.cs:302-303` | |

---

## カテゴリN: インベントリ・装備システムの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| N-1 | `Inventory.Add()`メソッドが重量チェックを行わない。`MaxWeight`プロパティは存在するが、Add時に`TotalWeight + item.Weight <= MaxWeight`の検証がない。重量上限を超えてアイテムを追加可能 | 致命的 | `Inventory.cs:35-56` | |
| N-2 | 両手武器装備時にオフハンド装備がnullに設定されるが、**インベントリへの返却処理が未実装**（コメントで「要実装」と記載あり）。オフハンド装備品が消失するバグ | 致命的 | `Equipment.cs:348-357` | |
| N-3 | 盾を装備した状態で両手武器を装備→盾消失は防がれるが、逆方向（両手武器装備中に盾を装備）の制約チェックがない | 中 | `Equipment.cs:348-357` | |

---

## カテゴリO: スキル・転職システムの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| O-1 | スキルポイントの付与・消費・追跡システムが存在しない。`SkillSystem`にはスキル使用とクールダウンの管理はあるが、スキル習得のためのポイント経済がない。レベルアップ時にスキルポイント付与もなし | 高 | `SkillSystem.cs`, `Player.cs:366-387` | |
| O-2 | `MultiClassSystem.CanClassChange()`は転職可否の判定のみで、転職実行時のステータス再計算メソッドが存在しない。転職してもステータスが変化しない | 高 | `MultiClassSystem.cs:27-32` | |

---

## カテゴリP: マップ表示の不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| P-1 | `TileType.NpcTrainer`のDisplayCharケースが`GetDisplayChar()`に存在しない。訓練師タイルが`?`で表示される | 高 | `Tile.cs:476-525` | |
| P-2 | `TileType.NpcLibrarian`のDisplayCharケースが`GetDisplayChar()`に存在しない。図書館司書タイルが`?`で表示される | 高 | `Tile.cs:476-525` | |

---

## カテゴリQ: タイルプロパティのドキュメント不整合（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| Q-1 | `TileType.SymbolMountain`のEnum定義コメントが「山岳（通行不可）」だが、実装では`BlocksMovement=false`（通行可能、移動コスト2.0）。`SymbolWater`も同様に「水域（通行不可）」と記載されるが実際は通行可能（移動コスト1.8） | 中 | `Tile.cs:130-142`, `SymbolMapSystem.cs:136-141` | |

---

## カテゴリR: 建設・投資システムの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| R-1 | `BaseConstructionSystem.GetDailyFoodProduction()`が定義されているが、GameControllerから一度も呼び出されていない。畑を建設しても食料が自動生産されない | 高 | `BaseConstructionSystem.cs:93`, `GameController.cs` | |
| R-2 | `InvestmentSystem`で投資が記録されるが、投資の完了・配当支払いを処理するコードがGameControllerに存在しない。投資したゴールドが回収不能 | 高 | `InvestmentSystem.cs`, `GameController.cs` | |

---

## カテゴリS: ステータスシステムの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| S-1 | ThirstStageとThirstLevelの2つの列挙型で`SevereDehydration`のダメージ値が異なる（ThirstStage=2、ThirstLevel=3）。レガシーコードとの不整合 | 中 | `ThirstSystem.cs:80-94` | |
| S-2 | 宗教の信仰上限ペナルティ（再加入時-20）が累積しない。何度脱退・再加入しても1回分の-20しか適用されず、繰り返しペナルティ回避が可能 | 中 | `Player.cs:320-322`, `GameConstants.cs` | |

---

## カテゴリT: Enum未使用値（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| T-1 | `ExtendedItemCategory`列挙型（8値: Material/SoulGem/TrapKit/RepairTool/CookedFood/Book/Key/Instrument）が**全値未使用**。定義のみで参照箇所なし | 高 | `Enums.cs:1478-1496` | |
| T-2 | `SkillTarget.SingleAlly`/`AllAllies`が定義済みだが使用なし。味方対象スキルが存在しない | 中 | `Enums.cs:442-450` | |
| T-3 | `TradeRouteStatus.Closed`/`Blocked`が定義済みだが使用なし。交易路の閉鎖・遮断状態が機能しない | 中 | `Enums.cs:1605-1615` | |

---

## カテゴリU: 未使用食材タイプ（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| U-1 | `FoodType`の5値（RawMeat/RawFish/Fruit/Vegetable/Rotten）が定義済みだが対応する食品アイテムが生成されない。料理素材として入手不能 | 高 | `Consumables.cs:223-251` | |

---

## カテゴリV: 魔法・呪文システムの不整合（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| V-1 | ルーン語"loka"（閉じる）が`CategorizeEffect()`のswitchに存在しない。デフォルトでDamage型に分類されるため、ドア閉鎖呪文が攻撃呪文として処理される | 致命的 | `SpellCastingSystem.cs:421-441`, `RuneWordDatabase.cs:92` | |
| V-2 | `DetermineElement()`にPoison元素のマッピングがない。毒属性呪文が`Element.None`として処理され、耐性計算が無効化される | 致命的 | `SpellCastingSystem.cs:388-419` | |
| V-3 | `StatusEffect.Invisibility`/`Slow`/`Vulnerability`/`Apostasy`の4種にCreate系ファクトリメソッドが存在しない。これらの状態異常を生成できない | 高 | `StatusEffectSystem.cs`, `Enums.cs:214-239` | |
| V-4 | Dark→Curse元素の相性が0.0（無効化）だが、逆方向Curse→Darkは1.0（中立）。一方向の完全耐性は不自然 | 中 | `ElementSystem.cs:143-164` | |
| V-5 | "springa"（爆発）の難易度が2でMP=10だが、同MPの"hylja"（隠す）は難易度3。高火力呪文が先に習得可能で学習曲線が崩壊 | 中 | `RuneWordDatabase.cs:55-92` | |
| V-6 | バフ"berserk"のAttackMultiplier=0.5fだが説明は「攻撃力+50%」。0.5は-50%を意味し、説明と効果が逆転している可能性 | 高 | `ExtendedStatusEffectSystem.cs:41-42` | |

---

## カテゴリW: クラフト・料理システムの不整合（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| W-1 | `material_coal`（石炭）がItemFactoryに存在せず入手不能。鋼鉄の剣レシピが作成不可 | 致命的 | `CraftingSystem.cs:275`, `ItemFactory.cs` | |
| W-2 | `material_leather`（革）がItemFactoryに存在せず入手不能。革鎧レシピが作成不可 | 致命的 | `CraftingSystem.cs:301`, `ItemFactory.cs` | |
| W-3 | `material_raw_meat`（生肉）がItemFactoryに存在せず入手不能。調理肉・非常食レシピ＋料理の焼き肉・干し肉が全て作成不可 | 致命的 | `CraftingSystem.cs:336,344`, `CookingSystem.cs:21,24` | |
| W-4 | `material_magical_essence`（魔法のエッセンス）がItemFactoryに存在せず入手不能。超回復薬レシピが作成不可 | 致命的 | `CraftingSystem.cs:328` | |
| W-5 | `material_fish`（魚）がItemFactoryに存在せず入手不能。蒸し魚の料理レシピが作成不可 | 致命的 | `CookingSystem.cs:23` | |
| W-6 | `material_salt`（塩）がItemFactoryに存在せず入手不能。干し肉の料理レシピが作成不可 | 致命的 | `CookingSystem.cs:24` | |

---

## カテゴリX: ポーション効果値の未定義（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| X-1 | `potion_invisibility`（透明化薬）のEffectValue=0。持続ターン数は設定されているが効果量が未定義 | 高 | `ItemFactory.cs:537-547` | |
| X-2 | `potion_cure_all`（万能薬）のEffectValue未定義。回復量のバランス基準がない | 高 | `ItemFactory.cs:573-582` | |
| X-3 | `potion_fire_resist`（耐火薬）の耐性パーセンテージ未定義。持続ターン50のみ設定 | 高 | `ItemFactory.cs:549-559` | |
| X-4 | `potion_cold_resist`（耐冷薬）の耐性パーセンテージ未定義。持続ターン50のみ設定 | 高 | `ItemFactory.cs:561-571` | |

---

## カテゴリY: 種族特性の未実装（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| Y-1 | スライム種族の「分裂」能力（HP50%以下で味方スライム召喚）が定義済みだが、戦闘/ダメージシステムにチェックロジックなし。特性が完全に不活性 | 致命的 | `RacialTraitSystem.cs:96-98`, `CharacterCreation.cs:73-77` | |
| Y-2 | デーモン種族の「魔力吸収」能力（魔法被弾時MP5%回復）が定義済みだが、ダメージ計算にチェックなし | 致命的 | `RacialTraitSystem.cs:87`, `CharacterCreation.cs:61-65` | |
| Y-3 | 堕天使種族の「浮遊」能力（穴落下罠無効・水上移動可能）が定義済みだが、`IsLevitating()`が罠/移動システムから呼び出されない | 致命的 | `RacialTraitSystem.cs:91,143-144` | |
| Y-4 | スライム種族の「装備制限」（武器・防具の大部分が装備不可）が定義済みだが、`HasEquipmentRestriction()`が装備ロジックから呼び出されない。全装備可能 | 致命的 | `RacialTraitSystem.cs:98,147-148` | |
| Y-5 | 堕天使種族の「光闇両属性」特性が定義済みだが、スキルシステムに属性制限がなく全種族が全属性使用可能。特性の差別化がない | 中 | `RacialTraitSystem.cs:92` | |
| Y-6 | 種族の隠しペナルティ（オーク:INT-3/CHA-2、アンデッド:正気度消耗1.3倍、堕天使:LUK-4）が説明文に記載なし。プレイヤーが把握できない | 中 | `CharacterCreation.cs:6-82` | |

---

## カテゴリZ: 背景・コンパニオンシステムの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| Z-1 | 全10職歴（冒険者/兵士/学者等）の初期装備品が`BackgroundBonusData`に定義されているが、`Player.Create()`で一度も適用されない。職歴固有の初期装備が受け取れない | 致命的 | `RacialTraitSystem.cs:218-262`, `Player.cs:625-674` | |
| Z-2 | `CompanionSystem`（仲間システム）が完全実装済み（追加/除去/AI行動/ターン処理）だが、GameControllerから`AddCompanion()`を呼ぶコードパスがない。仲間の募集が不可能 | 高 | `CompanionSystem.cs`, `GameController.cs` | |

---

## カテゴリAA: トラップ・GameControllerの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AA-1 | `TriggerTrap()`のswitch文に`TrapType.Poison`/`Arrow`/`Fire`/`Sleep`/`Confusion`の5種のケースが欠落。`TrapDefinition.cs`で定義されているが踏んでも何も起こらない | 致命的 | `GameController.cs:3340-3376`, `TrapDefinition.cs:112-150` | |
| AA-2 | GameController内に14箇所以上のマジックナンバー（ドロップ率25/2、訓練費用50/100/150、投資上限100-500等）が定数化されずハードコードされている | 中 | `GameController.cs:1289,1561,1597,5364,5380,5396,6077-6078` | |

---

## カテゴリAB: セーブデータ追加欠落（9件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AB-1 | `PetSystem`のペット状態（ID/名前/種類/レベル/経験値/空腹度/忠誠度/HP/騎乗状態）がセーブされない。ロード時にペット全消失 | 致命的 | `GameController.cs:6694-6801`, `PetSystem.cs` | |
| AB-2 | `CombatStance`（戦闘構え）がセーブされない。ロード時にBalancedにリセット。攻撃/防御修正が失われる | 高 | `GameController.cs:87,6694-6801` | |
| AB-3 | キャラクターの全`StatusEffects`（活性バフ/デバフとその残りターン数）がセーブされない。ロード時に全状態異常が消滅 | 致命的 | `Character.cs:79`, `GameController.cs:6694-6801` | |
| AB-4 | `ProficiencySystem`の全12カテゴリ（武器/採掘/釣り/鍛冶/料理等）の習熟レベル・経験値がセーブされない | 致命的 | `GameController.cs:65,6694-6801`, `ProficiencySystem.cs` | |
| AB-5 | `DiseaseSystem`の現在の病気種類と残りターン数（`_playerDisease`/`_diseaseRemainingTurns`）がセーブされない | 致命的 | `GameController.cs:90,2256-2293,6694-6801` | |
| AB-6 | `BodyConditionSystem`の傷の状態（切り傷/打撲/穿刺傷/骨折/火傷とその回復ターン）がセーブされない | 高 | `BodyConditionSystem.cs:8-24`, `GameController.cs:6694-6801` | |
| AB-7 | `Player.HasPrayedToday`フラグがセーブされない。ロード後に再度祈りが可能（1日1回制限が無効化） | 中 | `Player.cs:303`, `GameController.cs:6694-6801` | |
| AB-8 | `WeaponProficiency`（武器熟練度）の個別武器種ごとのレベル/経験値がセーブされない | 高 | `WeaponProficiencySystem.cs`, `GameController.cs:6694-6801` | |
| AB-9 | `Player.RestoreFromSave()`がThirst/Fatigue/Hygieneのパラメータを受け付けるが、LoadSaveData()から値が渡されず常にデフォルト100（最大値）でリセットされる | 致命的 | `Player.cs:698-724`, `GameController.cs:6810-6822` | |

---

## カテゴリAC: クエストシステムの不整合（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AC-1 | クエスト`quest_rat_hunt`の討伐対象`"enemy_rat"`がEnemyFactoryに存在しない。クエスト完了不可能 | 致命的 | `NpcSystem.cs:785-788`, `EnemyFactory.cs` | |
| AC-2 | クエスト`quest_bandit_clear`の討伐対象`"enemy_bandit"`がEnemyFactoryに存在しない。クエスト完了不可能 | 致命的 | `NpcSystem.cs:801-804`, `EnemyFactory.cs` | |
| AC-3 | クエスト`quest_herb_collect`の収集対象IDが`"item_herb"`だが、実際のアイテムIDは`"material_herb"`。ID不一致でクエスト完了判定が動作しない | 致命的 | `NpcSystem.cs:790-793`, `ItemFactory.cs` | |
| AC-4 | クエスト目標のNPC参照が名前ベース（`"deliver_ore_marco"`/`"talk_elwen"`）でNPC IDベース（`"npc_xxx"`）と不一致。完了判定の整合性なし | 高 | `NpcSystem.cs:808,819` | |
| AC-5 | クエスト目標のフロア参照（`"floor_forest_3"`/`"floor_coast_5"`/`"boss_mine_golem"`/`"floor_ruins_10"`）が検証不能。対応するフロア構造の実在が不明 | 中 | `NpcSystem.cs:797,813,825,841` | |

---

## カテゴリAD: 実績・エンディングシステムの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AD-1 | `AchievementSystem`に実績が**1件も登録されていない**。`Register()`が一度も呼ばれず、インフラだけが存在。Unlock()は動作するが実績一覧が空 | 致命的 | `AchievementSystem.cs:42-46`, `GameController.cs:68` | |
| AD-2 | `DetermineEnding()`の`allTerritoriesVisited`パラメータが**常にfalse**でハードコード。放浪者エンディング（Wanderer）が達成不可能 | 致命的 | `GameController.cs:7792-7795`, `MultiEndingSystem.cs:54` | |
| AD-3 | 実績ボーナス効果（`"stat_boost_small"=1`/`"gold_bonus"=50`/`"exp_multiplier"=10`等）が定義済みだが、実績未登録のためCalculateNextPlayBonus()が常に0を返す到達不能コード | 中 | `AchievementSystem.cs:86-95` | |

---

## カテゴリAE: マップ生成・タイルの不整合（7件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AE-1 | `TileType.DeepWater`/`Lava`/`Pit`/`Tree`の4種がプロパティ定義済みだがダンジョン生成で一切配置されない。遭遇不可能な地形 | 高 | `DungeonGenerator.cs`, `Tile.cs` | |
| AE-2 | `TileType.Altar`が祠部屋に配置されるが、GameControllerにインタラクションハンドラなし。踏んでも何も起こらない | 致命的 | `RoomCorridorGenerator.cs:487`, `GameController.cs:1406-1430` | |
| AE-3 | `TileType.Fountain`が通常部屋に50%確率で配置されるが、インタラクションハンドラなし。踏んでも何も起こらない | 致命的 | `RoomCorridorGenerator.cs:197`, `GameController.cs` | |
| AE-4 | `RoomType.Library`/`Prison`/`Storage`の3種が定義済みだが生成ロジックなし。ダンジョンに出現しない | 高 | `Tile.cs:531-577`, `DungeonGenerator.cs` | |
| AE-5 | `FeaturePlacer.cs`（秘密部屋生成）がコードベース全体で一度もインスタンス化されない。`RoomType.Secret`の部屋が完全に無効化 | 高 | `FeaturePlacer.cs`, 全ソース | |
| AE-6 | `GameRenderer.TileColors`辞書に21種のTileType（DeepWater/Lava/Pit/Pillar/RuneInscription/SecretDoor/Grass/Tree/全SymbolMap10種/NpcTrainer/NpcLibrarian）のカラーマッピングがない | 高 | `GameRenderer.cs:35-65` | |
| AE-7 | `EnvironmentalCombatSystem`のSurfaceType（Water/Fire/Poison等）とTileType（Lava/Water/DeepWater）が未接続。溶岩タイルを踏んでも火炎ダメージが適用されない | 高 | `EnvironmentalCombatSystem.cs`, `GameController.cs` | |

---

## カテゴリAF: ランダムイベントの不整合（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AF-1 | `RandomEventType.NpcEncounter`（NPC遭遇）がイベントプールに登録済みだがResolveメソッドが存在しない。イベント発生時にハンドラなし | 致命的 | `WorldMapSystem.cs:729,749` | |
| AF-2 | `RandomEventType.MerchantEncounter`（商人遭遇）が同様にResolveメソッドなし | 致命的 | `WorldMapSystem.cs:730,761` | |
| AF-3 | `RandomEventType.AmbushEvent`（待ち伏せ）が同様にResolveメソッドなし。イベントタイプからハンドラへのディスパッチswitch文自体が存在しない | 致命的 | `WorldMapSystem.cs:733,771` | |

---

## カテゴリAG: ギャンブルシステムの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AG-1 | `GamblingSystem.GetLuckBonus()`が定義済み（LUK1あたり0.5%、上限5%）だが**一度も呼び出されない**。高LUCKキャラでもギャンブル勝率に影響なし | 高 | `GamblingSystem.cs:40-44` | |
| AG-2 | カードゲーム（ハイロー）の引き分けが「負け」扱い。他のゲーム（サイコロ/丁半）は50%勝率だがカードは約48%で期待値0.912（8.8%のハウスエッジ）。他ゲームの5%と比較して不公平 | 中 | `GamblingSystem.cs:24-29` | |

---

## カテゴリAH: Engine層のEnum/計算不整合（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AH-1 | `ResourceSystem.GetClassHpBonus()`/`GetClassMpBonus()`がCore層と異なるクラス名（Warrior/Berserker/Summoner）を使用。Fighter/Knight/Thief/Ranger/Bard/Alchemistの6クラスがデフォルト値にフォールスルーし不正なHP/MP成長率 | 致命的 | `ResourceSystem.cs:30-102` | |
| AH-2 | `CalculateExpGain()`で`param.PlayerLevel`のゼロチェックがない。PlayerLevel=0時に`DivideByZeroException`でクラッシュ | 致命的 | `ResourceSystem.cs:309` | |
| AH-3 | `SpellCastingSystem`の三項演算子が常にMagicalを返す: `element == Element.None ? DamageType.Magical : DamageType.Magical`。純粋/物理ダメージ呪文が作成不可 | 高 | `SpellCastingSystem.cs:369` | |
| AH-4 | `Character.GetResistanceAgainst()`が常に0fを返す。種族の属性耐性（火耐性/毒耐性等）がダメージ計算に反映されない | 高 | `Character.cs:105` | |
| AH-5 | `Character.ConsumeMp()`に入力検証なし。負の値を渡すとMPが増加する（`ConsumeMp(-50)`でMP+50）。セキュリティリスク | 高 | `Character.cs:119` | |

---

## カテゴリAI: ダメージ計算の不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AI-1 | `DamageType.Pure`と`DamageType.Healing`がswitch文のdefaultケース（防御力0）にフォールスルー。明示的な処理がなく挙動が曖昧 | 中 | `Character.cs:85-90` | |
| AI-2 | 経験値計算で`Math.Pow(1.5, 98)`がint.MaxValueを超える可能性。レベル99以上の経験値テーブルがオーバーフロー | 中 | `ResourceSystem.cs:280-287` | |

---

## カテゴリAJ: デッドコードシステム（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AJ-1 | `MerchantGuildSystem`が完全なデッドコード。初期化（GC:59）とReset（GC:3153）のみで、JoinGuild()/EstablishRoute()/ExecuteTrade()が一度も呼ばれない。交易システムが完全に無機能 | 高 | `MerchantGuildSystem.cs`, `GameController.cs:59,3153` | |
| AJ-2 | `TerritoryInfluenceSystem`が完全なデッドコード。初期化（GC:62）とReset（GC:3147）のみで、Initialize()/ModifyInfluence()/GetDominantFaction()が一度も呼ばれない | 高 | `TerritoryInfluenceSystem.cs`, `GameController.cs:62,3147` | |
| AJ-3 | `ModLoaderSystem`が完全なデッドコード。初期化（GC:72）のみでLoadMod()/Validate()が呼ばれない。ファイルI/Oも未実装でモッド読み込みは機能しない | 中 | `ModLoaderSystem.cs`, `GameController.cs:72` | |

---

## カテゴリAK: カルマ・無限ダンジョンの永続性欠落（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AK-1 | `KarmaSystem`のカルマ値・履歴がセーブされない。カルマはショップ価格（GC:5482-5485）・NPC態度・闇市場アクセスに影響するが、セーブ/ロードで全消失 | 致命的 | `GameController.cs:6694-6800(CreateSaveData)`, `KarmaSystem.cs` | |
| AK-2 | `_infiniteDungeonMode`フラグと`_infiniteDungeonKills`がセーブされない。無限ダンジョン進行中にセーブ→ロードするとモード解除され通常ダンジョンに戻る | 高 | `GameController.cs:164,167,6694-6800` | |
| AK-3 | `BaseConstructionSystem`の建設済み施設データがセーブされない。建設した7種の施設（キャンプ/鍛冶場/農場等）がロード時に全消失 | 高 | `BaseConstructionSystem.cs`, `GameController.cs:48,6694-6800` | |

---

## カテゴリAL: 天候効果の未適用（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AL-1 | `WeatherSystem.GetElementDamageModifier()`/`GetRangedHitModifier()`が戦闘ダメージ計算から呼ばれない。雨天の雷+20%/火-20%、嵐の遠距離-30%等が全て無効 | 致命的 | `WeatherSystem.cs:102,105`, `GameController.cs:1874-1885` | |
| AL-2 | `WeatherSystem.GetMovementCostModifier()`が移動処理から呼ばれない。吹雪の移動コスト+50%、雨の+10%等が全て無効 | 高 | `WeatherSystem.cs:105`, `GameController.cs` | |
| AL-3 | `WeatherSystem.GetSightModifier()`が視界計算から呼ばれない。霧の視界50%低下、嵐の40%低下が無効。視界は常にComputeFov(12)の固定値 | 高 | `WeatherSystem.cs:90`, `GameController.cs:2224,2718` | |

---

## カテゴリAM: 釣り・採掘アイテムID不一致（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AM-1 | `FishingSystem`の全9種の魚ID（fish_common_1/fish_common_2/fish_medium_1等）がItemFactoryに未定義。釣り成功時にItemFactory.Create()が失敗しアイテム生成不可 | 致命的 | `FishingSystem.cs:22-42`, `ItemFactory.cs` | |
| AM-2 | `GatheringSystem`の鉱石ID（ore_iron/ore_silver/ore_gold/ore_mithril/gem_rough）がItemFactoryに未定義。採掘成功時にアイテム生成不可。唯一存在する`material_iron_ore`とも異なるID | 致命的 | `GatheringSystem.cs:28-31`, `ItemFactory.cs` | |
| AM-3 | `FishingSystem`の魚IDとGatheringSystemの魚IDが不一致。FishingSystemは`fish_common_1`、GatheringSystemは`fish_common`を使用 | 高 | `FishingSystem.cs:22-42`, `GatheringSystem.cs:38-41` | |
| AM-4 | マップ上に釣りスポットが配置されない。DungeonFeatureGeneratorにWaterTileChanceが存在するが、水タイルへのインタラクション（釣り開始）がGameControllerに未実装 | 高 | `DungeonFeatureGenerator.cs`, `GameController.cs` | |

---

## カテゴリAN: ルーン学習・エンチャント永続性（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AN-1 | プレイヤーにKnownRunes/LearnedRunesフィールドが存在しない。RuneWordDatabase（54+ルーン語定義済み）をプレイヤーが学習する手段がない | 致命的 | `Player.cs`, `RuneWordDatabase.cs:176-183` | |
| AN-2 | ルーン組み合わせシステム（SpellParser）がGameControllerに未接続。ルーンを組み合わせて呪文を作成するUIもメソッドもない | 高 | `SpellParser.cs`, `GameController.cs` | |
| AN-3 | エンチャントデータがアイテムに永続化されない。Item.csにAppliedEnchantmentsフィールドがなく、セーブ/ロードでエンチャント情報が全消失 | 致命的 | `EnchantmentSystem.cs`, `Item.cs`, `SaveData.cs` | |
| AN-4 | ソウルジェムがItemFactoryに定義されていない。EnchantmentSystemがソウルジェムを必要とするが、アイテムとして入手不可能 | 高 | `EnchantmentSystem.cs:98-106`, `ItemFactory.cs` | |

---

## カテゴリAO: ボスフロア・クエスト報酬の不整合（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AO-1 | ボスフロアで階段が制限されない。TryDescendStairs()にIsBossFloorチェックがなく、プレイヤーがボス戦をスキップして次の階に進める | 致命的 | `GameController.cs:2635-2683` | |
| AO-2 | クエスト報酬のItemIdsが処理されない。QuestRewardにItemIds配列が定義済みだが、TurnInQuest()でGold/Experience/GuildPointsのみ処理されItemIdsは無視される | 致命的 | `NpcSystem.cs:434-456`, `GameController.cs:6333-6346` | |
| AO-3 | クエスト報酬アイテム付与時にインベントリ容量チェックがない。報酬アイテムがインベントリ満杯時に消失する可能性 | 高 | `NpcSystem.cs:434-456` | |
| AO-4 | `HasPrayedToday`の日次リセットが実装されていない。ProcessTurnEffects()に日替わりリセットコードがなく、祈りの1日1回制限が保持されない（AB-7と関連するが根本原因が異なる） | 高 | `Player.cs:303,317`, `GameController.cs:2125+` | |

---

## カテゴリAP: 建設システムの未接続（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AP-1 | `BaseConstructionSystem.Build()`メソッドが一度も呼ばれない。7種の施設（Camp/Workbench/Smithy/Storage/Farm/Barricade/Barracks）が建設不可能。GetRestHpRecoveryMultiplier()とGetCraftingSuccessBonus()のみ使用 | 致命的 | `BaseConstructionSystem.cs`, `GameController.cs:48,7287,7293` | |
| AP-2 | `AmbientSoundSystem.GetCurrentAmbientSound()`（GC:7968）が一度も呼ばれない。9種のアンビエント音（Dungeon/Forest/Mountain等）が定義済みだが再生システムなし。`_currentAmbientSound`（GC:640）は設定されるが消費されない | 中 | `AmbientSoundSystem.cs`, `GameController.cs:640,2668,7968` | |

---

## カテゴリAQ: 環境パズル・ターンカウンタの不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AQ-1 | `EnvironmentalPuzzleSystem`の名前に反しパズルがダンジョン床に配置されない。NPC対話イベントとしてのみ動作し、探索型の環境パズルは存在しない | 中 | `EnvironmentalPuzzleSystem.cs`, `GameController.cs:7718-7734` | |
| AQ-2 | `TurnCount`がint型で宣言。int.MaxValue（2,147,483,647）到達後にオーバーフローし、TurnCount%600==0等の全周期処理が破綻。GameTimeのモジュロ演算も負値で不正動作 | 中 | `GameController.cs:191`, `GameTime.cs:38` | |

---

## カテゴリAR: スキル・パッシブ・状態異常の不整合（8件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AR-1 | `Charm`状態異常が行動制限に含まれない。Stun/Freeze/Sleep/Petrificationは行動阻止するがCharmは阻止対象外で、魅了された敵/プレイヤーが通常通り攻撃可能 | 高 | `GameController.cs:935-943`, `StatusEffectSystem.cs:198-204` | |
| AR-2 | `Madness`状態異常のランダム行動ロジック未適用。`StatusEffectSystem.GetMadnessAction()`が定義済みだがGameControllerから一度も呼ばれず、狂気状態でも通常行動可能 | 高 | `StatusEffectSystem.cs:401-411`, `GameController.cs` | |
| AR-3 | `InstantDeath`状態異常が確実に即死しない。DamagePerTick=int.MaxValue, Duration=1で設計されるが、TickStatusEffectsの呼び出しタイミングに依存し即死が保証されない | 中 | `StatusEffectSystem.cs:235-240`, `GameController.cs:1973-1979` | |
| AR-4 | パッシブスキル（weapon_mastery「武器ダメージ+20%」、hp_boost「最大HP+10%」等）が定義済みだがBasePower値が実際のステータス/ダメージ計算に反映されない。学習しても効果ゼロ | 致命的 | `SkillSystem.cs:95,156-157`, `GameController.cs` | |
| AR-5 | `Character.TickStatusEffects()`がGameControllerのターンループから呼ばれない。バフ/デバフの持続時間がデクリメントされず、一度適用された効果が永続化する | 致命的 | `GameController.cs`, `Character.cs:161-187` | |
| AR-6 | 相反するバフ/デバフの共存チェックなし。Strength（攻撃力1.25倍）とWeakness（全ステ0.80倍）が同時に適用可能。相互排他ルールが未実装 | 中 | `Character.cs:124-146` | |
| AR-7 | スキル前提条件のID検証なし。SkillTreeNodeのPrerequisites配列にタイプミスのスキルIDが含まれていても検出されず、そのスキルが学習不能になるがエラー表示なし | 中 | `SkillSystem.cs:29-42,163-252` | |
| AR-8 | 誓約（Oath）の違反チェック`IsViolation()`が一度も呼ばれない。use_alcohol/attack_enemy/use_torch等の禁止行動を実行しても誓約違反が発生しない | 致命的 | `OathSystem.cs:62-71`, `GameController.cs` | |

---

## カテゴリAS: セーブデータ詳細欠落（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AS-1 | `Item.Grade`（アイテム品質）がItemSaveDataに未保存。ロード時に全アイテムがItemGrade.Standardにリセットされ、レア/レジェンダリー等の品質情報が消失 | 致命的 | `SaveData.cs:214-236`, `GameController.cs:6980-6989` | |
| AS-2 | 地面に置かれたアイテム（GroundItems）が一切セーブされない。CreateSaveDataにGroundItemsの保存処理なし。ロード時にマップが再生成され全アイテム消失 | 致命的 | `GameController.cs:189,6694-6801` | |
| AS-3 | マップ探索状態（Tile.IsExplored）がセーブされない。ロード時にマップ再生成で全タイルが未探索に戻り、霧の戦争（Fog of War）がリセットされる | 高 | `SaveData.cs`, `Tile.cs:262`, `GameController.cs:6886-6887` | |
| AS-4 | セーブデータのバージョン移行ロジックが存在しない。SaveData.Versionフィールド（=1）が定義済みだがLoadSaveDataでチェックされず、フォーマット変更時に旧セーブが破損するリスク | 中 | `SaveData.cs:11`, `GameController.cs:6806-6978` | |
| AS-5 | SkillTreeBonusProviderコールバックがセーブされない。ロード後にスキルツリーボーナスが未再計算のまま失われ、UIが再初期化するまでステータスが不正 | 中 | `Player.cs:251`, `SaveData.cs` | |

---

## カテゴリAT: 休息・回復メカニクスの不整合（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AT-1 | 休息時にMP回復が適用されない。TryCamp()でRestSystem.GetRecoveryRates()からmpRecoveryを取得するが、Player.CurrentMpに加算するコードがない | 高 | `GameController.cs:7354,7368-7369` | |
| AT-2 | 休息時にSanity回復が適用されない。RestSystemがsanityRecovery（0.05-0.2）を定義するが、Player.ModifySanity()がTryCamp()内で呼ばれない | 高 | `GameController.cs:7354,7376` | |
| AT-3 | 休息中に食料/水が消費されない。TryCamp()が35-50ターン分の時間を経過させるが、対応する空腹/渇き減少がない。休息にサバイバルコストなし | 中 | `GameController.cs:7382-7383` | |
| AT-4 | キャンプ可能判定のフロア深度ロジックが不明瞭。`!isIndoor && floorDepth == 0`の条件が地上=depth未定義と矛盾し、キャンプ許可条件が予測不能 | 低 | `RestSystem.cs:29-35` | |

---

## カテゴリAU: 移動コスト・地形効果の未適用（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AU-1 | 地形別移動コスト（Water=2.0/Forest=1.5/Mountain=2.0等）がTile.MovementCostに定義済みだが、移動計算で参照されない。全地形が一律1ターンコスト | 高 | `Tile.cs:272,384,429,435`, `GameController.cs:1200-1209` | |
| AU-2 | 斜め移動コスト計算がint切り捨てで正規コストと同一になる。`(int)(1 * 14/10) = (int)(1.4) = 1`となり、斜めも直進も1ターンで斜め移動が常に有利 | 中 | `GameController.cs:1202`, `GameConstants.cs:22-24` | |
| AU-3 | 重量超過ペナルティ（1.5倍移動コスト）がint切り捨てで無効化。`(int)(1 * 1.5) = (int)(1.5) = 1`となり、過重状態でも移動速度に変化なし | 中 | `GameController.cs:1200-1209` | |

---

## カテゴリAV: 時間帯・渇き・疲労・衛生効果の未適用（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AV-1 | 時間帯の視界修正がFOV計算に適用されない。TimeOfDaySystem.GetSightRangeModifier()（夜間0.4倍等）を計算するがComputeFov()は常にDefaultViewRadius(8)を使用 | 高 | `GameController.cs:2223-2228,3347,4542` | |
| AV-2 | 時間帯が敵スポーン率に影響しない。SpawnEnemies()が`4 + CurrentFloor * 2`の固定式を使用し、夜間の増加/昼間の減少が未実装 | 高 | `GameController.cs:726-814,1856-1860` | |
| AV-3 | ThirstSystem.GetThirstModifiers()（STR/AGI/INT修正0.2-1.0倍）が一度も呼ばれない。脱水状態でもステータスペナルティなし | 高 | `ThirstSystem.cs` | |
| AV-4 | 疲労修正値がメッセージ表示のみで実効果なし。fatigueMod取得後に攻撃力/移動速度への反映コードがなく、疲労はフレーバーテキスト | 高 | `GameController.cs:2237-2240` | |
| AV-5 | BodyConditionSystem.GetHygieneInfectionRisk()（衛生状態による感染リスク0.5-4.0倍）が一度も呼ばれない。不衛生でも病気リスク増加なし | 中 | `BodyConditionSystem.cs:54-74` | |
| AV-6 | 料理素材（生肉/塩/魚）がItemFactoryに未定義でCookingSystemの全レシピが作成不可能（W-1〜W-5と別カテゴリだが料理固有の問題） | 高 | `CookingSystem.cs`, `ItemFactory.cs:1159-1250` | |

---

## カテゴリAW: 誓約・信仰・評判の未機能（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AW-1 | 誓約ボーナス（GetTotalExpBonus/GetTotalDropBonus）が一度も呼ばれない。誓約を守る報酬として経験値/ドロップ率ボーナスが定義されるが適用されない | 致命的 | `OathSystem.cs:50-60`, `GameController.cs` | |
| AW-2 | 誓約破棄にペナルティなし。BreakOath()が「ペナルティあり」とコメントされるが、実装は`_activeOaths.Remove(type)`のみで信仰/正気度/ステータスへの影響ゼロ | 高 | `OathSystem.cs:45-48` | |
| AW-3 | 信仰システムに献金/供物メカニクスが存在しない。Pray()のみが信仰ポイント獲得手段で、ゴールド/アイテムを捧げる経済的手段が未実装 | 中 | `ReligionSystem.cs:574-592` | |
| AW-4 | 奇跡/神の祝福を発動する手段がない。信仰ランクに基づくパッシブ恩恵は存在するが、RequestMiracle()等のアクティブな神の介入メカニクスが未定義 | 中 | `ReligionSystem.cs` | |
| AW-5 | ReputationSystem.GetQuestAvailability()が一度も呼ばれない。評判ランクに基づくクエスト解放率（0.0-1.0）が定義済みだが、クエスト掲示板でのフィルタリングに使用されない | 高 | `ReputationSystem.cs:65-76` | |

---

## カテゴリAX: コンパニオンシステムの未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AX-1 | コンパニオンのDamageCompanion()が戦闘中に一度も呼ばれない。敵はコンパニオンを攻撃対象にせず、コンパニオンは被ダメージゼロで不死身状態 | 致命的 | `GameController.cs:1800-1810`, `CompanionSystem.cs` | |
| AX-2 | コンパニオンのステータスが雇用時に固定（Attack=5+level×3等）で成長メカニクスが存在しない。レベルアップ/経験値蓄積なし | 致命的 | `CompanionSystem.cs:1-196`, `GameController.cs:6245-6256` | |
| AX-3 | CompanionSystem.SetAIMode()がGameControllerから一度も呼ばれない。AIモードが雇用時のAggressive固定で変更不可 | 高 | `CompanionSystem.cs:57-64`, `GameController.cs:6188-6280` | |
| AX-4 | RemoveDeadCompanions()の戻り値（死亡コンパニオンリスト）が未処理。死亡通知/追悼イベント/クエスト連動なしでサイレント除去 | 中 | `CompanionSystem.cs:177-183`, `GameController.cs:1796` | |

---

## カテゴリAY: ペットシステムの未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AY-1 | PetSystem.Feed()/Train()/TickHunger()が一度も呼ばれない。ペットの餌やり/訓練/空腹値減少が未接続で、空腹/忠誠度が初期値から変化しない | 致命的 | `PetSystem.cs:64-120`, `GameController.cs` | |
| AY-2 | ペット特殊能力（Wolf威嚇/Horse騎乗/Hawk偵察/Cat幸運/Bear防壁/Dragonブレス）がテキスト定義のみで実行コードなし。6種全ての能力が機能しない | 致命的 | `PetSystem.cs:43-48` | |
| AY-3 | PetSystem.GetMoveSpeedMultiplier()（Horse2.0/Bear1.3/Dragon2.5倍）がプレイヤー移動処理から呼ばれない。騎乗速度ボーナスがデータのみ | 高 | `PetSystem.cs:97-109`, `GameController.cs` | |
| AY-4 | PetSystem.GetObedienceRate()（忠誠度に基づく命令成功率）が一度も呼ばれない。ペットコマンドが忠誠度に関係なく100%成功 | 高 | `PetSystem.cs:132-137` | |

---

## カテゴリAZ: 敵AI・行動パターンの未機能（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| AZ-1 | ボス敵にBerserkerBehaviorが割り当てられるが、バーサーク状態（HP30%以下でダメージ増加）の実効果なし。ボスの特殊行動パターンが非機能 | 中 | `EnemyFactory.cs:114-120` | |
| AZ-2 | SummonerBehavior（敵が仲間を召喚）が定義済みだがEnemyFactoryで一度も割り当てられない。召喚者タイプの敵が存在しない完全なデッドコード | 高 | `BasicBehaviors.cs:438-495`, `EnemyFactory.cs:65-128` | |
| AZ-3 | 敵の逃走ロジック（ShouldFlee() HP20%以下で撤退）が定義済みだが、戦闘メッセージに「敵が逃げた」が表示されず逃走行動が確認不能 | 中 | `Enemy.cs:169-174`, `GameController.cs:1796-1810` | |

---

## カテゴリBA: 対話・百科事典・シンボルマップイベントの未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BA-1 | 16体のNPCの対話ID（dlg_leon_intro等）がDialogueSystemに未登録。StartDialogue()がnullを返しNPC固有の対話ツリーが機能しない。汎用対話にフォールバック | 高 | `NpcSystem.cs:26-66`, `GameController.cs:5649` | |
| BA-2 | SymbolMapEventSystemの10種イベント（商人キャラバン/盗賊襲撃/放浪治療師等）がGameControllerから一度もトリガーされない。メタデータ取得のみ | 中 | `SymbolMapEventSystem.cs:18-40`, `GameController.cs:7890-7893` | |
| BA-3 | TutorialTrigger.ReachFloor10が定義済みだが発火コードなし。10階到達チュートリアルが永久に表示されない | 中 | `SmithingSystem.cs:224`, `GameController.cs` | |
| BA-4 | EncyclopediaSystemのモンスター以外のカテゴリ（アイテム/場所/伝承）にUIアクセスポイントがない。百科事典エントリが登録可能だが閲覧不可 | 中 | `EncyclopediaSystem.cs:1-215`, `GameController.cs` | |

---

## カテゴリBB: ダンジョン生成・部屋タイプの未機能（10件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BB-1 | RoomTypeのShop/Trap/Prison/Storageの4タイプが一度も生成されない。DecorateRooms()がTreasure/Shrineのみ生成し、他は全てNormal部屋 | 高 | `DungeonGenerator.cs:468-481` | |
| BB-2 | 5つのRoomType（Entrance/Prison/Storage/Shop/Trap）にDecorateRoom()のハンドラがない。生成されても装飾/インタラクション不可 | 高 | `RoomCorridorGenerator.cs:104-131` | |
| BB-3 | DungeonFeatureGeneratorの9テーマ（Cave/Ruins/Sewer/Mine/Crypt/Temple/IceCavern/Volcanic/Forest）のパラメータがDungeonGeneratorから参照されない。WaterTileChance/LavaTileChance/CorridorTwistChance等が未適用 | 高 | `DungeonFeatureGenerator.cs`, `DungeonGenerator.cs` | |
| BB-4 | Water/Lavaタイルが定義済みだがダンジョン生成で一度も配置されない。テーマ別環境タイルが完全に欠落 | 高 | `DungeonGenerator.cs`, `DungeonFeatureGenerator.cs` | |
| BB-5 | SecretRoomSystem.CalculateDiscoveryChance()/CalculateSecretRoomCount()が定義済みだがDungeonGeneratorから呼ばれない。秘密の部屋が報酬なしのSecretDoorタイルのみ | 中 | `DungeonGenerator.cs:520-558` | |
| BB-6 | 宝箱がChestOpened=falseで配置されるが開封インタラクションシステムが未接続。宝箱を実際に開けて中身を取得する手段がない | 致命的 | `DungeonGenerator.cs:583-627` | |
| BB-7 | 祠（Shrine）部屋にAltarタイルが配置されるが祝福/バフ付与システムが未接続。祈祷やバフの実効果なし | 高 | `RoomCorridorGenerator.cs:108-109` | |
| BB-8 | フロア深度による部屋サイズのスケーリングなし。全階層で同一サイズの部屋が生成され、深層ほど複雑/広大になる設計が未実装 | 中 | `DungeonGenerator.cs:197-212` | |
| BB-9 | 無限ダンジョンの50階以降が全て同一ティア（Abyss）。マイルストーン報酬（50/100/150階等）やスコアボード永続化が未実装 | 中 | `InfiniteDungeonSystem.cs:17-38` | |
| BB-10 | 階段の到達性検証がDecorateRooms()後に再検証されない。装飾で配置された柱が階段へのルートを塞ぐ可能性あり | 中 | `DungeonGenerator.cs:365-409` | |

---

## カテゴリBC: 死亡・ゲームオーバーの不整合（7件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BC-1 | Ironmanモード（PermaDeath=true）でもセーブファイルが削除されない。SaveManager.DeleteSave()がGameOver時に呼ばれず、永久死がセーブ削除を伴わない | 致命的 | `GameController.cs:3061-3074`, `DifficultySettings.cs:45,172` | |
| BC-2 | DeathCauseに火傷/溺死/環境ダメージ/即死の4種が未定義。StatusEffect由来の死亡が全て「不明な原因」と表示される | 高 | `Enums.cs:126-139` | |
| BC-3 | GameOverSystem.GetDeathCauseDetail()にPoison/Fall/Suicide/SanityDeathのメッセージが未定義。これらの死因で「不明な原因」が表示される | 高 | `GameOverSystem.cs:76-88` | |
| BC-4 | TickStatusEffects()後にHP0チェックがない。状態異常ダメージで死亡した場合、死因追跡が不正確（後段の処理で別の原因に上書きされる可能性） | 高 | `GameController.cs:2184,2361-2366` | |
| BC-5 | ゲームオーバー画面にDeathLogSystemの詳細データ（与ダメージ/被ダメージ/敵撃破数等）が表示されない。フロア数と経過時間のみ表示 | 中 | `MainWindow.xaml.cs:631-665` | |
| BC-6 | 死亡時にゴールド/経験値ペナルティが一切ない。Rebirthで全リソースを保持したまま復活可能で、死のリスクが低い | 中 | `GameController.cs:3020-3076` | |
| BC-7 | 復活アイテム（フェニックスの羽等）/復活魔法が未実装。死亡回避手段がSanityによるRebirth以外に存在しない | 中 | `GameController.cs`, `ItemFactory.cs` | |

---

## カテゴリBD: キーバインド・UI・難易度設定の不整合（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BD-1 | Tキーに「アイテム投げ（ThrowItem）」と「街に入る（EnterTown）」の2アクションが重複バインド。コンテキスト判定で軽減されるが根本的なキー競合 | 致命的 | `KeyBindingSettings.cs:162,165` | |
| BD-2 | OpenCookingとQuitの2アクションにデフォルトキーバインドが未定義。キーボードからアクセス不可能 | 高 | `KeyBindingSettings.cs:145-193` | |
| BD-3 | DifficultySettingsの80%の乗数（ExpMultiplier/EnemyStatMultiplier/HungerDecayMultiplier/DamageTakenMultiplier/DamageDealtMultiplier）がGameControllerで使用されない。Hard/Nightmareを選択してもゲーム難易度がほぼ変わらない | 致命的 | `DifficultySettings.cs`, `GameController.cs:205` | |
| BD-4 | MainWindow.xamlのKeyBindActionマッピングにOpenCooking/Quitのcase文が欠落。キーバインドがあっても変換されずnull返却 | 高 | `MainWindow.xaml.cs:243-269` | |

---

## カテゴリBE: パーティ・隊列システムの欠落（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BE-1 | パーティ隊列（Formation）システムが存在しない。CombatStanceSystemはプレイヤー個人の構え（攻撃的/防御的/均衡）のみで、複数メンバーの戦術的配置が未実装 | 高 | `CombatStanceSystem.cs:1-62` | |
| BE-2 | パーティ全体への回復/バフ伝播メカニクスが存在しない。コンパニオンへの範囲回復やグループバフが未実装 | 高 | `GameController.cs` | |

---

## カテゴリBF: ランダムイベントのクールダウン欠如（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BF-1 | RandomEventSystemにクールダウン/重複防止メカニズムがない。RollEvent()が毎ターン同じイベントを連続発生させる可能性あり（TutorialSystemにはHashSetでの防止あり） | 中 | `WorldMapSystem.cs:794-811` | |

---

## カテゴリBG: バフ・デバフ・状態異常の未機能（11件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BG-1 | StatusEffectのAttackMultiplier/DefenseMultiplier/AllStatsMultiplierが一度も適用されない。GetAllStatModifiers()がStatModifierのみ処理し、乗算フィールドを無視。CreateStrengthBuff(1.25x)/CreateProtection(1.50x)/CreateBlessing(1.10x)の全バフが実効果ゼロ | 致命的 | `Character.cs:32-37`, `StatusEffectSystem.cs:267,279` | |
| BG-2 | StatusEffectsリストがセーブデータに含まれない。ゲームロード時に全てのアクティブバフ/デバフ/呪いが消失する | 致命的 | `SaveData.cs`, `Character.cs:79` | |
| BG-3 | StatusEffect適用時にCheckResistance()がPoison免疫チェックのみ実行。他の状態異常（Stun/Freeze/Sleep等）の耐性値が完全に無視される | 高 | `StatusEffectSystem.cs:297-344`, `GameController.cs:1940-2000` | |
| BG-4 | Equipment.OnEquip()のStatBuffエフェクトループが空実装。装備エンチャントのバフ効果が一切適用されない | 高 | `Equipment.cs:107-114` | |
| BG-5 | 祈祷の祝福バフがAllStatsMultiplier=1.10fを設定するが、BG-1により実効果ゼロ。祈祷が完全に視覚的のみ | 高 | `GameController.cs:4414-4416` | |
| BG-6 | 呪いステータスがAllStatsMultiplier=0.80fを設定するが、BG-1により実効果ゼロ。呪いが設計より20%弱い | 高 | `StatusEffectSystem.cs:174-181` | |
| BG-7 | PotionType.CureAllがStun/Freeze/Sleep/Petrification/Curseを除去対象に含まない。重篤なデバフの治療手段がない | 中 | `Consumables.cs:85-96` | |
| BG-8 | 食品アイテムがバフ効果を一切付与しない。Food.Use()がModifyHunger()/Heal()のみ呼び出し、戦術的な食事バフが存在しない | 中 | `Consumables.cs:150-218` | |
| BG-9 | アクティブバフ数に上限がない。無制限のバフスタックが可能で、パフォーマンス劣化やバランス崩壊の原因となりうる | 中 | `Character.cs:124-146` | |
| BG-10 | 環境デバフ（寒冷/灼熱/湿潤）のStatusEffectTypeが未定義。環境ハザードが状態異常を付与しない | 中 | `StatusEffectSystem.cs`, `Enums.cs` | |
| BG-11 | 状態異常メッセージの一部がenum名（英語）をそのまま表示。ローカライズされた日本語名でなく「Vulnerability」等と表示される | 低 | `GameController.cs` | |

---

## カテゴリBH: 転職・職業・スキルの未機能（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BH-1 | Player.CharacterClassプロパティがprivate set。転職ロジック（TryClassChange）が動作してもクラスを実際に変更不可能。O-2の根本原因 | 致命的 | `Player.cs:19`, `GameController.cs:7261-7282` | |
| BH-2 | SkillDatabaseの6つのパッシブスキル（weapon_mastery/hp_boost/mp_boost/poison_resist/critical_eye/treasure_sense）が定義のみで効果実装なし。ダメージ計算/HP/MP/耐性/クリティカル/ルート率に一切影響しない | 致命的 | `SkillSystem.cs:95,156-160`, `GameController.cs:1440-1515`, `Player.cs:247-248` | |
| BH-3 | GetLearnableSkills()がスキルメニュー表示時に呼ばれない。職業制限なしで全スキルが習得可能 | 高 | `SkillSystem.cs:336-338`, `GameController.cs` | |
| BH-4 | パッシブスキル「knowledge_collect」（バード自動鑑定能力）が登録されるが自動鑑定ロジックなし | 高 | `SkillSystem.cs:142` | |
| BH-5 | ProficiencySystem.GetBonusDamage()が一つのコードパスでのみ呼ばれ、他の戦闘パスではバイパスされる | 中 | `GameController.cs:1455-1459` | |

---

## カテゴリBI: ショップ経済・投資・通貨の未機能（9件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BI-1 | ショップ在庫がランダム性ゼロの固定リスト。同じショップタイプで毎回同一アイテムが並ぶ | 高 | `WorldMapSystem.cs:527-631` | |
| BI-2 | ショップ在庫の時間経過リフレッシュが未実装。InitializeShop()が同一ロジックで再生成するのみ | 高 | `WorldMapSystem.cs:465-469`, `GameController.cs:5451-5454` | |
| BI-3 | InvestmentSystem.Invest()がGameControllerから一度も呼ばれない。投資したゴールドが永久に消失し、配当/損失通知なし | 高 | `InvestmentSystem.cs:1-72`, `GameController.cs:49` | |
| BI-4 | BaseConstructionSystem.GetDailyFoodProduction()がGameControllerから一度も呼ばれない。「畑」建設の自動食料生産が非機能 | 高 | `BaseConstructionSystem.cs:118-121`, `GameController.cs` | |
| BI-5 | PriceFluctuationSystemの需給/テリトリー/カルマ/評判修飾子がUI表示のみで実際のBuy()/Sell()価格に適用されない。表示価格と取引価格が不一致 | 高 | `PriceFluctuationSystem.cs:1-72`, `WorldMapSystem.cs:482-507` | |
| BI-6 | ショップ価格が難易度設定（ExpMultiplier等）を無視。Hard/Nightmareでも同一価格 | 中 | `WorldMapSystem.cs:516-541`, `GameController.cs:205` | |
| BI-7 | 取引に手数料/税金システムが存在しない。売却時の固定60%損失以外の経済シンクなし | 中 | `WorldMapSystem.cs:482-507` | |
| BI-8 | 通貨がゴールド単一。ジェム/トークン等の高額通貨や通貨交換メカニクスが未実装 | 中 | `Enums.cs:517`, `GameController.cs` | |
| BI-9 | NPC購入嗜好/値切り/物々交換システムが未実装。全NPCで同一の売買ロジック | 低 | `NpcSystem.cs`, `WorldMapSystem.cs` | |

---

## カテゴリBJ: ワールドマップ・テリトリー探索の未機能（10件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BJ-1 | RollTerritoryEvent()がGameControllerから一度も呼ばれない。テリトリー固有イベント（森林の精霊/山岳の崩落等）が完全に非トリガー | 致命的 | `WorldMapSystem.cs:743-791`, `GameController.cs` | |
| BJ-2 | 旅行イベント（商人遭遇/盗賊襲撃/宝箱発見等）が表示されるが解決ハンドラなし。報酬/戦闘/選択が発生しない装飾イベント | 高 | `GameController.cs:4961-4973` | |
| BJ-3 | テリトリー間で敵/アイテムの差異なし。ダンジョン生成がテリトリーコンテキストを無視して同一プール使用 | 高 | `DungeonGenerator.cs`, `LocationMapGenerator.cs` | |
| BJ-4 | シンボルマップが全ロケーションを即座に表示（霧の向こう機能なし）。IsExplored追跡がダンジョンタイルのみでワールドマップに存在しない | 高 | `SymbolMapSystem.cs:38-51` | |
| BJ-5 | テリトリー発見時にXP/報酬/実績が一切付与されない。VisitedTerritoriesの追跡が報酬に未連動 | 高 | `WorldMapSystem.cs`, `GameController.cs` | |
| BJ-6 | テリトリー間の高速移動（ファストトラベル）が未実装。隣接テリトリーへの順次移動のみで後半のバックトラッキングが冗長 | 中 | `WorldMapSystem.cs:208-238` | |
| BJ-7 | ダンジョン内イベントがテリトリー別に分化しない。全テリトリーで同一のイベントプール使用 | 中 | `WorldMapSystem.cs:794-811` | |
| BJ-8 | TravelEventType列挙型の6タイプ（Merchant/Ambush/HelpRequest/Shrine/BadWeather/TreasureChest）に解決ロジックなし | 中 | `WorldMapSystem.cs:317-325` | |
| BJ-9 | テリトリー境界が静的定義のみ。動的な国境変動/戦争/占領メカニクスが未実装 | 低 | `WorldMapSystem.cs:129-173` | |
| BJ-10 | SymbolMapEventSystemとWorldMapSystemに重複するイベント処理が存在。責任境界が不明確 | 低 | `SymbolMapEventSystem.cs:46-57`, `WorldMapSystem.cs:743-987` | |

---

## カテゴリBK: 正気度システムの未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BK-1 | 正気度が戦闘パフォーマンスに影響しない。Player.Sanityがミミック検出（line 2398）のみで使用され、命中率/ダメージ/防御力に一切の修正なし | 高 | `GameController.cs:2398`, `Character.cs` | |
| BK-2 | RestSystemの正気度回復がGameControllerで適用されない。休息品質に基づく回復率（0.05-0.2）が定義済みだが未呼出 | 高 | `RestSystem.cs:9`, `GameController.cs:7354,7376` | |
| BK-3 | 正気度回復アイテム（回復ポーション/瞑想/祈祷ボーナス）が未実装。正気度は減少のみで回復手段が死亡時のRescue以外にない | 中 | `Player.cs:400-407`, `ItemFactory.cs` | |
| BK-4 | 幻覚エフェクトが定義済み（Perception-0.3f）だが低正気度で発動しない。マップ歪曲/偽敵等の視覚効果も未実装 | 中 | `ExtendedStatusEffectSystem.cs:35`, `GameController.cs` | |

---

## カテゴリBL: カルマシステムの未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BL-1 | KarmaSystem.GetShopPriceModifier()がGameControllerから一度も呼ばれない。PriceFluctuationSystemが独自に再計算するため完全なデッドコード | 高 | `KarmaSystem.cs:60-70`, `GameController.cs:5483-5514` | |
| BL-2 | KarmaSystem.GetNpcDispositionModifier()がGameControllerから一度も呼ばれない。聖者も悪人もNPC対話結果が同一 | 高 | `KarmaSystem.cs:72-83`, `GameController.cs:5694,5707` | |
| BL-3 | KarmaSystem.CanEnterHolyGround()がGameControllerから一度も呼ばれない。Criminal/Villainプレイヤーが教会/神殿に無制限アクセス可能 | 高 | `KarmaSystem.cs:89`, `GameController.cs` | |
| BL-4 | カルマ変動トリガーが3箇所のみ（処刑+5/+3、密輸-5、闇市-2）。NPC支援/窃盗/対話中の嘘等の大半の選択がカルマ無影響 | 中 | `GameController.cs:1522,7646,7709` | |

---

## カテゴリBM: 評判システムの未機能（3件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BM-1 | ReputationSystem.GetShopDiscount()がGameControllerから一度も呼ばれない。PriceFluctuationSystem.GetReputationModifier()で代替計算され、本来のメソッドがデッドコード | 高 | `ReputationSystem.cs:53-63`, `GameController.cs:5482,5513` | |
| BM-2 | ReputationSystem.IsWelcome()がGameControllerから一度も呼ばれない。「嫌悪」評判でもテリトリーに自由アクセス可能 | 高 | `ReputationSystem.cs:79-80`, `GameController.cs` | |
| BM-3 | 評判は増加のみ（正の修正値）で低下メカニクスが存在しない。クエスト失敗/NPC裏切り/犯罪行為が評判無影響 | 中 | `GameController.cs`, `NpcSystem.cs` | |

---

## カテゴリBN: NPC関係性・好感度の未機能（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BN-1 | NPC好感度データがRebirth（死亡→転生）時に引き継がれない。NpcSystem.CreateTransferData()が存在するがGameControllerで一度も呼ばれず、死亡時に全NPCの好感度が50にリセット | 致命的 | `Player.cs:484-496`, `NpcSystem.cs:122-129`, `GameController.cs:3017,3097,3120` | |
| BN-2 | ModifyAffinity()が対話システムの2箇所でのみ呼ばれる。クエスト完了/贈り物/戦闘協力/祈祷等のアクションが好感度に影響しない | 高 | `GameController.cs:5694,5707`, `NpcSystem.cs:108-112` | |
| BN-3 | 好感度ランクに基づく特殊対話/クエスト解放が未実装。高好感度NPCも低好感度NPCも同一の対話ツリー | 高 | `NpcSystem.cs:177-196`, `GameController.cs:5641-5710` | |
| BN-4 | NpcDefinition.GetAffinityRank()がUI表示に使用されない。プレイヤーがNPCとの関係レベルを確認する手段がない | 中 | `NpcSystem.cs:16-23`, `GameController.cs:6184` | |

---

## カテゴリBO: RelationshipSystemの完全未使用（1件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BO-1 | RelationshipSystem全体がGameControllerで初期化・リセットのみされ、SetRelation()/GetRelation()/ModifyRelation()が一度も呼ばれない。種族/テリトリー/宗教/個人関係の4種の関係性追跡が完全にデッドコード | 致命的 | `RelationshipSystem.cs:1-78`, `GameController.cs:55,3146` | |

---

## カテゴリBP: 価格計算システムの重複・不整合（2件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BP-1 | ReputationSystem/KarmaSystem/RelationshipSystemの各GetShopDiscount()が全てデッドコード。PriceFluctuationSystemのみが使用され、3システムの価格メソッドが冗長 | 高 | `ReputationSystem.cs:53-63`, `KarmaSystem.cs:60-70`, `RelationshipSystem.cs:52-59` | |
| BP-2 | カルマ/評判がNPCエンカウント率に影響しない。WorldMapSystem.RollRandomEvent()の確率がカルマ/評判無関係の固定値 | 中 | `WorldMapSystem.cs:794-811` | |

---

## カテゴリBQ: セーブデータ永続性 — システムステート大量未保存（24件）

24個のシステムがCreateSaveData()/LoadSaveData()に含まれず、ロード時に全進捗が消失する。

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BQ-1 | KarmaSystemステート（カルマ値/履歴）がセーブされない。店舗価格/NPC態度/聖地アクセスに影響するカルマが消失 | 高 | `GameController.cs:41,6694-6801` | |
| BQ-2 | ReputationSystemステート（テリトリー別評判値）がセーブされない | 高 | `GameController.cs:42,6694-6801` | |
| BQ-3 | CompanionSystemステート（仲間/忠誠度/ステータス）がセーブされない。パーティ全消失 | 致命的 | `GameController.cs:43,6694-6801` | |
| BQ-4 | EncyclopediaSystemステート（モンスター討伐数/発見ティア）がセーブされない | 中 | `GameController.cs:44,6694-6801` | |
| BQ-5 | DeathLogSystemステート（死亡記録/統計）がセーブされない | 中 | `GameController.cs:45,6694-6801` | |
| BQ-6 | OathSystemステート（誓約/盟約進捗）がセーブされない | 中 | `GameController.cs:46,6694-6801` | |
| BQ-7 | SkillTreeSystemステート（習得スキル/パッシブ）がセーブされない。スキルツリー進捗全消失 | 致命的 | `GameController.cs:47,6694-6801` | |
| BQ-8 | BaseConstructionSystemステート（建設済み建物/アップグレード）がセーブされない。拠点建設全消失 | 致命的 | `GameController.cs:48,6694-6801` | |
| BQ-9 | InvestmentSystemステート（投資記録/リターン）がセーブされない | 中 | `GameController.cs:49,6694-6801` | |
| BQ-10 | GridInventorySystemステート（グリッドレイアウト）がセーブされない | 中 | `GameController.cs:50,6694-6801` | |
| BQ-11 | NpcMemorySystemステート（NPC記憶/知識）がセーブされない | 中 | `GameController.cs:54,6694-6801` | |
| BQ-12 | RelationshipSystemステート（NPC間関係値）がセーブされない | 中 | `GameController.cs:55,6694-6801` | |
| BQ-13 | ItemIdentificationSystemステート（アイテム識別状態）がセーブされない | 低 | `GameController.cs:56,6694-6801` | |
| BQ-14 | DungeonEcosystemSystemステート（生態系状態）がセーブされない | 中 | `GameController.cs:57,6694-6801` | |
| BQ-15 | PetSystemステート（ペット名簿/ステータス）がセーブされない。全ペット消失 | 致命的 | `GameController.cs:58,6694-6801` | |
| BQ-16 | MerchantGuildSystemステート（ギルドランク/進捗）がセーブされない | 中 | `GameController.cs:59,6694-6801` | |
| BQ-17 | InscriptionSystemステート（刻印ルーン/効果）がセーブされない | 中 | `GameController.cs:60,6694-6801` | |
| BQ-18 | FactionWarSystemステート（派閥戦争/領土）がセーブされない | 中 | `GameController.cs:61,6694-6801` | |
| BQ-19 | TerritoryInfluenceSystemステート（テリトリー影響力）がセーブされない | 中 | `GameController.cs:62,6694-6801` | |
| BQ-20 | ProficiencySystemステート（武器/スキル熟練度）がセーブされない | 中 | `GameController.cs:65,6694-6801` | |
| BQ-21 | DungeonShortcutSystemステート（発見済みショートカット）がセーブされない | 中 | `GameController.cs:66,6694-6801` | |
| BQ-22 | SmithingSystemステート（鍛冶レシピ/進捗）がセーブされない | 中 | `GameController.cs:67,6694-6801` | |
| BQ-23 | AchievementSystemステート（解除済み実績）がセーブされない。実績がロード時にリセット | 高 | `GameController.cs:68,6694-6801` | |
| BQ-24 | TutorialSystemステート（完了済みチュートリアルステップ）がセーブされない。毎回チュートリアル再表示 | 高 | `GameController.cs,SmithingSystem.cs:167-173` | |

---

## カテゴリBR: セーブデータ永続性 — プレイヤー/ゲームステート（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BR-1 | 病気タイプ/残りターン（_playerDisease/_diseaseRemainingTurns）がセーブされない。ロード時に病気が自動治癒 | 致命的 | `GameController.cs:90-93,6694-6801` | |
| BR-2 | 戦闘スタンス（_playerStance）がセーブされない。ロード時にBalancedにリセット | 中 | `GameController.cs:87,6694-6801` | |
| BR-3 | NG+ティア（_ngPlusTier）とゲームクリア状態（_hasCleared）がセーブされない。NG+進行が追跡不可 | 致命的 | `GameController.cs:155-158,6694-6801` | |
| BR-4 | 無限ダンジョンモード（_infiniteDungeonMode）とキル数（_infiniteDungeonKills）がセーブされない。100キル実績進捗消失 | 高 | `GameController.cs:164-167,6694-6801` | |
| BR-5 | ダンジョン特性（_currentDungeonFeature）がセーブされない。敵密度/ルート倍率/罠確率がリセット | 高 | `GameController.cs:78,6694-6801` | |
| BR-6 | Player.HasPrayedTodayがセーブされない。毎日の祈りフラグがリセット | 中 | `Player.cs:303,GameController.cs:6694-6801` | |

---

## カテゴリBS: Engine層戦闘計算不整合（20件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BS-1 | 武器ダメージレンジ（Weapon.DamageRange）が物理ダメージ計算で完全に無視される。EffectiveStats.PhysicalAttackのみ使用 | 致命的 | `Engine/Combat/CombatSystem.cs:154` | |
| BS-2 | 両手武器ダメージボーナスが未実装。IsTwoHandedプロパティがダメージ計算で参照されない | 高 | `Engine/Combat/CombatSystem.cs:140-213` | |
| BS-3 | 武器熟練度がSkillMultiplierとして誤適用（攻撃力ボーナスではなくスキル倍率に適用） | 中 | `Engine/Combat/CombatSystem.cs:195` | |
| BS-4 | 防御貫通（Armor Penetration）が完全未実装。防御は常にdefensePower×0.5の固定割合 | 高 | `Engine/Combat/DamageCalculator.cs:35` | |
| BS-5 | クリティカルダメージ倍率が1.5f固定。設計書の「特化ビルドで最大×2.5」が不可能 | 高 | `Engine/Combat/CombatSystem.cs:199` | |
| BS-6 | 防御計算が物理0.5/魔法0.3のハードコード定数。GameConstantsに未定義 | 中 | `Engine/Combat/DamageCalculator.cs:35,82` | |
| BS-7 | 再生（Regeneration）回復量がhealPerTick=3固定。MND/INTスケーリングなし | 中 | `Engine/StatusEffectSystem.cs:286` | |
| BS-8 | レベルによるダメージスケーリングが完全未実装。PhysicalDamageParams/MagicalDamageParamsにレベルフィールドなし | 高 | `Engine/Combat/DamageCalculator.cs,CombatSystem.cs` | |
| BS-9 | WeaponCritBonus/SkillCritBonusが常に0にハードコード。武器固有クリティカルボーナスが無効 | 高 | `Engine/Combat/CombatSystem.cs:293-294` | |
| BS-10 | ダメージ分散範囲が0.9-1.1固定。名前付き定数未定義 | 低 | `Engine/Combat/DamageCalculator.cs:39,86` | |
| BS-11 | 盾ブロック/パリィメカニクスがDamageCalculatorに未実装。Shield.BlockChanceが参照されない | 高 | `Engine/Combat/DamageCalculator.cs:192` | |
| BS-12 | 防具タイプ別物理耐性なし。重装/軽装が同じ防御計算 | 中 | `Engine/Combat/DamageCalculator.cs:32` | |
| BS-13 | 回復魔法の威力がBaseMpCost×3固定。MND/INTスケーリングなし | 高 | `Engine/Magic/SpellCastingSystem.cs:362-363` | |
| BS-14 | 呪文成功率にキャスターステータス（INT/MND）が反映されない。SpellParser固定値のみ | 高 | `Engine/Magic/SpellCastingSystem.cs:161` | |
| BS-15 | DoTダメージ（毒2%/猛毒5%）がハードコード。毒源による威力差なし | 中 | `Engine/StatusEffectSystem.cs:26,41` | |
| BS-16 | 攻撃者/防御者のレベル差によるダメージスケーリングなし | 中 | `Engine/Combat/DamageCalculator.cs,CombatSystem.cs` | |
| BS-17 | 武器の属性ダメージ増強未適用。Weapon.Elementは攻撃シグネチャにのみ使用 | 中 | `Engine/Combat/CombatSystem.cs:196` | |
| BS-18 | 武器攻撃速度（AttackSpeed）がダメージ分散に影響しない | 低 | `Engine/Combat/DamageCalculator.cs` | |
| BS-19 | オフハンド武器ダメージが未計算。二刀流時にメインハンドのみ処理 | 高 | `Engine/Combat/CombatSystem.cs:154` | |
| BS-20 | 武器射程（Weapon.Range）がダメージ計算で無視。近接武器が任意距離で全ダメージ | 低 | `Engine/Combat/CombatSystem.cs` | |

---

## カテゴリBT: アイテム生成・エンチャント・クラフト不整合（17件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BT-1 | GenerateRandomEquipment()が4種のみ生成（IronSword/Dagger/LeatherArmor/WoodenShield）。Greatsword/BattleAxe/Spear等10+装備が生成不可 | 致命的 | `ItemFactory.cs:865-874` | |
| BT-2 | GenerateRandomConsumable()が4種のみ生成。8+巻物/7+ポーション（Fireball/Lightning/Strength/Invisibility等）が生成不可 | 致命的 | `ItemFactory.cs:910-919` | |
| BT-3 | Scroll.Use()のswitchで3種（Freeze/Summon/Return）がdefaultに落ち「何も起こらなかった」を返す | 高 | `Consumables.cs:256-319` | |
| BT-4 | レアリティスケーリングが弱すぎ。Depth 1とDepth 100のLegendary確率が同じ5%上限 | 中 | `ItemFactory.cs:848-861` | |
| BT-5 | エンチャントがEquipmentItemに実際に保存されない。Enchant()がsuccessを返すが**アイテムオブジェクトを変更しない** | 致命的 | `EnchantmentSystem.cs:139,Equipment.cs:60-76` | |
| BT-6 | エンチャント種別制限なし。ダメージエンチャント（FireDamage等）が防具に適用可。Lifestealが防具に適用可 | 高 | `EnchantmentSystem.cs:128-134` | |
| BT-7 | 8エンチャント種（Lifesteal/ManaSteal/ParalysisChance/ExpBoost/DropBoost/CriticalBoost/SpeedBoost/DefenseBoost）の効果計算が未実装（return 0） | 致命的 | `EnchantmentSystem.cs:186-206` | |
| BT-8 | エンチャント除去/置換メソッドが存在しない。RemoveEnchantment()/ReplaceEnchantment()なし | 高 | `EnchantmentSystem.cs:1-209` | |
| BT-9 | 複数エンチャント非対応。同一装備に2つ以上のエンチャントを持てない | 中 | `Equipment.cs:60-76` | |
| BT-10 | クラフトレシピが参照する`material_coal`がItemDefinitionsに定義されていない。鋼鉄の剣がクラフト不可 | 致命的 | `CraftingSystem.cs:275,ItemFactory.cs:1159-1260` | |
| BT-11 | クラフトにスキル/ツール/職業要件なし。全職業が全アイテムをクラフト可能 | 中 | `CraftingSystem.cs:92-111` | |
| BT-12 | ItemFactory.GenerateLoot()がDropTableSystemを使用しない。ボスドラゴンがゴブリンと同じランダムアイテムをドロップ | 致命的 | `ItemFactory.cs:950-969` | |
| BT-13 | 非人型敵（Dragon/Undead/Demon）がゴールドをドロップしない。Humanoidのみゴールド生成 | 高 | `DropTableSystem.cs:188` | |
| BT-14 | 宝箱システム（TreasureChest/ChestGeneration）が存在しない。ダンジョン宝箱のアイテム生成不可 | 高 | リポジトリ全体 | |
| BT-15 | 強化レベルの上限検証なし。Legendary生成でEnhancementLevel=9が可能だが上限チェックなし | 低 | `ItemFactory.cs:879-886` | |
| BT-16 | 未識別アイテムのUnidentifiedNameが設定されない。IsIdentified=falseだが未識別名なし | 中 | `ItemFactory.cs:888-890` | |
| BT-17 | 呪いアイテム率の計算式が不適切。Math.Min(5+depth*2,30)でdepth=13で31→30にclamp | 低 | `ItemFactory.cs:892-896` | |

---

## カテゴリBU: 時間・デイリー処理・チュートリアル・実績不整合（17件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BU-1 | 日変更検出メカニズムが存在しない。GameTime.Dayの変化を検出するパターンなし | 致命的 | `GameController.cs:2125+` | |
| BU-2 | ProcessReligionDailyTick()が定義済みだが**一度も呼び出されない**。信仰減衰/背教呪い/祈り日数が機能しない | 高 | `GameController.cs:4882` | |
| BU-3 | PetSystem.TickHunger()が一度も呼び出されない。ペットの空腹/忠誠度減衰が機能しない | 中 | `GameController.cs:58,PetSystem.cs:112` | |
| BU-4 | GameTimeのStartYear/StartMonth/StartDay/StartHour/StartMinuteがSaveDataに含まれない。ロード時にデフォルト値にリセット | 中 | `GameTime.cs:52-64,SaveData.cs:241-244` | |
| BU-5 | コンパニオンの忠誠度減衰/空腹メカニクスが未実装。戦闘時のみ処理される | 中 | `CompanionSystem.cs,GameController.cs:1796` | |
| BU-6 | 施設/ショップの営業時間チェックなし。TimeOfDaySystemが施設可用性に接続されていない | 低 | `WorldMapSystem.cs:372` | |
| BU-7 | NPC行動スケジュールが600ターンごとの固定間隔で更新。時間帯変更との連動なし | 中 | `GameController.cs:2313-2320` | |
| BU-8 | 天候システムの日次リセットなし。300ターンごとの固定間隔で変化 | 低 | `GameController.cs:2329-2337` | |
| BU-9 | TurnCountとGameTime.TotalTurnsの二重追跡。同期保証メカニズムなし | 低 | `GameController.cs:191-192` | |
| BU-10 | DifficultySettings.ExpMultiplier（0.6～1.5）が経験値獲得に適用されない。GoldMultiplierは適用済み | 高 | `GameController.cs:1540,DifficultySettings.cs:18-42` | |
| BU-11 | AchievementSystemステートがセーブされない（BQ-23と同根）。実績解除がセッション間で消失 | 高 | `GameController.cs:68,1727,2357-2359` | |
| BU-12 | TutorialSystemステートがセーブされない（BQ-24と同根）。完了済みチュートリアルが毎回再表示 | 高 | `SmithingSystem.cs:167-173` | |
| BU-13 | チュートリアル個別スキップ機能なし。IsEnabled=falseで全無効のみ。個別ステップのスキップ不可 | 中 | `SmithingSystem.cs:92,140-152` | |
| BU-14 | 難易度設定がゲーム中に制限なく変更可能。SetDifficulty()にターン数/ダンジョン状態のバリデーションなし | 中 | `GameController.cs:3228-3233` | |
| BU-15 | EncyclopediaSystemの発見進捗がセーブされない（BQ-4と同根）。モンスター討伐数が消失 | 中 | `GameController.cs:44,7092` | |
| BU-16 | DeathLogSystemの死亡記録がセーブされない（BQ-5と同根）。メタ統計が消失 | 中 | `GameController.cs:45,DeathLogSystem.cs:42-79` | |
| BU-17 | 時間ベースのステータス効果期限切れ未実装。「日没まで持続」等の効果パターンなし | 低 | `StatusEffect.cs,TimeOfDaySystem.cs` | |

---

## カテゴリBV: マップ生成・フロア遷移不整合（10件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BV-1 | PlaceStairs()で入口/ボス部屋がnullの場合に階段が生成されない。フォールバックなしでフロア進行不可 | 致命的 | `DungeonGenerator.cs:365-392` | |
| BV-2 | IsReachable() BFSがトラップ/チェスト配置前に実行される。配置後に通路がブロックされる可能性あり | 高 | `DungeonGenerator.cs:414-456` | |
| BV-3 | フロアキャッシュヒット時にプレイヤー位置が古いキャッシュ位置に復帰。初回訪問と再訪問の区別なし | 高 | `GameController.cs:650-663` | |
| BV-4 | キャッシュ復元時に階段存在確認なし。StairsUpPosition/EntrancePosition両方nullの場合、フォールバック(5,5)が壁の可能性 | 致命的 | `GameController.cs:650-663` | |
| BV-5 | 階段到達性リトライが階段位置を更新しない。回廊追加後も古い到達不可位置のまま | 中 | `DungeonGenerator.cs:395-408` | |
| BV-6 | ConnectRooms()のMSTが入口/ボス部屋の接続を保証しない。rooms[0]が入口でない場合に未接続 | 中 | `DungeonGenerator.cs:286-339` | |
| BV-7 | TryAscendStairs()がキャッシュマップのStairsDownPositionを使用。新フロアではなく古いマップの位置を参照 | 高 | `GameController.cs:2733` | |
| BV-8 | モンスター配置が階段タイル上を除外しない。敵が階段をブロックする可能性 | 中 | `GameController.cs:908-922` | |
| BV-9 | NewGame+/Rebirthでフロアキャッシュがクリアされない。前回プレイの古いフロアが再利用される | 中 | `GameController.cs:644-646` | |
| BV-10 | PlaceStairs()で入口/ボス部屋null時の緊急フォールバック（任意歩行可能タイル）が未実装 | 低 | `DungeonGenerator.cs:373-392` | |

---

## カテゴリBW: 転生・NewGame+・エンディング不整合（7件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BW-1 | InitializeNewGamePlus()がキャリーオーバーを表示するが適用しない。レベル1/初期ゴールド/無装備にリセット | 高 | `GameController.cs:7154-7167` | |
| BW-2 | NG+敵スケーリング乗数が計算されるが適用されない。EXPボーナスのみ使用でダンジョン生成時の敵強化なし | 高 | `GameController.cs:751-752,1666` | |
| BW-3 | Wandererエンディングが到達不可能。allTerritoriesVisitedがfalse固定値でハードコード | 高 | `GameController.cs:7811,MultiEndingSystem.cs:54-61` | |
| BW-4 | ExecuteRebirth()がGetRebirthEffect()を呼ばない。宗教による転生ボーナス（HP/MP/ステータス）が破棄 | 中 | `GameController.cs:4894-4896,3085-3100` | |
| BW-5 | TransferDataにLevelフィールドなし。転生時に常にLv1に戻る。進捗保持の手段がない | 高 | `Player.cs:849-860` | |
| BW-6 | TransferDataにGoldフィールドなし。転生時に全ゴールド消失。背景初期ゴールドのみ | 高 | `Player.cs:849-860,659` | |
| BW-7 | テリトリー訪問追跡メカニズム不在。6テリトリーのうちどれを訪問したか記録する仕組みがない | 高 | `GameController.cs:7810,MultiEndingSystem.cs:101` | |

---

## カテゴリBX: インベントリ・装備・重量詳細不整合（10件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BX-1 | Inventory.Add()が重量制限を無視。UsedSlots < MaxSlotsのみチェックし、TotalWeight vs MaxWeightを検証しない | 高 | `Inventory.cs:35-56` | |
| BX-2 | 盾が両手武器装備中でもオフハンドに装備可能。両手武器→盾装備禁止の逆チェックなし | 致命的 | `Equipment.cs:341-367` | |
| BX-3 | 両手武器装備時にオフハンド装備が消失。previousItemとしてMainHandのみ返却、OffHandはインベントリに戻らない | 致命的 | `Equipment.cs:349-356,GameController.cs:2828-2839` | |
| BX-4 | Armor.SpeedModifierが完全未適用。革鎧(0.95)/板金鎧(0.8)等の速度補正がStats.ActionSpeed計算に反映されない | 高 | `Equipment.cs:264,Stats.cs:33` | |
| BX-5 | OnUnequip()が空実装。統計修正以外の永続的効果が解除されない。呪い装備のチェックのみ | 中 | `Equipment.cs:116-123` | |
| BX-6 | 装備比較/プレビュー機能が完全未実装。装備前のステータス差分表示なし | 中 | リポジトリ全体 | |
| BX-7 | 耐久度0以下の装備が装備可能。CanEquip()にDurability <= 0チェックなし | 高 | `Item.cs:246-249,Equipment.cs:83` | |
| BX-8 | IsOverweightプロパティが存在するが効果なし。移動速度低下/スタミナ消費増/行動コスト増なし | 中 | `Player.cs:172` | |
| BX-9 | アクセサリスロット（Neck/Ring1/Ring2/Back/Waist）5箇所が未テスト。全スロット動作確認なし | 低 | `Item.cs:7-11` | |
| BX-10 | 両手武器装備時OffHandアイテムのOnUnequip()は呼ばれるが、返却処理のTODOコメントが放置 | 高 | `Equipment.cs:349-356` | |

---

## カテゴリBY: 魔法・呪文・言語システム詳細不整合（16件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BY-1 | ApplySpellHealがAllAlliesターゲットを無視。`graeda vinir`（全味方回復）がプレイヤーのみ回復 | 致命的 | `GameController.cs:4362-4377` | |
| BY-2 | ApplySpellBuffがTargetTypeを一切チェックしない。`styrkja vinir`（全味方バフ）がプレイヤーのみバフ | 致命的 | `GameController.cs:4401-4438` | |
| BY-3 | ApplySpellControlにAllAlliesケース欠落。`binda vinir`のハンドラなし | 致命的 | `GameController.cs:4441-4482` | |
| BY-4 | ApplySpellSealがAllAlliesターゲットを無視。`banna vinir`が失敗 | 致命的 | `GameController.cs:4648-4679` | |
| BY-5 | Object(hlutr)/Ground(jord)ターゲットタイプが全呪文適用で未処理。`opna hlutr`（解錠）が機能しない | 致命的 | `GameController.cs:4255-4314` | |
| BY-6 | ApplySpellDetectがトラップのみ検出。オブジェクト検出(`sja hlutr`)と地面透視(`sja jord`)が未実装 | 高 | `GameController.cs:4485-4511` | |
| BY-7 | ApplySpellUnlockがTargetTypeを未チェック。環境オブジェクトとの連携なし | 高 | `GameController.cs:4512-4535` | |
| BY-8 | 回復呪文にSingleAlly/AllAllies/SingleEnemyのケースなし。Selfのみ処理 | 致命的 | `GameController.cs:4362-4377` | |
| BY-9 | ApplySpellResurrectがTargetType未チェック。コンパニオン蘇生不可、プレイヤーのみ | 高 | `GameController.cs:4682-4702` | |
| BY-10 | 敵の魔法耐性が未実装。全敵が全属性に0%耐性。炎敵も炎呪文で通常ダメージ | 高 | `Enemy.cs (GetResistanceAgainst未オーバーライド)` | |
| BY-11 | 聖職者の初期単語に`vinir`（ターゲット修飾子）が含まれるが、単体では無効な呪文。効果語なしエラー | 高 | `Player.cs:682` | |
| BY-12 | 巻物アイテム（scroll_fireball/scroll_teleport等）がダンジョンに配置されるが使用メカニズムなし | 高 | `DungeonGenerator.cs:687,742` | |
| BY-13 | テレポート呪文が射程語を無視。`senda beinn`（5マス）も`senda heimr`（99マス）も同じランダム移動 | 中 | `GameController.cs:4536-4549` | |
| BY-14 | 呪文熟練度が上限100に到達してもフィードバックなし。プレイヤーが上限認識不可 | 低 | `Player.cs:216-220` | |
| BY-15 | SpellEffect.IsNoneの判定がPower==0 AND Range==0のみ。Power>0/Range=0の無効呪文が通過 | 中 | `SpellCastingSystem.cs:545-546` | |
| BY-16 | 呪文巻物とLearnFromAncientBookの接続なし。巻物から言葉を学習するパスが未実装 | 中 | `GameController.cs,DungeonGenerator.cs` | |

---

## カテゴリBZ: クエスト・ギルド・派閥詳細不整合（15件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| BZ-1 | TurnInQuest()がアイテム報酬を配布しない。Gold/Experienceのみ処理し、ItemIdsフィールドを無視 | 致命的 | `NpcSystem.cs:434-456` | |
| BZ-2 | メインクエストのGiverNpcIdが`guild_master`（接頭辞なし）。他の全クエストは`npc_*`接頭辞使用 | 高 | `NpcSystem.cs:539` | |
| BZ-3 | Deliver/Talk/Escortクエストタイプのハンドラなし。UpdateObjective()がID一致のみで進捗を増加 | 致命的 | `NpcSystem.cs:412-431` | |
| BZ-4 | クエスト前提条件/チェーンシステムなし。QuestDefinitionにPrerequisiteQuestIdsフィールドなし | 高 | `NpcSystem.cs:327-336` | |
| BZ-5 | MerchantGuildSystemがセーブ/ロードされない。ギルド会員/交易ルートが消失（BQ-16と同根） | 高 | `GameController.cs:6694-7000` | |
| BZ-6 | FactionWarSystemがセーブ/ロードされない。派閥戦争状態が消失（BQ-18と同根） | 高 | `GameController.cs:6694-7000` | |
| BZ-7 | MerchantGuildSystemの全機能がGameControllerから未公開。ギルド加入/交易実行の手段なし | 致命的 | `GameController.cs:59` | |
| BZ-8 | FactionWarSystemの全機能がGameControllerから未公開。派閥戦争参加/選択の手段なし | 致命的 | `GameController.cs:61` | |
| BZ-9 | QuestState.Failedが定義済みだが一度もセットされない。クエスト失敗条件/タイムアウトなし | 中 | `Enums.cs:484-490,NpcSystem.cs` | |
| BZ-10 | ギルドランクボーナスがショップ価格/NPC対話に適用されない。ランク上昇のゲームプレイ効果なし | 高 | `MerchantGuildSystem.cs:115-127` | |
| BZ-11 | QuestBoardWindowの「完了タブ」がGetActiveQuests()を呼ぶ紛らわしい実装。機能的には正常 | 低 | `QuestBoardWindow.xaml.cs:148-192` | |
| BZ-12 | QuestLogWindowが読み取り専用。クエスト受注/報告ボタンなし。QuestBoardWindow必須 | 中 | `QuestLogWindow.xaml.cs:42-79` | |
| BZ-13 | MerchantGuild JoinGuild()が初期ランクNone（ランク無し）を設定。GuildSystemはCopper設定。一貫性なし | 中 | `MerchantGuildSystem.cs:46-50` | |
| BZ-14 | 派閥選択がNPC行動/対話/クエスト可用性に影響しない。派閥選択が装飾のみ | 高 | `FactionWarSystem.cs:81-102` | |
| BZ-15 | ゲームロード時にクエスト完了状態の検証なし。破損データがサイレントに復元される | 低 | `GameController.cs:6957-6962` | |

---

## カテゴリCA: セーブデータ・シリアライズ詳細（8件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CA-1 | PlayerSaveDataにThirst/Fatigue/Hygieneフィールドなし。RestoreFromSave()がデフォルト値100で常にリセット | 致命的 | `SaveData.cs:83-163`, `GameController.cs:6810-6822` | |
| CA-2 | StatusEffects（毒/バフ/デバフ等）がSaveDataに含まれない。ロード時に全状態異常が消失 | 致命的 | `SaveData.cs(欠落)`, `GameController.cs:6694-6980` | |
| CA-3 | SaveDataにVersionフィールドあるが一切使用されない。スキーマ変更時のマイグレーションパスなし | 高 | `SaveData.cs:11`, `SaveManager.cs:36-43` | |
| CA-4 | セーブ操作がターン処理中に同期なしで実行可能。ProcessTurnEffects中のセーブで不整合な中間状態をキャプチャ | 致命的 | `GameController.cs:1185-1197`, `MainWindow.xaml.cs:830-841` | |
| CA-5 | SaveManager.Save()にtry/catchなし。ディスク容量不足/権限エラーで未処理例外によりクラッシュ | 高 | `SaveManager.cs:22-31` | |
| CA-6 | SaveManager.Load()で不正JSONのデシリアライズ失敗が未処理。破損セーブファイルでクラッシュの可能性 | 高 | `SaveManager.cs:36-43` | |
| CA-7 | MultiSlotSaveSystemがSaveManagerと連携しない。スロット管理がインメモリのみでファイルI/Oと不整合 | 中 | `MultiSlotSaveSystem.cs:6-69` | |
| CA-8 | FacingDirection（キャラクターの向き）がセーブに含まれない。ロード時にデフォルト方向にリセット | 中 | `SaveData.cs(欠落)`, `Character.cs` | |

---

## カテゴリCB: 状態異常・バフ・デバフ詳細（14件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CB-1 | StatusEffectType.Slow/Vulnerability/Invisibility/Blessing/Apostasyの5種がCombatSystem.CreateStatusEffectでnull返却。ファクトリメソッド欠落 | 致命的 | `CombatSystem.cs:343-370` | |
| CB-2 | StatusEffectSystemにCreateSlow/CreateVulnerability/CreateInvisibility/CreateBlessing/CreateApostasyの5メソッドなし | 高 | `StatusEffectSystem.cs:19-295` | |
| CB-3 | CureAllポーションが22種中9種のみ解除。「全てのデバフを解除」コメントと矛盾。Charm/Madness/Petrification/Fear/Sleep等13種が解除されない | 致命的 | `Consumables.cs:85-96` | |
| CB-4 | Curse/Petrificationがint.MaxValue持続だが解除手段がCurseのみ（専用アイテム）。Petrificationの解除方法なし | 高 | `StatusEffectSystem.cs:174-228` | |
| CB-5 | MonsterRaceSystem.IsStatusEffectImmune()がCombatSystem.TryApplyStatusEffect()で呼び出されない。ボス免疫が機能しない | 致命的 | `CombatSystem.cs:307-338`, `MonsterRaceSystem.cs:132` | |
| CB-6 | 耐性チェックが最大95%キャップ。真の免疫（100%確定）と耐性（確率的）の区別なし | 中 | `StatusEffectSystem.cs:299-332` | |
| CB-7 | 矛盾する効果（Haste+Slow、Strength+Weakness）が同時適用可能。相反効果の管理ロジックなし | 中 | `Character.cs:124-146` | |
| CB-8 | 出血効果が「移動ごとにHP減少」設計だがTickStatusEffects()はターン単位のみ。移動時ダメージハンドラなし | 中 | `Character.cs:161-187` | |
| CB-9 | Vulnerability効果にDefenseMultiplierなし。StatusEffectとして適用されるが実際の被ダメージ増加効果がゼロ | 致命的 | `StatusEffect.cs`, `CombatSystem.cs:368` | |
| CB-10 | Invisibility効果にHitRate/Evasion修飾なし。適用されても回避率/被命中率に影響なし | 致命的 | `StatusEffect.cs`, `CombatSystem.cs:368` | |
| CB-11 | フロア遷移時の状態異常クリア/永続化が未定義。無限持続効果が100フロア蓄積する可能性 | 中 | `GameController.cs(フロア遷移)` | |
| CB-12 | 毒による死亡はDeathCause.Poisonで検出されるが専用死亡シーケンスなし（汎用死亡と同一） | 低 | `GameController.cs:2364,3034` | |
| CB-13 | Slow効果のTurnCostModifier=1.50fがGameControllerで手動設定のみ。CombatSystemからのSlow適用はnull返却で機能しない | 高 | `GameController.cs:4455`, `CombatSystem.cs:368` | |
| CB-14 | Blessing/Apostasyの2種がEnum定義のみで完全未実装。ファクトリ/適用/効果の全ロジックなし | 中 | `Enums.cs(StatusEffectType)` | |

---

## カテゴリCC: ペット・コンパニオン・召喚詳細（15件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CC-1 | ペットの戦闘アクション（Attack/Guard/Heal/Forage）が未実装。SpecialAbility文字列は格納のみで実行メソッドなし | 致命的 | `PetSystem.cs:31,43-48` | |
| CC-2 | ペットの死亡/餓死メカニクスなし。Hunger=0でもLoyalty-2のみでペット消滅しない | 高 | `PetSystem.cs:10-21,112-120` | |
| CC-3 | ペットのLevel/Experienceフィールドが存在するが経験値付与/レベルアップメソッドなし。成長システムが完全未実装 | 高 | `PetSystem.cs:10-21` | |
| CC-4 | ペット装備スロットなし。PetStateに装備関連フィールドが一切存在しない | 中 | `PetSystem.cs:10-32` | |
| CC-5 | コンパニオンがIsAlive=false後にRemoveDeadCompanions()で永久削除。復活/蘇生メカニクスなし | 高 | `CompanionSystem.cs:155-183` | |
| CC-6 | コンパニオンがスキル/特殊能力を使用しない。全AIモードでCalculateCompanionDamage()の基本攻撃のみ | 高 | `CompanionSystem.cs:98-152` | |
| CC-7 | PartySystemが存在しない。前列/後列の隊列システムなし。全コンパニオンが同一条件で戦闘 | 中 | `CompanionSystem.cs(List<CompanionData>のみ)` | |
| CC-8 | コンパニオンのFollowMode/SupportModeがメッセージ返却のみで実行動（回復/バフ等）なし | 高 | `CompanionSystem.cs:143-145` | |
| CC-9 | コンパニオンがクエスト/ストーリーに反応しない。対話/意見/リアクションシステムなし | 中 | `CompanionSystem.cs:9-21` | |
| CC-10 | 召喚呪文で生成されたクリーチャーに持続時間なし。無限に存在し、自発的解散メカニクスもなし | 高 | `GameController.cs:4560-4596` | |
| CC-11 | ペットがマップ上に位置を持たない。PetStateにPosition/座標フィールドなし | 中 | `PetSystem.cs:10-21` | |
| CC-12 | ペット数の上限なし。CompanionSystem(MaxPartySize=4)と異なりPetSystemはDictionary上限なし | 中 | `PetSystem.cs:34,56-62` | |
| CC-13 | PetSystem.TickHunger()がGameControllerのProcessTurnEffects()から呼び出されない。ペット空腹が永遠に減少しない | 致命的 | `GameController.cs(ProcessTurnEffects)`, `PetSystem.cs` | |
| CC-14 | CompanionSystem.CheckDesertion()が定義済みだが一度も呼び出されない。低忠誠度コンパニオンが脱走しない | 高 | `CompanionSystem.cs:76-86` | |
| CC-15 | ペットExperienceフィールドが0で初期化後、値を変更するコードが全コードベースに存在しない | 高 | `PetSystem.cs:15,59` | |

---

## カテゴリCD: 難易度・バランス・設定不整合（9件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CD-1 | DifficultyConfig.ExpMultiplierが経験値計算に未適用。Hard/Nightmareの経験値減少が機能しない | 致命的 | `GameController.cs:1540` | |
| CD-2 | DifficultyConfig.HungerDecayMultiplierが空腹減少に未適用。全難易度で同一の空腹減少速度 | 高 | `GameController.cs:2132` | |
| CD-3 | DifficultyConfig.DamageTakenMultiplier/DamageDealtMultiplierがDamageCalculatorで未使用。Easyでも被ダメージ同一 | 致命的 | `DamageCalculator.cs:28-93` | |
| CD-4 | DifficultyConfig.ItemDropMultiplierがDropTableSystem呼び出しに未適用。全難易度でドロップ率同一 | 高 | `GameController.cs:1577-1593` | |
| CD-5 | DifficultyConfig.EnemyStatMultiplierが敵生成に未適用。深度スケーリングのみで難易度スケーリングなし | 高 | `DropTableSystem.cs:99-110` | |
| CD-6 | RestSystem.GetRecovery()に難易度乗数なし。Easy+高回復で戦闘が些末化する | 中 | `RestSystem.cs:9-16` | |
| CD-7 | AudioManager.ApplyVolumeSettings()がGameSettings.EffectiveVolume系プロパティを使用しない。設定UIとオーディオシステムが未接続 | 高 | `AudioManager.cs:129-138`, `GameSettings.cs:53-58` | |
| CD-8 | レベル50キャップ到達後の報酬/称号/特殊ボーナスなし。エンドゲームコンテンツが欠如 | 中 | `Player.cs:359` | |
| CD-9 | Ironman難易度のUI説明「敵ダメージ1.5倍」が実コード値1.2倍と不一致 | 高 | `DifficultySettings.cs:158-173`, `DifficultySelectWindow.xaml.cs` | |

---

## カテゴリCE: チュートリアル・ヘルプ・UIフロー（16件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CE-1 | ThrowItemとEnterTownが両方ともKey.Tにバインド。キー競合で一方のみ動作 | 致命的 | `KeyBindingSettings.cs:162,165` | |
| CE-2 | ContextHelpSystem.RegisterDefaultTopics()が一度も呼び出されない。ヘルプシステムが完全に空 | 致命的 | `GameController.cs:7830,7833`, `ContextHelpSystem.cs:124-133` | |
| CE-3 | ヘルプ表示用のキーバインド/メニュー項目が存在しない。プレイヤーがヘルプにアクセスする手段なし | 高 | `MainWindow.xaml.cs:175-313` | |
| CE-4 | TutorialSystem.OnTrigger()が_triggeredEventsで同一セッション内の再トリガーを完全ブロック。チュートリアル再表示不可 | 中 | `TutorialSystem.cs(OnTrigger)` | |
| CE-5 | ゲームオーバー時に「リスタート」オプションなし。タイトルへ戻る/ゲーム終了の2択のみ | 中 | `MainWindow.xaml.cs:631-665` | |
| CE-6 | チュートリアル表示がMessageBox.Show()のモーダルダイアログ。ゲーム入力を完全ブロックし、キャンセル/スキップ不可 | 中 | `MainWindow.xaml.cs:1000-1005` | |
| CE-7 | キャラクター作成時の名前バリデーションが最大長チェック/禁止文字チェックなし。空白時はサイレントにデフォルト名設定 | 低 | `CharacterCreationWindow.xaml.cs:95-112` | |
| CE-8 | SettingsWindow.SaveButton_Click()後にMainWindowへのコールバックなし。設定変更が再起動まで反映されない | 中 | `SettingsWindow.xaml.cs:59-68` | |
| CE-9 | ミニマップ表示/非表示状態がGameSettingsに永続化されない。ゲーム再起動で常にデフォルトに戻る | 低 | `MainWindow.xaml.cs:54,301-302` | |
| CE-10 | InventoryWindowのグリッド位置がアイテム数変更後に境界外参照の可能性。保存位置のバリデーションなし | 低 | `InventoryWindow.xaml.cs:40-78` | |
| CE-11 | DialogueWindow生成時に前回のウィンドウが明示的に破棄されない。繰り返し対話でメモリリーク | 低 | `MainWindow.xaml.cs:919-931` | |
| CE-12 | Fog of War（探索済み領域）の永続化が未確認。セーブ/ロードで探索状態が失われる可能性 | 中 | `GameRenderer.cs` | |
| CE-13 | メッセージログがMaxMessages=50で無通知で切り捨て。古いメッセージの消失をプレイヤーに通知しない | 低 | `MainWindow.xaml.cs:619-629` | |
| CE-14 | キャラクターステータス画面が装備/バフ/デバフ別のステータス内訳を表示しない。最終値のみで原因特定不可 | 中 | `StatusWindow.xaml.cs:37-58` | |
| CE-15 | ContextHelpSystem未初期化状態でもエラー表示なし。ヘルプが故障しているのか未実装なのか判別不可 | 中 | `全UIウィンドウ` | |
| CE-16 | PauseWindow/SettingsWindowの子ウィンドウがフォーカス喪失時にオーファンになる可能性。エラーハンドリングなし | 低 | `MainWindow.xaml.cs:1022` | |

---

## カテゴリCF: 天候・視界・環境効果詳細（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CF-1 | WeatherSystemが天候別戦闘修飾子（SightModifier/FireDamageModifier/RangedHitModifier/MovementCostModifier）を定義しているが、DamageCalculator/CombatSystemで一切使用されない。天候が純粋に装飾的 | 高 | `WeatherSystem.cs:24-80`, `DamageCalculator.cs:28-93` | |
| CF-2 | GameState.csの移動コスト計算がWeatherSystem.MovementCostModifier（吹雪1.5倍等）を参照しない。全天候で同一移動コスト | 高 | `GameState.cs:36-51`, `WeatherSystem.cs:65` | |
| CF-3 | ComputeFov()がIsVisibleフラグを設定するが、敵AIのHasLineOfSight()はFOV結果を使用せず独立計算。FOV外の敵が攻撃可能 | 中 | `DungeonMap.cs:247-267`, `BasicBehaviors.cs:212,318,370` | |
| CF-4 | 室内/室外の天候区別なし。ダンジョン内でも吹雪/雷雨の天候効果が適用される設計上の矛盾 | 中 | `WeatherSystem.cs`, `GameController.cs:7021` | |
| CF-5 | LightingSystemの光源（松明/ランタン）がFOV半径に影響しない。光源アイテムを持っても暗闇の視界範囲が同一 | 中 | `LightingSystem.cs`, `DungeonMap.cs:247-267` | |
| CF-6 | WeatherSystem.CurrentWeatherが更新されるがUI通知のみ。天候変化がゲームプレイに実質的影響ゼロ | 中 | `GameController.cs:7021`, `WeatherSystem.cs` | |

---

## カテゴリCG: 採集・釣り・採掘ノード詳細（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CG-1 | GatheringSystemが5種ノード（Herb/Mining/Logging/Fishing/Foraging）を定義しているが、TileクラスにGatheringNodeフィールドが存在しない | 致命的 | `GatheringSystem.cs:6-90`, `Tile.cs:245-325` | |
| CG-2 | DungeonGeneratorにPlaceTraps()/PlaceChests()メソッドはあるがPlaceGatheringNodes()メソッドが存在しない。採集ノードがマップ上に配置されない | 高 | `DungeonGenerator.cs:560-576` | |
| CG-3 | GatheringSystemのスキルレベルによる成功率計算ロジックが存在するが、PlayerにGatheringProficiencyフィールドなし | 高 | `GatheringSystem.cs:40-60`, `Player.cs` | |
| CG-4 | MiningSystem/FishingSystemの道具要件（ツルハシ/釣り竿）がアイテム定義に存在しない。道具なしで採集を試行しても同じ結果 | 中 | `GatheringSystem.cs`, `ItemFactory.cs` | |
| CG-5 | 採集システムの希少度計算（Rarity）が定義されているがドロップテーブルとの連携なし。採集品の品質がランダムのみ | 中 | `GatheringSystem.cs:70-90`, `DropTableSystem.cs` | |

---

## カテゴリCH: 傷・疾病・ステルスシステム詳細（7件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CH-1 | _playerDiseaseと_diseaseRemainingTurnsがGameControllerで追跡されるがSaveDataに含まれない。ロード時に疾病が消失 | 致命的 | `GameController.cs:90-93`, `SaveData.cs:8-78` | |
| CH-2 | BodyConditionSystemが5種傷定義（Cut/Bruise/Puncture/Fracture/Burn）をステータス修飾子付きで持つが、Player/Characterに傷トラッキングフィールドなし | 高 | `BodyConditionSystem.cs:9-24`, `Player.cs`, `Character.cs` | |
| CH-3 | 戦闘ダメージ時にBodyConditionSystem.CreateWound()が呼び出されない。TakeDamage()は傷を生成しない | 高 | `Character.cs:83-103`, `BodyConditionSystem.cs` | |
| CH-4 | CombatState.Stealth=4がEnum定義されProficiencyCategory.Stealthが存在するが、StealthSystem.csファイルが存在しない。ステルスが完全未実装 | 高 | `Enums.cs:63,661` | |
| CH-5 | 敵AIの検出メカニクス（距離/騒音/光量ベース）が存在しない。全敵がIsVisible判定のみで即座にプレイヤーを検知 | 中 | `BasicBehaviors.cs:212,318,370` | |
| CH-6 | DiseaseSystem.cs疾病進行メカニクスがGameController.cs:2256-2294で実装されているが、疾病の具体的な感染源（汚染水/腐敗食品/敵攻撃）との紐付けなし | 中 | `GameController.cs:2256-2294` | |
| CH-7 | 傷のステータス修飾子（STR-0.05f～-0.15f）と治癒時間（15～60ターン）が定義済みだがDamageCalculatorに参照なし | 中 | `BodyConditionSystem.cs:17-24`, `DamageCalculator.cs` | |

---

## カテゴリCI: ポーション・巻物・アクセサリ効果詳細（9件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CI-1 | PotionType.StrengthBoost/AgilityBoost/IntelligenceBoost/Invisibility/FireResistance/ColdResistanceの6種がItemFactoryで生成可能だがPotion.Use()のswitch文に対応caseなし。使用時「効果がなかった」 | 中 | `Consumables.cs:47-100`, `ItemFactory.cs:516-566` | |
| CI-2 | ScrollType.Sanctuary/Freezeの2種がEnum定義・ファクトリメソッド存在するがScroll.Use()のswitch文に対応caseなし。使用時「何も起こらなかった」 | 中 | `Consumables.cs:274-319`, `ItemFactory.cs:801,750` | |
| CI-3 | AccessoryクラスのPassiveAbility/ActivatedSkill/SkillCooldownプロパティがCombatSystemから一切参照されない。アクセサリ効果が完全非機能 | 高 | `Equipment.cs:296-308`, `CombatSystem.cs` | |
| CI-4 | Equipment.Effectsリスト（ItemEffect）がOnEquip()で参照されるが実際の効果適用コードなし。LifeSteal/ManaRegen/ElementalDamage等が機能しない | 高 | `Equipment.cs:76,108-113`, `CombatSystem.cs` | |
| CI-5 | 識別の巻物（ScrollType.Identify）使用時に「アイテムを識別した！」メッセージが表示されるが実際のIsIdentifiedフラグ変更なし。ItemIdentificationSystem.Identify()が呼び出されない | 中 | `Consumables.cs:283-286`, `ItemIdentificationSystem.cs:36` | |
| CI-6 | 未識別アイテムがCanUse()でuser.IsAliveのみチェック。IsIdentified=falseでも使用可能で識別システムの意義が消失 | 低 | `Consumables.cs:9-26` | |
| CI-7 | ItemEffectType列挙の11種（HealHp/RestoreMp/RestoreSp/Damage/ApplyStatus/RemoveStatus/StatBuff/Teleport/RevealMap/Identify/LearnRuneWord）が戦闘計算で一切適用されない | 中 | `Item.cs:155-182`, `CombatSystem.cs` | |
| CI-8 | CookingSystem.CalculateQuality()がPlayer.Level*2をハードコードで使用。料理熟練度（CookingProficiency）フィールドがPlayerに存在しない | 中 | `GameController.cs:7215`, `CookingSystem.cs:19-26` | |
| CI-9 | ゴールドが重量を持たない。Inventory.TotalWeight計算がゴールドを除外。大量のゴールド所持によるペナルティなし | 低 | `Player.cs:115-142`, `Inventory.cs:20` | |

---

## カテゴリCJ: 行動コスト・自動探索・メッセージ詳細（8件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CJ-1 | GameAction.UseInn処理でactionCost変数が設定されない。宿屋利用がターンを消費せずに無限回復可能 | 高 | `GameController.cs:1117-1122` | |
| CJ-2 | GameAction.VisitChurch処理でactionCost変数が設定されない。教会訪問がターンを消費しない | 高 | `GameController.cs:1117-1122` | |
| CJ-3 | ShouldStopAutoExplore()がHP≤50%のみチェック。空腹/渇き閾値チェックなし。自動探索中に餓死/渇死の可能性 | 高 | `GameController.cs:6466-6486` | |
| CJ-4 | TryUseInn()のHP/MP/SP完全回復がメッセージに表示されない。疲労/衛生/渇きの回復メッセージのみ表示 | 中 | `GameController.cs:5324-5340` | |
| CJ-5 | メッセージ履歴が1000件上限でFIFO削除。重要度による優先保持なし。ボス警告等の重要メッセージが消失可能 | 中 | `GameController.cs:6679-6687` | |
| CJ-6 | GetBossRoom()がnull時にGetRandomFloorPosition()へフォールバック。ボスが廊下/階段横等の不適切な位置に配置される可能性 | 低 | `GameController.cs:765-785` | |
| CJ-7 | Resurrect呪文がPlayer専用でコンパニオンに適用不可。CompanionSystem.RemoveDeadCompanions()との連携なし | 低 | `GameController.cs:4310-4312,4682-4702` | |
| CJ-8 | EnterTerrainFieldMap()がOnSymbolMapEnterTown?.Invoke()を発火。フィールド戦闘マップで町BGM/UIが再生される | 中 | `GameController.cs:5062-5092` | |

---

## カテゴリCK: 百科事典・実績・識別・投資詳細（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CK-1 | _encyclopediaSystemがGameController初期化時にnew()で生成されるがSaveDataに含まれない。モンスター図鑑/発見レベルがセーブ/ロードで消失 | 中 | `GameController.cs:44`, `SaveData.cs` | |
| CK-2 | _achievementSystemの達成状態がSaveDataに含まれない。実績進捗とNextPlayBonusがセッション間で消失。メタ進行システムが機能しない | 中 | `GameController.cs:68`, `AchievementSystem.cs:81-98`, `SaveData.cs` | |
| CK-3 | SpellParser.cs:38-42がルーンワード組み合わせのバリデーションなし。同一効果ワード重複/矛盾ターゲット（sjalfr+fjandi）を許容 | 中 | `SpellParser.cs:38-42,104-134` | |
| CK-4 | ルーンワード習熟度が難易度と逆相関。gain=Max(1,10-Difficulty)で高難度ワードの習熟が遅い。学習インセンティブが逆転 | 低 | `SpellCastingSystem.cs:292-294`, `RuneWordDatabase.cs` | |
| CK-5 | InvestmentSystem.ExpectedReturnが計算可能だが実際のゴールド支払いメカニズムなし。IsCompletedフラグが設定されるコードなし。投資が一方通行 | 中 | `InvestmentSystem.cs:6-65` | |
| CK-6 | レシピ発見システムなし。CookingSystemの5レシピが全て初期状態でアクセス可能。料理の発見/学習ループが存在しない | 中 | `CookingSystem.cs:19-26` | |

### CL: Enum未使用値・重複定義

| ID | 不整合の内容 | 深刻度 | 参照コード | 修正判断 |
|----|------------|--------|-----------|---------|
| CL-1 | GameCommand列挙型（25値）が完全未使用デッドコード。Move/Attack/Skill/Magic/Item等25値全てがコードベースで一切参照されていない | 中 | `Enums.cs:160-187` | |
| CL-2 | ItemType列挙型がEnums.csとItem.csで重複定義。Enums.cs版（6値: Weapon/Armor/Accessory/KeyItem/Scroll/Book）は完全にシャドウされItem.cs版のみ使用 | 高 | `Enums.cs:192-202`, `Item.cs:9-21` | |
| CL-3 | CharacterClass列挙型がEnums.csとResourceSystem.csで重複定義。値が異なる（Fighter/Knight vs Warrior/Berserker）。Enums.cs版はシャドウ | 高 | `Enums.cs:286-308`, `ResourceSystem.cs:400-421` | |
| CL-4 | Item.csのItemType.Miscellaneousが定義されているが一切参照なし。アイテム分類として使用されていない | 低 | `Item.cs:9-21` | |
| CL-5 | SkillTarget.SingleAllyとSkillTarget.AllAlliesが定義済みだが使用されていない。味方対象スキルのターゲティングが機能しない | 高 | `Enums.cs:442-450` | |
| CL-6 | NpcType列挙型の7値（Bard/MagicShopkeeper/BlackMarketDealer/Trainer/Alchemist/Guardian等）が未使用。対応NPCが生成されない | 中 | `Enums.cs:455-479` | |

### CM: セーブデータ永続性（追加33システム未保存）

| ID | 不整合の内容 | 深刻度 | 参照コード | 修正判断 |
|----|------------|--------|-----------|---------|
| CM-1 | _ngPlusTier（NewGamePlusTier?）がセーブされない。ロード時にNG+ティア情報が消失し、NG+特有の敵強化/報酬が適用されなくなる | 致命的 | `GameController.cs:155` | |
| CM-2 | _hasCleared（bool）/_clearRank（string）がセーブされない。ゲームクリア状態とクリアランクがロード後に消失 | 致命的 | `GameController.cs:158,161` | |
| CM-3 | _infiniteDungeonMode（bool）/_infiniteDungeonKills（int）がセーブされない。無限ダンジョンの進行状況がロード後にリセット | 致命的 | `GameController.cs:164,167` | |
| CM-4 | _surfaceMap（Dictionary<Position, SurfaceType>）がセーブされない。環境戦闘システムの地形効果（濡れた床/燃焼等）がロード後に消失 | 高 | `GameController.cs:84` | |
| CM-5 | _chantRemainingTurns/_pendingSpellResultがセーブされない。詠唱中の呪文がロード時に中断・消失 | 高 | `GameController.cs:178,181` | |
| CM-6 | _isInLocationMap/_isLocationField/_symbolMapReturnPosition等ナビゲーション状態がセーブされない。町/建物内でセーブ→ロードすると位置コンテキスト消失 | 高 | `GameController.cs:135,138,141` | |
| CM-7 | _buildingReturnMap/_buildingReturnPosition/_currentBuildingId/_visitedBuildingsがセーブされない。建物内でセーブ→ロードすると帰還先情報が消失 | 高 | `GameController.cs:144-149` | |
| CM-8 | _playerFacing（Direction）がセーブされない。プレイヤーの向きがロード後にDirection.Southにリセット | 低 | `GameController.cs:75` | |

### CN: スキルBasePower=0問題

| ID | 不整合の内容 | 深刻度 | 参照コード | 修正判断 |
|----|------------|--------|-----------|---------|
| CN-1 | 挑発（provoke）のBasePower=0。「敵の注意を引き付ける」効果が数値的にゼロで機能しない | 高 | `SkillSystem.cs:97` | |
| CN-2 | 瞑想（meditation）のBasePower=0。「HP・MP・SP回復」と説明があるが回復量ゼロ | 高 | `SkillSystem.cs:114` | |
| CN-3 | 魔法障壁（arcane_shield）のBasePower=0。「魔法防御力上昇」バフだが上昇量ゼロ | 高 | `SkillSystem.cs:124` | |
| CN-4 | 浄化（purify）/祝福（blessing）のBasePower=0。状態異常解除力/全ステ微増がゼロ | 高 | `SkillSystem.cs:128,130` | |
| CN-5 | 死霊召喚（summon_undead）/呪詛（curse）のBasePower=0。召喚パワーとデバフ効果がゼロ | 高 | `SkillSystem.cs:133,135` | |
| CN-6 | 鼓舞の歌（inspire_song）/子守唄（lullaby）/魅了の旋律（charm）のBasePower=0。バフ/睡眠/魅了効果が全てゼロ | 高 | `SkillSystem.cs:141,143,144` | |
| CN-7 | 付与（enchant）/変成（transmute）のBasePower=0。属性付与と素材変換のパワーがゼロで実質効果なし | 中 | `SkillSystem.cs:149,150` | |
| CN-8 | 解毒剤（potion_antidote）にEffectValueが未定義。解毒効果の数値が設定されていないため治癒が機能しない可能性 | 高 | `ItemFactory.cs:476` | |

### CO: マップ生成・階段配置致命的欠陥

| ID | 不整合の内容 | 深刻度 | 参照コード | 修正判断 |
|----|------------|--------|-----------|---------|
| CO-1 | PlaceStairs()で階段配置が失敗してもvoid返り値のためGenerate()が検出不可。階段なしマップを返す可能性 | 致命的 | `DungeonGenerator.cs:51,365-409` | |
| CO-2 | GetRandomWalkablePositionInRoom()が50回試行で失敗→null返却。上り/下り階段が配置されずプレイヤーが階移動不能になる | 致命的 | `DungeonGenerator.cs:373-391` | |
| CO-3 | TryAscendStairsで前フロアのStairsDownPositionがnullの場合、プレイヤー位置が設定されない。壁の中にスポーンする可能性 | 致命的 | `GameController.cs:2726-2741` | |
| CO-4 | 宝物部屋がnormalRooms.Count>2の場合のみ配置。小規模ダンジョン（3部屋以下）では宝物部屋が生成されない | 中 | `DungeonGenerator.cs:468-474` | |
| CO-5 | HasLineOfSight()にwhile(true)ループの最大反復制限なし。マップ状態破損時に無限ループのリスク | 中 | `DungeonMap.cs:187-200` | |

### CP: 戦闘計算致命的不整合

| ID | 不整合の内容 | 深刻度 | 参照コード | 修正判断 |
|----|------------|--------|-----------|---------|
| CP-1 | XP三重付与バグ。Player.GainExperience(totalExp) + OnEnemyDefeated内のreligionBonus + NG+bonusで1体の敵から3回XP獲得。レベリングが設計の3倍速 | 致命的 | `GameController.cs:1544,1653-1676` | |
| CP-2 | CheckCritical(CriticalCheckParams)メソッド（DEX/LUK考慮版）が完全デッドコード。実際にはCheckCritical(double)のみ使用され、DEX/LUKがクリティカル率に影響しない | 致命的 | `DamageCalculator.cs:168-182` | |
| CP-3 | maxHpが0の場合にGetHpStatePenalty()で除算ゼロクラッシュ。キャラ生成バグやセーブ破損時に発生 | 致命的 | `DamageCalculator.cs:202`, `ResourceSystem.cs:54` | |
| CP-4 | AttackTypeが6種（Slash/Pierce/Blunt/Unarmed/Ranged/Magic）だがMagic以外は全て同一物理計算。武器タイプ別の差別化が存在しない | 高 | `DamageCalculator.cs:127-138` | |
| CP-5 | CombatSystem.ExecuteAttack()がSP/MPコスト未消費で攻撃実行。リソース消費なしで無限攻撃可能 | 高 | `CombatSystem.cs:37-65` | |
| CP-6 | 魔法攻撃でspellElement引数が渡されるがダメージ計算に未使用。属性弱点が魔法ダメージに反映されない | 高 | `CombatSystem.cs:215-270` | |
| CP-7 | TryApplyStatusEffect()がtrue/false判定のみ返し、実際にキャラクターへの状態異常適用を行わない。戦闘中の状態異常が一切機能しない | 致命的 | `CombatSystem.cs:307-370` | |

### CQ: クエスト報酬・宗教スキル欠落

| ID | 不整合の内容 | 深刻度 | 参照コード | 修正判断 |
|----|------------|--------|-----------|---------|
| CQ-1 | クエスト報酬のItemIds（アイテム報酬）が配布されない。TurnInQuestでGoldとExperienceのみ処理しItemIds/GuildPoints/FaithPointsを無視 | 致命的 | `NpcSystem.cs:434-456` | |
| CQ-2 | 宗教スキル23個中21個が未実装。LightTemple(divine_protection/divine_miracle)、DarkCult(3個)、NatureWorship(4個全て)、DeathFaith(4個全て)、ChaosCult(4個全て)がSkillSystem未定義 | 致命的 | `ReligionSystem.cs:238-345`, `SkillSystem.cs` | |
| CQ-3 | ReligionBenefit（HealingBonus/DamageBonus/DefenseBonus等）のApplyBenefit()メソッドが見当たらない。宗教特典が定義のみで適用されない | 高 | `ReligionSystem.cs:223-345` | |
| CQ-4 | 宗教タブー（ReligionTaboo）のCheckViolation()が未実装。プレイヤーがタブーを破っても一切のペナルティなし | 高 | `ReligionSystem.cs:232-343` | |
| CQ-5 | 棄教呪い（Apostasy Curse）のDurationDaysが設定されるがデクリメントするコードなし。一時呪いが永続化 | 高 | `ReligionSystem.cs:395-414,529-530` | |
| CQ-6 | DialogueSystem.CurrentNodeがセーブ/ロード対応していない。対話途中でセーブ→ロードすると対話進行状態消失 | 中 | `NpcSystem.cs:201-313` | |

### CR: ランダムイベント・ワールドマップ欠落

| ID | 不整合の内容 | 深刻度 | 参照コード | 修正判断 |
|----|------------|--------|-----------|---------|
| CR-1 | NpcEncounterイベントの解決ハンドラ（ResolveNpcEncounter）が未実装。NPC遭遇イベント発生時にクラッシュまたは無動作 | 致命的 | `WorldMapSystem.cs:729` | |
| CR-2 | MerchantEncounterイベントの解決ハンドラ（ResolveMerchantEncounter）が未実装。商人遭遇イベント発生時に無動作 | 致命的 | `WorldMapSystem.cs:730` | |
| CR-3 | AmbushEventの解決ハンドラ（ResolveAmbushEvent）が未実装。待ち伏せイベント発生時に無動作 | 致命的 | `WorldMapSystem.cs:733` | |
| CR-4 | 人型種族以外の敵がゴールドをドロップしない。CalculateGoldReward()がRace!=Humanoidで即return 0。ドラゴン/アンデッド/ビースト等は報酬ゼロ | 高 | `GameController.cs:1558-1567` | |

### CS: 誓約・タブー・実効果未実装

| ID | 不整合の内容 | 深刻度 | 参照コード | 修正判断 |
|----|------------|--------|-----------|---------|
| CS-1 | OathSystem.IsViolation()が存在するが戦闘/アイテム使用/コンパニオン募集システムから自動呼出されない。誓約違反が検出されない | 高 | `OathSystem.cs:63-71` | |
| CS-2 | 誓約完了条件（CompleteOath）メソッドが存在しない。TakeOath/BreakOathのみで、誓約を「達成」する方法がない | 高 | `OathSystem.cs` | |
| CS-3 | SP上限（MaxStamina=100）がRestoreSp()等で強制されない。呼出元がMath.Min制約を忘れるとSP>100になる | 中 | `ResourceSystem.cs:135` | |
| CS-4 | GetStaminaCost()にCharacterClass引数がない。全職業で同一SP消費。戦士のDashとシーフのDashが同コスト | 中 | `ResourceSystem.cs:140-162` | |

### CT: 空腹ダメージ・攻撃タイプバランス

| ID | 不整合の内容 | 深刻度 | 参照コード | 修正判断 |
|----|------------|--------|-----------|---------|
| CT-1 | HungerState.StarvingがDamagePerTurn=0でHP回復無効のみ。飢餓状態でダメージがないのは設計意図と矛盾する可能性 | 中 | `ResourceSystem.cs:226` | |
| CT-2 | HungerState.StarvationがDamagePerTurn=999で即死級ダメージ。段階的な餓死ではなく突然死 | 中 | `ResourceSystem.cs:228` | |
| CT-3 | XP計算にオーバーフロー/負数チェックなし。float→intキャストでNaN/Infinityの場合クラッシュ。NG+倍率が上限なし | 高 | `GameController.cs:1544,1653-1676` | |
| CT-4 | レベルアップ時のHP/MP最大値更新・EffectiveStatsキャッシュ再計算が明示的に行われない。サイレントレベルアップで即時ステータス反映なし | 高 | `GameController.cs` | |

### CU: アイテム生成・Create()null返却

| ID | 不整合の内容 | 深刻度 | 参照コード | 修正判断 |
|----|------------|--------|-----------|---------|
| CU-1 | ItemFactory.Create()がitemId未定義の場合にnull返却。呼出元がnullチェックを怠るとNullReferenceExceptionでクラッシュ | 高 | `ItemFactory.cs:1267` | |
| CU-2 | GenerateDungeonFloorItem()/GenerateEnemyDropItem()がアイテム生成失敗時にCreateStone()にフォールバック。レアドロップ枠で石が出る不具合 | 中 | `ItemFactory.cs:1053,1103` | |
| CU-3 | NpcMemorySystemがReset()で全記憶消去。転生（Rebirth）後にNPC関係性が完全リセットされるが、これが設計通りか不明確 | 低 | `NpcMemorySystem.cs:64-68` | |

---

## カテゴリCV: 呪文ダメージタイプ・回復計算不整合（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CV-1 | SpellCastingSystemのダメージタイプ決定で三項演算子が`element == Element.None ? DamageType.Magical : DamageType.Magical`。両分岐が同じ値でエレメント属性がダメージ計算に全く反映されない | 致命的 | `SpellCastingSystem.cs:369` | |
| CV-2 | 回復呪文（graeda/blessa）がダメージ呪文と同じ`basePower = effectWord.BaseMpCost * 3`計算を使用。SpellEffectType.Healの分岐がなく回復量が不適切 | 致命的 | `SpellCastingSystem.cs:361-363` | |
| CV-3 | 呪文のSpellEffectType（Damage/Heal/Buff/Debuff/Summon/Teleport）のうち、Summon/Teleportの実効果が空。キャストしてもログ出力のみ | 高 | `SpellCastingSystem.cs` | |
| CV-4 | 呪文詠唱成功率(CastResult.PowerMultiplier)が0.0～2.0の範囲だが、0.0の場合でもキャスト成功として処理。威力ゼロの呪文が「成功」として発動 | 中 | `SpellCastingSystem.cs` | |
| CV-5 | SpellCastingSystemにMP消費チェックが不十分。CastSpell()内でMP不足判定後もキャスト処理が続行する経路がある | 高 | `SpellCastingSystem.cs` | |
| CV-6 | Element属性（Fire/Ice/Lightning/Earth/Wind/Water/Light/Dark）のうちEarth/Wind/Waterの属性相性テーブルがSpellCastingSystemに未定義 | 中 | `SpellCastingSystem.cs` | |

---

## カテゴリCW: コンパニオンSupportMode・ペット被ダメージ不在（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CW-1 | CompanionSystem.ProcessFollowMode()がテキスト返却のみの空実装。Supportモードのコンパニオンが戦闘補助（回復/バフ/援護攻撃）を一切行わない | 高 | `CompanionSystem.cs:143-146` | |
| CW-2 | PetSystemにダメージ適用メソッドが完全不在。ペットがHP減少/死亡/戦闘離脱する手段がなく不死身状態 | 致命的 | `PetSystem.cs:1-138` | |
| CW-3 | CompanionData.Hpが減少してもHP<=0時の死亡処理がない。コンパニオンもペット同様に不死身 | 致命的 | `CompanionSystem.cs` | |
| CW-4 | コンパニオンのスキル使用メソッドが定義されているがProcessAttackMode()内でのみ使用。SupportModeではスキルが一切発動しない | 高 | `CompanionSystem.cs` | |
| CW-5 | ペットの忠誠度(Loyalty)が0になってもデシリアライズ(CheckDesertion)が未呼出のため離反が発生しない | 中 | `PetSystem.cs` | |

---

## カテゴリCX: ギルドランク報酬・派閥評判追跡不在（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CX-1 | MerchantGuildSystem.CheckRankUp()がランク上昇を検出するが報酬（装備/ゴールド/パッシブボーナス）の配布がない | 高 | `MerchantGuildSystem.cs:86-90` | |
| CX-2 | DungeonFactionSystemが完全な静的読み取り専用。プレイヤーの派閥評判値を追跡するフィールド/メソッドがない | 高 | `DungeonFactionSystem.cs:1-65` | |
| CX-3 | GuildSystem.GetAvailableQuests()がギルドランクに応じたクエストフィルタリングを行わない。全ランクで同じクエストが表示 | 中 | `GuildSystem.cs` | |
| CX-4 | MerchantGuild.TotalProfitが集計されるが特典解放条件として使用されない。利益目標達成によるボーナスなし | 中 | `MerchantGuildSystem.cs` | |
| CX-5 | 派閥間の関係値(FactionRelation)が敵対/中立/友好の3段階だが、プレイヤーの行動で変化するメカニズムなし | 高 | `DungeonFactionSystem.cs` | |
| CX-6 | GuildPoints取得がExecuteTrade()のみ。クエスト完了/ダンジョン探索でのGuildPoints付与なし | 中 | `MerchantGuildSystem.cs` | |

---

## カテゴリCY: クラフト素材消費・強化曲線不整合（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CY-1 | CraftingSystem.Craft()内のinventory.RemoveItem()戻り値（bool）を無視。素材除去に失敗してもクラフトが成功する | 致命的 | `CraftingSystem.cs:125-128` | |
| CY-2 | 装備強化(EnhanceEquipment)の成功率がLv5:55%→Lv6:40%→Lv7:25%→Lv8:15%→Lv9:10%→Lv10:5%と極端に低下。+6以降が実質強化不能 | 中 | `CraftingSystem.cs:151-167` | |
| CY-3 | 強化失敗時のペナルティが定義されていない。失敗しても素材のみ消費で装備レベル維持。強化失敗のリスクが低すぎる | 低 | `CraftingSystem.cs:174-190` | |
| CY-4 | CanCraft()チェック通過後にCraft()実行するが、並行アクションでインベントリが変化した場合の再検証がない | 高 | `CraftingSystem.cs:100-130` | |
| CY-5 | EnhancementLevelの上限チェック(>=10)がEnhanceEquipment()内のみ。ItemFactory経由で生成される装備にEnhancementLevel初期値の保証なし | 中 | `CraftingSystem.cs:174-175` | |

---

## カテゴリCZ: ショップ在庫・建設効果・領地価格不整合（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| CZ-1 | ClearShopInventory()がテリトリー変更時に呼ばれるが、直後にInitializeShop()が呼ばれない。新テリトリーのショップが空のまま | 高 | `WorldMapSystem.cs:476-479`, `GameController.cs:4969` | |
| CZ-2 | GetTerritoryPriceMultiplier()の領地別価格倍率（山岳1.2x/南部1.3x/辺境1.5x）がBuy()/Sell()の価格計算に未適用 | 高 | `WorldMapSystem.cs:516-525` | |
| CZ-3 | BaseConstructionSystem.GetDailyFoodProduction()が値を返すがGameControllerから一度も呼ばれない。建設した農場/畑の食料生産が無効 | 高 | `BaseConstructionSystem.cs:120` | |
| CZ-4 | GetRestHpRecoveryMultiplier()の建物ボーナスがTownSystem.RestAtInn()に未適用。宿屋は常にplayer.Heal(player.MaxHp)固定 | 高 | `BaseConstructionSystem.cs:102`, `TownSystem.cs:379` | |
| CZ-5 | ReputationSystemのGetShopDiscount()がBuy()メソッド内の価格計算に未接続。評判による値引きが機能しない | 中 | `ReputationSystem.cs:53-63` | |
| CZ-6 | ショップ在庫にランダム性がない。同じテリトリーで常に同じアイテムリストが表示される | 中 | `WorldMapSystem.cs` | |

---

## カテゴリDA: ターン処理・時間経過スキップ（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DA-1 | ProcessTurnEffects()（空腹/渇き/疲労/HP回復/状態異常ティック等）がスキル使用/宿屋利用/教会訪問等の多数のアクションで呼ばれない。ターン経過系効果がスキップされる | 致命的 | `GameController.cs:2050-2300` | |
| DA-2 | StatusEffectsのTickが ProcessTurnEffects()内のみで実行。ProcessTurnEffects()がスキップされるとバフ/デバフの残りターン数が減少せず永続化する | 致命的 | `GameController.cs:2184` | |
| DA-3 | KarmaSystem.SetCurrentTurn()の呼び出しがGameController内に不在。カルマのターン連動機能（時間減衰等）が動作しない | 高 | `KarmaSystem.cs`, `GameController.cs` | |
| DA-4 | ReputationSystemにターン経過による評判減衰メソッドが存在しない。一度上げた評判が永久に維持される | 高 | `ReputationSystem.cs:31-41` | |
| DA-5 | ペットの空腹ティック(PetSystem.TickHunger)がProcessTurnEffects()内に含まれていない。ペットの空腹が進行しない | 高 | `PetSystem.cs`, `GameController.cs` | |
| DA-6 | 日変更検出(DayChanged)がGameTimeシステムに未実装。デイリーリセット（ショップ在庫/宗教祈り/イベント制限）が発動しない | 致命的 | `GameController.cs` | |

---

## カテゴリDB: カルマ閾値・評判効果デッドコード（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DB-1 | KarmaSystem.GetShopPriceModifier()がカルマランクに応じた価格修飾子を返すが、ShopSystem/TownSystemのBuy()/Sell()から呼ばれない | 高 | `KarmaSystem.cs:60-70` | |
| DB-2 | KarmaSystem.GetNpcDispositionModifier()がNPC態度修飾子を返すが、NpcSystem/DialogueSystemから参照されない | 高 | `KarmaSystem.cs:72-83` | |
| DB-3 | カルマ閾値（Saint≥80/Criminal≤-49等）を跨いだ際のイベント通知がない。プレイヤーがカルマ変動に気づけない | 中 | `KarmaSystem.cs:37-46` | |
| DB-4 | ModifyKarma()のトリガーがGameController内にわずか3箇所（処刑-5/密輸-5/闇市場-2）のみ。NPC殺害/窃盗/善行/寄付等の主要アクションにカルマ変動がない | 高 | `GameController.cs:1522,7646,7709` | |
| DB-5 | ReputationSystem.IsWelcome()がテリトリー入場可否を返すが、WorldMapSystem.TravelTo()で未チェック。評判最低でもどの領地にも入れる | 中 | `ReputationSystem.cs`, `WorldMapSystem.cs` | |

---

## カテゴリDC: 死亡ログ未記録・ゲームオーバーフロー不備（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DC-1 | DeathLogSystem.AddLog()がHandlePlayerDeath()フロー内で一度も呼ばれない。死亡ログが永久に空のまま | 致命的 | `GameController.cs:3011-3076`, `DeathLogSystem.cs` | |
| DC-2 | GameOverSystem.GameOverChoiceにRebirthオプションが定義されているが、ゲームオーバーUIにリスタート/転生選択肢が表示されない | 高 | `GameOverSystem.cs:9-16`, `MainWindow.xaml.cs:631-643` | |
| DC-3 | パーマデス（Ironmanモード）でのゲームオーバー時にDeleteSaveAsync()が呼ばれない。セーブファイルが残存し復帰可能 | 致命的 | `GameController.cs:3063-3076`, `Interfaces.cs:159` | |
| DC-4 | HandlePlayerDeath()のdeath cause判定で、状態異常死（毒/出血/飢餓）の死因テキストが汎用「力尽きた」のみ。詳細な死因記録がない | 中 | `GameController.cs:3028-3038` | |
| DC-5 | ExecuteRebirth()がSanity>0の場合のみ実行可能だが、Sanity<=0時の処理分岐（完全ゲームオーバー）にセーブ削除やスコア記録がない | 高 | `GameController.cs:3085-3160` | |
| DC-6 | 死亡統計（死亡回数/最頻死因/最長生存ターン）の集計・表示機能が不在。DeathLogSystemがデータを持つが閲覧UIなし | 中 | `DeathLogSystem.cs` | |

---

## カテゴリDD: 自動探索・パス計算不完全（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DD-1 | AutoExploreSystemがCheckStopConditions()で停止条件を検出できるが、FindPath()/GetNextStep()等の経路計算メソッドが存在しない | 致命的 | `AutoExploreSystem.cs:31-49` | |
| DD-2 | 自動探索停止条件にHunger/Thirst/Fatigueの危険閾値チェックがない。体力危険状態でも探索を続行する | 高 | `AutoExploreSystem.cs` | |
| DD-3 | DungeonGenerator内のBFS経路探索(Lines 412-456)にイテレーション上限がない。理論上は有界だが、大マップでのパフォーマンス保証がない | 中 | `DungeonGenerator.cs:412-456` | |
| DD-4 | _autoExploring/_autoExploreTargetStairsUpの状態がSaveDataに未保存。セーブ→ロード時に自動探索が中断される | 中 | `GameController.cs:102-105` | |
| DD-5 | 自動探索中にボスフロアに到達した場合の強制停止条件がない。ボス戦に意図せず突入するリスク | 高 | `AutoExploreSystem.cs`, `GameController.cs` | |

---

## カテゴリDE: タイル表示文字・ボス部屋生成エッジケース（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DE-1 | Tile.GetDisplayChar()のswitchにTileType.NpcTrainerが欠落。トレーナーNPCのマップ表示が'?'になる | 高 | `Tile.cs:476-525` | |
| DE-2 | Tile.GetDisplayChar()のswitchにTileType.NpcLibrarianが欠落。図書館NPCのマップ表示が'?'になる | 高 | `Tile.cs:476-525` | |
| DE-3 | DungeonGenerator.GenerateDungeon()でrooms.Count==1の場合、ボス部屋タイプが未設定。ボスフロアで単一部屋生成時にボス未配置 | 致命的 | `DungeonGenerator.cs:127-135` | |
| DE-4 | ボス部屋配置が最遠部屋選択のみ。部屋サイズ（小部屋にボス）や接続数（袋小路）の検証がなく不適切な部屋がボス部屋になる | 中 | `DungeonGenerator.cs` | |
| DE-5 | マップ内のWater/Lavaタイルが定義(TileType enum)されているがDungeonGeneratorの通常フロア生成で配置されない。テーマダンジョン以外で環境タイルが不在 | 中 | `DungeonGenerator.cs`, `Tile.cs` | |

---

## カテゴリDF: マルチスロットセーブ・ゲームクリアスコア不整合（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DF-1 | MultiSlotSaveSystem.GetOldestSlot()でSaveTimeがnullableのDateTime?型だが、.OrderBy(s => s.SaveTime)でnull比較が未定義動作。SaveTimeがnullのスロットが混在すると並べ替え結果が不確定 | 高 | `MultiSlotSaveSystem.cs:37` | |
| DF-2 | GetOldestSlot()で全スロットが空の場合にIsEmpty==trueでフィルタ後のコレクションが空になり、SlotNumber取得がnull参照例外のリスク。フォールバックのスロット1返却ロジックが不安定 | 高 | `MultiSlotSaveSystem.cs:64-67` | |
| DF-3 | GameClearSystem.CalculateScore()のターンボーナス計算で`totalTurns / 5`が整数除算。totalTurns=4999と5001でスコアが逆転する段差が発生し、ターンを多く消費した方が高スコアになるケースがある | 致命的 | `GameClearSystem.cs:70` | |
| DF-4 | GameClearSystem.DetermineRank()のタプルパターンマッチ順序バグ。ターン数4999/死亡5回のプレイヤーがB評価になるが、ターン数5001/死亡0回のプレイヤーがA評価。少ないターン数が低評価につながる逆転現象 | 致命的 | `GameClearSystem.cs:45-51` | |
| DF-5 | NewGamePlusSystem.DetermineInitialTier()がclearRank引数に関わらず常にPlus1を返却。DetermineInitialTier()メソッド全体がデッドロジック | 中 | `NewGamePlusSystem.cs:94-100` | |
| DF-6 | NewGamePlusSystem.GetCarryOverItems()のティア別キャリーオーバーが累積的でない。Plus5プレイヤーがPlus2/Plus3/Plus4の持ち越しアイテムをチェックしない可能性 | 中 | `NewGamePlusSystem.cs:52-55` | |

---

## カテゴリDG: ゲームオーバー・エンディング分岐不整合（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DG-1 | MultiEndingSystem.DetermineEnding()でDarkエンディング(karma<=-50)がTrueエンディング(clearRank S/A)より先にチェックされる。高ランククリア+低カルマプレイヤーが常にDarkエンディングになりTrueエンディング不可 | 高 | `MultiEndingSystem.cs:24,44` | |
| DG-2 | Salvationエンディングの条件がtotalDeaths==0。ローグライクゲームで死亡0回は極端に制限的であり実質到達不能。条件は`totalDeaths <= 3`等が適切 | 中 | `MultiEndingSystem.cs:34` | |
| DG-3 | Wandererエンディング(!hasClearedFinalBoss && allTerritoriesVisited)が、Dark/Salvation/Trueの条件と重なりDarkエンディングに吸収される。karma<=-50かつ全テリトリー訪問済みプレイヤーはWandererにならない | 中 | `MultiEndingSystem.cs:54-61` | |
| DG-4 | GameOverSystem.CanRebirth()がsanity>0のみチェック。転生のSanityコスト消費処理が未実装で、転生し放題の可能性 | 高 | `GameOverSystem.cs:29-31` | |
| DG-5 | GameOverSystem.GetAvailableChoices()とProcessChoice()でCanRebirth判定が二重実行。選択肢表示後にSanityが変化した場合に不整合が発生する可能性 | 中 | `GameOverSystem.cs:39,64-67` | |
| DG-6 | InfiniteDungeonSystem.GetFloorConfig()の敵レベル計算`10 + floor * 2`とドロップ率`1.0f + floor * 0.05f`と経験値`1.0f + floor * 0.03f`に上限キャップなし。floor 1000で敵レベル2010、ドロップ51倍、経験値31倍となり経済破綻 | 中 | `InfiniteDungeonSystem.cs:28,36-37,56` | |

---

## カテゴリDH: 描画最適化・ビューポート計算エラー（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DH-1 | RenderOptimizationSystem.CalculateViewport()のビューポート計算で`playerX + halfW`がMaxXに設定されるが、奇数幅ビューポートで最右列が切断される。halfW = viewportWidth/2（整数除算）で1ピクセル分不足 | 高 | `RenderOptimizationSystem.cs:14` | |
| DH-2 | 同様にMaxY計算でも奇数高さビューポートで最下行が切断される | 高 | `RenderOptimizationSystem.cs:14` | |
| DH-3 | IsInViewport()の境界チェックが`<=`演算子を使用。MaxX/MaxYの位置が隣接ビューポートと重複し、ビューポート境界上のエンティティが二重描画される可能性 | 高 | `RenderOptimizationSystem.cs:20` | |
| DH-4 | ShouldUpdate()が`updateFrequency <= 0`の場合trueを返却。負の更新頻度は論理的に無意味であり、falseを返却するか例外をスローすべき | 中 | `RenderOptimizationSystem.cs:45` | |
| DH-5 | ModularHudSystem.SetElementScale()でスケール値が`[0.5f, 2.0f]`にサイレントクランプ。アクセシビリティ用途で2.5倍以上のスケールが必要な場合に無通知で制限される | 中 | `ModularHudSystem.cs:51-54` | |

---

## カテゴリDI: 百科事典・MOD検証・ヘルプシステム不整合（6件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DI-1 | EncyclopediaSystem.RegisterMonsterEntry()で新規モンスターのDiscoveryLevelが1（名前・種族可視）で登録される。未遭遇モンスターの名前が初回から判明してしまう。初期値は0であるべき | 高 | `EncyclopediaSystem.cs:58` | |
| DI-2 | CalculateMonsterDiscoveryLevel(killCount=0)が0を返却するが、RegisterMonsterEntry()でDiscoveryLevel=1設定済み。レジストリと計算結果が矛盾し、初回遭遇で発見レベルが退化する | 高 | `EncyclopediaSystem.cs:82-86` | |
| DI-3 | ModLoaderSystem.ParseManifest()がIsValid=trueを常に返却。MOD IDが空文字/nullでもバリデーション通過。不正なMODがロードされるリスク | 高 | `ModLoaderSystem.cs:56` | |
| DI-4 | ModLoaderSystem.ParseManifest()のパラメータ(modId, name, author等)にnull/空白チェックなし。フィールド未定義のマニフェストから不正なMODオブジェクトが生成される | 中 | `ModLoaderSystem.cs:52-58` | |
| DI-5 | ContextHelpSystem.GetContextualHelp()がTake(5)でハードコード。コンテキストに関連するヘルプが6件以上あっても5件しか表示されず、残りが切り捨てられる通知なし | 低 | `ContextHelpSystem.cs:106` | |
| DI-6 | ContextHelpSystem.CompleteTutorialStep()でstep完了後にcurrentStep++を実行。最終ステップ完了時にcurrentStepがsteps.Countと等しくなり、GetCurrentTutorial()が永続的にnullを返却。チュートリアル再開不可 | 中 | `ContextHelpSystem.cs:86-92` | |

---

## カテゴリDJ: アクセシビリティ・色覚補正・ゲーム速度計算（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DJ-1 | AccessibilitySystem.CalculateEffectiveFontSize()がフロート→int変換で切り捨て。FontSize 11 × 1.5 = 16.5 → 16に切り捨て。Math.Round()を使用すべき | 中 | `AccessibilitySystem.cs:113` | |
| DJ-2 | AccessibilitySystem.TransformForColorBlindness()がRed/Green/Blue/Yellowの4色のみ対応。Orange/Brown/Cyan/Pink/Gray等の色が未変換で色覚障害者に視認困難 | 中 | `AccessibilitySystem.cs:142-161` | |
| DJ-3 | Protanopia(赤色覚異常)の色変換でRed→DarkYellowに変換するが、医学的に赤色覚異常者にはDarkYellowも視認困難な場合がある。Tritanopia(青色覚異常)ではBlue→Cyanに変換するが同系色のため変換効果が不十分 | 低 | `AccessibilitySystem.cs:142-163` | |
| DJ-4 | DifficultySettings.Nightmare: rescueCount=1とPermaDeath=falseが矛盾。PermaDeath=falseなら転生可能であり、rescueCountの意味が不明確。Nightmareは実質Hard+αになってしまう | 高 | `DifficultySettings.cs:150` | |
| DJ-5 | DifficultySettings.Nightmare: hungerDecayMultiplier=1.5かつturnLimitMultiplier=0.6の組み合わせ。ターン制限315,360内に1.5倍速空腹でプレイヤーが餓死し、turnLimitが実質無意味なパラメータになる | 中 | `DifficultySettings.cs:148-149` | |

---

## カテゴリDK: 成長システム・レベル1ステータス欠如（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DK-1 | GrowthSystem.GetLevelBonus()が`level - 1`を使用。レベル1キャラクターの成長ボーナスが全ステータスで0になり、レベル1→2のレベルアップまでベースステータスのみで戦闘することになる | 高 | `GrowthSystem.cs:20-40` | |
| DK-2 | GrowthSystem.GetHpBonus()/GetMpBonus()が`(int)(HpPerLevel * (level - 1))`を計算。レベル1でHP/MPボーナスが0。レベル1キャラクターのHP/MPが定義値そのままで成長システムが機能しない | 高 | `GrowthSystem.cs:37-40` | |
| DK-3 | CalculateTotalExpForLevel()で`raceExpMultiplier`で除算。倍率>1.0の種族は必要経験値が減少し、倍率<1.0の種族は必要経験値が増加。通常のRPG設計（高倍率=レベルアップ容易）と逆の挙動 | 中 | `GrowthSystem.cs:161-168` | |
| DK-4 | HP/MP成長計算で`(int)`キャストによる切り捨てが蓄積。レベル30到達時に最大3-5ポイントのHP/MPが失われる誤差が発生 | 低 | `GrowthSystem.cs:177,191` | |
| DK-5 | CalculateLevelUpBonus()とGetHpBonus()で異なる丸め方式（前者はRollGrowth()でランダム、後者は固定値×切り捨て）。同じレベルのキャラクターでもHP/MPが計算方法により異なる値になる | 中 | `GrowthSystem.cs` | |

---

## カテゴリDL: フラグ条件解析・イベント確率不整合（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DL-1 | FlagConditionSystem.ParseCondition()でkarma条件パース時に文字列位置オフセットが不正。"karma >= 50"をパースする際にキーワード"karma"をスキップせずにsplitするため、演算子と値の抽出が失敗しカルマ基準の条件判定が常に不成立 | 致命的 | `FlagConditionSystem.cs:71-76` | |
| DL-2 | FlagConditionSystem.ParseCondition()がAND/OR混在条件を非対応。"A AND B OR C"のような複合条件でパーサーが最初のANDのみを処理しORを無視 | 中 | `FlagConditionSystem.cs:88-90` | |
| DL-3 | SymbolMapEventSystem.RollEvent()の累積確率計算で全イベントのBaseChance合計が0.69（1.0未満）。残り31%の確率でイベントが何も発生しない。意図的な設計かバグか不明 | 中 | `SymbolMapEventSystem.cs:18-40,55-66` | |
| DL-4 | SymbolMapEventSystem.RollEvent()でrandomValueの範囲検証なし。randomValueが1.0以上の場合、どのイベントも選択されないサイレント失敗 | 中 | `SymbolMapEventSystem.cs:55-66` | |
| DL-5 | BackgroundClearSystem.IncrementFlag()でamount引数に負値が渡せてしまい、フラグ値がデクリメントされる。boss_killsなどのカウンターが不正に減少する可能性 | 中 | `BackgroundClearSystem.cs:31` | |

---

## カテゴリDM: 難易度パラメータ矛盾・Ironman設計不備（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DM-1 | DifficultySettings: Ironman難易度のdamageTakenMultiplier=1.2がNightmare(1.6)より低い。Ironman（永久死亡）がNightmare（復活1回）より被ダメージが少なく、難易度の階層が逆転 | 高 | `DifficultySettings.cs:159-162` | |
| DM-2 | DifficultySettings: NightmareのexpMultiplier=0.6が極端に低い。レベルキャップに到達できずダメージ乗数(1.6倍被弾)と組み合わせると実質クリア不可能な難易度になる | 中 | `DifficultySettings.cs` | |
| DM-3 | DifficultySettings: 難易度enum拡張時にdefault => Normalへのサイレントフォールバック。新難易度追加時にIronmanプレイヤーがNormal設定でプレイしてしまう致命的バグのリスク | 中 | `DifficultySettings.cs:86` | |
| DM-4 | BackgroundClearSystem.GetFlagValue()が未設定フラグに対し0を返却。0が「未設定」なのか「値が0」なのか区別不能。"has_boss_defeated"フラグが初期値0と未設定で同一扱い | 中 | `BackgroundClearSystem.cs:37` | |
| DM-5 | BackgroundClearSystem.CheckCondition()の`_ => false`デフォルトケースが、BackgroundBonusData側の新条件フラグ追加時にサイレント失敗を引き起こす。条件が常にfalseで解除不能な実績が発生する | 中 | `BackgroundClearSystem.cs:61` | |

---

## カテゴリDN: 開始マップ解決・テンプレートマップ（4件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DN-1 | StartingMapResolver: 新規Background enum値追加時にサイレントフォールバックで種族別マップ選択になる。新職業追加時に意図しない開始マップが設定される | 中 | `StartingMapResolver.cs:15-20` | |
| DN-2 | StartingMapResolver: 新規Race enum値追加時に"capital_guild"がデフォルト返却。新種族が常にギルド開始になり種族特有の開始位置が無視される | 中 | `StartingMapResolver.cs:76-90` | |
| DN-3 | SymbolMapSystem.GetLocationDescription()でLocationType.Fieldとdefaultが同一テキストを返却。Fieldケースが冗長なデッドコード | 低 | `SymbolMapSystem.cs:111-112` | |
| DN-4 | NG+でEnemyMultiplier倍率がPlus4(3.0)→Plus5(4.0)で33%ジャンプ。Plus1→2→3→4が0.5ずつの均等増加に対し、Plus5のみ1.0増加で難易度スパイク | 低 | `NewGamePlusSystem.cs:29-33` | |

---

## カテゴリDO: 無限ダンジョンスコア・セーブスロットソート（5件）

| # | 問題 | 重要度 | 場所 | 修正判断 |
|---|------|--------|------|---------|
| DO-1 | InfiniteDungeonSystem.GetScoreRank()のランク境界: floor 0-4がD評価だがfloor=0（未挑戦）もD。未挑戦とfloor 4到達が同一評価で区別不能 | 低 | `InfiniteDungeonSystem.cs:62-71` | |
| DO-2 | MultiSlotSaveSystem: セーブスロットのSaveTime比較でnull SafeTimeを含むスロットのソートが非決定的。異なる実行でスロット選択結果が変わる可能性 | 高 | `MultiSlotSaveSystem.cs` | |
| DO-3 | GameClearSystem.CalculateScore()でMath.Max(0, 10000 - totalTurns/5)のTurnBonus計算。totalTurns=50000以上でTurnBonus=0固定となり、50000ターン以上のプレイでターン効率の差がスコアに反映されない | 中 | `GameClearSystem.cs:70` | |
| DO-4 | NewGamePlusSystem: GetCarryOverItems()のPlus2で「装備品」、Plus3で「ゴールド」のキャリーオーバーが記載されるが、実際の引き継ぎ処理(TransferToNewGame)がGameControllerに未実装 | 高 | `NewGamePlusSystem.cs`, `GameController.cs` | |
| DO-5 | MultiEndingSystem: TrueエンディングとNormalエンディングが両方hasClearedFinalBoss==trueを条件としており、文の順序のみで分岐。コードリファクタリング時にelse if構造が崩れるとNormalがTrueを上書きするリスク | 中 | `MultiEndingSystem.cs:44-70` | |

### DP: 処刑システム・武器アニメーション欠落

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| DP-1 | ExecutionSystem.GetExecutionAnimationName()が全15武器種のうちSword/Dagger/Axe/Hammer/Spear/Bowの6種のみ対応。Greatsword/Greataxe/Staff/Crossbow/Thrown/Whip/Fist/Unarmedの8種がデフォルト「止めの一撃」で武器固有の処刑演出がない | 致命的 | `ExecutionSystem.cs:41-50` | |
| DP-2 | ExecutionSystem.GetExecutionKarmaPenalty()が10種族中Beast/Humanoid/Undead/Demon/Dragonの5種のみ対応。Plant/Insect/Spirit/Constructの処刑時にカルマペナルティが0で倫理的影響なし | 中 | `ExecutionSystem.cs:30-38` | |
| DP-3 | GameOverSystem.GetDeathCauseDetail()がSuicide/SanityDeath/Fall/Unknown死因に未対応。デフォルト「不明な原因」が表示されプレイヤーに死因が伝わらない | 低 | `GameOverSystem.cs:77-87` | |
| DP-4 | GameClearSystem.IsFinalBossDefeated()で`currentFloor >= 30`判定。フロア31以上でも最終ボス撃破条件が成立し、フロア30以外でのクリアフラグが不正に立つ | 高 | `GameClearSystem.cs:85-88` | |
| DP-5 | GameClearSystem.CalculateScore()のターンボーナス計算`10000 - totalTurns/5`で整数除算。小数部分が切り捨てられスコア精度が低下 | 中 | `GameClearSystem.cs:70` | |

### DQ: ミミック・モンスター種族・属性矛盾

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| DQ-1 | MimicSystem.GetMimicRewardMultiplier()が固定値1.5倍を返却。階層深度に関係なく報酬倍率が一定で深層ミミック撃破のインセンティブ不足 | 中 | `MimicSystem.cs` | |
| DQ-2 | MimicSystem.GetDisguiseTypes()が「宝箱」「木箱」「収納箱」の3種のみ。偽装バリエーション不足でプレイヤーが容易にミミックを予測可能 | 低 | `MimicSystem.cs:38-42` | |
| DQ-3 | MonsterRaceSystem: Spirit/Amorphous両方ともPhysicalDamageMultiplier=0.5f。種族特性に実効的差がなく、ゲーム上のモンスター種族区別が不明確 | 中 | `MonsterRaceSystem.cs:40-42,83-85` | |
| DQ-4 | MonsterRaceSystem: AttackType.Ranged/Magicの物理耐性がどの種族にも定義されていない。遠距離/魔法攻撃への種族別耐性システムが機能しない | 高 | `MonsterRaceSystem.cs` | |
| DQ-5 | ElementalAffinitySystem: SpiritがElement.LightとElement.Darkの両方にWeakness（弱点）設定。通常Light↔Darkは対立属性だが同時弱点は設計矛盾 | 高 | `ElementalAffinitySystem.cs` | |
| DQ-6 | ElementalAffinitySystem: DarkAffinityトレイト（Enums.cs定義済み）が属性親和システムで一切参照されない | 低 | `Enums.cs:356`, `ElementalAffinitySystem.cs` | |

### DR: マルチクラス・鍛冶結果型不整合

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| DR-1 | MultiClassSystem: ClassTier.Masterが定義済みだがRequirementsリストにMasterティアへの転職条件が一切存在しない。Master職への昇進が不可能 | 致命的 | `MultiClassSystem.cs:17-24,50-51` | |
| DR-2 | MultiClassSystem: 全転職要件がRequiredLevel:20で統一。Base→Advanced間の段階的な難易度差がなくゲーム進行が平坦 | 中 | `MultiClassSystem.cs:19-23` | |
| DR-3 | MultiClassSystem.GetSubclassExpRate()が常に0.5f固定返却。クラス組み合わせによるサブクラス経験値の戦略性がない | 中 | `MultiClassSystem.cs:56` | |
| DR-4 | SmithingSystem.Synthesize()がSmithingResult.NewEnhanceLevel=0をデフォルト返却。合成結果の強化レベル情報が欠落し呼出側で誤参照リスク | 中 | `SmithingSystem.cs:38-45,76-80` | |
| DR-5 | SmithingSystem.Repair()のSmithingResult戻り値にNewEnhanceLevel/ResultItemIdが未設定（デフォルト値のまま） | 低 | `SmithingSystem.cs:25-35` | |

### DS: 罠二重定義・ギャンブル中毒ロジック

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| DS-1 | PlayerTrapType(5種)とTrapType(8種)が別enum定義。PitfallTrap/PitFall・SleepTrap/Sleep・AlarmTrap/Alarmの表記不一致でプレイヤー罠とダンジョン罠が別系統計算 | 中 | `TrapCraftingSystem.cs:9-25`, `Enums.cs:8-33,1230-1242` | |
| DS-2 | TrapCraftingSystemのPitfallTrap.Damage=15とTrapDefinitionのPitFall.BaseDamage=10が矛盾。同じ落とし穴罠でプレイヤー製が50%高ダメージ | 中 | `TrapCraftingSystem.cs:20-21`, `TrapDefinition.cs:117-120` | |
| DS-3 | PlayerTrapType.ExplosiveTrapに対応するTrapType値が存在しない。TrapType.Poisonに対応するPlayerTrapType値も存在しない。双方向の対応関係が不完全 | 低 | `TrapCraftingSystem.cs`, `Enums.cs:8-33` | |
| DS-4 | GamblingSystem.CheckAddiction(): 正気度の影響係数が0.001f（0.1%/ポイント）と極小。sanity=100でもrisk低減はわずか0.1で中毒判定に実質影響なし。正気度システムとの連動が形骸化 | 高 | `GamblingSystem.cs:64-69` | |
| DS-5 | GamblingSystem: 期待値コメント「Card期待値0.87」に対し実計算は約0.874。コメントと実装の精度乖離で設計意図が不明確 | 低 | `GamblingSystem.cs:32-38` | |

### DT: 能力値フラグ・方向補正・熟練度曲線

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| DT-1 | StatFlagSystem: CHA/LUKのフラグ発動閾値が20、他の8能力値（STR/INT/PER/AGI/VIT/DEX/MND/WIS）は閾値25。CHA/LUKが5ポイント低い閾値で発動しバランス不一致 | 中 | `StatFlagSystem.cs` | |
| DT-2 | DirectionSystem.CalculateElevationBonus(): 高所→低所がDamage+0.15f/Hit+0.10fだが低所→高所がDamage-0.10f/Hit-0.15f。ダメージと命中の減算値が非対称で高度差ペナルティが不均等 | 高 | `DirectionSystem.cs` | |
| DT-3 | WeaponProficiencySystem: UnarmedのPrimaryAttackTypeがAttackType.Blunt設定。ElementalAffinitySystemではAttackType.Unarmedとして個別処理。素手の攻撃タイプが2システム間で矛盾 | 高 | `WeaponProficiencySystem.cs`, `ElementalAffinitySystem.cs` | |
| DT-4 | ProficiencySystem.GetRequiredExp()が`100*Math.Pow(1.15, Level)`の指数関数。Level99で約38,890exp必要。後期のレベリングが極端に重く到達困難 | 中 | `ProficiencySystem.cs:27` | |
| DT-5 | ProficiencySystem: MaxLevel(100)/DamageBonusPerLevel(0.005)/経験値係数(1.15)がファイル内ハードコード。GameConstants等での一元管理がされていない | 低 | `ProficiencySystem.cs:27,48-49` | |

### DU: ダンジョン生成パラメータ・環境戦闘・秘密部屋

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| DU-1 | DungeonFeatureGenerator: CorridorTwistChance/SpecialRoomChance/WaterTileChance/LavaTileChanceの4パラメータが登録されるが実際のダンジョン生成で一切使用されない。テーマ別多様性が反映されない | 高 | `DungeonFeatureGenerator.cs:27-30` | |
| DU-2 | DungeonFeatureGenerator: DungeonFeatureDefinition.TrapDensityとDungeonFeatureParams.TrapChanceが同一概念（罠出現確率）を重複定義。正式な値源が不明確 | 中 | `DungeonFeatureGenerator.cs` | |
| DU-3 | EnvironmentalCombatSystem: SurfaceInteraction.DamageMultiplier（水+雷=2.0倍、油+火=1.5倍等）が定義されるが使用コードが存在しない。属性相互作用のダメージ倍率が完全に機能しない | 高 | `EnvironmentalCombatSystem.cs:21,26-34` | |
| DU-4 | EnvironmentalPuzzleSystem: CanAttempt()はintelligence+knowledgeLevelで判定するがCalculateSuccessRate()はintelligenceのみで計算。知識レベルが成功率に反映されない | 中 | `EnvironmentalPuzzleSystem.cs:45,51` | |
| DU-5 | SecretRoomSystem: CalculateSecretRoomCount()がRuins/Temple/Cryptのみ明示、ShouldGenerateSecretPassage()がRuins/Temple/Caveのみ明示。Crypt/Sewer/Mine/Volcanic等のダンジョンタイプで秘密部屋生成が不均等に扱われる | 中 | `SecretRoomSystem.cs` | |

### DV: 自動探索・碑文リセット・状態異常命名

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| DV-1 | AutoExploreSystem: HP停止閾値0.3f（30%）に対し満腹度停止閾値0.15f（15%）。満腹度の方が許容度が低い非対称設計でHP危機より先に餓死リスク | 低 | `AutoExploreSystem.cs` | |
| DV-2 | InscriptionSystem: 死に戻り時に碑文解読状態がリセットされるが、StatFlagSystem能力値フラグ等は保持される。システム間のリセットポリシーが非対称 | 中 | `InscriptionSystem.cs` | |
| DV-3 | ExtendedStatusEffectSystem: StatModifierキーが"HitRate"/"AttackMultiplier"（英語複合語）と"STR"/"AGI"（略称）で命名不統一。GetStatModifier()呼出時の文字列ミスリスク | 低 | `ExtendedStatusEffectSystem.cs` | |
| DV-4 | ExtendedStatusEffectSystem: "berserk"がIsBuff=false（デバフ分類）だが攻撃力+50%の効果あり。バフ/デバフ分類と実効果が直感的に矛盾 | 低 | `ExtendedStatusEffectSystem.cs` | |
| DV-5 | ContextHelpSystem: RegisterDefaultTopics()でKeyBind="hjklyubn"（複数キー連結）を登録するがGetHelpForKey()は単一キー完全一致検索。"h"キー押下でヘルプトピックがヒットしない | 中 | `ContextHelpSystem.cs:72-75,124-133` | |

### DW: GrowthSystem二重管理・DungeonFaction境界値

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| DW-1 | GrowthSystem: CalculateMaxHp/CalculateMaxMpがraceGrowth/classGrowthテーブルとRaceDefinition/ClassDefinition両方からHP/MPボーナスを合算。同一情報の二重管理でバランス調整時の不一致リスク | 中 | `GrowthSystem.cs:172-197`, `CharacterCreation.cs:17-82` | |
| DW-2 | DungeonFactionSystem.AreHostile()が`>0.5f`判定（`>=`でない）。Dragon+Demon=0.5fが丁度境界値のため敵対判定されない（0.5>0.5はfalse） | 致命的 | `DungeonFactionSystem.cs:20,27,46,52` | |
| DW-3 | DungeonFactionSystem.AreAllied()が`<0.3f`判定（`<=`でない）。Undead+Spirit=0.3fが丁度境界値のため同盟判定されない（0.3<0.3はfalse） | 致命的 | `DungeonFactionSystem.cs:20,27,46,52` | |
| DW-4 | FactionWarSystem: Faction1/Faction2の参加報酬が同一（20ポイント）。攻撃側/防衛側に戦略差がなくプレイヤーの派閥選択に意味がない | 中 | `FactionWarSystem.cs` | |
| DW-5 | FactionWarSystem: War.Duration値が計算されるがゲーム上の終了条件/効果に使用されない。戦争期間が装飾的 | 低 | `FactionWarSystem.cs` | |
| DW-6 | TerritoryInfluenceSystem: Reset()後にInitialize()の呼出フローが明示されず、死に戻り時の勢力状態が不定 | 中 | `TerritoryInfluenceSystem.cs:15-18,55-58` | |

### DX: 戦闘計算・魔法防御係数・クリティカル不整合

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| DX-1 | DamageCalculator: 物理防御係数0.5に対し魔法防御係数0.3。同攻撃力・防御力で魔法ダメージが物理の約1.4倍となりINT投資が圧倒的有利。ゲームバランス崩壊レベル | 高 | `DamageCalculator.cs:35,82` | |
| DX-2 | 魔法ダメージ計算でクリティカルヒットが不可能（IsCritical=falseハードコード）。物理ビルドのクリティカル期待値ボーナスに対し魔法ビルドが不利 | 高 | `DamageCalculator.cs:91` | |
| DX-3 | クリティカル率計算でDEX×0.3%だが回避計算でAGI×0.5%。DEXとAGIの寄与係数が非対称でステータス投資の選択が歪む | 中 | `DamageCalculator.cs:144,168-176` | |
| DX-4 | CheckCritical()が2つのオーバーロード（パラメータ付きとdouble直接渡し）で存在。インターフェース混在で保守性低下 | 低 | `DamageCalculator.cs:168-176,179-181` | |
| DX-5 | SpellCastingSystem: 暴発ダメージがMpCost×0.5のハードコード。低MP呪文でHP30のキャラに大打撃、高MP呪文でHP300のキャラに軽微。スケーリングが逆転 | 中 | `SpellCastingSystem.cs:178-185` | |

### DY: アイテム等級・ドロップ・素材品質未機能

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| DY-1 | DropTableSystem.GenerateLoot()がDropTableEntry.MinGradeパラメータを完全に無視。全アイテムがItemGrade.Standard固定で等級システムが機能しない | 高 | `DropTableSystem.cs:10,55-94` | |
| DY-2 | Material.Qualityプロパティ（品質1-100）が定義されるがItemFactory/DropTableSystemで品質値が未設定。全素材が品質50固定 | 低 | `Consumables.cs:377-390`, `ItemFactory.cs` | |
| DY-3 | Equipment.RequiredStats: Luck/Charisma/Perceptionがプロパティ定義されているがCanEquip()の条件文に含まれない。これらステータスの装備制限が機能しない | 低 | `Equipment.cs:69-105` | |
| DY-4 | Food.HydrationValueプロパティが定義されているがFood.Use()でThirstSystemとの連携コードなし。Water/CleanWaterの渇き回復効果が機能しない | 中 | `Consumables.cs:152-218` | |
| DY-5 | Scroll.Use(Character user)の署名でゲームマップ/プレイヤー位置へのアクセスが不可能。テレポート/マップ表示/帰還巻物がゲーム状態を変更できない | 中 | `Consumables.cs:274-318` | |

### DZ: GameController計算結果破棄・戻り値無視

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| DZ-1 | TrySmithRepair(): repairAmount計算後にメッセージ表示のみで装備の耐久値加算処理なし。「追加修理: +N」と表示されるがdurabilityが実際には増加しない | 致命的 | `GameController.cs:7564-7569` | |
| DZ-2 | ApplySpellDetect(): 敵感知数をカウントしメッセージ表示するが、敵の検出状態/追跡対象への反映なし。感知呪文が装飾的 | 中 | `GameController.cs:4487-4508` | |
| DZ-3 | ProcessDialogueAction() "smuggle"ケース: TrySmuggle("")の戻り値（bool成功/失敗）が完全に無視。密輸失敗時も成功時もTurnCount+=10が実行される | 中 | `GameController.cs:6069` | |
| DZ-4 | SpellCastingSystem: MaxSpellWords=7固定。語彙習得が進んでも使える組み合わせ数が同じ7語制限。深度に応じた段階的拡張メカニズムなし | 中 | `SpellCastingSystem.cs:89`, `GameConstants.cs` | |
| DZ-5 | RoomCorridorGenerator.IsDoorCandidate(): ドア配置判定が「南北壁+東西開通」条件で実際には通路交差点を指す可能性。部屋境界ドアではなく通路内にドアが設置されるエッジケース | 中 | `RoomCorridorGenerator.cs:419-461` | |

### EA: AIビヘイビア・Spirit疑似テレポート

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EA-1 | RacialBehaviors.SpiritBehavior: テレポート判定後にMoveRandom()（隣接マスランダム移動）で代替実装。「瞬間移動」という設計意図と1マス移動の実装が乖離 | 低 | `RacialBehaviors.cs:214-216` | |
| EA-2 | SpellEffectResolver.DetermineElement(): Element.Curse/Lightningに対応する効果語定義が不完全。一部属性が詠唱結果に反映されない可能性 | 低 | `SpellCastingSystem.cs:388-419` | |

### EB: AddGold(-負値)サイレント失敗・ゴールド消費不能

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EB-1 | Player.AddGold()はamount>0のガードがあり負値を無視する。一方SpendGold()メソッドが別途存在する。GameControllerの6箇所でAddGold(-xxx)を呼び出しており、投資・雇用・ギャンブル・ペナルティ・クラフト・購入の全ゴールド消費が**サイレントに失敗**する | 致命的 | `Player.cs:118-125(AddGold)`, `Player.cs:128-137(SpendGold)` | |
| EB-2 | 投資処理: Player.AddGold(-investAmount)が無効。投資金額が減らずに投資効果だけ得られる無限ゴールドバグ | 致命的 | `GameController.cs:6085` | |
| EB-3 | コンパニオン雇用: Player.AddGold(-companion.HireCost)が無効。雇用コスト0で無制限雇用可能 | 致命的 | `GameController.cs:6277` | |
| EB-4 | ギャンブル: Player.AddGold(-betAmount)が無効。賭け金が減らず常にプラス収支 | 致命的 | `GameController.cs:7403` | |
| EB-5 | 密輸ペナルティ: Player.AddGold(-Math.Min(penalty, Player.Gold))が無効。密輸失敗のゴールドペナルティが適用されない | 高 | `GameController.cs:7645` | |
| EB-6 | クラフト素材費: Player.AddGold(-recipe.MaterialCost)が無効。クラフト無料化 | 致命的 | `GameController.cs:7679` | |
| EB-7 | ショップ購入: Player.AddGold(-item.Price)が無効。全商品が実質無料 | 致命的 | `GameController.cs:7708` | |
| EB-8 | SpendGold()は3箇所（訓練・学習）でのみ使用。残り6箇所がAddGold(-xxx)パターンで不統一 | 高 | `GameController.cs:5371,5387,5412(SpendGold)` | |

### EC: 死亡プレイヤーアクション制御不備

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EC-1 | ProcessInput()のメイン処理パスにPlayer.IsAlive検証なし。IsGameOver/IsRunningチェック後、状態異常チェック内でのみIsAliveを検証。通常行動（移動・攻撃・スキル・アイテム等）時にIsAlive未検証 | 致命的 | `GameController.cs:930-932,962-970` | |
| EC-2 | UseItem()メソッドにIsAlive検証なし。死亡後に回復アイテム使用で疑似復活可能 | 高 | `GameController.cs:2762-2807` | |
| EC-3 | TryUseFirstReadySkill()にIsAlive検証なし。死亡後もスキル発動可能 | 高 | `GameController.cs:3692-3724` | |
| EC-4 | TryRangedAttack()にIsAlive検証なし。死亡後も遠距離攻撃可能 | 高 | `GameController.cs:3550付近` | |
| EC-5 | IsGameOverフラグがtrueになるまでの間にIsAliveがfalseの状態が存在。HandlePlayerDeath()呼出前の複数ターン処理で死亡プレイヤーが行動継続 | 中 | `GameController.cs:952-955` | |

### ED: スポーン位置ハードコードフォールバック・壁内配置リスク

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| ED-1 | ダンジョンフロア移動時: StairsUpPosition/EntrancePositionが両方nullの場合、Position(5,5)にフォールバック。マップサイズやタイルタイプ検証なしで壁内に配置される可能性 | 致命的 | `GameController.cs:690-691` | |
| ED-2 | ロケーションマップ遷移時: locationMap.EntrancePosition/StairsUpPositionが両方nullの場合、Position(5,5)にフォールバック | 致命的 | `GameController.cs:5037-5038` | |
| ED-3 | フィールドマップ遷移時: fieldMap.EntrancePositionがnullの場合、Position(Width/2, Height-1)にフォールバック。マップ最下端行が壁の場合に壁内配置 | 高 | `GameController.cs:5077-5078` | |
| ED-4 | 建物内部マップ遷移時: interiorMap.EntrancePositionがnullの場合、Position(Width/2, Height-2)にフォールバック。検証なし | 高 | `GameController.cs:5228-5229` | |
| ED-5 | 全フォールバックでGetRandomWalkablePosition()等の安全な代替手段を使用していない。壁内配置時はプレイヤーが移動不能になり詰み | 中 | `GameController.cs:690,5037,5077,5228` | |

### EE: StatusEffects.First()のInvalidOperationException

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EE-1 | ProcessInput()内でHasStatusEffect()チェック（935-938行）とFirst()呼び出し（940行）が分離。チェック通過後にProcessTurnEffects()等のイベントで状態異常が解除された場合、First()がInvalidOperationExceptionをスロー | 高 | `GameController.cs:935-942` | |
| EE-2 | FirstOrDefault()を使用してnullチェックすべきだが、First()で条件付き検索を行っている。コレクション変更のタイミング次第でクラッシュ | 中 | `GameController.cs:940-942` | |

### EF: AudioManager Dispose不完全・リソースリーク

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EF-1 | Dispose()でStopAll()とイベント解除のみ実行。_bgmPlayer自体のDispose()/Close()が未呼び出しでMediaPlayerリソースリーク | 高 | `AudioManager.cs:198-206` | |
| EF-2 | _sePlayers（最大8個のMediaPlayer）のDispose()/Close()が未呼び出し。長時間プレイでオーディオデバイスリソース枯渇の可能性 | 高 | `AudioManager.cs:198-206` | |
| EF-3 | GC.SuppressFinalize(this)を呼び出すがファイナライザー未定義。Suppress不要だが害はない | 低 | `AudioManager.cs:205` | |

### EG: SaveManager例外ハンドリング欠落

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EG-1 | Load()メソッドでJsonSerializer.Deserialize()にtry-catchなし。破損JSONでJsonExceptionがスロー、ゲーム起動時クラッシュ | 高 | `SaveManager.cs:36-42` | |
| EG-2 | File.ReadAllText()にIOExceptionハンドリングなし。ファイル読み取り中のI/Oエラーで未処理例外 | 中 | `SaveManager.cs:40` | |
| EG-3 | セーブファイル破損時のリカバリ手段なし。バックアップセーブ機構も不在 | 中 | `SaveManager.cs全体` | |

### EH: Damage抵抗値の負値範囲によるダメージ増幅

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EH-1 | Damage.CalculateFinal()でresistanceが[-1f, 0.9f]にクランプ。resistance=-1.0fで(1-(-1))=2.0倍ダメージ増幅。防御側の抵抗値が負になるケースで味方攻撃が意図せず2倍化 | 高 | `Damage.cs:70` | |
| EH-2 | 状態異常やデバフでresistanceが負値に設定される場合、最大2倍ダメージブーストが発生。バランス破壊の可能性 | 中 | `Damage.cs:70` | |
| EH-3 | 意図的な「弱点」設計の場合でも上限-1.0f(2倍)が適切か不明。設計書に仕様記載なし | 低 | `Damage.cs:70` | |

### EI: ObjectPool非アトミックサイズチェック

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EI-1 | Return()メソッドで_pool.Count < _maxSize判定後に_pool.Add()実行。ConcurrentBagのCountプロパティとAdd()がアトミックでないため、複数スレッドが同時にチェック通過してmaxSize超過の可能性 | 中 | `ObjectPool.cs:76-79` | |
| EI-2 | maxSize超過時のelse分岐でInterlocked.Decrementを使用しスレッド安全だが、Add分岐のCount+Add非アトミック問題は残存 | 低 | `ObjectPool.cs:76-84` | |

### EJ: クエスト状態遷移検証不備

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EJ-1 | NpcSystem内のクエスト完了遷移でprogress.State = QuestState.Completedの前にState == Activeの検証なし。IsComplete条件のみで遷移するため、既にCompleted/TurnedIn状態から再度Completedに戻る可能性 | 中 | `NpcSystem.cs:426-429` | |
| EJ-2 | TurnInQuest()ではState != QuestState.Completedを検証するが、Active→Completed→TurnedInの厳密な状態マシン遷移ガードが不完全 | 中 | `NpcSystem.cs:434-456` | |
| EJ-3 | クエスト状態の不正遷移がセーブデータ経由で持ち込まれた場合のバリデーションなし | 低 | `NpcSystem.cs全体` | |

### EK: SkillDatabase.GetById() null-forgiving演算子

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EK-1 | TryUseFirstReadySkill()内のWhere句でs != nullチェック後、s!.Idでnull-forgiving演算子使用。Whereのnullチェックで安全だが、コンパイラ警告抑制の設計パターンとして不統一 | 低 | `GameController.cs:3698-3700` | |
| EK-2 | SkillDatabase.GetById()のnull返却に対し、呼び出し側でのnullチェックが箇所により?.演算子/if null/!演算子と3パターン混在 | 中 | `GameController.cs:3698,584-587` | |

### EL: CraftingSystem素材消費アトミシティ違反

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EL-1 | Craft()メソッドがforeachで素材を消費（L125-128）してからItemDefinitions.Create()（L137）を呼び出す。Create()がnull返却時に素材・ゴールドが既に消費済みでロールバック不可。アトミシティ原則違反 | 致命的 | `CraftingSystem.cs:116-142` | |
| EL-2 | RemoveItem()の戻り値（成功/失敗）を一切チェックしていない。在庫不足で部分消費が発生しても検知不可 | 致命的 | `CraftingSystem.cs:125-128` | |
| EL-3 | SpendGold()が素材消費後に実行される。素材消費成功→ゴールド不足の場合、素材のみ消失 | 致命的 | `CraftingSystem.cs:131-134` | |
| EL-4 | CanCraft()事前チェックとCraft()実行の間に状態変化（マルチスレッド/イベント）が起きた場合、CanCraft=trueでもCraft中にリソース不足になる可能性がある | 中 | `CraftingSystem.cs:121-133` | |
| EL-5 | CraftingResult失敗時のメッセージが日本語「結果アイテムの生成に失敗した」のみでアイテムID等のデバッグ情報なし | 低 | `CraftingSystem.cs:139` | |

### EM: PetSystem null-forgiving・死亡ペット騎乗

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EM-1 | Feed()/Train()/ToggleRide()/TickHunger()でTryGetValue失敗時に`return pet!`でnull-forgiving演算子使用。petがnull状態でnullを返却し、呼び出し側でNullReferenceException | 致命的 | `PetSystem.cs:67,80,89,114` | |
| EM-2 | ToggleRide()にCurrentHp > 0の生存チェックなし。死亡ペット（HP=0）に対して騎乗切替が可能 | 高 | `PetSystem.cs:87-95` | |
| EM-3 | ToggleRide()にLoyaltyの閾値チェックなし。忠誠度0のペットでも騎乗可能 | 中 | `PetSystem.cs:87-95` | |
| EM-4 | Horse(BaseSpeed=15,倍率2.0x)とDragon(BaseSpeed=10,倍率2.5x)でBaseSpeedとMultiplierが逆転。Dragonが全ステータスで優位になりHorse選択肢が死に駒 | 中 | `PetSystem.cs:43-48,102-109` | |
| EM-5 | GetObedienceRate()が存在しないペットIDで0を返却。忠誠度0の既存ペットと区別不能 | 中 | `PetSystem.cs:133-137` | |
| EM-6 | TickHunger()にターン当たり1回の頻度制限なし。複数回呼び出しで空腹度即時枯渇・忠誠度急降下が可能 | 中 | `PetSystem.cs:112-120` | |
| EM-7 | 空腹度ペナルティがHunger==0の厳密一致のみ。Hunger=1→0変化時にのみ忠誠度-2。継続的な飢餓状態での追加ペナルティなし | 低 | `PetSystem.cs:116-117` | |

### EN: FishingSystem宝箱・確率超過

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EN-1 | fish_treasure（宝箱）とfish_junk（ガラクタ）のRarity=0。GetAvailableFish()がf.Rarity>0でフィルタするため両アイテムが釣果リストから完全除外。CalculateTreasureRate()は存在するが適用先なし | 高 | `FishingSystem.cs:38-41,48-59` | |
| EN-2 | CalculateCatchRate()/CalculateJunkRate()/CalculateTreasureRate()の3確率関数が独立計算。高レベル・高Luck時に合計>1.0（100%超過）となり確率空間が破綻 | 高 | `FishingSystem.cs:62-78` | |
| EN-3 | Luck修正値が1ポイント=+0.1f。Luck3.0で+0.3fのボーナスはfishingLevel10相当（0.03f×10）に匹敵。Luck偏重でfishing skill意味消失 | 中 | `FishingSystem.cs:64` | |
| EN-4 | GetAvailableFish()がMinFishingLevelでフィルタするが、CalculateCatchRate()はレベル差を考慮しない。条件を満たせば低レベルでも高レア魚を同確率で釣れる | 中 | `FishingSystem.cs:48-59,62-66` | |
| EN-5 | fish_treasure/fish_junkのActiveSeasons・ActiveTimesがArray.Empty()。仮にRarityフィルタを修正してもSeason/Timeフィルタで除外される | 低 | `FishingSystem.cs:38-41` | |

### EO: StatusEffect MaxStack未強制・永続呪い

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EO-1 | Curse（呪い）がduration=int.MaxValueで生成。AllStatsMultiplier=0.80f（全ステ-20%）が実質永続。解除メカニズムがStatusEffectSystem内に不在 | 高 | `StatusEffectSystem.cs:174-181` | |
| EO-2 | Petrification（石化）がduration=int.MaxValueでTurnCostModifier=float.MaxValue。完全行動不能が永続で詰み状態 | 高 | `StatusEffectSystem.cs:220-228` | |
| EO-3 | InstantDeath（即死）がDamagePerTick=int.MaxValueで実装。「即死」名称だがtick処理で適用されるため1ターン遅延。int.MaxValueの演算でオーバーフローリスク | 高 | `StatusEffectSystem.cs:233-241` | |
| EO-4 | MaxStackプロパティが定義（Poison=3, Bleeding=5等）されているが、Apply/AddEffect側でMaxStackチェックなし。Stack()メソッド内のみでキャップ。重複適用時に無限スタック | 中 | `StatusEffectSystem.cs:12,46` | |
| EO-5 | TurnCostModifier=float.MaxValueの麻痺/石化がint型ターンコストに変換される際、float→int変換で未定義動作の可能性 | 中 | `StatusEffectSystem.cs:85,110,167` | |
| EO-6 | CheckResistance()の合計耐性が0.95fでハードコードキャップ。装備・バフ・種族特性の組合せでも95%超不可。ボスクリーチャー向けの免疫（100%）実装不可 | 中 | `StatusEffectSystem.cs:328` | |
| EO-7 | Poison DamagePerTick計算がMath.Max(1, (int)(maxHp*0.02))。int切り捨てでmaxHp=51時に1.02→1ダメージ（1.96%、設計値2%未満） | 低 | `StatusEffectSystem.cs:24-26` | |

### EP: EnchantmentSystem ExpBoost/DropBoostデッドコード

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EP-1 | ExpBoostエンチャント（EffectValue=1.15f）がCalculateEnchantedDamageBonus()のswitch式で_ => 0のデフォルトケースに該当。効果値が一切適用されないデッドコード | 高 | `EnchantmentSystem.cs:59-60,186-206` | |
| EP-2 | DropBoostエンチャント（EffectValue=1.2f）も同様にデフォルトケースで0返却。アイテムドロップ率上昇効果が完全に未機能 | 高 | `EnchantmentSystem.cs:61-62,204` | |
| EP-3 | Enchant()メソッドにアイテムのnullチェック・装備種別チェック・エンチャント上限チェックなし。null/消耗品/既エンチャント装備への重複適用が可能 | 中 | `EnchantmentSystem.cs:139-154` | |
| EP-4 | RequiredQualityチェックがenum値の>=比較に依存（gem >= definition.RequiredQuality）。SoulGemQualityのenum定義順が変更された場合に比較結果が逆転するリスク | 中 | `EnchantmentSystem.cs:128-134` | |

### EQ: ReputationSystem閾値非対称・IsWelcome緩さ

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EQ-1 | Indifferent（無関心）の範囲が[-19, 19]=39ポイント幅。他ランクは20-30ポイント幅。中立帯が広すぎて評判変動の実感が薄い | 中 | `ReputationSystem.cs:96-105` | |
| EQ-2 | IsWelcome()がReputationRank.Hatedのみブロック。Hostile（敵意: [-79,-50]）でも入境許可。敵対領域に自由にアクセス可能 | 中 | `ReputationSystem.cs:79-80` | |
| EQ-3 | 評判値上限100到達後のModifyReputation(+5)がClamp後に同値でイベント未発火。呼び出し側が加算成否を判定不可 | 低 | `ReputationSystem.cs:31-41` | |
| EQ-4 | Enum.GetValues\<TerritoryId\>()をコンストラクタで毎インスタンス実行。リフレクション使用でパフォーマンス非効率 | 低 | `ReputationSystem.cs:24-27` | |

### ER: SkillSystem パッシブ重複・クールダウン永続

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| ER-1 | hp_boostがFighter/Knight/Cleric/Monk、critical_eyeがFighter/Ranger/Monkの複数クラスツリーに登録。転職でlearnedSkillsのHashSetに重複は防がれるが、同一パッシブIDで別クラス分岐からの学習済みチェック不備 | 中 | `SkillSystem.cs:156-160,171-225` | |
| ER-2 | SkillSystemにReset()メソッドが不在。PetSystem/ReputationSystem等にはReset()実装あり。ゲームオーバー/新規プレイ時にクールダウン状態がリセットされない | 中 | `SkillSystem.cs` | |
| ER-3 | RestoreCooldownState()がcooldown値の正値検証・最大値検証なし。破損セーブデータで負値/超大値クールダウンが復元可能 | 中 | `SkillSystem.cs:353-362` | |
| ER-4 | GetLearnableSkills()が!learnedSkills.Contains()でフィルタ。別クラスで既習得の共通パッシブが再表示されない（HashSet重複防止は機能）が、UI上で「既に別クラスで習得済み」の説明なし | 低 | `SkillSystem.cs:336-338` | |

### ES: WeatherSystem RangedHitModifier適用方式不統一

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| ES-1 | Clear天候のRangedHitModifier=0.05f。他の修正値（Fire/Ice/Lightning等）が1.0fベースの乗算値なのに対し、RangedHitModifierは加算値として使用。+5%ボーナスのはずがベース×0.05で95%減少になる可能性 | 高 | `WeatherSystem.cs:32` | |
| ES-2 | Rain/Fog/Snow/StormのRangedHitModifierが-0.1f/-0.2f/-0.1f/-0.3fの減算値。Clear=+0.05fとの非対称（加算最大+5% vs 減算最大-30%）でペナルティ偏重 | 中 | `WeatherSystem.cs:43,54,65,76` | |
| ES-3 | WeatherEffectのSightModifier=1.0f/MovementCostModifier=1.0fは乗算値（1.0=100%）だがRangedHitModifierのみ加減算値。同一record内でセマンティクス不統一 | 中 | `WeatherSystem.cs:26-80` | |
| ES-4 | RangedHitModifierの適用先（GameController内）で加算/乗算のどちらで使用されているかにより影響が大きく異なるが、WeatherEffect定義側にコメントなし | 低 | `WeatherSystem.cs:26-80` | |

### ET: GameController料理品質・失敗チェック未実装

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| ET-1 | TryCook()でCookingSystem.CalculateQuality()にPlayer.Level*2を渡している。本来の料理熟練度（ProficiencyCategory.Cooking）ではなくプレイヤーレベルベースで品質決定。料理スキル育成が無意味 | 高 | `GameController.cs:7215` | |
| ET-2 | CookingSystem.CheckCookingFailure()メソッドが存在するがGameController.TryCook()から一切呼び出されていない。料理が常に成功し、失敗確率（熟練度0で30%）が機能しない | 高 | `GameController.cs:7181-7229` | |
| ET-3 | CalculateQuality()が0.3f+Min(prof,100)*0.01fで品質0.3～1.3倍。Player.Level*2でLevel50=prof100相当、Level25でprof50相当。レベル依存で料理品質がリニアスケール | 中 | `CookingSystem.cs:49-53` | |

### EU: TerritoryInfluenceSystem正規化前クランプ

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EU-1 | ModifyInfluence()が個別faction値をMath.Clamp(0,1)で制限した後にNormalizeInfluence()を呼び出す。クランプ→正規化の順序で、正規化後に合計1.0の不変条件が保証されない場合がある | 中 | `TerritoryInfluenceSystem.cs:20-29` | |
| EU-2 | 複数factionに連続で大きなdelta（+0.5等）を適用した場合、個別クランプで各0.6→合計1.8後に正規化で0.33ずつに。意図した勢力バランスからの大幅乖離 | 中 | `TerritoryInfluenceSystem.cs:20-29,61-69` | |
| EU-3 | NormalizeInfluence()のゼロ除算チェック：全faction影響度が0の場合のフォールバック動作が不明確 | 中 | `TerritoryInfluenceSystem.cs:61-69` | |

### EV: CompanionSystem最小1ダメージ貫通

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EV-1 | DamageCompanion()の式Math.Max(1, damage-companion.Defense)で防御超過時も必ず1ダメージ貫通。Defense > damageでも回避不可。高Defense仲間でも確定ダメージ | 高 | `CompanionSystem.cs:161` | |
| EV-2 | CalculateCompanionDamage()がMath.Max(1, attack+level/2)で最低1保証。レベル0・攻撃0でも1ダメージ。攻撃不能状態の仲間が存在不可 | 中 | `CompanionSystem.cs:149-152` | |
| EV-3 | DamageCompanion()で仲間が死亡（HP=0）した場合にIsAlive=falseを設定するが、復活メカニズム（ReviveCompanion等）がCompanionSystem内に不在 | 中 | `CompanionSystem.cs:155-165` | |

### EW: Inventory重量制限未強制・スタック整理不備

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EW-1 | Inventory.Add()がMaxWeight超過チェックなし。Player.CanPickUp()でのみ重量検証するがAdd()自体では未検証。スタック加算時もTotalWeight再チェックなしで重量制限バイパス可能 | 高 | `Inventory.cs:35-56` | |
| EW-2 | Inventory.Organize()でMaxStack=0の場合にtransferAmount計算が不正。MaxStack>0のバリデーションなし。不正データでスタック整理が無限ループまたはデータ破損のリスク | 中 | `Inventory.cs:167-206` | |
| EW-3 | Player.IsOverweightプロパティが存在するがInventory.Add()内で参照されない。重量超過状態でも新規アイテム追加が可能で、ゲーム設計のオーバーウェイトペナルティが形骸化 | 高 | `Player.cs:172, Inventory.cs:35-56` | |
| EW-4 | Inventory.Remove()でStackCount -= countの結果が負値にならないチェックがif文で実施されるが、外部からStackableオブジェクトに直接アクセスされた場合の保護なし | 中 | `Inventory.cs:69-83` | |
| EW-5 | Inventory.UseItem()でConsumable.Use(user, random)のuserパラメータnullチェック不在。null渡しでNullReferenceExceptionクラッシュ | 高 | `Inventory.cs:146-162` | |

### EX: Player生存ステータス初期化不整合・Sanity境界問題

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EX-1 | Player.Create(name, baseStats)簡易ファクトリで_thirstと_fatigueが未初期化（0のまま）。ThirstStageがCriticalDehydration、FatigueStageがCollapseで即座に致命的状態。完全ファクトリCreate(name,race,class,bg)では全5項目初期化済みで矛盾 | 高 | `Player.cs:597-611 vs 625-674` | |
| EX-2 | Sanity=0でSanityStage.Brokenになるが、CanBeRescued()がSanity>0を要求。Sanity=0のプレイヤーは救助不可能でゲーム進行不能（復帰手段なし）のパラドックス | 高 | `Player.cs:400-408, 148` | |
| EX-3 | CreateTransferData()でTotalDeaths=0にハードコード設定。コメント「外部で管理」だが外部管理の実装なし。転生繰り返しで死亡回数が失われ永続進行追跡が機能しない | 中 | `Player.cs:484-496` | |
| EX-4 | TransferData.Sanityプロパティが定義されているがCreateTransferData()で設定されない。ApplyTransferData()ではInitialSanityにフォールバック。転生時のSanity状態引き継ぎが未実装のデッドプロパティ | 低 | `Player.cs:859, 486, 505` | |
| EX-5 | RestoreFromSave()でlevelとexperienceの整合性バリデーションなし。破損セーブでlevel=99/experience=5等の不正状態がロード可能 | 中 | `Player.cs:698-724` | |

### EY: Character基底クラスリソース管理・負値消費バイパス

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EY-1 | ConsumeMp()/ConsumeSp()にamount>=0バリデーションなし。負値渡し（ConsumeMp(-100)）でMP回復が可能。AddGold()はamount>0チェック有りで非対称 | 中 | `Character.cs:119-120, Player.cs:118-137` | |
| EY-2 | RestoreMp()/RestoreSp()が+=直接代入でプロパティsetterのClamp処理をバイパスする可能性。バッキングフィールド_currentMp/_currentSpへの直接操作で上限超過リスク | 中 | `Character.cs:116-120` | |
| EY-3 | LevelUp()でEffectiveStats（装備ボーナス含む）に基づくHP/MP差分計算。レベルアップ処理中の装備変更でMaxHp変動時にHP増加量が不正計算 | 中 | `Player.cs:366-387` | |
| EY-4 | Player.AddGold()のamount>0ガードが既にEB-1で報告済みだが、Character基底クラスレベルでもTakeDamage()の負値ダメージ（回復として機能）に対するバリデーションなし | 中 | `Character.cs:95-110` | |

### EZ: ItemFactory乱数生成・レアリティ適用不備

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| EZ-1 | GenerateRandomItem()のitemType=(ItemType)_random.Next(3)でItemType列挙型7種中Equipment/Consumable/Foodの3種のみ生成。Material/Key/Quest/Etcタイプが絶対に生成されずドロップテーブルに大きな偏り | 高 | `ItemFactory.cs:837` | |
| EZ-2 | GenerateRandomEquipment()がrarity引数で強化値を計算するが、生成アイテム本体のRarityプロパティを変更していない。全ランダム装備がCommon/Uncommonのまま残り、レアリティシステムが機能しない | 高 | `ItemFactory.cs:863-906` | |
| EZ-3 | GenerateRandomConsumable()でレアリティに応じて異なるポーション種を選択するが、効果値の調整なし。上位レアリティでもHealingPotionは常に75回復固定で階層進行に応じた報酬スケーリングが不在 | 中 | `ItemFactory.cs:908-928` | |
| EZ-4 | StackableItem.CanStackWith()がItemId/IsIdentified/IsCursed/IsBlessedのみ比較しEnhancementLevelを比較しない。+1ポーションと通常ポーションがスタックされ強化値が消失 | 高 | `Item.cs:321-329` | |

### FA: Equipment呪い解除・防具スロット判定不整合

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| FA-1 | Equipment.OnUnequip()でIsCursed&&HasStatusEffect(Curse)判定後の処理ブロックが空実装。呪い装備の外す制限が実装されておらず、呪いシステムが完全に無機能 | 高 | `Equipment.cs:116-123` | |
| FA-2 | IsArmorSlot()がOffHandを含むが、盾とサブ武器でGetEffectiveStatModifier()のVitality加算が適用される。両手武器時のOffHandスロットでVitality強化が不正に付与 | 中 | `Equipment.cs:139-140` | |
| FA-3 | 両手武器装備時のオフハンド解除処理でインベントリへの戻し処理がコメントのみで未実装。オフハンド装備品がデータ上消失するリスク | 高 | `Equipment.cs:354付近` | |
| FA-4 | GetDisplayName()で未識別アイテムはUnidentifiedName表示だが、EnhancementLevel!=0チェックが先行するため強化値情報が未識別状態でも推測可能な設計漏れ | 中 | `Item.cs:279-296` | |

### FB: Consumable腐敗食料・ポーション効果値ゼロ問題

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| FB-1 | Potion.Use()でEffectValue=0かつEffectPercentage=0のとき回復量が0。ポーション使用しても何も起きないが成功として扱われアイテム消費。プレイヤーへのフィードバックなし | 中 | `Consumables.cs:54-78` | |
| FB-2 | IsRotten食料のStatusEffect(Poison, 10)付与がItemEffectレコードのDurationフィールドに反映されず0のまま。効果表示と実際の毒持続ターンが不整合 | 中 | `Consumables.cs:189-199` | |
| FB-3 | Food.HydrationValue定義済みだがThirstSystemとの連携なし（DY-4と関連）。食事による水分補給が機能せず、Thirst管理が食料タイプと独立した別系統のみ | 中 | `Consumables.cs, ThirstSystem.cs` | |
| FB-4 | Scroll.Use()のメソッドシグネチャがゲーム状態を変更できない設計。巻物効果（テレポート、識別等）が実際のゲーム状態に反映されないデッドコード | 高 | `Consumables.cs（Scrollクラス）` | |

### FC: Damage抵抗計算・Stats CriticalRate偏重

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| FC-1 | Damage.CalculateFinal()のresistanceクランプ範囲[-1f,0.9f]で最大軽減90%制限。100%耐性（完全無効化）が設計上不可能だがゲーム内アイテム/スキルで「○○無効」の記述と矛盾 | 中 | `Damage.cs:61-73` | |
| FC-2 | Stats.CriticalRateでLuck*0.01+Dexterity*0.005の係数設定。LuckがDexterityの2倍のクリティカル寄与を持ち、器用さベースの戦闘クラスが不利。ゲームバランス設計に要検討 | 低 | `Stats.cs:30` | |
| FC-3 | TurnAction.WaitForInput（BaseTurnCost=0）とTurnAction.Wait（BaseTurnCost=TurnCosts.Wait）が同じTurnActionType.Waitを使用。ターンコスト処理で区別不可能 | 中 | `TurnAction.cs:110, 146-150` | |
| FC-4 | Position.GetDirectionTo()で同一位置の場合に常にDirection.North返却。AI自己参照攻撃等のエッジケースで不正な方向が返される | 低 | `Position.cs:66-82` | |

### FD: SymbolMap通行可能設定・タイル表示重複

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| FD-1 | TileType.SymbolMountainがBlocksMovement=falseで山岳地形を通行可能。ワールドマップの移動制限が機能せずゲームマップの地形概念が崩壊 | 致命的 | `Tile.cs:432-437` | |
| FD-2 | TileType.SymbolWaterがBlocksMovement=falseで水域を通行可能。船/橋なしでの水上移動が可能になりマップ探索の障壁設計が無効化 | 致命的 | `Tile.cs:438-442` | |
| FD-3 | StairsDown('>')とBuildingExit('<')で異なる機能のタイルが視覚的に似た表示文字を使用。プレイヤーが上り階段と建物出口を混同する視覚的UX問題 | 中 | `Tile.cs:486, 522` | |
| FD-4 | NpcTrainer/NpcLibrarianのGetDisplayChar()ケース欠落（DE-1と関連だが直接的なタイル定義レベルの問題）。タイル生成時のデフォルト表示文字が'?'でマップ上の未知NPCとの区別不可 | 高 | `Tile.cs:516-522` | |

### FE: AIビヘイビア・CompositeBehavior常時適用

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| FE-1 | SpiritBehaviorのテレポート実装がMoveRandom()（隣接1マス移動）で代替。設計上のテレポート（マップ内ランダム位置への瞬間移動）とは根本的に異なりSpirit種族の特殊能力が形骸化 | 高 | `AI/Behaviors/RacialBehaviors.cs:211-216` | |
| FE-2 | CompositeBehavior.IsApplicable()が常にtrue返却。内部ビヘイビアの適用可能性チェックをバイパスし、優先度ベースのAIビヘイビア選択ロジックが正しく機能しない可能性 | 中 | `AI/IAIBehavior.cs:66` | |
| FE-3 | PackHuntingBehaviorが群れ狩り（Pack Hunting）の名称だが、実装は通常の単体追跡・攻撃と同一。仲間敵との連携（挟み込み、集団包囲等）のロジックが完全に不在 | 中 | `AI/Behaviors/RacialBehaviors.cs:9-36` | |
| FE-4 | DungeonMap.GetEnvironmentModifier(position, actionType)がactionType引数を完全無視し常にMovementCostを返却。攻撃/魔法/回復等のアクション種別による環境修正が機能しない | 中 | `DungeonMap.cs:203-209` | |

### FF: セーブ復元・レベルアップ装備依存問題

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| FF-1 | Player.RestoreFromSave()でExperienceを復元するがLevel値との整合性バリデーションなし。Level=99/Experience=5等の不正なセーブデータがそのままロードされゲーム状態が矛盾 | 中 | `Player.cs:698-724` | |
| FF-2 | LevelUp()でEffectiveStats（装備ボーナス含む）に基づくHP/MP差分計算。レベルアップ時に一時的に装備を外すとHP増加量が変動するエクスプロイト可能性 | 中 | `Player.cs:366-387` | |
| FF-3 | CreateTransferData()のCarriedGold計算がPlayer.Goldの直接参照だが、転生後のApplyTransferData()でのゴールド適用がAddGold()経由。AddGold()のamount>0ガードでCarriedGold=0時にサイレント無視 | 中 | `Player.cs:484-496, 500-520` | |

### FG: GameConstants一元管理・マジックナンバー散在

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| FG-1 | GameConstants.csに定義された定数（InitialHunger/InitialThirst等）が各Systemファイルで直接数値リテラルとして重複使用。定数変更時に一元反映されず不整合発生リスク | 中 | `GameConstants.cs, 各Systemファイル` | |
| FG-2 | MaxSpellWords=7、ドロップ確率閾値、HP停止閾値30%等のゲームバランスパラメータがGameControllerやSystemファイル内にハードコード。GameConstants.csへの集約が不完全 | 低 | `GameController.cs, 各Systemファイル` | |
| FG-3 | Position(5,5)等のスポーンフォールバック位置がGameConstants.csに定義なく各マップ種別で個別ハードコード（ED-1と関連するが定数管理観点の問題） | 低 | `GameController.cs` | |

### FH: Interfaces設計・IMap未使用パラメータ

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| FH-1 | IMap.GetEnvironmentModifier()インターフェースがTurnActionType引数を定義するが全実装で無視。インターフェース契約違反で将来の実装者に誤解を与える | 低 | `Interfaces/IMap.cs, DungeonMap.cs:203-209` | |
| FH-2 | IMap.cs内のインターフェース定義とInterfaces.cs内の定義が分離。同一名前空間内で2ファイルにインターフェースが散在し発見性が低下 | 低 | `Interfaces/IMap.cs, Interfaces/Interfaces.cs` | |

### FI: EnemyFactory生成・Enum定義粒度不整合

| ID | 不整合の詳細 | 重要度 | ソースコード箇所 | 修正状況 |
|----|-------------|--------|-----------------|----------|
| FI-1 | EnemyFactory.CreateEnemyForFloor()のフロア深度に対する敵種選択で一部MonsterRace（Construct/Plant）のフロア出現条件が定義されずnullフォールバック発生リスク | 中 | `EnemyFactory.cs` | |
| FI-2 | Enums.csに定義されたGameCommand列挙型（25値）が完全未使用（CL-1と関連するがEnum定義粒度の設計問題）。25個のコマンド定義がデッドコードとしてメモリとリフレクションに影響 | 低 | `Enums/Enums.cs` | |
| FI-3 | MonsterRace/ItemType/CharacterClass等の列挙型がEnums.csと各エンティティファイルで重複定義の可能性。DefinitionsとEnumsの境界が曖昧で保守性低下 | 低 | `Enums/Enums.cs, Entities/Enemy.cs` | |

|---------|--------|---|---|---|---------|------|
| A: 商人・ショップ | 0 | 4 | 4 | 0 | 1 | 9 |
| B: アイテム・消耗品 | 6 | 6 | 1 | 0 | 0 | 13 |
| C: 装備・ステータス | 0 | 4 | 2 | 0 | 0 | 6 |
| D: 敵・ダンジョン | 0 | 0 | 4 | 1 | 0 | 5 |
| E: NPC対話・テキスト | 0 | 0 | 3 | 0 | 0 | 3 |
| F: 宗教システム | 0 | 0 | 2 | 1 | 0 | 3 |
| G: テリトリー | 0 | 0 | 3 | 0 | 0 | 3 |
| H: ペットシステム | 0 | 0 | 0 | 1 | 0 | 1 |
| I: ArmorType | 0 | 0 | 0 | 1 | 0 | 1 |
| J: 経験値・レベルカーブ | 1 | 1 | 1 | 0 | 0 | 3 |
| K: 戦闘バランス | 0 | 1 | 4 | 0 | 0 | 5 |
| L: ショップ経済 | 0 | 2 | 0 | 0 | 0 | 2 |
| M: セーブデータ | 2 | 0 | 1 | 0 | 0 | 3 |
| N: インベントリ・装備 | 2 | 0 | 1 | 0 | 0 | 3 |
| O: スキル・転職 | 0 | 2 | 0 | 0 | 0 | 2 |
| P: マップ表示 | 0 | 2 | 0 | 0 | 0 | 2 |
| Q: タイルドキュメント | 0 | 0 | 1 | 0 | 0 | 1 |
| R: 建設・投資 | 0 | 2 | 0 | 0 | 0 | 2 |
| S: ステータスシステム | 0 | 0 | 2 | 0 | 0 | 2 |
| T: Enum未使用値 | 0 | 1 | 2 | 0 | 0 | 3 |
| U: 未使用食材タイプ | 0 | 1 | 0 | 0 | 0 | 1 |
| V: 魔法・呪文システム | 2 | 2 | 2 | 0 | 0 | 6 |
| W: クラフト・料理素材 | 5 | 0 | 0 | 0 | 0 | 5 |
| X: ポーション効果値 | 0 | 4 | 0 | 0 | 0 | 4 |
| Y: 種族特性の未実装 | 4 | 0 | 2 | 0 | 0 | 6 |
| Z: 背景・コンパニオン | 1 | 1 | 0 | 0 | 0 | 2 |
| AA: トラップ・GC不整合 | 1 | 0 | 1 | 0 | 0 | 2 |
| AB: セーブデータ追加欠落 | 5 | 3 | 1 | 0 | 0 | 9 |
| AC: クエストシステム | 3 | 1 | 1 | 0 | 0 | 5 |
| AD: 実績・エンディング | 2 | 0 | 1 | 0 | 0 | 3 |
| AE: マップ生成・タイル | 2 | 4 | 0 | 0 | 1 | 7 |
| AF: ランダムイベント | 3 | 0 | 0 | 0 | 0 | 3 |
| AG: ギャンブルシステム | 0 | 1 | 1 | 0 | 0 | 2 |
| AH: Engine層Enum/計算 | 2 | 3 | 0 | 0 | 0 | 5 |
| AI: ダメージ計算 | 0 | 0 | 2 | 0 | 0 | 2 |
| AJ: デッドコードシステム | 0 | 2 | 1 | 0 | 0 | 3 |
| AK: カルマ・無限D永続性 | 1 | 2 | 0 | 0 | 0 | 3 |
| AL: 天候効果の未適用 | 1 | 2 | 0 | 0 | 0 | 3 |
| AM: 釣り・採掘ID不一致 | 2 | 2 | 0 | 0 | 0 | 4 |
| AN: ルーン・エンチャント | 2 | 2 | 0 | 0 | 0 | 4 |
| AO: ボスフロア・報酬 | 2 | 2 | 0 | 0 | 0 | 4 |
| AP: 建設・サウンド未接続 | 1 | 0 | 1 | 0 | 0 | 2 |
| AQ: パズル・ターンカウンタ | 0 | 0 | 2 | 0 | 0 | 2 |
| AR: スキル・パッシブ・状態異常 | 3 | 2 | 3 | 0 | 0 | 8 |
| AS: セーブデータ詳細欠落 | 2 | 1 | 2 | 0 | 0 | 5 |
| AT: 休息・回復メカニクス | 0 | 2 | 1 | 1 | 0 | 4 |
| AU: 移動コスト・地形効果 | 0 | 1 | 2 | 0 | 0 | 3 |
| AV: 時間帯・渇き・疲労・衛生 | 0 | 5 | 1 | 0 | 0 | 6 |
| AW: 誓約・信仰・評判 | 1 | 2 | 2 | 0 | 0 | 5 |
| **AX: コンパニオンシステム** | **2** | **1** | **1** | **0** | **0** | **4** |
| **AY: ペットシステム未機能** | **2** | **2** | **0** | **0** | **0** | **4** |
| **AZ: 敵AI・行動パターン** | **0** | **1** | **2** | **0** | **0** | **3** |
| **BA: 対話・百科事典・イベント** | **0** | **1** | **3** | **0** | **0** | **4** |
| **BB: ダンジョン生成・部屋タイプ** | **1** | **5** | **4** | **0** | **0** | **10** |
| **BC: 死亡・ゲームオーバー** | **1** | **3** | **3** | **0** | **0** | **7** |
| **BD: キーバインド・UI・難易度** | **2** | **2** | **0** | **0** | **0** | **4** |
| **BE: パーティ・隊列システム** | **0** | **2** | **0** | **0** | **0** | **2** |
| **BF: ランダムイベントクールダウン** | **0** | **0** | **1** | **0** | **0** | **1** |
| **BG: バフ・デバフ・状態異常** | **2** | **4** | **4** | **1** | **0** | **11** |
| **BH: 転職・職業・スキル** | **2** | **2** | **1** | **0** | **0** | **5** |
| **BI: ショップ経済・投資・通貨** | **0** | **5** | **3** | **1** | **0** | **9** |
| **BJ: ワールドマップ・テリトリー** | **1** | **4** | **3** | **2** | **0** | **10** |
| **BK: 正気度システム** | **0** | **2** | **2** | **0** | **0** | **4** |
| **BL: カルマシステム** | **0** | **3** | **1** | **0** | **0** | **4** |
| **BM: 評判システム** | **0** | **2** | **1** | **0** | **0** | **3** |
| **BN: NPC関係性・好感度** | **1** | **2** | **1** | **0** | **0** | **4** |
| **BO: RelationshipSystem未使用** | **1** | **0** | **0** | **0** | **0** | **1** |
| **BP: 価格計算の重複** | **0** | **1** | **1** | **0** | **0** | **2** |
| **BQ: セーブ永続性(システム)** | **4** | **4** | **14** | **1** | **0** | **24** |
| **BR: セーブ永続性(プレイヤー)** | **2** | **2** | **2** | **0** | **0** | **6** |
| **BS: Engine層戦闘計算** | **1** | **10** | **6** | **3** | **0** | **20** |
| **BT: アイテム生成・エンチャント** | **6** | **4** | **4** | **3** | **0** | **17** |
| **BU: 時間・チュートリアル・実績** | **1** | **4** | **6** | **3** | **0** | **17** |
| **BV: マップ生成・フロア遷移** | **2** | **3** | **4** | **1** | **0** | **10** |
| **BW: 転生・NG+・エンディング** | **0** | **6** | **1** | **0** | **0** | **7** |
| **BX: インベントリ・装備・重量詳細** | **2** | **4** | **3** | **1** | **0** | **10** |
| **BY: 魔法・呪文・言語詳細** | **6** | **5** | **3** | **1** | **0** | **16** |
| **BZ: クエスト・ギルド・派閥詳細** | **4** | **5** | **3** | **2** | **0** | **15** |
| **CA: セーブデータ・シリアライズ詳細** | **3** | **3** | **2** | **0** | **0** | **8** |
| **CB: 状態異常・バフ・デバフ詳細** | **4** | **3** | **5** | **1** | **0** | **14** |
| **CC: ペット・コンパニオン・召喚詳細** | **2** | **7** | **5** | **0** | **0** | **15** |
| **CD: 難易度・バランス・設定不整合** | **2** | **4** | **2** | **0** | **0** | **9** |
| **CE: チュートリアル・ヘルプ・UIフロー** | **2** | **1** | **6** | **6** | **0** | **16** |
| **CF: 天候・視界・環境効果詳細** | **0** | **2** | **4** | **0** | **0** | **6** |
| **CG: 採集・釣り・採掘ノード詳細** | **1** | **2** | **2** | **0** | **0** | **5** |
| **CH: 傷・疾病・ステルスシステム詳細** | **1** | **3** | **3** | **0** | **0** | **7** |
| **CI: ポーション・巻物・アクセサリ効果詳細** | **0** | **2** | **5** | **2** | **0** | **9** |
| **CJ: 行動コスト・自動探索・メッセージ詳細** | **0** | **3** | **3** | **2** | **0** | **8** |
| **CK: 百科事典・実績・識別・投資詳細** | **0** | **0** | **5** | **1** | **0** | **6** |
| **CL: Enum未使用値・重複定義** | **0** | **3** | **2** | **1** | **0** | **6** |
| **CM: セーブデータ永続性（追加33システム未保存）** | **3** | **4** | **0** | **1** | **0** | **8** |
| **CN: スキルBasePower=0問題** | **0** | **7** | **1** | **0** | **0** | **8** |
| **CO: マップ生成・階段配置致命的欠陥** | **3** | **0** | **2** | **0** | **0** | **5** |
| **CP: 戦闘計算致命的不整合** | **4** | **3** | **0** | **0** | **0** | **7** |
| **CQ: クエスト報酬・宗教スキル欠落** | **2** | **3** | **1** | **0** | **0** | **6** |
| **CR: ランダムイベント・ワールドマップ欠落** | **3** | **1** | **0** | **0** | **0** | **4** |
| **CS: 誓約・タブー・実効果未実装** | **0** | **2** | **2** | **0** | **0** | **4** |
| **CT: 空腹ダメージ・攻撃タイプバランス** | **0** | **2** | **2** | **0** | **0** | **4** |
| **CU: アイテム生成・Create()null返却** | **0** | **1** | **1** | **1** | **0** | **3** |
| **CV: 呪文ダメージタイプ・回復計算不整合** | **2** | **2** | **2** | **0** | **0** | **6** |
| **CW: コンパニオンSupportMode・ペット被ダメージ不在** | **2** | **2** | **1** | **0** | **0** | **5** |
| **CX: ギルドランク報酬・派閥評判追跡不在** | **0** | **3** | **3** | **0** | **0** | **6** |
| **CY: クラフト素材消費・強化曲線不整合** | **1** | **1** | **2** | **1** | **0** | **5** |
| **CZ: ショップ在庫・建設効果・領地価格不整合** | **0** | **4** | **2** | **0** | **0** | **6** |
| **DA: ターン処理・時間経過スキップ** | **3** | **3** | **0** | **0** | **0** | **6** |
| **DB: カルマ閾値・評判効果デッドコード** | **0** | **3** | **2** | **0** | **0** | **5** |
| **DC: 死亡ログ未記録・ゲームオーバーフロー不備** | **2** | **2** | **2** | **0** | **0** | **6** |
| **DD: 自動探索・パス計算不完全** | **1** | **2** | **2** | **0** | **0** | **5** |
| **DE: タイル表示文字・ボス部屋生成エッジケース** | **1** | **2** | **2** | **0** | **0** | **5** |
| **DF: マルチスロットセーブ・ゲームクリアスコア** | **2** | **2** | **2** | **0** | **0** | **6** |
| **DG: ゲームオーバー・エンディング分岐** | **0** | **2** | **3** | **1** | **0** | **6** |
| **DH: 描画最適化・ビューポート計算エラー** | **0** | **3** | **2** | **0** | **0** | **5** |
| **DI: 百科事典・MOD検証・ヘルプシステム** | **0** | **3** | **2** | **1** | **0** | **6** |
| **DJ: アクセシビリティ・色覚補正・ゲーム速度** | **0** | **1** | **3** | **1** | **0** | **5** |
| **DK: 成長システム・レベル1ステータス欠如** | **0** | **2** | **2** | **1** | **0** | **5** |
| **DL: フラグ条件解析・イベント確率不整合** | **1** | **0** | **4** | **0** | **0** | **5** |
| **DM: 難易度パラメータ矛盾・Ironman設計不備** | **0** | **1** | **4** | **0** | **0** | **5** |
| **DN: 開始マップ解決・テンプレートマップ** | **0** | **0** | **2** | **2** | **0** | **4** |
| **DO: 無限ダンジョンスコア・セーブスロットソート** | **0** | **2** | **2** | **1** | **0** | **5** |
| **DP: 処刑システム・武器アニメーション欠落** | **1** | **1** | **2** | **1** | **0** | **5** |
| **DQ: ミミック・モンスター種族・属性矛盾** | **0** | **2** | **2** | **2** | **0** | **6** |
| **DR: マルチクラス・鍛冶結果型不整合** | **1** | **0** | **3** | **1** | **0** | **5** |
| **DS: 罠二重定義・ギャンブル中毒ロジック** | **0** | **1** | **2** | **2** | **0** | **5** |
| **DT: 能力値フラグ・方向補正・熟練度曲線** | **0** | **2** | **2** | **1** | **0** | **5** |
| **DU: ダンジョン生成パラメータ・環境戦闘・秘密部屋** | **0** | **2** | **3** | **0** | **0** | **5** |
| **DV: 自動探索・碑文リセット・状態異常命名** | **0** | **0** | **2** | **3** | **0** | **5** |
| **DW: GrowthSystem二重管理・DungeonFaction境界値** | **2** | **0** | **3** | **1** | **0** | **6** |
| **DX: 戦闘計算・魔法防御係数・クリティカル不整合** | **0** | **2** | **2** | **1** | **0** | **5** |
| **DY: アイテム等級・ドロップ・素材品質未機能** | **0** | **1** | **2** | **2** | **0** | **5** |
| **DZ: GameController計算結果破棄・戻り値無視** | **1** | **0** | **4** | **0** | **0** | **5** |
| **EA: AIビヘイビア・Spirit疑似テレポート** | **0** | **0** | **0** | **2** | **0** | **2** |
| **EB: AddGold(-負値)サイレント失敗** | **5** | **2** | **0** | **0** | **0** | **8** |
| **EC: 死亡プレイヤーアクション制御不備** | **1** | **3** | **1** | **0** | **0** | **5** |
| **ED: スポーン位置ハードコードフォールバック** | **2** | **2** | **1** | **0** | **0** | **5** |
| **EE: StatusEffects.First()例外リスク** | **0** | **1** | **1** | **0** | **0** | **2** |
| **EF: AudioManager Dispose不完全** | **0** | **2** | **0** | **1** | **0** | **3** |
| **EG: SaveManager例外ハンドリング欠落** | **0** | **1** | **2** | **0** | **0** | **3** |
| **EH: Damage抵抗値負値ダメージ増幅** | **0** | **1** | **1** | **1** | **0** | **3** |
| **EI: ObjectPool非アトミックサイズチェック** | **0** | **0** | **1** | **1** | **0** | **2** |
| **EJ: クエスト状態遷移検証不備** | **0** | **0** | **2** | **1** | **0** | **3** |
| **EK: SkillDatabase null-forgiving演算子** | **0** | **0** | **1** | **1** | **0** | **2** |
| **EL: CraftingSystem素材消費アトミシティ違反** | **3** | **0** | **1** | **1** | **0** | **5** |
| **EM: PetSystem null-forgiving・死亡ペット騎乗** | **1** | **1** | **4** | **1** | **0** | **7** |
| **EN: FishingSystem宝箱・確率超過** | **0** | **2** | **2** | **1** | **0** | **5** |
| **EO: StatusEffect MaxStack未強制・永続呪い** | **0** | **3** | **3** | **1** | **0** | **7** |
| **EP: EnchantmentSystem ExpBoost/DropBoostデッドコード** | **0** | **2** | **2** | **0** | **0** | **4** |
| **EQ: ReputationSystem閾値非対称・IsWelcome緩さ** | **0** | **0** | **2** | **2** | **0** | **4** |
| **ER: SkillSystem パッシブ重複・クールダウン永続** | **0** | **0** | **3** | **1** | **0** | **4** |
| **ES: WeatherSystem RangedHitModifier適用方式不統一** | **0** | **1** | **2** | **1** | **0** | **4** |
| **ET: GameController料理品質・失敗チェック未実装** | **0** | **2** | **1** | **0** | **0** | **3** |
| **EU: TerritoryInfluenceSystem正規化前クランプ** | **0** | **0** | **3** | **0** | **0** | **3** |
| **EV: CompanionSystem最小1ダメージ貫通** | **0** | **1** | **2** | **0** | **0** | **3** |
| **EW: Inventory重量制限未強制・スタック整理不備** | **0** | **3** | **2** | **0** | **0** | **5** |
| **EX: Player生存ステータス初期化不整合・Sanity境界** | **0** | **2** | **2** | **1** | **0** | **5** |
| **EY: Character基底リソース管理・負値消費バイパス** | **0** | **0** | **4** | **0** | **0** | **4** |
| **EZ: ItemFactory乱数生成・レアリティ適用不備** | **0** | **3** | **1** | **0** | **0** | **4** |
| **FA: Equipment呪い解除・防具スロット判定不整合** | **0** | **2** | **2** | **0** | **0** | **4** |
| **FB: Consumable腐敗食料・ポーション効果値ゼロ** | **0** | **1** | **3** | **0** | **0** | **4** |
| **FC: Damage抵抗計算・Stats CriticalRate偏重** | **0** | **0** | **2** | **2** | **0** | **4** |
| **FD: SymbolMap通行可能設定・タイル表示重複** | **2** | **1** | **1** | **0** | **0** | **4** |
| **FE: AIビヘイビア・CompositeBehavior常時適用** | **0** | **1** | **3** | **0** | **0** | **4** |
| **FF: セーブ復元・レベルアップ装備依存問題** | **0** | **0** | **3** | **0** | **0** | **3** |
| **FG: GameConstants一元管理・マジックナンバー散在** | **0** | **0** | **1** | **2** | **0** | **3** |
| **FH: Interfaces設計・IMap未使用パラメータ** | **0** | **0** | **0** | **2** | **0** | **2** |
| **FI: EnemyFactory生成・Enum定義粒度不整合** | **0** | **0** | **1** | **2** | **0** | **3** |
| **合計** | **163** | **303** | **318** | **85** | **2** | **882** |

---

## 修正方針（ユーザー判断待ち）

ユーザーが各項目の「修正判断」欄に ✅（修正する）/ ❌（修正しない）/ 🔄（保留）を記入後、
確定した修正対象をまとめて実装する。

### 修正優先度の目安
1. **致命的**（163件）: 使用するとクラッシュ/データ消失/機能しない → 最優先で修正推奨
2. **高**（303件）: 設計と実装の明確な乖離 → 修正推奨
3. **中**（318件）: 違和感・バランス問題 → 選択的に修正
4. **低**（85件）: 軽微なテーマ不一致 → 余裕があれば修正
5. **設計課題**（2件）: アーキテクチャ改善 → 長期検討

### 新規追加カテゴリの概要（第2回調査分）
- **J: 経験値カーブ** — 1.5倍指数関数により実質Lv20が上限。推奨レベルが到達不能
- **K: 戦闘バランス** — 魔法防御が弱すぎ、クリティカル率の不整合、ボスHP式がハードコード
- **L: ショップ経済** — 売買差額80%損失、ポーション回復量スケーリングなし
- **M: セーブデータ** — 渇き/疲労/衛生/コンパニオンがセーブされない
- **N: インベントリ・装備** — 重量チェックなし、両手武器装備時のオフハンド消失バグ
- **O: スキル・転職** — スキルポイント経済なし、転職時ステータス再計算なし
- **P: マップ表示** — NpcTrainer/NpcLibrarianの表示文字未定義
- **Q: タイルドキュメント** — 山岳・水域の「通行不可」コメントと実装の矛盾
- **R: 建設・投資** — 食料自動生産・投資配当が呼び出されない
- **S: ステータスシステム** — 渇きダメージ値の列挙型間不整合、信仰ペナルティ非累積

### 新規追加カテゴリの概要（第3回調査分）
- **T: Enum未使用値** — ExtendedItemCategory全8値未使用、SkillTarget味方対象未使用、TradeRouteStatus未使用
- **U: 未使用食材タイプ** — FoodType 5値（RawMeat/RawFish等）にアイテム生成なし
- **V: 魔法・呪文システム** — ルーン語"loka"未分類、Poison元素マッピング欠落、状態異常4種のファクトリ欠落、berserkバフ効果値逆転
- **W: クラフト・料理素材** — 石炭/革/生肉/魔法エッセンス/魚/塩の6素材がItemFactoryに存在せず全レシピ作成不可
- **X: ポーション効果値** — 透明化薬/万能薬/耐火薬/耐冷薬のEffectValue未定義
- **Y: 種族特性の未実装** — スライム分裂/デーモン魔力吸収/堕天使浮遊/スライム装備制限が定義のみで不活性
- **Z: 背景・コンパニオン** — 全10職歴の初期装備未適用、CompanionSystem未接続
- **AA: トラップ・GC不整合** — TriggerTrap()に5種のトラップケース欠落、14箇所のマジックナンバー

### 新規追加カテゴリの概要（第4回調査分）
- **AB: セーブデータ追加欠落** — ペット/戦闘構え/状態異常/習熟度/病気/傷/祈りフラグ/武器熟練度の9システムがセーブ非対応。Thirst/Fatigue/Hygieneが常に最大値でリセット
- **AC: クエストシステム** — enemy_rat/enemy_banditがEnemyFactory未定義でクエスト完了不可、item_herbのID不一致、NPC名/ID混在
- **AD: 実績・エンディング** — AchievementSystemに実績0件登録、放浪者エンディングのパラメータがfalseハードコードで達成不可能
- **AE: マップ生成・タイル** — Altar/Fountainにインタラクションなし、Library/Prison/Storage部屋未生成、FeaturePlacer未使用、21種のTileColorマッピング欠落
- **AF: ランダムイベント** — NpcEncounter/MerchantEncounter/AmbushEventの3イベントにResolveハンドラなし
- **AG: ギャンブルシステム** — GetLuckBonus()未呼び出し、カードゲームのハウスエッジが他ゲームの約2倍
- **AH: Engine層Enum/計算** — ResourceSystemがCore層と異なるクラス名使用（6クラスのHP/MP成長率が不正）、PlayerLevel=0でゼロ除算、呪文DamageType三項演算子が常にMagical
- **AI: ダメージ計算** — DamageType.Pure/Healingの明示的処理なし、レベル99+の経験値テーブルオーバーフロー

### 新規追加カテゴリの概要（第5回調査分）
- **AJ: デッドコードシステム** — MerchantGuildSystem/TerritoryInfluenceSystem/ModLoaderSystemの3システムが完全なデッドコード。初期化・リセットのみで機能メソッド未呼び出し
- **AK: カルマ・無限D永続性** — KarmaSystemのカルマ値（ショップ価格・NPC態度に影響）がセーブ非対応。無限ダンジョンモード・建設済み施設もロード時消失
- **AL: 天候効果の未適用** — WeatherSystemの戦闘ダメージ修正/移動コスト修正/視界修正の3種がGameControllerから呼ばれず表示テキストのみ
- **AM: 釣り・採掘ID不一致** — FishingSystemの全9魚/GatheringSystemの全5鉱石のIDがItemFactoryに未定義。釣り・採掘の成果物が生成不能
- **AN: ルーン・エンチャント** — プレイヤーにルーン学習フィールドなし、エンチャントデータがItem/SaveDataに未永続化、ソウルジェムがItemFactory未定義
- **AO: ボスフロア・報酬** — ボスフロアで階段制限なし（ボス戦スキップ可能）、クエスト報酬ItemIdsが処理されず消失
- **AP: 建設・サウンド未接続** — BaseConstructionSystem.Build()未呼び出しで全施設建設不可、AmbientSoundの再生システムなし
- **AQ: パズル・ターンカウンタ** — EnvironmentalPuzzleがNPC対話のみでダンジョン配置なし、TurnCountがintでオーバーフローリスク

### 新規追加カテゴリの概要（第6回調査分）
- **AR: スキル・パッシブ・状態異常** — パッシブスキル（weapon_mastery/hp_boost等）が定義のみで効果ゼロ、TickStatusEffects()未呼び出しでバフ永続化、Charm/Madness状態異常の行動制限未実装、誓約違反チェック未呼び出し
- **AS: セーブデータ詳細欠落** — Item.Grade（品質）未保存でレア品がStandardに降格、地面アイテム全消失、マップ探索状態リセット、セーブバージョン移行ロジックなし
- **AT: 休息・回復メカニクス** — 休息時MP/Sanity回復が未適用（取得のみ）、休息中の食料/水消費なし
- **AU: 移動コスト・地形効果** — 地形別移動コスト（森1.5/山2.0等）がTileに定義済みだが移動計算で無視、斜め移動/重量超過ペナルティがint切り捨てで無効化
- **AV: 時間帯・渇き・疲労・衛生効果** — 時間帯の視界/スポーン率修正が未適用、渇き/疲労/衛生のステータスペナルティが未適用（表示テキストのみ）
- **AW: 誓約・信仰・評判** — 誓約ボーナス（経験値/ドロップ率）未適用、誓約破棄ペナルティなし、信仰の献金メカニクス未実装、評判によるクエスト解放未使用

### 新規追加カテゴリの概要（第7回調査分）
- **AX: コンパニオンシステム** — コンパニオンが被ダメージゼロで不死身、ステータス成長なし、AIモード変更不可、死亡時の通知なし
- **AY: ペットシステム未機能** — Feed()/Train()/TickHunger()未呼び出し、6種の特殊能力が全てテキスト定義のみ、騎乗速度ボーナス未適用、忠誠度が装飾
- **AZ: 敵AI・行動パターン** — ボスのバーサーク状態が無効果、SummonerBehaviorが完全なデッドコード、敵の逃走行動が確認不能
- **BA: 対話・百科事典・イベント** — 16体NPCの対話IDが未登録、シンボルマップイベント10種が未トリガー、10階チュートリアル未発火、百科事典の閲覧UI不在
- **BB: ダンジョン生成・部屋タイプ** — 4つのRoomType未生成、9テーマのパラメータ未適用、Water/Lavaタイル未配置、宝箱開封不可、祠バフ未接続、無限ダンジョンマイルストーン未実装
- **BC: 死亡・ゲームオーバー** — Ironmanモードでセーブ未削除、4種の死因メッセージ未定義、状態異常死後のHP0チェック欠如、死亡統計未表示、復活アイテム未実装
- **BD: キーバインド・UI・難易度** — Tキー重複バインド、2アクションのキー未定義、難易度設定の80%の乗数が未使用
- **BE: パーティ・隊列システム** — 隊列システム自体が存在しない、パーティ全体回復/バフ伝播なし
- **BF: ランダムイベントクールダウン** — イベント連続発生防止メカニズムなし

### 新規追加カテゴリの概要（第8回調査分）
- **BG: バフ・デバフ・状態異常** — バフ乗数（AttackMultiplier/DefenseMultiplier/AllStatsMultiplier）が完全未適用、状態異常がセーブに含まれない、耐性チェックがPoison免疫のみ、装備エンチャントバフ空実装、祝福/呪い実効果ゼロ
- **BH: 転職・職業・スキル** — Player.CharacterClassがprivate setで変更不可（転職の根本障害）、6パッシブスキル効果未実装、職業制限なしで全スキル習得可能
- **BI: ショップ経済・投資・通貨** — 在庫ランダム性ゼロ/リフレッシュなし、InvestmentSystem完全未呼出、BaseConstruction食料生産未適用、PriceFluctuation修飾子がBuy/Sell未適用（UI表示のみ）
- **BJ: ワールドマップ・テリトリー** — テリトリー固有イベント未トリガー、旅行イベント解決ハンドラなし、テリトリー間の敵/アイテム差異なし、霧の向こう機能なし、探索XP/報酬なし
- **BK: 正気度システム** — 正気度が戦闘パフォーマンス無影響、休息の正気度回復未適用、回復アイテム/幻覚エフェクト未実装
- **BL: カルマシステム** — GetShopPriceModifier/GetNpcDispositionModifier/CanEnterHolyGround全てデッドコード、カルマ変動トリガーが3箇所のみ
- **BM: 評判システム** — GetShopDiscount/IsWelcome全てデッドコード、評判低下メカニクス不在
- **BN: NPC関係性・好感度** — 好感度データがRebirth時に消失（CreateTransferData未呼出）、好感度変動が対話2箇所のみ、好感度ランクによる対話分岐なし
- **BO: RelationshipSystem未使用** — 全メソッドが完全デッドコード（初期化/リセットのみ）
- **BP: 価格計算の重複** — 3システムの価格メソッドがPriceFluctuationSystemに重複、エンカウント率がカルマ/評判無関係

### 新規追加カテゴリの概要（第9回調査分）
- **BQ: セーブデータ永続性（システムステート大量未保存）** — Karma/Reputation/Companion/SkillTree/Pet/Achievement/Encyclopedia/BaseConstruction等24システムのステートがSaveDataに含まれない。ロード時に全進捗消失
- **BR: セーブデータ永続性（プレイヤー/ゲームステート）** — 病気状態/戦闘スタンス/NG+ティア/ゲームクリア状態/無限ダンジョンモード/ダンジョン特性がセーブされない
- **BS: Engine層戦闘計算不整合** — 武器ダメージレンジ未参照/両手武器ボーナスなし/防御貫通なし/クリティカル上限固定/盾ブロックなし/オフハンド攻撃なし/レベルスケーリングなし
- **BT: アイテム生成・エンチャント・クラフト不整合** — 装備生成が4種のみ/消耗品生成が4種のみ/エンチャント効果が実際に適用されない/8エンチャント種が効果ゼロ/クラフト素材定義欠落
- **BU: 時間・デイリー処理・チュートリアル・実績** — 日変更検出なし/宗教デイリーティック未呼出/ペット空腹ティック未呼出/難易度ExpMultiplier未適用/実績セーブなし/チュートリアルスキップ不可

### 新規追加カテゴリの概要（第10回調査分）
- **BV: マップ生成・フロア遷移** — 階段配置失敗時のフォールバック欠落/キャッシュ復元時の位置検証なし/到達性チェックがトラップ配置前/NG+でフロアキャッシュ未クリア/モンスター階段上配置
- **BW: 転生・NG+・エンディング** — NG+キャリーオーバー未適用/敵スケーリング未適用/Wandererエンディング到達不可能/テリトリー訪問追跡なし/TransferDataにLevel/Gold欠落/転生ボーナス未呼出
- **BX: インベントリ・装備・重量詳細** — 重量制限未強制/盾+両手武器同時装備可/オフハンド装備消失/ArmorSpeedModifier未適用/壊れた装備が装備可能/過重量ペナルティなし/装備比較未実装
- **BY: 魔法・呪文・言語詳細** — AllAlliesターゲット全4呪文種で無視/Object/Groundターゲット未処理/回復呪文Self以外未対応/敵魔法耐性ゼロ固定/巻物使用不可/テレポート射程語無視/初期語「vinir」単体無効
- **BZ: クエスト・ギルド・派閥詳細** — クエストアイテム報酬未配布/Deliver/Talk/Escort未処理/前提条件なし/MerchantGuild・FactionWar完全未公開/ギルドランク効果なし/派閥選択装飾のみ/Failed状態未使用

### 新規追加カテゴリの概要（第11回調査分）
- **CA: セーブデータ・シリアライズ詳細** — Thirst/Fatigue/Hygiene/StatusEffects/FacingDirectionがセーブ非対応。SaveDataバージョンマイグレーションなし。ターン処理中のセーブで中間状態キャプチャ。SaveManager I/Oエラー未処理。MultiSlotSaveSystem未連携
- **CB: 状態異常・バフ・デバフ詳細** — 5種StatusEffectType(Slow/Vulnerability/Invisibility/Blessing/Apostasy)のファクトリ欠落。CureAllが22種中9種のみ解除。ボス免疫チェック未呼出。矛盾効果同時適用可。Vulnerability/Invisibility効果ゼロ。出血が移動ごとでなくターンごと
- **CC: ペット・コンパニオン・召喚詳細** — ペット戦闘アクション全未実装。死亡/餓死/経験値/装備/レベルアップ全なし。コンパニオン復活不可・スキル未使用・SupportMode空実装。召喚永続。TickHunger/CheckDesertion未呼出。ペット上限なし
- **CD: 難易度・バランス・設定不整合** — DifficultyConfigの6種乗数(Exp/Hunger/Damage×2/ItemDrop/EnemyStat)が実際の計算に未適用。AudioManager設定未接続。Ironman UI説明と実値の不一致。レベルキャップ報酬なし
- **CE: チュートリアル・ヘルプ・UIフロー** — T キー競合(ThrowItem/EnterTown)。ContextHelp未初期化。ヘルプキーバインドなし。チュートリアル再表示不可。ゲームオーバーにリスタートなし。設定変更のライブ反映なし。名前バリデーション不足。メッセージログ切り捨て無通知

### 新規追加カテゴリの概要（第12回調査分）
- **CF: 天候・視界・環境効果詳細** — WeatherSystem天候修飾子が戦闘/移動計算に未適用。FOVと戦闘LOSが独立計算。室内/室外天候区別なし。光源がFOV半径に影響なし
- **CG: 採集・釣り・採掘ノード詳細** — TileにGatheringNodeフィールドなし。DungeonGeneratorにPlaceGatheringNodes()なし。採集熟練度フィールドなし。道具要件未実装。ドロップテーブル非連携
- **CH: 傷・疾病・ステルスシステム詳細** — 疾病がSaveDataに非保存。BodyConditionSystem傷定義がPlayerに未追跡。StealthSystem.cs未存在。敵AI検出メカニクスなし。疾病感染源未紐付
- **CI: ポーション・巻物・アクセサリ効果詳細** — 6種ポーション/2種巻物のUse()ケース欠落。アクセサリPassiveAbility/ActivatedSkill非参照。Equipment.Effects未適用。識別巻物が実際に識別しない。料理熟練度なし
- **CJ: 行動コスト・自動探索・メッセージ詳細** — UseInn/VisitChurchのactionCost未設定。自動探索に空腹/渇きチェックなし。メッセージ優先度なし。フィールドマップで町イベント発火。ボス配置フォールバック
- **CK: 百科事典・実績・識別・投資詳細** — Encyclopedia/AchievementSystemがSaveData非保存。SpellParserルーン組合せ未検証。ルーン習熟度逆相関。Investment支払い未実装。レシピ発見システムなし

### 新規追加カテゴリの概要（第13回調査分）
- **CL: Enum未使用値・重複定義** — GameCommand列挙型(25値)が完全未使用。ItemType/CharacterClassがEnums.csと別ファイルで重複定義。SkillTarget.SingleAlly/AllAlliesが未使用でパーティスキル不能。NpcType7値(Bard/Trainer等)が未参照
- **CM: セーブデータ永続性（追加33システム未保存）** — NG+ティア/ゲームクリア状態/クリアランク/無限ダンジョンモード・キル数がセーブされない。surfaceMap/詠唱状態/ナビゲーション状態/建物内コンテキストもロード後に消失
- **CN: スキルBasePower=0問題** — 挑発/瞑想/魔法障壁/浄化/祝福/死霊召喚/呪詛/鼓舞の歌/子守唄/魅了/付与/変成の12スキルがBasePower=0で実質効果なし。解毒剤にEffectValue未定義
- **CO: マップ生成・階段配置致命的欠陥** — PlaceStairs()失敗時のフォールバックなし(void返却で検出不可)。上り階段昇降時にStairsDownPosition=nullでプレイヤー位置未設定。HasLineOfSight無限ループリスク
- **CP: 戦闘計算致命的不整合** — XP三重付与バグ(base+religion+NG+)。CheckCritical(DEX/LUK版)がデッドコード。maxHp=0で除算ゼロクラッシュ。攻撃タイプ6種がMagic以外全て同一計算。状態異常が適用されない(true/false返却のみ)
- **CQ: クエスト報酬・宗教スキル欠落** — クエストItemIds/GuildPoints/FaithPointsが配布されない。宗教スキル21/23個が未実装。ReligionBenefit適用メソッドなし。タブー違反チェックなし。棄教呪い期限デクリメントなし
- **CR: ランダムイベント・ワールドマップ欠落** — NpcEncounter/MerchantEncounter/AmbushEventの3種イベント解決ハンドラが未実装。非人型敵のゴールドドロップがゼロ
- **CS: 誓約・タブー・実効果未実装** — OathSystem違反検出が自動化されていない。誓約完了条件(CompleteOath)なし。SP上限未強制。職業別SP消費なし
- **CT: 空腹ダメージ・攻撃タイプバランス** — Starving DamagePerTurn=0(飢餓ダメージなし)。Starvation DamagePerTurn=999(即死)。XPオーバーフロー未チェック。レベルアップ時のステータス即時反映なし
- **CU: アイテム生成・Create()null返却** — ItemFactory.Create()がnull返却可能でNullRefリスク。生成失敗時にStoneフォールバック。NpcMemory転生リセットの設計意図不明確

### 新規追加カテゴリの概要（第14回調査分）
- **CV: 呪文ダメージタイプ・回復計算不整合** — DamageType三項演算子が両分岐Magical固定でエレメント属性無効。回復呪文がダメージ計算式を共用。Summon/Teleportが空実装。威力0呪文が成功扱い。MP消費チェック不十分。Earth/Wind/Water属性相性テーブル未定義
- **CW: コンパニオンSupportMode・ペット被ダメージ不在** — ProcessFollowMode()がテキスト返却のみの空実装。PetSystemにダメージ適用メソッド不在でペット不死身。コンパニオンHP<=0時の死亡処理なし。SupportModeスキル不発動。ペット忠誠度0でもCheckDesertion未呼出
- **CX: ギルドランク報酬・派閥評判追跡不在** — MerchantGuildランクアップ報酬配布なし。DungeonFactionSystemがプレイヤー評判値を追跡しない静的読み取り専用。ギルドランクによるクエストフィルタなし。TotalProfit特典なし。派閥関係値がプレイヤー行動で変化しない。GuildPoints取得がTrade限定
- **CY: クラフト素材消費・強化曲線不整合** — RemoveItem()戻り値無視で素材消費バイパス可能。強化成功率がLv6以降急落し+6が実質上限。強化失敗ペナルティ未定義。CanCraft→Craft間のインベントリ再検証なし。EnhancementLevel初期値保証なし
- **CZ: ショップ在庫・建設効果・領地価格不整合** — テリトリー変更後にショップ再初期化なし。領地別価格倍率がBuy/Sell未適用。食料生産メソッド未呼出。宿屋HP回復に建設ボーナス未適用。評判値引きBuy()未接続。ショップ在庫にランダム性なし
- **DA: ターン処理・時間経過スキップ** — ProcessTurnEffects()が多数アクションで未呼出。StatusEffectsティックスキップでバフ永続化。KarmaSystem.SetCurrentTurn()未呼出。評判減衰メカニズム不在。ペット空腹ティック未呼出。日変更検出未実装
- **DB: カルマ閾値・評判効果デッドコード** — GetShopPriceModifier/GetNpcDispositionModifierが完全デッドコード。カルマ閾値変動通知なし。ModifyKarma()トリガーが3箇所のみ。IsWelcome()がTravelTo()で未チェック
- **DC: 死亡ログ未記録・ゲームオーバーフロー不備** — DeathLogSystem.AddLog()がHandlePlayerDeath()で未呼出。ゲームオーバーUIにリスタート/転生選択肢なし。Ironmanモードでセーブ未削除。状態異常死の死因テキストが汎用のみ。Sanity<=0時のセーブ削除/スコア記録なし。死亡統計閲覧UI不在
- **DD: 自動探索・パス計算不完全** — AutoExploreSystemにFindPath()/GetNextStep()不在。空腹/渇き/疲労の危険閾値停止条件なし。BFS経路探索にイテレーション上限なし。自動探索状態がSaveData未保存。ボスフロア到達時の強制停止条件なし
- **DE: タイル表示文字・ボス部屋生成エッジケース** — NpcTrainer/NpcLibrarianのGetDisplayChar()ケース欠落で'?'表示。単一部屋ダンジョンでボス部屋未設定。ボス部屋選択で部屋サイズ/接続数未検証。Water/Lavaタイルが通常フロアに未配置

### 新規追加カテゴリの概要（第15回調査分）
- **DF: マルチスロットセーブ・ゲームクリアスコア** — SaveTimeがnullableでOrderByソート非決定的。GetOldestSlot()空スロット時null参照リスク。CalculateScore()整数除算でスコア逆転。DetermineRank()タプル順序でランク逆転。DetermineInitialTier()常にPlus1返却。キャリーオーバー非累積
- **DG: ゲームオーバー・エンディング分岐** — Dark/True/Salvationエンディング優先度不整合。Salvation条件(死亡0回)実質到達不能。Wanderer条件がDarkに吸収。転生Sanityコスト未消費。CanRebirth二重判定。無限ダンジョン乗数無上限
- **DH: 描画最適化・ビューポート計算エラー** — CalculateViewport()奇数幅/高さで最端行列切断。IsInViewport境界<=演算子で二重描画。updateFrequency<=0でtrue返却。HUDスケール2.0上限でアクセシビリティ制限
- **DI: 百科事典・MOD検証・ヘルプシステム** — モンスター登録時DiscoveryLevel=1で未遭遇名前判明。killCount=0とレジストリ値矛盾。ParseManifest()常にIsValid=true。パラメータnullチェックなし。ヘルプTake(5)切り捨て。チュートリアル再開不可
- **DJ: アクセシビリティ・色覚補正・ゲーム速度** — FontSize int切り捨て。色変換4色のみで不完全。Protanopia/Tritanopia変換が医学的に不正確。Nightmare rescueCount/permaDeath矛盾。hungerDecay+turnLimit組み合わせで餓死必至
- **DK: 成長システム・レベル1ステータス欠如** — GetLevelBonus(level=1)=0でレベル1成長なし。HP/MPボーナスもlevel-1で0。raceExpMultiplier除算で高倍率種族が有利(逆設計)。int切り捨て蓄積誤差。RollGrowthとGetHpBonusで丸め方式不統一
- **DL: フラグ条件解析・イベント確率不整合** — karma条件パースオフセット不正で条件常時不成立。AND/OR混在非対応。イベント確率合計0.69で31%空振り。randomValue範囲未検証。IncrementFlag負値許容でカウンター不正減少
- **DM: 難易度パラメータ矛盾・Ironman設計不備** — Ironman被ダメ1.2<Nightmare被ダメ1.6で難易度逆転。Nightmare expMultiplier=0.6で実質クリア不可。default=>Normalサイレントフォールバック。GetFlagValue()の0と未設定が区別不能。新条件フラグのサイレント失敗
- **DN: 開始マップ解決・テンプレートマップ** — 新Background/Race enum追加時のサイレントフォールバック。LocaionType.Fieldデッドコード。NG+ Plus5難易度スパイク(1.0ジャンプ)
- **DO: 無限ダンジョンスコア・セーブスロットソート** — floor=0と未挑戦が同一D評価。SaveTimeソート非決定的。TurnBonus 50000ターン以上で差がなくなる。NG+キャリーオーバー転送処理未実装。True/Normalエンディング順序依存

### 新規追加カテゴリの概要（第16回調査分）
- **DP: 処刑システム・武器アニメーション欠落** — GetExecutionAnimationName()が15武器種中6種のみ対応（8種未実装）。GetExecutionKarmaPenalty()が10種族中5種のみ対応。GameOverSystem死因4種未対応。IsFinalBossDefeated()のfloor>=30判定。CalculateScore()整数除算精度低下
- **DQ: ミミック・モンスター種族・属性矛盾** — ミミック報酬倍率が固定1.5倍で階層深度無関係。偽装タイプ3種のみ。Spirit/Amorphous物理耐性が同一0.5f。Ranged/Magic耐性が全種族未定義。SpiritがLight/Dark同時弱点。DarkAffinityトレイト未使用
- **DR: マルチクラス・鍛冶結果型不整合** — ClassTier.Masterの転職条件が完全欠落（到達不可能）。全転職がLevel20固定。サブクラス経験値率が固定0.5f。SmithingSystem.Synthesize()のNewEnhanceLevel=0デフォルト返却。Repair()のSmithingResult未設定
- **DS: 罠二重定義・ギャンブル中毒ロジック** — PlayerTrapType(5種)とTrapType(8種)が別enum・表記不一致。PitfallTrap Damage=15 vs PitFall BaseDamage=10の矛盾。ExplosiveTrap/Poisonの対応なし。CheckAddiction()の正気度影響0.1%/ptで形骸化。期待値コメント精度乖離
- **DT: 能力値フラグ・方向補正・熟練度曲線** — CHA/LUK閾値20 vs 他8能力値閾値25の不均衡。ElevationBonus低所→高所のDamage/Hit減算値が非対称。Unarmed攻撃タイプがWeaponProficiency=BluntとElementalAffinity=Unarmedで矛盾。熟練度経験値が指数関数増加。ハードコード値未一元管理
- **DU: ダンジョン生成パラメータ・環境戦闘・秘密部屋** — DungeonFeatureGeneratorの4パラメータ(Corridor/Special/Water/Lava)が未使用。TrapDensity/TrapChance重複定義。SurfaceInteraction.DamageMultiplier未使用で属性相互作用無効。EnvironmentalPuzzleの知識レベルが成功率に非反映。秘密部屋生成のダンジョンタイプ不均等
- **DV: 自動探索・碑文リセット・状態異常命名** — HP停止閾値30%/満腹度15%の非対称。碑文リセットと能力値フラグ保持のポリシー非対称。StatModifierキー命名不統一(英語複合/略称混在)。berserkのIsBuff=falseと攻撃+50%の矛盾。ContextHelpの複数キー登録vs単一キー検索不整合
- **DW: GrowthSystem二重管理・DungeonFaction境界値** — HP/MPボーナスがGrowthTable+Definition二重管理。DungeonFaction.AreHostile()の>0.5f判定でDragon+Demon=0.5fが敵対されない。AreAllied()の<0.3f判定でUndead+Spirit=0.3fが同盟されない。FactionWar両陣営報酬同一。Duration未使用。TerritoryInfluence Reset後の初期化フロー不明
- **DX: 戦闘計算・魔法防御係数・クリティカル不整合** — 物理防御係数0.5 vs 魔法防御係数0.3でINT投資が圧倒的有利。魔法クリティカル不可（IsCritical=false固定）。DEX/AGI寄与係数の非対称。CheckCritical()オーバーロード混在。暴発ダメージがMP×0.5固定でスケーリング逆転
- **DY: アイテム等級・ドロップ・素材品質未機能** — DropTableSystem.GenerateLoot()がMinGrade無視で全Standard固定。Material.Quality未設定で品質50固定。装備RequiredStatsのLuck/CHA/PER未チェック。Food.HydrationValueとThirst未連携。Scroll.Use()署名でゲーム状態変更不可
- **DZ: GameController計算結果破棄・戻り値無視** — TrySmithRepair()のrepairAmount計算後にdurability加算なし（表示のみ）。ApplySpellDetect()感知結果が反映されない。TrySmuggle()戻り値完全無視で失敗時もターン消費。MaxSpellWords=7固定で深度拡張なし。ドア配置判定が通路交差点を含む
- **EA: AIビヘイビア・Spirit疑似テレポート** — SpiritBehaviorのテレポート判定後にMoveRandom()（1マス移動）で代替。Spirit属性詠唱語の一部未定義

### 新規追加カテゴリの概要（第17回調査分）
- **EB: AddGold(-負値)サイレント失敗** — Player.AddGold()はamount>0ガードで負値無視。SpendGold()メソッドが存在するのにGameControllerの6箇所でAddGold(-xxx)呼び出し。投資・雇用・ギャンブル・ペナルティ・クラフト・購入の全ゴールド消費がサイレント失敗。実質無限ゴールド
- **EC: 死亡プレイヤーアクション制御不備** — ProcessInput()にPlayer.IsAlive検証なし。状態異常チェック内でのみIsAlive検証。死亡後も移動・攻撃・スキル・アイテム使用が可能。UseItem()/TryUseFirstReadySkill()/TryRangedAttack()もIsAlive未検証
- **ED: スポーン位置ハードコードフォールバック** — ダンジョン/ロケーション/フィールド/建物マップ全4種のフォールバック位置がPosition(5,5)等のハードコード値。タイルタイプ検証なしで壁内配置リスク。GetRandomWalkablePosition()等の安全な代替手段を未使用
- **EE: StatusEffects.First()例外リスク** — ProcessInput()内のHasStatusEffect()チェックとFirst()呼び出しが分離。イベントによる状態異常解除でFirst()がInvalidOperationExceptionをスロー
- **EF: AudioManager Dispose不完全** — _bgmPlayerと_sePlayers(最大8個)のDispose()/Close()未呼び出し。MediaPlayerリソースリーク。長時間プレイでオーディオデバイスリソース枯渇
- **EG: SaveManager例外ハンドリング欠落** — Load()でJsonSerializer.Deserialize()にtry-catchなし。破損JSONでクラッシュ。File.ReadAllText()のIOExceptionも未ハンドル。バックアップセーブ機構不在
- **EH: Damage抵抗値負値ダメージ増幅** — CalculateFinal()でresistanceが[-1f, 0.9f]にクランプ。resistance=-1.0fで2倍ダメージ増幅。デバフによる負値設定でバランス破壊の可能性
- **EI: ObjectPool非アトミックサイズチェック** — Return()の_pool.Count < _maxSize判定とAdd()が非アトミック。複数スレッドが同時チェック通過でmaxSize超過の可能性
- **EJ: クエスト状態遷移検証不備** — State = Completed遷移前にActive検証なし。既にCompleted/TurnedInから再度Completed遷移の可能性。セーブ経由の不正状態にもバリデーションなし
- **EK: SkillDatabase null-forgiving演算子** — GetById()のnull返却に対し?.演算子/if null/!演算子の3パターン混在。コーディング規約の不統一

### 新規追加カテゴリの概要（第18回調査分）
- **EL: CraftingSystem素材消費アトミシティ違反** — Craft()が素材消費→ゴールド消費→結果アイテム生成の順で処理。ItemDefinitions.Create()失敗時に素材・ゴールドが消失しロールバック不可。RemoveItem()戻り値未チェック。SpendGold()の素材消費後実行で部分消失リスク
- **EM: PetSystem null-forgiving・死亡ペット騎乗** — Feed/Train/ToggleRide/TickHungerの4メソッドでTryGetValue失敗時にpet!でnull返却。ToggleRide()にHP>0生存チェック・忠誠度閾値チェックなし。Horse vs Dragonのステータス逆転。TickHunger()頻度制限なし
- **EN: FishingSystem宝箱・確率超過** — fish_treasure/fish_junkのRarity=0でGetAvailableFish()のRarity>0フィルタにより完全除外。3確率関数が独立計算で合計100%超過。Luck修正値が釣りレベル10相当の重みで偏重
- **EO: StatusEffect MaxStack未強制・永続呪い** — Curse/Petrificationがint.MaxValue永続。InstantDeathがDamagePerTick実装で1ターン遅延。MaxStackプロパティ定義済みだがApply側でチェックなし。float.MaxValueのint変換で未定義動作。耐性95%ハードコードキャップ
- **EP: EnchantmentSystem ExpBoost/DropBoostデッドコード** — CalculateEnchantedDamageBonus()のswitch式でExpBoost/DropBoostがデフォルト=0。効果値1.15f/1.2fが一切適用されない。Enchant()にnull/消耗品/上限チェックなし。enum順序依存のRequiredQuality比較
- **EQ: ReputationSystem閾値非対称・IsWelcome緩さ** — Indifferent範囲39ポイント（他は20-30）で中立帯広すぎ。IsWelcome()がHatedのみブロックでHostileでも入境可。上限到達時のイベント未発火。Enumリフレクション毎回実行
- **ER: SkillSystem パッシブ重複・クールダウン永続** — hp_boost/critical_eye等が複数クラスツリーに登録。Reset()メソッド不在でゲームオーバー時クールダウン永続。RestoreCooldownState()に正値/最大値検証なし
- **ES: WeatherSystem RangedHitModifier適用方式不統一** — Clear天候のRangedHitModifier=0.05fが他修正値（1.0fベース乗算）と異なる加算値。ペナルティ偏重（+5% vs -30%）。同一record内でセマンティクス不統一
- **ET: GameController料理品質・失敗チェック未実装** — CalculateQuality()にPlayer.Level*2を渡し料理熟練度を無視。CheckCookingFailure()が存在するがTryCook()から未呼出で料理常時成功。レベル依存品質でスキル育成無意味
- **EU: TerritoryInfluenceSystem正規化前クランプ** — ModifyInfluence()が個別Clamp(0,1)後にNormalize。連続大delta適用でクランプ→正規化の精度劣化。全faction=0時のゼロ除算リスク
- **EV: CompanionSystem最小1ダメージ貫通** — DamageCompanion()のMath.Max(1, damage-defense)で防御超過時も1ダメージ確定。高Defense仲間の防御が無意味。死亡仲間の復活メカニズム不在

### 新規追加カテゴリの概要（第19回調査分）
- **EW: Inventory重量制限未強制・スタック整理不備** — Inventory.Add()がMaxWeight超過チェックなし。Player.IsOverweightプロパティが存在するがAdd()内で未参照で重量制限バイパス可能。スタック加算時のTotalWeight再検証なし。Organize()でMaxStack=0のバリデーション不在。UseItem()のuserパラメータnullチェック不在
- **EX: Player生存ステータス初期化不整合・Sanity境界** — 簡易Create()で_thirst/_fatigueが未初期化（即座にCriticalDehydration/Collapse）。Sanity=0でBrokenだがCanBeRescued()がSanity>0要求でパラドックス。TransferData.Sanity未設定のデッドプロパティ。CreateTransferData()のTotalDeaths=0ハードコード。RestoreFromSave()のlevel/experience整合性バリデーションなし
- **EY: Character基底リソース管理・負値消費バイパス** — ConsumeMp()/ConsumeSp()に負値バリデーションなし（負値渡しでMP回復）。RestoreMp()/RestoreSp()の直接代入でClamp処理バイパス可能性。LevelUp()のEffectiveStats依存HP計算で装備変更エクスプロイト。TakeDamage()の負値ダメージバリデーションなし
- **EZ: ItemFactory乱数生成・レアリティ適用不備** — GenerateRandomItem()がItemType7種中3種のみ生成（Material/Key等が絶対未生成）。GenerateRandomEquipment()がrarity引数を受けるが生成アイテムのRarityプロパティ未変更。ポーション効果値のレアリティスケーリングなし。CanStackWith()がEnhancementLevel未比較でスタック時に強化値消失
- **FA: Equipment呪い解除・防具スロット判定不整合** — OnUnequip()の呪い制限が空実装で呪いシステム無機能。IsArmorSlot()がOffHand含むが盾/サブ武器のVitality加算が不正。両手武器装備時のオフハンド戻し処理未実装でアイテム消失リスク。未識別アイテムの強化値露出
- **FB: Consumable腐敗食料・ポーション効果値ゼロ** — EffectValue=0/EffectPercentage=0のポーションが回復0で消費されるのみ。IsRotten食料の毒Duration=0でItemEffectレコード不整合。Food.HydrationValueとThirstSystem非連携。Scroll.Use()がゲーム状態変更不可のデッドコード
- **FC: Damage抵抗計算・Stats CriticalRate偏重** — resistance最大90%制限で「○○無効」記述と矛盾。CriticalRateのLuck偏重（DEXの2倍寄与）。WaitForInput/Wait同一TypeでTurnCost区別不能。Position.GetDirectionTo()同一位置でNorth固定
- **FD: SymbolMap通行可能設定・タイル表示重複** — SymbolMountain/SymbolWaterのBlocksMovement=falseで山岳・水域が通行可能（致命的）。StairsDown/BuildingExitの表示文字類似。NpcTrainer/NpcLibrarianの表示文字ケース欠落
- **FE: AIビヘイビア・CompositeBehavior常時適用** — SpiritBehaviorテレポートがMoveRandom()（1マス移動）で代替。CompositeBehavior.IsApplicable()が常時true。PackHuntingBehaviorに群れ連携ロジック不在。GetEnvironmentModifier()がactionType引数を完全無視
- **FF: セーブ復元・レベルアップ装備依存問題** — RestoreFromSave()のlevel/experience整合性バリデーションなし。LevelUp()のEffectiveStats依存で装備変更によるHP増加量変動。TransferData.CarriedGold=0時のAddGold()サイレント無視
- **FG: GameConstants一元管理・マジックナンバー散在** — GameConstants定数が各Systemで数値リテラル重複使用。ゲームバランスパラメータのハードコード散在。スポーンフォールバック位置の未定数化
- **FH: Interfaces設計・IMap未使用パラメータ** — IMap.GetEnvironmentModifier()のTurnActionType引数が全実装で無視。IMap.csとInterfaces.csの2ファイル分離で発見性低下
- **FI: EnemyFactory生成・Enum定義粒度不整合** — CreateEnemyForFloor()の一部MonsterRaceフロア出現条件未定義。GameCommand列挙型25値が完全デッドコード。列挙型のEnums.csとエンティティファイル間の重複定義リスク
