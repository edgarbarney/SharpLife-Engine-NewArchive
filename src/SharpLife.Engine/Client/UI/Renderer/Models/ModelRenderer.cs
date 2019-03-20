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

using SharpLife.Engine.Client.UI.Renderer.Models.MDL;
using SharpLife.Engine.Client.UI.Renderer.Models.SPR;
using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Models.BSP.Rendering;
using SharpLife.Engine.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace SharpLife.Engine.Client.UI.Renderer.Models
{
    /// <summary>
    /// Responsible for rendering models
    /// </summary>
    public sealed class ModelRenderer : IModelRenderer, IRenderable
    {
        private readonly IModelResourcesManager _resourcesManager;

        private bool _active;

        private RenderContext _renderContext;

        private readonly HashSet<RenderableComponent> _renderables = new HashSet<RenderableComponent>();

        public RenderPasses RenderPasses => RenderPasses.Standard;

        public SpriteModelRenderer SpriteRenderer { get; }

        public StudioModelRenderer StudioRenderer { get; }

        public BrushModelRenderer BrushRenderer { get; }

        public ModelRenderer(IModelResourcesManager resourcesManager,
            SpriteModelRenderer spriteRenderer,
            StudioModelRenderer studioRenderer,
            BrushModelRenderer brushRenderer)
        {
            _resourcesManager = resourcesManager ?? throw new ArgumentNullException(nameof(resourcesManager));

            SpriteRenderer = spriteRenderer ?? throw new ArgumentNullException(nameof(spriteRenderer));
            StudioRenderer = studioRenderer ?? throw new ArgumentNullException(nameof(studioRenderer));
            BrushRenderer = brushRenderer ?? throw new ArgumentNullException(nameof(brushRenderer));
        }

        public void AddRenderable(RenderableComponent renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            _renderables.Add(renderable);
        }

        public void RemoveRenderable(RenderableComponent renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            _renderables.Remove(renderable);
        }

        public void RenderSpriteModel(ref SpriteModelRenderData renderData)
        {
            if (renderData.Model == null)
            {
                throw new ArgumentNullException(nameof(renderData), $"{nameof(renderData.Model)} cannot be null");
            }

            if (!_active)
            {
                throw new InvalidOperationException($"Cannot call {nameof(RenderSpriteModel)} outside the render operation");
            }

            var resources = _resourcesManager.GetResources(renderData.Model);

            SpriteRenderer.Render(
                _renderContext.GraphicsDevice,
                _renderContext.CommandList,
                _renderContext.SceneContext,
                _renderContext.RenderPass,
                (SpriteModelResourceContainer)resources,
                ref renderData);
        }

        public void RenderStudioModel(ref StudioModelRenderData renderData)
        {
            if (renderData.Model == null)
            {
                throw new ArgumentNullException(nameof(renderData), $"{nameof(renderData.Model)} cannot be null");
            }

            if (!_active)
            {
                throw new InvalidOperationException($"Cannot call {nameof(RenderStudioModel)} outside the render operation");
            }

            var resources = _resourcesManager.GetResources(renderData.Model);

            StudioRenderer.Render(
                _renderContext.GraphicsDevice,
                _renderContext.CommandList,
                _renderContext.SceneContext,
                _renderContext.RenderPass,
                (StudioModelResourceContainer)resources,
                ref renderData);
        }

        public void RenderBrushModel(ref BrushModelRenderData renderData)
        {
            if (renderData.Model == null)
            {
                throw new ArgumentNullException(nameof(renderData), $"{nameof(renderData.Model)} cannot be null");
            }

            if (!_active)
            {
                throw new InvalidOperationException($"Cannot call {nameof(RenderBrushModel)} outside the render operation");
            }

            var resources = _resourcesManager.GetResources(renderData.Model);

            BrushRenderer.Render(
                _renderContext.GraphicsDevice,
                _renderContext.CommandList,
                _renderContext.SceneContext,
                _renderContext.RenderPass,
                (BSPModelResourceContainer)resources,
                ref renderData);
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return new RenderOrderKey();
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            _active = true;
            _renderContext = new RenderContext { GraphicsDevice = gd, CommandList = cl, SceneContext = sc, RenderPass = renderPass };

            foreach (var renderable in _renderables)
            {
                renderable.Render(this);
            }

            //TODO: render all entities known to the renderer
            //_renderModels(this, sc.ViewState);

            /*
            //TODO: let game render world?
            if (sc.Scene.WorldModel != null)
            {
                var data = new BrushModelRenderData
                {
                    Shared = new SharedModelRenderData
                    {
                        Index = 0,

                        Origin = Vector3.Zero,
                        Angles = Vector3.Zero,
                        Scale = Vector3.One,

                        Effects = EffectsFlags.None,

                        RenderMode = RenderMode.Normal,
                        RenderAmount = 0,
                        RenderColor = Vector3.Zero,
                        RenderFX = RenderFX.None,
                    },
                    Model = sc.Scene.WorldModel
                };

                RenderBrushModel(ref data);
            }
            */

            _renderContext = new RenderContext();
            _active = false;
        }
    }
}
