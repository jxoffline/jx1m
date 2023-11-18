using System;

namespace FS.Drawing
{
    /// <summary>
    /// Point with integer position
    /// </summary>
    [Serializable]
    public struct Point
    {
        private int x, y;

        /// <summary>
        ///	Empty Shared Field
        /// </summary>
        ///
        /// <remarks>
        ///	An uninitialized Point Structure.
        /// </remarks>

        public static readonly Point Empty;

        /// <summary>
        ///	Ceiling Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a Point structure from a PointF structure by
        ///	taking the ceiling of the X and Y properties.
        /// </remarks>

        public static Point Ceiling(PointF value)
        {
            int x, y;
            checked
            {
                x = (int)Math.Ceiling(value.X);
                y = (int)Math.Ceiling(value.Y);
            }

            return new Point(x, y);
        }

        /// <summary>
        ///	Round Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a Point structure from a PointF structure by
        ///	rounding the X and Y properties.
        /// </remarks>

        public static Point Round(PointF value)
        {
            int x, y;
            checked
            {
                x = (int)Math.Round(value.X);
                y = (int)Math.Round(value.Y);
            }

            return new Point(x, y);
        }

        /// <summary>
        ///	Truncate Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a Point structure from a PointF structure by
        ///	truncating the X and Y properties.
        /// </remarks>

        // LAMESPEC: Should this be floor, or a pure cast to int?

        public static Point Truncate(PointF value)
        {
            int x, y;
            checked
            {
                x = (int)value.X;
                y = (int)value.Y;
            }

            return new Point(x, y);
        }

        /// <summary>
        ///	Equality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two Point objects. The return value is
        ///	based on the equivalence of the X and Y properties 
        ///	of the two points.
        /// </remarks>

        public static bool operator ==(Point left, Point right)
        {
            return ((left.X == right.X) && (left.Y == right.Y));
        }

        /// <summary>
        ///	Inequality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two Point objects. The return value is
        ///	based on the equivalence of the X and Y properties 
        ///	of the two points.
        /// </remarks>

        public static bool operator !=(Point left, Point right)
        {
            return ((left.X != right.X) || (left.Y != right.Y));
        }

        /// <summary>
        ///	Point to PointF Conversion
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a PointF based on the coordinates of a given 
        ///	Point. No explicit cast is required.
        /// </remarks>

        public static implicit operator PointF(Point p)
        {
            return new PointF(p.X, p.Y);
        }


        /// <summary>
        ///	Point Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a Point from an integer which holds the Y
        ///	coordinate in the high order 16 bits and the X
        ///	coordinate in the low order 16 bits.
        /// </remarks>

        public Point(int dw)
        {
            y = dw >> 16;
            x = dw & 0xffff;
        }

        /// <summary>
        ///	Point Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a Point from a specified x,y coordinate pair.
        /// </remarks>

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }


        /// <summary>
        ///	IsEmpty Property
        /// </summary>
        ///
        /// <remarks>
        ///	Indicates if both X and Y are zero.
        /// </remarks>

        public bool IsEmpty
        {
            get
            {
                return ((x == 0) && (y == 0));
            }
        }

        /// <summary>
        ///	X Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the Point.
        /// </remarks>

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        /// <summary>
        ///	Y Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the Point.
        /// </remarks>

        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        /// <summary>
        ///	Equals Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks equivalence of this Point and another object.
        /// </remarks>

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
                return false;

            return (this == (Point)obj);
        }

        /// <summary>
        ///	GetHashCode Method
        /// </summary>
        ///
        /// <remarks>
        ///	Calculates a hashing value.
        /// </remarks>

        public override int GetHashCode()
        {
            return x ^ y;
        }

        /// <summary>
        ///	Offset Method
        /// </summary>
        ///
        /// <remarks>
        ///	Moves the Point a specified distance.
        /// </remarks>

        public void Offset(int dx, int dy)
        {
            x += dx;
            y += dy;
        }

#if NET_2_0
		public static Point Add (Point pt, Size sz)
		{
			return new Point (pt.X + sz.Width, pt.Y + sz.Height);
		}

		public void Offset (Point p)
		{
			Offset (p.X, p.Y);
		}

		public static Point Subtract (Point pt, Size sz)
		{
			return new Point (pt.X - sz.Width, pt.Y - sz.Height);
		}
#endif

        /// <summary>
        /// Get string format of this point
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("({0},{1})", this.X, this.Y);
        }

    }
}
