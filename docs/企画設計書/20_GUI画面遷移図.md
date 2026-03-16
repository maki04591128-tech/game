# GUI画面遷移図

> **📌 本書の目的**
>
> 各GUI画面間の遷移関係、操作キー、遷移条件を一覧化し、ユーザー操作フローを明確にする。
> 全17ウィンドウの遷移を網羅。

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
              │  ▶ ニューゲーム ──────────┼──→ DifficultySelectWindow → CharacterCreationWindow
              │  ▶ コンティニュー ────────┼──→ SaveDataSelectWindow
              │  ⚙ 設定 ────────────────┼──→ SettingsWindow
              │  ✕ 終了 → アプリ終了     │
              │  LCtrl+RCtrl+D → デバッグ │
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
│  │  J → WorldMapWindow（ワールドマップ）                       │      │
│  │  H → CraftingWindow（合成工房）                            │      │
│  │  B → TownWindow（街施設） ※地上時のみ                      │      │
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
│  │  領地移動 → TravelEventWindow（移動イベント）              │       │
│  │  チュートリアル → MessageBoxヒント表示                     │       │
│  │  ゲームオーバー → MessageBox → ウィンドウ終了              │       │
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
  ├─ --skip-title 引数あり → MainWindow（ニューゲーム即開始）
  ├─ --debug-map 引数あり → MainWindow（デバッグマップ）
  │
  └─ 通常起動 → TitleWindow.ShowDialog()
       │
       ├─ 「ニューゲーム」選択
       │    ├─ DifficultySelectWindow.ShowDialog()
       │    │    ├─ 確定 → 難易度決定
       │    │    └─ キャンセル/Esc → TitleWindowに戻る
       │    │
       │    └─ CharacterCreationWindow.ShowDialog()
       │         ├─ 確定 → キャラ情報決定 → MainWindow起動
       │         └─ キャンセル/Esc → TitleWindowに戻る
       │
       ├─ 「コンティニュー」選択（セーブ存在時のみ有効）
       │    └─ SaveDataSelectWindow.ShowDialog()
       │         ├─ スロット選択 → MainWindow起動（ロード）
       │         └─ キャンセル/Esc → TitleWindowに戻る
       │
       ├─ 「設定」選択
       │    └─ SettingsWindow.ShowDialog()
       │         ├─ 保存 → 設定反映、TitleWindowに戻る
       │         └─ キャンセル/Esc → TitleWindowに戻る
       │
       ├─ 「終了」/ Esc → アプリケーション終了
       │
       └─ LCtrl+RCtrl+D → MainWindow（デバッグマップ起動）
```

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
| ゲームオーバー | MessageBox → ウィンドウ終了 | HP0/ターン制限超過 |

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

### 3.4 CharacterCreationWindow（キャラクター作成画面）

| キー | 動作 |
|------|------|
| Enter | 作成確定 |
| Esc | キャンセル（TitleWindowに戻る） |

> ※マウス操作: 種族/職業/素性をリストから選択、名前をテキストボックスに入力

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
    ├─ ニューゲーム ──→ [DifficultySelectWindow] ──→ [CharacterCreationWindow]
    │                         │ Esc                        │ Esc
    │                         └→ [TitleWindow]              └→ [TitleWindow]
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

    ※ ニューゲーム完了 or コンティニュー完了
    │
    ▼
[MainWindow]
    │
    ├─ I ────→ [InventoryWindow] ──── Esc ──→ [MainWindow]
    ├─ C ────→ [StatusWindow] ─────── Esc/C → [MainWindow]
    ├─ L ────→ [MessageLogWindow] ─── Esc ──→ [MainWindow]
    ├─ V ────→ [SpellCastingWindow] ─ Esc/C → [MainWindow]
    ├─ K ────→ [QuestLogWindow] ───── Esc ──→ [MainWindow]
    ├─ O ────→ [ReligionWindow] ───── Esc ──→ [MainWindow]
    ├─ H ────→ [CraftingWindow] ───── Esc ──→ [MainWindow]
    │
    ├─ J ────→ [WorldMapWindow]
    │              ├─ 移動確定 ──────────────→ [MainWindow]（領地移動）
    │              │                                └→ [TravelEventWindow] → [MainWindow]
    │              ├─ 街入場 ────────────────→ [TownWindow]
    │              └─ Esc ──────────────────→ [MainWindow]
    │
    ├─ B ────→ [TownWindow]
    │              ├─ ショップ系施設 ────────→ [ShopWindow] → [MainWindow]
    │              ├─ 鍛冶屋 ───────────────→ [CraftingWindow] → [TownWindow]
    │              ├─ 冒険者ギルド ──────────→ [QuestLogWindow] → [TownWindow]
    │              ├─ 神殿 ─────────────────→ [ReligionWindow] → [TownWindow]
    │              ├─ ダンジョン入場 ────────→ [MainWindow]（階段降下）
    │              └─ Esc ──────────────────→ [MainWindow]
    │
    ├─ NPC会話 → [DialogueWindow] ──── Esc ──→ [MainWindow]
    ├─ ゲームオーバー → [MessageBox] ─────────→ [ウィンドウ終了]
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
| 13 | CraftingWindow | CraftingWindow.xaml/.cs | MainWindow / TownWindow | Hキー / 鍛冶屋選択 |
| 14 | WorldMapWindow | WorldMapWindow.xaml/.cs | MainWindow | Jキー |
| 15 | TownWindow | TownWindow.xaml/.cs | MainWindow | Bキー（地上時） |
| 16 | ShopWindow | ShopWindow.xaml/.cs | MainWindow | TownWindowショップ選択後 |
| 17 | DialogueWindow | DialogueWindow.xaml/.cs | MainWindow | NPC会話イベント |
| 18 | TravelEventWindow | TravelEventWindow.xaml/.cs | MainWindow | 領地間移動イベント |
