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

namespace SharpLife.Engine.Entities.KeyValues.Converters
{
    public sealed class EnumConverter : IKeyValueConverter
    {
        private static readonly Dictionary<Type, object> _defaultValues = new Dictionary<Type, object>();

        private object GetDefaultValue(Type enumType)
        {
            if (!_defaultValues.TryGetValue(enumType, out var result))
            {
                //If it has 0, use that, otherwise use first listed value, which should be 0 if it exists
                if (Enum.IsDefined(enumType, 0))
                {
                    result = 0;
                }
                else
                {
                    var names = Enum.GetNames(enumType);

                    if (names.Length == 0)
                    {
                        throw new InvalidOperationException($"Enum {enumType.FullName} has no values but is used as a keyvalue");
                    }

                    result = Enum.Parse(enumType, names[0]);
                }

                _defaultValues.Add(enumType, result);
            }

            return result;
        }

        public object FromString(Type destinationType, string key, string value)
        {
            //TODO: uses int as its parsing type, may need something more flexible
            return KeyValueUtils.ParseEnum(value, destinationType, GetDefaultValue(destinationType));
        }
    }
}
