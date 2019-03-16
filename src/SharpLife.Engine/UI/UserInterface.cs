/***
*
*	Copyright (c) 1996-2001, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology").  Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
*   This source code contains proprietary and confidential information of
*   Valve LLC and its suppliers.  Access to this code is restricted to
*   persons who have executed a written SDK license with Valve.  Any access,
*   use or distribution of this code by or to any unlicensed person is illegal.
*
****/

using SDL2;
using Serilog;
using SharpLife.CommandSystem;
using SharpLife.Engine.Client;
using SharpLife.Engine.Shared;
using SharpLife.FileSystem;
using SharpLife.Input;
using SharpLife.Utility;
using System;

namespace SharpLife.Engine.UI
{
    /// <summary>
    /// Controls the User Interface components
    /// </summary>
    internal sealed class UserInterface
    {
        private readonly ILogger _logger;

        private readonly IFileSystem _fileSystem;

        private readonly Renderer.Renderer _renderer;

        public IInputSystem InputSystem { get; } = new InputSystem();

        public Window Window { get; private set; }

        /// <summary>
        /// Invoked when the Quit event has been received
        /// </summary>
        public event Action Quit;

        public UserInterface(ILogger logger, ITime engineTime, IFileSystem fileSystem, ICommandContext commandContext, EngineClient client,
            bool noOnTop, string windowTitle, SDL.SDL_WindowFlags additionalFlags = 0)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            //Disable to prevent debugger from shutting down the game
            SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");

            if (noOnTop)
            {
                SDL.SDL_SetHint(SDL.SDL_HINT_ALLOW_TOPMOST, "0");
            }

            SDL.SDL_SetHint(SDL.SDL_HINT_VIDEO_X11_XRANDR, "1");
            SDL.SDL_SetHint(SDL.SDL_HINT_VIDEO_X11_XVIDMODE, "1");

            SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);

            Window = new Window(_logger, _fileSystem, windowTitle, additionalFlags);

            Window.Center();

            _renderer = new Renderer.Renderer(logger, engineTime, commandContext, fileSystem, this, client, Framework.Path.Shaders);
        }

        /// <summary>
        /// Destroys the main window if exists
        /// </summary>
        public void DestroyMainWindow()
        {
            if (Window != null)
            {
                Window.Destroy();
                Window = null;
            }
        }

        /// <summary>
        /// Sleep up to <paramref name="milliSeconds"/> milliseconds, waking to process events
        /// </summary>
        /// <param name="milliSeconds"></param>
        public void SleepUntilInput(int milliSeconds)
        {
            InputSystem.ProcessEvents(milliSeconds);

            var snapshot = InputSystem.Snapshot;

            for (var i = 0; i < snapshot.Events.Count; ++i)
            {
                var sdlEvent = snapshot.Events[i];

                switch (sdlEvent.type)
                {
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                        {
                            Window.ProcessEvent(ref sdlEvent);

                            break;
                        }
                    case SDL.SDL_EventType.SDL_QUIT:
                        {
                            Quit?.Invoke();
                            break;
                        }
                }
            }
        }

        public void Update(float deltaSeconds)
        {
            _renderer.Update(deltaSeconds);
        }

        public void Draw()
        {
            _renderer.Draw();
        }

        /// <summary>
        /// Shuts down the user interface
        /// The interface can no longer be used after this
        /// </summary>
        public void Shutdown()
        {
            SDL.SDL_Quit();
        }
    }
}
