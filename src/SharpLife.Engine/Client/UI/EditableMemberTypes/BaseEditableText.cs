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
using ImGuiNET;
using System;
using System.Reflection;

namespace SharpLife.Engine.Client.UI.EditableMemberTypes
{
    public abstract class BaseEditableText : IEditableMemberType
    {
        private const int MaxLength = 1024;

        private readonly string _label;

        protected readonly MemberInfo _info;

        private string _currentValue = string.Empty;

        private readonly ImGuiInputTextFlags _flags;

        protected BaseEditableText(int index, object editObject, MemberInfo info, Type type, ObjectAccessor objectAccessor, ImGuiInputTextFlags flags)
        {
            _label = $"{index}: {info.Name}";

            _info = info;

            _flags = flags;
        }

        public abstract void Initialize(int index, object editObject, MemberInfo info, ObjectAccessor objectAccessor);

        public void Display(object editObject, ObjectAccessor objectAccessor)
        {
            if (ImGui.InputText(_label, ref _currentValue, MaxLength, _flags | ImGuiInputTextFlags.EnterReturnsTrue, null))
            {
                OnValueChanged(objectAccessor, _currentValue);
            }
        }

        protected void SetValue(string value)
        {
            _currentValue = value ?? string.Empty;
        }

        protected abstract void OnValueChanged(ObjectAccessor objectAccessor, string newValue);
    }
}
