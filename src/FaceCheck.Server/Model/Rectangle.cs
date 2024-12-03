using System;

namespace FaceCheck.Server.Model
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public struct Rectangle(int x, int y, int width, int height) : IEquatable<Rectangle>
    {
        /// <summary>
        /// </summary>
        public static readonly Rectangle Empty;

        /// <summary>
        /// </summary>
        public int X { set; get; } = x;

        /// <summary>
        /// </summary>
        public int Y { set; get; } = y;

        /// <summary>
        /// </summary>
        public int Width { set; get; } = width;

        /// <summary>
        /// </summary>
        public int Height { set; get; } = height;

        /// <summary>
        /// </summary>
        public readonly int Left => X;

        /// <summary>
        /// </summary>
        public readonly int Top => Y;

        /// <summary>
        /// </summary>
        public readonly int Right => X + Width;

        /// <summary>
        /// </summary>
        public readonly int Bottom => Y + Height;

        /// <summary>
        /// </summary>
        public readonly bool IsEmpty
        {
            get
            {
                return Height == 0
                    && Width == 0
                    && X == 0
                    && Y == 0;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public static Rectangle FromLTRB(int left, int top, int right, int bottom)
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static bool operator ==(Rectangle left, Rectangle right)
        {
            if (left.X == right.X && left.Y == right.Y && left.Width == right.Width)
            {
                return left.Height == right.Height;
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !(left == right);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{X=" + X + ",Y=" + Y + ",Width=" + Width + ",Height=" + Height + "}";
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is not Rectangle)
            {
                return false;
            }

            return Equals((Rectangle)obj);
        }

        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Rectangle other)
        {
            return other.X == X
                && other.Y == Y
                && other.Width == Width
                && other.Height == Height;
        }
    }
}