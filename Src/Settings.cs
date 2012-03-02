using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.Forms;

namespace PieceDeal
{
    [Settings("PieceDeal", SettingsKind.UserSpecific)]
    class Settings : SettingsBase
    {
        public Piece[] Stock;
        public Piece[][] Board;
        public Joker[] UnusedJokers;
        public Joker[] JokersOnBoard;
        public int NextJokerAt;
        public int NextJokerAtPrev;
        public int NextJokerAtStep;
        public int Score;

        public ManagedWindow.Settings MainWindowSettings = new ManagedWindow.Settings();

        public Settings() { StartNewGame(); }

        public void StartNewGame()
        {
            Stock = new Piece[4];
            Board = new[] { new Piece[4], new Piece[4], new Piece[4], new Piece[4] };
            UnusedJokers = new Joker[2];
            JokersOnBoard = new Joker[0];
            Score = 0;
            NextJokerAt = 250;
            NextJokerAtPrev = 0;
            NextJokerAtStep = 250;
        }

        public bool IsValid
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
                if (Score < 0 || NextJokerAt <= Score || NextJokerAtPrev > Score || NextJokerAt % 250 != 0 || NextJokerAtPrev % 250 != 0)
                    return false;
                if (NextJokerAtStep % 250 != 0 || NextJokerAtStep > 1500)
                    return false;

                if (Stock.Count(s => s != null) > FreeSpaces)
                    return false;

                return true;
            }
        }

        public int FreeSpaces
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

        public bool IsGameOver
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

    static class Program
    {
        public static Settings Settings;

        /// <summary>
        /// Returns true if and only if the input sequence has at least four elements, and the first four elements constitute two pairs of equal elements.
        /// </summary>
        public static bool TwoPairs<T>(this IEnumerable<T> source) where T : IEquatable<T>
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
        public static bool OnePair<T>(this IEnumerable<T> source) where T : IEquatable<T>
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
    }
}
