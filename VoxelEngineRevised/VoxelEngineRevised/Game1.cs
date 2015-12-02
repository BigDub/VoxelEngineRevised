using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

// Using Right-hand Coordinate System

namespace VoxelEngine
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Constants

        public const int
            BUFFER_WIDTH = 800,
            BUFFER_HEIGHT = 600,
            BUFFER_CENTER_X = BUFFER_WIDTH / 2,
            BUFFER_CENTER_Y = BUFFER_HEIGHT / 2,
            INTERFACE_LINE_HEIGHT = 20,
            BLOCK_EDIT_DISTANCE = 8;
        public const float
            ASPECT_RATIO = BUFFER_WIDTH / BUFFER_HEIGHT,
            NEAR_CLIPPING = 0.1f,
            FAR_CLIPPING = 200,
            FREECAM_SPEED = 20;
        public static readonly Color
            INTERFACE_TEST_COLOR = Color.Yellow,
            BACKGROUND_COLOR = Color.CornflowerBlue;
        readonly int[] FrustumIndicies = new int[24]
                {
                    0,1,
                    1,2,
                    2,3,
                    3,0,
                    0,4,
                    1,5,
                    2,6,
                    3,7,
                    4,5,
                    5,6,
                    6,7,
                    7,4,
                };
        /* 0 = Near Top Left
         * 1 = Near Top Right
         * 2 = Near Bottom Right
         * 3 = Near Bottom Left
         * 4 = Far Top Left
         * 5 = Far Top Right
         * 6 = Far Bottom Right
         * 7 = Far Bottom Left
         */

        #endregion
        #region Fields

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState ksCurrent, ksPrevious;
        MouseState msCurrent, msPrevious;
        Vector3 CameraPosition = new Vector3(Control.CHUNK_SIZE * 5.5f, Control.CHUNK_SIZE * 8, Control.CHUNK_SIZE * 5.5f);
        Vector2 CameraAngle = new Vector2(0.75f, 2.15f);
        float Elapsed;
        SpriteFont font;
        Random random;
        Texture2D blank;

        #endregion
        #region Constructors

        public Game1()
        {
            this.TargetElapsedTime = new TimeSpan(0,0,0,0,33);
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = BUFFER_WIDTH;
            graphics.PreferredBackBufferHeight = BUFFER_HEIGHT;
            Content.RootDirectory = "Content";
        }

        #endregion
        #region Methods

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            random = new Random();
            Control.Init(this);
            Control.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(75), ASPECT_RATIO, NEAR_CLIPPING, FAR_CLIPPING);
            Control.View = Matrix.CreateLookAt(CameraPosition,
                    CameraPosition + Vector3.Transform(Vector3.Up, Matrix.CreateFromYawPitchRoll(CameraAngle.X, CameraAngle.Y, 0)),
                    Vector3.Up);
            Control.Frustum = new BoundingFrustum(Control.View * Control.Projection);

            Control.ColorShader.Parameters["Projection"].SetValue(Control.Projection);
            Control.NormalShader.Parameters["Projection"].SetValue(Control.Projection);
            Control.SolidShader.Parameters["Projection"].SetValue(Control.Projection);
            Control.SetFog(50, BACKGROUND_COLOR);
            GameState.Init(this, GS.Testing);
            font = Content.Load<SpriteFont>("SpriteFont1");
            msCurrent = Mouse.GetState();
            msPrevious = msCurrent;
            MapManager.Init("Content\\Map01");
            blank = new Texture2D(GraphicsDevice, 1, 1);
            blank.SetData(new Color[] { Color.White });
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            MapManager.Dispose();
        }

        
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        #region Update Functions
        void RemoveBlock()
        {
            Vector3 tempPos = CameraPosition;
            Vector3 d = Vector3.Transform(Vector3.UnitY, Matrix.CreateFromYawPitchRoll(CameraAngle.X, CameraAngle.Y, 0)) / 4;
            Int3 BlockPos = (Int3)tempPos;
            for (int step = 1; step < BLOCK_EDIT_DISTANCE * 4; step++)
            {
                tempPos += d;
                Int3 tempi3 = (Int3)tempPos;
                if (tempi3 != BlockPos)
                {
                    BlockPos = tempi3;
                    byte cData = MapManager.GetBlock(BlockPos);
                    if (cData != (byte)BT.Reserved && cData != (byte)BT.Empty)
                    {
                        Console.WriteLine((BT)cData + " block found");
                        MapManager.SetBlock(BlockPos, (byte)BT.Empty);
                        return;
                    }
                }
            }
            Console.WriteLine("No block found");
        }
        void UpdateInput()
        {
            #region GetInput
            ksPrevious = ksCurrent;
            ksCurrent = Keyboard.GetState();
            msPrevious = msCurrent;
            msCurrent = Mouse.GetState();
            if (!GameState.isMouseFree)
                Mouse.SetPosition(BUFFER_CENTER_X, BUFFER_CENTER_Y);
            #endregion
            #region ProcessInput
            #region Mouse
            if (GameState.canCameraLook)
            {
                //Have the Y inverted.
                Vector2 MouseMovement = new Vector2((BUFFER_CENTER_X - msCurrent.X) / (float)BUFFER_WIDTH, (msCurrent.Y - BUFFER_CENTER_Y) / (float)BUFFER_HEIGHT);
                CameraAngle += MouseMovement;
                CameraAngle.Y = MathHelper.Clamp(CameraAngle.Y, 0.01f, MathHelper.Pi - 0.01f);
                CameraAngle.X = MathHelper.WrapAngle(CameraAngle.X);
            }
            if (msCurrent.LeftButton == ButtonState.Pressed && msPrevious.LeftButton == ButtonState.Released)
            {
                RemoveBlock();
            }
            #endregion
            #region Keyboard
            if (ksCurrent.IsKeyDown(Keys.Space) && ksPrevious.IsKeyUp(Keys.Space))
            {
            }
            if (ksCurrent.IsKeyUp(Keys.Tab) && ksPrevious.IsKeyDown(Keys.Tab))
            {
                if (GameState.State == GS.Playing)
                {
                    GameState.State = GS.Testing;
                }
                else if (GameState.State == GS.Testing)
                {
                    GameState.State = GS.Playing;
                }
            }
            if (ksCurrent.IsKeyDown(Keys.Escape))
                this.Exit();
            Vector3 Movement = new Vector3();
            if (ksCurrent.IsKeyDown(Keys.W))
                Movement.Z += 1;
            if (ksCurrent.IsKeyDown(Keys.A))
                Movement.X += 1;
            if (ksCurrent.IsKeyDown(Keys.S))
                Movement.Z -= 1;
            if (ksCurrent.IsKeyDown(Keys.D))
                Movement.X -= 1;
            if (GameState.State == GS.Testing)
            {
                if (ksCurrent.IsKeyDown(Keys.Q))
                    Movement.Y += 1;
                if (ksCurrent.IsKeyDown(Keys.E))
                    Movement.Y -= 1;
                if (Movement != Vector3.Zero)
                {
                    Movement.Normalize();
                    CameraPosition += Vector3.Transform(Movement, Matrix.CreateRotationY(CameraAngle.X)) * FREECAM_SPEED * Elapsed;
                }
            }
            #endregion
            #endregion
        }
        void UpdateControl()
        {
            if (GameState.canCameraLook)
            {
                Control.View = Matrix.CreateLookAt(CameraPosition,
                    CameraPosition + Vector3.Transform(Vector3.Up, Matrix.CreateFromYawPitchRoll(CameraAngle.X, CameraAngle.Y, 0)),
                    Vector3.Up);
                Control.Frustum = new BoundingFrustum(Control.View * Control.Projection);
            }
        }
        #endregion
        protected override void Update(GameTime gameTime)
        {
            Elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateInput();
            UpdateControl();
            MapManager.Update(CameraPosition);

            base.Update(gameTime);
        }

        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        #region Draw Functions
        void DrawFrustrum()
        {
            Vector3[] verts = Control.Frustum.GetCorners();
            VertexPosition[] verticies = new VertexPosition[8];
            for (int i = 0; i < 8; i++)
                verticies[i] = new VertexPosition(verts[i]);
            Control.SolidShader.Parameters["World"].SetValue(Matrix.Identity);
            foreach (EffectPass pass in Control.SolidShader.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives<VertexPosition>(PrimitiveType.LineList, verticies, 0, 8, FrustumIndicies, 0, 12, VertexPosition.VertexDeclaration);
            }
        }
        void DTILine(String text, int line)
        {
            spriteBatch.DrawString(font, text, new Vector2(0, line * INTERFACE_LINE_HEIGHT), INTERFACE_TEST_COLOR);
        }
        void DrawTestInterface()
        {
            int y = 0;
            DTILine("Position: " + Control.Format(CameraPosition), y++);
            DTILine("I3Position: " + (Int3)CameraPosition, y++);
            DTILine("CameraAngle: " + Control.Format(CameraAngle), y++);
            DTILine("Chunks in mem: " + MapManager.CountAll, y++);
            DTILine("Chunks loading: " + MapManager.CountLoad, y++);
            DTILine("Chunks rebuilding: " + MapManager.CountRebuild, y++);
            DTILine("Chunks unloading: " + MapManager.CountUnload, y++);
            DTILine("Chunks rendering: " + MapManager.CountRender, y++);
        }
        #endregion
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BACKGROUND_COLOR);

            Control.PrepEffects(CameraPosition);
            MapManager.Draw();
            //DrawFrustrum();

            spriteBatch.Begin();
            spriteBatch.Draw(blank, new Vector2(BUFFER_CENTER_X, BUFFER_CENTER_Y), Color.White);
            if (GameState.showTestInterface)
                DrawTestInterface();
            spriteBatch.End();
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            base.Draw(gameTime);
        }

        #endregion
    }
}
