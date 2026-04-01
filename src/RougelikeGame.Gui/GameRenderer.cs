using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Map;

namespace RougelikeGame.Gui;

/// <summary>
/// ゲームマップのレンダリングを担当
/// </summary>
public class GameRenderer
{
    private readonly Canvas _canvas;
    private readonly Dictionary<Position, Rectangle> _tileRects = new();
    private readonly Dictionary<Position, TextBlock> _tileTexts = new();

    /// <summary>再利用可能なRectangleプール</summary>
    private readonly List<Rectangle> _rectPool = new();
    /// <summary>再利用可能なTextBlockプール</summary>
    private readonly List<TextBlock> _textPool = new();
    /// <summary>現在使用中のRectangleインデックス</summary>
    private int _rectPoolIndex;
    /// <summary>現在使用中のTextBlockインデックス</summary>
    private int _textPoolIndex;

    private const int TileSize = 20;
    private const string FontFamily = "Consolas";

    // タイル色定義
    private static readonly Dictionary<TileType, (Brush Background, Brush Foreground)> TileColors = new()
    {
        { TileType.Floor, (new SolidColorBrush(Color.FromRgb(30, 30, 40)), Brushes.Gray) },
        { TileType.Wall, (new SolidColorBrush(Color.FromRgb(60, 60, 80)), Brushes.DarkGray) },
        { TileType.Corridor, (new SolidColorBrush(Color.FromRgb(25, 25, 35)), Brushes.Gray) },
        { TileType.DoorClosed, (new SolidColorBrush(Color.FromRgb(139, 90, 43)), Brushes.SaddleBrown) },
        { TileType.DoorOpen, (new SolidColorBrush(Color.FromRgb(100, 60, 30)), Brushes.Peru) },
        { TileType.StairsUp, (new SolidColorBrush(Color.FromRgb(30, 60, 100)), Brushes.CornflowerBlue) },
        { TileType.StairsDown, (new SolidColorBrush(Color.FromRgb(30, 80, 50)), Brushes.LimeGreen) },
        { TileType.Water, (new SolidColorBrush(Color.FromRgb(20, 50, 80)), Brushes.DodgerBlue) },
        { TileType.TrapHidden, (new SolidColorBrush(Color.FromRgb(30, 30, 40)), Brushes.Gray) },
        { TileType.TrapVisible, (new SolidColorBrush(Color.FromRgb(80, 30, 80)), Brushes.Magenta) },
        { TileType.Altar, (new SolidColorBrush(Color.FromRgb(50, 40, 60)), Brushes.Gold) },
        { TileType.Fountain, (new SolidColorBrush(Color.FromRgb(30, 40, 60)), Brushes.Aqua) },
        { TileType.Chest, (new SolidColorBrush(Color.FromRgb(60, 50, 20)), Brushes.Goldenrod) },
        // デバッグ専用タイル（目立つ色）
        { TileType.DebugEnemySpawn, (new SolidColorBrush(Color.FromRgb(80, 20, 20)), Brushes.OrangeRed) },
        { TileType.DebugAIToggle, (new SolidColorBrush(Color.FromRgb(20, 60, 80)), Brushes.DeepSkyBlue) },
        { TileType.DebugDayAdvance, (new SolidColorBrush(Color.FromRgb(60, 60, 20)), Brushes.Yellow) },
        { TileType.DebugNpc, (new SolidColorBrush(Color.FromRgb(20, 60, 40)), Brushes.SpringGreen) },
        // 町内NPC用タイル
        { TileType.NpcGuildReceptionist, (new SolidColorBrush(Color.FromRgb(30, 30, 40)), Brushes.Gold) },
        { TileType.NpcPriest, (new SolidColorBrush(Color.FromRgb(30, 30, 40)), Brushes.White) },
        { TileType.NpcShopkeeper, (new SolidColorBrush(Color.FromRgb(30, 30, 40)), Brushes.Cyan) },
        { TileType.NpcBlacksmith, (new SolidColorBrush(Color.FromRgb(30, 30, 40)), Brushes.OrangeRed) },
        { TileType.NpcInnkeeper, (new SolidColorBrush(Color.FromRgb(30, 30, 40)), Brushes.LightGreen) },

        // 建物入口/出口
        { TileType.BuildingEntrance, (new SolidColorBrush(Color.FromRgb(80, 60, 30)), Brushes.Wheat) },
        { TileType.BuildingExit, (new SolidColorBrush(Color.FromRgb(30, 60, 100)), Brushes.CornflowerBlue) },
    };

    private static readonly Brush ExploredBackground = new SolidColorBrush(Color.FromRgb(15, 15, 20));
    private static readonly Brush ExploredForeground = new SolidColorBrush(Color.FromRgb(50, 50, 60));
    private static readonly Brush UnexploredBackground = new SolidColorBrush(Color.FromRgb(10, 10, 15));

    public GameRenderer(Canvas canvas)
    {
        _canvas = canvas;
    }

    public void Clear()
    {
        // プールインデックスをリセット（オブジェクトは再利用する）
        _rectPoolIndex = 0;
        _textPoolIndex = 0;
        _tileRects.Clear();
        _tileTexts.Clear();
    }

    /// <summary>
    /// プール済みRectangleを取得（不足時は新規作成してCanvasに追加）
    /// </summary>
    private Rectangle RentRect()
    {
        if (_rectPoolIndex < _rectPool.Count)
        {
            var rect = _rectPool[_rectPoolIndex];
            rect.Visibility = Visibility.Visible;
            rect.Stroke = null;
            rect.StrokeThickness = 0;
            _rectPoolIndex++;
            return rect;
        }

        var newRect = new Rectangle();
        _canvas.Children.Add(newRect);
        _rectPool.Add(newRect);
        _rectPoolIndex++;
        return newRect;
    }

    /// <summary>
    /// プール済みTextBlockを取得（不足時は新規作成してCanvasに追加）
    /// </summary>
    private TextBlock RentText()
    {
        if (_textPoolIndex < _textPool.Count)
        {
            var text = _textPool[_textPoolIndex];
            text.Visibility = Visibility.Visible;
            _textPoolIndex++;
            return text;
        }

        var newText = new TextBlock
        {
            FontFamily = new FontFamily(FontFamily),
            TextAlignment = TextAlignment.Center
        };
        _canvas.Children.Add(newText);
        _textPool.Add(newText);
        _textPoolIndex++;
        return newText;
    }

    /// <summary>
    /// 未使用のプールオブジェクトを非表示にする
    /// </summary>
    private void HideUnusedPoolItems()
    {
        for (int i = _rectPoolIndex; i < _rectPool.Count; i++)
            _rectPool[i].Visibility = Visibility.Collapsed;
        for (int i = _textPoolIndex; i < _textPool.Count; i++)
            _textPool[i].Visibility = Visibility.Collapsed;
    }

    public void Render(DungeonMap map, Player player, IEnumerable<Enemy> enemies, IEnumerable<(Item Item, Position Position)> groundItems)
    {
        // カメラオフセット計算（プレイヤー中心）
        double canvasWidth = _canvas.ActualWidth;
        double canvasHeight = _canvas.ActualHeight;

        int offsetX = (int)(canvasWidth / 2) - (player.Position.X * TileSize);
        int offsetY = (int)(canvasHeight / 2) - (player.Position.Y * TileSize);

        // 描画範囲計算
        int startX = Math.Max(0, -offsetX / TileSize - 1);
        int startY = Math.Max(0, -offsetY / TileSize - 1);
        int endX = Math.Min(map.Width, startX + (int)(canvasWidth / TileSize) + 3);
        int endY = Math.Min(map.Height, startY + (int)(canvasHeight / TileSize) + 3);

        // 既存のタイルをクリア
        Clear();

        // タイル描画
        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                var pos = new Position(x, y);
                var tile = map.GetTile(pos);

                RenderTile(pos, tile, offsetX, offsetY);
            }
        }

        // 地面のアイテム描画
        foreach (var (item, itemPos) in groundItems)
        {
            var tile = map.GetTile(itemPos);
            if (tile.IsVisible)
            {
                RenderEntity(itemPos, '!', Brushes.Cyan, offsetX, offsetY);
            }
        }

        // 敵描画
        foreach (var enemy in enemies.Where(e => e.IsAlive))
        {
            var tile = map.GetTile(enemy.Position);
            if (tile.IsVisible)
            {
                var color = GetEnemyColor(enemy);
                RenderEntity(enemy.Position, enemy.Name[0], color, offsetX, offsetY);
            }
        }

        // プレイヤー描画
        RenderEntity(player.Position, '@', Brushes.Yellow, offsetX, offsetY, true);

        // 未使用プールオブジェクトを非表示
        HideUnusedPoolItems();
    }

    private void RenderTile(Position pos, Tile tile, int offsetX, int offsetY)
    {
        double x = pos.X * TileSize + offsetX;
        double y = pos.Y * TileSize + offsetY;

        Brush background;
        Brush foreground;
        char displayChar;

        if (tile.IsVisible)
        {
            if (TileColors.TryGetValue(tile.Type, out var colors))
            {
                background = colors.Background;
                foreground = colors.Foreground;
            }
            else
            {
                background = new SolidColorBrush(Color.FromRgb(30, 30, 40));
                foreground = Brushes.Gray;
            }
            displayChar = tile.DisplayChar;
        }
        else if (tile.IsExplored)
        {
            background = ExploredBackground;
            foreground = ExploredForeground;
            displayChar = tile.DisplayChar;
        }
        else
        {
            background = UnexploredBackground;
            foreground = Brushes.Transparent;
            displayChar = ' ';
        }

        // 背景矩形
        var rect = RentRect();
        rect.Width = TileSize;
        rect.Height = TileSize;
        rect.Fill = background;
        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);
        Panel.SetZIndex(rect, 0);
        _tileRects[pos] = rect;

        // タイル文字
        if (displayChar != ' ')
        {
            var text = RentText();
            text.Text = displayChar.ToString();
            text.Foreground = foreground;
            text.FontSize = TileSize - 4;
            text.FontWeight = FontWeights.Normal;
            Canvas.SetLeft(text, x + 2);
            Canvas.SetTop(text, y);
            Panel.SetZIndex(text, 1);
            _tileTexts[pos] = text;
        }
    }

    private void RenderEntity(Position pos, char symbol, Brush color, int offsetX, int offsetY, bool highlight = false)
    {
        double x = pos.X * TileSize + offsetX;
        double y = pos.Y * TileSize + offsetY;

        if (highlight)
        {
            // プレイヤーハイライト
            var highlightRect = RentRect();
            highlightRect.Width = TileSize;
            highlightRect.Height = TileSize;
            highlightRect.Fill = new SolidColorBrush(Color.FromArgb(80, 255, 255, 0));
            highlightRect.Stroke = Brushes.Yellow;
            highlightRect.StrokeThickness = 1;
            Canvas.SetLeft(highlightRect, x);
            Canvas.SetTop(highlightRect, y);
            Panel.SetZIndex(highlightRect, 2);
        }

        var text = RentText();
        text.Text = symbol.ToString();
        text.Foreground = color;
        text.FontSize = TileSize - 2;
        text.FontWeight = FontWeights.Bold;
        Canvas.SetLeft(text, x + 2);
        Canvas.SetTop(text, y - 1);
        Panel.SetZIndex(text, 3);
    }

    private static Brush GetEnemyColor(Enemy enemy)
    {
        // 敵の種類や強さに応じて色を変える
        return enemy.EnemyTypeId switch
        {
            "slime" => Brushes.LimeGreen,
            "goblin" => Brushes.Orange,
            "skeleton" => Brushes.LightGray,
            "orc" => Brushes.OrangeRed,
            "giant_spider" => Brushes.Purple,
            "dark_elf" => Brushes.DarkViolet,
            "troll" => Brushes.DarkGreen,
            "draugr" => Brushes.LightBlue,
            _ => Brushes.Red
        };
    }

    /// <summary>ミニマップ用のWriteableBitmapとImageコントロール</summary>
    private WriteableBitmap? _minimapBitmap;
    private Image? _minimapImage;
    private int _minimapLastMapWidth;
    private int _minimapLastMapHeight;

    /// <summary>
    /// ミニマップを描画（WriteableBitmapベース高速描画）
    /// </summary>
    public void RenderMinimap(Canvas minimapCanvas, DungeonMap map, Player player, IEnumerable<Enemy> enemies)
    {
        double canvasWidth = minimapCanvas.Width;
        double canvasHeight = minimapCanvas.Height;

        int bmpWidth = Math.Max(1, (int)canvasWidth);
        int bmpHeight = Math.Max(1, (int)canvasHeight);

        // Bitmap/Image再作成が必要かチェック
        if (_minimapBitmap == null || _minimapBitmap.PixelWidth != bmpWidth || _minimapBitmap.PixelHeight != bmpHeight
            || _minimapLastMapWidth != map.Width || _minimapLastMapHeight != map.Height)
        {
            _minimapBitmap = new WriteableBitmap(bmpWidth, bmpHeight, 96, 96, PixelFormats.Bgra32, null);
            if (_minimapImage == null)
            {
                _minimapImage = new Image();
                minimapCanvas.Children.Add(_minimapImage);
            }
            _minimapImage.Source = _minimapBitmap;
            _minimapImage.Width = bmpWidth;
            _minimapImage.Height = bmpHeight;
            Canvas.SetLeft(_minimapImage, 0);
            Canvas.SetTop(_minimapImage, 0);
            _minimapLastMapWidth = map.Width;
            _minimapLastMapHeight = map.Height;

            // 不要な旧Childrenを除去（Image以外）
            for (int i = minimapCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (minimapCanvas.Children[i] != _minimapImage)
                    minimapCanvas.Children.RemoveAt(i);
            }
        }

        // マップ全体がミニマップに収まるスケールを計算
        double scaleX = canvasWidth / map.Width;
        double scaleY = canvasHeight / map.Height;
        double scale = Math.Min(scaleX, scaleY);

        double mapOffsetX = (canvasWidth - map.Width * scale) / 2;
        double mapOffsetY = (canvasHeight - map.Height * scale) / 2;

        // ピクセル配列を一括操作
        int stride = bmpWidth * 4;
        byte[] pixels = new byte[stride * bmpHeight];
        // 背景を暗色で塗りつぶし (10,10,15)
        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = 15;     // B
            pixels[i + 1] = 10; // G
            pixels[i + 2] = 10; // R
            pixels[i + 3] = 255; // A
        }

        // 探索済みタイルを描画
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                var pos = new Position(x, y);
                var tile = map.GetTile(pos);
                if (!tile.IsExplored) continue;

                byte r, g, b;
                if (tile.Type == TileType.StairsDown) { r = 50; g = 205; b = 50; }
                else if (tile.Type == TileType.StairsUp) { r = 100; g = 149; b = 237; }
                else if (tile.Type == TileType.Wall || tile.BlocksMovement) { r = 60; g = 60; b = 80; }
                else if (tile.IsVisible) { r = 80; g = 80; b = 100; }
                else { r = 40; g = 40; b = 55; }

                int pixelSize = Math.Max(1, (int)scale);
                int px = (int)(mapOffsetX + x * scale);
                int py = (int)(mapOffsetY + y * scale);

                for (int dy = 0; dy < pixelSize && py + dy < bmpHeight; dy++)
                {
                    for (int dx = 0; dx < pixelSize && px + dx < bmpWidth; dx++)
                    {
                        int idx = ((py + dy) * bmpWidth + (px + dx)) * 4;
                        if (idx >= 0 && idx + 3 < pixels.Length)
                        {
                            pixels[idx] = b;
                            pixels[idx + 1] = g;
                            pixels[idx + 2] = r;
                            pixels[idx + 3] = 255;
                        }
                    }
                }
            }
        }

        // 敵を描画（視界内のみ）
        foreach (var enemy in enemies.Where(e => e.IsAlive))
        {
            var tile = map.GetTile(enemy.Position);
            if (tile.IsVisible)
            {
                int dotSize = Math.Max(2, (int)(scale * 1.5));
                int px = (int)(mapOffsetX + enemy.Position.X * scale);
                int py = (int)(mapOffsetY + enemy.Position.Y * scale);
                for (int dy = 0; dy < dotSize && py + dy < bmpHeight; dy++)
                {
                    for (int dx = 0; dx < dotSize && px + dx < bmpWidth; dx++)
                    {
                        int idx = ((py + dy) * bmpWidth + (px + dx)) * 4;
                        if (idx >= 0 && idx + 3 < pixels.Length)
                        {
                            pixels[idx] = 0;     // B
                            pixels[idx + 1] = 0; // G
                            pixels[idx + 2] = 255; // R
                            pixels[idx + 3] = 255;
                        }
                    }
                }
            }
        }

        // プレイヤーを描画
        {
            int dotSize = Math.Max(3, (int)(scale * 2));
            int px = (int)(mapOffsetX + player.Position.X * scale - 0.5);
            int py = (int)(mapOffsetY + player.Position.Y * scale - 0.5);
            for (int dy = 0; dy < dotSize && py + dy < bmpHeight; dy++)
            {
                for (int dx = 0; dx < dotSize && px + dx < bmpWidth; dx++)
                {
                    int idx = ((py + dy) * bmpWidth + (px + dx)) * 4;
                    if (idx >= 0 && idx + 3 < pixels.Length)
                    {
                        pixels[idx] = 0;     // B
                        pixels[idx + 1] = 255; // G
                        pixels[idx + 2] = 255; // R (Yellow = R+G)
                        pixels[idx + 3] = 255;
                    }
                }
            }
        }

        // 一括書き込み
        _minimapBitmap.WritePixels(new Int32Rect(0, 0, bmpWidth, bmpHeight), pixels, stride, 0);
    }

    private static readonly Brush MinimapWallBrush = new SolidColorBrush(Color.FromRgb(60, 60, 80));
    private static readonly Brush MinimapFloorVisibleBrush = new SolidColorBrush(Color.FromRgb(80, 80, 100));
    private static readonly Brush MinimapFloorExploredBrush = new SolidColorBrush(Color.FromRgb(40, 40, 55));
}
