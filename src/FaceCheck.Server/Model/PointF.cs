using System;

namespace FaceCheck.Server.Model
{
    /// <summary>
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <exception cref="NullReferenceException"></exception>
    public struct PointF(float x, float y) : IEquatable<PointF>
    {
        /// <summary>
        /// </summary>
        public static readonly PointF Empty;

        /// <summary>
        /// </summary>
        public float X { set; get; } = x;

        /// <summary>
        /// </summary>
        public float Y { set; get; } = y;

        /// <summary>
        /// </summary>
        public readonly bool IsEmpty
        {
            get
            {
                return Math.Abs(X - 0.0d) < double.Epsilon
                    && Math.Abs(Y - 0.0d) < double.Epsilon;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(PointF other)
        {
            return Math.Abs(other.X - X) < double.Epsilon
                && Math.Abs(other.Y - Y) < double.Epsilon;
        }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is not PointF)
            {
                return false;
            }
            return Equals((PointF)obj);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(PointF left, PointF right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(PointF left, PointF right)
        {
            return !left.Equals(right);
        }
    }
}