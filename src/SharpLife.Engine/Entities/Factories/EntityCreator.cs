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

using Serilog;
using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Entities.KeyValues.Converters;
using System;
using System.Collections.Generic;

namespace SharpLife.Engine.Entities.Factories
{
    /// <summary>
    /// Scene-specific factory to create entity instances by classname
    /// </summary>
    public sealed class EntityCreator
    {
        public Scene Scene { get; }

        public ILogger Logger => Scene.Logger;

        public EntityCreator(Scene scene)
        {
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
        }

        private Entity CreateUninitializedEntity(string className, EntityFactory factory)
        {
            var entity = Scene.CreateEntity();

            entity.ClassName = className;

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

                Logger.Warning("No entity factory for class {ClassName}", className);
                return false;
            }

            entity = CreateUninitializedEntity(className, factory);

            entity.SendMessage(BuiltInComponentMethods.Initialize);

            if (!factory.Initialize(this, entity, keyValues))
            {
                Scene.DestroyEntity(entity);
                entity = null;

                Logger.Warning("Failed to initialize entity of class {ClassName}", className);
                return false;
            }

            //If the scene is already running, all components should be activated now
            if (EntitySystem.Scene.Running)
            {
                entity.SendMessage(BuiltInComponentMethods.Activate);
            }

            entity._activated = true;

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
                if (component.MetaData.KeyValues.TryGetValue(keyValue.Key, out var member))
                {
                    component.MetaData.Accessor[component, member.Member.Name] = member.Converter.FromString(member.MemberType, keyValue.Key, keyValue.Value);
                }

                //Special handling for spawnflags to remap them to booleans
                if (keyValue.Key == "spawnflags")
                {
                    var spawnFlags = KeyValueUtils.ParseInt(keyValue.Value);

                    foreach (var flag in component.MetaData.SpawnFlags)
                    {
                        //All flags are assumed to default to false, so only set to true here
                        if ((flag.Flag & spawnFlags) != 0)
                        {
                            component.MetaData.Accessor[component, flag.Member.Name] = true;
                        }
                    }
                }
            }

            return true;
        }
    }
}
