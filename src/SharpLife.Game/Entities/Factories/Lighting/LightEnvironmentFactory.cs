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
using SharpLife.Engine.Entities.Factories;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SharpLife.Game.Entities.Factories.Lighting
{
    [LinkEntityToFactory(ClassName = "light_environment")]
    public class LightEnvironmentFactory : LightFactory
    {
        protected override void GetComponentTypes(ImmutableHashSet<Type>.Builder types)
        {
            base.GetComponentTypes(types);

            types.Add(typeof(LightEnvironment));
        }

        public override bool Initialize(EntityCreator creator, Entity entity, IReadOnlyList<KeyValuePair<string, string>> keyValues)
        {
            if (!base.Initialize(creator, entity, keyValues))
            {
                return false;
            }

            if (!creator.InitializeComponent(entity.GetComponent<LightEnvironment>(), keyValues))
            {
                return false;
            }

            return true;
        }
    }
}
