using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace VoxelEngine
{
    static class MapManager
    {
        #region Constants

        private const int VISIBILITY_RADIUS = 10,
            VISIBILITY_RADIUS_SQUARED = VISIBILITY_RADIUS * VISIBILITY_RADIUS,
            UNLOAD_RADIUS = 15,
            UNLOAD_RADIUS_SQUARED = UNLOAD_RADIUS * UNLOAD_RADIUS,
            MAX_ASYNC_LOAD = 12;

        #endregion
        #region Fields

        private static Int3 _origin = new Int3(0);
        private static int _AsyncLoaders = 0, _loadcount, _rebuildcount, _unloadcount;
        private static List<Chunk> _AllChunks, _Load, _Unload, _Render, _Rebuild;

        #endregion
        #region Properties

        public static List<Chunk> AllChunks { get { return _AllChunks; } }
        public static int CountAll
        {
            get
            {
                return _AllChunks.Count;
            }
        }
        public static int CountLoad
        {
            get
            {
                return _loadcount;
            }
        }
        public static int CountRebuild
        {
            get
            {
                return _rebuildcount;
            }
        }
        public static int CountUnload
        {
            get
            {
                return _unloadcount;
            }
        }
        public static int CountRender
        {
            get
            {
                return _Render.Count;
            }
        }

        #endregion
        #region Methods

        public static void Init(String MapDirectory)
        {
            _AllChunks = new List<Chunk>();
            _Load = new List<Chunk>();
            _Unload = new List<Chunk>();
            _Render = new List<Chunk>();
            _Rebuild = new List<Chunk>();
        }
        public static void SetBlock(int x, int y, int z, byte data)
        {
            throw new NotImplementedException();
        }
        public static void SetBlock(Int3 block, byte data)
        {
            Chunk c = GetChunk(WorldToChunk(block));
            if (c == null)
                return;
            c[WorldToBlock(block)] = data;
        }
        public static Block GetBlock(int x, int y, int z)
        {
            return GetBlock(new Int3(x, y, z));
        }
        public static Block GetBlock(Int3 block)
        {
            Chunk c = GetChunk(WorldToChunk(block));
            if (c == null)
                return null;
            if (!c[CF.isLoaded])
                return null;
            return (c[WorldToBlock(block)]);
        }
        public static Chunk GetChunk(int x, int y, int z)
        {
            return GetChunk(new Int3(x, y, z));
        }
        public static Chunk GetChunk(Int3 chunk)
        {
            //TODO: Optimize GetChunk
            foreach (Chunk c in _AllChunks)
            {
                if (c.Position == chunk)
                    return c;
            }
            return null;
        }

        /// <summary>
        /// Converts from world-space to block-space.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public static Int3 WorldToBlock(Int3 world)
        {
            return world % Control.CHUNK_SIZE;
        }

        /// <summary>
        /// Converts from world-space to chunk-space
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public static Int3 WorldToChunk(Int3 world)
        {
            return world / Control.CHUNK_SIZE;
        }

        /// <summary>
        /// Converts from chunk-space to region-space
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public static Int3 ChunkToRegion(Int3 chunk)
        {
            return chunk % Control.REGION_SIZE;
        }

        public static void Dispose()
        {
            //TODO: Save chunks on unload
            foreach (Chunk c in _AllChunks)
                c.Unload();
            _AllChunks.Clear();
            _Load.Clear();
            _Unload.Clear();
            _Render.Clear();
            _Rebuild.Clear();
        }

        #region Update Functions
        #region ASYNC LOAD
        private delegate void LoadChunkDelegate(Chunk c);
        private static void AsyncLoadChunk(Chunk c)
        {
            ++_AsyncLoaders;
            AllChunks.Add(c);
            RegionReader reader = new RegionReader(ChunkToRegion(c.Position));
            LoadChunkDelegate del = reader.LoadChunk;
            del.BeginInvoke(c, FinishLoadChunk, del);
        }
        private static void FinishLoadChunk(IAsyncResult ir)
        {
            --_AsyncLoaders;
            LoadChunkDelegate del = ((LoadChunkDelegate)ir.AsyncState);
            del.EndInvoke(ir);
        }
        #endregion
        #region ASYNC BUILD
        private delegate void BuildChunkDelegate();
        private static void AsyncBuildChunk(Chunk c)
        {
            BuildChunkDelegate del = c.Build;
            del.BeginInvoke(FinishBuildChunk, del);
        }
        private static void FinishBuildChunk(IAsyncResult ir)
        {
            BuildChunkDelegate del = ((BuildChunkDelegate)ir.AsyncState);
            del.EndInvoke(ir);
        }
        #endregion
        private static int CompareByDistance(Chunk x, Chunk y)
        {
            int dx = (_origin - x.Position).Square(),
                dy = (_origin - y.Position).Square();
            if (dx < dy)
                return -1;
            if (dx > dy)
                return 1;
            return 0;
        }
        private static void UpdateLoad()
        {
            //TODO: Load from new Region files
            _loadcount = _Load.Count;
            if (_AsyncLoaders < MAX_ASYNC_LOAD)
            {
                _Load.Sort(CompareByDistance);
                int index = 0;
                while (_AsyncLoaders < MAX_ASYNC_LOAD && index < _Load.Count)
                {
                    AsyncLoadChunk(_Load.ElementAt(index++));
                }
            }
            _Load.Clear();
        }
        private static void UpdateRebuild()
        {
            _rebuildcount = _Rebuild.Count;
            foreach (Chunk c in _Rebuild)
                AsyncBuildChunk(c);
            _Rebuild.Clear();
        }
        private static void UpdateUnload()
        {
            //TODO: Save chunks in new Region files
            _unloadcount = _Unload.Count;
            foreach (Chunk c in _Unload)
            {
                c.Unload();
                _AllChunks.Remove(c);
            }
            _Unload.Clear();
        }
        private static void UpdateRender()
        {
            //TODO: Add some sort of Occlusion culling
            _Render.Clear();
            foreach (Chunk c in _AllChunks)
            {
                BoundingBox bb = new BoundingBox(c.Position * Control.CHUNK_SIZE, (c.Position + Int3.One) * Control.CHUNK_SIZE);
                if (Control.Frustum.Contains(bb) != ContainmentType.Disjoint)
                {
                    _Render.Add(c);
                }
            }
            _Render.Sort(CompareByDistance);
        }
        #endregion
        public static void Update(Vector3 inOrigin)
        {
            //TODO: Reorganize update function, include visibility call
            bool CheckForUnload = false;
            Int3 temp = (Int3)(inOrigin / Control.CHUNK_SIZE);
            if (_origin != temp)
            {
                _origin = temp;
                CheckForUnload = true;
            }
            for (int x = -VISIBILITY_RADIUS; x < VISIBILITY_RADIUS; x++)
            {
                int x2 = x * x;
                for (int y = -VISIBILITY_RADIUS; y < VISIBILITY_RADIUS; y++)
                {
                    int y2 = y * y;
                    for (int z = -VISIBILITY_RADIUS; z < VISIBILITY_RADIUS; z++)
                    {
                        if ((x2 + y2 + z * z) > VISIBILITY_RADIUS_SQUARED)
                            continue;
                        Chunk c = GetChunk(x + _origin.X, y + _origin.Y, z + _origin.Z);
                        if (c != null)
                            if (c[CF.Load])
                                _Load.Add(c);
                    }
                }
            }
            foreach (Chunk c in _AllChunks)
            {
                if (!c[CF.isLoaded])
                    continue;
                if (CheckForUnload)
                {
                    if ((_origin - c.Position).Square() > UNLOAD_RADIUS_SQUARED)
                    {
                        _Unload.Add(c);
                        continue;
                    }
                }
                if (c[CF.Build])
                    _Rebuild.Add(c);
            }
            UpdateRebuild();
            UpdateLoad();
            UpdateUnload();
            UpdateRender();
        }

        public static void Draw()
        {
            foreach (Chunk c in _Render)
                c.Draw();
        }

        #endregion
    }
}
