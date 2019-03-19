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

using SharpLife.Engine.Shared.Entities.Components;
using System;
using System.Collections.Generic;

namespace SharpLife.Engine.Shared.Entities.Factories
{
    /// <summary>
    /// Scene-specific factory to create entity instances by classname
    /// </summary>
    public sealed class EntityCreator
    {
        public Scene Scene { get; }

        public EntityCreator(Scene scene)
        {
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
        }

        private Entity CreateUninitializedEntity(EntityFactory factory)
        {
            var entity = Scene.CreateEntity();

            entity.AddComponents(factory.ComponentTypes);

            return entity;
        }

        /// <summary>
        /// Try to create an entity in the given scene with the given class name
        /// </summary>
        /// <param name="className"></param>
        /// <param name="keyValues"></param>
        /// <param name="entity"></param>
        public bool TryCreateEntity(string className, IReadOnlyList<KeyValuePair<string, string>> keyValues, out Entity entity)
        {
            if (className == null)
            {
                throw new ArgumentNullException(nameof(className));
            }

            if (keyValues == null)
            {
                throw new ArgumentNullException(nameof(keyValues));
            }

            if (!Scene.EntitySystemMetaData.EntityFactories.TryGetValue(className, out var factory))
            {
                entity = null;
                return false;
            }

            entity = CreateUninitializedEntity(factory);

            if (!factory.Initialize(this, entity, keyValues))
            {
                Scene.DestroyEntity(entity);
                entity = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes a component's keyvalues from a list of keyvalues
        /// </summary>
        /// <param name="component"></param>
        /// <param name="keyValues"></param>
        public bool InitializeComponent(Component component, IReadOnlyList<KeyValuePair<string, string>> keyValues)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (keyValues == null)
            {
                throw new ArgumentNullException(nameof(keyValues));
            }

            foreach (var keyValue in keyValues)
            {
                if (component._metaData.KeyValues.TryGetValue(keyValue.Key, out var field))
                {
                    component._metaData.Accessor[component, field.Field.Name] = field.Converter.FromString(field.Field.FieldType, keyValue.Value);
                }
            }

            return true;
        }
    }
}
