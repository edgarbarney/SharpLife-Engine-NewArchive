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
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharpLife.CommandSystem.Commands
{
    /// <summary>
    /// A variable that acts as a proxy for a variable or property in an object
    /// </summary>
    internal sealed class ProxyVariable<T> : Variable<T>
    {
        private readonly ObjectAccessor _accessor;
        private readonly MemberInfo _member;

        public override bool IsReadOnly { get; }

        public override T Value
        {
            get => (T)_accessor[_member.Name];

            set
            {
                if (!IsReadOnly)
                {
                    _accessor[_member.Name] = value;
                }
                else
                {
                    LogReadOnlyMessage();
                }
            }
        }

        public ProxyVariable(CommandContext commandContext, string name, CommandFlags flags, string helpInfo,
            object instance, MemberInfo member, bool? isReadOnly,
            ITypeProxy<T> typeProxy,
            IReadOnlyList<VariableChangeHandler<T>> changeHandlers,
            object tag = null)
            : base(commandContext, name, CreateAccessorAndGetValue(instance, member, out var accessor), flags, helpInfo, typeProxy, changeHandlers, tag)
        {
            _accessor = accessor;
            _member = member;

            if (isReadOnly.HasValue)
            {
                IsReadOnly = isReadOnly.Value;
            }
            else
            {
                //TODO: maybe convert this into an extension method
                switch (_member)
                {
                    case FieldInfo field:
                        {
                            IsReadOnly = field.IsLiteral || field.IsInitOnly;
                            break;
                        }

                    case PropertyInfo prop:
                        {
                            //Treat properties with non-public setters as read only
                            IsReadOnly = prop.GetSetMethod(false) == null;
                            break;
                        }
                }
            }
        }

        private static T CreateAccessorAndGetValue(object instance, MemberInfo member, out ObjectAccessor accessor)
        {
            accessor = ObjectAccessor.Create(instance);

            return (T)accessor[member.Name];
        }

        public override void WriteCommandInfo(StringBuilder builder)
        {
            if (IsReadOnly)
            {
                builder.Append("Read Only ");
            }

            builder.AppendFormat("Proxy Variable {0} {1} = {2}", Type.Name, Name, ValueString);
        }

        public override string ToString()
        {
            return $"Proxy variable {Name}: {ValueString}";
        }
    }
}
