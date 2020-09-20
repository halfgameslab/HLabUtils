using UnityEngine;

namespace Mup.Misc.SimpleMath
{
    [System.Serializable]
    public class M_Triangle
    {
        [SerializeField] private Vector2 _a;
        [SerializeField] private Vector2 _b;
        [SerializeField] private Vector2 _c;

        public Vector2 A
        {
            get
            {
                return _a;
            }

            set
            {
                _a = value;
            }
        }

        public Vector2 B
        {
            get
            {
                return _b;
            }

            set
            {
                _b = value;
            }
        }

        public Vector2 C
        {
            get
            {
                return _c;
            }

            set
            {
                _c = value;
            }
        }

        public M_Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public bool Contains(Vector2 pt)
        {
            return Contains(pt, this);
        }

        public static Vector2 RandomPoint(M_Triangle tri)
        {
            Vector2 p1 = Vector2.Lerp(tri.A, tri.B, Random.value);
            Vector2 p2 = Vector2.Lerp(tri.B, tri.C, Random.value);
            Vector2 p3 = Vector2.Lerp(tri.C, tri.A, Random.value);

            return (p1 + p2 + p3) / 3f;
        }

        public static bool Contains(Vector2 pt, M_Triangle tri)
        {
            bool b1, b2, b3;

            b1 = Sign(pt, tri.A, tri.B) < 0.0f;
            b2 = Sign(pt, tri.B, tri.C) < 0.0f;
            b3 = Sign(pt, tri.C, tri.A) < 0.0f;

            return ((b1 == b2) && (b2 == b3));
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }
    }
}