using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using RT.Util.Drawing;
using RT.Util.Forms;
using System.IO;

namespace PieceDeal
{
    public partial class Mainform : ManagedForm
    {
        private class DraggableObject { }
        private class Piece : DraggableObject, IEqualityComparer<Piece>, IEquatable<Piece>
        {
            public int Number;
            public int Col;
            public bool Locked;

            public bool Equals(Piece x, Piece y)
            {
                return x.Col == y.Col && x.Number == y.Number;
            }

            public int GetHashCode(Piece obj)
            {
                return (Col.ToString() + Number.ToString()).GetHashCode();
            }

            public bool Equals(Piece other)
            {
                return other.Col == Col && other.Number == Number;
            }
        }

        private class Joker : DraggableObject
        {
            public int IndexX;
            public int IndexY;
            public bool Locked;
        }

        private class Slot
        {
            public SlotType Type;
            public int IndexX;
            public int IndexY;
        }

        private Piece[] stock;
        private Piece[][] board;
        private Joker[] jokersUnplaced;
        private Joker[] jokersPlaced;
        private int jokerTarget;
        private int jokerTargetStep;
        private int score;

        private int leftMargin;
        private int topMargin;
        private Size gameSize;
        private int pieceSize;
        private Point stockPos;
        private Point boardPos;
        private Point jokersPos;
        private Point scorePos;
        private Rectangle dealButton;

        private DraggableObject dragging;
        private enum SlotType { Stock, Board, Joker };
        private Slot draggingFrom;
        private int draggingX;
        private int draggingY;
        private Rectangle? dragHighlight;
        private bool pressingDeal = false;
        private bool mouseOnDeal = false;

        private Image background = null;
        private FMOD.Sound sndPickUp;
        private FMOD.Sound sndPutDown;

        public Mainform()
            : base(Program.Settings)
        {
            var s = Path.GetTempFileName();
            s = Path.GetDirectoryName(s) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(s) + ".ogg";
            File.WriteAllBytes(s, Resources.sound_up);
            Program.FModSystem.createSound(s, FMOD.MODE.HARDWARE, ref sndPickUp);
            File.WriteAllBytes(s, Resources.sound_down);
            Program.FModSystem.createSound(s, FMOD.MODE.HARDWARE, ref sndPutDown);
            try { File.Delete(s); }
            catch { }

            InitializeComponent();
            recalculateSizes();
            startGame();
        }

        private void startGame()
        {
            stock = new Piece[4];
            board = new[] { new Piece[4], new Piece[4], new Piece[4], new Piece[4] };
            jokersUnplaced = new Joker[2];
            jokersPlaced = new Joker[0];
            score = 0;
            jokerTarget = 250;
            jokerTargetStep = 250;
            pnlMain.Refresh();
        }

        private void recalculateSizes()
        {
            int pieceSizeX = ClientSize.Width / 7;
            int pieceSizeY = ClientSize.Height / 8;

            if (pieceSizeX > pieceSizeY)
            {
                pieceSize = pieceSizeY;
                gameSize = new Size(pieceSize * 7, ClientSize.Height);
                leftMargin = (ClientSize.Width - gameSize.Width) / 2;
                topMargin = 0;
            }
            else
            {
                pieceSize = pieceSizeX;
                gameSize = new Size(ClientSize.Width, pieceSize * 8);
                topMargin = (ClientSize.Height - gameSize.Height) / 2;
                leftMargin = 0;
            }
            stockPos = new Point(leftMargin + (gameSize.Width - 5 * pieceSize) / 3, topMargin + (gameSize.Height - 6 * pieceSize) / 2 + pieceSize);
            boardPos = new Point(leftMargin + (gameSize.Width - 5 * pieceSize) * 2 / 3 + pieceSize, topMargin + (gameSize.Height - 6 * pieceSize) / 2 + pieceSize);
            jokersPos = new Point(leftMargin + (gameSize.Width - 5 * pieceSize) * 2 / 3 + pieceSize, topMargin + (gameSize.Height - 6 * pieceSize) / 4);
            scorePos = new Point(leftMargin + (gameSize.Width - 5 * pieceSize) / 3, topMargin + (gameSize.Height - 6 * pieceSize) * 3 / 4 + 5 * pieceSize);
            dealButton = new Rectangle(leftMargin + (gameSize.Width - 5 * pieceSize) / 3, topMargin + (gameSize.Height - 6 * pieceSize) / 4, pieceSize, pieceSize);
        }

        private void resize(object sender, EventArgs e)
        {
            recalculateSizes();
            pnlMain.Refresh();
        }

        private static int[][] sites;

        private void deal()
        {
            if (sites == null)
            {
                sites = new int[20][];
                // horizontal rows
                for (int i = 0; i < 4; i++)
                    sites[i] = new int[] { 4 * i, 4 * i + 1, 4 * i + 2, 4 * i + 3 };
                // vertical columns
                for (int i = 0; i < 4; i++)
                    sites[4 + i] = new int[] { i, 4 + i, 8 + i, 12 + i };
                // 2×2 squares
                for (int j = 0; j < 3; j++)
                    for (int i = 0; i < 3; i++)
                        sites[8 + (j * 3) + i] = new int[] { 4 * j + i, 4 * j + i + 1, 4 * j + i + 4, 4 * j + i + 5 };
                // diagonal 1
                sites[17] = new int[] { 0, 5, 10, 15 };
                // diagonal 2
                sites[18] = new int[] { 3, 6, 9, 12 };
                // four corners
                sites[19] = new int[] { 0, 3, 12, 15 };
            }

            // determine points gained
            var sitesToClear = new List<int[]>();
            foreach (var site in sites)
            {
                if (site.Any(s => board[s / 4][s % 4] == null && !jokersPlaced.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4)))
                    continue;
                if (site.All(s => board[s / 4][s % 4] != null && board[s / 4][s % 4].Locked && !jokersPlaced.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4)))
                    continue;

                int pointsGained = 0;
                int numJokers = site.Count(s => jokersPlaced.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4));

                if (numJokers == 0)
                {
                    var pieces = site.Select(s => board[s / 4][s % 4]);
                    var numbers = pieces.Select(d => d.Number);
                    var colors = pieces.Select(d => d.Col);

                    if (numbers.Distinct().Count() == 1 && colors.Distinct().Count() == 1) // all the same
                    {
                        pointsGained = 400;
                        sitesToClear.Add(site);
                    }
                    else if (numbers.Distinct().Count() == 4 && colors.Distinct().Count() == 1) // same color, all numbers
                    {
                        pointsGained = 200;
                        sitesToClear.Add(site);
                    }
                    else if (numbers.Distinct().Count() == 1 && colors.Distinct().Count() == 4) // same number, all colors
                    {
                        pointsGained = 200;
                        sitesToClear.Add(site);
                    }
                    else if (numbers.Distinct().Count() == 4 && colors.Distinct().Count() == 4) // all numbers, all colors
                    {
                        pointsGained = 100;
                        sitesToClear.Add(site);
                    }
                    else if (pieces.TwoPairs()) // two pairs
                        pointsGained = 60;
                    else if (colors.Distinct().Count() == 1) // same color
                        pointsGained = 40;
                    else if (numbers.Distinct().Count() == 1) // same number
                        pointsGained = 40;
                    else if (colors.TwoPairs() && numbers.TwoPairs()) // pair color, pair number
                        pointsGained = 20;
                    else if (colors.Distinct().Count() == 4) // each color
                        pointsGained = 10;
                    else if (numbers.Distinct().Count() == 4) // each number
                        pointsGained = 10;
                    else if (colors.TwoPairs()) // pair color
                        pointsGained = 5;
                    else if (numbers.TwoPairs()) // pair number
                        pointsGained = 5;
                }
                else if (numJokers == 1)
                {
                    var pieces = site.Where(s => !jokersPlaced.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4)).Select(s => board[s / 4][s % 4]);
                    var numbers = pieces.Select(d => d.Number);
                    var colors = pieces.Select(d => d.Col);

                    if (numbers.Distinct().Count() == 1 && colors.Distinct().Count() == 1) // all the same
                    {
                        pointsGained = 400;
                        sitesToClear.Add(site);
                    }
                    else if (numbers.Distinct().Count() == 3 && colors.Distinct().Count() == 1) // same color, all numbers
                    {
                        pointsGained = 200;
                        sitesToClear.Add(site);
                    }
                    else if (numbers.Distinct().Count() == 1 && colors.Distinct().Count() == 3) // same number, all colors
                    {
                        pointsGained = 200;
                        sitesToClear.Add(site);
                    }
                    else if (numbers.Distinct().Count() == 3 && colors.Distinct().Count() == 3) // all numbers, all colors
                    {
                        pointsGained = 100;
                        sitesToClear.Add(site);
                    }
                    else if (pieces.OnePair()) // two pairs
                        pointsGained = 60;
                    else if (colors.Distinct().Count() == 1) // same color
                        pointsGained = 40;
                    else if (numbers.Distinct().Count() == 1) // same number
                        pointsGained = 40;
                    else if (colors.OnePair() && numbers.OnePair()) // pair color, pair number
                        pointsGained = 20;
                    else if (colors.Distinct().Count() == 3) // each color
                        pointsGained = 10;
                    else if (numbers.Distinct().Count() == 3) // each number
                        pointsGained = 10;

                    pointsGained = pointsGained * 3 / 4;
                }
                else
                {
                    if (numJokers == 2)
                    {
                        var pieces = site.Where(s => !jokersPlaced.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4)).Select(s => board[s / 4][s % 4]).ToArray();
                        var numbers = new[] { pieces[0].Number, pieces[1].Number };
                        var colors = new[] { pieces[0].Col, pieces[1].Col };

                        if (numbers[0] == numbers[1] && colors[0] == colors[1]) // all the same
                            pointsGained = 200;
                        else if (colors[0] == colors[1]) // same color, all numbers
                            pointsGained = 100;
                        else if (numbers[0] == numbers[1]) // same number, all colors
                            pointsGained = 100;
                        else // all numbers, all colors
                            pointsGained = 50;
                    }
                    else if (numJokers == 3)
                        pointsGained = 100;
                    sitesToClear.Add(site);
                }

                score += pointsGained;

                if (score >= jokerTarget)
                {
                    if (jokersUnplaced[0] == null)
                        jokersUnplaced[0] = new Joker();
                    else if (jokersUnplaced[1] == null)
                        jokersUnplaced[1] = new Joker();
                    else
                        score += 1000;
                    if (jokerTargetStep < 1500)
                        jokerTargetStep += 250;
                    jokerTarget += jokerTargetStep;
                }
            }
            int spacesCleared = sitesToClear.SelectMany(s => s).Distinct().Count();
            if (spacesCleared > 4)
            {
                int bonus = (spacesCleared - 4) * 50;
                score += bonus;
            }
            foreach (var s in sitesToClear)
                foreach (var index in s)
                    board[index / 4][index % 4] = null;
            jokersPlaced = jokersPlaced.Where(j => !sitesToClear.SelectMany(s => s).Any(s => j.IndexX == s % 4 && j.IndexY == s / 4)).ToArray();

            // lock pieces on the board
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    if (board[y][x] != null)
                        board[y][x].Locked = true;
            foreach (var j in jokersPlaced)
                j.Locked = true;

            // create new pieces if the stock is empty
            if (stock.All(d => d == null))
                for (int i = 0; i < Math.Min(stock.Length, board.SelectMany(s => s).Count(d => d == null)); i++)
                    stock[i] = new Piece { Col = Ut.Rnd.Next(0, 4), Number = Ut.Rnd.Next(0, 4), Locked = false };

            pnlMain.Refresh();
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            if (e.X >= stockPos.X && e.X < stockPos.X + pieceSize && e.Y >= stockPos.Y && e.Y < stockPos.Y + 4 * pieceSize)
            {
                var index = (e.Y - stockPos.Y) / pieceSize;
                if (stock[index] != null)
                {
                    dragging = stock[index];
                    stock[index] = null;
                    draggingX = e.X;
                    draggingY = e.Y;
                    draggingFrom = new Slot { IndexX = index, Type = SlotType.Stock };
                }
            }
            else if (e.X >= boardPos.X && e.X < boardPos.X + 4 * pieceSize && e.Y >= boardPos.Y && e.Y < boardPos.Y + 4 * pieceSize)
            {
                int dfx = (e.X - boardPos.X) / pieceSize;
                int dfy = (e.Y - boardPos.Y) / pieceSize;
                var index = dfy * 4 + dfx;
                draggingX = e.X;
                draggingY = e.Y;
                if (jokersPlaced.Any(j => j.IndexX == dfx && j.IndexY == dfy && !j.Locked))
                {
                    draggingFrom = new Slot { Type = SlotType.Board, IndexX = dfx, IndexY = dfy };
                    dragging = jokersPlaced.First(j => j.IndexX == dfx && j.IndexY == dfy);
                    jokersPlaced = jokersPlaced.Where(j => j.IndexX != dfx || j.IndexY != dfy).ToArray();
                }
                else if (board[dfy][dfx] != null && !board[dfy][dfx].Locked)
                {
                    draggingFrom = new Slot { Type = SlotType.Board, IndexX = dfx, IndexY = dfy };
                    dragging = board[dfy][dfx];
                    board[dfy][dfx] = null;
                }
            }
            else if (jokersUnplaced[0] != null && e.X >= jokersPos.X && e.X < jokersPos.X + pieceSize && e.Y >= jokersPos.Y && e.Y < jokersPos.Y + pieceSize)
            {
                draggingFrom = new Slot { Type = SlotType.Joker, IndexX = 1 };
                dragging = jokersUnplaced[0];
                draggingX = e.X;
                draggingY = e.Y;
                jokersUnplaced[0] = null;
            }
            else if (jokersUnplaced[1] != null && e.X >= jokersPos.X + pieceSize && e.X < jokersPos.X + 2 * pieceSize && e.Y >= jokersPos.Y && e.Y < jokersPos.Y + pieceSize)
            {
                draggingFrom = new Slot { Type = SlotType.Joker, IndexX = 2 };
                dragging = jokersUnplaced[1];
                draggingX = e.X;
                draggingY = e.Y;
                jokersUnplaced[1] = null;
            }
            else
                draggingFrom = null;

            if (draggingFrom != null)
            {
                FMOD.Channel ch = null;
                Program.FModSystem.playSound(FMOD.CHANNELINDEX.FREE, sndPickUp, false, ref ch);
                pnlMain.Refresh();
            }

            if (e.X >= dealButton.Left && e.X <= dealButton.Right && e.Y >= dealButton.Top && e.Y <= dealButton.Bottom)
            {
                pressingDeal = true;
                mouseOnDeal = true;
                pnlMain.Invalidate();
            }
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (draggingFrom != null)
            {
                draggingX = e.X;
                draggingY = e.Y;
                pnlMain.Invalidate();
            }

            if (pressingDeal)
            {
                var newMouseOnDeal = (e.X >= dealButton.Left && e.X <= dealButton.Right && e.Y >= dealButton.Top && e.Y <= dealButton.Bottom);
                if (mouseOnDeal != newMouseOnDeal)
                {
                    mouseOnDeal = newMouseOnDeal;
                    pnlMain.Invalidate();
                }
            }
        }

        private Slot dragTarget(int x, int y)
        {
            if (draggingFrom == null)
                return null;

            if (!(dragging is Joker) && x >= stockPos.X && x < stockPos.X + pieceSize && y >= stockPos.Y && y < stockPos.Y + 4 * pieceSize)
            {
                var targetIndex = (y - stockPos.Y) / pieceSize;
                if (stock[targetIndex] == null)
                    return new Slot { Type = SlotType.Stock, IndexX = targetIndex };
            }
            else if (!(dragging is Joker) && x >= boardPos.X && x < boardPos.X + 4 * pieceSize && y >= boardPos.Y && y < boardPos.Y + 4 * pieceSize)
            {
                int dfx = (x - boardPos.X) / pieceSize;
                int dfy = (y - boardPos.Y) / pieceSize;
                if (board[dfy][dfx] == null && !jokersPlaced.Any(j => j.IndexX == dfx && j.IndexY == dfy && j.Locked))
                    return new Slot { Type = SlotType.Board, IndexX = dfx, IndexY = dfy };
            }
            else if (dragging is Joker && x >= boardPos.X && x < boardPos.X + 4 * pieceSize && y >= boardPos.Y && y < boardPos.Y + 4 * pieceSize)
            {
                int dfx = (x - boardPos.X) / pieceSize;
                int dfy = (y - boardPos.Y) / pieceSize;
                if (!jokersPlaced.Any(j => j.IndexX == dfx && j.IndexY == dfy))
                    return new Slot { Type = SlotType.Board, IndexX = dfx, IndexY = dfy };
            }
            else if (dragging is Joker && x >= jokersPos.X && x < jokersPos.X + 2 * pieceSize && y >= jokersPos.Y && y < jokersPos.Y + pieceSize)
            {
                bool ontoFirst = x < jokersPos.X + pieceSize;
                if (ontoFirst && jokersUnplaced[0] == null)
                    return new Slot { IndexX = 0, Type = SlotType.Joker };
                else if (!ontoFirst && jokersUnplaced[1] == null)
                    return new Slot { IndexX = 1, Type = SlotType.Joker };
            }
            return null;
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            if (pressingDeal)
            {
                if (mouseOnDeal)
                    deal();
                pressingDeal = false;
                mouseOnDeal = false;
                pnlMain.Invalidate();
                return;
            }

            if (dragging == null)
                return;

            Slot dt = dragTarget(e.X, e.Y);
            if (dt == null)
            {
                if (draggingFrom.Type == SlotType.Board && dragging is Joker)
                    jokersPlaced = jokersPlaced.Add((Joker) dragging).ToArray();
                else if (draggingFrom.Type == SlotType.Board)
                    board[draggingFrom.IndexY][draggingFrom.IndexX] = (Piece) dragging;
                else if (draggingFrom.Type == SlotType.Stock)
                    stock[draggingFrom.IndexX] = (Piece) dragging;
                else if (draggingFrom.Type == SlotType.Joker)
                {
                    if (draggingFrom.IndexX == 0)
                        jokersUnplaced[0] = (Joker) dragging;
                    else if (draggingFrom.IndexX == 1)
                        jokersUnplaced[1] = (Joker) dragging;
                }
            }
            else if (dt.Type == SlotType.Stock && dragging is Piece)
                stock[dt.IndexX] = (Piece) dragging;
            else if (dt.Type == SlotType.Board && dragging is Joker)
                jokersPlaced = jokersPlaced.Add(new Joker() { Locked = false, IndexX = dt.IndexX, IndexY = dt.IndexY }).ToArray();
            else if (dt.Type == SlotType.Board && dragging is Piece)
                board[dt.IndexY][dt.IndexX] = (Piece) dragging;
            else if (dt.Type == SlotType.Joker && dt.IndexX == 0 && dragging is Joker)
                jokersUnplaced[0] = (Joker) dragging;
            else if (dt.Type == SlotType.Joker && dt.IndexX == 1 && dragging is Joker)
                jokersUnplaced[1] = (Joker) dragging;

            FMOD.Channel ch = null;
            Program.FModSystem.playSound(FMOD.CHANNELINDEX.FREE, sndPutDown, false, ref ch);

            draggingFrom = null;
            dragging = null;
            pnlMain.Refresh();
        }

        private float fontSizeFromHeight(Graphics g, string fontName, FontStyle style, float targetHeight)
        {
            float low = 1;
            float high = 100;
            float height = g.MeasureString("Wg", new Font(fontName, high, style)).Height;
            while (height < targetHeight)
            {
                low = high;
                high *= 2;
                height = g.MeasureString("Wg", new Font(fontName, high, style)).Height;
            }
            while (high - low > 1)
            {
                height = g.MeasureString("Wg", new Font(fontName, (low + high) / 2, style)).Height;
                if (height > targetHeight)
                    high = (high + low) / 2;
                else
                    low = (high + low) / 2;
            }
            return low;
        }

        private void paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            if (draggingFrom != null)
            {
                var dt = dragTarget(draggingX, draggingY);
                if (dt != null)
                {
                    RectangleF highlight;
                    if (dt.Type == SlotType.Board)
                        highlight = new RectangleF(boardPos.X + pieceSize * dt.IndexX, boardPos.Y + pieceSize * dt.IndexY, pieceSize, pieceSize);
                    else if (dt.Type == SlotType.Joker)
                        highlight = new RectangleF(jokersPos.X + pieceSize * dt.IndexX, jokersPos.Y, pieceSize, pieceSize);
                    else
                        highlight = new RectangleF(stockPos.X, stockPos.Y + pieceSize * dt.IndexX, pieceSize, pieceSize);
                    e.Graphics.FillPath(new SolidBrush(Color.FromArgb(32, 255, 255, 255)), GraphicsUtil.RoundedRectangle(highlight, (float) pieceSize / 5));
                    // e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 255, 255, 255)), highlight);
                }

                if (dragging is Joker)
                {
                    GraphicsUtil.DrawImageAlpha(e.Graphics, Resources.joker, new Rectangle(draggingX - pieceSize / 2, draggingY - pieceSize / 2, pieceSize, pieceSize), 0.75f);
                }
                else if (dragging is Piece)
                {
                    Bitmap bmp = new Bitmap(pieceSize * 13 / 10, pieceSize * 13 / 10, PixelFormat.Format32bppArgb);
                    Graphics g = Graphics.FromImage(bmp);
                    paintPieceAndOrJoker(g, 0, 0, pieceSize * 13 / 10, (Piece) dragging, null);
                    GraphicsUtil.DrawImageAlpha(e.Graphics, bmp, new Rectangle(draggingX - pieceSize * 13 / 20, draggingY - pieceSize * 13 / 20, pieceSize * 13 / 10, pieceSize * 13 / 10), 0.75f);
                }
            }
            if (pressingDeal && mouseOnDeal)
                e.Graphics.DrawImage(Resources.dealpressed, dealButton);
            else
                e.Graphics.DrawImage(Resources.deal, dealButton);
        }

        private void paintBuffer(object sender, PaintEventArgs e)
        {
            if (background == null)
            {
                Bitmap watermark = new Bitmap(2048, 1536, PixelFormat.Format24bppRgb);
                Graphics w = Graphics.FromImage(watermark);
                w.Clear(Color.Black);
                w.TranslateTransform(1024, 768);
                w.RotateTransform(-30);
                w.TranslateTransform(-1024, -768);
                w.DrawString("PIECE", new Font("Berlin Sans FB", fontSizeFromHeight(w, "Berlin Sans FB", FontStyle.Bold, 640), FontStyle.Bold), new SolidBrush(Color.White), new PointF(1024, 768 + 96), new StringFormat { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Center });
                w.DrawString("DEAL", new Font("Berlin Sans FB Demi", fontSizeFromHeight(w, "Berlin Sans FB Demi", FontStyle.Bold, 640), FontStyle.Bold), new SolidBrush(Color.White), new PointF(1024, 768 - 96), new StringFormat { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center });

                unsafe
                {
                    var d = watermark.LockBits(new Rectangle(0, 0, 2048, 1536), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    background = new Bitmap(2048, 1536, PixelFormat.Format24bppRgb);
                    Graphics g = Graphics.FromImage(background);
                    g.Clear(Color.FromArgb(134, 88, 43));
                    g.SmoothingMode = SmoothingMode.HighQuality;

                    for (int y = 1530; y >= 0; y -= 10)
                    {
                        byte* b = (byte*) d.Scan0 + y * d.Stride;
                        for (int x = 4; x < 2048; x += 15)
                        {
                            bool darker = (b[3 * x] > 0);
                            g.TranslateTransform(x, y);
                            g.RotateTransform((float) (Ut.Rnd.NextDouble() * 40 - 20));
                            var i = Ut.Rnd.Next(0, 24);
                            g.FillEllipse(new SolidBrush(Color.FromArgb((darker ? 124 : 134) + i, (darker ? 81 : 88) + i / 2, (darker ? 39 : 43) + i / 3)), -20, -30, 20, 30);
                            g.ResetTransform();
                        }
                    }
                    watermark.UnlockBits(d);
                }
            }

            if (ClientSize.Width > background.Width * ClientSize.Height / background.Height)
            {
                int newWidth = ClientSize.Width;
                int newHeight = newWidth * background.Height / background.Width;
                e.Graphics.DrawImage(background, new Rectangle(0, (ClientSize.Height - newHeight) / 2, newWidth, newHeight));
            }
            else
            {
                int newHeight = ClientSize.Height;
                int newWidth = newHeight * background.Width / background.Height;
                e.Graphics.DrawImage(background, new Rectangle((ClientSize.Width - newWidth) / 2, 0, newWidth, newHeight));
            }
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Pen black = new Pen(Color.Black);
            draw3dInlet(e.Graphics, stockPos.X, stockPos.Y, pieceSize, 4 * pieceSize, gameSize.Width > 800 ? 2 : 1);
            draw3dInlet(e.Graphics, boardPos.X, boardPos.Y, 4 * pieceSize, 4 * pieceSize, gameSize.Width > 800 ? 2 : 1);
            draw3dInlet(e.Graphics, jokersPos.X, jokersPos.Y, 4 * pieceSize, pieceSize, gameSize.Width > 800 ? 2 : 1);
            draw3dInlet(e.Graphics, scorePos.X, scorePos.Y, gameSize.Width / 3 + 10 * pieceSize / 3, pieceSize, gameSize.Width > 800 ? 2 : 1);
            float h = fontSizeFromHeight(e.Graphics, "Calibri", FontStyle.Regular, pieceSize);
            e.Graphics.DrawString("Score:", new Font("Calibri", h / 2, FontStyle.Regular), new SolidBrush(Color.Lime), scorePos.X, scorePos.Y + pieceSize / 2, new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
            e.Graphics.DrawString(score.ToString(), new Font("Calibri", h * 3 / 4, FontStyle.Bold), new SolidBrush(Color.White), scorePos.X + gameSize.Width / 3 + 10 * pieceSize / 3, scorePos.Y + pieceSize / 2, new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });

            h = fontSizeFromHeight(e.Graphics, "Calibri", FontStyle.Regular, pieceSize / 3);
            e.Graphics.DrawString("Next joker at:", new Font("Calibri", h, FontStyle.Regular), new SolidBrush(Color.Lime), new Point(jokersPos.X + 4 * pieceSize, jokersPos.Y), new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near });
            h = fontSizeFromHeight(e.Graphics, "Calibri", FontStyle.Regular, pieceSize * 2 / 3);
            e.Graphics.DrawString(jokerTarget.ToString(), new Font("Calibri", h, FontStyle.Regular), new SolidBrush(Color.White), new Point(jokersPos.X + 4 * pieceSize, jokersPos.Y + pieceSize), new StringFormat { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Far });

            for (int i = 0; i < stock.Length; i++)
                if (stock[i] != null)
                    paintPieceAndOrJoker(e.Graphics, stockPos.X, stockPos.Y + pieceSize * i, pieceSize, stock[i], null);
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    paintPieceAndOrJoker(e.Graphics, boardPos.X + pieceSize * x, boardPos.Y + pieceSize * y, pieceSize, board[y][x], jokersPlaced.FirstOrDefault(j => j.IndexX == x && j.IndexY == y));

            if (jokersUnplaced[0] != null)
                e.Graphics.DrawImage(Resources.joker, new Rectangle(jokersPos.X, jokersPos.Y, pieceSize, pieceSize));
            if (jokersUnplaced[1] != null)
                e.Graphics.DrawImage(Resources.joker, new Rectangle(jokersPos.X + pieceSize, jokersPos.Y, pieceSize, pieceSize));
        }

        private void draw3dInlet(Graphics g, int x, int y, int w, int h, int th)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(64, 0, 0, 0)), new Rectangle(x, y, w, h));
            g.FillPolygon(new LinearGradientBrush(new Rectangle(x - th, y - th, 2 * th, h + 2 * th), Color.FromArgb(96, 0, 0, 0), Color.FromArgb(64, 32, 32, 32), 45), new[] { new Point(x - th, y - th), new Point(x + th, y + th), new Point(x + th, y + h - th), new Point(x - th, y + h + th) });
            g.FillPolygon(new LinearGradientBrush(new Rectangle(x - th, y - th, w + 2 * th, 2 * th), Color.FromArgb(96, 0, 0, 0), Color.FromArgb(64, 32, 32, 32), 45), new[] { new Point(x - th, y - th), new Point(x + w + th, y - th), new Point(x + w - th, y + th), new Point(x + th, y + th) });
            g.FillPolygon(new LinearGradientBrush(new Rectangle(x + w - th, y - th, 2 * th, h + 2 * th), Color.FromArgb(64, 223, 223, 223), Color.FromArgb(96, 255, 255, 255), 45), new[] { new Point(x + w + th, y - th), new Point(x + w + th, y + h + th), new Point(x + w - th, y + h - th), new Point(x + w - th, y + th) });
            g.FillPolygon(new LinearGradientBrush(new Rectangle(x - th, y + h - th, w + 2 * th, 2 * th), Color.FromArgb(64, 223, 223, 223), Color.FromArgb(96, 255, 255, 255), 45), new[] { new Point(x + th, y + h - th), new Point(x + w - th, y + h - th), new Point(x + w + th, y + h + th), new Point(x - th, y + h + th) });
        }

        private Dictionary<int, Dictionary<int, Image>> cache = new Dictionary<int, Dictionary<int, Image>>();
        private DateTime lastCacheFlush = DateTime.Now;
        private Image[] Res = new Image[]
        {
            Resources.redcube, Resources.redsphere, Resources.redcone, Resources.redcross,
            Resources.greencube, Resources.greensphere, Resources.greencone, Resources.greencross,
            Resources.bluecube, Resources.bluesphere, Resources.bluecone, Resources.bluecross,
            Resources.yellowcube, Resources.yellowsphere, Resources.yellowcone, Resources.yellowcross
        };

        private void paintPieceAndOrJoker(Graphics g, int x, int y, int size, Piece piece, Joker joker)
        {
            if ((DateTime.Now - lastCacheFlush).TotalMinutes > 1)
            {
                cache = new Dictionary<int, Dictionary<int, Image>>();
                lastCacheFlush = DateTime.Now;
            }
            if (!cache.ContainsKey(size))
                cache[size] = new Dictionary<int, Image>();

            Image jokerImage = null;
            Image lockedImage = null;

            if (piece != null)
            {
                Image pieceImage = null;
                int ind = piece.Col * 4 + piece.Number;
                if (!cache[size].ContainsKey(ind))
                {
                    pieceImage = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                    Graphics tmpg = Graphics.FromImage(pieceImage);
                    tmpg.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    tmpg.DrawImage(Res[piece.Col * 4 + piece.Number], size / 10, size / 10, size * 4 / 5, size * 4 / 5);
                    cache[size][ind] = pieceImage;
                }
                else
                    pieceImage = cache[size][ind];

                g.DrawImage(pieceImage, x, y);
            }

            if (joker != null)
            {
                if (!cache[size].ContainsKey(-2))
                {
                    jokerImage = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                    Graphics tmpg = Graphics.FromImage(jokerImage);
                    tmpg.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    tmpg.DrawImage(Resources.joker, 0, 0, size, size);
                    cache[size][-2] = jokerImage;
                }
                else
                    jokerImage = cache[size][-2];
            }

            if ((piece != null && piece.Locked) || (joker != null && joker.Locked))
            {
                if (!cache[size].ContainsKey(-1))
                {
                    lockedImage = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                    Graphics tmpg = Graphics.FromImage(lockedImage);
                    tmpg.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    tmpg.DrawImage(Resources.lockimage, 0, 0, size, size);
                    cache[size][-1] = lockedImage;
                }
                else
                    lockedImage = cache[size][-1];
            }

            if (piece != null && piece.Locked && joker != null && !joker.Locked && lockedImage != null)
            {
                g.DrawImage(lockedImage, x, y);
                lockedImage = null;
            }

            if (jokerImage != null)
                g.DrawImage(jokerImage, x, y);

            if (lockedImage != null)
                g.DrawImage(lockedImage, x, y);
        }
    }
}
