using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VoxelEngine
{
    class RegionReader
    {
        #region Fields

        private string _file;
        private FileStream _filestream;
        private Stack<CPT> _stack;
        private Chunk _chunk;

        #endregion
        #region Constructors

        public RegionReader(Int3 Region)
        {
            _file = Region.X + '.' + Region.Y + '.' + Region.Z + ".rg";
        }

        #endregion
        #region Methods

        public void LoadChunk(Chunk c)
        {
            _chunk = c;
            if (File.Exists(_file))
                throw new FileNotFoundException();
            _filestream = new FileStream(_file, FileMode.Open, FileAccess.Read, FileShare.Read);
            _filestream.Position = 1 + ((_chunk.Position.X * Control.REGION_SIZE * Control.REGION_SIZE) + (_chunk.Position.Y * Control.REGION_SIZE) + _chunk.Position.Z) * (sizeof(long) + 1);
            byte data;
            byte[] buffer = new byte[sizeof(long)];
            data = (byte)_filestream.ReadByte();
            switch ((CET)data)
            {
                case (CET.Null):
                    throw new NullReferenceException();
                case (CET.Solid):
                    data = (byte)_filestream.ReadByte();
                    for (int x = 0; x < Control.CHUNK_SIZE; x++)
                    {
                        for (int y = 0; y < Control.CHUNK_SIZE; y++)
                        {
                            for (int z = 0; z < Control.CHUNK_SIZE; z++)
                            {
                                _chunk[x, y, z] = data;
                            }
                        }
                    }
                    _chunk[CF.isLoaded] = true;
                    break;
                case (CET.Array):
                    _filestream.Read(buffer, 0, sizeof(long));
                    _filestream.Position = BitConverter.ToInt64(buffer, 0);
                    for (int x = 0; x < Control.CHUNK_SIZE; x++)
                    {
                        for (int y = 0; y < Control.CHUNK_SIZE; y++)
                        {
                            for (int z = 0; z < Control.CHUNK_SIZE; z++)
                            {
                                _chunk[x, y, z] = (byte)_filestream.ReadByte();
                            }
                        }
                    }
                    _chunk[CF.isLoaded] = true;
                    break;
                case (CET.Subdivided):
                    LoadSubdivided();
                    _chunk[CF.isLoaded] = true;
                    break;
            }
            _filestream.Close();
            _filestream.Dispose();
        }
        private void LoadSubdivided()
        {
            _stack = new Stack<CPT>();
            _stack.Push(CPT.subdiv);
            LoadSD(Int3.Zero, Control.CHUNK_SIZE);
        }
        private void LoadSD(Int3 min, int size)
        {
            byte data = (byte)_filestream.ReadByte();
            switch (_stack.Pop())
            {
                case (CPT.subdiv):
                    if ((data & (byte)CSF.RTB) == (byte)CSF.RTB)
                        _stack.Push(CPT.subdiv);
                    else
                        _stack.Push(CPT.data);
                    if ((data & (byte)CSF.RTF) == (byte)CSF.RTF)
                        _stack.Push(CPT.subdiv);
                    else
                        _stack.Push(CPT.data);
                    if ((data & (byte)CSF.RBB) == (byte)CSF.RBB)
                        _stack.Push(CPT.subdiv);
                    else
                        _stack.Push(CPT.data);
                    if ((data & (byte)CSF.RBF) == (byte)CSF.RBF)
                        _stack.Push(CPT.subdiv);
                    else
                        _stack.Push(CPT.data);
                    if ((data & (byte)CSF.LTB) == (byte)CSF.LTB)
                        _stack.Push(CPT.subdiv);
                    else
                        _stack.Push(CPT.data);
                    if ((data & (byte)CSF.LTF) == (byte)CSF.LTF)
                        _stack.Push(CPT.subdiv);
                    else
                        _stack.Push(CPT.data);
                    if ((data & (byte)CSF.LBB) == (byte)CSF.LBB)
                        _stack.Push(CPT.subdiv);
                    else
                        _stack.Push(CPT.data);
                    if ((data & (byte)CSF.LBF) == (byte)CSF.LBF)
                        _stack.Push(CPT.subdiv);
                    else
                        _stack.Push(CPT.data);
                    int nSize = size / 2;
                    Int3 mid = min + Int3.One * nSize;
                    LoadSD(min, nSize);
                    LoadSD(new Int3(min.X, min.Y, mid.Z), nSize);
                    LoadSD(new Int3(min.X, mid.Y, min.Z), nSize);
                    LoadSD(new Int3(min.X, mid.Y, mid.Z), nSize);
                    LoadSD(new Int3(mid.X, min.Y, min.Z), nSize);
                    LoadSD(new Int3(mid.X, min.Y, mid.Z), nSize);
                    LoadSD(new Int3(mid.X, mid.Y, min.Z), nSize);
                    LoadSD(mid, nSize);
                    break;
                case (CPT.data):
                    for (int x = min.X; x < min.X + size; x++)
                    {
                        for (int y = min.Y; x < min.Y + size; y++)
                        {
                            for (int z = min.Z; z < min.Z + size; z++)
                            {
                                _chunk[x, y, z] = data;
                            }
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}
