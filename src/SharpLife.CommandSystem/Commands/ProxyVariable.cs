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

using FastMember;
using SharpLife.CommandSystem.TypeProxies;
using System.Reflection;

namespace SharpLife.CommandSystem.Commands
{
    /// <summary>
    /// A variable that acts as a proxy for a variable or property in an object
    /// </summary>
    internal sealed class ProxyVariable<T> : BaseVariable<T>
    {
        private readonly ObjectAccessor _accessor;
        private readonly MemberInfo _member;

        public override T Value
        {
            get => (T)_accessor[_member.Name];
            set => _accessor[_member.Name] = value;
        }

        public ProxyVariable(CommandContext commandContext, string name, CommandFlags flags, string helpInfo,
            object instance, MemberInfo member, ITypeProxy<T> typeProxy,
            object tag = null)
            : base(commandContext, name, CreateAccessorAndGetValue(instance, member, out var accessor), flags, helpInfo, typeProxy, null, tag)
        {
            _accessor = accessor;
            _member = member;
        }

        private static T CreateAccessorAndGetValue(object instance, MemberInfo member, out ObjectAccessor accessor)
        {
            accessor = ObjectAccessor.Create(instance);

            return (T)accessor[member.Name];
        }
    }
}
