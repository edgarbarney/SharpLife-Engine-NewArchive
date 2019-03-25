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

using SharpLife.Engine.Entities;
using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Entities.Factories;
using SharpLife.Engine.Models.BSP;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SharpLife.Game.Entities.Factories.BrushModels
{
    [LinkEntityToFactory(ClassName = "func_wall")]
    public sealed class FuncWallFactory : EntityFactory
    {
        protected override void GetComponentTypes(ImmutableHashSet<Type>.Builder types)
        {
            types.Add(typeof(Transform));
            types.Add(typeof(BSPRenderableComponent));
        }

        public override bool Initialize(EntityCreator creator, Entity entity, IReadOnlyList<KeyValuePair<string, string>> keyValues)
        {
            if (!creator.InitializeComponent(entity.GetComponent<Transform>(), keyValues))
            {
                return false;
            }

            var renderable = entity.GetComponent<BSPRenderableComponent>();

            //TODO: need to refactor this
            var modelName = keyValues.FirstOrDefault(p => p.Key == "model").Value;

            if (modelName == null)
            {
                creator.Logger.Warning("No model for env_sprite");
                return false;
            }

            var model = EntitySystem.Scene.Models.Load(modelName);

            if (!(model is BSPModel brush))
            {
                creator.Logger.Warning("Model has wrong format");
                return false;
            }

            renderable.BSPModel = brush;

            if (!creator.InitializeComponent(renderable, keyValues))
            {
                return false;
            }

            return true;
        }
    }
}
