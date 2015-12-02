using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace VoxelEngine
{
    static class Control
    {
        #region Constants

        public const byte CHUNK_SIZE = 16;
        public const int BLOCKS_IN_CHUNK = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
        public const int REGION_SIZE = 16;

        #endregion
        #region Properties

        public static Matrix Projection { get; set; }
        public static Matrix View { get; set; }
        public static BoundingFrustum Frustum { get; set; }
        public static GraphicsDevice GraphicsDevice { get; private set; }
        public static ContentManager Content { get; private set; }
        public static Effect ColorShader { get; private set; }
        public static Effect NormalShader { get; private set; }
        public static Effect SolidShader { get; private set; }

        #endregion
        #region Methods

        public static void Init(Game1 game)
        {
            GraphicsDevice = game.GraphicsDevice;
            Content = game.Content;
            ColorShader = Content.Load<Effect>("ColorShader");
            NormalShader = Content.Load<Effect>("NormalShader");
            SolidShader = Content.Load<Effect>("SolidShader");
        }
        public static void SetFog(float fogStart, Color fogColor)
        {
            NormalShader.Parameters["FogColor"].SetValue(fogColor.ToVector4());
            NormalShader.Parameters["FogDistance"].SetValue(new Vector2(fogStart, Game1.FAR_CLIPPING - fogStart));
            ColorShader.Parameters["FogColor"].SetValue(fogColor.ToVector4());
            ColorShader.Parameters["FogDistance"].SetValue(new Vector2(fogStart, Game1.FAR_CLIPPING - fogStart));
        }
        public static void PrepEffects(Vector3 CameraPosition)
        {
            SolidShader.Parameters["View"].SetValue(View);
            NormalShader.Parameters["View"].SetValue(View);
            ColorShader.Parameters["View"].SetValue(View);
            NormalShader.Parameters["CameraPosition"].SetValue(CameraPosition);
            ColorShader.Parameters["CameraPosition"].SetValue(CameraPosition);
        }

        public static String TrimFloat(float flTrim, int dec)
        {
            String toTrim = "";
            if (flTrim >= 0)
            {
                toTrim = "+";
            }
            toTrim += flTrim.ToString();
            if (toTrim.Length > dec + 2)
            {
                toTrim = toTrim.Substring(0, dec + 2);
            }
            else if (toTrim.Length == dec + 2)
                return toTrim;
            else
            {
                if ((float)Convert.ToInt32(flTrim) == flTrim)
                {
                    toTrim += ".";
                }
                while (toTrim.Length < dec + 2)
                {
                    toTrim = toTrim + "0";
                }
            }
            return (toTrim);
        }
        public static String Format(float f, int len)
        {
            String temp;
            if (f >= 0)
                temp = '+' + f.ToString();
            else
                temp = f.ToString();

            if (temp.Length == len)
                return temp;
            if (temp.Length < len)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(temp);
                if (sb.Length < len && (float)(int)f == f)
                    sb.Append('.');
                while (sb.Length < len)
                    sb.Append('0');
                return sb.ToString();
            }
            return temp.Substring(0, len);
        }
        public static String Format(Vector3 vec)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{X:");
            sb.Append(TrimFloat(vec.X, 6));
            sb.Append(" Y:");
            sb.Append(TrimFloat(vec.Y, 6));
            sb.Append(" Z:");
            sb.Append(TrimFloat(vec.Z, 6));
            sb.Append('}');
            return sb.ToString();
        }
        public static String Format(Vector2 vec)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{X:");
            sb.Append(TrimFloat(vec.X, 6));
            sb.Append(" Y:");
            sb.Append(TrimFloat(vec.Y, 6));
            sb.Append('}');
            return sb.ToString();
        }

        #endregion
    }
}

