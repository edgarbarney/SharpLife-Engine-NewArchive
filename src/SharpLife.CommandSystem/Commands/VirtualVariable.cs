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

using SharpLife.CommandSystem.TypeProxies;
using System.Collections.Generic;

namespace SharpLife.CommandSystem.Commands
{
    /// <summary>
    /// A variable that is managed by the command system
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class VirtualVariable<T> : Variable<T>
    {
        public override T Value { get; set; }

        public VirtualVariable(CommandContext commandContext, string name, in T value, CommandFlags flags, string helpInfo,
            ITypeProxy<T> typeProxy,
            IReadOnlyList<VariableChangeHandler<T>> changeHandlers,
            object tag = null)
            : base(commandContext, name, value, flags, helpInfo, typeProxy, changeHandlers, tag)
        {
            SetValue(value, true, false);
        }
    }
}
