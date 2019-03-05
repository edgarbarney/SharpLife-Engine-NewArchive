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
using SharpLife.Engine.Shared.Configuration;
using SharpLife.Engine.Shared.Logging;
using SharpLife.Utility;
using System.Collections.Generic;
using System.IO;

namespace SharpLife.Engine.Host
{
    /// <summary>
    /// Given a command line and host type, launches an engine that can be run
    /// </summary>
    internal sealed class Launcher
    {
        private const string ErrorGameDirectory = "sharplife_error";

        private static readonly List<string> CommandLineKeyPrefixes = new List<string> { "-", "+" };

        private ICommandLine _commandLine;

        private string _gameDirectory;

        private EngineConfiguration _engineConfiguration;

        public ILogger Logger { get; private set; }

        private readonly ForwardingTextWriter _logTextWriter = new ForwardingTextWriter();

        public Engine Launch(string[] args, HostType hostType)
        {
            //This two stage setup is required to ensure that a logger always exists

            _commandLine = new CommandLine(args, CommandLineKeyPrefixes);

            _gameDirectory = _commandLine.GetValue("-game");

            //This can't actually happen since SharpLife loads from its own directory, so unless somebody placed the installation in the default game directory this isn't an issue
            //It's an easy way to verify that nothing went wrong during user setup though
            if (_gameDirectory == null)
            {
                FatalError("No game directory specified, cannot continue");
            }

            _engineConfiguration = LoadEngineConfiguration(_gameDirectory);

            Logger = CreateLogger(_gameDirectory, _engineConfiguration.LoggingConfiguration);

            //Now that the logger has been set up the engine can take care of the rest
            return new Engine(hostType, _commandLine, _gameDirectory, _engineConfiguration, Logger, _logTextWriter);
        }

        private EngineConfiguration LoadEngineConfiguration(string gameDirectory)
        {
            EngineConfiguration engineConfiguration;

            using (var stream = new FileStream($"{gameDirectory}/cfg/SharpLife-Engine.xml", FileMode.Open))
            {
                engineConfiguration = (EngineConfiguration)EngineConfiguration.Serializer.Deserialize(stream);
            }

            if (string.IsNullOrWhiteSpace(engineConfiguration.DefaultGame))
            {
                FatalError("Default game must be specified");
            }

            if (string.IsNullOrWhiteSpace(engineConfiguration.DefaultGameName))
            {
                FatalError("Default game name must be specified");
            }

            //Use a default configuration if none was provided
            if (engineConfiguration.LoggingConfiguration == null)
            {
                engineConfiguration.LoggingConfiguration = new LoggingConfiguration();
            }

            return engineConfiguration;
        }

        private ILogger CreateLogger(string gameDirectory, LoggingConfiguration loggingConfiguration)
        {
            var config = new LoggerConfiguration();

            config.MinimumLevel.Verbose();

            ITextFormatter fileFormatter = null;

            switch (loggingConfiguration.LogFormat)
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
                retainedFileCountLimit: loggingConfiguration.RetainedFileCountLimit);

            //Use basic formatting for console output
            var logFormatter = new MessageTemplateTextFormatter("{Message:lj}{NewLine}{Exception}", null);

            config.WriteTo.TextWriter(logFormatter, _logTextWriter);

            return config.CreateLogger();
        }

        /// <summary>
        /// Creates a logger that outputs to a hardcoded path
        /// </summary>
        private void CreateErrorLogger()
        {
            //Create the configuration if we didn't load it yet
            Logger = CreateLogger(ErrorGameDirectory, _engineConfiguration?.LoggingConfiguration ?? new LoggingConfiguration());
        }

        private void FatalError(string reason)
        {
            if (Logger == null)
            {
                CreateErrorLogger();
            }

            throw new EngineStartupException(reason ?? "Unknown error");
        }

        /// <summary>
        /// In the event that logger creation fails this will allow an error to be logged to a fallback file
        /// </summary>
        /// <param name="message"></param>
        public void FallbackErrorLog(string message)
        {
            File.AppendAllText($"{ErrorGameDirectory}/logs/error.log", message);
        }
    }
}
