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

using SharpLife.CommandSystem.Commands;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WildcardMatch;

namespace SharpLife.CommandSystem
{
    public static class CommandContextExtensions
    {
        public static IVariable<T> RegisterVariable<T>(this ICommandContext context, string name, Expression<Func<T>> expression, string helpInfo = "")
            => context.RegisterVariable(new ProxyVariableInfo<T>(name, expression).WithHelpInfo(helpInfo));

        public static IEnumerable<IBaseCommand> FindCommands(this ICommandContext context, string keyword, FindCommandFlag flags = FindCommandFlag.None)
        {
            if (keyword == null)
            {
                throw new ArgumentNullException(nameof(keyword));
            }

            return FindCommandsIterator();

            IEnumerable<IBaseCommand> FindCommandsIterator()
            {
                if (keyword.Length == 0)
                {
                    yield break;
                }

                foreach (var command in context.Commands.Values)
                {
                    if (keyword.WildcardMatch(command.Name, true)
                        || ((flags & FindCommandFlag.SearchInHelpInfo) != 0 && keyword.WildcardMatch(command.HelpInfo, true))
                        || ((flags & FindCommandFlag.SearchInValue) != 0 && command is IVariable variable && keyword.WildcardMatch(variable.ValueString, true)))
                    {
                        yield return command;
                    }
                }
            }
        }
    }
}
