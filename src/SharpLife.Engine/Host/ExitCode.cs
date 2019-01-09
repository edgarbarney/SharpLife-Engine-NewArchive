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

namespace SharpLife.Engine.Host
{
    /// <summary>
    /// Must match the ExitCode enum in the native wrapper
    /// </summary>
    public enum ExitCode
    {
        Success = 0,

        /// <summary>
        /// An unknown error not classified by this enum occurred
        /// </summary>
        UnknownError = 1,

        /// <summary>
        /// Only used by native wrapper, never return from engine
        /// </summary>
        WrapperError = 2,

        /// <summary>
        /// No command line arguments were passed into the engine, preventing startup and logging
        /// </summary>
        NoCommandLineArguments = 3,

        /// <summary>
        /// An unhandled exception occurred and was caught by the launcher
        /// </summary>
        UnhandledException = 4,
    }
}
