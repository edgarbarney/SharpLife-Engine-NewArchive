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
using SharpLife.Engine.Physics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpLife.Game.Entities.Factories
{
    [LinkEntityToFactory(ClassName = "worldspawn")]
    public sealed class WorldFactory : EntityFactory
    {
        protected override void GetComponentTypes(ImmutableHashSet<Type>.Builder types)
        {
            types.Add(typeof(Transform));
            types.Add(typeof(Collider));
            types.Add(typeof(BSPRenderableComponent));
        }

        public override bool Initialize(EntityCreator creator, Entity entity, IReadOnlyList<KeyValuePair<string, string>> keyValues)
        {
            if (!creator.InitializeComponent(entity.GetComponent<Transform>(), keyValues))
            {
                return false;
            }

            if (!creator.InitializeComponent(entity.GetComponent<Collider>(), keyValues))
            {
                return false;
            }

            var bspRenderable = entity.GetComponent<BSPRenderableComponent>();

            if (creator.Scene.WorldState.MapInfo.Model == null)
            {
                creator.Logger.Error("Missing world model for worldspawn. This indicates a serious problem with the engine");
                return false;
            }

            bspRenderable.BSPModel = creator.Scene.WorldState.MapInfo.Model;

            return true;
        }
    }
}
