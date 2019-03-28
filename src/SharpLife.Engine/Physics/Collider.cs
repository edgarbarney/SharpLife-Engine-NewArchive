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
using SharpLife.Engine.Models.BSP.FileFormat;
using SharpLife.Engine.ObjectEditor;
using System;
using System.Numerics;

namespace SharpLife.Engine.Physics
{
    /// <summary>
    /// Represents the physical part of an entity
    /// </summary>
    public sealed class Collider : Component
    {
        private readonly short[] _leafNums = new short[PhysicsConstants.MaxLeafs];

        private OwnerComponent _owner;

        [ObjectEditorVisible(Visible = false)]
        public int LeafCount { get; private set; }

        [ObjectEditorVisible(Visible = false)]
        public int HeadNode { get; set; }

        public uint GroupInfo { get; set; }

        [ObjectEditorVisible(Visible = false)]
        public AreaNode Area { get; set; }

        //Cached components
        [ObjectEditorVisible(Visible = false)]
        public Transform Transform { get; private set; }

        [ObjectEditorVisible(Visible = false)]
        public RenderableComponent Renderable { get; private set; }

        /// <summary>
        /// Owner can be null if the entity does not support it
        /// </summary>
        [ObjectEditorVisible(Visible = false)]
        public Entity Owner
        {
            get => _owner?.Owner;

            set
            {
                if (_owner != null)
                {
                    _owner.Owner = value;
                }
            }
        }

        //Public API
        //TODO: need to see if these need to be changed into fields, maybe internal if only physics touches it
        public Solid Solid;

        public MoveType MoveType;

        [ObjectEditorVisible(Visible = false)]
        public Vector3 Mins;

        [ObjectEditorVisible(Visible = false)]
        public Vector3 Maxs;

        [ObjectEditorVisible(Visible = false)]
        public Vector3 AbsMin;

        [ObjectEditorVisible(Visible = false)]
        public Vector3 AbsMax;

        [ObjectEditorVisible(Visible = false)]
        public Vector3 Size;

        [ObjectEditorVisible(Visible = false)]
        public Collider GroundEntity;

        //Default to 1 so there is no need to check if it's 0
        //This does allow for 0 gravity, which is fine
        public float Gravity = 1;

        public float Friction;

        public float Buoyancy;

        public WaterLevel WaterLevel;

        public Contents WaterType;

        //Brush contents
        //TODO: move to another component?
        public Contents Contents;

        public float LastThinkTime;

        public float NextThink;

        public void Initialize()
        {
            Transform = Entity.GetComponent<Transform>();

            if (Transform == null)
            {
                EntitySystem.Scene.Logger.Warning($"Missing {nameof(Transform)} component for {nameof(Collider)}");
            }

            _owner = Entity.GetComponent<OwnerComponent>();

            Renderable = Entity.GetComponent<RenderableComponent>(true);

            if (Renderable == null)
            {
                EntitySystem.Scene.Logger.Warning($"Missing {nameof(RenderableComponent)} component for {nameof(Collider)}");
            }
        }

        public void Activate()
        {
            //This is tested now because Initialize happens before entity initialization
            if (Renderable.Model == null)
            {
                EntitySystem.Scene.Logger.Warning($"Missing model for {nameof(Collider)}");
            }
        }

        public short GetLeafNumber(int index) => _leafNums[index];

        public void AddLeafNumber(short number)
        {
            if (LeafCount < PhysicsConstants.MaxLeafs)
            {
                _leafNums[LeafCount++] = number;
            }
            else
            {
                LeafCount = PhysicsConstants.MaxLeafs + 1;
            }
        }

        public void ClearNodeState()
        {
            LeafCount = 0;
            HeadNode = -1;
        }

        public void MarkLeafCountOverflowed(int topNode)
        {
            LeafCount = 0;
            HeadNode = topNode;
            Array.Fill(_leafNums, (short)255);
        }

        public void CopyNodeStateFrom(Collider other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            HeadNode = other.HeadNode;
            LeafCount = other.LeafCount;

            Array.Copy(other._leafNums, _leafNums, _leafNums.Length);
        }

        /// <summary>
        /// Sets the size of the entity's bounds
        /// </summary>
        /// <param name="mins"></param>
        /// <param name="maxs"></param>
        public void SetSize(in Vector3 mins, in Vector3 maxs)
        {
            if (mins.X > maxs.X
                || mins.Y > maxs.Y
                || mins.Z > maxs.Z)
            {
                throw new InvalidOperationException("backwards mins/maxs");
            }

            Mins = mins;
            Maxs = maxs;
            Size = maxs - mins;

            EntitySystem.Scene.Physics.LinkEdict(this, false);
        }
    }
}
