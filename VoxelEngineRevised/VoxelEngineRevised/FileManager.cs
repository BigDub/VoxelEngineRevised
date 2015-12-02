using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VoxelEngine
{
    static class FileManager
    {
        #region Constants

        private const long CHUNK_HEADER_LENGTH = sizeof(long) + 1;

        #endregion
        #region Methods

        public static void GenerateRegion()
        {
            Random random = new Random();
            ChunkWriter cw = new ChunkWriter();
            long pos = CHUNK_HEADER_LENGTH * Control.REGION_SIZE * Control.REGION_SIZE * Control.REGION_SIZE + sizeof(long);
            FileStream fs;
            fs = File.Create("0.0.0.rg");
            fs.Position = pos;
            for (int RegionX = 0; RegionX < Control.REGION_SIZE; RegionX++)
            {
                for (int RegionY = 0; RegionY < Control.REGION_SIZE; RegionY++)
                {
                    for (int RegionZ = 0; RegionZ < Control.REGION_SIZE; RegionZ++)
                    {
                        Chunk c;
                        if (RegionY < 4)
                        {
                            c = new Chunk((byte)BT.Stone);
                        }
                        else if (RegionY < 8)
                        {
                            c = new Chunk((byte)BT.Dirt);
                        }
                        else if (RegionY == 8)
                        {
                            int height = random.Next() % (Control.CHUNK_SIZE / 2);
                            c = new Chunk((byte)BT.Empty);
                            for (int x = 0; x < Control.CHUNK_SIZE; x++)
                            {
                                for (int y = 0; y < Control.CHUNK_SIZE; y++)
                                {
                                    for (int z = 0; z < Control.CHUNK_SIZE; z++)
                                    {
                                        if (y < height)
                                            c[x, y, z] = (byte)BT.Dirt;
                                        else if (y == height)
                                            c[x, y, z] = (byte)BT.Grass;
                                    }
                                }
                            }
                        }
                        else
                        {
                            c = new Chunk((byte)BT.Empty);
                        }
                        fs.Position = CHUNK_HEADER_LENGTH * 
                            (RegionX * Control.REGION_SIZE * Control.REGION_SIZE + 
                            RegionY * Control.REGION_SIZE + 
                            RegionZ);
                        cw.WriteChunk(fs, c, pos);
                        pos = fs.Position;
                    }
                }
            }
            pos = fs.Position;
            fs.Position = 0;
            fs.Write(BitConverter.GetBytes(pos), 0, sizeof(long));
            fs.Close();
            fs.Dispose();
        }

        #endregion
    }
}
