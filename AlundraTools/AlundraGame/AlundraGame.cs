using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.Closing;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Etc;
using AlundraEngine.RuntimeInspection;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using FontStashSharp;
using MGUI.Backend.MonoGame;
using MGUI.Core.UI;
using MGUI.FontStashSharp;
using MGUI.Shared.Rendering;
using MGUI.Shared.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace AlundraGame
{
    public class AlundraGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch = null!;
        private GameEngine _gameEngine = null!;
        private MonoGameSoundPlaybackBackend? _soundBackend;
        private RenderTarget2D _renderTarget = null!;
        private InputManager _inputManager = null!;
        private RuntimeInspectorHost? _runtimeInspector;
        private DelegateRenderHost? _mguiHost;
        private IMonoGameDesktopBackend? _mguiRenderer;
        private MGDesktop? _desktop;
        private FrmGameDebugPanelController? _debugPanelController;
        private const string DebugPanelFontFamily = "JetBrainsMono";
        private const int DebugPanelFontSize = 9;
        private const int DefaultScaleFactor = 4;
        private const int DebugPanelWidth = 512;
        private static readonly int[] TemporaryWarpEffectIds = [0, 2, 4, 5, 6, 8, 9, 10, 11];
        private int _renderScaleFactor = DefaultScaleFactor;
        private int _temporaryWarpEffectIndex = Array.IndexOf(TemporaryWarpEffectIds, 8);
        private readonly string? _datasBinFilePath;
        private KeyboardState _previousKeyboardState;
        private GameState _state = GameState.Game;
        private ClosingEngine _closingEngine;
        private int GameRenderWidth => StaticVariables.ScreenWidth * _renderScaleFactor;
        private int GameRenderHeight => StaticVariables.ScreenHeight * _renderScaleFactor;

        public AlundraGame(string? datasBinFilePath = null)
        {
            _datasBinFilePath = datasBinFilePath;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            SetPreferredBackBufferSize(false);
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
            
            string dataFolder;
            DatasBin datasBin;

            var gamePath = "D:\\development\\repo\\Alundra Remake\\Alundra (France)\\Alundra (France)_extracted";

            if (!string.IsNullOrWhiteSpace(_datasBinFilePath))
            {
                gamePath = _datasBinFilePath.Replace(Path.Combine("DATA", "DATAS.BIN"), string.Empty);
                dataFolder = Path.GetDirectoryName(_datasBinFilePath)
                    ?? throw new DirectoryNotFoundException($"Unable to resolve the data folder from '{_datasBinFilePath}'.");
                datasBin = new DatasBin(_datasBinFilePath);
            }
            else
            {
                dataFolder = Path.Combine(gamePath, "DATA");
                datasBin = new DatasBin(Path.Combine(dataFolder, "DATAS.BIN"));
            }

            var balanceFile = Path.Combine(dataFolder, "BALANCE.BIN");
            var balanceBin = new BalanceBin(balanceFile);
            var soundBinFileName = Path.Combine(dataFolder, "SOUND.BIN");
            var soundBin = new SoundBin(soundBinFileName);
            _soundBackend = new MonoGameSoundPlaybackBackend();
            soundBin.AttachPlaybackBackend(_soundBackend);
            var font3 = new Font3(Path.Combine(dataFolder, "..", "TAKI\\SCREEN"));
            var etcResFileName = PathHelper.GetEtcFileName(dataFolder);
            EtcRes etcRes;

            if (Path.GetFileName(etcResFileName).Contains("usa", StringComparison.InvariantCultureIgnoreCase))
            {
                AlundraConfiguration.Version = AlundraVersion.Usa;
                etcRes = new EtcResUsa(etcResFileName);
            }
            else
            {
                AlundraConfiguration.Version = AlundraVersion.European;
                etcRes = new EtcResR(etcResFileName);
            }

            var alundraRenderer = new AlundraRenderer(_spriteBatch, GraphicsDevice);

            _gameEngine = new GameEngine(datasBin, balanceBin, soundBin, etcRes, font3, alundraRenderer);
            _gameEngine.InitializeEngine(true);

            _closingEngine = new ClosingEngine(alundraRenderer);
            _closingEngine.InitializeEngine(gamePath);

            // Desktop adaptation of the original 60 Hz VSync/timer sound interrupt: the sound
            // tick keeps running on its own thread while this thread blocks on map loading.
            _gameEngine.SoundManager.HasExternalSoundTickDriver = true;
            _soundBackend.StartTickDriver(_gameEngine.SoundManager.AdvanceSoundFrame);


            _inputManager = new InputManager(_gameEngine);
            _runtimeInspector = RuntimeInspectorHost.TryStart(this, _gameEngine);
            if (_runtimeInspector != null)
            {
                _gameEngine.AttachRuntimeInspector(_runtimeInspector);
            }

            InitializeMguiDebugPanel();
            UpdateTemporaryDebugHotkeyTitle();
        }

        protected override void Update(GameTime gameTime)
        {
            _mguiHost?.NotifyPreviewUpdate(gameTime.TotalGameTime);

            var keyboardState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed 
                || keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            HandleTemporaryDebugHotkeys(keyboardState);

            _runtimeInspector?.Checkpoint("AlundraGame.Update");
            _inputManager.Update();
            _debugPanelController?.Refresh();
            _desktop?.Update();
            base.Update(gameTime);
            _mguiHost?.NotifyEndUpdate();
        }

        protected override void Draw(GameTime gameTime)
        {
            _runtimeInspector?.Checkpoint("AlundraGame.Draw");

            //draw game
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);

            switch (_state)
            {
                case GameState.MainMenu:
                    //_state = _mainMenuEngine.MainLoop();
                    Breakpoint.TriggerBreak();
                    break;

                case GameState.Game:
                    _state = _gameEngine.MainLoop();
                    break;

                case GameState.EndScene:
                    _state = _closingEngine.MainLoop();
                    break;
            }


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
                GameRenderWidth,
                GameRenderHeight);

            _spriteBatch.Draw(_renderTarget, destinationRectangle, Color.White);
            _spriteBatch.End();

            _desktop?.Draw();

            base.Draw(gameTime);
        }

        // JUSTIFICATION: backend MonoGame only
        private void InitializeMguiDebugPanel()
        {
            MonoGameBackendSession<DelegateRenderHost> backend = MonoGameBackendBootstrap.Create(
                new DelegateRenderHost(
                    GraphicsDevice,
                    () => new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height),
                    Services));

            _mguiHost = backend.Host;
            _mguiRenderer = backend.Renderer;
            _desktop = new MGDesktop((IUIDesktopRuntime)_mguiRenderer);
            _desktop.InputTracker.GamePad.Enabled = false;
            InitializeMguiFonts(_desktop, _mguiRenderer);
            _desktop.Resources.DefaultTheme = new MGTheme(MGTheme.BuiltInTheme.Dark, DebugPanelFontFamily);
            _desktop.Theme.FontSettings.DefaultFontSize = DebugPanelFontSize;
            _desktop.Theme.FontSettings.SmallFontSize = DebugPanelFontSize;
            _desktop.Theme.FontSettings.MediumFontSize = DebugPanelFontSize;
            _debugPanelController = FrmGameDebugPanelController.Load(
                _desktop,
                _gameEngine,
                GameRenderWidth,
                GameRenderHeight,
                () => SaveSnapshot(),
                SetZoomLevel);
        }

        // JUSTIFICATION: backend MonoGame only
        private static void InitializeMguiFonts(MGDesktop desktop, IMonoGameDesktopBackend renderer)
        {
            string fontDirectory = Path.Combine(AppContext.BaseDirectory, "Content", DebugPanelFontFamily);
            FontStashSharpTextEngine textEngine = new();

            byte[] regular = File.ReadAllBytes(Path.Combine(fontDirectory, "JetBrainsMono-Regular.ttf"));
            textEngine.AddFontSystem(DebugPanelFontFamily, CustomFontStyles.Normal, CreateFontSystem(regular), regular);

            byte[] bold = File.ReadAllBytes(Path.Combine(fontDirectory, "JetBrainsMono-Bold.ttf"));
            textEngine.AddFontSystem(DebugPanelFontFamily, CustomFontStyles.Bold, CreateFontSystem(bold));

            byte[] boldItalic = File.ReadAllBytes(Path.Combine(fontDirectory, "JetBrainsMono-BoldItalic.ttf"));
            textEngine.AddFontSystem(DebugPanelFontFamily, CustomFontStyles.Bold | CustomFontStyles.Italic, CreateFontSystem(boldItalic));

            renderer.FontManager.DefaultFontFamily = DebugPanelFontFamily;
            desktop.Theme.FontSettings.DefaultFontFamily = DebugPanelFontFamily;
            desktop.Theme.FontSettings.DefaultFontSize = DebugPanelFontSize;
            desktop.Theme.FontSettings.SmallFontSize = DebugPanelFontSize;
            desktop.Theme.FontSettings.MediumFontSize = DebugPanelFontSize;
            desktop.TextEngine = textEngine;
        }

        // JUSTIFICATION: backend MonoGame only
        private static FontSystem CreateFontSystem(byte[] fontBytes)
        {
            FontSystem fontSystem = new();
            fontSystem.AddFont(fontBytes);
            return fontSystem;
        }

        // JUSTIFICATION: backend MonoGame only
        private void SetZoomLevel(int zoomScale)
        {
            _renderScaleFactor = zoomScale;
            SetPreferredBackBufferSize(true);
            _debugPanelController?.SetPanelBounds(GameRenderWidth, GameRenderHeight);
        }

        // JUSTIFICATION: backend MonoGame only
        private void SetPreferredBackBufferSize(bool applyChanges)
        {
            _graphics.PreferredBackBufferWidth = GameRenderWidth + DebugPanelWidth;
            _graphics.PreferredBackBufferHeight = GameRenderHeight;

            if (applyChanges)
            {
                _graphics.ApplyChanges();
            }
        }

        // JUSTIFICATION: backend MonoGame only
        private void HandleTemporaryDebugHotkeys(KeyboardState keyboardState)
        {
            bool isPreviousEffectHotkeyPressed = keyboardState.IsKeyDown(Keys.F7)
                && (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl));
            bool wasPreviousEffectHotkeyPressed = _previousKeyboardState.IsKeyDown(Keys.F7)
                && (_previousKeyboardState.IsKeyDown(Keys.LeftControl) || _previousKeyboardState.IsKeyDown(Keys.RightControl));
            bool isQueueEffectHotkeyPressed = keyboardState.IsKeyDown(Keys.F8)
                && (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl));
            bool wasQueueEffectHotkeyPressed = _previousKeyboardState.IsKeyDown(Keys.F8)
                && (_previousKeyboardState.IsKeyDown(Keys.LeftControl) || _previousKeyboardState.IsKeyDown(Keys.RightControl));
            bool isNextEffectHotkeyPressed = keyboardState.IsKeyDown(Keys.F9)
                && (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl));
            bool wasNextEffectHotkeyPressed = _previousKeyboardState.IsKeyDown(Keys.F9)
                && (_previousKeyboardState.IsKeyDown(Keys.LeftControl) || _previousKeyboardState.IsKeyDown(Keys.RightControl));

            if (isPreviousEffectHotkeyPressed && !wasPreviousEffectHotkeyPressed)
            {
                _temporaryWarpEffectIndex = (_temporaryWarpEffectIndex + TemporaryWarpEffectIds.Length - 1) % TemporaryWarpEffectIds.Length;
                UpdateTemporaryDebugHotkeyTitle();
            }

            if (isNextEffectHotkeyPressed && !wasNextEffectHotkeyPressed)
            {
                _temporaryWarpEffectIndex = (_temporaryWarpEffectIndex + 1) % TemporaryWarpEffectIds.Length;
                UpdateTemporaryDebugHotkeyTitle();
            }

            if (isQueueEffectHotkeyPressed && !wasQueueEffectHotkeyPressed)
            {
                _gameEngine.QueueTemporaryWarpTransitionEffect(TemporaryWarpEffectIds[_temporaryWarpEffectIndex]);
            }

            _previousKeyboardState = keyboardState;
        }

        // JUSTIFICATION: backend MonoGame only
        private void UpdateTemporaryDebugHotkeyTitle()
        {
            Window.Title = $"AlundraGame - Temp warp effect {TemporaryWarpEffectIds[_temporaryWarpEffectIndex]} (Ctrl+F7/F9 select, Ctrl+F8 queue)";
        }

        // JUSTIFICATION: backend MonoGame only
        private string SaveSnapshot()
        {
            string snapshotDirectory = Path.Combine(Environment.CurrentDirectory, "Snapshots");
            Directory.CreateDirectory(snapshotDirectory);

            int snapshotIndex = 0;
            while (true)
            {
                string fileName = Path.Combine(snapshotDirectory, $"Snapshot_{snapshotIndex++}.png");
                if (File.Exists(fileName))
                {
                    continue;
                }

                using FileStream stream = File.Create(fileName);
                _renderTarget.SaveAsPng(stream, _renderTarget.Width, _renderTarget.Height);
                return fileName;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _soundBackend?.StopTickDriver();
                _runtimeInspector?.Dispose();
                _renderTarget?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
