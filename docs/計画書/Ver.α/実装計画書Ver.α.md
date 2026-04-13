# 実装計画書 Ver.α.0.1〜2（世界観補強・没入感向上）

**目標**: ストーリーテキスト、フレーバーテキスト、NPC設定・会話内容、クエスト詳細の実装
**状態**: ✅ Ver.α.0.1 全45タスク完了（2026-04-08）

---

## 1. 概要

Ver.α はゲームの世界観を補強し、没入感を高めるフェーズ。
Ver.prt で構築した全ゲームシステム（NPC基盤、会話システム、クエスト基盤、魔法言語、宗教等）の上に、
実際のテキストコンテンツ・ストーリー・キャラクター設定を載せてゲーム世界に命を吹き込む。

> **注**: システム・仕組みは Ver.prt で完成済みの前提。
> 本バージョンでは「中身」（テキスト、設定、物語）の作成・投入に集中する。

---

## 2. メインストーリー

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.1 | メインストーリーライン執筆 | ✅ | 素性ごとの10本のメインストーリー骨格（MainStoryData.cs）|
| α.2 | エンディング分岐シナリオ | ✅ | 素性×5エンディング分岐テキスト（MainStoryData.cs）|
| α.3 | オープニング・プロローグ | ✅ | 素性ごとの導入テキスト10本（MainStoryData.cs）|
| α.4 | 周回ストーリー変化テキスト | ✅ | 死に戻り周回数に応じた台詞変化テキスト（MainStoryData.cs）|

---

## 3. NPC設定・会話コンテンツ

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.5 | 主要NPC人物設定 | ✅ | 8主要NPCの性格・背景・動機・秘密（NpcCharacterData.cs）|
| α.6 | NPC会話テキスト執筆 | ✅ | 全17NPCの多段階会話テキスト実装（レオン/マルコ/アルバート/マーヴィン/エルウェン/リーナ/ガルド/ドワル/ブロック/カリーナ/ミラ/トーマス/ハッサン/サラ/ヴォルフ/イゴール）|
| α.7 | NPC関係性テキスト | ✅ | 素性/宗教/周回数等に応じた反応差分テキスト（NpcCharacterData.cs）|
| α.8 | 商人の品揃え説明・台詞 | ✅ | マルコ台詞・各領地ショップNPC台詞（NpcCharacterData.cs）|

---

## 4. クエストコンテンツ

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.9 | メインクエスト詳細内容 | ✅ | 4章+終章のクエスト目標・ストーリーテキスト・クリア演出（QuestLoreData.cs）|
| α.10 | サブクエスト内容作成 | ✅ | 10クエストの詳細説明・背景ストーリー（QuestLoreData.cs）|
| α.11 | ギルド依頼テキスト | ✅ | 冒険者ギルド掲示板スタイルの依頼文（QuestLoreData.cs）|
| α.12 | クエスト報酬演出テキスト | ✅ | 完了時の感謝台詞・後日談テキスト（QuestLoreData.cs）|

---

## 5. 宗教テキストコンテンツ

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.13 | 宗教の教義・経典テキスト | ✅ | 6宗教それぞれの教義説明、祈りの言葉、経典抜粋（ReligionLoreData.cs） |
| α.14 | 入信・改宗イベントテキスト | ✅ | 入信儀式の描写、改宗時の葛藤・ペナルティ演出テキスト（ReligionLoreData.cs） |
| α.15 | 司祭NPC会話テキスト | ✅ | 6宗教の司祭の個性ある台詞、信仰度に応じた反応（ReligionLoreData.GetPriestGreeting） |
| α.16 | 神の恩恵・禁忌のフレーバー | ✅ | 恩恵発動時/禁忌違反時の演出テキスト（CheckTabooViolation統合済み） |

---

## 6. 魔法言語テキストコンテンツ

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.17 | ルーン語彙のフレーバーテキスト | ✅ | RuneWordLoreData.cs - 35語彙のフレーバーテキスト、習得時演出 |
| α.18 | ルーン碑文テキスト | ✅ | RuneWordLoreData.cs - ダンジョン碑文テキスト14種類 |
| α.19 | 古代の書テキスト | ✅ | RuneWordLoreData.cs - 古代魔術師記録4種類 |
| α.20 | 詠唱演出テキスト | ✅ | RuneWordLoreData.GetCastingText - 詠唱成功時演出テキスト、TryCastSpellに統合 |

---

## 7. フレーバーテキスト・世界観描写

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.21 | 12領地のロケーション描写 | ✅ | TerritoryLoreData.cs - 12領地の到着時テキスト・時間帯テキスト・詳細テキスト実装 |
| α.22 | アイテムフレーバーテキスト | ✅ | 武器(4種)/消耗品(11種)の説明文拡充 |
| α.23 | 敵モンスター図鑑テキスト | ✅ | 10種族×5段階の生態・伝承・弱点テキスト（MonsterLoreData.cs）|
| α.24 | ダンジョンイベントテキスト | ✅ | 宝箱/泉/遺跡/NPC遭遇等のイベント描写テキスト（DungeonEventLoreData.cs）|
| α.25 | 死亡・死に戻り演出テキスト | ✅ | 死因別フレーバー・死に戻りテキスト・正気度0ゲームオーバー（GameOverSystem.cs）|
| α.26 | ゲーム内時間・季節描写 | ✅ | 時間帯/季節/天候の雰囲気テキスト（TimeSeasonLoreData.cs）|
| α.26b | 図鑑UI強化 | ✅ | 検索/フィルタ機能、コンプリート率プログレスバー追加 |
| α.26c | 図鑑コンプリートボーナス | ✅ | 種族コンプ→ダメージ+5%、地域コンプ→商品割引10%、全コンプ→実績「博物学者」 |
| α.26d | NPC・地域図鑑テキスト | ✅ | 6NPCプロフィール・6領地5段階説明・施設解説（NpcLocationLoreData.cs）|
| α.26e | 図鑑発見度5段階拡張 | ✅ | 遭遇→調査→熟知→精通→極致の5段階名、EncyclopediaSystem.GetDiscoveryStageName |

---

## 8. Ver.prt.0.2連携コンテンツ（システム実装後に着手）

> Ver.prt.0.2 で実装されるシステムに対応するテキストコンテンツ。
> システム実装完了後に着手する。

### 8.1 カルマ関連テキスト

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.27 | カルマ変動時のNPC反応テキスト | ✅ | 善人/悪漢/外道等の称号に応じたNPC台詞差分 |
| α.28 | カルマ条件の会話分岐テキスト | ✅ | カルマ値による選択肢追加分のテキスト執筆 |
| α.29 | 闇市NPC会話テキスト | ✅ | 密輸商人の台詞、闇取引の描写 |

### 8.2 能力値フラグ関連テキスト

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.30 | 能力値フラグ条件の会話テキスト | ✅ | 「怪力」「博識」「鷹の目」等のフラグ別NPC反応テキスト |
| α.31 | 能力値条件イベントテキスト | ✅ | 岩破壊/古文書解読/特殊ルート等のイベント描写 |

### 8.3 関係値関連テキスト

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.32 | 種族間関係テキスト | ✅ | エルフ⇔ドワーフ対立会話、人間中立反応等 |
| α.33 | 領地間関係テキスト | ✅ | 国境紛争/貿易同盟等の領地別NPC反応差分 |

### 8.4 仲間・傭兵関連テキスト

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.34 | 仲間NPC人物設定 | ✅ | 加入可能NPC各キャラの性格/背景/動機の設定 |
| α.35 | 仲間NPC会話テキスト | ✅ | 加入時/通常/好感度別/イベント会話テキスト |
| α.36 | 傭兵NPC台詞テキスト | ✅ | 雇用時/戦闘中/契約終了時のテキスト |

### 8.5 新NPC関連テキスト

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.37 | 吟遊詩人テキスト | ✅ | 歌詞/伝承語り/情報提供テキスト |
| α.38 | 賞金稼ぎテキスト | ✅ | 依頼提示/報酬交渉/傭兵斡旋テキスト |
| α.39 | 占い師テキスト | ✅ | 予言/ヒント/運勢テキスト |
| α.40 | 亡霊NPCテキスト | ✅ | 成仏クエストの会話/背景ストーリー |

### 8.6 具体的マップ関連テキスト

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.41 | 手作りマップのロケーション描写 | ✅ | ボスフロア/特殊ダンジョンの雰囲気テキスト |
| α.42 | 街マップのNPC配置・会話 | ✅ | 各領地の街内部NPCの位置・台詞 |
| α.43 | 遺産・遺跡発見テキスト | ✅ | 前回死亡時のメモ/他冒険者の遺書テキスト |

### 8.7 季節関連テキスト

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.44 | 季節別NPC台詞差分 | ✅ | 春の祭り/夏の暑さ/秋の収穫/冬の寒さに関するNPC発言 |
| α.45 | 季節イベントテキスト | ✅ | 季節限定イベントの描写テキスト |

---

## 9. ブラッシュアップ記録

### Ver.α.0.1 完了ブラッシュアップ（2026-04-08）

#### 実施内容
- 全45タスク（α.1〜α.45）実装完了を確認
- テスト状況: Core.Tests 6,353件全合格（うちAlpha専用テスト: AlphaLoreDataTests.cs 38件 + AlphaLoreDataTests2.cs 49件）
- 新規追加ファイル（17ファイル）:
  - `src/RougelikeGame.Core/Data/MainStoryData.cs`（メインストーリー・エンディング・プロローグ）
  - `src/RougelikeGame.Core/Data/NpcCharacterData.cs`（NPC設定・会話テキスト）
  - `src/RougelikeGame.Core/Data/QuestLoreData.cs`（クエスト詳細テキスト）
  - `src/RougelikeGame.Core/Data/ReligionLoreData.cs`（宗教教義・司祭会話）
  - `src/RougelikeGame.Core/Data/RuneWordLoreData.cs`（ルーン語フレーバー・詠唱演出）
  - `src/RougelikeGame.Core/Data/TerritoryLoreData.cs`（領地ロケーション描写）
  - `src/RougelikeGame.Core/Data/MonsterLoreData.cs`（モンスター図鑑テキスト）
  - `src/RougelikeGame.Core/Data/DungeonEventLoreData.cs`（ダンジョンイベント描写）
  - `src/RougelikeGame.Core/Data/TimeSeasonLoreData.cs`（時間・季節・天候描写）
  - `src/RougelikeGame.Core/Data/NpcLocationLoreData.cs`（NPC・地域図鑑テキスト）
  - `src/RougelikeGame.Core/Data/KarmaRelatedData.cs`（カルマ関連NPC反応・闇市テキスト）
  - `src/RougelikeGame.Core/Data/StatFlagEventData.cs`（能力値フラグ条件テキスト）
  - `src/RougelikeGame.Core/Data/RaceRelationData.cs`（種族間・領地間関係テキスト）
  - `src/RougelikeGame.Core/Data/CompanionNpcData.cs`（仲間・傭兵NPC設定・台詞）
  - `src/RougelikeGame.Core/Data/SpecialNpcData.cs`（吟遊詩人・賞金稼ぎ・占い師・亡霊NPCテキスト）
  - `src/RougelikeGame.Core/Data/DungeonLocationData.cs`（ダンジョンマップロケーション・遺跡テキスト）
  - `src/RougelikeGame.Core/Data/SeasonalEventData.cs`（季節別NPC台詞・季節イベント）

#### 関連設計書更新
- `docs/企画設計書/17_デバッグ・テスト設計書.md`: テスト件数更新（5,650→6,353件）
- `docs/計画書/マスター実装計画書.md`: Ver.α.0.1 完了ステータス更新
- `docs/企画設計書/11_クラス設計書.md`: Ver.α追加クラス反映

---

## 10. Ver.α.0.2 シンボルマップ拡張（2026-04-10）

**目標**: 12領地対応、可変サイズシンボルマップ、村/町/都自動配置、ランダムダンジョン生成、勢力影響敵種別マッピング、シンボルマップ上アクション制限

**状態**: ✅ 全タスク完了

### 10.1 領地拡張

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.46 | TerritoryId 12領地化 | ✅ | Desert, Swamp, Tundra, Lake, Volcanic, Sacred追加（Enums.cs）|
| α.47 | 新TileType追加 | ✅ | SymbolVillage, SymbolCapital, SymbolBanditDen, SymbolGoblinNest（Tile.cs）|
| α.48 | 新LocationType追加 | ✅ | Capital, BanditDen, GoblinNest（WorldMapSystem.cs）|
| α.49 | 12領地TerritoryDefinition | ✅ | 隣接関係・施設・レベル帯の定義（WorldMapSystem.cs）|
| α.50 | 12領地LocationDefinition | ✅ | 新6領地の町/村/施設/ダンジョン定義（WorldMapSystem.cs）|
| α.51 | 12領地TerritoryLoreData | ✅ | 到着テキスト・詳細テキスト・時間帯描写の6領地分追加 |

### 10.2 シンボルマップ生成

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.52 | 可変サイズマップ(23000-50000マス) | ✅ | 領地ごとに幅×高さを定義（SymbolMapGenerator.cs）|
| α.53 | 複雑形状生成 | ✅ | ノイズベース楕円+突起/湾で複雑形状マスク生成 |
| α.54 | 隣接境界一致 | ✅ | 決定論的シード値で同一辺のノイズカーブを共有 |
| α.55 | 村自動配置(マス数/500箇所) | ✅ | PlaceSettlementsメソッドで自動生成 |
| α.56 | 町自動配置(マス数/1000箇所) | ✅ | 同上 |
| α.57 | 都自動配置(1箇所/領地) | ✅ | マップ中心付近に優先配置 |
| α.58 | ランダムダンジョン生成 | ✅ | 野盗のねぐら/ゴブリンの巣等、50マス距離制限/100マス間隔、1-3階層 |

### 10.3 勢力影響システム

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.59 | 勢力名定数定義 | ✅ | FactionNames（王国/賊/ゴブリン/野生動物/アンデッド/魔族/エルフ/ドワーフ）|
| α.60 | タイルレベル勢力判定 | ✅ | GetDominantFactionForTile（集落近く=野生動物、遠方=領地支配勢力）|
| α.61 | 勢力→敵種別マッピング | ✅ | GetEnemyTypeForFaction（賊→野盗、ゴブリン→ゴブリン等）|
| α.62 | 領地デフォルト勢力 | ✅ | GetDefaultFaction（12領地×デフォルト勢力）|

### 10.4 シンボルマップ上アクション制限（Ver.prt）

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.63 | アクション制限実装 | ✅ | IsAllowedOnSymbolMap: 移動/インベントリ/情報確認/進入のみ許可 |

### 10.5 テスト

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.64 | Ver.αテスト追加 | ✅ | VerAlphaSymbolMapTests.cs 71件追加、既存テスト修正 |

### Ver.α.0.2 ブラッシュアップ記録（2026-04-10）

#### 実施内容
- 全19タスク（α.46〜α.64）実装完了
- テスト状況: Core.Tests 6,614件全合格（うちVer.αシンボルマップテスト: 71件）
- 変更ファイル:
  - `src/RougelikeGame.Core/Enums/Enums.cs`: TerritoryId 12領地化
  - `src/RougelikeGame.Core/Map/Tile.cs`: 新TileType（SymbolVillage/Capital/BanditDen/GoblinNest）
  - `src/RougelikeGame.Core/Systems/WorldMapSystem.cs`: 新LocationType、12領地TerritoryDefinition/LocationDefinition
  - `src/RougelikeGame.Core/Map/Generation/SymbolMapGenerator.cs`: 大幅改修（可変サイズ/複雑形状/自動配置/ランダムダンジョン）
  - `src/RougelikeGame.Core/Systems/TerritoryInfluenceSystem.cs`: 勢力名定数/タイルレベル勢力/敵種別マッピング追加
  - `src/RougelikeGame.Core/Systems/SymbolMapSystem.cs`: 新LocationType対応（BanditDen/GoblinNest）
  - `src/RougelikeGame.Core/Systems/SymbolMapEventSystem.cs`: 新6領地専用イベント7件追加
  - `src/RougelikeGame.Core/Data/TerritoryLoreData.cs`: 新6領地のロア/時間帯テキスト
  - `src/RougelikeGame.Gui/GameController.cs`: シンボルマップ上アクション制限/ダンジョン遷移対応
  - `tests/RougelikeGame.Core.Tests/VerAlphaSymbolMapTests.cs`: 新規71件テスト
  - `tests/RougelikeGame.Core.Tests/SymbolMapTransitionTests.cs`: 可変サイズ/12領地対応修正
  - `tests/RougelikeGame.Core.Tests/WorldMapSystemTests.cs`: 12領地対応修正
  - `tests/RougelikeGame.Core.Tests/SystemExpansionPhase7Tests.cs`: イベント数修正
  - `tests/RougelikeGame.Core.Tests/SystemExpansionPhase9Tests.cs`: 12領地対応修正

---

## 11. Ver.α.0.5 安全圏/危険圏重複処理 + 関所NPC統一（2026-04-13）

**目標**: A1（安全圏/危険圏重複処理ルール）、A2（ワールドマップ廃止→関所NPC統一）の実装と関連修正

**状態**: ✅ 全タスク完了

### 11.1 A1: 安全圏/危険圏重複処理ルール

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.65 | 安全圏同士の重複処理 | ✅ | そのまま処理（安全圏のまま維持）|
| α.66 | 安全圏と危険圏の重なり処理 | ✅ | GetDominantFactionForTileWithDays: 日数経過で安全圏距離縮小（30日/1マス、最小半分）|
| α.67 | 危険圏同士の重複処理 | ✅ | IsInFactionTerritory: 派閥勢力（Influence値）による支配範囲決定 |
| α.68 | BorderGate常時安全圏 | ✅ | BorderGateは圏域判定前に安全圏として返却 |
| α.69 | Dungeon判定対象外 | ✅ | Dungeon/BanditDen/GoblinNestは圏域計算に含めない |

### 11.2 A2: ワールドマップ廃止→関所NPC統一

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.70 | TryTravelTo関所案内メッセージ | ✅ | 常にfalseを返し、関所案内メッセージを表示 |
| α.71 | 外周移動時メッセージ変更 | ✅ | 「関所（BorderGate）を通じて隣接領地に移動できます」に変更 |
| α.72 | BorderGate進入時NPC演出 | ✅ | 関所番兵の台詞付き、通行チェック・旅路イベント・領地遷移を実行 |
| α.73 | WorldMapWindow情報参照用維持 | ✅ | ExecuteTravel→MessageBox案内→DialogResult=false、直接移動不可 |
| α.74 | MainWindow整理 | ✅ | ShowWorldMapDialogのデッドコード除去 |

### 11.3 A3～C4: 関連修正・テスト増強

| # | タスク | 状態 | 備考 |
|---|--------|------|------|
| α.75 | WorldMapWindow.ExecuteTravel修正 | ✅ | MessageBox後にDialogResult=false, Close()を追加 |
| α.76 | MainWindow.ShowWorldMapDialog整理 | ✅ | TravelDestination参照のデッドコードを削除 |
| α.77 | A1境界値テスト追加 | ✅ | 安全圏距離下限(半分)、BorderGate+Dungeon独立、勢力比判定、等勢力距離判定 |
| α.78 | A2追加テスト | ✅ | 通行料不足テスト、旅路イベントタイプ定義テスト |
| α.79 | 設計書更新 | ✅ | 14_マップシステム設計書にA1/A2仕様反映 |

### Ver.α.0.5 ブラッシュアップ記録（2026-04-13）

#### 実施内容
- A1: 安全圏/危険圏重複処理ルール、A2: ワールドマップ廃止→関所NPC統一、A3〜C4: 関連修正
- テスト状況: VerAlphaSymbolMapTests 167件全合格（161→167: 6件追加）
- 変更ファイル:
  - `src/RougelikeGame.Core/Systems/TerritoryInfluenceSystem.cs`: A1ロジック（GetDominantFactionForTileWithDays/IsInFactionTerritory/DangerExpansionDaysPerTile）
  - `src/RougelikeGame.Gui/GameController.cs`: A2リファクタ（TryTravelTo→関所案内、BorderGate遷移強化、旅路イベント統合）
  - `src/RougelikeGame.Gui/WorldMapWindow.xaml.cs`: ExecuteTravel→案内MessageBox+DialogResult=false+Close()
  - `src/RougelikeGame.Gui/MainWindow.xaml.cs`: ShowWorldMapDialogデッドコード除去
  - `docs/企画設計書/14_マップシステム設計書.md`: A1/A2仕様追記
  - `tests/RougelikeGame.Core.Tests/VerAlphaSymbolMapTests.cs`: A1境界値テスト4件+A2テスト2件追加
