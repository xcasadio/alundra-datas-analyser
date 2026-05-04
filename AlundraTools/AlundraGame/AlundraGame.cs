using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using AlundraEngine.Etc;
using AlundraEngine.RuntimeInspection;

namespace AlundraGame
{
    public class AlundraGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameEngine _gameEngine;
        private RenderTarget2D _renderTarget;
        private InputManager _inputManager;
        private RuntimeInspectorHost? _runtimeInspector;
        private const int ScaleFactor = 4;

        public AlundraGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // Configure window size to 4x native resolution
            _graphics.PreferredBackBufferWidth = StaticVariables.ScreenWidth * ScaleFactor;
            _graphics.PreferredBackBufferHeight = StaticVariables.ScreenHeight * ScaleFactor;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            _renderTarget = new RenderTarget2D(
                GraphicsDevice,
                StaticVariables.ScreenWidth,
                StaticVariables.ScreenHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.PreserveContents);
            
            var gamePath = "D:\\development\\repo\\Alundra Remake\\Alundra (France)\\Alundra (France)_extracted";

            var dataFolder = Path.Combine(gamePath, "DATA");

            var datasBin = new DatasBin(Path.Combine(dataFolder, "DATAS.BIN"));
            var balanceFile = Path.Combine(dataFolder, "BALANCE.BIN");
            var balanceBin = new BalanceBin(balanceFile);
            var soundBinFileName = Path.Combine(dataFolder, "SOUND.BIN");
            var soundBin = new SoundBin(soundBinFileName);
            var font3 = new Font3(Path.Combine(dataFolder, "..", "TAKI\\SCREEN"));
            var etcResFileName = PathHelper.GetEtcFileName(dataFolder);
            EtcRes etcRes;

            if (Path.GetFileName(etcResFileName).Contains("usa", StringComparison.InvariantCultureIgnoreCase))
            {
                etcRes = new EtcResUsa(etcResFileName);
            }
            else
            {
                etcRes = new EtcResR(etcResFileName);
            }

            var alundraRenderer = new AlundraRenderer(_spriteBatch, GraphicsDevice);

            StaticVariables.ForceDesiredMap = -1;

            var savePath = @"D:\development\repo\alundra-datas-analyser\AlundraTools\AlundraTools\bin\Debug\net9.0-windows7.0\SaveStates\";
            StaticVariables.GameStateFileNameToLoad =
                savePath + "67 - boss - crypte de lars.json";
                //savePath + "71 - bonaire's dream save room.json";
                //savePath + "73 - bonaire's dream before boss.json";
                

        _gameEngine = new GameEngine(datasBin, balanceBin, soundBin, etcRes, font3, alundraRenderer);
            _gameEngine.InitializeEngine(false);
            
            _inputManager = new InputManager(_gameEngine);
            _runtimeInspector = RuntimeInspectorHost.TryStart(this, _gameEngine);
            if (_runtimeInspector != null)
            {
                _gameEngine.AttachRuntimeInspector(_runtimeInspector);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed 
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            _runtimeInspector?.Checkpoint("AlundraGame.Update");
            _inputManager.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _runtimeInspector?.Checkpoint("AlundraGame.Draw");

            //draw game
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);
            _gameEngine.MainLoop();

            //draw texture to screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone);

            var destinationRectangle = new Rectangle(
                0, 0,
                StaticVariables.ScreenWidth * ScaleFactor,
                StaticVariables.ScreenHeight * ScaleFactor);

            _spriteBatch.Draw(_renderTarget, destinationRectangle, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _runtimeInspector?.Dispose();
                _renderTarget?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
