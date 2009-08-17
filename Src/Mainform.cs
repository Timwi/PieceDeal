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
using System.Drawing.Text;
using RT.Util.Dialogs;

namespace PieceDeal
{
    public partial class Mainform : ManagedForm
    {
        private static SmoothingMode GlobalSmoothingMode = SmoothingMode.HighQuality;
        private static TextRenderingHint GlobalTextRenderingHint = TextRenderingHint.AntiAlias;
        private static InterpolationMode GlobalInterpolationMode = InterpolationMode.HighQualityBicubic;

        private int leftMargin;
        private int topMargin;
        private Size gameSize;
        private int pieceSize;
        private Point stockPos;
        private Point boardPos;
        private Point jokersPos;
        private Rectangle scoreBox;
        private Rectangle dealButton;

        private DraggableObject dragging;
        private Slot draggingFrom;
        private int draggingX;
        private int draggingY;
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
        }

        private void startNewGame()
        {
            Program.Settings.StartNewGame();
            pnlMain.Refresh();
        }

        private void recalculateSizes()
        {
            int pieceSizeX = pnlMain.ClientSize.Width / 7;
            int pieceSizeY = pnlMain.ClientSize.Height / 8;

            if (pieceSizeX > pieceSizeY)
            {
                pieceSize = pieceSizeY;
                gameSize = new Size(pieceSize * 7, pnlMain.ClientSize.Height);
                leftMargin = (pnlMain.ClientSize.Width - gameSize.Width) / 2;
                topMargin = 0;
            }
            else
            {
                pieceSize = pieceSizeX;
                gameSize = new Size(pnlMain.ClientSize.Width, pieceSize * 8);
                topMargin = (pnlMain.ClientSize.Height - gameSize.Height) / 2;
                leftMargin = 0;
            }
            stockPos = new Point(leftMargin + (gameSize.Width - 5 * pieceSize) / 3, topMargin + (gameSize.Height - 6 * pieceSize) / 2 + pieceSize);
            boardPos = new Point(leftMargin + (gameSize.Width - 5 * pieceSize) * 2 / 3 + pieceSize, topMargin + (gameSize.Height - 6 * pieceSize) / 2 + pieceSize);
            jokersPos = new Point(leftMargin + (gameSize.Width - 5 * pieceSize) * 2 / 3 + pieceSize, topMargin + (gameSize.Height - 6 * pieceSize) / 4);
            scoreBox = new Rectangle(leftMargin + (gameSize.Width - 5 * pieceSize) / 3, topMargin + (gameSize.Height - 6 * pieceSize) * 3 / 4 + 5 * pieceSize, gameSize.Width / 3 + 10 * pieceSize / 3, pieceSize);
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
                if (site.Any(s => Program.Settings.Board[s / 4][s % 4] == null && !Program.Settings.JokersOnBoard.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4)))
                    continue;
                if (site.All(s => Program.Settings.Board[s / 4][s % 4] != null && Program.Settings.Board[s / 4][s % 4].Locked && !Program.Settings.JokersOnBoard.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4 && !j.Locked)))
                    continue;

                int pointsGained = 0;
                int numJokers = site.Count(s => Program.Settings.JokersOnBoard.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4));

                if (numJokers == 0)
                {
                    var pieces = site.Select(s => Program.Settings.Board[s / 4][s % 4]);
                    var shapes = pieces.Select(d => d.Shape);
                    var colors = pieces.Select(d => d.Colour);

                    if (shapes.Distinct().Count() == 1 && colors.Distinct().Count() == 1) // all the same
                    {
                        pointsGained = 400;
                        sitesToClear.Add(site);
                    }
                    else if (shapes.Distinct().Count() == 4 && colors.Distinct().Count() == 1) // same color, all shapes
                    {
                        pointsGained = 200;
                        sitesToClear.Add(site);
                    }
                    else if (shapes.Distinct().Count() == 1 && colors.Distinct().Count() == 4) // same shape, all colors
                    {
                        pointsGained = 200;
                        sitesToClear.Add(site);
                    }
                    else if (shapes.Distinct().Count() == 4 && colors.Distinct().Count() == 4) // all shapes, all colors
                    {
                        pointsGained = 100;
                        sitesToClear.Add(site);
                    }
                    else if (pieces.TwoPairs()) // two pairs
                        pointsGained = 60;
                    else if (colors.Distinct().Count() == 1) // same color
                        pointsGained = 40;
                    else if (shapes.Distinct().Count() == 1) // same shape
                        pointsGained = 40;
                    else if (colors.TwoPairs() && shapes.TwoPairs()) // pair color, pair shape
                        pointsGained = 20;
                    else if (colors.Distinct().Count() == 4) // each color
                        pointsGained = 10;
                    else if (shapes.Distinct().Count() == 4) // each shape
                        pointsGained = 10;
                    else if (colors.TwoPairs()) // pair color
                        pointsGained = 5;
                    else if (shapes.TwoPairs()) // pair shape
                        pointsGained = 5;
                }
                else if (numJokers == 1)
                {
                    var pieces = site.Where(s => !Program.Settings.JokersOnBoard.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4)).Select(s => Program.Settings.Board[s / 4][s % 4]);
                    var shapes = pieces.Select(d => d.Shape);
                    var colors = pieces.Select(d => d.Colour);

                    if (shapes.Distinct().Count() == 1 && colors.Distinct().Count() == 1) // all the same
                    {
                        pointsGained = 400;
                        sitesToClear.Add(site);
                    }
                    else if (shapes.Distinct().Count() == 3 && colors.Distinct().Count() == 1) // same color, all shapes
                    {
                        pointsGained = 200;
                        sitesToClear.Add(site);
                    }
                    else if (shapes.Distinct().Count() == 1 && colors.Distinct().Count() == 3) // same shape, all colors
                    {
                        pointsGained = 200;
                        sitesToClear.Add(site);
                    }
                    else if (shapes.Distinct().Count() == 3 && colors.Distinct().Count() == 3) // all shapes, all colors
                    {
                        pointsGained = 100;
                        sitesToClear.Add(site);
                    }
                    else if (pieces.OnePair()) // two pairs
                        pointsGained = 60;
                    else if (colors.Distinct().Count() == 1) // same color
                        pointsGained = 40;
                    else if (shapes.Distinct().Count() == 1) // same shape
                        pointsGained = 40;
                    else if (colors.OnePair() && shapes.OnePair()) // pair color, pair shape
                        pointsGained = 20;
                    else if (colors.Distinct().Count() == 3) // each color
                        pointsGained = 10;
                    else if (shapes.Distinct().Count() == 3) // each shape
                        pointsGained = 10;

                    pointsGained = pointsGained * 3 / 4;
                }
                else
                {
                    if (numJokers == 2)
                    {
                        var pieces = site.Where(s => !Program.Settings.JokersOnBoard.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4)).Select(s => Program.Settings.Board[s / 4][s % 4]).ToArray();
                        var shapes = new[] { pieces[0].Shape, pieces[1].Shape };
                        var colors = new[] { pieces[0].Colour, pieces[1].Colour };

                        if (shapes[0] == shapes[1] && colors[0] == colors[1]) // all the same
                            pointsGained = 200;
                        else if (colors[0] == colors[1]) // same color, all shapes
                            pointsGained = 100;
                        else if (shapes[0] == shapes[1]) // same shape, all colors
                            pointsGained = 100;
                        else // all shapes, all colors
                            pointsGained = 50;
                    }
                    else if (numJokers == 3)
                        pointsGained = 100;
                    sitesToClear.Add(site);
                }

                Program.Settings.Score += pointsGained;
            }
            int spacesCleared = sitesToClear.SelectMany(s => s).Distinct().Count();
            if (spacesCleared > 4)
            {
                int bonus = (spacesCleared - 4) * 50;
                Program.Settings.Score += bonus;
            }
            foreach (var s in sitesToClear)
                foreach (var index in s)
                    Program.Settings.Board[index / 4][index % 4] = null;
            Program.Settings.JokersOnBoard = Program.Settings.JokersOnBoard.Where(j => !sitesToClear.SelectMany(s => s).Any(s => j.IndexX == s % 4 && j.IndexY == s / 4)).ToArray();

            while (Program.Settings.Score >= Program.Settings.NextJokerAt)
            {
                if (Program.Settings.UnusedJokers[0] == null)
                    Program.Settings.UnusedJokers[0] = new Joker();
                else if (Program.Settings.UnusedJokers[1] == null)
                    Program.Settings.UnusedJokers[1] = new Joker();
                else
                    Program.Settings.Score += 1000;
                if (Program.Settings.NextJokerAtStep < 1500)
                    Program.Settings.NextJokerAtStep += 250;
                Program.Settings.NextJokerAtPrev = Program.Settings.NextJokerAt;
                Program.Settings.NextJokerAt += Program.Settings.NextJokerAtStep;
            }

            // lock pieces on the board
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    if (Program.Settings.Board[y][x] != null)
                        Program.Settings.Board[y][x].Locked = true;
            foreach (var j in Program.Settings.JokersOnBoard)
                j.Locked = true;

            // create new pieces if the stock is empty
            if (Program.Settings.Stock.All(d => d == null))
                for (int i = 0; i < Math.Min(Program.Settings.Stock.Length, Program.Settings.FreeSpaces); i++)
                    Program.Settings.Stock[i] = new Piece { Colour = Ut.Rnd.Next(0, 4), Shape = Ut.Rnd.Next(0, 4), Locked = false };

            pnlMain.Refresh();
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            if (e.X >= stockPos.X && e.X < stockPos.X + pieceSize && e.Y >= stockPos.Y && e.Y < stockPos.Y + 4 * pieceSize)
            {
                var index = (e.Y - stockPos.Y) / pieceSize;
                if (Program.Settings.Stock[index] != null)
                {
                    dragging = Program.Settings.Stock[index];
                    Program.Settings.Stock[index] = null;
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
                if (Program.Settings.JokersOnBoard.Any(j => j.IndexX == dfx && j.IndexY == dfy && !j.Locked))
                {
                    draggingFrom = new Slot { Type = SlotType.Board, IndexX = dfx, IndexY = dfy };
                    dragging = Program.Settings.JokersOnBoard.First(j => j.IndexX == dfx && j.IndexY == dfy);
                    Program.Settings.JokersOnBoard = Program.Settings.JokersOnBoard.Where(j => j.IndexX != dfx || j.IndexY != dfy).ToArray();
                }
                else if (Program.Settings.Board[dfy][dfx] != null && !Program.Settings.Board[dfy][dfx].Locked)
                {
                    draggingFrom = new Slot { Type = SlotType.Board, IndexX = dfx, IndexY = dfy };
                    dragging = Program.Settings.Board[dfy][dfx];
                    Program.Settings.Board[dfy][dfx] = null;
                }
            }
            else if (Program.Settings.UnusedJokers[0] != null && e.X >= jokersPos.X && e.X < jokersPos.X + pieceSize && e.Y >= jokersPos.Y && e.Y < jokersPos.Y + pieceSize)
            {
                draggingFrom = new Slot { Type = SlotType.Joker, IndexX = 1 };
                dragging = Program.Settings.UnusedJokers[0];
                draggingX = e.X;
                draggingY = e.Y;
                Program.Settings.UnusedJokers[0] = null;
            }
            else if (Program.Settings.UnusedJokers[1] != null && e.X >= jokersPos.X + pieceSize && e.X < jokersPos.X + 2 * pieceSize && e.Y >= jokersPos.Y && e.Y < jokersPos.Y + pieceSize)
            {
                draggingFrom = new Slot { Type = SlotType.Joker, IndexX = 2 };
                dragging = Program.Settings.UnusedJokers[1];
                draggingX = e.X;
                draggingY = e.Y;
                Program.Settings.UnusedJokers[1] = null;
            }
            else
                draggingFrom = null;

            if (draggingFrom != null)
            {
                FMOD.Channel ch = null;
                Program.FModSystem.playSound(FMOD.CHANNELINDEX.FREE, sndPickUp, false, ref ch);
                pnlMain.Invalidate();
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
                if (Program.Settings.Stock[targetIndex] == null)
                    return new Slot { Type = SlotType.Stock, IndexX = targetIndex };
            }
            else if (!(dragging is Joker) && x >= boardPos.X && x < boardPos.X + 4 * pieceSize && y >= boardPos.Y && y < boardPos.Y + 4 * pieceSize)
            {
                int dfx = (x - boardPos.X) / pieceSize;
                int dfy = (y - boardPos.Y) / pieceSize;
                if (Program.Settings.Board[dfy][dfx] == null && !Program.Settings.JokersOnBoard.Any(j => j.IndexX == dfx && j.IndexY == dfy && j.Locked))
                    return new Slot { Type = SlotType.Board, IndexX = dfx, IndexY = dfy };
            }
            else if (dragging is Joker && x >= boardPos.X && x < boardPos.X + 4 * pieceSize && y >= boardPos.Y && y < boardPos.Y + 4 * pieceSize)
            {
                int dfx = (x - boardPos.X) / pieceSize;
                int dfy = (y - boardPos.Y) / pieceSize;
                if (!Program.Settings.JokersOnBoard.Any(j => j.IndexX == dfx && j.IndexY == dfy))
                    return new Slot { Type = SlotType.Board, IndexX = dfx, IndexY = dfy };
            }
            else if (dragging is Joker && x >= jokersPos.X && x < jokersPos.X + 2 * pieceSize && y >= jokersPos.Y && y < jokersPos.Y + pieceSize)
            {
                bool ontoFirst = x < jokersPos.X + pieceSize;
                if (ontoFirst && Program.Settings.UnusedJokers[0] == null)
                    return new Slot { IndexX = 0, Type = SlotType.Joker };
                else if (!ontoFirst && Program.Settings.UnusedJokers[1] == null)
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
                    Program.Settings.JokersOnBoard = Program.Settings.JokersOnBoard.Add((Joker) dragging).ToArray();
                else if (draggingFrom.Type == SlotType.Board)
                    Program.Settings.Board[draggingFrom.IndexY][draggingFrom.IndexX] = (Piece) dragging;
                else if (draggingFrom.Type == SlotType.Stock)
                    Program.Settings.Stock[draggingFrom.IndexX] = (Piece) dragging;
                else if (draggingFrom.Type == SlotType.Joker)
                {
                    if (draggingFrom.IndexX == 0)
                        Program.Settings.UnusedJokers[0] = (Joker) dragging;
                    else if (draggingFrom.IndexX == 1)
                        Program.Settings.UnusedJokers[1] = (Joker) dragging;
                }
            }
            else if (dt.Type == SlotType.Stock && dragging is Piece)
                Program.Settings.Stock[dt.IndexX] = (Piece) dragging;
            else if (dt.Type == SlotType.Board && dragging is Joker)
                Program.Settings.JokersOnBoard = Program.Settings.JokersOnBoard.Add(new Joker() { Locked = false, IndexX = dt.IndexX, IndexY = dt.IndexY }).ToArray();
            else if (dt.Type == SlotType.Board && dragging is Piece)
                Program.Settings.Board[dt.IndexY][dt.IndexX] = (Piece) dragging;
            else if (dt.Type == SlotType.Joker && dt.IndexX == 0 && dragging is Joker)
                Program.Settings.UnusedJokers[0] = (Joker) dragging;
            else if (dt.Type == SlotType.Joker && dt.IndexX == 1 && dragging is Joker)
                Program.Settings.UnusedJokers[1] = (Joker) dragging;

            FMOD.Channel ch = null;
            Program.FModSystem.playSound(FMOD.CHANNELINDEX.FREE, sndPutDown, false, ref ch);

            draggingFrom = null;
            dragging = null;
            pnlMain.Invalidate();
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

        private float fontSizeFromWidth(Graphics g, string fontName, FontStyle style, float targetWidth, string targetString)
        {
            float low = 1;
            float high = 100;
            float width = g.MeasureString(targetString, new Font(fontName, high, style)).Width;
            while (width < targetWidth)
            {
                low = high;
                high *= 2;
                width = g.MeasureString(targetString, new Font(fontName, high, style)).Width;
            }
            while (high - low > 1)
            {
                width = g.MeasureString(targetString, new Font(fontName, (low + high) / 2, style)).Width;
                if (width > targetWidth)
                    high = (high + low) / 2;
                else
                    low = (high + low) / 2;
            }
            return low;
        }

        private void paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = GlobalInterpolationMode;
            e.Graphics.SmoothingMode = GlobalSmoothingMode;
            e.Graphics.TextRenderingHint = GlobalTextRenderingHint;

            drawScore(e.Graphics, Program.Settings.Score);

            // Draw "next joker at" score and progress bar
            var h = fontSizeFromHeight(e.Graphics, "Calibri", FontStyle.Regular, pieceSize * 7 / 12);
            e.Graphics.DrawString(Program.Settings.NextJokerAt.ToString(), new Font("Calibri", h, FontStyle.Regular), new SolidBrush(Color.White), new Point(jokersPos.X + pieceSize * 13 / 4, jokersPos.Y + pieceSize * 23 / 40), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(128, 0, 64, 0)), jokersPos.X + pieceSize * 5 / 2, jokersPos.Y + pieceSize * 9 / 10, pieceSize * 3 / 2, pieceSize / 10);
            if (Program.Settings.Score > Program.Settings.NextJokerAtPrev)
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(128, 0, 255, 64)), jokersPos.X + pieceSize * 5 / 2, jokersPos.Y + pieceSize * 9 / 10, (pieceSize * 3 / 2) * (Program.Settings.Score - Program.Settings.NextJokerAtPrev) / (Program.Settings.NextJokerAt - Program.Settings.NextJokerAtPrev), pieceSize / 10);

            // Pieces in the stock
            for (int i = 0; i < Program.Settings.Stock.Length; i++)
                if (Program.Settings.Stock[i] != null)
                    paintPieceAndOrJoker(e.Graphics, stockPos.X, stockPos.Y + pieceSize * i, pieceSize, Program.Settings.Stock[i], null);

            // Unused jokers
            if (Program.Settings.UnusedJokers[0] != null)
                e.Graphics.DrawImage(Resources.joker, new Rectangle(jokersPos.X, jokersPos.Y, pieceSize, pieceSize));
            if (Program.Settings.UnusedJokers[1] != null)
                e.Graphics.DrawImage(Resources.joker, new Rectangle(jokersPos.X + pieceSize, jokersPos.Y, pieceSize, pieceSize));

            // Draw pieces and jokers on the board only if they are NOT locked (paintBuffer() already draws the ones that are locked)
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    var pieceOnBoard = Program.Settings.Board[y][x];
                    if (pieceOnBoard != null && pieceOnBoard.Locked)
                        pieceOnBoard = null;
                    var joker = Program.Settings.JokersOnBoard.FirstOrDefault(j => j.IndexX == x && j.IndexY == y && !j.Locked);
                    paintPieceAndOrJoker(e.Graphics, boardPos.X + pieceSize * x, boardPos.Y + pieceSize * y, pieceSize, pieceOnBoard, joker);
                }
            }

            // Draw piece or joker that is being dragged
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

            // Draw the "Deal" button
            if (pressingDeal && mouseOnDeal)
                e.Graphics.DrawImage(Resources.dealpressed, dealButton);
            else
                e.Graphics.DrawImage(Resources.deal, dealButton);

            // Draw the "Game over" message
            if (Program.Settings.IsGameOver && dragging == null)
            {
                GraphicsPath inside = new GraphicsPath();
                inside.AddString("GAME OVER", new FontFamily("Impact"), (int) FontStyle.Bold, Math.Min(fontSizeFromHeight(e.Graphics, "Impact", FontStyle.Bold, pnlMain.ClientSize.Height / 2), fontSizeFromWidth(e.Graphics, "Impact", FontStyle.Bold, pnlMain.ClientSize.Width * 7 / 8, "GAME OVER")),
                    new Point(pnlMain.ClientSize.Width / 2, pnlMain.ClientSize.Height / 2), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                GraphicsPath outside = (GraphicsPath) inside.Clone();
                outside.Widen(new Pen(Color.White, Math.Min(pnlMain.ClientSize.Width, pnlMain.ClientSize.Height) / 30) { LineJoin = LineJoin.Round });
                var bounds = outside.GetBounds();
                e.Graphics.FillPath(new LinearGradientBrush(bounds.Location, new PointF(bounds.Right, bounds.Bottom), Color.FromArgb(192, 128, 0, 0), Color.FromArgb(192, 32, 0, 0)), outside);
                e.Graphics.FillPath(new LinearGradientBrush(bounds.Location, new PointF(bounds.Right, bounds.Bottom), Color.FromArgb(255, 128, 0), Color.FromArgb(255, 0, 0)), inside);
            }
        }

        private void paintBuffer(object sender, PaintEventArgs e)
        {
            if (background == null)
                generateBackground();

            e.Graphics.SmoothingMode = GlobalSmoothingMode;
            e.Graphics.InterpolationMode = GlobalInterpolationMode;
            e.Graphics.TextRenderingHint = GlobalTextRenderingHint;

            if (pnlMain.ClientSize.Width > background.Width * pnlMain.ClientSize.Height / background.Height)
            {
                int newWidth = pnlMain.ClientSize.Width;
                int newHeight = newWidth * background.Height / background.Width;
                e.Graphics.DrawImage(background, new Rectangle(0, (pnlMain.ClientSize.Height - newHeight) / 2, newWidth, newHeight));
            }
            else
            {
                int newHeight = pnlMain.ClientSize.Height;
                int newWidth = newHeight * background.Width / background.Height;
                e.Graphics.DrawImage(background, new Rectangle((pnlMain.ClientSize.Width - newWidth) / 2, 0, newWidth, newHeight));
            }

            draw3dInlet(e.Graphics, stockPos.X, stockPos.Y, pieceSize, 4 * pieceSize, gameSize.Width > 800 ? 2 : 1);
            draw3dInlet(e.Graphics, boardPos.X, boardPos.Y, 4 * pieceSize, 4 * pieceSize, gameSize.Width > 800 ? 2 : 1);
            draw3dInlet(e.Graphics, jokersPos.X, jokersPos.Y, 2 * pieceSize, pieceSize, gameSize.Width > 800 ? 2 : 1);
            draw3dInlet(e.Graphics, jokersPos.X + pieceSize * 5 / 2, jokersPos.Y, pieceSize * 3 / 2, pieceSize, gameSize.Width > 800 ? 2 : 1);
            draw3dInlet(e.Graphics, scoreBox.X, scoreBox.Y, scoreBox.Width, scoreBox.Height, gameSize.Width > 800 ? 2 : 1);

            float h = fontSizeFromHeight(e.Graphics, "Calibri", FontStyle.Regular, pieceSize);
            e.Graphics.DrawString("Score:", new Font("Calibri", h / 2, FontStyle.Regular), new SolidBrush(Color.Lime), scoreBox.X, scoreBox.Y + pieceSize / 2, new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });

            h = fontSizeFromHeight(e.Graphics, "Calibri", FontStyle.Regular, pieceSize / 3);
            e.Graphics.DrawString("Next joker at:", new Font("Calibri", h, FontStyle.Regular), new SolidBrush(Color.Lime), new Point(jokersPos.X + pieceSize * 13 / 4, jokersPos.Y), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near });

            // Draw pieces and jokers on the board only if they are locked (paint() will draw the ones that are NOT locked)
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    var pieceOnBoard = Program.Settings.Board[y][x];
                    if (pieceOnBoard != null && !pieceOnBoard.Locked)
                        pieceOnBoard = null;
                    var joker = Program.Settings.JokersOnBoard.FirstOrDefault(j => j.IndexX == x && j.IndexY == y && j.Locked);
                    paintPieceAndOrJoker(e.Graphics, boardPos.X + pieceSize * x, boardPos.Y + pieceSize * y, pieceSize, pieceOnBoard, joker);
                }
            }
        }

        private void generateBackground()
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
                g.SmoothingMode = GlobalSmoothingMode;

                for (int y = 1530; y >= 0; y -= 10)
                {
                    byte* b = (byte*) d.Scan0 + y * d.Stride;
                    for (int x = 4; x <= 2059; x += 15)
                    {
                        bool darker = (b[3 * x] > 0);
                        g.TranslateTransform(x, y);
                        g.RotateTransform((float) (Ut.Rnd.NextDouble() * 40 - 20));
                        var i = Ut.Rnd.Next(0, 24);
                        g.FillEllipse(new SolidBrush(Color.FromArgb((darker ? 120 : 134) + i, (darker ? 78 : 88) + i / 2, (darker ? 37 : 43) + i / 3)), -20, -30, 20, 30);
                        g.ResetTransform();
                    }
                }
                watermark.UnlockBits(d);
            }
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
            Resources.cube_red, Resources.circle_red, Resources.cone_red, Resources.cross_red,
            Resources.cube_green, Resources.circle_green, Resources.cone_green, Resources.cross_green,
            Resources.cube_blue, Resources.circle_blue, Resources.cone_blue, Resources.cross_blue,
            Resources.cube_yellow, Resources.circle_yellow, Resources.cone_yellow, Resources.cross_yellow
        };

        private void paintPieceAndOrJoker(Graphics g, int x, int y, int size, Piece piece, Joker joker)
        {
            if (piece == null && joker == null)
                return;

            if ((DateTime.Now - lastCacheFlush).TotalMinutes > 1)
            {
                if (cache.Sum(kvp => kvp.Value.Count) > 256)
                    cache.Clear();
                lastCacheFlush = DateTime.Now;
            }

            if (!cache.ContainsKey(size))
                cache[size] = new Dictionary<int, Image>(64);

            Image jokerImage = null;
            Image lockedImage = null;

            if (piece != null)
            {
                Image pieceImage;
                int ind = piece.Colour * 4 + piece.Shape;
                if (!cache[size].ContainsKey(ind))
                {
                    pieceImage = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                    Graphics tmpg = Graphics.FromImage(pieceImage);
                    tmpg.InterpolationMode = GlobalInterpolationMode;
                    tmpg.DrawImage(Res[piece.Colour * 4 + piece.Shape], size / 10, size / 10, size * 4 / 5, size * 4 / 5);
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
                    tmpg.InterpolationMode = GlobalInterpolationMode;
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
                    tmpg.InterpolationMode = GlobalInterpolationMode;
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

        private Dictionary<int, Dictionary<int, Image>> digitCache = new Dictionary<int, Dictionary<int, Image>>();
        private DateTime lastDigitCacheFlush = DateTime.Now;
        private Image[] DigitRes = new Image[] { Resources.d0, Resources.d1, Resources.d2, Resources.d3, Resources.d4, Resources.d5, Resources.d6, Resources.d7, Resources.d8, Resources.d9 };

        private void drawScore(Graphics g, int sc)
        {
            if ((DateTime.Now - lastDigitCacheFlush).TotalMinutes > 1)
            {
                if (digitCache.Sum(kvp => kvp.Value.Count) > 256)
                    digitCache.Clear();
                lastDigitCacheFlush = DateTime.Now;
            }
            string str = sc.ToString();
            for (int i = 0; i < str.Length; i++)
            {
                if (!digitCache.ContainsKey(str[i] - '0'))
                    digitCache[str[i] - '0'] = new Dictionary<int, Image>();
                Image img;
                if (!digitCache[str[i] - '0'].ContainsKey(pieceSize))
                {
                    img = new Bitmap(pieceSize, pieceSize, PixelFormat.Format32bppArgb);
                    Graphics tmpg = Graphics.FromImage(img);
                    tmpg.InterpolationMode = GlobalInterpolationMode;
                    tmpg.DrawImage(DigitRes[str[i] - '0'], 0, 0, pieceSize, pieceSize);
                    digitCache[str[i] - '0'][pieceSize] = img;
                }
                else
                    img = digitCache[str[i] - '0'][pieceSize];
                g.DrawImage(img, scoreBox.Right - pieceSize * 11 / 12 - pieceSize * (str.Length - i - 1) * 7 / 12, scoreBox.Y);
            }
        }

        private void startNewGame(object sender, EventArgs e)
        {
            if (Program.Settings.IsGameOver || DlgMessage.ShowQuestion("Are you sure you wish to concede this game?", "&Yes", "&No") == 0)
            {
                Program.Settings.StartNewGame();
                pnlMain.Refresh();
            }
        }

        private void exit(object sender, EventArgs e)
        {
            Close();
        }
    }
}
