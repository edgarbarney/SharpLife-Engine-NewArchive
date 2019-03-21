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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace SharpLife.Engine.Client.UI.Rendering.Models
{
    /// <summary>
    /// Responsible for rendering models
    /// </summary>
    public sealed class RendererModels : IRendererModels, IRenderable
    {
        private readonly IModelRenderer[] _renderers;

        private readonly HashSet<RenderableComponent> _renderables = new HashSet<RenderableComponent>();

        public RenderPasses RenderPasses => RenderPasses.Standard;

        public RendererModels(IEnumerable<IModelRenderer> renderers)
        {
            if (renderers == null)
            {
                throw new ArgumentNullException(nameof(renderers));
            }

            _renderers = renderers.ToArray();
        }

        public TModelRenderer GetRenderer<TModelRenderer>() where TModelRenderer : class, IModelRenderer
        {
            //TODO: may need to make a dictionary to speed this up
            foreach (var renderer in _renderers)
            {
                if (renderer is TModelRenderer modelRenderer)
                {
                    return modelRenderer;
                }
            }

            throw new ArgumentException($"Could not find model renderer of type {typeof(TModelRenderer).FullName}", nameof(TModelRenderer));
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

        public RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return new RenderOrderKey();
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            var renderContext = new RenderContext(gd, cl, sc, renderPass);

            foreach (var renderable in _renderables)
            {
                renderable.Render(this, renderContext);
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
        }
    }
}
