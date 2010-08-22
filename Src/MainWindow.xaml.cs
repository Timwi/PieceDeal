using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PieceDeal
{
    public partial class MainWindow : ManagedWindow
    {
        private BitmapSource dealButtonImage = new BitmapImage(new Uri("pack://application:,,,/Resources/deal.png", UriKind.Absolute));
        private BitmapSource dealButtonImagePressed = new BitmapImage(new Uri("pack://application:,,,/Resources/dealpressed.png", UriKind.Absolute));
        private BitmapSource lockImage = new BitmapImage(new Uri("pack://application:,,,/Resources/lock.png", UriKind.Absolute));
        private BitmapSource jokerImage = new BitmapImage(new Uri("pack://application:,,,/Resources/joker.png", UriKind.Absolute));

        private BitmapSource[] digitImages = new BitmapSource[] {
            new BitmapImage(new Uri("pack://application:,,,/Resources/d0.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/d1.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/d2.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/d3.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/d4.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/d5.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/d6.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/d7.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/d8.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/d9.png", UriKind.Absolute))
        };

        private BitmapSource[] pieceImages = new BitmapSource[] {
            new BitmapImage(new Uri("pack://application:,,,/Resources/circle-blue.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/circle-green.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/circle-yellow.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/circle-red.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cone-blue.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cone-green.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cone-yellow.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cone-red.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cross-blue.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cross-green.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cross-yellow.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cross-red.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cube-blue.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cube-green.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cube-yellow.png", UriKind.Absolute)),
            new BitmapImage(new Uri("pack://application:,,,/Resources/cube-red.png", UriKind.Absolute)),
        };

        private Polygon[] jokers3d;
        private Polygon[] nextJoker3d;
        private Polygon[] stock3d;
        private Polygon[] board3d;
        private Polygon[] score3d;
        private Image[] scoreDigits;
        private Image[] nextJokerDigits;
        private OutlinedText gameOver;

        private LinearGradientBrush gameOverFill = new LinearGradientBrush(Color.FromRgb(255, 128, 64), Color.FromRgb(255, 64, 0), 90);
        private LinearGradientBrush gameOverStroke = new LinearGradientBrush(Color.FromRgb(32, 0, 0), Color.FromRgb(64, 0, 0), 90);

        private class site
        {
            public int[] Squares;
            public FrameworkElement[] Displays;
            public Point DisplayCenter;
        }

        private double pieceSize;
        private site[] sites;
        private bool pressingDeal;
        private bool animating;

        private List<UIElement> firstUpdate = new List<UIElement>();

        public MainWindow()
            : base(Program.Settings.MainWindowSettings)
        {
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

            sites = new site[20];
            // horizontal rows
            for (int i = 0; i < 4; i++)
                sites[i] = new site { Squares = new int[] { 4 * i, 4 * i + 1, 4 * i + 2, 4 * i + 3 } };
            // vertical columns
            for (int i = 0; i < 4; i++)
                sites[4 + i] = new site { Squares = new int[] { i, 4 + i, 8 + i, 12 + i } };
            // 2×2 squares
            for (int j = 0; j < 3; j++)
                for (int i = 0; i < 3; i++)
                    sites[8 + (j * 3) + i] = new site { Squares = new int[] { 4 * j + i, 4 * j + i + 1, 4 * j + i + 4, 4 * j + i + 5 } };
            // diagonal 1
            sites[17] = new site { Squares = new int[] { 0, 5, 10, 15 } };
            // diagonal 2
            sites[18] = new site { Squares = new int[] { 3, 6, 9, 12 } };
            // four corners
            sites[19] = new site { Squares = new int[] { 0, 3, 12, 15 } };

            pressingDeal = false;
            animating = false;

            InitializeComponent();
            OutlinedText ot = new OutlinedText { Font = new FontFamily("Impact"), FontSize = 128 };
            mainCanvas.Children.Add(ot);
            moveAndResize(ot, mainCanvas.ActualWidth + pieceSize, mainCanvas.ActualHeight + pieceSize, ot.MinWidth, ot.MinHeight);
            mainCanvas.Children.Remove(ot);
        }

        private void resize(object sender, SizeChangedEventArgs e)
        {
            // Resize/reposition the background image
            if (mainCanvas.ActualWidth > backgroundImage.Source.Width * mainCanvas.ActualHeight / backgroundImage.Source.Height)
            {
                double newActualWidth = mainCanvas.ActualWidth;
                double newActualHeight = newActualWidth * backgroundImage.Source.Height / backgroundImage.Source.Width;
                Canvas.SetLeft(backgroundImage, 0);
                Canvas.SetTop(backgroundImage, (mainCanvas.ActualHeight - newActualHeight) / 2);
                backgroundImage.Width = newActualWidth;
                backgroundImage.Height = newActualHeight;
            }
            else
            {
                double newActualHeight = mainCanvas.ActualHeight;
                double newActualWidth = newActualHeight * backgroundImage.Source.Width / backgroundImage.Source.Height;
                Canvas.SetLeft(backgroundImage, (mainCanvas.ActualWidth - newActualWidth) / 2);
                Canvas.SetTop(backgroundImage, 0);
                backgroundImage.Width = newActualWidth;
                backgroundImage.Height = newActualHeight;
            }

            // Determine the size of the various boxes
            double pieceSizeX = mainCanvas.ActualWidth / 7;
            double pieceSizeY = mainCanvas.ActualHeight / 8;
            double gameWidth, gameHeight, leftMargin, topMargin;

            if (pieceSizeX > pieceSizeY)
            {
                pieceSize = pieceSizeY;
                gameWidth = pieceSize * 7;
                gameHeight = mainCanvas.ActualHeight;
                leftMargin = (mainCanvas.ActualWidth - gameWidth) / 2;
                topMargin = 0;
            }
            else
            {
                pieceSize = pieceSizeX;
                gameWidth = mainCanvas.ActualWidth;
                gameHeight = pieceSize * 8;
                topMargin = (mainCanvas.ActualHeight - gameHeight) / 2;
                leftMargin = 0;
            }

            moveAndResize(dealButton, leftMargin + (gameWidth - 5 * pieceSize) / 3, topMargin + (gameHeight - 6 * pieceSize) / 4, pieceSize, pieceSize);
            moveAndResize(jokersBox, leftMargin + (gameWidth - 5 * pieceSize) * 2 / 3 + pieceSize, topMargin + (gameHeight - 6 * pieceSize) / 4, 2 * pieceSize, pieceSize);
            moveAndResize(nextJokerBox, leftMargin + (gameWidth - 5 * pieceSize) * 2 / 3 + 3.5 * pieceSize, topMargin + (gameHeight - 6 * pieceSize) / 4, 1.5 * pieceSize, pieceSize);
            moveAndResize(progressBar, leftMargin + (gameWidth - 5 * pieceSize) * 2 / 3 + 3.5 * pieceSize, topMargin + (gameHeight - 6 * pieceSize) / 4 + pieceSize * 7 / 8, pieceSize * 1.5, pieceSize / 8);
            moveAndResize(stockBox, leftMargin + (gameWidth - 5 * pieceSize) / 3, topMargin + (gameHeight - 6 * pieceSize) / 2 + pieceSize, pieceSize, 4 * pieceSize);
            moveAndResize(boardBox, leftMargin + (gameWidth - 5 * pieceSize) * 2 / 3 + pieceSize, topMargin + (gameHeight - 6 * pieceSize) / 2 + pieceSize, 4 * pieceSize, 4 * pieceSize);
            moveAndResize(scoreBox, leftMargin + (gameWidth - 5 * pieceSize) / 3, topMargin + (gameHeight - 6 * pieceSize) * 3 / 4 + 5 * pieceSize, gameWidth / 3 + 10 * pieceSize / 3, pieceSize);

            moveAndResize(nextJokerLabel, Canvas.GetLeft(nextJokerBox), Canvas.GetTop(nextJokerBox), nextJokerBox.Width, pieceSize);
            nextJokerLabel.FontSize = pieceSize / 5;

            updateImages();
        }

        private void moveAndResize(FrameworkElement elem, double x, double y, double width, double height)
        {
            elem.Width = width;
            elem.Height = height;
            Canvas.SetLeft(elem, x);
            Canvas.SetTop(elem, y);
        }

        private void dealMouseDown(object sender, MouseButtonEventArgs e)
        {
            dealButton.Source = dealButtonImagePressed;
            pressingDeal = true;
        }
        private void cnvMouseUp(object sender, MouseButtonEventArgs e)
        {
            pressingDeal = false;
            focusRect.Visibility = Visibility.Hidden;
        }
        private void dealMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (pressingDeal)
            {
                dealButton.Source = dealButtonImage;
                pressingDeal = false;
                deal();
            }
        }
        private void dealMouseEnter(object sender, MouseEventArgs e)
        {
            if (pressingDeal)
                dealButton.Source = dealButtonImagePressed;
        }
        private void dealMouseLeave(object sender, MouseEventArgs e)
        {
            if (pressingDeal)
                dealButton.Source = dealButtonImage;
        }

        private void deal()
        {
            if (animating)
                return;
            animating = true;
            double delay = 0;

            // determine points gained
            var sitesToClear = new List<site>();
            foreach (var site in sites)
            {
                if (site.Squares.Any(s => Program.Settings.Board[s / 4][s % 4] == null && !Program.Settings.JokersOnBoard.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4)))
                    continue;
                if (site.Squares.All(s => Program.Settings.Board[s / 4][s % 4] != null && Program.Settings.Board[s / 4][s % 4].Locked && !Program.Settings.JokersOnBoard.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4 && !j.Locked)))
                    continue;

                int pointsGained = 0;
                int numJokers = site.Squares.Count(s => Program.Settings.JokersOnBoard.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4));
                bool clearSite = false;

                if (numJokers == 0)
                {
                    var pieces = site.Squares.Select(s => Program.Settings.Board[s / 4][s % 4]);
                    var shapes = pieces.Select(d => d.Shape);
                    var colors = pieces.Select(d => d.Colour);

                    if (shapes.Distinct().Count() == 1 && colors.Distinct().Count() == 1) // all the same
                    {
                        pointsGained = 400;
                        clearSite = true;
                    }
                    else if (shapes.Distinct().Count() == 4 && colors.Distinct().Count() == 1) // same color, all shapes
                    {
                        pointsGained = 200;
                        clearSite = true;
                    }
                    else if (shapes.Distinct().Count() == 1 && colors.Distinct().Count() == 4) // same shape, all colors
                    {
                        pointsGained = 200;
                        clearSite = true;
                    }
                    else if (shapes.Distinct().Count() == 4 && colors.Distinct().Count() == 4) // all shapes, all colors
                    {
                        pointsGained = 100;
                        clearSite = true;
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
                    var pieces = site.Squares.Where(s => !Program.Settings.JokersOnBoard.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4)).Select(s => Program.Settings.Board[s / 4][s % 4]);
                    var shapes = pieces.Select(d => d.Shape);
                    var colors = pieces.Select(d => d.Colour);

                    if (shapes.Distinct().Count() == 1 && colors.Distinct().Count() == 1) // all the same
                    {
                        pointsGained = 400;
                        clearSite = true;
                    }
                    else if (shapes.Distinct().Count() == 3 && colors.Distinct().Count() == 1) // same color, all shapes
                    {
                        pointsGained = 200;
                        clearSite = true;
                    }
                    else if (shapes.Distinct().Count() == 1 && colors.Distinct().Count() == 3) // same shape, all colors
                    {
                        pointsGained = 200;
                        clearSite = true;
                    }
                    else if (shapes.Distinct().Count() == 3 && colors.Distinct().Count() == 3) // all shapes, all colors
                    {
                        pointsGained = 100;
                        clearSite = true;
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
                        var pieces = site.Squares.Where(s => !Program.Settings.JokersOnBoard.Any(j => j.IndexX == s % 4 && j.IndexY == s / 4)).Select(s => Program.Settings.Board[s / 4][s % 4]).ToArray();
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
                    clearSite = true;
                }

                if (pointsGained > 0)
                {
                    // We need to remember these values because 'site' is a loop variable.
                    var displays = site.Displays;
                    var center = site.DisplayCenter;

                    Animate.LaterMs(delay, () =>
                    {
                        // Show the site's display element (flashes up the relevant parts of the board)
                        foreach (var display in displays)
                        {
                            moveAfter(display, null);
                            display.Visibility = Visibility.Visible;
                        }
                        var displayAnim = new AnimateLinear(1, 0, TimeSpan.FromMilliseconds(1000), v =>
                        {
                            foreach (var display in displays)
                                display.Opacity = v;
                        });
                        displayAnim.Completed += () =>
                        {
                            foreach (var display in displays)
                                display.Visibility = Visibility.Hidden;
                        };

                        animateScore(pointsGained, center, 0.5);
                        Program.Settings.Score += pointsGained;
                        updateScore();
                    });
                    delay += 500;
                }

                if (clearSite)
                    sitesToClear.Add(site);
            }
            Animate.LaterMs(delay, () =>
            {
                int[] spacesCleared = sitesToClear.SelectMany(s => s.Squares).Distinct().ToArray();
                foreach (var index in spacesCleared.Where(i => Program.Settings.Board[i / 4][i % 4] != null))
                {
                    deleteImage(Program.Settings.Board[index / 4][index % 4].Image);
                    deleteImage(Program.Settings.Board[index / 4][index % 4].LockImage);
                    Program.Settings.Board[index / 4][index % 4] = null;
                }
                var jokersKept = new List<Joker>();
                foreach (var j in Program.Settings.JokersOnBoard)
                {
                    if (spacesCleared.Any(s => j.IndexX == s % 4 && j.IndexY == s / 4))
                    {
                        deleteImage(j.Image);
                        deleteImage(j.LockImage);
                    }
                    else
                        jokersKept.Add(j);
                }
                Program.Settings.JokersOnBoard = jokersKept.ToArray();
                if (spacesCleared.Length > 4)
                {
                    int bonus = (spacesCleared.Length - 4) * 50;
                    animateScore(bonus, new Point(Canvas.GetLeft(boardBox) + boardBox.Width / 2, Canvas.GetTop(boardBox) + boardBox.Height / 2), 1);
                    Program.Settings.Score += bonus;
                }

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
                    updateNextJokerScore();
                }

                // lock pieces on the board
                for (int y = 0; y < 4; y++)
                    for (int x = 0; x < 4; x++)
                        if (Program.Settings.Board[y][x] != null)
                        {
                            Program.Settings.Board[y][x].Locked = true;
                            if (Program.Settings.Board[y][x].Image != null)
                                Program.Settings.Board[y][x].Image.MouseDown -= imageMouseDown;
                        }
                foreach (var j in Program.Settings.JokersOnBoard)
                    j.Locked = true;

                // create new pieces if the stock is empty
                if (Program.Settings.Stock.All(d => d == null))
                    for (int i = 0; i < Math.Min(Program.Settings.Stock.Length, Program.Settings.FreeSpaces); i++)
                        Program.Settings.Stock[i] = new Piece { Colour = Rnd.Next(0, 4), Shape = Rnd.Next(0, 4), Locked = false };

                updateImages();
                animating = false;
            });
        }

        private void animateScore(int pointsGained, Point center, double sizeFactor)
        {
            OutlinedText scoreText = new OutlinedText
            {
                Text = pointsGained.ToString(),
                Font = new FontFamily("Impact"),
                FontSize = pieceSize * sizeFactor,
                Stroke = new Pen(Brushes.Black, pieceSize / 20 * sizeFactor)
            };
            moveAndResize(scoreText, center.X - scoreText.MinWidth / 2, center.Y - scoreText.MinHeight / 2, scoreText.MinWidth, scoreText.MinHeight);
            ScaleTransform stf = new ScaleTransform(1, 1, scoreText.MinWidth / 2, scoreText.MinHeight + pieceSize / 4);
            scoreText.RenderTransform = stf;
            // scoreText.Effect = new DropShadowEffect { Opacity = 0.7, ShadowDepth = pieceSize / 10 };
            mainCanvas.Children.Add(scoreText);
            var scoreAnim = new AnimateLinear(1, 2, TimeSpan.FromMilliseconds(1000), v => { scoreText.Opacity = (2 - v); stf.ScaleX = v; stf.ScaleY = v; });
            scoreAnim.Completed += () => mainCanvas.Children.Remove(scoreText);
        }

        private void deleteImage(Image image)
        {
            if (image == null)
                return;

            var tag = image.Tag as ImageTag;
            ScaleTransform stf;

            if (tag == null)
            {
                stf = new ScaleTransform(1, 1, mainCanvas.ActualWidth / 2 - Canvas.GetLeft(image) + pieceSize * Rnd.NextDouble(-2, 2), mainCanvas.ActualHeight / 2 - Canvas.GetTop(image) + pieceSize * Rnd.NextDouble(-2, 2));
                image.RenderTransform = stf;
            }
            else
            {
                tag.AboutToDisappear = true;
                stf = tag.ScaleTransform;
                stf.CenterX = mainCanvas.ActualWidth / 2 - Canvas.GetLeft(image) + pieceSize * Rnd.NextDouble(-2, 2);
                stf.CenterY = mainCanvas.ActualHeight / 2 - Canvas.GetTop(image) + pieceSize * Rnd.NextDouble(-2, 2);
            }
            moveAfter(image, null);
            var anim = new AnimateLinear(1, 2, TimeSpan.FromMilliseconds(750), v => { stf.ScaleX = v; stf.ScaleY = v; image.Opacity = 2 - v; });
            anim.Completed += () => mainCanvas.Children.Remove(image);
        }

        private void updateInlet(Rectangle box, ref Polygon[] polys)
        {
            if (polys == null)
                polys = new Polygon[4];

            double thickness = pieceSize / 30;

            // top
            updatePoly(ref polys[0],
                Canvas.GetLeft(box) - thickness / 2, Canvas.GetTop(box) - thickness / 2, box.Width + thickness, thickness,
                0, 0, box.Width + thickness, 0, box.Width, thickness, thickness, thickness,
                new LinearGradientBrush(Color.FromArgb(96, 0, 0, 0), Color.FromArgb(64, 32, 32, 32), 45));

            // left
            updatePoly(ref polys[1],
                Canvas.GetLeft(box) - thickness / 2, Canvas.GetTop(box) - thickness / 2, thickness, box.Height + thickness,
                0, 0, thickness, thickness, thickness, box.Height, 0, box.Height + thickness,
                new LinearGradientBrush(Color.FromArgb(96, 0, 0, 0), Color.FromArgb(64, 32, 32, 32), 45));

            // right
            updatePoly(ref polys[2],
                Canvas.GetLeft(box) + box.Width - thickness / 2, Canvas.GetTop(box) - thickness / 2, thickness, box.Height + thickness,
                thickness, 0, thickness, box.Height + thickness, 0, box.Height, 0, thickness,
                new LinearGradientBrush(Color.FromArgb(64, 223, 223, 223), Color.FromArgb(96, 255, 255, 255), 45));

            // bottom
            updatePoly(ref polys[3],
                Canvas.GetLeft(box) - thickness / 2, Canvas.GetTop(box) + box.Height - thickness / 2, box.Width + thickness, thickness,
                thickness, 0, box.Width, 0, box.Width + thickness, thickness, 0, thickness,
                new LinearGradientBrush(Color.FromArgb(64, 223, 223, 223), Color.FromArgb(96, 255, 255, 255), 45));
        }

        private void updatePoly(ref Polygon poly, double x, double y, double width, double height, double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, Brush brush)
        {
            if (poly == null)
            {
                poly = new Polygon { Fill = brush };
                mainCanvas.Children.Add(poly);
            }
            moveAndResize(poly, x, y, width, height);
            poly.Points.Clear();
            poly.Points.Add(new Point(x1, y1));
            poly.Points.Add(new Point(x2, y2));
            poly.Points.Add(new Point(x3, y3));
            poly.Points.Add(new Point(x4, y4));
        }

        private Point pointFromSlot(Slot slot)
        {
            switch (slot.Type)
            {
                case SlotType.Stock:
                    return new Point(Canvas.GetLeft(stockBox), Canvas.GetTop(stockBox) + slot.IndexX * pieceSize);

                case SlotType.Board:
                    return new Point(Canvas.GetLeft(boardBox) + slot.IndexX * pieceSize, Canvas.GetTop(boardBox) + slot.IndexY * pieceSize);

                case SlotType.Joker:
                    return new Point(Canvas.GetLeft(jokersBox) + slot.IndexX * pieceSize, Canvas.GetTop(jokersBox));
            }

            throw new InternalErrorException("Unrecognized slot type in pointFromSlot()");
        }

        private Slot slotFromPoint(Point p) { return slotFromPoint(p.X, p.Y); }

        private Slot slotFromPoint(double x, double y)
        {
            if (x >= Canvas.GetLeft(stockBox) && x < Canvas.GetLeft(stockBox) + pieceSize && y >= Canvas.GetTop(stockBox) && y < Canvas.GetTop(stockBox) + 4 * pieceSize)
                return new Slot { Type = SlotType.Stock, IndexX = (int) ((y - Canvas.GetTop(stockBox)) / pieceSize) };

            if (x >= Canvas.GetLeft(boardBox) && x < Canvas.GetLeft(boardBox) + 4 * pieceSize && y >= Canvas.GetTop(boardBox) && y < Canvas.GetTop(boardBox) + 4 * pieceSize)
                return new Slot { Type = SlotType.Board, IndexX = (int) ((x - Canvas.GetLeft(boardBox)) / pieceSize), IndexY = (int) ((y - Canvas.GetTop(boardBox)) / pieceSize) };

            if (x >= Canvas.GetLeft(jokersBox) && x < Canvas.GetLeft(jokersBox) + 2 * pieceSize && y >= Canvas.GetTop(jokersBox) && y < Canvas.GetTop(jokersBox) + pieceSize)
                return new Slot { Type = SlotType.Joker, IndexX = (x < Canvas.GetLeft(jokersBox) + pieceSize) ? 0 : 1 };

            return null;
        }

        private void updateImages()
        {
            // Update piece images in the stock
            for (int i = 0; i < 4; i++)
            {
                if (Program.Settings.Stock[i] != null)
                    updateImage(Program.Settings.Stock[i], new Slot { Type = SlotType.Stock, IndexX = i });
            }

            // Update joker images in the joker row
            for (int i = 0; i < 2; i++)
            {
                if (Program.Settings.UnusedJokers[i] != null)
                    updateImage(Program.Settings.UnusedJokers[i], null, new Slot { Type = SlotType.Joker, IndexX = i });
            }

            // Update piece, joker and lock images on the board
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Image lockImg = null;
                    Slot slot = new Slot { IndexX = x, IndexY = y, Type = SlotType.Board };
                    if (Program.Settings.Board[y][x] != null)
                    {
                        updateImage(Program.Settings.Board[y][x], slot);
                        lockImg = Program.Settings.Board[y][x].LockImage;
                    }
                    Joker joker = Program.Settings.JokersOnBoard.FirstOrDefault(j => j.IndexX == x && j.IndexY == y);
                    if (joker != null)
                        updateImage(joker, lockImg, slot);
                }
            }

            // Update size and other properties of the focus rectangle (but the position will be handled by imageMouseMove)
            focusRect.Width = pieceSize;
            focusRect.Height = pieceSize;
            focusRect.RadiusX = pieceSize / 5;
            focusRect.RadiusY = pieceSize / 5;
            focusRect.StrokeThickness = pieceSize / 100;

            // Update the markers for each of the sites
            double boardLeft = Canvas.GetLeft(boardBox);
            double boardTop = Canvas.GetTop(boardBox);
            // horizontal rows
            for (int i = 0; i < 4; i++)
                updateSiteMarkerRectangle(ref sites[i].Displays, out sites[i].DisplayCenter, boardLeft, boardTop + i * pieceSize, boardBox.Width, pieceSize, false, false);
            // vertical columns
            for (int i = 0; i < 4; i++)
                updateSiteMarkerRectangle(ref sites[4 + i].Displays, out sites[4 + i].DisplayCenter, boardLeft + i * pieceSize, boardTop, pieceSize, boardBox.Height, false, false);
            // 2×2 squares
            for (int j = 0; j < 3; j++)
                for (int i = 0; i < 3; i++)
                    updateSiteMarkerRectangle(ref sites[8 + (j * 3) + i].Displays, out sites[8 + (j * 3) + i].DisplayCenter, boardLeft + i * pieceSize, boardTop + j * pieceSize, 2 * pieceSize, 2 * pieceSize, false, false);
            // diagonal 1
            double diagonalHeight = boardBox.Height * Math.Sqrt(2) - pieceSize / 2;
            updateSiteMarkerRectangle(ref sites[17].Displays, out sites[17].DisplayCenter, boardLeft + 1.5 * pieceSize, boardTop + 2 * pieceSize - diagonalHeight / 2, pieceSize, diagonalHeight, true, false);
            // diagonal 2
            updateSiteMarkerRectangle(ref sites[18].Displays, out sites[18].DisplayCenter, boardLeft + 1.5 * pieceSize, boardTop + 2 * pieceSize - diagonalHeight / 2, pieceSize, diagonalHeight, false, true);
            // four corners
            if (sites[19].Displays == null)
                sites[19].Displays = new FrameworkElement[4];
            for (int i = 0; i < 4; i++)
            {
                Rectangle r;
                if (sites[19].Displays[i] == null || !(sites[19].Displays[i] is Rectangle))
                {
                    if (sites[19].Displays[i] != null)
                        mainCanvas.Children.Remove(sites[19].Displays[i]);
                    sites[19].Displays[i] = r = new Rectangle { Fill = new SolidColorBrush(Color.FromArgb(128, 255, 128, 64)), Visibility = Visibility.Hidden };
                    mainCanvas.Children.Add(r);
                }
                else
                    r = (Rectangle) sites[19].Displays[i];
                moveAndResize(r, boardLeft + (i % 2) * 3 * pieceSize, boardTop + (i / 2) * 3 * pieceSize, pieceSize, pieceSize);
                r.RadiusX = r.RadiusY = pieceSize / 4;
            }
            sites[19].DisplayCenter = new Point(boardLeft + boardBox.Width / 2, boardTop + boardBox.Height / 2);

            // If we've only just started the game, animate the opacity of the pieces and jokers so that they appear to fade in
            if (firstUpdate != null)
            {
                var list = firstUpdate;
                firstUpdate = null;
                var anim = new AnimateLinear(0, 1, TimeSpan.FromSeconds(2), v =>
                {
                    foreach (var elem in list)
                        elem.Opacity = v;
                });
                anim.Completed += () => updateImages();
            }

            // Update the polygons that make each of the boxes appear like a three-dimensional inlet
            updateInlet(jokersBox, ref jokers3d);
            updateInlet(nextJokerBox, ref nextJoker3d);
            updateInlet(stockBox, ref stock3d);
            updateInlet(boardBox, ref board3d);
            updateInlet(scoreBox, ref score3d);
            updateScore();
            updateNextJokerScore();

            if (Program.Settings.IsGameOver && gameOver == null)
            {
                gameOver = new OutlinedText { Font = new FontFamily("Impact"), FontSize = pieceSize * 1.2, Opacity = 0, Text = "GAME OVER" };
                gameOver.Fill = gameOverFill;
                gameOver.Stroke = new Pen(gameOverStroke, pieceSize / 5) { LineJoin = PenLineJoin.Round };
                var stf = new ScaleTransform(0, 0, gameOver.MinWidth / 2, gameOver.MinHeight / 2);
                gameOver.RenderTransform = stf;

                var anim = new AnimateLinear(0, 1, TimeSpan.FromSeconds(2), v =>
                {
                    stf.ScaleX = v;
                    stf.ScaleY = v;
                    gameOver.Opacity = v;
                });

                mainCanvas.Children.Add(gameOver);
            }
            else if (!Program.Settings.IsGameOver && gameOver != null)
            {
                var go = gameOver;
                gameOver = null;
                var stf = new ScaleTransform(1, 1, go.Width / 2, go.Height / 2);
                go.RenderTransform = stf;
                var anim = new AnimateLinear(1, 2, TimeSpan.FromSeconds(1), v =>
                {
                    stf.ScaleX = v;
                    stf.ScaleY = v;
                    go.Opacity = 2 - v;
                });
                anim.Completed += () => mainCanvas.Children.Remove(go);
            }

            if (gameOver != null)
            {
                gameOver.Stroke = new Pen(gameOverStroke, pieceSize / 5) { LineJoin = PenLineJoin.Round };
                gameOver.FontSize = pieceSize * 1.2;
                moveAndResize(gameOver, mainCanvas.ActualWidth / 2 - gameOver.MinWidth / 2, mainCanvas.ActualHeight / 2 - gameOver.MinHeight / 2, gameOver.MinWidth, gameOver.MinHeight);
            }
        }

        private void updateScore()
        {
            string score = Program.Settings.Score.ToString();
            if (scoreDigits == null)
                scoreDigits = new Image[score.Length];
            else if (scoreDigits.Length < score.Length)
            {
                var newScoreDigits = new Image[score.Length];
                Array.Copy(scoreDigits, 0, newScoreDigits, score.Length - scoreDigits.Length, scoreDigits.Length);
                scoreDigits = newScoreDigits;
            }
            int? firstChange = null;
            for (int i = 0; i < scoreDigits.Length; i++)
            {
                int s = i + score.Length - scoreDigits.Length;
                if (s < 0 && scoreDigits[i] != null)
                {
                    if (firstChange == null)
                        firstChange = i;
                    var sd = scoreDigits[i];
                    scoreDigits[i] = null;
                    var sf = new ScaleTransform(1, 1, sd.Width / 2, sd.Height / 2);
                    sd.RenderTransform = sf;
                    Animate.LaterMs(50 * (i - firstChange.Value + 2), () =>
                    {
                        var anim = new AnimateLinear(1, 0, TimeSpan.FromMilliseconds(200), v => sf.ScaleY = v);
                        anim.Completed += () => mainCanvas.Children.Remove(sd);
                    });
                }
                else if (s >= 0 && scoreDigits[i] == null)
                {
                    if (firstChange == null)
                        firstChange = i;
                    scoreDigits[i] = new Image { Source = digitImages[score[s] - '0'], Width = pieceSize, Height = pieceSize };
                    mainCanvas.Children.Add(scoreDigits[i]);
                    var sf = new ScaleTransform(1, 0, pieceSize / 2, pieceSize / 2);
                    scoreDigits[i].RenderTransform = sf;
                    Animate.LaterMs(50 * (i - firstChange.Value + 5), () => new AnimateLinear(0, 1, TimeSpan.FromMilliseconds(200), v => sf.ScaleY = v));
                }
                else if (s >= 0 && scoreDigits[i] != null && scoreDigits[i].Source != digitImages[score[s] - '0'])
                {
                    if (firstChange == null)
                        firstChange = i;
                    var sd = scoreDigits[i];
                    var sf = new ScaleTransform(1, 1, sd.Width / 2, sd.Height / 2);
                    sd.RenderTransform = sf;
                    int digit = score[s] - '0';
                    Animate.LaterMs(50 * (i - firstChange.Value + 2), () => new AnimateLinear(1, 0, TimeSpan.FromMilliseconds(200), v => sf.ScaleY = v));
                    Animate.LaterMs(50 * (i - firstChange.Value + 5), () =>
                    {
                        sd.Source = digitImages[digit];
                        new AnimateLinear(0, 1, TimeSpan.FromMilliseconds(200), v => sf.ScaleY = v);
                    });
                }

                if (scoreDigits[i] != null)
                    moveAndResize(scoreDigits[i], Canvas.GetLeft(scoreBox) + scoreBox.Width - (scoreDigits.Length - i) * pieceSize * 2 / 3 - pieceSize / 5, Canvas.GetTop(scoreBox), pieceSize, pieceSize);
            }
        }

        private void updateNextJokerScore()
        {
            // Update the next-joker progress bar
            double ratio = (double) (Program.Settings.Score - Program.Settings.NextJokerAtPrev) / (Program.Settings.NextJokerAt - Program.Settings.NextJokerAtPrev);
            moveAndResize(progressBarProgress, Canvas.GetLeft(progressBar), Canvas.GetTop(progressBar), progressBar.Width * ratio, progressBar.Height);

            string score = Program.Settings.NextJokerAt.ToString();

            double theoreticalDigitWidth = nextJokerBox.Width * 0.9 / score.Length;
            double imageSourceWidth = theoreticalDigitWidth * 3 / 2;

            FormattedText ft = new FormattedText("Next joker at:", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(nextJokerLabel.FontFamily, nextJokerLabel.FontStyle, nextJokerLabel.FontWeight, nextJokerLabel.FontStretch), nextJokerLabel.FontSize, Brushes.Black);
            double y = Canvas.GetTop(nextJokerBox) + nextJokerBox.Height / 2 + ft.Height / 2 - progressBar.Height / 2 - imageSourceWidth / 2;

            if (nextJokerDigits == null)
                nextJokerDigits = new Image[score.Length];
            else if (nextJokerDigits.Length < score.Length)
            {
                var newScoreDigits = new Image[score.Length];
                Array.Copy(nextJokerDigits, 0, newScoreDigits, score.Length - nextJokerDigits.Length, nextJokerDigits.Length);
                nextJokerDigits = newScoreDigits;
            }
            double offset = (imageSourceWidth - theoreticalDigitWidth) / 2 - nextJokerBox.Width * 0.05 + (nextJokerDigits.Length - score.Length) * theoreticalDigitWidth;
            int? firstChange = null;
            for (int i = 0; i < nextJokerDigits.Length; i++)
            {
                int s = i + score.Length - nextJokerDigits.Length;
                bool makeNull = false;
                if (s < 0 && nextJokerDigits[i] != null)
                {
                    if (firstChange == null)
                        firstChange = i;
                    var sd = nextJokerDigits[i];
                    makeNull = true;
                    var sf = new ScaleTransform(1, 1, imageSourceWidth / 2, imageSourceWidth / 2);
                    sd.RenderTransform = sf;
                    Animate.LaterMs(50 * (i - firstChange.Value + 2), () =>
                    {
                        var anim = new AnimateLinear(1, 0, TimeSpan.FromMilliseconds(200), v => sf.ScaleY = v);
                        anim.Completed += () => mainCanvas.Children.Remove(sd);
                    });
                }
                else if (s >= 0 && nextJokerDigits[i] == null)
                {
                    if (firstChange == null)
                        firstChange = i;
                    nextJokerDigits[i] = new Image { Source = digitImages[score[s] - '0'], Width = pieceSize, Height = pieceSize };
                    mainCanvas.Children.Add(nextJokerDigits[i]);
                    var sf = new ScaleTransform(1, 0, imageSourceWidth / 2, imageSourceWidth / 2);
                    nextJokerDigits[i].RenderTransform = sf;
                    Animate.LaterMs(50 * (i - firstChange.Value + 5), () => new AnimateLinear(0, 1, TimeSpan.FromMilliseconds(200), v => sf.ScaleY = v));
                }
                else if (s >= 0 && nextJokerDigits[i] != null && nextJokerDigits[i].Source != digitImages[score[s] - '0'])
                {
                    if (firstChange == null)
                        firstChange = i;
                    var sd = nextJokerDigits[i];
                    var sf = new ScaleTransform(1, 1, imageSourceWidth / 2, imageSourceWidth / 2);
                    sd.RenderTransform = sf;
                    int digit = score[s] - '0';
                    Animate.LaterMs(50 * (i - firstChange.Value + 2), () => new AnimateLinear(1, 0, TimeSpan.FromMilliseconds(200), v => sf.ScaleY = v));
                    Animate.LaterMs(50 * (i - firstChange.Value + 5), () =>
                    {
                        sd.Source = digitImages[digit];
                        new AnimateLinear(0, 1, TimeSpan.FromMilliseconds(200), v => sf.ScaleY = v);
                    });
                }

                if (nextJokerDigits[i] != null)
                    moveAndResize(nextJokerDigits[i], Canvas.GetLeft(nextJokerBox) - offset + theoreticalDigitWidth * i, y, imageSourceWidth, imageSourceWidth);
                if (makeNull)
                    nextJokerDigits[i] = null;
            }
        }

        private void updateSiteMarkerRectangle(ref FrameworkElement[] elem, out Point center, double x, double y, double width, double height, bool rotateLeft, bool rotateRight)
        {
            if (elem == null)
                elem = new FrameworkElement[1];

            Rectangle rct;
            if (elem[0] == null || !(elem[0] is Rectangle))
            {
                if (elem[0] != null)
                    mainCanvas.Children.Remove(elem[0]);
                elem[0] = rct = new Rectangle { Fill = new SolidColorBrush(Color.FromArgb(128, 255, 128, 64)), Visibility = Visibility.Hidden };
                mainCanvas.Children.Add(rct);
            }
            else
                rct = (Rectangle) elem[0];
            moveAndResize(rct, x, y, width, height);
            if (rotateLeft || rotateRight)
                rct.RenderTransform = new RotateTransform { CenterX = width / 2, CenterY = height / 2, Angle = rotateLeft ? -45 : 45 };
            rct.RadiusX = rct.RadiusY = pieceSize / 4;
            center = new Point(x + width / 2, y + height / 2);
        }

        private void updateImage(Piece piece, Slot slot)
        {
            var p = pointFromSlot(slot);

            if (piece.Image == null)
            {
                piece.Image = createImage(pieceImages[piece.Colour + 4 * piece.Shape], slot, false);
                if (firstUpdate != null)
                    firstUpdate.Add(piece.Image);
            }
            else
            {
                ImageTag tag = (ImageTag) piece.Image.Tag;
                if (tag.AnimatePosition != null && !tag.AnimatePosition.IsCompleted)
                {
                    tag.AnimatePosition.ToX = p.X;
                    tag.AnimatePosition.ToY = p.Y;
                    piece.Image.Width = pieceSize;
                    piece.Image.Height = pieceSize;
                }
                else
                    moveAndResize(piece.Image, p.X, p.Y, pieceSize, pieceSize);
            }

            if (piece.Locked)
            {
                if (piece.LockImage == null)
                {
                    piece.LockImage = new Image { Source = lockImage, Stretch = Stretch.Fill };
                    mainCanvas.Children.Add(piece.LockImage);
                    if (firstUpdate != null)
                        firstUpdate.Add(piece.LockImage);
                }
                piece.LockImage.Width = pieceSize;
                piece.LockImage.Height = pieceSize;
                Canvas.SetLeft(piece.LockImage, p.X);
                Canvas.SetTop(piece.LockImage, p.Y);
            }
        }

        private void updateImage(Joker joker, Image lockImg, Slot slot)
        {
            var p = pointFromSlot(slot);

            if (joker.Image == null)
            {
                joker.Image = createImage(jokerImage, slot, true);
                if (firstUpdate != null)
                    firstUpdate.Add(joker.Image);
            }
            else
                moveAndResize(joker.Image, p.X, p.Y, pieceSize, pieceSize);

            if (joker.Locked)
            {
                if (lockImg == null)
                {
                    if (joker.LockImage == null)
                    {
                        joker.LockImage = new Image { Source = lockImage, Stretch = Stretch.Fill };
                        mainCanvas.Children.Add(joker.LockImage);
                        if (firstUpdate != null)
                            firstUpdate.Add(joker.LockImage);
                    }
                    joker.LockImage.Width = pieceSize;
                    joker.LockImage.Height = pieceSize;
                    Canvas.SetLeft(joker.LockImage, p.X);
                    Canvas.SetTop(joker.LockImage, p.Y);
                }
                else
                    moveAfter(lockImg, joker.Image);
            }
        }

        private void moveAfter(UIElement moveWhat, UIElement after)
        {
            var ind = mainCanvas.Children.IndexOf(moveWhat);
            mainCanvas.Children.RemoveAt(ind);
            if (after == null)
                mainCanvas.Children.Add(moveWhat);
            else
                mainCanvas.Children.Insert(mainCanvas.Children.IndexOf(after) + 1, moveWhat);
        }

        private Image createImage(BitmapSource bitmapSource, Slot slot, bool isJoker)
        {
            var toPt = pointFromSlot(slot);
            var fromPt = new Point(Canvas.GetLeft(dealButton), Canvas.GetTop(dealButton));
            var stf = new ScaleTransform(0, 0, pieceSize / 2, pieceSize / 2);
            var img = new Image
            {
                Source = bitmapSource,
                Stretch = Stretch.Fill,
                Width = pieceSize,
                Height = pieceSize,
                RenderTransform = stf,
                Opacity = firstUpdate == null ? 1 : 0
            };
            ImageTag tag;
            img.Tag = tag = new ImageTag
            {
                Slot = slot,
                ScaleTransform = stf,
                IsJoker = isJoker
            };
            img.MouseDown += imageMouseDown;
            img.MouseMove += imageMouseMove;
            img.MouseUp += imageMouseUp;
            Canvas.SetLeft(img, toPt.X);
            Canvas.SetTop(img, toPt.Y);

            if (slot.Type == SlotType.Stock && firstUpdate == null)
            {
                img.Visibility = Visibility.Hidden;
                Animate.LaterMs(1 + 100 * (3 - slot.IndexX), () =>
                {
                    double x = fromPt.X + ((slot.IndexX % 2) * 2 - 1) * pieceSize * (slot.IndexX + 1) / 2;
                    var controlPoints = new Point[] { fromPt, new Point(x, (fromPt.Y * 2 + toPt.Y) / 3), new Point(x, (fromPt.Y + toPt.Y * 2) / 3), toPt };
                    img.Visibility = Visibility.Visible;
                    tag.CancelableAnimate = new AnimateMultiple(
                        new AnimateLinear(0, 1, TimeSpan.FromMilliseconds(250), v => { stf.ScaleX = v; stf.ScaleY = v; }),
                        new AnimateBézier(controlPoints, TimeSpan.FromMilliseconds(250), p => { Canvas.SetLeft(img, p.X); Canvas.SetTop(img, p.Y); })
                    );
                });
            }
            else
            {
                tag.CancelableAnimate = new AnimateLinear(0, 1, TimeSpan.FromMilliseconds(250), v => { stf.ScaleX = v; stf.ScaleY = v; });
            }

            mainCanvas.Children.Add(img);
            return img;
        }

        private void imageMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (animating)
                return;

            Image img = (Image) sender;
            ImageTag tag = (ImageTag) img.Tag;
            if (tag.AboutToDisappear)
                return;

            img.CaptureMouse();
            moveAfter(img, null);

            if (tag.CancelableAnimate != null)
                tag.CancelableAnimate.Stop();

            var stf = tag.ScaleTransform;
            var animateScale = new AnimateLinear(stf.ScaleX, 1.25, TimeSpan.FromMilliseconds(200), val => { stf.ScaleX = stf.ScaleY = val; });
            var animateOpacity = new AnimateLinear(img.Opacity, 0.7, TimeSpan.FromMilliseconds(200), val => { img.Opacity = val; });
            tag.AnimatePosition = new AnimateLinearXY(
                Canvas.GetLeft(img), e.GetPosition(mainCanvas).X - pieceSize / 2,
                Canvas.GetTop(img), e.GetPosition(mainCanvas).Y - pieceSize / 2,
                TimeSpan.FromMilliseconds(200),
                (x, y) =>
                {
                    Canvas.SetLeft(img, x);
                    Canvas.SetTop(img, y);
                }
            );
            tag.CancelableAnimate = new AnimateMultiple(animateScale, animateOpacity, tag.AnimatePosition);
        }

        private void imageMouseMove(object sender, MouseEventArgs e)
        {
            if (animating)
                return;
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Image img = (Image) sender;
            if (!img.IsMouseCaptured)
                return;

            ImageTag tag = (ImageTag) img.Tag;
            if (tag.AboutToDisappear)
                return;

            var pos = e.GetPosition(mainCanvas);
            if (tag.AnimatePosition != null && !tag.AnimatePosition.IsCompleted)
            {
                tag.AnimatePosition.ToX = pos.X - pieceSize / 2;
                tag.AnimatePosition.ToY = pos.Y - pieceSize / 2;
            }
            else
            {
                Canvas.SetLeft(img, pos.X - pieceSize / 2);
                Canvas.SetTop(img, pos.Y - pieceSize / 2);
            }
            var slot = slotFromPoint(e.GetPosition(mainCanvas));
            if (canMove(tag.Slot, slot, tag.IsJoker) || tag.Slot.Equals(slot))
            {
                var p = pointFromSlot(slot);
                moveAndResize(focusRect, p.X, p.Y, pieceSize, pieceSize);
                focusRect.Visibility = Visibility.Visible;
            }
            else
                focusRect.Visibility = Visibility.Hidden;
        }

        private void imageMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (animating)
                return;

            Image img = (Image) sender;
            if (!img.IsMouseCaptured)
                return;

            ImageTag tag = (ImageTag) img.Tag;
            if (tag.AboutToDisappear)
                return;

            img.ReleaseMouseCapture();
            focusRect.Visibility = Visibility.Hidden;

            if (tag.CancelableAnimate != null)
                tag.CancelableAnimate.Stop();

            var stf = tag.ScaleTransform;
            var anim1 = new AnimateLinear(stf.ScaleX, 1, TimeSpan.FromMilliseconds(200), val => { stf.ScaleX = stf.ScaleY = val; });
            var anim2 = new AnimateLinear(img.Opacity, 1, TimeSpan.FromMilliseconds(200), val => { img.Opacity = val; });

            var newSlot = slotFromPoint(e.GetPosition(mainCanvas));
            Point dest;
            if (canMove(tag.Slot, newSlot, tag.IsJoker))
            {
                move(tag.Slot, newSlot, tag.IsJoker);
                dest = pointFromSlot(newSlot);
                tag.Slot = newSlot;
            }
            else
                dest = pointFromSlot(tag.Slot);
            tag.AnimatePosition = new AnimateLinearXY(Canvas.GetLeft(img), dest.X, Canvas.GetTop(img), dest.Y, TimeSpan.FromMilliseconds(200), (x, y) =>
            {
                Canvas.SetLeft(img, x);
                Canvas.SetTop(img, y);
            });
            tag.CancelableAnimate = new AnimateMultiple(anim1, anim2, tag.AnimatePosition);
        }

        private bool canMove(Slot oldSlot, Slot newSlot, bool movingJoker)
        {
            if (oldSlot == null || newSlot == null)
                return false;

            if (movingJoker)
                return ((oldSlot.Type == SlotType.Joker && Program.Settings.UnusedJokers[oldSlot.IndexX] != null)
                        || (oldSlot.Type == SlotType.Board && Program.Settings.JokersOnBoard.Any(j => j.IndexX == oldSlot.IndexX && j.IndexY == oldSlot.IndexY)))
                    && ((newSlot.Type == SlotType.Joker && Program.Settings.UnusedJokers[newSlot.IndexX] == null)
                        || (newSlot.Type == SlotType.Board && !Program.Settings.JokersOnBoard.Any(j => j.IndexX == newSlot.IndexX && j.IndexY == newSlot.IndexY)));

            return ((oldSlot.Type == SlotType.Stock && Program.Settings.Stock[oldSlot.IndexX] != null)
                    || (oldSlot.Type == SlotType.Board && Program.Settings.Board[oldSlot.IndexY][oldSlot.IndexX] != null))
                && ((newSlot.Type == SlotType.Stock && Program.Settings.Stock[newSlot.IndexX] == null)
                    || (newSlot.Type == SlotType.Board && Program.Settings.Board[newSlot.IndexY][newSlot.IndexX] == null));
        }

        // Warning: Assumes that moving a piece from oldSlot to newSlot is possible and valid; use canMove() first to check
        private void move(Slot oldSlot, Slot newSlot, bool moveJoker)
        {
            if (moveJoker)
            {
                Joker joker;

                // Remove the joker from the old slot
                if (oldSlot.Type == SlotType.Joker)
                {
                    joker = Program.Settings.UnusedJokers[oldSlot.IndexX];
                    Program.Settings.UnusedJokers[oldSlot.IndexX] = null;
                }
                else
                {
                    joker = Program.Settings.JokersOnBoard.FirstOrDefault(j => j.IndexX == oldSlot.IndexX && j.IndexY == oldSlot.IndexY);
                    Program.Settings.JokersOnBoard = Program.Settings.JokersOnBoard.Where(j => j != joker).ToArray();
                }

                // Insert the joker at the new slot
                joker.IndexX = newSlot.IndexX;
                joker.IndexY = newSlot.IndexY;
                if (newSlot.Type == SlotType.Joker)
                    Program.Settings.UnusedJokers[newSlot.IndexX] = joker;
                else if (newSlot.Type == SlotType.Board)
                    Program.Settings.JokersOnBoard = Program.Settings.JokersOnBoard.Concat(joker).ToArray();
            }
            else
            {
                Piece piece;

                // Remove the piece from the old slot
                if (oldSlot.Type == SlotType.Stock)
                {
                    piece = Program.Settings.Stock[oldSlot.IndexX];
                    Program.Settings.Stock[oldSlot.IndexX] = null;
                }
                else
                {
                    piece = Program.Settings.Board[oldSlot.IndexY][oldSlot.IndexX];
                    Program.Settings.Board[oldSlot.IndexY][oldSlot.IndexX] = null;
                }

                // Insert the piece at the new slot
                if (newSlot.Type == SlotType.Stock)
                    Program.Settings.Stock[newSlot.IndexX] = piece;
                else if (newSlot.Type == SlotType.Board)
                    Program.Settings.Board[newSlot.IndexY][newSlot.IndexX] = piece;
            }
        }

        private void newGame()
        {
            var getRidOf = new List<Image>();
            foreach (var s in Program.Settings.Stock.Where(c => c != null))
            {
                if (s.Image != null)
                    getRidOf.Add(s.Image);
                if (s.LockImage != null)
                    getRidOf.Add(s.LockImage);
            }
            foreach (var b in Program.Settings.Board.SelectMany(r => r).Where(c => c != null))
            {
                if (b.Image != null)
                    getRidOf.Add(b.Image);
                if (b.LockImage != null)
                    getRidOf.Add(b.LockImage);
            }
            foreach (var j in Program.Settings.JokersOnBoard.Concat(Program.Settings.UnusedJokers).Where(c => c != null))
            {
                if (j.Image != null)
                    getRidOf.Add(j.Image);
                if (j.LockImage != null)
                    getRidOf.Add(j.LockImage);
            }
            foreach (var tag in getRidOf.Select(img => img.Tag as ImageTag))
                if (tag != null)
                    tag.AboutToDisappear = true;

            var stfs = new ScaleTransform[getRidOf.Count];
            for (int i = 0; i < stfs.Length; i++)
            {
                stfs[i] = new ScaleTransform(1, 1, mainCanvas.ActualWidth / 2 - Canvas.GetLeft(getRidOf[i]) + pieceSize * Rnd.NextDouble(-5, 5), mainCanvas.ActualHeight / 2 - Canvas.GetTop(getRidOf[i]) + pieceSize * Rnd.NextDouble(-5, 5));
                getRidOf[i].RenderTransform = stfs[i];
            }
            var anim = new AnimateLinear(1, 2, TimeSpan.FromSeconds(1), v =>
            {
                for (int i = 0; i < stfs.Length; i++)
                {
                    stfs[i].ScaleX = v;
                    stfs[i].ScaleY = v;
                    getRidOf[i].Opacity = 2 - v;
                }
            });
            anim.Completed += () =>
            {
                foreach (var image in getRidOf)
                    mainCanvas.Children.Remove(image);
            };

            Program.Settings.StartNewGame();
            updateImages();
        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                if (Program.Settings.IsGameOver || MessageBox.Show("Are you sure you wish to abandon this game and start a new one?", "New game", MessageBoxButton.YesNo, MessageBoxImage.Hand) == MessageBoxResult.Yes)
                    newGame();
            }
        }
    }
}
