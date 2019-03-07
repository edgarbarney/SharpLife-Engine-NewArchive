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


namespace SharpLife.CommandSystem.Commands
{
    public static class VariableExtensions
    {
        /// <summary>
        /// Attempt to get a typed value from an untyped variable
        /// Does not cast the type, and will fail if primitive types are involved
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable"></param>
        /// <param name="defaultValue"></param>
        public static T ValueAs<T>(this IVariable variable, T defaultValue = default)
        {
            //Testing using IVariable avoids boxing the value
            if (variable is IVariable<T> concreteVariable)
            {
                return concreteVariable.Value;
            }

            return defaultValue;
        }
    }
}
