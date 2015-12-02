using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelEngine
{
    static class GameState
    {
        #region Fields

        private static GS _state;
        private static Game1 _game;

        #endregion
        #region Properties

        public static bool isMouseFree { get; private set; }
        public static bool canCameraLook { get; private set; }
        public static bool canCameraMove { get; private set; }
        public static bool showTestInterface { get; private set; }
        public static GS State
        {
            get
            {
                return _state;
            }
            set
            {
                switch (value)
                {
                    case (GS.Playing):
                        isMouseFree = false;
                        _game.IsMouseVisible = false;
                        canCameraLook = true;
                        canCameraMove = false;
                        showTestInterface = true;
                        break;
                    case (GS.Testing):
                        isMouseFree = false;
                        _game.IsMouseVisible = false;
                        canCameraLook = true;
                        canCameraMove = true;
                        showTestInterface = true;
                        break;
                    case (GS.Menu):
                        isMouseFree = true;
                        _game.IsMouseVisible = true;
                        canCameraLook = false;
                        canCameraMove = false;
                        showTestInterface = false;
                        break;
                }
                _state = value;
            }
        }

        #endregion
        #region Methods

        public static void Init(Game1 game, GS InitialState)
        {
            _game = game;
            State = InitialState;
        }

        #endregion
    }
}
