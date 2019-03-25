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

namespace SharpLife.Engine.Entities
{
    /// <summary>
    /// Represents entities in the world
    /// </summary>
    public sealed partial class Entity : IEquatable<Entity>
    {
        public uint Id { get; }

        public bool Destroyed { get; internal set; }

        public string ClassName { get; set; }

        public string TargetName { get; set; }

        internal Entity(uint id)
        {
            Id = id;
        }

        public void Destroy() => EntitySystem.Scene.DestroyEntity(this);

        //Enforce reference equality for entities
        public bool Equals(Entity other) => ReferenceEquals(this, other);

        public override bool Equals(object obj) => ReferenceEquals(this, obj);

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return $"Entity {Id}:{ClassName}:{TargetName}";
        }
    }
}
