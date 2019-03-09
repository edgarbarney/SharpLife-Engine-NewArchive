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

        /// <summary>
        /// Visits a variable and invokes the delegate that matches the given type
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <param name="variable"></param>
        /// <param name="visitor0"></param>
        /// <param name="unhandled">If not null, will be invoked if none of the types matched</param>
        public static void Visit<T0>(
            this IVariable variable,
            Action<IVariable<T0>> visitor0,
            Action<IVariable> unhandled = null)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            if (visitor0 == null)
            {
                throw new ArgumentNullException(nameof(visitor0));
            }

            if (variable is IVariable<T0> var0)
            {
                visitor0(var0);
            }
            else
            {
                unhandled?.Invoke(variable);
            }
        }

        public static void Visit<T0, T1>(
            this IVariable variable,
            Action<IVariable<T0>> visitor0,
            Action<IVariable<T1>> visitor1,
            Action<IVariable> unhandled = null)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            if (visitor0 == null)
            {
                throw new ArgumentNullException(nameof(visitor0));
            }

            if (visitor1 == null)
            {
                throw new ArgumentNullException(nameof(visitor1));
            }

            if (variable is IVariable<T0> var0)
            {
                visitor0(var0);
            }
            else if (variable is IVariable<T1> var1)
            {
                visitor1(var1);
            }
            else
            {
                unhandled?.Invoke(variable);
            }
        }

        public static void Visit<T0, T1, T2>(
            this IVariable variable,
            Action<IVariable<T0>> visitor0,
            Action<IVariable<T1>> visitor1,
            Action<IVariable<T2>> visitor2,
            Action<IVariable> unhandled = null)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            if (visitor0 == null)
            {
                throw new ArgumentNullException(nameof(visitor0));
            }

            if (visitor1 == null)
            {
                throw new ArgumentNullException(nameof(visitor1));
            }

            if (visitor2 == null)
            {
                throw new ArgumentNullException(nameof(visitor2));
            }

            if (variable is IVariable<T0> var0)
            {
                visitor0(var0);
            }
            else if (variable is IVariable<T1> var1)
            {
                visitor1(var1);
            }
            else if (variable is IVariable<T2> var2)
            {
                visitor2(var2);
            }
            else
            {
                unhandled?.Invoke(variable);
            }
        }

        public static void Visit<T0, T1, T2, T3>(
            this IVariable variable,
            Action<IVariable<T0>> visitor0,
            Action<IVariable<T1>> visitor1,
            Action<IVariable<T2>> visitor2,
            Action<IVariable<T3>> visitor3,
            Action<IVariable> unhandled = null)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            if (visitor0 == null)
            {
                throw new ArgumentNullException(nameof(visitor0));
            }

            if (visitor1 == null)
            {
                throw new ArgumentNullException(nameof(visitor1));
            }

            if (visitor2 == null)
            {
                throw new ArgumentNullException(nameof(visitor2));
            }

            if (visitor3 == null)
            {
                throw new ArgumentNullException(nameof(visitor3));
            }

            if (variable is IVariable<T0> var0)
            {
                visitor0(var0);
            }
            else if (variable is IVariable<T1> var1)
            {
                visitor1(var1);
            }
            else if (variable is IVariable<T2> var2)
            {
                visitor2(var2);
            }
            else if (variable is IVariable<T3> var3)
            {
                visitor3(var3);
            }
            else
            {
                unhandled?.Invoke(variable);
            }
        }

        public static void Visit<T0, T1, T2, T3, T4>(
            this IVariable variable,
            Action<IVariable<T0>> visitor0,
            Action<IVariable<T1>> visitor1,
            Action<IVariable<T2>> visitor2,
            Action<IVariable<T3>> visitor3,
            Action<IVariable<T4>> visitor4,
            Action<IVariable> unhandled = null)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            if (visitor0 == null)
            {
                throw new ArgumentNullException(nameof(visitor0));
            }

            if (visitor1 == null)
            {
                throw new ArgumentNullException(nameof(visitor1));
            }

            if (visitor2 == null)
            {
                throw new ArgumentNullException(nameof(visitor2));
            }

            if (visitor3 == null)
            {
                throw new ArgumentNullException(nameof(visitor3));
            }

            if (visitor4 == null)
            {
                throw new ArgumentNullException(nameof(visitor4));
            }

            if (variable is IVariable<T0> var0)
            {
                visitor0(var0);
            }
            else if (variable is IVariable<T1> var1)
            {
                visitor1(var1);
            }
            else if (variable is IVariable<T2> var2)
            {
                visitor2(var2);
            }
            else if (variable is IVariable<T3> var3)
            {
                visitor3(var3);
            }
            else if (variable is IVariable<T4> var4)
            {
                visitor4(var4);
            }
            else
            {
                unhandled?.Invoke(variable);
            }
        }

        public static void Visit<T0, T1, T2, T3, T4, T5>(
            this IVariable variable,
            Action<IVariable<T0>> visitor0,
            Action<IVariable<T1>> visitor1,
            Action<IVariable<T2>> visitor2,
            Action<IVariable<T3>> visitor3,
            Action<IVariable<T4>> visitor4,
            Action<IVariable<T5>> visitor5,
            Action<IVariable> unhandled = null)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            if (visitor0 == null)
            {
                throw new ArgumentNullException(nameof(visitor0));
            }

            if (visitor1 == null)
            {
                throw new ArgumentNullException(nameof(visitor1));
            }

            if (visitor2 == null)
            {
                throw new ArgumentNullException(nameof(visitor2));
            }

            if (visitor3 == null)
            {
                throw new ArgumentNullException(nameof(visitor3));
            }

            if (visitor4 == null)
            {
                throw new ArgumentNullException(nameof(visitor4));
            }

            if (visitor5 == null)
            {
                throw new ArgumentNullException(nameof(visitor5));
            }

            if (variable is IVariable<T0> var0)
            {
                visitor0(var0);
            }
            else if (variable is IVariable<T1> var1)
            {
                visitor1(var1);
            }
            else if (variable is IVariable<T2> var2)
            {
                visitor2(var2);
            }
            else if (variable is IVariable<T3> var3)
            {
                visitor3(var3);
            }
            else if (variable is IVariable<T4> var4)
            {
                visitor4(var4);
            }
            else if (variable is IVariable<T5> var5)
            {
                visitor5(var5);
            }
            else
            {
                unhandled?.Invoke(variable);
            }
        }

        public static void Visit<T0, T1, T2, T3, T4, T5, T6>(
            this IVariable variable,
            Action<IVariable<T0>> visitor0,
            Action<IVariable<T1>> visitor1,
            Action<IVariable<T2>> visitor2,
            Action<IVariable<T3>> visitor3,
            Action<IVariable<T4>> visitor4,
            Action<IVariable<T5>> visitor5,
            Action<IVariable<T6>> visitor6,
            Action<IVariable> unhandled = null)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            if (visitor0 == null)
            {
                throw new ArgumentNullException(nameof(visitor0));
            }

            if (visitor1 == null)
            {
                throw new ArgumentNullException(nameof(visitor1));
            }

            if (visitor2 == null)
            {
                throw new ArgumentNullException(nameof(visitor2));
            }

            if (visitor3 == null)
            {
                throw new ArgumentNullException(nameof(visitor3));
            }

            if (visitor4 == null)
            {
                throw new ArgumentNullException(nameof(visitor4));
            }

            if (visitor5 == null)
            {
                throw new ArgumentNullException(nameof(visitor5));
            }

            if (visitor6 == null)
            {
                throw new ArgumentNullException(nameof(visitor6));
            }

            if (variable is IVariable<T0> var0)
            {
                visitor0(var0);
            }
            else if (variable is IVariable<T1> var1)
            {
                visitor1(var1);
            }
            else if (variable is IVariable<T2> var2)
            {
                visitor2(var2);
            }
            else if (variable is IVariable<T3> var3)
            {
                visitor3(var3);
            }
            else if (variable is IVariable<T4> var4)
            {
                visitor4(var4);
            }
            else if (variable is IVariable<T5> var5)
            {
                visitor5(var5);
            }
            else if (variable is IVariable<T6> var6)
            {
                visitor6(var6);
            }
            else
            {
                unhandled?.Invoke(variable);
            }
        }

        public static void Visit<T0, T1, T2, T3, T4, T5, T6, T7>(
            this IVariable variable,
            Action<IVariable<T0>> visitor0,
            Action<IVariable<T1>> visitor1,
            Action<IVariable<T2>> visitor2,
            Action<IVariable<T3>> visitor3,
            Action<IVariable<T4>> visitor4,
            Action<IVariable<T5>> visitor5,
            Action<IVariable<T6>> visitor6,
            Action<IVariable<T7>> visitor7,
            Action<IVariable> unhandled = null)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }

            if (visitor0 == null)
            {
                throw new ArgumentNullException(nameof(visitor0));
            }

            if (visitor1 == null)
            {
                throw new ArgumentNullException(nameof(visitor1));
            }

            if (visitor2 == null)
            {
                throw new ArgumentNullException(nameof(visitor2));
            }

            if (visitor3 == null)
            {
                throw new ArgumentNullException(nameof(visitor3));
            }

            if (visitor4 == null)
            {
                throw new ArgumentNullException(nameof(visitor4));
            }

            if (visitor5 == null)
            {
                throw new ArgumentNullException(nameof(visitor5));
            }

            if (visitor6 == null)
            {
                throw new ArgumentNullException(nameof(visitor6));
            }

            if (visitor7 == null)
            {
                throw new ArgumentNullException(nameof(visitor7));
            }

            if (variable is IVariable<T0> var0)
            {
                visitor0(var0);
            }
            else if (variable is IVariable<T1> var1)
            {
                visitor1(var1);
            }
            else if (variable is IVariable<T2> var2)
            {
                visitor2(var2);
            }
            else if (variable is IVariable<T3> var3)
            {
                visitor3(var3);
            }
            else if (variable is IVariable<T4> var4)
            {
                visitor4(var4);
            }
            else if (variable is IVariable<T5> var5)
            {
                visitor5(var5);
            }
            else if (variable is IVariable<T6> var6)
            {
                visitor6(var6);
            }
            else if (variable is IVariable<T7> var7)
            {
                visitor7(var7);
            }
            else
            {
                unhandled?.Invoke(variable);
            }
        }
    }
}
