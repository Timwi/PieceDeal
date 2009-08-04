using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PieceDeal
{
    internal class DraggableObject { }

    internal class Piece : DraggableObject, IEqualityComparer<Piece>, IEquatable<Piece>
    {
        internal int Shape;
        internal int Colour;
        internal bool Locked;

        public bool Equals(Piece x, Piece y) { return x.Colour == y.Colour && x.Shape == y.Shape; }
        public int GetHashCode(Piece obj) { return (Colour.ToString() + Shape.ToString()).GetHashCode(); }
        public bool Equals(Piece other) { return other.Colour == Colour && other.Shape == Shape; }
    }

    internal class Joker : DraggableObject
    {
        internal int IndexX;
        internal int IndexY;
        internal bool Locked;
    }

    internal enum SlotType
    {
        Stock,
        Board,
        Joker
    };

    internal class Slot
    {
        internal SlotType Type;
        internal int IndexX;
        internal int IndexY;
    }

    internal class Settings : ManagedForm.Settings
    {
        internal Piece[] Stock;
        internal Piece[][] Board;
        internal Joker[] UnusedJokers;
        internal Joker[] JokersOnBoard;
        internal int NextJokerAt;
        internal int NextJokerAtStep;
        internal int Score;

        public Settings() { StartNewGame(); }

        internal void StartNewGame()
        {
            Stock = new Piece[4];
            Board = new[] { new Piece[4], new Piece[4], new Piece[4], new Piece[4] };
            UnusedJokers = new Joker[2];
            JokersOnBoard = new Joker[0];
            Score = 0;
            NextJokerAt = 250;
            NextJokerAtStep = 250;
        }

        internal bool IsValid
        {
            get
            {
                if (Stock == null || Stock.Length != 4)
                    return false;
                if (Board == null || Board.Length != 4 || Board.Any(b => b == null || b.Length != 4))
                    return false;
                if (Board.Any(row => row.Any(cell => cell != null && (cell.Colour < 0 || cell.Colour > 3 || cell.Shape < 0 || cell.Shape > 3))))
                    return false;
                if (UnusedJokers == null || UnusedJokers.Length != 2 || UnusedJokers.Any(j => j != null && j.Locked))
                    return false;
                if (JokersOnBoard == null || JokersOnBoard.Any(j => j == null || j.IndexX < 0 || j.IndexX > 3 || j.IndexY < 0 || j.IndexY > 3))
                    return false;
                if (Score < 0 || NextJokerAt < Score)
                    return false;
                if (NextJokerAtStep % 250 != 0 || NextJokerAtStep > 1500)
                    return false;

                if (Stock.Count(s => s != null) > FreeSpaces)
                    return false;

                return true;
            }
        }

        internal int FreeSpaces
        {
            get
            {
                int spaces = 0;
                for (int x = 0; x < 4; x++)
                    for (int y = 0; y < 4; y++)
                        if (Board[y][x] == null && !JokersOnBoard.Any(j => j.IndexX == x && j.IndexY == y && j.Locked))
                            spaces++;
                return spaces;
            }
        }

        internal bool IsGameOver
        {
            get
            {
                if (JokersOnBoard.Any(j => !j.Locked))
                    return false;
                if (UnusedJokers.Any(j => j != null))
                    return false;

                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        if (Board[y][x] == null && !JokersOnBoard.Any(j => j.IndexX == x && j.IndexY == y && j.Locked))
                            return false;
                        if (Board[y][x] != null && !Board[y][x].Locked)
                            return false;
                    }
                }
                return true;
            }
        }
    }

    internal static class Program
    {
        public static Settings Settings;
        public static FMOD.System FModSystem;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                FMOD.Factory.System_Create(ref FModSystem);
                uint version = 0;
                FModSystem.getVersion(ref version);
                if (version < FMOD.VERSION.number)
                {
                    MessageBox.Show("You are using an old version of FMOD (" + version.ToString("X") + ").  This program requires " + FMOD.VERSION.number.ToString("X") + ".\n\nPlease ensure the required version is available and then try again.");
                    return;
                }
                FModSystem.init(32, FMOD.INITFLAG.NORMAL, IntPtr.Zero);
            }
            catch (Exception e)
            {
                MessageBox.Show("The sound system could not be initialised.\n\n" + e.Message + "\n\nPlease ensure all required components are available and then try again.");
                return;
            }

            SettingsUtil.LoadSettings(out Settings, "PieceDeal");
            if (Settings == null || !Settings.IsValid)
            {
#if DEBUG
                throw new Exception("Settings file is invalid!");
#else
                Settings = new Settings();
#endif
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Mainform());
            SettingsUtil.SaveSettings(Settings, "PieceDeal", SettingsUtil.OnFailure.DoNothing);
        }

        /// <summary>
        /// Returns true if and only if the input sequence has at least four elements, and the first four elements constitute two pairs of equal elements.
        /// </summary>
        internal static bool TwoPairs<T>(this IEnumerable<T> source) where T : IEquatable<T>
        {
            var e = source.GetEnumerator();
            if (!e.MoveNext()) return false;
            T one = e.Current;
            if (!e.MoveNext()) return false;
            T two = e.Current;
            if (!e.MoveNext()) return false;
            T three = e.Current;
            if (!e.MoveNext()) return false;
            T four = e.Current;
            return (one.Equals(two) && three.Equals(four)) ||
                    (one.Equals(three) && two.Equals(four)) ||
                    (one.Equals(four) && two.Equals(three));
        }

        /// <summary>
        /// Returns true if and only if the input sequence has at least three elements, and the first three elements contain two elements that are equal.
        /// </summary>
        internal static bool OnePair<T>(this IEnumerable<T> source) where T : IEquatable<T>
        {
            var e = source.GetEnumerator();
            if (!e.MoveNext()) return false;
            T one = e.Current;
            if (!e.MoveNext()) return false;
            T two = e.Current;
            if (!e.MoveNext()) return false;
            T three = e.Current;
            return one.Equals(two) || one.Equals(three) || two.Equals(three);
        }

        /// <summary>
        /// Expects two 800x600 bitmaps which represent a graphic against a black and a white background.
        /// Returns an approximation of the graphic with alpha transparency.
        /// </summary>
        internal static Bitmap createAlphaBitmap(Bitmap bmpBlack, Bitmap bmpWhite)
        {
            Bitmap output = new Bitmap(600, 600, PixelFormat.Format32bppArgb);
            unsafe
            {
                var blackData = bmpBlack.LockBits(new Rectangle(0, 0, bmpBlack.Width, bmpBlack.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var whiteData = bmpWhite.LockBits(new Rectangle(0, 0, bmpWhite.Width, bmpWhite.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var outputData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                for (int y = 0; y < 600; y++)
                {
                    byte* pBlack = (byte*) blackData.Scan0 + y * blackData.Stride;
                    byte* pWhite = (byte*) whiteData.Scan0 + y * whiteData.Stride;
                    byte* pOut = (byte*) outputData.Scan0 + y * outputData.Stride;
                    for (int outputX = 0; outputX < 600; outputX++)
                    {
                        int intputX = outputX + 100;
                        double bBlack = (double) pBlack[3 * intputX] / 255;
                        double gBlack = (double) pBlack[3 * intputX + 1] / 255;
                        double rBlack = (double) pBlack[3 * intputX + 2] / 255;

                        double bWhite = (double) pWhite[3 * intputX] / 255;
                        double gWhite = (double) pWhite[3 * intputX + 1] / 255;
                        double rWhite = (double) pWhite[3 * intputX + 2] / 255;

                        double alpha = Math.Min(rBlack, Math.Min(gBlack, bBlack)) + 1 - Math.Max(rWhite, Math.Max(gWhite, bWhite));
                        double red = rBlack / (rBlack + 1 - rWhite);
                        double green = gBlack / (gBlack + 1 - gWhite);
                        double blue = bBlack / (bBlack + 1 - bWhite);

                        pOut[4 * outputX] = (byte) (255 * blue);
                        pOut[4 * outputX + 1] = (byte) (255 * green);
                        pOut[4 * outputX + 2] = (byte) (255 * red);
                        pOut[4 * outputX + 3] = (byte) (255 * alpha);
                    }
                }
                bmpBlack.UnlockBits(blackData);
                bmpWhite.UnlockBits(whiteData);
                output.UnlockBits(outputData);
            }
            return output;
        }
    }
}
