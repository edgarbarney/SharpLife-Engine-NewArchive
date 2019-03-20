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

using SharpLife.Engine.Models;
using SharpLife.Engine.Models.BSP;
using System;
using System.Collections.Generic;

namespace SharpLife.Engine.Entities.Components
{
    public static class EntityComponentExtensions
    {
        //TODO: this should really be done better than this
#pragma warning disable RCS1213 // Remove unused member declaration.
        private delegate RenderableComponent RenderableFactory(Entity entity, IModel model);
#pragma warning restore RCS1213 // Remove unused member declaration.

        private static readonly IReadOnlyDictionary<Type, RenderableFactory> _renderableFactories = new Dictionary<Type, RenderableFactory>
        {
            [null] = (entity, __) =>
            {
                entity.RemoveComponents(typeof(RenderableComponent), true);
                return null;
            },

            [typeof(BSPModel)] = (entity, model) =>
            {
                var renderable = entity.GetOrCreateRenderable<BSPRenderableComponent>();

                renderable.BSPModel = (BSPModel)model;

                return renderable;
            }
        };

        private static TRenderableComponent GetOrCreateRenderable<TRenderableComponent>(this Entity entity)
            where TRenderableComponent : RenderableComponent
        {
            var renderable = entity.GetComponent<TRenderableComponent>();

            if (renderable == null)
            {
                entity.RemoveComponents(typeof(RenderableComponent), true);

                renderable = entity.AddComponent<TRenderableComponent>();
            }

            return renderable;
        }

        public static RenderableComponent SetModel(this Entity entity, string modelName)
        {
            var model = EntitySystem.Scene.Models.Load(modelName);

            var factory = _renderableFactories[model?.GetType()];

            return factory(entity, model);
        }
    }
}
