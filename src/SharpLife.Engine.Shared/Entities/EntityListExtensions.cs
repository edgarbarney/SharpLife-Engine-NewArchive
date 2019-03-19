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

namespace SharpLife.Engine.Shared.Entities
{
    public static class EntityListExtensions
    {
        /// <summary>
        /// Enumerates all entities
        /// </summary>
        public static EntitiesEnumerable EnumerateAll(this EntityList entities) => new EntitiesEnumerable(entities);

        public readonly ref struct EntitiesEnumerable
        {
            private readonly EntityList _entities;

            internal EntitiesEnumerable(EntityList entities)
            {
                _entities = entities;
            }

            public Enumerator GetEnumerator() => new Enumerator(_entities);

            public ref struct Enumerator
            {
                private readonly EntityList _list;

                private int _index;

                public Entity Current { get; private set; }

                internal Enumerator(EntityList list)
                {
                    _list = list;
                    _index = -1;
                    Current = null;
                }

                public bool MoveNext()
                {
                    //Once all entities have been enumerated, disable the enumerator to avoid edge cases
                    if (_index != -2 && _index + 1 < _list.HighestIndex)
                    {
                        ++_index;

                        while (_index < _list.HighestIndex)
                        {
                            var entity = _list[_index];

                            if (entity != null)
                            {
                                Current = entity;
                                return true;
                            }

                            ++_index;
                        }
                    }

                    _index = -2;
                    return false;
                }
            }
        }

        public static TargetNameEnumerable EnumerateNamedTargets(this EntityList entities, string targetName) => new TargetNameEnumerable(entities, targetName);

        public readonly ref struct TargetNameEnumerable
        {
            private readonly EntityList _entities;
            private readonly string _targetName;

            internal TargetNameEnumerable(EntityList entities, string targetName)
            {
                _entities = entities;
                _targetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
            }

            public Enumerator GetEnumerator() => new Enumerator(_entities, _targetName);

            public ref struct Enumerator
            {
                private EntitiesEnumerable.Enumerator _enumerator;

                private readonly string _targetName;

                public Entity Current => _enumerator.Current;

                internal Enumerator(EntityList list, string targetName)
                {
                    _enumerator = new EntitiesEnumerable.Enumerator(list);
                    _targetName = targetName;
                }

                public bool MoveNext()
                {
                    var result = _enumerator.MoveNext();

                    while (result && !_targetName.Equals(_enumerator.Current.TargetName))
                    {
                        result = _enumerator.MoveNext();
                    }

                    return result;
                }
            }
        }

        public static ComponentEnumerable EnumerateHasComponent(this EntityList entities, Type componentType) => new ComponentEnumerable(entities, componentType);

        public static ComponentEnumerable EnumerateHasComponent<TComponent>(this EntityList entities) where TComponent : Component => new ComponentEnumerable(entities, typeof(TComponent));

        public readonly ref struct ComponentEnumerable
        {
            private readonly EntityList _entities;
            private readonly Type _type;

            internal ComponentEnumerable(EntityList entities, Type type)
            {
                _entities = entities;
                _type = type ?? throw new ArgumentNullException(nameof(type));

                if (!typeof(Component).IsAssignableFrom(_type))
                {
                    throw new ArgumentException("The given type is not a component type");
                }
            }

            public Enumerator GetEnumerator() => new Enumerator(_entities, _type);

            public ref struct Enumerator
            {
                private EntitiesEnumerable.Enumerator _enumerator;

                private readonly Type _type;

                public Entity Current => _enumerator.Current;

                internal Enumerator(EntityList list, Type type)
                {
                    _enumerator = new EntitiesEnumerable.Enumerator(list);
                    _type = type;
                }

                public bool MoveNext()
                {
                    var result = _enumerator.MoveNext();

                    while (result && !_enumerator.Current.HasComponent(_type))
                    {
                        result = _enumerator.MoveNext();
                    }

                    return result;
                }
            }
        }

        public static TransformableEnumerable EnumerateTransformables(this EntityList entities) => new TransformableEnumerable(entities);

        public readonly ref struct TransformableEnumerable
        {
            private readonly EntityList _entities;

            internal TransformableEnumerable(EntityList entities)
            {
                _entities = entities;
            }

            public Enumerator GetEnumerator() => new Enumerator(_entities);

            public ref struct Enumerator
            {
                private EntitiesEnumerable.Enumerator _enumerator;

                public Entity Current => _enumerator.Current;

                internal Enumerator(EntityList list)
                {
                    _enumerator = new EntitiesEnumerable.Enumerator(list);
                }

                public bool MoveNext()
                {
                    var result = _enumerator.MoveNext();

                    while (result && !_enumerator.Current.HasComponent<Transform>())
                    {
                        result = _enumerator.MoveNext();
                    }

                    return result;
                }
            }
        }
    }
}
