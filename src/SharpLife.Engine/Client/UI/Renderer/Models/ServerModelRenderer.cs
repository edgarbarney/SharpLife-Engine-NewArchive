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

using SharpLife.Engine.Client.UI.Renderer.Models.SPR;
using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Models.BSP.Rendering;
using SharpLife.Engine.Models.MDL.Rendering;

namespace SharpLife.Engine.Client.UI.Renderer.Models
{
    /// <summary>
    /// Dummy renderer that does not do anything
    /// </summary>
    internal sealed class ServerModelRenderer : IModelRenderer
    {
        public void AddRenderable(RenderableComponent renderable)
        {
        }

        public void RemoveRenderable(RenderableComponent renderable)
        {
        }

        public void RenderSpriteModel(ref SpriteModelRenderData renderData)
        {
        }

        public void RenderStudioModel(ref StudioModelRenderData renderData)
        {
        }

        public void RenderBrushModel(ref BrushModelRenderData renderData)
        {
        }
    }
}
