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
using SharpLife.Engine.Models.MDL;
using SharpLife.Game.Entities.Factories.Animation;
using System.Collections.Generic;

namespace SharpLife.Game.Entities.Factories.NPCs
{
    [LinkEntityToFactory(ClassName = "monster_human_grunt")]
    public sealed class HumanGruntFactory : BaseAnimatingFactory
    {
        public override bool Initialize(EntityCreator creator, Entity entity, IReadOnlyList<KeyValuePair<string, string>> keyValues)
        {
            if (!base.Initialize(creator, entity, keyValues))
            {
                return false;
            }

            var renderable = entity.GetComponent<StudioRenderableComponent>();

            //Allow custom models to override
            if (renderable.StudioModel == null)
            {
                renderable.TrySetModel("models/hgrunt.mdl");
            }

            renderable.FrameRate = 1;

            return true;
        }
    }
}
