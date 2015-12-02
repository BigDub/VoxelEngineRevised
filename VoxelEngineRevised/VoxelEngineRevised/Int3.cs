using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VoxelEngine
{
    struct Int3
    {
        #region Fields

        private int _x;
        private int _y;
        private int _z;

        #endregion
        #region Constructors

        public Int3(int x)
        {
            _x = x;
            _y = x;
            _z = x;
        }
        public Int3(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        #endregion
        #region Properties

        public static Int3 UnitX { get { return new Int3(1, 0, 0); } }
        public static Int3 UnitY { get { return new Int3(0, 1, 0); } }
        public static Int3 UnitZ { get { return new Int3(0, 0, 1); } }
        public static Int3 Left { get { return -UnitX; } }
        public static Int3 Right { get { return UnitX; } }
        public static Int3 Up { get { return UnitY; } }
        public static Int3 Down { get { return -UnitY; } }
        public static Int3 Forward { get { return UnitZ; } }
        public static Int3 Backward { get { return -UnitZ; } }
        public static Int3 One { get { return new Int3(1); } }
        public static Int3 Zero { get { return new Int3(0); } }
        public int X { get { return _x; } set { _x = value; } }
        public int Y { get { return _y; } set { _y = value; } }
        public int Z { get { return _z; } set { _z = value; } }
        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case (0):
                        return _x;
                    case (1):
                        return _y;
                    case (2):
                        return _z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case (0):
                        _x = value;
                        break;
                    case (1):
                        _y = value;
                        break;
                    case (2):
                        _z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        #endregion
        #region Methods

        public static Int3 operator -(Int3 a)
        {
            return new Int3(-a._x, -a._y, -a._z);
        }
        public static Int3 operator -(Int3 a, Int3 b)
        {
            return new Int3(a._x - b._x, a._y - b._y, a._z - b._z);
        }
        public static Int3 operator +(Int3 a)
        {
            return new Int3(a._x, a._y, a._z);
        }
        public static Int3 operator +(Int3 a, Int3 b)
        {
            return new Int3(a._x + b._x, a._y + b._y, a._z + b._z);
        }
        public static Int3 operator *(Int3 a, int b)
        {
            return new Int3(a._x * b, a._y * b, a._z * b);
        }
        public static Int3 operator *(int a, Int3 b)
        {
            return b * a;
        }
        public static Int3 operator /(Int3 a, int b)
        {
            return new Int3(a._x / b, a._y / b, a._z / b);
        }
        public static bool operator ==(Int3 a, Int3 b)
        {
            return (a._x == b._x && a._y == b._y && a._z == b._z);
        }
        public static bool operator !=(Int3 a, Int3 b)
        {
            return !(a == b);
        }
        public static Int3 operator %(Int3 a, int b)
        {
            return new Int3(a._x % b, a._y % b, a._z % b);
        }

        public static implicit operator Vector3(Int3 a)
        {
            return new Vector3(a._x, a._y, a._z);
        }
        public static explicit operator Int3(Vector3 a)
        {
            return new Int3((int)a.X, (int)a.Y, (int)a.Z);
        }

        public static int Square(int x, int y, int z)
        {
            return x * x + y * y + z * z;
        }
        public int Square()
        {
            return _x * _x + _y * _y + _z * _z;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
                return (Int3)obj == this;
            return base.Equals(obj);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(13);
            sb.Append("{_x:");
            sb.Append(_x);
            sb.Append(" _y:");
            sb.Append(_y);
            sb.Append(" _z:");
            sb.Append(_z);
            sb.Append('}');
            return sb.ToString();
        }

        #endregion
    }
}
