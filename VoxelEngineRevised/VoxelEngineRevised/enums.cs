using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelEngine
{
    /// <summary>
    /// Chunk Parser Tokens
    /// </summary>
    enum CPT
    {
        subdiv,
        data,
    };

    /// <summary>
    /// Chunk Subdivision Flags
    /// If true, that block is subdivided
    /// </summary>
    enum CSF : byte
    {
        None = 0x00,

        /// <summary>
        /// Left Bottom Front
        /// </summary>
        LBF = 0x80,

        /// <summary>
        /// Left Bottom Back
        /// </summary>
        LBB = 0x40,

        /// <summary>
        /// Left Top Front
        /// </summary>
        LTF = 0x20,

        /// <summary>
        /// Left Top Back
        /// </summary>
        LTB = 0x10,

        /// <summary>
        /// Right Bottom Front
        /// </summary>
        RBF = 0x08,

        /// <summary>
        /// Right Bottom Back
        /// </summary>
        RBB = 0x04,

        /// <summary>
        /// Right Top Front
        /// </summary>
        RTF = 0x02,

        /// <summary>
        /// Right Top Back
        /// </summary>
        RTB = 0x01,
    };

    /// <summary>
    /// Chunk Encryption Type
    /// </summary>
    enum CET : byte
    {
        Solid = 0x01,
        Subdivided = 0x02,
        Array = 0x03,
        Null = 0x00,
    };

    /// <summary>
    /// Game State
    /// </summary>
    enum GS
    {
        Playing,
        Testing,
        Menu,
    };

    /// <summary>
    /// Block Type
    /// </summary>
    enum BT : byte
    {
        Empty = 0x00,
        Stone = 0x01,
        Dirt = 0x02,
        Grass = 0x03,
        Reserved = 0xFF,
    };

    /// <summary>
    /// Chunk Flags
    /// </summary>
    enum CF : byte
    {
        None = 0x00,

        /// <summary>
        /// This is the state a newly initialized Chunk should be in.
        /// </summary>
        NewChunk = Load | Build,

        /// <summary>
        /// Whether or not this Chunk is finished loading.
        /// </summary>
        isLoaded = 0x01,

        /// <summary>
        /// Whether or not this Chunk needs to be loaded.
        /// </summary>
        Load = 0x02,

        /// <summary>
        /// Whether or not this Chunk needs to be saved.
        /// </summary>
        Save = 0x04,

        /// <summary>
        /// Whether or not this Chunk needs to be Built;
        /// </summary>
        Build = 0x08,

        /// <summary>
        /// Whether the Chunk has a mesh or not.
        /// </summary>
        Mesh = 0x10,
    };

    struct VertexPosition
    {
        public Vector3 Position;
        public VertexPosition(Vector3 pos)
        {
            Position = pos;
        }
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
        );
    }
    struct VertexPositionColorNormal
    {
        public Vector3 Position;
        public Color color;
        public Vector3 Normal;
        public VertexPositionColorNormal(Vector3 pos, Vector3 nor, Color col)
        {
            Position = pos;
            Normal = nor;
            color = col;
        }
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
    }
}
