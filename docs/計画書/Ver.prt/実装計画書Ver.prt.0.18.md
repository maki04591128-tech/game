# 実装計画書 Ver.prt.0.18 — ダンジョン生成バグ修正

**ステータス**: ✅ 全タスク完了
**目的**: 60x30マップでダンジョンの部屋が1つしか生成されない致命的バグの修正
**テスト結果**: 5,462テスト全合格（Core: 5,462 + Gui: 148）

---

## タスク一覧

| # | タスク | カテゴリ | ステータス |
|---|--------|---------|-----------|
| 1 | 60x30マップでダンジョンの部屋が1つしか生成されない | バグ修正 | ✅ 完了 |

---

## 詳細

### タスク1: 60x30マップでダンジョンの部屋が1つしか生成されない

- **問題**: ダンジョンB1Fで部屋が1つだけ生成され、廊下も他の部屋もない状態が発生する
- **根本原因**: BSPアルゴリズムのパラメータが実際のゲームマップサイズ（60x30）に対して不整合
  - **GameControllerの実マップサイズ**: `Width=60, Height=30`（コード上のデフォルト80x50とは異なる）
  - **BSP領域**: 外周4マス除外で`(2, 2, 56, 26)`、有効高さ26
  - **SplitBSPのMinSize=8**: 高さ26を分割すると8〜18のノードが生成される
  - **CreateRoomInNodeの要件**: `Margin*2(4) + MinRoomSize(7) = 11`以上のノードサイズが必要
  - **不一致**: 高さ8〜10のノードでは`maxHeight = height - 4 = 4〜6 < MinRoomSize(7)`となり、部屋を生成できない
  - **結果**: 多くのリーフノードで`CreateRoomInNode`がnullを返し、極端な場合1部屋のみに
  - **forceMinRooms不足**: リトライ時の強制分割がdepth<3のみで、浅い階層の大きなノードしか分割しない
  - **最終リトライ問題**: MaxRetries=5の最後のリトライでもMinRooms未満の場合、その結果がそのまま使用されていた

- **修正内容**（4箇所の変更 + 1メソッド新規追加）:

  1. **SplitBSP MinSize 8→9**: BSP分割の最小ノードサイズを9に引き上げ。`Margin*2(4) + MinRoomSize(5) = 9`以上を保証し、全リーフノードで部屋生成が可能に
  2. **CreateRoomInNode MinRoomSize 7→5**: 部屋の最小サイズを壁込み5x5（歩行可能面3x3）に縮小。狭いBSPノードでも部屋を生成可能に
  3. **forceMinRooms depth<3→<4**: リトライ時の強制分割を1段階深くし、より多くのリーフノードを生成
  4. **PlaceFallbackRooms新規メソッド**: BSPリトライ最終回でもMinRooms未満の場合、マップの空き領域にランダムで部屋を追加配置。既存部屋との重なりチェック付き（マージン1マス）、最大100回試行
  5. **GenerateRooms統合**: リトライループ脱出後に`rooms.Count < MinRooms`判定でPlaceFallbackRoomsを呼び出し

- **変更ファイル**: `DungeonGenerator.cs`

---

## テスト追加

| テストファイル | テスト数 | 内容 |
|--------------|---------|------|
| `VersionPrt018SystemTests.cs` | 20 | 60x30マップ部屋数保証×10シード、入口/ボス部屋×1、階段配置×1、部屋重なり無し×1、マップ境界内×1、深層マップ部屋数×5シード、50x40リグレッション×1 |

### テスト内訳

| テスト名 | 内容 |
|---------|------|
| `Generate_60x30Map_AlwaysCreatesAtLeast5Rooms` (×10) | 10種のシードで60x30マップ生成し最低5部屋を確認 |
| `Generate_60x30Map_HasEntranceAndBossRoom` | 入口部屋とボス部屋が存在し正しいRoomTypeを持つ |
| `Generate_60x30Map_PlacesStairs` | 上り・下り階段が配置される |
| `Generate_60x30Map_RoomsDoNotOverlap` | 全部屋の矩形が重ならない |
| `Generate_60x30Map_RoomsWithinMapBounds` | 全部屋がマップ境界内に収まる |
| `Generate_60x30Map_DeeperFloors_AlsoCreatesSufficientRooms` (×5) | 深層(Depth=5, RoomCount=11)でも最低5部屋を確認 |
| `Generate_50x40Map_StillCreatesAtLeast5Rooms` | 従来テストサイズでリグレッションなし |

---

## 変更ファイル一覧

| ファイル | 変更種別 | 概要 |
|---------|---------|------|
| `src/RougelikeGame.Core/Map/Generation/DungeonGenerator.cs` | 修正 | SplitBSP MinSize 8→9、forceMinRooms depth<3→<4、CreateRoomInNode MinRoomSize 7→5、PlaceFallbackRooms新規メソッド追加、GenerateRoomsにフォールバック呼び出し追加 |
| `tests/RougelikeGame.Core.Tests/VersionPrt018SystemTests.cs` | 新規 | Phase18テスト20件（60x30マップ部屋数保証、重なり検証、境界検証、深層テスト、リグレッションテスト） |

---

## ブラッシュアップ記録

Phase 18全タスク完了後、プロジェクトガイドラインに従い自動ドキュメントブラッシュアップを実施。

### 更新対象ドキュメント

| ドキュメント | 更新内容 |
|-------------|---------|
| `docs/00_ドキュメント概要.md` | Ver.prt.0.18エントリ追加、フォルダ構成ツリーに0.18追加、バージョン範囲0.17→0.18更新、用語集にPlaceFallbackRooms追加 |
| `docs/計画書/マスター実装計画書.md` | バージョン体系にVer.prt.0.18追加、マイルストーン詳細にVer.prt.0.18セクション追加、フロー図に0.18追加、開発フェーズ概要テーブルにVer.prt.0.18行追加 |
| `docs/企画設計書/11_クラス設計書.md` | 実装状況テーブルにPhase 18変更4項目追加（BSP MinSize修正/MinRoomSize縮小/forceMinRooms強化/PlaceFallbackRooms新規） |
| `docs/企画設計書/14_マップシステム設計書.md` | 実装状況にVer.prt.0.18変更記録追加、BSP分割パラメータテーブルを最新値に更新（MinSize 9、MinRoomSize 5、forceMinRooms depth<4、フォールバック部屋配置追記） |
| `docs/企画設計書/17_デバッグ・テスト設計書.md` | テスト総数5,590→5,610、Core 5,442→5,462に更新、テストピラミッド/プロジェクト一覧/Core.Tests内訳を最新化、VersionPrt018SystemTests.csをファイル一覧に追加 |
| `docs/企画設計書/10_プロジェクト構造設計書.md` | テスト数5,590→5,610、Core 5,442→5,462/136→137ファイルに更新 |
