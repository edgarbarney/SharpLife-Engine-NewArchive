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

using Serilog;
using SharpLife.CommandSystem;
using SharpLife.CommandSystem.Commands;
using SharpLife.Engine.Client;
using SharpLife.Engine.CommandSystem;
using SharpLife.Engine.Configuration;
using SharpLife.Engine.Events;
using SharpLife.Engine.GameWorld;
using SharpLife.Engine.Logging;
using SharpLife.Engine.Plugins;
using SharpLife.Engine.Server;
using SharpLife.FileSystem;
using SharpLife.Utility;
using SharpLife.Utility.Events;
using SharpLife.Utility.FileSystem;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace SharpLife.Engine.Host
{
    /// <summary>
    /// Manages top level engine components (client, server) and shared components
    /// Can host clients, dedicated servers and clients running listen servers
    /// </summary>
    internal sealed class Engine
    {
        private static readonly string[] ExecPathIDs = new[]
        {
            FileSystemConstants.PathID.GameConfig,
            FileSystemConstants.PathID.Game,
            FileSystemConstants.PathID.All
        };

        private const int MaximumFPS = 1000;

        private const int DefaultFPS = 60;

        /// <summary>
        /// Gets the command line passed to the engine
        /// </summary>
        public ICommandLine CommandLine { get; }

        /// <summary>
        /// Gets the filesystem used by the engine
        /// </summary>
        public IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the game directory that this game was loaded from
        /// </summary>
        public string GameDirectory { get; }

        /// <summary>
        /// Gets the command system
        /// </summary>
        public ICommandSystem CommandSystem { get; }

        /// <summary>
        /// Gets the engine command context, shared with the client and server
        /// </summary>
        public ICommandContext EngineContext { get; }

        public WorldState World { get; }

        /// <summary>
        /// The client system, if this is a client instance
        /// </summary>
        public EngineClient Client { get; }

        /// <summary>
        /// The server system
        /// Always exists, but only used when hosting servers (singleplayer, listen server, dedicated server)
        /// </summary>
        public EngineServer Server { get; }

        private readonly Stopwatch _engineTimeStopwatch = new Stopwatch();

        private readonly SnapshotTime _engineTime = new SnapshotTime();

        /// <summary>
        /// Gets the engine time
        /// </summary>
        public ITime EngineTime => _engineTime;

        /// <summary>
        /// The engine wide event system
        /// </summary>
        public IEventSystem EventSystem { get; } = new EventSystem();

        public PluginManager PluginManager { get; }

        /// <summary>
        /// The engine configuration
        /// </summary>
        public EngineConfiguration EngineConfiguration { get; }

        /// <summary>
        /// Gets the date that the engine was built
        /// </summary>
        public DateTimeOffset BuildDate { get; }

        /// <summary>
        /// Gets the log text writer used to forward logs to the console
        /// </summary>
        public ForwardingTextWriter LogTextWriter { get; }

        private readonly HostType _hostType;

        //Internal so the host can access it if needed
        internal ILogger Logger { get; }

        private bool _exiting;

        private double _desiredFrameLengthSeconds = 1.0 / DefaultFPS;

        private readonly IVariable<uint> _fpsMax;

        public Engine(HostType hostType, ICommandLine commandLine, string gameDirectory, EngineConfiguration engineConfiguration, ILogger logger, ForwardingTextWriter forwardingTextWriter)
        {
            _hostType = hostType;

            CommandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
            GameDirectory = gameDirectory ?? throw new ArgumentNullException(nameof(gameDirectory));
            EngineConfiguration = engineConfiguration ?? throw new ArgumentNullException(nameof(engineConfiguration));
            Log.Logger = Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            LogTextWriter = forwardingTextWriter ?? throw new ArgumentNullException(nameof(forwardingTextWriter));

            FileSystem = new DiskFileSystem();

            SetupFileSystem(GameDirectory);

            CommandSystem = new SharpLife.CommandSystem.CommandSystem(Logger, CultureInfo.InvariantCulture);

            EngineContext = CommandSystem.CreateContext("EngineContext");

            var startupState = new EngineStartupState(Logger, GameDirectory);

            //Add the engine assembly so builtin data gets added
            startupState.EntitySystemMetaData.AddAssembly(typeof(Engine).Assembly);

            //create the game window if this is a client
            if (_hostType == HostType.Client)
            {
                Client = new EngineClient(this, startupState);
            }

            Server = new EngineServer(this, Logger, startupState);

            PluginManager = startupState.PluginManager.Build();

            //Automatically add in all plugin assemblies to the entity system
            foreach (var pluginAssembly in PluginManager.Assemblies)
            {
                startupState.EntitySystemMetaData.AddAssembly(pluginAssembly);
            }

            World = new WorldState(Logger, EventSystem, FileSystem, startupState.EntitySystemMetaData.Build());

            _engineTimeStopwatch.Start();

            EventUtils.RegisterEvents(EventSystem, new EngineEvents());

            EngineContext.AddStuffCmds(Logger, CommandLine);
            EngineContext.AddExec(Logger, FileSystem, ExecPathIDs);
            EngineContext.AddEcho(Logger);
            EngineContext.AddAlias(Logger);
            EngineContext.AddFind(Logger);
            EngineContext.AddHelp(Logger);

            _fpsMax = EngineContext.RegisterVariable(
                new VirtualVariableInfo<uint>("fps_max", DefaultFPS)
                .WithHelpInfo("Sets the maximum frames per second")
                .WithChangeHandler((ref VariableChangeEvent<uint> @event) =>
                {
                    @event.Value = Math.Min(@event.Value, MaximumFPS);

                    var desiredFPS = @event.Value;

                    if (desiredFPS == 0)
                    {
                        desiredFPS = MaximumFPS;
                    }

                    _desiredFrameLengthSeconds = 1.0 / desiredFPS;
                }));

            EngineContext.RegisterVariable("engine_builddate", () => BuildDate, "The engine's build date");

            EngineContext.RegisterCommand(new CommandInfo("map", StartNewMap).WithHelpInfo("Loads the specified map"));

            //Get the build date from the generated resource file
            var assembly = typeof(Engine).Assembly;
            using (var reader = new StreamReader(assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.BuildDate.txt")))
            {
                string buildTimestamp = reader.ReadToEnd();

                BuildDate = DateTimeOffset.Parse(buildTimestamp);

                Logger.Information($"Exe: {BuildDate.ToString("HH:mm:ss MMM dd yyyy")}");
            }

            //TODO: initialize subsystems
        }

        public void Run()
        {
            double previousFrameSeconds = 0;

            while (!_exiting)
            {
                var currentFrameSeconds = _engineTimeStopwatch.ElapsedTicks / (double)Stopwatch.Frequency;
                double deltaSeconds = currentFrameSeconds - previousFrameSeconds;

                while (deltaSeconds < _desiredFrameLengthSeconds)
                {
                    currentFrameSeconds = _engineTimeStopwatch.ElapsedTicks / (double)Stopwatch.Frequency;
                    deltaSeconds = currentFrameSeconds - previousFrameSeconds;
                }

                //TODO: need to provide a way to query real time
                //TODO: need to properly handle frame time calculation
                //TODO: engine time is advanced after physics in the original engine
                _engineTime.FrameTime = currentFrameSeconds - previousFrameSeconds;

                //Engine time is relative, so advance by frame time
                _engineTime.ElapsedTime += _engineTime.FrameTime;

                previousFrameSeconds = currentFrameSeconds;

                Client?.SleepUntilInput(0);

                Update((float)deltaSeconds);

                if (_exiting)
                {
                    break;
                }

                Client?.Draw();
            }

            Shutdown();
        }

        /// <summary>
        /// Exit the game and shut it down
        /// </summary>
        public void Exit()
        {
            _exiting = true;
        }

        private void Update(float deltaSeconds)
        {
            CommandSystem.Execute();

            Client?.Update(deltaSeconds);
        }

        private void Shutdown()
        {
            Server.Shutdown();

            Client?.Shutdown();

            EventUtils.UnregisterEvents(EventSystem, new EngineEvents());

            EngineContext.Dispose();
            CommandSystem.Dispose();
        }

        private void SetupFileSystem(string gameDirectory)
        {
            //Note: the engine has no-Steam directory paths used for testing, but since this is Steam-only, we won't add those
            FileSystem.RemoveAllSearchPaths();

            //Strip off the exe name
            var baseDir = Path.GetDirectoryName(CommandLine[0]);

            FileSystem.SetupFileSystem(
                baseDir,
                EngineConfiguration.DefaultGame,
                gameDirectory,
                Framework.DefaultLanguage,
                Framework.DefaultLanguage,
                false,
                !CommandLine.Contains("-nohdmodels") && EngineConfiguration.EnableHDModels,
                CommandLine.Contains("-addons") || EngineConfiguration.EnableAddonsFolder);
        }

        /// <summary>
        /// Clear map specific memory
        /// </summary>
        private void ClearMemory()
        {
            //Done here so server and client don't wipe eachother's data while loading
            World.Clear();
        }

        /// <summary>
        /// Start a new map, loading entities from the map entity data string
        /// </summary>
        /// <param name="command"></param>
        private void StartNewMap(ICommandArgs command)
        {
            if (command.Count == 0)
            {
                Logger.Information("map <levelname> : changes server to specified map");
                return;
            }

            Client?.Disconnect(false);

            ClearMemory();

            var mapName = command[0];

            //Remove BSP extension
            if (mapName.EndsWith(FileExtensionUtils.AsExtension(Framework.Extension.BSP)))
            {
                mapName = Path.GetFileNameWithoutExtension(mapName);
            }

            EventSystem.DispatchEvent(EngineEvents.EngineNewMapRequest);

            if (!World.IsMapValid(mapName))
            {
                Logger.Error($"map change failed: '{mapName}' not found on server.");
                return;
            }

            Server.Stop();

            EventSystem.DispatchEvent(EngineEvents.EngineStartingServer);

            //Reset time
            //TODO: define constant for initial time
            _engineTime.ElapsedTime = 1;
            _engineTime.FrameTime = 0;

            const ServerStartFlags flags = ServerStartFlags.None;

            if (!Server.Start(mapName, null, flags))
            {
                return;
            }

            Server.Activate();

            //Listen server hosts need to connect to their own server
            if (Client != null)
            {
                //Client.CommandContext.QueueCommands($"connect {NetAddresses.Local}");
                //TODO: set up client
                Client.LocalConnect();
            }
        }
    }
}
