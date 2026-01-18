using System.Collections.Generic;
using AlundraEngine;
using AlundraEngine.Gameplay;
using Microsoft.Xna.Framework.Input;

namespace AlundraGame;

public class InputManager
{
    private readonly GameEngine _gameEngine;
    private const float JoystickThreshold = 0.50f;
    private const int CameraStep = 10;

    private static readonly Dictionary<Keys, uint> KeyboardMappings = new()
    {
        { Keys.Up, PadState.Up },
        { Keys.Down, PadState.Down },
        { Keys.Left, PadState.Left },
        { Keys.Right, PadState.Right },
        { Keys.U, PadState.Cross },
        { Keys.I, PadState.Circle },
        { Keys.J, PadState.Square },
        { Keys.K, PadState.Triangle },
        { Keys.Enter, PadState.Start },
        { Keys.Back, PadState.Select },
        { Keys.Q, PadState.L1 },
        { Keys.E, PadState.R1 },
        { Keys.LeftShift, PadState.L2 },
        { Keys.RightShift, PadState.R2 }
    };

    public InputManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    public void Update()
    {
        PadManager.ButtonStates = 0;

        UpdateGamePad();
        UpdateKeyboard();
    }

    private void UpdateGamePad()
    {
        var gamePadState = GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);

        if (!gamePadState.IsConnected)
        {
            return;
        }

        // D-Pad and Left Stick for movement
        if (gamePadState.DPad.Up == ButtonState.Pressed || gamePadState.ThumbSticks.Left.Y > JoystickThreshold)
        {
            PadManager.ButtonStates |= PadState.Up;
        }

        if (gamePadState.DPad.Down == ButtonState.Pressed || gamePadState.ThumbSticks.Left.Y < -JoystickThreshold)
        {
            PadManager.ButtonStates |= PadState.Down;
        }

        if (gamePadState.DPad.Left == ButtonState.Pressed || gamePadState.ThumbSticks.Left.X < -JoystickThreshold)
        {
            PadManager.ButtonStates |= PadState.Left;
        }

        if (gamePadState.DPad.Right == ButtonState.Pressed || gamePadState.ThumbSticks.Left.X > JoystickThreshold)
        {
            PadManager.ButtonStates |= PadState.Right;
        }

        // Face buttons
        if (gamePadState.Buttons.A == ButtonState.Pressed)
        {
            PadManager.ButtonStates |= PadState.Cross;
        }

        if (gamePadState.Buttons.B == ButtonState.Pressed)
        {
            PadManager.ButtonStates |= PadState.Circle;
        }

        if (gamePadState.Buttons.X == ButtonState.Pressed)
        {
            PadManager.ButtonStates |= PadState.Square;
        }

        if (gamePadState.Buttons.Y == ButtonState.Pressed)
        {
            PadManager.ButtonStates |= PadState.Triangle;
        }

        // Shoulder buttons
        if (gamePadState.Buttons.RightShoulder == ButtonState.Pressed)
        {
            PadManager.ButtonStates |= PadState.R1;
        }

        if (gamePadState.Buttons.LeftShoulder == ButtonState.Pressed)
        {
            PadManager.ButtonStates |= PadState.L1;
        }

        // Start and Select
        if (gamePadState.Buttons.Start == ButtonState.Pressed)
        {
            PadManager.ButtonStates |= PadState.Start;
        }

        if (gamePadState.Buttons.Back == ButtonState.Pressed)
        {
            PadManager.ButtonStates |= PadState.Select;
        }

        // Triggers
        if (gamePadState.Triggers.Left > 0.5f)
        {
            PadManager.ButtonStates |= PadState.L2;
        }

        if (gamePadState.Triggers.Right > 0.5f)
        {
            PadManager.ButtonStates |= PadState.R2;
        }

        // Right stick for camera debug offset
        if (gamePadState.ThumbSticks.Right.Y > JoystickThreshold)
        {
            _gameEngine.StaticVariables.g_cameraDebugOffsetY -= CameraStep;
        }

        if (gamePadState.ThumbSticks.Right.Y < -JoystickThreshold)
        {
            _gameEngine.StaticVariables.g_cameraDebugOffsetY += CameraStep;
        }

        if (gamePadState.ThumbSticks.Right.X > JoystickThreshold)
        {
            _gameEngine.StaticVariables.g_cameraDebugOffsetX += CameraStep;
        }

        if (gamePadState.ThumbSticks.Right.X < -JoystickThreshold)
        {
            _gameEngine.StaticVariables.g_cameraDebugOffsetX -= CameraStep;
        }
    }

    private void UpdateKeyboard()
    {
        var keyboardState = Keyboard.GetState();

        foreach (var (key, flag) in KeyboardMappings)
        {
            if (keyboardState.IsKeyDown(key))
            {
                PadManager.ButtonStates |= flag;
            }
        }
    }
}
