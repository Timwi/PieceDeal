using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using RT.Util.Xml;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;

namespace PieceDeal
{
    class Piece : IEqualityComparer<Piece>, IEquatable<Piece>
    {
        public int Shape;
        public int Colour;
        public bool Locked;

        [XmlIgnore]
        public Image Image;
        [XmlIgnore]
        public Image LockImage;

        public bool Equals(Piece x, Piece y) { return x.Colour == y.Colour && x.Shape == y.Shape; }
        public int GetHashCode(Piece obj) { return (Colour.ToString() + Shape.ToString()).GetHashCode(); }
        public bool Equals(Piece other) { return other.Colour == Colour && other.Shape == Shape; }
    }

    class Joker
    {
        public int IndexX;
        public int IndexY;
        public bool Locked;

        [XmlIgnore]
        public Image Image;
        [XmlIgnore]
        public Image LockImage;
    }

    enum SlotType
    {
        Stock,
        Board,
        Joker
    };

    class Slot : IEquatable<Slot>
    {
        public SlotType Type;
        public int IndexX;
        public int IndexY;

        public bool Equals(Slot other)
        {
            if (other == null)
                return false;

            // IndexY only matters on the board
            if (Type == SlotType.Board)
                return other.Type == SlotType.Board && other.IndexX == IndexX && other.IndexY == IndexY;
            else
                return other.Type == Type && other.IndexX == IndexX;
        }
    }

    class ImageTag
    {
        public Slot Slot;
        public bool IsJoker;
        public ScaleTransform ScaleTransform;
        public IAnimate CancelableAnimate;
        public AnimateLinearXY AnimatePosition;
        public bool AboutToDisappear;
    }
}
