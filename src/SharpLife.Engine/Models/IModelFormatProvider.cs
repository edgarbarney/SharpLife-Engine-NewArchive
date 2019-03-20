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
using SharpLife.Engine.Client.UI.Rendering;
using SharpLife.Engine.Client.UI.Rendering.Models;

namespace SharpLife.Engine.Models
{
    /// <summary>
    /// Provides methods to create parts needed to use a model format
    /// </summary>
    public interface IModelFormatProvider
    {
        /// <summary>
        /// Creates a renderer for this format
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="logger"></param>
        /// <param name="commandContext"></param>
        /// TODO: find better way to share objects
        IModelRenderer CreateRenderer(Scene scene, ILogger logger, ICommandContext commandContext);

        /// <summary>
        /// Create a loader capable of loading models of this format
        /// </summary>
        IModelLoader CreateLoader();
    }
}
