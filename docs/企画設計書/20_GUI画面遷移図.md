# GUI画面遷移図

> **📌 本書の目的**
>
> 各GUI画面間の遷移関係、操作キー、遷移条件を一覧化し、ユーザー操作フローを明確にする。
> 全20ウィンドウ（ダイアログ）の遷移を網羅。

---

## 1. 全体遷移概要図

```
┌─────────────────────────────────────────────────────────────────────┐
│                      アプリケーション起動                            │
│                     (App.xaml.cs)                                   │
└───────────────────────────┬─────────────────────────────────────────┘
                            │
                            ▼
              ┌──────────────────────────┐
              │     TitleWindow          │
              │     タイトル画面          │
              │                          │
              │  ▶ ニューゲーム ──────────┼──→ DifficultySelectWindow
              │  │                       │          │
              │  │                       │          ▼ 確定後
              │  │                       │    CharacterCreationWindow
              │  │                       │          │
              │  │                       │          ▼ 「開始」ボタン押下
              │  │                       │    種族・素性から初期マップ決定
              │  │                       │    （StartingMapResolver）
              │  │                       │          │
              │  │                       │          ▼
              │  │                       │    MainWindow起動
              │  │                       │    （初期マップでプレイ開始）
              │  │                       │
              │  ▶ コンティニュー ────────┼──→ SaveDataSelectWindow
              │  │                       │          │
              │  │                       │          ▼ スロット選択
              │  │                       │    MainWindow起動（セーブロード）
              │  │                       │
              │  ⚙ 設定 ────────────────┼──→ SettingsWindow → TitleWindowに戻る
              │  ✕ 終了 → アプリ終了     │
              │  LCtrl+RCtrl+D → デバッグ │──→ MainWindow（デバッグマップ起動）
              └──────────────────────────┘
                            │
                    選択完了後
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      MainWindow（メインゲーム画面）                   │
│                                                                     │
│  ┌──────────────── 直接キー操作 ─────────────────────────────┐      │
│  │                                                           │      │
│  │  I → InventoryWindow（インベントリ）                       │      │
│  │  C → StatusWindow（ステータス）                            │      │
│  │  L → MessageLogWindow（メッセージログ）                     │      │
│  │  V → SpellCastingWindow（魔法詠唱）                        │      │
│  │  K → QuestLogWindow（クエストログ）                         │      │
│  │  O → ReligionWindow（宗教）                                │      │
│  │  J → WorldMapWindow（領地移動）                            │      │
│  │  E → SkillTreeWindow（スキルツリー）                       │      │
│  │  1-5 → スキルスロット使用（割当済スキル発動）           │      │
│  │  ※ H（旧鍛冶）/ B（旧街入場）はキー削除済み            │      │
│  │  M → ミニマップ表示/非表示（画面内トグル）                   │      │
│  │  F5 → セーブ実行（ダイアログなし）                          │      │
│  │  F9 → ロード実行（ダイアログなし）                          │      │
│  │  Q → ゲーム終了                                            │      │
│  │                                                           │      │
│  └───────────────────────────────────────────────────────────┘      │
│                                                                     │
│  ┌──── GameControllerイベント起動 ──────────────────────────┐       │
│  │                                                          │       │
│  │  NPC会話 → DialogueWindow（会話）                         │       │
│  │  ギルド受付「仲間募集」 → RecruitCompanionWindow      │       │
│  │  ギルド受付「クエスト確認」 → QuestBoardWindow          │       │
│  │  領地移動 → TravelEventWindow（移動イベント）              │       │
│  │  チュートリアル → MessageBoxヒント表示                     │       │
│  │  死亡（正気度>0） → 死に戻り（画面内メッセージ、遷移なし） │       │
│  │  死亡（正気度=0、救済あり） → 救済+死に戻り（遷移なし）    │       │
│  │  真のゲームオーバー → MessageBox「タイトルに戻りますか？」 │       │
│  │    → 「はい」→ ExitReason=ReturnToTitle → TitleWindow   │       │
│  │    → 「いいえ」→ ExitReason=Quit → アプリ終了          │       │
│  │  ゲームクリア → スコア表示MessageBox                │       │
│  │    → NG+解放時：MessageBox「NG+を開始しますか？」       │       │
│  │      → 「はい」→ ExitReason=StartNewGamePlus              │       │
│  │               → 同キャラ設定でNG+ MainWindow起動     │       │
│  │      → 「いいえ」→ ExitReason=ReturnToTitle → TitleWindow│       │
│  │    → NG+未解放時：ExitReason=ReturnToTitle → TitleWindow  │       │
│  │  シンボルマップ街入場 → BGM切替（Town）                  │       │
│  │  シンボルマップダンジョン入場 → BGM切替（DungeonNormal）      │       │
│  │                                                          │       │
│  └──────────────────────────────────────────────────────────┘       │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. 画面遷移詳細

### 2.1 起動フロー（App → TitleWindow → MainWindow）

```
App.Application_Startup
  │
  ├─ --skip-title 引数あり → RunMainWindow() → Shutdown()
  ├─ --debug-map 引数あり → RunMainWindow(debugMap:true) → Shutdown()
  │
  └─ 通常起動 → RunGameLoop()
       │
       └─ while(true) ループ:
            │
            ├─ TitleWindow.ShowDialog()
            │    │
            │    ├─ 「ニューゲーム」選択
            │    │    │
            │    │    ├─ ❶ DifficultySelectWindow.ShowDialog()
            │    │    │    ├─ 難易度を選択して「確定」ボタン/Enter → 難易度決定
            │    │    │    │    難易度一覧:
            │    │    │    │    ┌──────────┬───────────────────────────────────┐
            │    │    │    │    │ Easy     │ 敵ダメージ0.7倍/EXP1.5倍/制限1.5倍│
            │    │    │    │    │ Normal   │ 標準バランス（推奨）               │
            │    │    │    │    │ Hard     │ 敵1.3倍/EXP0.8倍/制限0.8倍        │
            │    │    │    │    │ Nightmare│ 敵1.6倍/EXP0.6倍/制限0.6倍        │
            │    │    │    │    │ Ironman  │ 敵1.5倍/EXP0.7倍/死亡時セーブ削除 │
            │    │    │    │    └──────────┴───────────────────────────────────┘
            │    │    │    └─ キャンセル/Esc → TitleWindowに戻る
            │    │    │
            │    │    ├─ ❷ CharacterCreationWindow.ShowDialog()
            │    │    │    ├─ 操作内容:
            │    │    │    │    ├─ 種族選択（10種族リスト）
            │    │    │    │    ├─ 職業選択（10職業リスト）
            │    │    │    │    ├─ 素性選択（10素性リスト）
            │    │    │    │    ├─ 名前入力（テキストボックス、デフォルト「冒険者」）
            │    │    │    │    └─ ステータスプレビュー（種族+職業+素性のボーナス反映値）
            │    │    │    │         STR/VIT/AGI/DEX/INT/MND/PER/LUK/CHA + HP/MP + 初期金 + 特性/初期スキル
            │    │    │    │
            │    │    │    ├─ 「開始」ボタン/Enter → キャラ情報確定
            │    │    │    │    └─ TitleWindow.SelectedAction = NewGame → TitleWindow閉じる
            │    │    │    └─ キャンセル/Esc → TitleWindowに戻る（難易度選択には戻らない）
            │    │    │
            │    │    └─ ❸ App.RunMainWindow() → MainWindow起動
            │    │         │
            │    │         └─ MainWindow.Window_Loaded()
            │    │              └─ GameController.Initialize(name, race, class, background, difficulty)
            │    │                   ├─ 種族・素性から初期マップ名を決定（StartingMapResolver）
            │    │                   ├─ 開始領地を設定（GetStartingTerritory）
            │    │                   ├─ プレイヤー作成（Player.Create）
            │    │                   ├─ 初期装備支給（鉄の剣 + 革の鎧）
            │    │                   ├─ 初期アイテム支給（回復薬×2 + パン×1）
            │    │                   ├─ ダンジョン第1層マップ生成（GenerateFloor）
            │    │                   └─ プレイ画面表示（初期マップでゲーム開始）
            │    │
            │    ├─ 「コンティニュー」選択（セーブ存在時のみ有効）
            │    │    └─ SaveDataSelectWindow.ShowDialog()
            │    │         ├─ スロット選択 → MainWindow起動
            │    │         │    └─ Window_Loaded → GameController.Initialize → SaveManager.Load → LoadSaveData
            │    │         └─ キャンセル/Esc → TitleWindowに戻る
            │    │
            │    ├─ 「設定」選択
            │    │    └─ SettingsWindow.ShowDialog()
            │    │         ├─ 保存 → 設定反映（JSON永続化）、TitleWindowに戻る
            │    │         └─ キャンセル/Esc → 変更破棄、TitleWindowに戻る
            │    │
            │    ├─ 「終了」/ Esc → アプリケーション終了
            │    │
            │    └─ LCtrl+RCtrl+D → MainWindow（デバッグマップ起動）
            │         └─ GameController.InitializeDebug()
            │              ├─ デフォルトプレイヤー作成（STR14 etc.）
            │              ├─ テスト用装備・アイテム支給
            │              └─ 32×24テストアリーナ生成
            │
            └─ MainWindow終了後の分岐（GameExitReason）:
                 ├─ ReturnToTitle → ループ継続（再びTitleWindowへ）
                 ├─ StartNewGamePlus → NG+用MainWindow起動 → 終了後ループ継続
                 └─ Quit → Shutdown()（アプリ終了）
```

### 2.1.1 種族・素性による初期マップ決定（StartingMapResolver）

ニューゲーム開始時、選択した**素性**を優先して初期マップが決定されます。素性が「冒険者（Adventurer）」の場合のみ**種族**で判定されます。

**素性別の初期マップ（素性優先判定）:**

| 素性 | 初期マップ名 | 表示名 | 開始領地 |
|------|-------------|--------|---------|
| Soldier（兵士） | capital_barracks | 王都・兵舎 | Capital（王都） |
| Scholar（学者） | capital_academy | 王都・学院 | Capital |
| Merchant（商人） | capital_market | 王都・市場通り | Capital |
| Peasant（農民） | capital_slums | 王都・貧民街 | Capital |
| Noble（貴族） | capital_manor | 王都・貴族邸 | Capital |
| Criminal（犯罪者） | capital_prison | 王都・牢獄 | Capital |
| Priest（聖職者） | capital_cathedral | 王都・大聖堂 | Capital |
| Penitent（贖罪者） | capital_monastery | 王都・修道院 | Capital |
| Wanderer（放浪者） | wanderer_camp | 流浪者の野営地 | Capital |
| Adventurer（冒険者） | ※種族で判定 | ※下表参照 | ※種族による |

**種族別の初期マップ（素性がAdventurerの場合）:**

| 種族 | 初期マップ名 | 表示名 | 開始領地 |
|------|-------------|--------|---------|
| Human（人間） | capital_guild | 王都・冒険者ギルド | Capital |
| Elf（エルフ） | forest_village | 森の集落 | Forest（森林） |
| Dwarf（ドワーフ） | mountain_hold | 山岳砦 | Mountain（山岳） |
| Halfling（ハーフリング） | coast_port | 海岸港町 | Coast（沿岸） |
| Orc（オーク） | mountain_hold | 山岳砦 | Mountain |
| Beastfolk（獣人） | forest_village | 森の集落 | Forest |
| Undead（アンデッド） | underground_ruins | 地下遺跡 | Capital |
| Demon（悪魔） | dark_sanctuary | 暗黒聖域 | Capital |
| FallenAngel（堕天使） | fallen_temple | 堕天の神殿 | Capital |
| Slime（スライム） | swamp_den | 沼地の洞窟 | Capital |

> **補足**: 現時点では全マップで同じダンジョン構造（BSP法）が生成されますが、マップ名は管理用識別子として保持されており、後ほどマップ名ごとに異なる構造・演出を実装可能です。

### 2.2 MainWindow からの遷移一覧

| 操作 | キー | 遷移先 | 戻り方 | 備考 |
|------|------|--------|--------|------|
| インベントリ | I | InventoryWindow | Esc / アイテム使用後 | 数字キーで選択、Enter使用 |
| ステータス | C | StatusWindow | Esc / C | 表示のみ |
| メッセージログ | L | MessageLogWindow | Esc | フィルタ・スクロール操作あり |
| 魔法詠唱 | V | SpellCastingWindow | Esc（キャンセル）/ C（詠唱実行） | ルーン語選択・追加・削除 |
| クエストログ | K | QuestLogWindow | Esc | クエスト受注/報告可能 |
| 宗教 | O | ReligionWindow | Esc | 入信/離脱/祈り操作 |
| ワールドマップ | J | WorldMapWindow | Esc | 領地移動/街入場 |
| 合成工房 | H | CraftingWindow | Esc | 合成/強化/エンチャント |
| 街入場 | B | TownWindow | Esc | 地上時のみ、施設利用 |
| ミニマップ | M | —（内部トグル） | M | 表示/非表示切り替え |
| セーブ | F5 | —（即時実行） | — | ダイアログなし |
| ロード | F9 | —（即時実行） | — | ダイアログなし |
| ゲーム終了 | Q | アプリ終了 | — | — |

### 2.3 GameControllerイベントによる自動遷移

| トリガー | 遷移先 | 条件 |
|---------|--------|------|
| NPC話しかけ | DialogueWindow | NPC隣接+インタラクション |
| 領地間移動開始 | TravelEventWindow | WorldMapWindowから移動確定時 |
| チュートリアル発生 | MessageBox | TutorialTrigger発生時 |
| 死亡（正気度>0） | 画面遷移なし（死に戻り） | HP0+正気度残存 |
| 死亡（正気度=0、救済あり） | 画面遷移なし（救済+死に戻り） | HP0+正気度0+救済回数>0 |
| 真のゲームオーバー | MessageBox → MainWindow終了 | HP0+正気度0+救済回数0 or ターン制限超過 |

### 2.4 死亡・ゲームオーバー時の遷移フロー

プレイヤーが死亡した際の処理は正気度と救済回数によって3パターンに分岐します。

```
プレイヤー死亡（HP=0 等）
  │
  ├─ 正気度 > 0 → 「死に戻り」実行
  │    ├─ メッセージ表示（MainWindow内、画面遷移なし）
  │    ├─ プレイヤー再作成（肉体リセット、知識引き継ぎ）
  │    ├─ 初期装備再支給（鉄の剣 + 革の鎧 + 回復薬 + パン）
  │    ├─ フロア1から再開
  │    └─ そのままMainWindowでプレイ続行
  │
  ├─ 正気度 = 0、救済回数 > 0 → 「廃人化からの救済」実行
  │    ├─ メッセージ表示（MainWindow内、画面遷移なし）
  │    ├─ 正気度を20まで回復
  │    ├─ 知識（ルーン語・スキル）喪失
  │    ├─ 救済回数を消費
  │    ├─ 死に戻り処理（上記と同様）
  │    └─ そのままMainWindowでプレイ続行
  │
  └─ 正気度 = 0、救済回数 = 0 → 「真のゲームオーバー」
       ├─ または 正気度 > 0 でもゲーム終了条件（ターン制限超過等）
       │
       ├─ GameOverBGM再生
       ├─ MessageBox表示
       │    ├─「ゲームオーバー」（HP0時）: 死亡階層+ゲーム内時刻
       │    ├─「ゲームオーバー」（ターン制限超過時）: 到達階層+時刻
       │    └─「冒険終了」（その他）: 到達階層+時刻
       ├─ OK ボタン押下
       └─ MainWindow.Close() → アプリケーション終了
```

> **重要**: 死に戻り（正気度>0）と救済（正気度=0+救済残あり）はMainWindow内で処理が完結し、画面遷移は発生しません。タイトル画面には戻りません。真のゲームオーバーのみMessageBox表示後にMainWindowが閉じてアプリが終了します。

---

## 3. 各画面のキー操作一覧

### 3.1 TitleWindow（タイトル画面）

| キー | 動作 |
|------|------|
| W / ↑ | カーソル上移動 |
| S / ↓ | カーソル下移動 |
| Enter / Space | 選択項目の決定 |
| Esc | 終了 |
| LCtrl+RCtrl+D | デバッグマップ起動（隠しコマンド） |

### 3.2 MainWindow（メインゲーム画面）

#### 移動操作
| キー | 動作 |
|------|------|
| W / ↑ | 上移動 |
| S / ↓ | 下移動 |
| A / ← | 左移動 |
| D / → | 右移動 |
| W+D / ↑+→ 同時押し | 右上斜め移動 |
| W+A / ↑+← 同時押し | 左上斜め移動 |
| S+D / ↓+→ 同時押し | 右下斜め移動 |
| S+A / ↓+← 同時押し | 左下斜め移動 |
| Home | 左上斜め移動（単独） |
| PgUp | 右上斜め移動（単独） |
| End | 左下斜め移動（単独） |
| PgDn | 右下斜め移動（単独） |

#### アクション操作
| キー | 動作 |
|------|------|
| Space | 待機（1ターン消費） |
| G | アイテム拾う |
| Shift + . | 階段を降りる |
| Shift + , | 階段を上る |
| Tab | 自動探索開始/停止 |
| F | 周囲探索（隠しドア・罠発見） |
| X | ドア閉じる |
| R | 遠隔攻撃（射撃） |
| T | アイテム投擲 |
| P | 祈り（宗教行動） |
| E | スキル使用 |
| N | ギルド登録 |

#### 画面遷移操作
| キー | 遷移先 |
|------|--------|
| I | InventoryWindow |
| C | StatusWindow |
| L | MessageLogWindow |
| V | SpellCastingWindow |
| K | QuestLogWindow |
| O | ReligionWindow |
| J | WorldMapWindow |
| H | CraftingWindow |
| B | TownWindow |
| M | ミニマップ表示/非表示 |
| F5 | セーブ実行 |
| F9 | ロード実行 |
| Q | ゲーム終了 |

### 3.3 DifficultySelectWindow（難易度選択画面）

| キー | 動作 |
|------|------|
| Enter | 選択確定 |
| Esc | キャンセル（TitleWindowに戻る） |

> ※マウス操作: 難易度リストから選択（Easy/Normal/Hard/Nightmare/Ironman）、詳細パラメータ表示、確定/キャンセルボタン

**難易度一覧:**

| 難易度 | 敵ダメージ倍率 | 経験値倍率 | ターン制限倍率 | 特殊ルール |
|--------|-------------|-----------|-------------|-----------|
| Easy | 0.7倍 | 1.5倍 | 1.5倍 | — |
| Normal | 1.0倍 | 1.0倍 | 1.0倍 | — |
| Hard | 1.3倍 | 0.8倍 | 0.8倍 | — |
| Nightmare | 1.6倍 | 0.6倍 | 0.6倍 | — |
| Ironman | 1.5倍 | 0.7倍 | — | 死亡時セーブデータ削除 |

### 3.4 CharacterCreationWindow（キャラクター作成画面）

| キー | 動作 |
|------|------|
| Enter | 作成確定（「開始」ボタンと同等） |
| Esc | キャンセル（TitleWindowに戻る） |

> ※マウス操作: 種族/職業/素性をリストから選択、名前をテキストボックスに入力

**操作フロー:**
1. **種族選択**（10種族）: リストから選択 → ステータスプレビューが自動更新
2. **職業選択**（10職業）: リストから選択 → ステータスプレビューが自動更新
3. **素性選択**（10素性）: リストから選択 → ステータスプレビューが自動更新
4. **名前入力**: テキストボックスに名前を入力（空欄の場合は「冒険者」がデフォルト）
5. **ステータスプレビュー**: 種族+職業+素性のボーナスを反映した最終ステータスをリアルタイム表示
   - 基本9ステータス（STR/VIT/AGI/DEX/INT/MND/PER/LUK/CHA）
   - HP/MP、初期所持金、種族特性、初期スキル
6. **「開始」ボタン/Enter** → キャラ情報確定 → 初期マップ決定 → MainWindow起動

**確定後の初期化処理:**
```
「開始」ボタン押下
  │
  ├─ CharacterCreationWindow閉じる（Confirmed = true）
  ├─ TitleWindow閉じる（SelectedAction = NewGame）
  │
  └─ App.StartMainWindow() → MainWindow生成
       └─ Window_Loaded → GameController.Initialize()
            ├─ StartingMapResolver.Resolve(race, background)
            │    → 素性優先で初期マップ名決定（詳細はセクション2.1.1参照）
            ├─ StartingMapResolver.GetStartingTerritory(mapName)
            │    → 初期領地決定（Capital/Forest/Mountain/Coast）
            ├─ Player.Create(name, race, class, background)
            │    → 種族/職業/素性のステータスボーナス適用
            ├─ 初期装備: 鉄の剣（装備）+ 革の鎧（装備）
            ├─ 初期アイテム: 回復薬×2 + パン×1
            ├─ GenerateFloor() → ダンジョン第1層マップ生成（BSP法）
            └─ メッセージ表示:「{マップ表示名} ─ ダンジョン第1層に入った！」
```

### 3.5 SaveDataSelectWindow（セーブデータ選択画面）

| キー | 動作 |
|------|------|
| Enter | スロット選択確定 |
| Delete | 選択中のセーブデータ削除 |
| Esc | キャンセル（TitleWindowに戻る） |

### 3.6 SettingsWindow（設定画面）

| キー | 動作 |
|------|------|
| Esc | キャンセル（変更を破棄して閉じる） |

> ※マウス操作: スライダーで音量/フォントサイズ調整、保存/キャンセル/初期値ボタン

### 3.7 InventoryWindow（インベントリ画面）

| キー | 動作 |
|------|------|
| 1〜9 | アイテム番号選択 |
| Enter | 選択アイテムを使用/装備 |
| Esc | 閉じる（MainWindowに戻る） |

### 3.8 StatusWindow（ステータス画面）

| キー | 動作 |
|------|------|
| Esc / C | 閉じる（MainWindowに戻る） |

### 3.9 MessageLogWindow（メッセージログ画面）

| キー | 動作 |
|------|------|
| ↑ | 上スクロール |
| ↓ | 下スクロール |
| Home | 先頭にジャンプ |
| End | 末尾にジャンプ |
| Esc | 閉じる（MainWindowに戻る） |

> ※マウス操作: フィルタボタン（全て/戦闘/アイテム/システム/探索）でメッセージ絞り込み

### 3.10 SpellCastingWindow（魔法詠唱画面）

| キー | 動作 |
|------|------|
| Enter | 選択中のルーン語を詠唱に追加 |
| Delete / BackSpace | 最後に追加したルーン語を削除 |
| C | 詠唱実行 |
| Esc | 詠唱キャンセル（MainWindowに戻る） |

> ※マウス操作: ルーン語リストから選択、追加/削除/詠唱ボタン

### 3.11 QuestLogWindow（クエストログ画面）

| キー | 動作 |
|------|------|
| Enter | アクティブクエスト: 報告 / 受注可能クエスト: 受注 |
| Esc | 閉じる（MainWindowに戻る） |

> ※マウス操作: タブ切り替え（アクティブ/受注可能）、クエスト選択、受注/報告ボタン

### 3.12 ReligionWindow（宗教画面）

| キー | 動作 |
|------|------|
| Esc | 閉じる（MainWindowに戻る） |

> ※マウス操作: 宗教選択、入信/離脱/祈りボタン

### 3.13 WorldMapWindow（ワールドマップ画面）

| キー | 動作 |
|------|------|
| Enter | 選択中の領地へ移動 |
| T | 街に入る（地上＋施設あり時のみ） |
| Esc | 閉じる（MainWindowに戻る） |

> ※マウス操作: 領地リストから選択、移動/街入場/閉じるボタン

**WorldMapWindow遷移先:**

```
WorldMapWindow
  ├─ 「移動」確定 → MainWindow（領地移動処理実行）
  │                   └→ TravelEventWindow（移動イベント発生時）
  ├─ 「街に入る」→ MainWindow経由 → TownWindow
  └─ Esc/閉じる → MainWindow
```

### 3.14 CraftingWindow（合成工房画面）

| キー | 動作 |
|------|------|
| Enter | 合成/強化/エンチャント実行 |
| Esc | 閉じる（MainWindowに戻る） |

> ※マウス操作: モード切り替えタブ（合成/強化/エンチャント）、アイテム選択、実行ボタン

### 3.15 TownWindow（街施設メニュー画面）

| キー | 動作 |
|------|------|
| Enter | 選択施設を利用 |
| D | ダンジョンに入る |
| Esc | 閉じる（MainWindowに戻る） |

**TownWindow内部遷移:**

```
TownWindow
  ├─ 宿屋 → 利用処理（GameAction.UseInn）
  ├─ 教会 → 利用処理（GameAction.VisitChurch）
  ├─ 銀行 → 銀行パネル表示（入出金操作）
  ├─ 雑貨店/武器店/防具店/魔法店 → TownWindow閉じる → ShopWindow
  ├─ 鍛冶屋 → CraftingWindow
  ├─ 冒険者ギルド → ギルド登録 → QuestLogWindow
  ├─ 神殿 → ReligionWindow
  ├─ 図書館 → ルーン語学習（MessageBox）
  ├─ 闘技場 → 準備中メッセージ（MessageBox）
  └─ ダンジョン入場 → MainWindow（階段降下処理）
```

### 3.16 ShopWindow（ショップ画面）

| キー | 動作 |
|------|------|
| Enter | 売買実行 |
| Esc | 閉じる（MainWindowに戻る） |

> ※マウス操作: タブ切り替え（購入/売却）、アイテム選択、売買ボタン

### 3.17 DialogueWindow（会話画面）

| キー | 動作 |
|------|------|
| Enter / Space | 会話を進める |
| 1〜4 | 選択肢を選ぶ（選択肢表示時） |
| Esc | 会話終了（MainWindowに戻る） |

> ※Ver.prt.0.8より、NPC会話の選択肢からゲームアクション（ギルド登録、宗教入信、ショップ表示、宿泊、祈り）を実行可能。選択肢のNextNodeIdに`action:`プレフィックスを使用し、GameController.DispatchNpcAction()でディスパッチされる。

### 3.18 TravelEventWindow（領地間移動イベント画面）

| キー | 動作 |
|------|------|
| 1 | 強行突破（ボタン表示時のみ） |
| 2 | 交渉（ボタン表示時のみ） |
| 3 | 回避 |
| Esc | 結果確認後に閉じる |

---

## 4. 遷移フロー図（全体）

```
[アプリ起動]
    │
    ▼
[TitleWindow]
    │
    ├─ ニューゲーム ──→ [DifficultySelectWindow] ─確定→ [CharacterCreationWindow]
    │                         │ Esc                        │ Esc
    │                         └→ [TitleWindow]              └→ [TitleWindow]
    │                                                       │ 「開始」ボタン/Enter
    │                                                       ▼
    │                                              種族・素性から初期マップ決定
    │                                              （StartingMapResolver）
    │                                                       │
    │                                                       ▼
    ├─────────────────────────────────────────────→ [MainWindow]
    │                                              （初期マップでプレイ開始）
    │
    ├─ コンティニュー ─→ [SaveDataSelectWindow]
    │                         │ Esc
    │                         └→ [TitleWindow]
    │
    ├─ 設定 ───────────→ [SettingsWindow]
    │                         │ Esc/保存
    │                         └→ [TitleWindow]
    │
    └─ 終了 / Esc ────→ [アプリ終了]

[MainWindow]
    │
    ├─ I ────→ [InventoryWindow] ──── Esc ──→ [MainWindow]
    ├─ C ────→ [StatusWindow] ─────── Esc/C → [MainWindow]
    ├─ L ────→ [MessageLogWindow] ─── Esc ──→ [MainWindow]
    ├─ V ────→ [SpellCastingWindow] ─ Esc/C → [MainWindow]
    ├─ K ────→ [QuestLogWindow] ───── Esc ──→ [MainWindow]
    ├─ O ────→ [ReligionWindow] ───── Esc ──→ [MainWindow]
    ├─ ~~H~~ → （削除済み：旧CraftingWindowキー。TownWindow鍛冶屋経由のみ）
    │
    ├─ J ────→ [WorldMapWindow]
    │              ├─ 移動確定 ──────────────→ [MainWindow]（領地移動）
    │              │                                └→ [TravelEventWindow] → [MainWindow]
    │              └─ Esc ──────────────────→ [MainWindow]
    │
    ├─ ~~B~~ → （削除済み：旧TownWindowキー。シンボルマップ階段経由で[TownWindow]へ）
    │              ├─ ショップ系施設 ────────→ [ShopWindow] → [MainWindow]
    │              ├─ 鍛冶屋 ───────────────→ [CraftingWindow] → [TownWindow]
    │              ├─ 冒険者ギルド ──────────→ [QuestLogWindow] → [TownWindow]
    │              ├─ 神殿 ─────────────────→ [ReligionWindow] → [TownWindow]
    │              ├─ ダンジョン入場 ────────→ [MainWindow]（階段降下）
    │              └─ Esc ──────────────────→ [MainWindow]
    │
    ├─ NPC会話 → [DialogueWindow] ──── Esc ──→ [MainWindow]
    │              ├─ 選択肢「商品を見る」等 → [ShopWindow] → [MainWindow]
    │              ├─ 選択肢「ギルド登録」等 → アクション実行 → [MainWindow]
    │              └─ 選択肢「入信する」等 ─→ アクション実行 → [MainWindow]
    │
    ├─ 死亡（正気度>0）───→ 死に戻り処理 → [MainWindow]（フロア1から再開）
    ├─ 死亡（正気度=0+救済あり）→ 救済処理 → [MainWindow]（フロア1から再開）
    ├─ 真のゲームオーバー → [MessageBox] ──→ [MainWindow終了] → [アプリ終了]
    │
    └─ Q ────→ [アプリ終了]
```

---

## 5. 画面遷移の原則

| 原則 | 説明 |
|------|------|
| Esc統一 | 全サブウィンドウはEscキーで閉じてMainWindowに戻る |
| ShowDialog | 全サブウィンドウはモーダルダイアログ（MainWindowは操作不可） |
| Owner設定 | 全サブウィンドウの `Owner = MainWindow`（or TitleWindow） |
| Focus復帰 | サブウィンドウ閉じた後、MainWindowの `Focus()` を呼び出し |
| 自動探索停止 | サブウィンドウ表示時に `StopAutoExploreTimer()` を呼び出し |
| BGM継続 | サブウィンドウ表示中もBGMは継続再生 |

---

## 6. 補足：TownWindow内のサブ画面遷移

TownWindowは他のウィンドウと異なり、施設に応じて**さらにサブウィンドウを開く**ことがあります。

```
[TownWindow]
    │
    ├─ Inn（宿屋）─────→ 即時処理（HP/MP回復、ゴールド消費）
    ├─ Church（教会）───→ 即時処理（状態回復、ゴールド消費）
    ├─ Bank（銀行）────→ TownWindow内の銀行パネル（入出金テキストボックス操作）
    │
    ├─ GeneralShop ────→ TownWindow閉じる → MainWindow経由 → [ShopWindow]
    ├─ WeaponShop ─────→ TownWindow閉じる → MainWindow経由 → [ShopWindow]
    ├─ ArmorShop ──────→ TownWindow閉じる → MainWindow経由 → [ShopWindow]
    ├─ MagicShop ──────→ TownWindow閉じる → MainWindow経由 → [ShopWindow]
    │
    ├─ Smithy（鍛冶屋）→ [CraftingWindow]（TownWindow内から直接）
    ├─ Guild（ギルド）──→ ギルド登録 → [QuestLogWindow]（TownWindow内から直接）
    ├─ Temple（神殿）───→ [ReligionWindow]（TownWindow内から直接）
    ├─ Library（図書館）→ ルーン語学習 → [MessageBox]
    ├─ Arena（闘技場）──→ 準備中 → [MessageBox]
    │
    └─ Dungeon入場 ────→ TownWindow閉じる → MainWindow（UseStairs処理）
```

> **注意**: ショップ系施設（GeneralShop/WeaponShop/ArmorShop/MagicShop）はTownWindowが閉じてからMainWindow経由でShopWindowが開きます。
> 鍛冶屋・ギルド・神殿のサブウィンドウはTownWindow内から直接開きます（Owner = MainWindow）。

---

## 7. ウィンドウ一覧サマリー

| # | ウィンドウ | ファイル | 起動元 | 起動方法 |
|---|-----------|---------|--------|---------|
| 1 | TitleWindow | TitleWindow.xaml/.cs | App.xaml.cs | アプリ起動時 |
| 2 | DifficultySelectWindow | DifficultySelectWindow.xaml/.cs | TitleWindow | ニューゲーム選択時 |
| 3 | CharacterCreationWindow | CharacterCreationWindow.xaml/.cs | TitleWindow | 難易度選択後 |
| 4 | SaveDataSelectWindow | SaveDataSelectWindow.xaml/.cs | TitleWindow | コンティニュー選択時 |
| 5 | SettingsWindow | SettingsWindow.xaml/.cs | TitleWindow | 設定選択時 |
| 6 | MainWindow | MainWindow.xaml/.cs | App.xaml.cs | キャラ作成完了/ロード後 |
| 7 | InventoryWindow | InventoryWindow.xaml/.cs | MainWindow | Iキー |
| 8 | StatusWindow | StatusWindow.xaml/.cs | MainWindow | Cキー |
| 9 | MessageLogWindow | MessageLogWindow.xaml/.cs | MainWindow | Lキー |
| 10 | SpellCastingWindow | SpellCastingWindow.xaml/.cs | MainWindow | Vキー |
| 11 | QuestLogWindow | QuestLogWindow.xaml/.cs | MainWindow / TownWindow | Kキー / ギルド選択 |
| 12 | ReligionWindow | ReligionWindow.xaml/.cs | MainWindow / TownWindow | Oキー / 神殿選択 |
| 13 | CraftingWindow | CraftingWindow.xaml/.cs | TownWindow | —（旧Hキー削除済み：TownWindow鍛冶屋経由のみ） |
| 14 | WorldMapWindow | WorldMapWindow.xaml/.cs | MainWindow | Jキー（領地移動） |
| 15 | TownWindow | TownWindow.xaml/.cs | MainWindow | —（旧Bキー削除済み：シンボルマップ階段経由） |
| 16 | ShopWindow | ShopWindow.xaml/.cs | MainWindow | TownWindowショップ選択後 |
| 17 | DialogueWindow | DialogueWindow.xaml/.cs | MainWindow | NPC会話イベント |
| 18 | TravelEventWindow | TravelEventWindow.xaml/.cs | MainWindow | 領地間移動イベント |
| 19 | EncyclopediaWindow | EncyclopediaWindow.xaml/.cs | MainWindow | Yキー |
| 20 | DeathLogWindow | DeathLogWindow.xaml/.cs | MainWindow | Zキー |
| 21 | SkillTreeWindow | SkillTreeWindow.xaml/.cs | MainWindow | Eキー（スキルツリー表示） |
| 22 | CompanionWindow | CompanionWindow.xaml/.cs | MainWindow | Uキー |
| 23 | CookingWindow | CookingWindow.xaml/.cs | MainWindow / TownWindow | —（旧Hキー削除済み：TownWindow施設選択経由） |
| 24 | BaseManagementWindow | BaseManagementWindow.xaml/.cs | MainWindow | 拠点エリア進入時 |
| 25 | RecruitCompanionWindow | RecruitCompanionWindow.xaml/.cs | MainWindow | ギルド受付「仲間を募集する」選択時 |
| 26 | QuestBoardWindow | QuestBoardWindow.xaml/.cs | MainWindow | ギルド受付「クエストを確認する」選択時 |

---

## 8. セーブ/ロード時の内部フロー

### 8.1 セーブ（F5キー）

```
F5キー押下
  └─ MainWindow.HandleSaveGame()
       ├─ GameController.CreateSaveData() → SaveData生成
       ├─ SaveManager.Save(saveData) → JSONファイルに永続化
       └─ メッセージ表示「💾 ゲームをセーブした」
       ※ 画面遷移は発生しない（ダイアログなし）
```

### 8.2 ロード（F9キー）

```
F9キー押下
  └─ MainWindow.HandleLoadGame()
       ├─ SaveManager.Load() → SaveData読み込み
       ├─ GameController.LoadSaveData(saveData) → ゲーム状態復元
       ├─ メッセージ履歴クリア
       └─ メッセージ表示「💾 セーブデータをロードした」
       ※ 画面遷移は発生しない（MainWindow内で完結）
```

### 8.3 コンティニュー時のロード

```
TitleWindow「コンティニュー」→ SaveDataSelectWindow → スロット選択
  └─ MainWindow.Window_Loaded()
       ├─ GameController.Initialize() → 初期化（ニューゲームと同様）
       └─ SaveManager.Load(slotNumber) → セーブデータ上書きロード
```

---

## 9. 将来の遷移で未実装の画面・機能

以下は設計書やコードにIDが存在するが、現時点では画面遷移として完全に実装されていない項目です。

| 項目 | 現状 | 将来計画 |
|------|------|---------|
| 闘技場（Arena） | 「準備中」MessageBox表示 | Ver.prt.0.2以降で専用画面追加予定 |
| キャラクター作成GUI画面の拡張 | 現在はリスト選択のみ | Ver.αでステータス振り分け・詳細設定画面 |
| アニメーション・画面遷移効果 | 即時切り替え | Ver.αでフェードイン/アウト等 |
| ゲームオーバー後のタイトル戻り | MainWindow終了→アプリ終了 | 将来的にタイトル画面に戻る選択肢を追加検討 |
| ゲームクリア画面 | 未実装 | Ver.αでエンディング画面追加予定 |
| 実際のBGM/SE音声ファイル | SilentAudioManager使用 | Ver.βでsuno生成BGM/フリー素材SE |
| 正気度演出（画面効果） | 未実装 | Ver.prt.0.2以降で画面歪み・ノイズ等 |
