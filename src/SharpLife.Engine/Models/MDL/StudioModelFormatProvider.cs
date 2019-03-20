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
using SharpLife.Engine.Models.MDL.Rendering;

namespace SharpLife.Engine.Models.MDL
{
    internal sealed class StudioModelFormatProvider : IModelFormatProvider
    {
        public IModelRenderer CreateRenderer(Scene scene, ILogger logger, ICommandContext commandContext)
        {
            return new StudioModelRenderer(scene, commandContext);
        }

        public IModelLoader CreateLoader()
        {
            return new StudioModelLoader();
        }
    }
}
