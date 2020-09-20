// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MathfExtension.cs" company="Dauler Palhares">
//  © Copyright Dauler Palhares da Costa Viana 2017.
//          http://github.com/DaulerPalhares
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;

namespace Ultility
{
    public static class MathfExtension
    {
        /// <summary>
        /// Check if given number is btween 2 values.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        /// <param name="inclusive">Max Inclusive or exclusive</param>
        /// <returns>Return if is in range</returns>
        public static bool Between(int value, int min, int max, bool inclusive)
        {
            if (inclusive)
            {
                return value >= min && value <= max;
            }
            else
            {
                return value > min && value < max;
            }
        }
        /// <summary>
        /// Clamp a vector 3 value inside a range of 2 position
        /// </summary>
        /// <param name="value"> Value to clamp</param>
        /// <param name="min">Min value</param>
        /// <param name="max">Max Value</param>
        /// <returns>Return a vector 3 inside the 2 param</returns>
        public static Vector3 ClampVector3(Vector3 value, Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y), Mathf.Clamp(value.z, min.z, max.z));
        }

        public static Vector3 RandomVector3(Vector3 min, Vector3 max)
        {
            Vector3 rnd = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
            return rnd;
        }
        /// <summary>
        /// Divide 2 vector3 first/second
        /// </summary>
        /// <param name="first">First param</param>
        /// <param name="second">Second param</param>
        /// <returns>Divided vector3</returns>
        public static Vector3 DivVector3(Vector3 first, Vector3 second)
        {
            return new Vector3((first.x / second.x), (first.y / second.y), (first.z / second.z));
        }
        
        public static Vector3 MultiplyByNumber(Vector3 value, float number)
        {
            return new Vector3(value.x * number, value.y * number, value.z * number);
        }
        /// <summary>
        /// Make a curve from poina A to C and B is the "influence point".
        /// </summary>
        /// <param name="a">Start</param>
        /// <param name="b">influence</param>
        /// <param name="c">End</param>
        /// <param name="t">0-1</param>
        /// <returns></returns>
        public static Vector3 QuadraticCurve(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            Vector3 p0 = Vector3.Lerp(a, b, t);
            Vector3 p1 = Vector3.Lerp(b, c, t);

            return Vector3.Lerp(p0, p1, t);
        }
        /// <summary>
        /// Make a Bezier curve from point A to D.
        /// </summary>
        /// <param name="a">Start</param>
        /// <param name="b">First Influence</param>
        /// <param name="c">Second Influence</param>
        /// <param name="d">End</param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 CubicCurve(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            Vector3 p0 = QuadraticCurve(a, b, c, t);
            Vector3 p1 = QuadraticCurve(b, c, d, t);
            return Vector3.Lerp(p0, p1, t);
        }
        /// <summary>
        /// Get a closest point between 2 points in straight line.
        /// </summary>
        /// <param name="PointA">Point A</param>
        /// <param name="PointB">Point B</param>
        /// <param name="Position">Position to check</param>
        /// <returns>Return a world space btween A and B and closest to Position</returns>
        public static Vector3 GetClosestPointBetween(Vector3 PointA, Vector3 PointB,Vector3 Position)
        {
            float pointx = 0;
            float pointy = 0;
            float pointz = 0;
            if (PointA.x != PointB.x)
            {
                pointx = (((Position.x - PointA.x) * (PointB.x - PointA.x)) / Mathf.Pow(Mathf.Abs(PointB.x - PointA.x), 2)) * (PointB.x - PointA.x);
            }
            if(PointA.y != PointB.y)
            {
                pointy = (((Position.y - PointA.y) * (PointB.y - PointA.y)) / Mathf.Pow(Mathf.Abs(PointB.y - PointA.y), 2)) * (PointB.y - PointA.y);
            }
            if (PointA.z != PointB.z)
            {
                pointz = (((Position.z - PointA.z) * (PointB.z - PointA.z)) / Mathf.Pow(Mathf.Abs(PointB.z - PointA.z), 2)) * (PointB.z - PointA.z);
            }
            return PointA + new Vector3(pointx, pointy, pointz);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="total"></param>
        /// <param name="percent"></param>
        /// <returns>Return value btween 0-1</returns>
        public static float GetPercent(float total, float percent)
        {
            return (percent / 100) * total;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="total"></param>
        /// <param name="percent"></param>
        /// <returns>Return value btween 0-100</returns>
        public static float PercentValue(float total, float percent)
        {
            return (percent / total) * 100;
        }
    }
}