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
using SharpLife.Engine.Models.BSP.FileFormat;
using SharpLife.Engine.Physics;
using SharpLife.Game.Entities.Components;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace SharpLife.Game.Entities.Factories.Doors
{
    [LinkEntityToFactory(ClassName = "func_door")]
    public sealed class FuncDoorFactory : EntityFactory
    {
        protected override void GetComponentTypes(ImmutableHashSet<Type>.Builder types)
        {
            types.Add(typeof(Transform));
            types.Add(typeof(Collider));
            types.Add(typeof(RenderProperties));
            types.Add(typeof(BSPRenderableComponent));
            types.Add(typeof(LinearMovementLocomotor));
            types.Add(typeof(LinearDoor));
        }

        public override bool Initialize(EntityCreator creator, Entity entity, IReadOnlyList<KeyValuePair<string, string>> keyValues)
        {
            var transform = entity.GetComponent<Transform>();

            if (!creator.InitializeComponent(transform, keyValues))
            {
                return false;
            }

            EntityUtils.SetMoveDir(transform);

            var collider = entity.GetComponent<Collider>();

            if (!creator.InitializeComponent(collider, keyValues))
            {
                return false;
            }

            if (!creator.InitializeComponent(entity.GetComponent<RenderProperties>(), keyValues))
            {
                return false;
            }

            if (!creator.InitializeComponent(entity.GetComponent<BSPRenderableComponent>(), keyValues))
            {
                return false;
            }

            var locomotor = entity.GetComponent<LinearMovementLocomotor>();

            if (!creator.InitializeComponent(locomotor, keyValues))
            {
                return false;
            }

            var door = entity.GetComponent<LinearDoor>();

            if (!creator.InitializeComponent(door, keyValues))
            {
                return false;
            }

            if (collider.Contents == Contents.Node)
            {
                //normal door
                if (door.Passable)
                {
                    collider.Solid = Solid.Not;
                }
                else
                {
                    collider.Solid = Solid.BSP;
                }
            }
            else
            {
                // special contents
                collider.Solid = Solid.Not;
                door.Silent = true; // water is silent for now
            }

            collider.MoveType = MoveType.Push;
            //TODO:
            //UTIL_SetOrigin(pev, pev->origin);
            //SET_MODEL(ENT(pev), STRING(pev->model));

            if (transform.Speed == 0)
            {
                transform.Speed = 100;
            }

            door.Position1 = transform.Origin;
            // Subtract 2 from size because the engine expands bboxes by 1 in all directions making the size too big
            door.Position2 = door.Position1 + (transform.MoveDirection
                * (Math.Abs(transform.MoveDirection.X * (collider.Size.X - 2))
                + Math.Abs(transform.MoveDirection.Y * (collider.Size.Y - 2))
                + Math.Abs(transform.MoveDirection.Z * (collider.Size.Z - 2))
                - door.Lip));

            Debug.Assert(door.Position1 != door.Position2, "door start/end positions are equal");

            if (door.StartsOpen)
            {
                // swap pos1 and pos2, put door at pos2
                transform.Origin = door.Position2;
                door.Position2 = door.Position1;
                door.Position1 = transform.Origin;
            }

            return true;
        }
    }
}
