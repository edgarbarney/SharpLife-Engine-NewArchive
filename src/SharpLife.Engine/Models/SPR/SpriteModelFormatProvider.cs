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
using SharpLife.Engine.Client.UI.Renderer;
using SharpLife.Engine.Client.UI.Renderer.Models;
using SharpLife.Engine.Models.SPR.Rendering;

namespace SharpLife.Engine.Models.SPR
{
    internal sealed class SpriteModelFormatProvider : IModelFormatProvider
    {
        public IModelRenderer CreateRenderer(Scene scene, ILogger logger, ICommandContext commandContext)
        {
            return new SpriteModelRenderer(scene, logger);
        }

        public IModelLoader CreateLoader()
        {
            return new SpriteModelLoader();
        }
    }
}
