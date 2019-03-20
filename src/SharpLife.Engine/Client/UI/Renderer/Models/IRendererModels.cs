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

using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Models.BSP.Rendering;
using SharpLife.Engine.Models.MDL.Rendering;
using SharpLife.Engine.Models.SPR.Rendering;

namespace SharpLife.Engine.Client.UI.Renderer.Models
{
    /// <summary>
    /// Renders models
    /// </summary>
    public interface IRendererModels
    {
        TModelRenderer GetRenderer<TModelRenderer>() where TModelRenderer : class, IModelRenderer;

        void AddRenderable(RenderableComponent renderable);
        void RemoveRenderable(RenderableComponent renderable);

        void RenderSpriteModel(ref SpriteModelRenderData renderData);

        void RenderStudioModel(ref StudioModelRenderData renderData);

        void RenderBrushModel(ref BrushModelRenderData renderData);
    }
}
