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
    public interface IVariable : IBaseCommand
    {
        /// <summary>
        /// The type of the variable
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Indicates whether this variable is read only
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// The initial value assigned to this variable as an object
        /// </summary>
        object InitialValueObject { get; }

        /// <summary>
        /// The initial value assigned to this variable as a string
        /// </summary>
        string InitialValueString { get; }

        /// <summary>
        /// The current value as an object
        /// </summary>
        object ValueObject { get; set; }

        /// <summary>
        /// The current value as a string
        /// </summary>
        string ValueString { get; set; }

        /// <summary>
        /// Resets this variable to the initial value
        /// </summary>
        void RevertToInitialValue();
    }

    public interface IVariable<T> : IVariable
    {
        /// <summary>
        /// The initial value assigned to this variable
        /// </summary>
        T InitialValue { get; }

        /// <summary>
        /// The current value
        /// </summary>
        T Value { get; set; }

        /// <summary>
        /// Invoked after the variable has changed
        /// Change handlers may change the variable by using the change event interface
        /// If the variable is reset to its old value, the change message is suppressed
        /// </summary>
        event VariableChangeHandler<T> OnChange;
    }
}
