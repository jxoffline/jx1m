using System;

namespace FS.Drawing
{
    /// <summary>
    /// Point with float position
    /// </summary>
    [Serializable]
    public struct PointF
    {
        // Private x and y coordinate fields.
        private float x, y;

        /// <summary>
        ///	Empty Shared Field
        /// </summary>
        /// <remarks>
        ///	An uninitialized PointF Structure.
        /// </remarks>

        public static readonly PointF Empty;

        /// <summary>
        ///	Equality Operator
        /// </summary>
        /// <remarks>
        ///	Compares two PointF objects. The return value is
        ///	based on the equivalence of the X and Y properties 
        ///	of the two points.
        /// </remarks>

        public static bool operator ==(PointF left, PointF right)
        {
            return ((left.X == right.X) && (left.Y == right.Y));
        }

        /// <summary>
        ///	Inequality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two PointF objects. The return value is
        ///	based on the equivalence of the X and Y properties 
        ///	of the two points.
        /// </remarks>

        public static bool operator !=(PointF left, PointF right)
        {
            return ((left.X != right.X) || (left.Y != right.Y));
        }


        /// <summary>
        ///	PointF Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a PointF from a specified x,y coordinate pair.
        /// </remarks>

        public PointF(float x, float y)
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
                return ((x == 0.0) && (y == 0.0));
            }
        }

        /// <summary>
        ///	X Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the PointF.
        /// </remarks>

        public float X
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
        ///	The Y coordinate of the PointF.
        /// </remarks>

        public float Y
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
        ///	Checks equivalence of this PointF and another object.
        /// </remarks>

        public override bool Equals(object obj)
        {
            if (!(obj is PointF))
                return false;

            return (this == (PointF)obj);
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
            return (int)x ^ (int)y;
        }

#if NET_2_0
		public static PointF Add (PointF pt, Size sz)
		{
			return new PointF (pt.X + sz.Width, pt.Y + sz.Height);
		}

		public static PointF Add (PointF pt, SizeF sz)
		{
			return new PointF (pt.X + sz.Width, pt.Y + sz.Height);
		}

		public static PointF Subtract (PointF pt, Size sz)
		{
			return new PointF (pt.X - sz.Width, pt.Y - sz.Height);
		}

		public static PointF Subtract (PointF pt, SizeF sz)
		{
			return new PointF (pt.X - sz.Width, pt.Y - sz.Height);
		}
#endif

    }
}
