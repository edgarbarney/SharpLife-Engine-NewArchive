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
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Display;
using SharpLife.CommandSystem;
using SharpLife.CommandSystem.Commands;
using SharpLife.CommandSystem.Commands.VariableFilters;
using SharpLife.Engine.Shared;
using SharpLife.Engine.Shared.CommandSystem;
using SharpLife.Engine.Shared.Configuration;
using SharpLife.Engine.Shared.Events;
using SharpLife.Engine.Shared.Logging;
using SharpLife.Engine.Shared.Loop;
using SharpLife.Engine.Shared.UI;
using SharpLife.FileSystem;
using SharpLife.Models;
using SharpLife.Utility;
using SharpLife.Utility.Events;
using SharpLife.Utility.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace SharpLife.Engine.Host
{
    /// <summary>
    /// Manages top level engine components (client, server) and shared components
    /// Can host clients, dedicated servers and clients running listen servers
    /// </summary>
    internal class ClientServerEngine : IEngineLoop
    {
        private static readonly List<string> CommandLineKeyPrefixes = new List<string> { "-", "+" };

        private static readonly List<string> ExecPathIDs = new List<string>
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
        /// Gets the user interface component
        /// This component is optional and should be created only if needed
        /// </summary>
        public IUserInterface UserInterface { get; private set; }

        private readonly Stopwatch _engineTimeStopwatch = new Stopwatch();

        private readonly SnapshotTime _engineTime = new SnapshotTime();

        /// <summary>
        /// Gets the engine time
        /// </summary>
        public ITime EngineTime => _engineTime;

        public IModelManager ModelManager { get; }

        /// <summary>
        /// The engine wide event system
        /// </summary>
        public IEventSystem EventSystem { get; } = new EventSystem();

        /// <summary>
        /// The engine configuration
        /// </summary>
        public EngineConfiguration EngineConfiguration { get; }

        /// <summary>
        /// Gets the date that the engine was built
        /// </summary>
        public DateTimeOffset BuildDate { get; }

        /// <summary>
        /// Whether this is a dedicated server
        /// </summary>
        public bool IsDedicatedServer => _hostType == HostType.DedicatedServer;

        /// <summary>
        /// Gets the log text writer used to forward logs to the console
        /// </summary>
        public ForwardingTextWriter LogTextWriter { get; } = new ForwardingTextWriter();

        private readonly HostType _hostType;

        //Internal so the host can access it if needed
        internal ILogger Logger { get; }

        private bool _exiting;

        public bool Exiting
        {
            get => _exiting;

            set
            {
                //Don't allow continuing loop once exit has been signalled
                if (!_exiting)
                {
                    _exiting = value;
                }
            }
        }

        private double _desiredFrameLengthSeconds = 1.0 / DefaultFPS;

        private readonly IVariable _fpsMax;

        /// <summary>
        /// Creates the user interface if it does not exist
        /// </summary>
        public IUserInterface CreateUserInterface()
        {
            if (UserInterface == null)
            {
                UserInterface = new UserInterface(Logger, FileSystem, this, CommandLine.Contains("-noontop"));
            }

            return UserInterface;
        }

        public ClientServerEngine(string[] args, HostType hostType)
        {
            _hostType = hostType;

            CommandLine = new CommandLine(args, CommandLineKeyPrefixes);

            GameDirectory = CommandLine.GetValue("-game");

            //This can't actually happen since SharpLife loads from its own directory, so unless somebody placed the installation in the default game directory this isn't an issue
            //It's an easy way to verify that nothing went wrong during user setup though
            if (GameDirectory == null)
            {
                throw new InvalidOperationException("No game directory specified, cannot continue");
            }

            EngineConfiguration = LoadEngineConfiguration(GameDirectory);

            Log.Logger = Logger = CreateLogger(GameDirectory);

            _engineTimeStopwatch.Start();

            EventUtils.RegisterEvents(EventSystem, new EngineEvents());

            FileSystem = new DiskFileSystem();

            SetupFileSystem(GameDirectory);

            CommandSystem = new CommandSystem.CommandSystem(Logger);

            CommonCommands.AddStuffCmds(CommandSystem.SharedContext, Logger, CommandLine);
            CommonCommands.AddExec(CommandSystem.SharedContext, Logger, FileSystem, ExecPathIDs);
            CommonCommands.AddEcho(CommandSystem.SharedContext, Logger);
            CommonCommands.AddAlias(CommandSystem.SharedContext, Logger);

            _fpsMax = CommandSystem.SharedContext.RegisterVariable(
                new VariableInfo("fps_max")
                .WithValue(DefaultFPS)
                .WithHelpInfo("Sets the maximum frames per second")
                .WithNumberFilter(true)
                //Avoid negative maximum
                .WithMinMaxFilter(0, MaximumFPS)
                .WithChangeHandler((ref VariableChangeEvent @event) =>
                {
                    var desiredFPS = @event.Integer;

                    if (desiredFPS == 0)
                    {
                        desiredFPS = MaximumFPS;
                    }
                    _desiredFrameLengthSeconds = 1.0 / desiredFPS;
                }));

            //Get the build date from the generated resource file
            var assembly = typeof(ClientServerEngine).Assembly;
            using (var reader = new StreamReader(assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.BuildDate.txt")))
            {
                string buildTimestamp = reader.ReadToEnd();

                BuildDate = DateTimeOffset.Parse(buildTimestamp);

                Logger.Information($"Exe: {BuildDate.ToString("HH:mm:ss MMM dd yyyy")}");
            }

            ModelManager = new ModelManager(FileSystem);

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

                UserInterface?.SleepUntilInput(0);

                Update((float)deltaSeconds);

                if (_exiting)
                {
                    break;
                }
            }

            Shutdown();
        }

        private void Update(float deltaSeconds)
        {
            CommandSystem.Execute();
        }

        private static EngineConfiguration LoadEngineConfiguration(string gameDirectory)
        {
            EngineConfiguration engineConfiguration;

            using (var stream = new FileStream($"{gameDirectory}/cfg/SharpLife-Engine.xml", FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(EngineConfiguration));

                engineConfiguration = (EngineConfiguration)serializer.Deserialize(stream);
            }

            if (string.IsNullOrWhiteSpace(engineConfiguration.DefaultGame))
            {
                throw new InvalidOperationException("Default game must be specified");
            }

            if (string.IsNullOrWhiteSpace(engineConfiguration.DefaultGameName))
            {
                throw new InvalidOperationException("Default game name must be specified");
            }

            //Use a default configuration if none was provided
            if (engineConfiguration.LoggingConfiguration == null)
            {
                engineConfiguration.LoggingConfiguration = new LoggingConfiguration();
            }

            return engineConfiguration;
        }

        private ILogger CreateLogger(string gameDirectory)
        {
            var config = new LoggerConfiguration();

            config.MinimumLevel.Verbose();

            ITextFormatter fileFormatter = null;

            switch (EngineConfiguration.LoggingConfiguration.LogFormat)
            {
                case LoggingConfiguration.Format.Text:
                    {
                        fileFormatter = new MessageTemplateTextFormatter("{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null);
                        break;
                    }

                case LoggingConfiguration.Format.CompactJSON:
                    {
                        fileFormatter = new CompactJsonFormatter();
                        break;
                    }
            }

            //Invalid config setting for RetainedFileCountLimit will throw
            config
                .WriteTo.File(fileFormatter, $"{gameDirectory}/logs/engine.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: EngineConfiguration.LoggingConfiguration.RetainedFileCountLimit);

            //Use basic formatting for console output
            var logFormatter = new MessageTemplateTextFormatter("{Message:lj}{NewLine}{Exception}", null);

            config.WriteTo.TextWriter(logFormatter, LogTextWriter);

            return config.CreateLogger();
        }

        private void Shutdown()
        {
            UserInterface?.Shutdown();

            EventUtils.UnregisterEvents(EventSystem, new EngineEvents());
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
            ModelManager.Clear();
        }
    }
}
