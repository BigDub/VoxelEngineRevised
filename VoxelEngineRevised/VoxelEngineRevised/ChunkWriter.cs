using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VoxelEngine
{
    class ChunkWriter
    {
        #region Fields

        private FileStream _filestream;
        private Chunk _chunk;
        private long _pos, _subDivPos, _header;

        #endregion
        #region Constructors

        public ChunkWriter() { }

        #endregion
        #region Methods

        public void WriteChunk(FileStream fs, Chunk c, long pos)
        {
            _header = fs.Position;
            _pos = pos;
            _filestream = fs;
            _chunk = c;

            if (!WriteNullChunk())
            {
                if (!WriteChunkSolid())
                {
                    if (!WriteChunkSubdiv())
                    {
                        WriteChunkArray();
                    }
                }
            }
        }

        private bool WriteNullChunk()
        {
            if (_chunk != null)
                return false;

            _filestream.WriteByte((byte)CET.Null);
            _filestream.Position = _pos;
            return true;
        }

        /// <summary>
        /// Attempts to write a Solid Chunk. If it finds the chunk is not solid it returns false.
        /// </summary>
        /// <returns></returns>
        private bool WriteChunkSolid()
        {
            byte cData = _chunk[0, 0, 0].Type;
            for (int x = 0; x < Control.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < Control.CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < Control.CHUNK_SIZE; z++)
                    {
                        if (_chunk[x, y, z].Type != cData)
                            return false;
                    }
                }
            }
            _filestream.WriteByte((byte)CET.Solid);
            _filestream.WriteByte(cData);
            _filestream.Position = _pos;
            return true;
        }

        private int WriteSubdiv(Int3 min, int size)
        {
            bool Subdivide = false;
            byte cData = _chunk[min];
            if (size > 1)
            {
                for (int x = min.X; x < min.X + size; x++)
                {
                    for (int y = min.Y; y < min.Y + size; y++)
                    {
                        for (int z = min.Z; z < min.Z + size; z++)
                        {
                            if (cData != _chunk[x, y, z])
                            {
                                Subdivide = true;
                                break;
                            }
                        }
                        if (Subdivide)
                            break;
                    }
                    if (Subdivide)
                        break;
                }
                if (Subdivide)
                {
                    long headerPosition = _filestream.Position;
                    byte header = (byte)CSF.None;
                    _filestream.Position += 1;
                    int nSize = size / 2;
                    Int3 mid = min + Int3.One * nSize;
                    int retValue;
                    #region LBF
                    retValue = WriteSubdiv(min, nSize);
                    if (retValue == -1)
                        return -1;
                    else if (retValue == 1)
                        header |= (byte)CSF.LBF;
                    #endregion
                    #region LBB
                    retValue = WriteSubdiv(new Int3(min.X, min.Y, mid.Z), nSize);
                    if (retValue == -1)
                        return -1;
                    else if (retValue == 1)
                        header |= (byte)CSF.LBB;
                    #endregion
                    #region LTF
                    retValue = WriteSubdiv(new Int3(min.X, mid.Y, min.Z), nSize);
                    if (retValue == -1)
                        return -1;
                    else if (retValue == 1)
                        header |= (byte)CSF.LTF;
                    #endregion
                    #region LTB
                    retValue = WriteSubdiv(new Int3(min.X, mid.Y, mid.Z), nSize);
                    if (retValue == -1)
                        return -1;
                    else if (retValue == 1)
                        header |= (byte)CSF.LTB;
                    #endregion
                    #region RBF
                    retValue = WriteSubdiv(new Int3(mid.X, min.Y, min.Z), nSize);
                    if (retValue == -1)
                        return -1;
                    else if (retValue == 1)
                        header |= (byte)CSF.RBF;
                    #endregion
                    #region RBB
                    retValue = WriteSubdiv(new Int3(mid.X, min.Y, mid.Z), nSize);
                    if (retValue == -1)
                        return -1;
                    else if (retValue == 1)
                        header |= (byte)CSF.RBB;
                    #endregion
                    #region RTF
                    retValue = WriteSubdiv(new Int3(mid.X, mid.Y, min.Z), nSize);
                    if (retValue == -1)
                        return -1;
                    else if (retValue == 1)
                        header |= (byte)CSF.RTF;
                    #endregion
                    #region RTB
                    retValue = WriteSubdiv(mid, nSize);
                    if (retValue == -1)
                        return -1;
                    else if (retValue == 1)
                        header |= (byte)CSF.RTB;
                    #endregion
                    long endPos = _filestream.Position;
                    _filestream.Position = headerPosition;
                    _filestream.WriteByte(header);
                    _filestream.Position = endPos;
                }
                else
                {
                    _filestream.WriteByte(cData);
                }
            }
            else
            {
                _filestream.WriteByte(cData);
            }

            //If the file is larger because of this encoding, return false.
            if (_filestream.Position - _pos > Control.BLOCKS_IN_CHUNK)
                return -1;

            if (Subdivide)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Attempts to write a Subdivided Chunk. If, while writing, it finds it would take less space to simply write out each block, it returns false.
        /// </summary>
        /// <returns></returns>
        private bool WriteChunkSubdiv()
        {
            _filestream.WriteByte((byte)CET.Subdivided);
            _filestream.Write(BitConverter.GetBytes(_pos), 0, sizeof(long));
            _filestream.Position = _pos;

            if (WriteSubdiv(Int3.Zero, Control.CHUNK_SIZE) == -1)
            {
                _filestream.Position = _header;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Writes out each block of a chunk.
        /// </summary>
        private void WriteChunkArray()
        {
            _filestream.WriteByte((byte)CET.Array);
            _filestream.Write(BitConverter.GetBytes(_pos), 0, sizeof(long));
            _filestream.Position = _pos;
            for (int x = 0; x < Control.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < Control.CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < Control.CHUNK_SIZE; z++)
                    {
                        _filestream.WriteByte(_chunk[x, y, z].Type);
                    }
                }
            }
        }

        #endregion
    }
}
