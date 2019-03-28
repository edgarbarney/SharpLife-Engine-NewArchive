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

using SharpLife.Engine.Entities.KeyValues;
using SharpLife.Engine.ObjectEditor;
using System.Numerics;

namespace SharpLife.Engine.Entities.Components
{
    public sealed class Transform : Component
    {
        private Transform _parent;

        private Transform _firstChild;

        private Transform _nextSibling;

        [ObjectEditorVisible(Visible = false)]
        public Transform Parent
        {
            get => _parent;

            set
            {
                _parent?.InternalDetachChild(this);

                _parent = value;

                _parent?.InternalAttachChild(this);
            }
        }

        public Vector3 RelativeOrigin { get; set; }

        [KeyValue(Name = "origin")]
        public Vector3 Origin
        {
            get => ComputeAbsoluteOrigin();

            set
            {
                if (Parent != null)
                {
                    RelativeOrigin = value - Parent.Origin;
                }
                else
                {
                    RelativeOrigin = value;
                }
            }
        }

        [ObjectEditorVector3(DisplayFormat = Vector3DisplayFormat.AnglesDegrees)]
        public Vector3 RelativeAngles { get; set; }

        //TODO: implement angle calculations
        [KeyValue(Name = "angles")]
        [ObjectEditorVector3(DisplayFormat = Vector3DisplayFormat.AnglesDegrees)]
        public Vector3 Angles { get; set; }

        //TODO: needs relativebasevelocity?
        public Vector3 BaseVelocity;

        //TODO: implement
        public Vector3 RelativeVelocity
        {
            get => Velocity;
            set => Velocity = value;
        }

        [KeyValue(Name = "velocity")]
        public Vector3 Velocity;

        [KeyValue(Name = "avelocity")]
        public Vector3 AngularVelocity { get; set; }

        [KeyValue(Name = "scale")]
        public float Scale { get; set; }

        public Vector3 ScaleVector => Scale == 0 ? Vector3.One : new Vector3(Scale);

        //TODO: these should probably be isolated to specific components
        public float Speed;

        public Vector3 MoveDirection;

        public Vector3 ViewOffset;

        public FixAngleMode FixAngle;

        public void OnDisable()
        {
            //Detach from hierarchy
            Parent = null;

            //Orphan all children
            DetachAllChildren();
        }

        public bool IsAncestor(Transform transform)
        {
            for (var parent = _parent; parent != null; parent = parent._parent)
            {
                if (ReferenceEquals(parent, transform))
                {
                    return true;
                }
            }

            return false;
        }

        private bool InternalAttachChild(Transform child)
        {
            //Determine if the child is one of our ancestors
            if (IsAncestor(child))
            {
                //TODO: Log error
                return false;
            }

            if (child.Parent != null)
            {
                if (ReferenceEquals(child.Parent, this))
                {
                    return false;
                }

                child.Parent.InternalDetachChild(child);
            }

            child._nextSibling = _firstChild;
            _firstChild = child;

            return true;
        }

        private bool InternalDetachChild(Transform child)
        {
            if (!ReferenceEquals(child.Parent, this))
            {
                return false;
            }

            Transform previous = null;

            for (var transform = _firstChild; transform != null; transform = transform._nextSibling)
            {
                if (ReferenceEquals(transform, child))
                {
                    if (previous != null)
                    {
                        previous._nextSibling = transform._nextSibling;
                    }
                    else
                    {
                        _firstChild = transform._nextSibling;
                    }

                    break;
                }

                previous = transform;
            }

            return true;
        }

        public void AttachChild(Transform child)
        {
            if (InternalAttachChild(child))
            {
                child._parent = this;
            }
        }

        public void DetachChild(Transform child)
        {
            if (InternalDetachChild(child))
            {
                child._parent = null;
            }
        }

        public void DetachAllChildren()
        {
            if (_firstChild != null)
            {
                Transform next = null;

                for (var child = _firstChild; child != null; child = next)
                {
                    next = child._nextSibling;

                    child._nextSibling = null;
                }

                _firstChild = null;
            }
        }

        private Vector3 ComputeAbsoluteOrigin()
        {
            var origin = RelativeOrigin;

            if (_parent != null)
            {
                origin += _parent.Origin;
            }

            return origin;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        public ref struct Enumerator
        {
            private readonly Transform _transform;

            private Transform _current;
            private Transform _next;

            public Transform Current => _current;

            public Enumerator(Transform transform)
            {
                _transform = transform;
                _current = null;
                _next = _transform._firstChild;
            }

            public bool MoveNext()
            {
                if (_next == null)
                {
                    return false;
                }

                _current = _next;

                if (_current != null)
                {
                    _next = _current._nextSibling;
                    return true;
                }

                _next = null;
                return false;
            }
        }
    }
}
