# GUIシステム設計書

> **📌 実装状況**
>
> 本書に記載のGUI要素は全て `RougelikeGame.Gui` プロジェクトで実装済みです。
> Phase 6 でタイトル画面、設定画面、BGM/SE管理、描画最適化が追加されました。
> Ver.prt.0.7 でNPC隣接インタラクト、ウィンドウトグル操作、魔法詠唱ターン遅延、D&Dドロップ修正、フロアキャッシュ分離、町脱出改善が追加されました。
> Ver.prt.0.9 でインベントリ常時表示化（コールバックパターン）・自動ソート・グリッド外ドロップ、ショップD&D統合・スクロール数量、仲間募集ウィンドウ（RecruitCompanionWindow）、クエストボード（QuestBoardWindow）が追加されました。
> Ver.prt.0.10 でインベントリウィンドウに装備パネル（11スロット）+ステータス表示を追加（850px幅化）、ソート状態維持修正（_isSortedフラグ）、スキルツリーウィンドウに「習得済み/スロット」タブとD&Dスキルスロット（6スロット）を追加、SpellCastingWindowからルーン学習ボタンを撤去しました。
> Ver.prt.0.11 でスキルスロットを1-5→1-6キーに拡張（SkillTreeSystem.MaxSkillSlots=6と統一）しました。
> Ver.prt.0.12 でタイトル画面を「タイトル未定」/「ろーぐらいくげーむ」に変更、インベントリ装備パネルをPoE風Canvas身体配置（11スロット空間配置、身体シルエットガイドライン）に全面改修、ショップウィンドウをリスト表示形式に全面書き換え、スキルツリーにTier制（1-4）・レベル制限（Lv1/5/10/15）を導入しGrimDawn風UIに改修しました。
> 未実装の画面・機能は「将来計画」セクションに記載しています。

---

## 1. 技術基盤

| 項目 | 内容 |
|------|------|
| フレームワーク | WPF（.NET 10.0-windows） |
| 描画方式 | Canvas + Rectangle/TextBlock による即時描画 |
| フォント | Consolas（等幅フォント） |
| タイルサイズ | 20×20 ピクセル |
| ウィンドウサイズ | 1024×720 ピクセル |
| 入力方式 | KeyDown イベント + Keyboard.IsKeyDown（同時押し検出） |
| 自動探索タイマー | DispatcherTimer（80ms間隔） |

---

## 2. 画面構成

### 2.1 メインウィンドウ（MainWindow.xaml）

```
┌──────────────────────────────────────────────────────┐
│ [ステータスバー]                                      │
│  第1層 | 冒険歴1024年 緑風の月15日 08:00 朝           │
│          Lv:1 EXP:0/100 HP:100/100 MP:50/50 ...     │
├──────────────────────────────────────────────────────┤
│                                       ┌──────────┐  │
│                                       │ミニマップ │  │
│         ゲームマップ表示エリア          │ 160×100  │  │
│        （GameCanvas）                  └──────────┘  │
│         プレイヤー中心のスクロール描画                  │
│                                                      │
├──────────────────────────────────────────────────────┤
│ [メッセージログ] 最新メッセージをスクロール表示          │
├──────────────────────────────────────────────────────┤
│ [操作説明] キー操作一覧（常時表示）                     │
└──────────────────────────────────────────────────────┘
```

**Grid行定義**:
| 行 | Height | 内容 |
|----|--------|------|
| Row 0 | Auto | ステータスバー |
| Row 1 | * | ゲームマップ表示エリア（可変） |
| Row 2 | Auto | メッセージログ（MaxHeight=80） |
| Row 3 | Auto | 操作説明バー |

### 2.2 タイトル画面（TitleWindow.xaml）

アプリケーション起動時に最初に表示される画面。`App.xaml.cs` の `Application_Startup` からShowDialogで表示。

```
┌──────────────────────────────────────────────────────┐
│                                                      │
│              — Abyss Reborn —                        │
│              深淵より蘇りし者                          │
│              Ver.prt.0.1 — Prototype                 │
│                                                      │
│              ▶ ニューゲーム                            │
│              ▶ コンティニュー（セーブ存在時のみ有効）     │
│              ⚙ 設定                                   │
│              ✕ 終了                                   │
│                                                      │
│          操作: ↑↓ 選択  Enter 決定  Esc 終了          │
└──────────────────────────────────────────────────────┘
```

| 機能 | 説明 |
|------|------|
| キーボードナビゲーション | W/↑、S/↓でカーソル移動、Enter/Space決定、Esc終了 |
| コンティニュー制御 | `SaveManager.SaveExists()` でセーブデータ存在チェック |
| TitleAction列挙型 | NewGame/Continue/Settings/Quit |

### 2.3 設定画面（SettingsWindow.xaml）

タイトル画面またはメインウィンドウから表示される設定ダイアログ。

| 設定項目 | UI要素 | 範囲 |
|---------|--------|------|
| マスター音量 | スライダー | 0-100% |
| BGM音量 | スライダー | 0-100% |
| SE音量 | スライダー | 0-100% |
| フォントサイズ | スライダー | 10-24 |
| キーバインド | テキスト表示 | 参照用（変更不可） |

| ボタン | 動作 |
|--------|------|
| 保存 | `GameSettings.Save()` でJSON永続化 |
| キャンセル | 変更を破棄して閉じる |
| 初期値に戻す | `GameSettings.CreateDefault()` でリセット |

### 2.4 サブウィンドウ

| ウィンドウ | ファイル | 起動キー/条件 | 用途 |
|-----------|---------|-------------|------|
| CharacterCreationWindow | CharacterCreationWindow.xaml/.cs | ニューゲーム時 | キャラクター作成（10種族/10職業/10素性選択、名前入力、ステータスプレビュー） |
| DifficultySelectWindow | DifficultySelectWindow.xaml/.cs | ニューゲーム時 | 難易度選択（Easy/Normal/Hard/Nightmare/Ironman） |
| StatusWindow | StatusWindow.xaml/.cs | C | 基本9ステータス、戦闘パラメータ、装備、状態異常の詳細表示 |
| InventoryWindow | InventoryWindow.xaml/.cs | I | PoE風グリッドインベントリ（10×6グリッド、アイテムサイズ自動判定、レアリティ色分け、装備マーク、D&D移動・位置保持、コールバックパターンで使用後も開放維持、Sキー自動ソート、グリッド外D&Dでアイテムドロップ、右カラムにPoE風Canvas身体配置装備パネル11スロット（Head/Neck/Back/MainHand/Body/OffHand/Hands/Ring1/Waist/Ring2/Feet）+身体シルエットガイドライン+ステータス表示、850px幅） |
| MessageLogWindow | MessageLogWindow.xaml/.cs | L | メッセージログの履歴閲覧、フィルタリング |
| ShopWindow | ShopWindow.xaml/.cs | ショップ選択時 | リスト表示形式のアイテム売買（購入/売却タブ切替、アイテム一覧リスト表示、選択で購入/売却実行） |
| CraftingWindow | CraftingWindow.xaml/.cs | —（キー削除済） | 合成・強化・エンチャント操作（内部イベント経由のみ） |
| SpellCastingWindow | SpellCastingWindow.xaml/.cs | V | ルーン語による魔法詠唱（習得済みルーン一覧表示、未習得時は空表示） |
| DialogueWindow | DialogueWindow.xaml/.cs | NPC隣接時に方向キー押下 | NPC会話・選択肢表示（NPCの隣接タイルから方向キーで発動） |
| QuestLogWindow | QuestLogWindow.xaml/.cs | K | クエスト進捗確認（受注済みのみ、詳細表示専用） |
| ReligionWindow | ReligionWindow.xaml/.cs | O | 宗教情報表示・祈り（入信はNPC経由のみ） |
| TownWindow | TownWindow.xaml/.cs | —（キー削除済、シンボルマップ階段経由） | 街施設メニュー（宿屋/教会/銀行/鍛冶屋/ギルド/神殿等） |
| WorldMapWindow | WorldMapWindow.xaml/.cs | J | 領地移動画面（町入場ボタン削除済、移動先選択のみ） |
| TravelEventWindow | TravelEventWindow.xaml/.cs | 領地間移動時 | 移動イベント（強行/交渉/回避の選択） |
| SaveDataSelectWindow | SaveDataSelectWindow.xaml/.cs | F5/F9 | セーブスロット選択 |
| SkillTreeWindow | SkillTreeWindow.xaml/.cs | E | GrimDawn風スキルツリー（Tier 1-4段階表示、レベル制限Lv1/5/10/15、ノード一覧Tier順ソート・🔒レベルロック表示・解放・リスペック・キーストーン確認、「習得済み/スロット」タブでD&Dスキルスロット6枚配置） |
| EncyclopediaWindow | EncyclopediaWindow.xaml/.cs | Y | 図鑑（カテゴリタブ切替・発見度表示・エントリ詳細） |
| CompanionWindow | CompanionWindow.xaml/.cs | U | 仲間管理（パーティ一覧・AIモード変更・解雇・忠誠度警告） |
| DeathLogWindow | DeathLogWindow.xaml/.cs | Z | 死亡録（統計表示・死因内訳・死亡記録一覧） |
| RecruitCompanionWindow | RecruitCompanionWindow.xaml/.cs | ギルド受付「仲間を募集する」選択時 | 仲間募集候補一覧（候補ごとに名前・タイプ・レベル・ステータス・雇用コスト表示、雇うボタンでゴールド消費→AddCompanion） |
| QuestBoardWindow | QuestBoardWindow.xaml/.cs | ギルド受付「クエストを確認する」選択時 | クエストボード3タブ（受注可能/進行中/完了済み）。受注ボタン・報告ボタンでQuestSystem API呼び出し |

---

## 3. 描画エンジン（GameRenderer）

### 3.1 参照ファイル

`src/RougelikeGame.Gui/GameRenderer.cs`

### 3.2 描画フロー

```
Render(map, player, enemies, groundItems)
  │
  ├─ カメラオフセット計算（プレイヤー中心）
  │   offsetX = canvasWidth/2 - player.X × TileSize
  │   offsetY = canvasHeight/2 - player.Y × TileSize
  │
  ├─ 描画範囲計算（Canvas内のみ描画で最適化）
  │
  ├─ タイル描画（背景Rectangle + 文字TextBlock）
  │   ├─ 可視: TileColors辞書に基づく色
  │   ├─ 探索済み: 暗い灰色（ExploredBackground/Foreground）
  │   └─ 未探索: 黒（UnexploredBackground、文字なし）
  │
  ├─ 地面アイテム描画（'!' シアン色、可視タイルのみ）
  │
  ├─ 敵描画（名前先頭文字、種別別色、可視タイルのみ）
  │
  └─ プレイヤー描画（'@' 黄色、ハイライト矩形付き）
```

### 3.3 タイル色定義

| TileType | 背景色 (RGB) | 前景色 | 表示文字 |
|----------|-------------|--------|---------|
| Floor | (30, 30, 40) | Gray | . |
| Wall | (60, 60, 80) | DarkGray | # |
| Corridor | (25, 25, 35) | Gray | . |
| DoorClosed | (139, 90, 43) | SaddleBrown | + |
| DoorOpen | (100, 60, 30) | Peru | / |
| StairsUp | (30, 60, 100) | CornflowerBlue | < |
| StairsDown | (30, 80, 50) | LimeGreen | > |
| Water | (20, 50, 80) | DodgerBlue | ~ |
| TrapHidden | (30, 30, 40) | Gray | .（床と同じ） |
| TrapVisible | (80, 30, 80) | Magenta | ^ |
| Altar | (50, 40, 60) | Gold | _ |
| Fountain | (30, 40, 60) | Aqua | { |
| Chest | (60, 50, 20) | Goldenrod | □ |
| DebugEnemySpawn | (80, 20, 20) | OrangeRed | E（デバッグ専用） |
| DebugAIToggle | (20, 60, 80) | DeepSkyBlue | A（デバッグ専用） |
| DebugDayAdvance | (60, 60, 20) | Yellow | D（デバッグ専用） |
| DebugNpc | (20, 60, 40) | SpringGreen | N（デバッグ専用） |

**FOV状態による色分け**:
| 状態 | 背景色 (RGB) | 前景色 (RGB) |
|------|-------------|-------------|
| 可視 | タイル固有色 | タイル固有色 |
| 探索済み（非可視） | (15, 15, 20) | (50, 50, 60) |
| 未探索 | (10, 10, 15) | 透明 |

### 3.4 敵種別色

| EnemyTypeId | 色 |
|-------------|-----|
| slime | LimeGreen |
| goblin | Orange |
| skeleton | LightGray |
| orc | OrangeRed |
| giant_spider | Purple |
| dark_elf | DarkViolet |
| troll | DarkGreen |
| draugr | LightBlue |
| その他 | Red |

### 3.5 エンティティ描画記号

| エンティティ | 記号 | 色 | 特殊効果 |
|-------------|------|-----|---------|
| プレイヤー | @ | Yellow | 半透明黄色ハイライト矩形 + 黄色枠線 |
| 敵 | 名前先頭文字 | 種別別（上表参照） | なし |
| 地面アイテム | ! | Cyan | なし |

---

## 4. ミニマップ

### 4.1 仕様

| 項目 | 値 |
|------|-----|
| サイズ | 160×100 ピクセル |
| 位置 | ゲームマップ右上オーバーレイ |
| 背景 | 半透明黒 (#80000000) |
| 枠線 | #404060、太さ1、角丸3 |
| 切り替え | Mキーで表示/非表示 |
| スケール | マップ全体がCanvas内に収まるよう自動計算 |

### 4.2 描画内容

| 要素 | 色 | サイズ |
|------|-----|-------|
| 壁 | (60, 60, 80) | scale × scale |
| 可視床 | (80, 80, 100) | scale × scale |
| 探索済み床 | (40, 40, 55) | scale × scale |
| 階段（下） | LimeGreen | scale × scale |
| 階段（上） | CornflowerBlue | scale × scale |
| 敵（視界内のみ） | Red | scale × 1.5 |
| プレイヤー | Yellow | scale × 2 |

---

## 5. ステータスバー

### 5.1 色設計（色分け表示）

| 項目 | 通常色 | 条件変化色 |
|------|--------|----------|
| 領地名 | #e94560（赤） | — |
| 地上/ダンジョン | #4ecca3（緑） | — |
| 階層 | #e94560（赤） | — |
| 日時 | #c0a0ff（紫） | — |
| 時間帯 | #ffd93d（黄） | — |
| ターン制限 | #ff6b6b（赤） | — |
| Lv | #ffa500（オレンジ） | — |
| EXP | #ffa500（オレンジ） | — |
| HP | #4ecca3（緑） | 残量に応じて黄→赤へ変化 |
| MP | #00d9ff（水色） | — |
| SP | #ffd93d（黄） | — |
| 満腹度 | #ff6b6b（赤） | 段階に応じて色変化 |
| 正気度 | #c0a0ff（紫） | 段階に応じて色変化 |
| 所持金 | #ffd93d（黄） | — |
| 重量 | #c0c0c0（灰） | 超過時に赤へ変化 |

### 5.2 ウィンドウ配色

| 要素 | 色コード | 用途 |
|------|---------|------|
| #1a1a2e | ウィンドウ背景 |
| #16213e | ステータスバー/メッセージログ背景 |
| #0f0f23 | ゲームマップエリア背景 |
| #0f3460 | 操作説明バー背景 |

---

## 6. 入力処理

### 6.1 キー割り当て

| キー | GameAction | 処理 |
|------|------------|------|
| W / ↑ | MoveUp | 上方向移動 |
| S / ↓ | MoveDown | 下方向移動 |
| A / ← | MoveLeft | 左方向移動 |
| D / → | MoveRight | 右方向移動 |
| W+D / ↑+→ 同時押し | — | 右上斜め移動 |
| W+A / ↑+← 同時押し | — | 左上斜め移動 |
| S+D / ↓+→ 同時押し | — | 右下斜め移動 |
| S+A / ↓+← 同時押し | — | 左下斜め移動 |
| Home | MoveUpLeft | 左上斜め移動（単独キー） |
| PgUp | MoveUpRight | 右上斜め移動（単独キー） |
| End | MoveDownLeft | 左下斜め移動（単独キー） |
| PgDn | MoveDownRight | 右下斜め移動（単独キー） |
| Space | Wait | 待機（1ターン消費） |
| G | Pickup | アイテム拾う |
| I | OpenInventory | インベントリ画面表示 |
| C | OpenStatus | ステータス画面表示 |
| L | OpenMessageLog | メッセージログ画面表示 |
| Tab | AutoExplore | 自動探索開始/停止 |
| M | ToggleMinimap | ミニマップ表示切り替え |
| F | Search | 周囲探索（隠しドア・罠発見） |
| X | CloseDoor | ドア閉じる |
| R | RangedAttack | 遠隔攻撃（射撃） |
| T | ThrowItem | アイテム投擲 |
| V | StartCasting | 魔法詠唱画面表示 |
| P | Pray | 祈り（宗教行動） |
| E | OpenSkillTree | スキルツリー画面表示（スロット割当） |
| ~~N~~ | ~~RegisterGuild~~ | ~~ギルド登録~~（削除済：NPC会話選択肢に統合 Ver.prt.0.8） |
| J | OpenWorldMap | ワールドマップ画面表示 |
| K | ViewQuestLog | クエストログ画面表示 |
| O | OpenReligion | 宗教画面表示 |
| ~~H~~ | ~~OpenCrafting~~ | ~~合成・鍛冶画面表示~~（削除済：内部イベント経由のみ） |
| ~~B~~ | ~~EnterTown~~ | ~~街入場~~（削除済：シンボルマップ階段経由） |
| 1-6 | UseSkillSlot | スキルスロット1〜6使用 |
| Y | OpenEncyclopedia | 図鑑画面表示 |
| U | OpenCompanion | 仲間管理画面表示 |
| Z | OpenDeathLog | 死亡記録画面表示 |
| Shift + . | UseStairs | 階段を降りる |
| Shift + , | AscendStairs | 階段を上る |
| F5 | Save | セーブ |
| F9 | Load | ロード |
| Q | Quit | ゲーム終了 |
| Esc | — | ダイアログ閉じる（各ウィンドウは開くキーでも閉じるトグル対応） |

### 6.2 斜め移動（同時押し検出）

`Keyboard.IsKeyDown` を使い、KeyDownイベント時に複数キーの押下状態を確認して8方向移動を実現。

### 6.3 自動探索

| 項目 | 値 |
|------|-----|
| 起動 | Tabキー |
| 実行間隔 | 80ms（DispatcherTimer） |
| アルゴリズム | BFS経路探索（最寄りの未探索タイル） |
| 停止条件 | 敵発見、アイテム発見、階段発見、HP低下、手動入力 |

---

## 7. ゲームコントローラー（GameController）

### 7.1 参照ファイル

`src/RougelikeGame.Gui/GameController.cs`

### 7.2 主要プロパティ

| プロパティ | 型 | 説明 |
|-----------|-----|------|
| Player | Player | プレイヤーエンティティ |
| Map | DungeonMap | 現在のマップ |
| Enemies | List\<Enemy\> | 現在フロアの敵リスト |
| GroundItems | List\<(Item, Position)\> | 地面アイテム |
| CurrentFloor | int | 現在の階層 |
| TurnCount | int | 累計ターン数 |
| GameTime | GameTime | ゲーム内時間管理 |
| IsAutoExploring | bool | 自動探索中フラグ |

### 7.3 イベント

| イベント | 用途 |
|---------|------|
| OnMessage | メッセージログにテキスト追加 |
| OnStateChanged | UI更新通知（ステータスバー再描画等） |
| OnGameOver | ゲームオーバー処理 |
| OnShowInventory | インベントリ画面表示要求 |
| OnShowStatus | ステータス画面表示要求 |
| OnShowDialogue | NPC会話ダイアログ表示要求（隣接インタラクト時、選択肢付き） |
| OnOpenShop | ショップウィンドウ表示要求（NPC会話選択肢からFacilityType指定） |
| OnCastingStarted | 魔法詠唱開始通知（詠唱ターン数表示） |
| OnShowRecruitCompanion | 仲間募集ウィンドウ表示要求（候補リスト付き） |
| OnShowQuestBoard | クエストボードウィンドウ表示要求 |

### 7.4 処理フロー

```
ProcessInput(GameAction)
  │
  ├─ 移動 → TryMove → 敵がいれば攻撃
  │   ├─ NPC隣接判定（IsNpcTile）→ HandleNpcTile → OnShowDialogue（選択肢付き）
  │   │   └─ 選択肢アクション: DispatchNpcAction（ギルド登録/入信/売買/宿泊/祈り）
  │   ├─ 町マップ端判定（_isInLocationMap）→ TryLeaveTown
  │   └─ BlocksMovement判定
  ├─ 待機 → AdvanceTurns(1)
  ├─ アイテム拾う → TryPickupItem（CanFitInGrid容量チェック + 図鑑自動登録）
  ├─ インベントリ → OnShowInventory
  ├─ ステータス → OnShowStatus
  ├─ 階段 → DescendStairs → GenerateNewFloor
  └─ 自動探索 → StartAutoExplore / StopAutoExplore
  │
  ├─ ProcessEnemyTurns（敵AI処理、ActiveRange=10内のみ）
  ├─ ProcessTurnEffects（飢餓減少、HP/SP自然回復、状態異常ティック）
  │   └─ ProcessChanting（詠唱カウントダウン、完了時魔法発動）
  ├─ FOV再計算
  └─ OnStateChanged（UI更新通知）
```

---

## 8. BGM/SE管理（AudioManager）

### 8.1 参照ファイル

```
src/RougelikeGame.Gui/Audio/
├── IAudioManager.cs          # オーディオ管理インターフェース
├── AudioManager.cs           # WPF MediaPlayer実装
├── SilentAudioManager.cs     # テスト・無音環境用実装
└── AudioIds.cs               # BGM/SE ID定数（BgmIds 10種、SeIds 21種）
```

### 8.2 BGM再生

| 項目 | 仕様 |
|------|------|
| 再生方式 | WPF MediaPlayer |
| ループ | MediaEnded → Position=0 で実現 |
| 同時再生 | BGMは常に1曲のみ |
| 切り替え | 同一ID再生中はスキップ、別IDは停止→再生 |

### 8.3 SE再生

| 項目 | 仕様 |
|------|------|
| 同時再生 | 最大8チャンネル（MediaPlayerプール） |
| 重複抑制 | 同一SEは50ms以上間隔を空ける |
| プレイヤー再利用 | 再生完了済みのMediaPlayerを優先再利用 |

### 8.4 シーン別BGM

| シーン | BGM ID | 備考 |
|--------|--------|------|
| タイトル画面 | BGM_001 | 荘厳、ダークファンタジー |
| ダンジョン探索 | BGM_002 | 不穏、緊張感 |
| 通常戦闘 | BGM_003 | テンポ速い |
| ボス戦 | BGM_004 | 重厚、激しい |
| ゲームオーバー | BGM_006 | 暗い、短い |
| 拠点（街） | BGM_008 | 穏やか |

---

## 9. 描画最適化（Phase 6）

### 9.1 GameRenderer プーリング

| 最適化 | 説明 |
|--------|------|
| Rectangle プール | Canvas上のRectangleを再利用（毎フレーム再生成を廃止） |
| TextBlock プール | Canvas上のTextBlockを再利用 |
| Visibility切り替え | 未使用オブジェクトはCollapsedに設定 |
| プール自動拡張 | 描画範囲拡大時に必要分だけ新規追加 |

### 9.2 ObjectPool\<T\>

| 項目 | 仕様 |
|------|------|
| ベース | ConcurrentBag\<T\>（スレッドセーフ） |
| リセット | 返却時にリセットアクション実行 |
| 最大サイズ | 上限超過時は返却を破棄 |
| 参照ファイル | `src/RougelikeGame.Core/Utilities/ObjectPool.cs` |

---

## 10. 将来計画

| 項目 | フェーズ | 説明 | 状態 |
|------|---------|------|------|
| タイトル画面 | Phase 6 | スタート/コンティニュー/設定/終了 | ✅ 実装済み |
| 設定画面 | Phase 6 | 音量調整、フォントサイズ | ✅ 実装済み |
| BGM/SE管理 | Phase 6 | AudioManager、10BGM+21SE定義 | ✅ 実装済み |
| 描画最適化 | Phase 6 | プーリング、Visibility切替 | ✅ 実装済み |
| NPC隣接インタラクト | Ver.prt.0.7 | 方向キーでNPC対話（タイル踏み不要） | ✅ 実装済み |
| ウィンドウトグル操作 | Ver.prt.0.7 | 8ウィンドウを開くキーで閉じる | ✅ 実装済み |
| 魔法詠唱ターン遅延 | Ver.prt.0.7 | TurnCost>1で詠唱状態、毎ターンログ | ✅ 実装済み |
| D&Dドロップ位置保持 | Ver.prt.0.7 | ドラッグ後のグリッド位置が正しく保存 | ✅ 実装済み |
| フロアキャッシュ分離 | Ver.prt.0.7 | ダンジョン別にレイアウト・探索状態を分離 | ✅ 実装済み |
| 町脱出方式変更 | Ver.prt.0.7 | マップ端移動で脱出（階段方式から変更） | ✅ 実装済み |
| キャラクター作成GUI画面 | Ver.α | 種族・職業選択、ステータス振り分け | ⬜ |
| アニメーション | Ver.α | 攻撃エフェクト、画面遷移 | ⬜ |
| 実際のBGM/SE音声ファイル | Ver.β | suno生成BGM、フリー素材SE配置 | ⬜ |
