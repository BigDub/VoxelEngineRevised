using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VoxelEngine
{
    class Block
    {
        #region Fields

        private byte _type;

        #endregion
        #region Constructors

        public Block(byte type)
        {
            _type = type;
        }

        #endregion
        #region Properties

        public byte Type
        {
            get
            {
                return _type;
            }
        }
        public Color color
        {
            get
            {
                switch ((BT)_type)
                {
                    case (BT.Stone):
                        return Color.LightGray;
                    case (BT.Dirt):
                        return Color.Brown;
                    case (BT.Grass):
                        return Color.Green;
                    default:
                        return Color.Pink;
                }
            }
        }
        public bool collide
        {
            get
            {
                switch ((BT)_type)
                {
                    case (BT.Empty):
                    case (BT.Reserved):
                        return false;
                    default:
                        return true;
                }
            }
        }

        #endregion
        #region Methods

        public static implicit operator byte(Block a)
        {
            return a._type;
        }
        public static implicit operator Block(byte a)
        {
            return new Block(a);
        }

        #endregion
    }
}
