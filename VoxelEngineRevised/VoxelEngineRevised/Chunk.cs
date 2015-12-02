using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelEngine
{
    class Chunk
    {
        #region Fields
        private int _numBlocks;
        private byte[, ,] _data = null;
        private byte _flags = (byte)CF.None;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Matrix translation;
        private Int3 _position;
        #endregion
        #region Constructors

        public Chunk(Int3 pos)
        {
            Initialize();
            _position = pos;
            translation = Matrix.CreateTranslation(Position * Control.CHUNK_SIZE);
        }
        public Chunk(byte fill)
        {
            Initialize();
            _position = Int3.Zero;
            for (int xi = 0; xi < Control.CHUNK_SIZE; xi++)
                for (int yi = 0; yi < Control.CHUNK_SIZE; yi++)
                    for (int zi = 0; zi < Control.CHUNK_SIZE; zi++)
                        _data[xi, yi, zi] = fill;
            this[CF.isLoaded] = true;
            if (fill != 0x00)
            {
                _numBlocks = Control.BLOCKS_IN_CHUNK;
            }
        }

        #endregion
        #region Properties
        /// <summary>
        /// The chunk's position in Chunk-Space
        /// </summary>
        public Int3 Position
        {
            get
            {
                return _position;
            }
        }

        /// <summary>
        /// Returns whether or not the chunk is completely filled.
        /// </summary>
        public bool Solid
        {
            get
            {
                return (_numBlocks == Control.BLOCKS_IN_CHUNK);
            }
        }

        /// <summary>
        /// Returns whether or not the chunk is completely empty.
        /// </summary>
        public bool Empty
        {
            get
            {
                return (_numBlocks == 0);
            }
        }

        public bool this[CF flag]
        {
            get
            {
                return ((_flags & (byte)flag) == (byte)flag);
            }
            set
            {
                if (value)
                    _flags |= (byte)flag;
                else
                    _flags &= (byte)~flag;
            }
        }
        public Block this[int x, int y, int z]
        {
            get
            {
                #region Read Neighbors
                if (x < 0)
                {
                    Chunk c = MapManager.GetChunk(Position + Int3.Left);
                    if (c == null)
                        return (byte)BT.Reserved;
                    if (!c[CF.isLoaded])
                    {
                        c = null;
                        return (byte)BT.Reserved;
                    }
                    byte cData = c[x + Control.CHUNK_SIZE, y, z];
                    c = null;
                    return cData;
                }
                else if (x >= Control.CHUNK_SIZE)
                {
                    Chunk c = MapManager.GetChunk(Position + Int3.Right);
                    if (c == null)
                        return (byte)BT.Reserved;
                    if (!c[CF.isLoaded])
                        return (byte)BT.Reserved;
                    return c[x - Control.CHUNK_SIZE, y, z];
                }
                else if (y < 0)
                {
                    Chunk c = MapManager.GetChunk(Position + Int3.Down);
                    if (c == null)
                        return (byte)BT.Reserved;
                    if (!c[CF.isLoaded])
                        return (byte)BT.Reserved;
                    return c[x, y + Control.CHUNK_SIZE, z];
                }
                else if (y >= Control.CHUNK_SIZE)
                {
                    Chunk c = MapManager.GetChunk(Position + Int3.Up);
                    if (c == null)
                        return (byte)BT.Reserved;
                    if (!c[CF.isLoaded])
                        return (byte)BT.Reserved;
                    return c[x, y - Control.CHUNK_SIZE, z];
                }
                else if (z < 0)
                {
                    Chunk c = MapManager.GetChunk(Position + Int3.Backward);
                    if (c == null)
                        return (byte)BT.Reserved;
                    if (!c[CF.isLoaded])
                        return (byte)BT.Reserved;
                    return c[x, y, z + Control.CHUNK_SIZE];
                }
                else if (z >= Control.CHUNK_SIZE)
                {
                    Chunk c = MapManager.GetChunk(Position + Int3.Forward);
                    if (c == null)
                        return (byte)BT.Reserved;
                    if (!c[CF.isLoaded])
                        return (byte)BT.Reserved;
                    return c[x, y, z - Control.CHUNK_SIZE];
                }
                #endregion
                return _data[x, y, z];
            }
            set
            {
                if (!this[CF.isLoaded])
                {
                    byte cData = _data[x, y, z];
                    if (cData == value)
                        return;
                    RebuildNeighbors();
                    this[CF.Build] = true;
                    this[CF.Save] = true;
                    if (cData == 0x00)
                    {
                        ++_numBlocks;
                    }
                    else if (value == 0x00)
                    {
                        --_numBlocks;
                    }
                }
                _data[x, y, z] = value;
            }
        }
        public byte this[Int3 a]
        {
            get
            {
                return this[a.X, a.Y, a.Z];
            }
            set
            {
                this[a.X, a.Y, a.Z] = value;
            }
        }
        #endregion
        #region Methods

        private void Initialize()
        {
            _flags = (byte)CF.NewChunk;
            _numBlocks = 0;
            _data = new byte[Control.CHUNK_SIZE, Control.CHUNK_SIZE, Control.CHUNK_SIZE];
            if (_vertexBuffer != null)
                _vertexBuffer.Dispose();
            _vertexBuffer = null;
            if (_indexBuffer != null)
                _indexBuffer.Dispose();
            _indexBuffer = null;
        }

        private void RebuildNeighbors()
        {
            Chunk c = MapManager.GetChunk(Position + Int3.Left);
            if (c != null)
                if (c[CF.isLoaded])
                    c[CF.Build] = true;
            c = MapManager.GetChunk(Position + Int3.Right);
            if (c != null)
                if (c[CF.isLoaded])
                    c[CF.Build] = true;
            c = MapManager.GetChunk(Position + Int3.Up);
            if (c != null)
                if (c[CF.isLoaded])
                    c[CF.Build] = true;
            c = MapManager.GetChunk(Position + Int3.Down);
            if (c != null)
                if (c[CF.isLoaded])
                    c[CF.Build] = true;
            c = MapManager.GetChunk(Position + Int3.Forward);
            if (c != null)
                if (c[CF.isLoaded])
                    c[CF.Build] = true;
            c = MapManager.GetChunk(Position + Int3.Backward);
            if (c != null)
                if (c[CF.isLoaded])
                    c[CF.Build] = true;
            c = null;
        }

        private bool CreateMesh(out VertexPositionColorNormal[] verts, out short[] indices)
        {
            List<VertexPositionColorNormal> vertsList = new List<VertexPositionColorNormal>();
            List<short> indicesList = new List<short>();
            #region NonSolid
            if (!Solid)
            {
                for (int x = 0; x < Control.CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < Control.CHUNK_SIZE; y++)
                    {
                        for (int z = 0; z < Control.CHUNK_SIZE; z++)
                        {
                            Block cData = _data[x, y, z];
                            if (cData == 0x00 || cData == (byte)BT.Reserved)
                                continue;
                            Color color = cData.color;
                            #region Left
                            if (this[x - 1, y, z] == 0x00) //Left side visible?
                            {
                                short numVerts = (short)vertsList.Count;
                                Vector3 Normal = Vector3.Left;
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z), Normal, color));//bottom front
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z + 1), Normal, color));//bottom back
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z), Normal, color));//top front
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z + 1), Normal, color));//top back
                                indicesList.Add(numVerts);
                                indicesList.Add((short)(numVerts + 2));
                                indicesList.Add((short)(numVerts + 1));
                                indicesList.Add((short)(numVerts + 1));
                                indicesList.Add((short)(numVerts + 2));
                                indicesList.Add((short)(numVerts + 3));
                            }
                            #endregion
                            #region Right
                            if (this[x + 1, y, z] == 0x00) //Right side visible?
                            {
                                short numVerts = (short)vertsList.Count;
                                Vector3 Normal = Vector3.Right;
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z), Normal, color));//bottom front
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z + 1), Normal, color));//bottom back
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z), Normal, color));//top front
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z + 1), Normal, color));//top back
                                indicesList.Add(numVerts);
                                indicesList.Add((short)(numVerts + 1));
                                indicesList.Add((short)(numVerts + 2));
                                indicesList.Add((short)(numVerts + 2));
                                indicesList.Add((short)(numVerts + 1));
                                indicesList.Add((short)(numVerts + 3));
                            }
                            #endregion
                            #region Bottom
                            if (this[x, y - 1, z] == 0x00) //Bottom side visible?
                            {
                                short numVerts = (short)vertsList.Count;
                                Vector3 Normal = Vector3.Down;
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z), Normal, color)); //left front
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z + 1), Normal, color)); //left back
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z), Normal, color)); //right front
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z + 1), Normal, color)); //right back
                                indicesList.Add(numVerts);
                                indicesList.Add((short)(numVerts + 1));
                                indicesList.Add((short)(numVerts + 2));
                                indicesList.Add((short)(numVerts + 2));
                                indicesList.Add((short)(numVerts + 1));
                                indicesList.Add((short)(numVerts + 3));
                            }
                            #endregion
                            #region Top
                            if (this[x, y + 1, z] == 0x00) //Top side visible?
                            {
                                short numVerts = (short)vertsList.Count;
                                Vector3 Normal = Vector3.Up;
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z), Normal, color)); //left front
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z + 1), Normal, color)); //left back
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z), Normal, color)); //right front
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z + 1), Normal, color)); //right back
                                indicesList.Add(numVerts);
                                indicesList.Add((short)(numVerts + 2));
                                indicesList.Add((short)(numVerts + 1));
                                indicesList.Add((short)(numVerts + 1));
                                indicesList.Add((short)(numVerts + 2));
                                indicesList.Add((short)(numVerts + 3));
                            }
                            #endregion
                            #region Front
                            if (this[x, y, z - 1] == 0x00) //Front side visible?
                            {
                                short numVerts = (short)vertsList.Count;
                                Vector3 Normal = Vector3.Forward;
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z), Normal, color)); //left bottom
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z), Normal, color)); //left top
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z), Normal, color)); //right bottom
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z), Normal, color)); //right top
                                indicesList.Add(numVerts);
                                indicesList.Add((short)(numVerts + 2));
                                indicesList.Add((short)(numVerts + 3));
                                indicesList.Add((short)(numVerts + 3));
                                indicesList.Add((short)(numVerts + 1));
                                indicesList.Add(numVerts);
                            }
                            #endregion
                            #region Back
                            if (this[x, y, z + 1] == 0x00) //Back visible?
                            {
                                short numVerts = (short)vertsList.Count;
                                Vector3 Normal = Vector3.Backward;
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z + 1), Normal, color)); //left bottom
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z + 1), Normal, color)); //left top
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z + 1), Normal, color)); //right bottom
                                vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z + 1), Normal, color)); //right top
                                indicesList.Add(numVerts);
                                indicesList.Add((short)(numVerts + 1));
                                indicesList.Add((short)(numVerts + 2));
                                indicesList.Add((short)(numVerts + 2));
                                indicesList.Add((short)(numVerts + 1));
                                indicesList.Add((short)(numVerts + 3));
                            }
                            #endregion
                        }
                    }
                }
            }
            #endregion
            #region Solid
            else
            {
                #region Check Sides
                bool drawFront = true, drawBack = true, drawLeft = true, drawRight = true, drawBottom = true, drawTop = true;
                Chunk c = MapManager.GetChunk(Position + Int3.Backward);
                if (c != null)
                    if (c[CF.isLoaded] && c.Solid)
                        drawFront = false;
                c = MapManager.GetChunk(Position + Int3.Forward);
                if (c != null)
                    if (c[CF.isLoaded] && c.Solid)
                        drawBack = false;
                c = MapManager.GetChunk(Position + Int3.Left);
                if (c != null)
                    if (c[CF.isLoaded] && c.Solid)
                        drawLeft = false;
                c = MapManager.GetChunk(Position + Int3.Right);
                if (c != null)
                    if (c[CF.isLoaded] && c.Solid)
                        drawRight = false;
                c = MapManager.GetChunk(Position + Int3.Up);
                if (c != null)
                    if (c[CF.isLoaded] && c.Solid)
                        drawTop = false;
                c = MapManager.GetChunk(Position + Int3.Down);
                if (c != null)
                    if (c[CF.isLoaded] && c.Solid)
                        drawBottom = false;
                c = null;
                #endregion
                if (drawBack || drawBottom || drawFront || drawLeft || drawRight || drawTop)
                {
                    for (int index0 = 0; index0 < Control.CHUNK_SIZE; index0++)
                    {
                        for (int index1 = 0; index1 < Control.CHUNK_SIZE; index1++)
                        {
                            int x, y, z;
                            Block cData;
                            Color color;
                            #region Front
                            if (drawFront)
                            {
                                x = index0;
                                y = index1;
                                z = 0;
                                cData = _data[x, y, z];
                                color = cData.color;
                                #region Front Verts
                                if (this[x, y, z - 1] == 0x00) //Front side visible?
                                {
                                    short numVerts = (short)vertsList.Count;
                                    Vector3 Normal = Vector3.Forward;
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z), Normal, color)); //left bottom
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z), Normal, color)); //left top
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z), Normal, color)); //right bottom
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z), Normal, color)); //right top
                                    indicesList.Add(numVerts);
                                    indicesList.Add((short)(numVerts + 2));
                                    indicesList.Add((short)(numVerts + 3));
                                    indicesList.Add((short)(numVerts + 3));
                                    indicesList.Add((short)(numVerts + 1));
                                    indicesList.Add(numVerts);
                                }
                                #endregion
                            }
                            #endregion
                            #region Back
                            if (drawBack)
                            {
                                x = index0;
                                y = index1;
                                z = Control.CHUNK_SIZE - 1;
                                cData = _data[x, y, z];
                                color = cData.color;
                                #region Back Verts
                                if (this[x, y, z + 1] == 0x00) //Back visible?
                                {
                                    short numVerts = (short)vertsList.Count;
                                    Vector3 Normal = Vector3.Backward;
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z + 1), Normal, color)); //left bottom
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z + 1), Normal, color)); //left top
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z + 1), Normal, color)); //right bottom
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z + 1), Normal, color)); //right top
                                    indicesList.Add(numVerts);
                                    indicesList.Add((short)(numVerts + 1));
                                    indicesList.Add((short)(numVerts + 2));
                                    indicesList.Add((short)(numVerts + 2));
                                    indicesList.Add((short)(numVerts + 1));
                                    indicesList.Add((short)(numVerts + 3));
                                }
                                #endregion
                            }
                            #endregion
                            #region Bottom
                            if (drawBottom)
                            {
                                x = index0;
                                y = 0;
                                z = index1;
                                cData = _data[x, y, z];
                                color = cData.color;
                                #region Bottom Verts
                                if (this[x, y - 1, z] == 0x00) //Bottom side visible?
                                {
                                    short numVerts = (short)vertsList.Count;
                                    Vector3 Normal = Vector3.Down;
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z), Normal, color)); //left front
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z + 1), Normal, color)); //left back
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z), Normal, color)); //right front
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z + 1), Normal, color)); //right back
                                    indicesList.Add(numVerts);
                                    indicesList.Add((short)(numVerts + 1));
                                    indicesList.Add((short)(numVerts + 2));
                                    indicesList.Add((short)(numVerts + 2));
                                    indicesList.Add((short)(numVerts + 1));
                                    indicesList.Add((short)(numVerts + 3));
                                }
                                #endregion
                            }
                            #endregion
                            #region Top
                            if (drawTop)
                            {
                                x = index0;
                                y = Control.CHUNK_SIZE - 1;
                                z = index1;
                                cData = _data[x, y, z];
                                color = cData.color;
                                #region Top Verts
                                if (this[x, y + 1, z] == 0x00) //Top side visible?
                                {
                                    short numVerts = (short)vertsList.Count;
                                    Vector3 Normal = Vector3.Up;
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z), Normal, color)); //left front
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z + 1), Normal, color)); //left back
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z), Normal, color)); //right front
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z + 1), Normal, color)); //right back
                                    indicesList.Add(numVerts);
                                    indicesList.Add((short)(numVerts + 2));
                                    indicesList.Add((short)(numVerts + 1));
                                    indicesList.Add((short)(numVerts + 1));
                                    indicesList.Add((short)(numVerts + 2));
                                    indicesList.Add((short)(numVerts + 3));
                                }
                                #endregion
                            }
                            #endregion
                            #region Left
                            if (drawLeft)
                            {
                                x = 0;
                                y = index0;
                                z = index1;
                                cData = _data[x, y, z];
                                color = cData.color;
                                #region Left Verts
                                if (this[x - 1, y, z] == 0x00) //Left side visible?
                                {
                                    short numVerts = (short)vertsList.Count;
                                    Vector3 Normal = Vector3.Left;
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z), Normal, color));//bottom front
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y, z + 1), Normal, color));//bottom back
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z), Normal, color));//top front
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x, y + 1, z + 1), Normal, color));//top back
                                    indicesList.Add(numVerts);
                                    indicesList.Add((short)(numVerts + 2));
                                    indicesList.Add((short)(numVerts + 1));
                                    indicesList.Add((short)(numVerts + 1));
                                    indicesList.Add((short)(numVerts + 2));
                                    indicesList.Add((short)(numVerts + 3));
                                }
                                #endregion
                            }
                            #endregion
                            #region Right
                            if (drawRight)
                            {
                                x = Control.CHUNK_SIZE - 1;
                                y = index0;
                                z = index1;
                                cData = _data[x, y, z];
                                color = cData.color;
                                #region Right Verts
                                if (this[x + 1, y, z] == 0x00) //Right side visible?
                                {
                                    short numVerts = (short)vertsList.Count;
                                    Vector3 Normal = Vector3.Right;
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z), Normal, color));//bottom front
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y, z + 1), Normal, color));//bottom back
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z), Normal, color));//top front
                                    vertsList.Add(new VertexPositionColorNormal(new Vector3(x + 1, y + 1, z + 1), Normal, color));//top back
                                    indicesList.Add(numVerts);
                                    indicesList.Add((short)(numVerts + 1));
                                    indicesList.Add((short)(numVerts + 2));
                                    indicesList.Add((short)(numVerts + 2));
                                    indicesList.Add((short)(numVerts + 1));
                                    indicesList.Add((short)(numVerts + 3));
                                }
                                #endregion
                            }
                            #endregion
                        }
                    }
                }
            }
            #endregion
            if (indicesList.Count == 0)
            {
                verts = null;
                indices = null;
                return false;
            }
            verts = vertsList.ToArray();
            indices = indicesList.ToArray();
            vertsList.Clear();
            indicesList.Clear();
            return true;
        }
        public void Build()
        {
            lock (this)
            {
                this[CF.Build] = false;

                if (!this[CF.isLoaded])
                    throw new Exception("Trying to build unLoaded chunk.");

                if (Empty)
                {
                    this[CF.Mesh] = false;
                    return;
                }
                VertexPositionColorNormal[] verts;
                short[] indices;
                if (CreateMesh(out verts, out indices))
                {
                    if (_vertexBuffer != null)
                        _vertexBuffer.Dispose();
                    _vertexBuffer = null;
                    if (_indexBuffer != null)
                        _indexBuffer.Dispose();
                    _indexBuffer = null;
                    while (_vertexBuffer == null || _indexBuffer == null)
                    {
                        _vertexBuffer = new VertexBuffer(Control.GraphicsDevice, VertexPositionColorNormal.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
                        _indexBuffer = new IndexBuffer(Control.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
                    }
                    _vertexBuffer.SetData(verts);
                    _indexBuffer.SetData(indices);
                    this[CF.Mesh] = true;
                }
                else
                {
                    this[CF.Mesh] = false;
                }
            }
        }

        public void Unload()
        {
            if (this[CF.Save])
            {
                throw new NotImplementedException();
            }
            Initialize();
        }

        public void Draw()
        {
            lock (this)
            {
                if (!this[CF.isLoaded])
                    throw new Exception("Draw call to unLoaded chunk!");
                if (!this[CF.Mesh])
                    return;

                Control.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                Control.GraphicsDevice.Indices = _indexBuffer;
                Control.ColorShader.Parameters["World"].SetValue(translation);
                foreach (EffectPass pass in Control.ColorShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Control.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertexBuffer.VertexCount, 0, _indexBuffer.IndexCount / 3);
                }
                Control.SolidShader.Parameters["World"].SetValue(translation);
                RasterizerState oldR = Control.GraphicsDevice.RasterizerState, newR = new RasterizerState();
                newR.FillMode = FillMode.WireFrame;
                Control.GraphicsDevice.RasterizerState = newR;
                foreach (EffectPass pass in Control.SolidShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Control.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertexBuffer.VertexCount, 0, _indexBuffer.IndexCount / 3);
                }
                Control.GraphicsDevice.RasterizerState = oldR;
            }
        }

        #endregion
    }
}
