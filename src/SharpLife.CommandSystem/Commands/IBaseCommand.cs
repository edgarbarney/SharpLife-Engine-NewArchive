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

using System.Text;

namespace SharpLife.CommandSystem.Commands
{
    public interface IBaseCommand
    {
        string Name { get; }

        CommandFlags Flags { get; }

        /// <summary>
        /// Gets user defined flags
        /// </summary>
        uint UserFlags { get; }

        string HelpInfo { get; }

        object Tag { get; }

        /// <summary>
        /// Write information about the command to a given builder
        /// This should be information that describes only instance-specific elements
        /// </summary>
        /// <param name="builder"></param>
        void WriteCommandInfo(StringBuilder builder);
    }
}
