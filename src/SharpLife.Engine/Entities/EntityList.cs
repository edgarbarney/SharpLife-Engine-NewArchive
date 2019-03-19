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

using System;
using System.Collections.Generic;

namespace SharpLife.Engine.Entities
{
    public sealed class EntityList
    {
        private readonly List<EntityInfo> _entities = new List<EntityInfo>();

        public int HighestIndex { get; private set; }

        public Entity this[int index]
        {
            get
            {
                if (index < 0 || index >= HighestIndex)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _entities[index].Entity;
            }
        }

        public Entity CreateEntity()
        {
            var info = GetFreeInfo();

            info.Entity = new Entity(info.Index);

            if (HighestIndex < info.Index)
            {
                HighestIndex = info.Index;
            }

            return info.Entity;
        }

        private EntityInfo GetFreeInfo()
        {
            foreach (var info in _entities)
            {
                if (info.Entity == null)
                {
                    return info;
                }
            }

            //No free slots, add some
            {
                var info = new EntityInfo(_entities.Count);

                _entities.Add(info);

                return info;
            }
        }

        internal void DestroyEntity(Entity entity)
        {
            //Ignore repeated destroy calls
            if (entity.Destroyed)
            {
                return;
            }

            //The id should always be valid since only we can create entities
            var info = _entities[entity._id];

            //This should never happen
            if (!ReferenceEquals(info.Entity, entity))
            {
                throw new ArgumentException("Attempting to remove entity with invalid index", nameof(entity));
            }

            //Remove this first to allow new entities to use this slot if they are created by component destruction logic
            info.Entity = null;

            if (HighestIndex == info.Index)
            {
                //Find the next highest index in use
                while (--HighestIndex > 0)
                {
                    if (_entities[HighestIndex].Entity != null)
                    {
                        break;
                    }
                }
            }

            entity.Destroyed = true;

            entity.RemoveAllComponents();
        }
    }
}
