using UnityEngine;

namespace Mup.Misc.SimpleMath
{
    [System.Serializable]
    public class M_Quad
    {

        [SerializeField] private Vector2 _a;
        [SerializeField] private Vector2 _b;
        [SerializeField] private Vector2 _c;
        [SerializeField] private Vector2 _d;

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

        public Vector2 D
        {
            get
            {
                return _d;
            }

            set
            {
                _d = value;
            }
        }

        public M_Triangle T1
        {
            get
            {
                return new M_Triangle(A, B, C);
            }
        }

        public M_Triangle T2
        {
            get
            {
                return new M_Triangle(B, C, D);
            }
        }

        public M_Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public bool Contains(Vector2 pt)
        {
            return Contains(pt, this);
        }

        public static bool Contains(Vector2 pt, M_Quad quad)
        {
            return M_Triangle.Contains(pt, quad.T1) || M_Triangle.Contains(pt, quad.T2);
        }

        public static Vector2 RandomPoint(M_Quad quad)
        {
            if (Random.Range(0, 2) == 0) return M_Triangle.RandomPoint(new M_Triangle(quad.A, quad.B, quad.C));

            return M_Triangle.RandomPoint(new M_Triangle(quad.B, quad.C, quad.D));
        }
    }
}