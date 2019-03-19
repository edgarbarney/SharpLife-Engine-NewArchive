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
using System.Collections.Immutable;

namespace SharpLife.Engine.Entities.Factories
{
    /// <summary>
    /// Constructs an entity from a list of keyvalues
    /// </summary>
    public abstract class EntityFactory
    {
        private ImmutableHashSet<Type> _componentTypes;

        public ImmutableHashSet<Type> ComponentTypes
        {
            get
            {
                if (_componentTypes == null)
                {
                    var builder = ImmutableHashSet.CreateBuilder<Type>();

                    GetComponentTypes(builder);

                    _componentTypes = builder.ToImmutable();
                }

                return _componentTypes;
            }
        }

        /// <summary>
        /// Implementing classes must override this to add all component types that the entity uses
        /// </summary>
        /// <param name="types"></param>
        protected abstract void GetComponentTypes(ImmutableHashSet<Type>.Builder types);

        /// <summary>
        /// Given an entity with all components specified by <see cref="GetComponentTypes(ImmutableHashSet{Type}.Builder)"/> and a list of keyvalues, initialize all components
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="entity"></param>
        /// <param name="keyvalues"></param>
        /// <returns>Whether initialization succeeded</returns>
        public abstract bool Initialize(EntityCreator creator, Entity entity, IReadOnlyList<KeyValuePair<string, string>> keyvalues);
    }
}
