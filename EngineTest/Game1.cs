﻿#undef USE_GEARSET

using System;
using BEPUphysics;
using EngineTest.Gearset;
using EngineTest.Main;
using EngineTest.Recources;
using Gearset;
using Gearset.Components.Profiler;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector3 = BEPUutilities.Vector3;

namespace EngineTest
{
    public class Game1 : Game
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly GraphicsDeviceManager _graphics;

        private readonly ScreenManager _screenManager;

        private bool _isActive = true;

        private readonly Space _physicsSpace;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public Game1()
        {
            //Initialize graphics and content
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //Initialize screen manager, which controls draw / logic for our screens
            _screenManager = new ScreenManager();

            //Initialize our physics and give it gravity
            _physicsSpace = new Space
            {
                ForceUpdater = { Gravity = new Vector3(0, 0, -9.81f) }
            };

            //Set up graphics properties, no vsync, no framelock
            _graphics.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = false;
            //TargetElapsedTime = TimeSpan.FromMilliseconds(100);

            //Size of our application / starting back buffer
            _graphics.PreferredBackBufferWidth = GameSettings.g_ScreenWidth;
            _graphics.PreferredBackBufferHeight = GameSettings.g_ScreenHeight;

            _graphics.PreferMultiSampling = true;
            _graphics.PreferredBackBufferFormat = SurfaceFormat.Color;

            _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
            //HiDef enables usable shaders
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;

            //_graphics.GraphicsDevice.DeviceLost += new EventHandler<EventArgs>(ClientLostDevice);

            //Mouse should not disappear
            IsMouseVisible = true;

            //Window settings
            Window.AllowUserResizing = true;
            Window.IsBorderless = false;

            //Update all our rendertargets when we resize
            Window.ClientSizeChanged += ClientChangedWindowSize;
            
            //Update framerate etc. when not the active window
            Activated += IsActivated;
            Deactivated += IsDeactivated;
        }

        private void IsActivated(object sender, EventArgs e)
        {
            //_isActive = true;
        }

        private void IsDeactivated(object sender, EventArgs e)
        {
            //_isActive = false;
        }

        /// <summary>
        /// Update rendertargets and backbuffer when resizing window size
        /// </summary>
        private void ClientChangedWindowSize(object sender, EventArgs e)
        {
            if (GraphicsDevice.Viewport.Width != _graphics.PreferredBackBufferWidth ||
                GraphicsDevice.Viewport.Height != _graphics.PreferredBackBufferHeight)
            {
                if (Window.ClientBounds.Width == 0) return;
                _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                _graphics.ApplyChanges();

                GameSettings.g_ScreenWidth = Window.ClientBounds.Width;
                GameSettings.g_ScreenHeight = Window.ClientBounds.Height;

                _screenManager.UpdateResolution();
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _screenManager.Load(Content, GraphicsDevice);
            // TODO: Add your initialization logic here
            _screenManager.Initialize(GraphicsDevice, _physicsSpace);

            GraphicsDevice.PresentationParameters.MultiSampleCount = 4;

            //Initialise Gearset in 'headless' mode with no 'external' UI - (overlays are still available).
            //We want to monitor managed memory allocations from our app and the UIs generate a fair amount of garbage which would distort the profiling.
            GS.Initialize(this, createUI: true);
            
            GS.Action(() => GS.GearsetComponent.Console.Inspect("Profiler", new ProfilerInpectorSettings(GS.GearsetComponent.Console.Profiler)));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            _screenManager.Unload(Content);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //Exit the game when pressing escape
            if (Input.WasKeyPressed(Keys.Escape))
                Exit();
            
            _screenManager.Update(gameTime, _isActive);

            //BEPU Physics
            if(GameSettings.p_Physics)
                _physicsSpace.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);

            const string mark = "Update";


            GS.StartFrame();
            GS.BeginMark(mark, FlatTheme.PeterRiver);
            //GS.Plot("FPS", (float) GameStats.fps_avg, 100);

#if USE_GEARSET
            if (GS.GearsetComponent.Console.Profiler.DoUpdate() == false)
            {
                return;
            }
#endif

            GS.EndMark(mark);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //Don't draw when the game is not running
            if (!_isActive) return;

            GS.BeginMark("Draw", FlatTheme.Pomegrantate);

            _screenManager.Draw(gameTime);


            base.Draw(gameTime);
            GS.EndMark("Draw");

        }
    }
}
