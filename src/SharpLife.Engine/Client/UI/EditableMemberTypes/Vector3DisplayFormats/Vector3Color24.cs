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
using System.Numerics;
using System.Reflection;

namespace SharpLife.Engine.Client.UI.EditableMemberTypes.Vector3DisplayFormats
{
    public sealed class Vector3Color24 : IVector3Display
    {
        private readonly string _label;

        private readonly MemberInfo _info;

        private readonly bool _scaleRange;

        private Vector3 _value;

        public Vector3Color24(int index, object editObject, MemberInfo info, Type type, ObjectAccessor objectAccessor, bool scaleRange)
        {
            _label = $"{index}: {info.Name}";

            _info = info;

            _scaleRange = scaleRange;

            _value = (Vector3)objectAccessor[info.Name];

            //Rescale to [0, 1] range
            if (_scaleRange)
            {
                _value /= 255.0f;
            }
        }

        public void Initialize(int index, object editObject, MemberInfo info, ObjectAccessor objectAccessor)
        {
        }

        public void Display(object editObject, ObjectAccessor objectAccessor)
        {
            if (ImGui.ColorEdit3(_label, ref _value, ImGuiColorEditFlags.None | ImGuiColorEditFlags.Uint8))
            {
                var newValue = _value;

                //Rescale to [0, 255] range
                if (_scaleRange)
                {
                    newValue *= 255.0f;
                }

                objectAccessor[_info.Name] = newValue;
            }
        }
    }
}
