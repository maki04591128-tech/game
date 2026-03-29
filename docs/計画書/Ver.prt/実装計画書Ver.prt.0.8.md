# 実装計画書 Ver.prt.0.8（NPC会話アクション・町脱出修正）

**目標**: NPC会話に選択肢付きアクションを追加、町脱出の階段使用時バグ修正、鍛冶屋・宿屋NPCに売買機能追加
**状態**: ✅ 全タスク完了 — テスト全体2,706件（GUIオートテスト除外）全合格
**前提**: Ver.prt.0.7（ゲームプレイ改善・バグ修正）完了済み
**完了時テスト数**: 全体 = 2,706件（Core 2,559 + GUI 147）

---

## 1. 概要

Ver.prt.0.8 は Ver.prt.0.7 完了後のプレイテストに基づき、
NPC会話のインタラクション強化と町脱出のバグ修正を行うフェーズである。

主な改善点:
- 町からの脱出時に階段（Shift+<）を使った場合の`_isInLocationMap`フラグ未クリアバグの修正
- NPC会話に選択肢を追加し、ギルド登録・宗教入信をNPCとの対話で行えるようにする
- 鍛冶屋・宿屋NPCに売買機能（ShopWindow連携）を追加
- Nキーのギルド登録ショートカットを廃止（NPC会話に統合）

---

## 2. タスク一覧

### Phase G: NPC会話アクション・町脱出修正（3タスク）

| # | タスク名 | 内容 | 状態 |
|---|---------|------|------|
| T.1 | 町脱出階段バグ修正 | TryAscendStairsでロケーションマップの場合はTryLeaveTownに委譲 | ✅ 完了 |
| T.2 | NPC会話選択肢追加 | HandleNpcTileを選択肢付きDialogueNodeに書き直し（5種NPC対応） | ✅ 完了 |
| T.3 | NPC会話アクションディスパッチ＋ショップ連携 | TrySelectDialogueChoiceにaction:プレフィックス検出、DispatchNpcAction、OnOpenShopイベント追加 | ✅ 完了 |

---

## 3. タスク詳細

### T.1: 町脱出階段バグ修正

**目的**: 町（ロケーションマップ）内で階段（Shift+<）を使った場合に正しく町脱出処理が行われるようにする

**根本原因**: `TryAscendStairs()`がロケーションマップ内かどうかを判定せず、ダンジョン脱出と同じ処理を実行していた。これにより`_isInLocationMap`フラグがクリアされず、以降の町脱出が正しく動作しなかった。

**変更内容**:
1. `TryAscendStairs()` の先頭に `_isInLocationMap` チェックを追加
2. ロケーションマップ内の場合は `TryLeaveTown()` に委譲して正しく町脱出処理を行う

**変更ファイル**: `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] 町内で階段を使った場合、TryLeaveTownが呼ばれ正しくシンボルマップに帰還する
- [x] _isInLocationMapフラグが正しくクリアされる
- [x] ダンジョン内の階段使用には影響しない
- [x] 既存テストが全て合格する

### T.2: NPC会話選択肢追加

**目的**: NPC会話をテキスト表示のみから、選択肢付きの対話システムに拡張する

**変更内容**:
1. `HandleNpcTile()` を書き直し、各NPCタイプに応じた `DialogueChoice[]` を含む `DialogueNode` を生成
2. 選択肢の `NextNodeId` に `"action:"` プレフィックスを使用してゲームアクションを識別
3. `_dialogueSystem.RegisterNode()` と `_dialogueSystem.StartDialogue()` を呼び出して会話を正式に開始

**NPC別選択肢**:

| NPCタイプ | 条件 | 選択肢 |
|-----------|------|--------|
| ギルド受付 | 未登録 | 「ギルドに登録する」(action:register_guild) / 「やめておく」(action:close) |
| ギルド受付 | 登録済 | 「クエストを確認する」(action:view_quests) / 「話を聞く」(action:close) |
| 神父 | 未入信 | 「光の神殿に入信する」/「闇の教団に入信する」/「自然信仰に入信する」/「死の信仰に入信する」 |
| 神父 | 入信済 | 「祈りを捧げる」(action:pray) / 「信仰情報を見る」(action:view_religion) / 「何もしない」(action:close) |
| 商人 | — | 「商品を見る」(action:open_shop_GeneralShop) / 「立ち去る」(action:close) |
| 鍛冶屋 | — | 「武器を見る」(action:open_shop_WeaponShop) / 「防具を見る」(action:open_shop_ArmorShop) / 「立ち去る」(action:close) |
| 宿屋主人 | — | 「宿に泊まる」(action:use_inn) / 「食料を買う」(action:open_shop_GeneralShop) / 「立ち去る」(action:close) |

**変更ファイル**: `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] 各NPCに話しかけると選択肢付き会話ウィンドウが表示される
- [x] ギルド受付の選択肢が登録状態に応じて切り替わる
- [x] 神父の選択肢が入信状態に応じて切り替わる
- [x] 既存テストが全て合格する

### T.3: NPC会話アクションディスパッチ＋ショップ連携

**目的**: NPC会話の選択肢からゲームアクション（ギルド登録、入信、売買等）を実行できるようにする

**変更内容**:
1. `TrySelectDialogueChoice()` を拡張: 選択前に `CurrentNode.Choices[choiceIndex].NextNodeId` を取得し、`"action:"` プレフィックスの場合は `DispatchNpcAction()` に委譲
2. `DispatchNpcAction()` メソッドを新規追加: アクション文字列に応じてゲームロジック（TryRegisterGuild, TryJoinReligion, TryPray, TryUseInn等）を実行
3. `OnOpenShop` イベント (`Action<FacilityType>`) を `GameController` に追加
4. `MainWindow.xaml.cs` で `OnOpenShop` をサブスクライブし、`ShopWindow` を表示
5. `MainWindow.xaml.cs` から N キー → `GameAction.RegisterGuild` のキーバインドを削除

**アクション一覧**:

| アクション文字列 | 実行内容 |
|-----------------|---------|
| `register_guild` | `TryRegisterGuild()` |
| `view_quests` | クエストログ表示メッセージ |
| `pray` | `TryPray()` |
| `view_religion` | 信仰情報表示（`OnReligionChanged` 発火） |
| `use_inn` | `TryUseInn()` |
| `close` | 会話終了（何もしない） |
| `open_shop_{FacilityType}` | `OnOpenShop` イベント発火 → ShopWindow表示 |
| `join_religion_{ReligionId}` | `TryJoinReligion(religionId)` |

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`
- `src/RougelikeGame.Gui/MainWindow.xaml.cs`

**受入基準**:
- [x] ギルド受付で「ギルドに登録する」を選ぶとギルドに登録される
- [x] 神父で宗教を選ぶと入信できる
- [x] 鍛冶屋で「武器を見る」を選ぶとShopWindowが開く
- [x] 宿屋で「宿に泊まる」を選ぶと宿泊処理が行われる
- [x] Nキーでのギルド登録ショートカットが廃止されている
- [x] 既存テストが全て合格する

---

## 4. 実装結果

| 項目 | 値 |
|------|-----|
| 変更ファイル数 | 2ファイル |
| テスト合計 | 2,706件（GUIオート除外） |
| テスト合格 | 2,706件（100%） |
| ビルドエラー | 0件 |

### 変更ファイル一覧

| ファイル | 変更種別 | 内容 |
|---------|----------|------|
| GameController.cs | 修正 | TryAscendStairs町脱出修正、HandleNpcTile選択肢追加、TrySelectDialogueChoice拡張、DispatchNpcAction追加、OnOpenShopイベント追加 |
| MainWindow.xaml.cs | 修正 | OnOpenShopサブスクリプション＋ShopWindow表示ハンドラー追加、Nキーバインド削除 |
