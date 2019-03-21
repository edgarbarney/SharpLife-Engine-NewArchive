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
using Serilog;
using SharpLife.Engine.Client.UI.EditableMemberTypes;
using SharpLife.Engine.Entities;
using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.ObjectEditor;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace SharpLife.Engine.Client.UI
{
    internal sealed class ObjectEditor
    {
        private sealed class ComponentData
        {
            public readonly Component Component;

            public readonly ObjectAccessor Accessor;

            public readonly List<IEditableMemberType> EditableMembers;

            public string InvokeBuffer = string.Empty;

            public ComponentData(Component component, ObjectAccessor accessor, List<IEditableMemberType> editableMembers)
            {
                Component = component ?? throw new ArgumentNullException(nameof(component));
                Accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
                EditableMembers = editableMembers ?? throw new ArgumentNullException(nameof(editableMembers));
            }
        }

        private const bool DefaultObjectEditorMemberVisibility = true;

        private const uint InvokeMaxLength = 1024;

        private readonly ILogger _logger;

        private bool _objectEditorVisible;

        private delegate IEditableMemberType EditableMemberFactory(int index, object editObject, MemberInfo info, Type type, ObjectAccessor objectAccessor);

        private readonly EditableMemberFactory _enumEditableMemberTypeFactory = (index, editObject, info, type, objectAccessor) =>
            new EditableEnum(index, editObject, info, type, objectAccessor);

        private readonly IReadOnlyDictionary<Type, EditableMemberFactory> _editableMemberTypes = new Dictionary<Type, EditableMemberFactory>
        {
            { typeof(bool), (index, editObject, info, type, objectAccessor) => new EditableBoolean(index, editObject, info,type, objectAccessor) },
            { typeof(int), (index, editObject, info, type, objectAccessor) => new EditableInt32(index, editObject, info,type, objectAccessor) },
            { typeof(uint), (index, editObject, info,type, objectAccessor) => new EditableUInt32(index, editObject, info,type, objectAccessor) },
            { typeof(float), (index, editObject, info, type,objectAccessor) => new EditableFloat(index, editObject, info,type, objectAccessor) },
            { typeof(string), (index, editObject, info, type,objectAccessor) => new EditableString(index, editObject, info,type, objectAccessor) },
            { typeof(Vector3), (index, editObject, info, type,objectAccessor) => new EditableVector3(index, editObject, info,type, objectAccessor) }
        };

        private Entity _editObjectHandle;

        private readonly List<ComponentData> _editableComponents = new List<ComponentData>();

        private Scene _scene;

        public ObjectEditor(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnMapStart(Scene scene)
        {
            _scene = scene;
        }

        public void OnMapEnd()
        {
            _scene = null;
            _editObjectHandle = null;
            _editableComponents.Clear();
        }

        public void AddMenuItem()
        {
            ImGui.Checkbox("Toggle Object Editor", ref _objectEditorVisible);
        }

        private static (Type, bool) GetMemberData(MemberInfo info)
        {
            switch (info)
            {
                case FieldInfo fieldInfo: return (fieldInfo.FieldType, !fieldInfo.IsInitOnly);
                case PropertyInfo propInfo: return (propInfo.PropertyType, propInfo.CanWrite);

                default: return (null, false);
            }
        }

        private ComponentData SetupComponentData(Component component)
        {
            var accessor = ObjectAccessor.Create(component);

            var defaultVisibility = component.GetType().GetCustomAttribute<ObjectEditorVisibleAttribute>()?.Visible ?? DefaultObjectEditorMemberVisibility;

            var editableMembers = new List<IEditableMemberType>();

            foreach (var info in component.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                (var memberType, var isWritable) = GetMemberData(info);

                if (memberType != null && isWritable)
                {
                    var visible = info.GetCustomAttribute<ObjectEditorVisibleAttribute>()?.Visible ?? defaultVisibility;

                    if (visible)
                    {
                        EditableMemberFactory factory = null;

                        if (memberType.IsEnum)
                        {
                            factory = _enumEditableMemberTypeFactory;
                        }
                        else
                        {
                            _editableMemberTypes.TryGetValue(memberType, out factory);
                        }

                        if (factory != null)
                        {
                            var index = editableMembers.Count;

                            var editableMember = factory(index, component, info, memberType, accessor);
                            editableMember.Initialize(index, component, info, accessor);

                            editableMembers.Add(editableMember);
                        }
                    }
                }
            }

            return new ComponentData(component, accessor, editableMembers);
        }

        private void SetupNewObject(Entity editObject)
        {
            _editableComponents.Clear();

            if (editObject != null)
            {
                foreach (var component in editObject.Components)
                {
                    _editableComponents.Add(SetupComponentData(component));
                }
            }
        }

        public void Draw()
        {
            if (_objectEditorVisible && ImGui.Begin("Object Editor", ref _objectEditorVisible, ImGuiWindowFlags.NoCollapse))
            {
                var oldObjectHandle = _editObjectHandle;

                var isValid = false;

                if (_scene != null)
                {
                    if (ImGui.BeginCombo("Object", "Select object...", ImGuiComboFlags.HeightLargest))
                    {
                        foreach (var entity in _scene.Entities.EnumerateAll())
                        {
                            var isSelected = ReferenceEquals(_editObjectHandle, entity);

                            if (ImGui.Selectable(entity.ToString(), isSelected))
                            {
                                _editObjectHandle = entity;
                            }

                            if (isSelected)
                            {
                                ImGui.SetItemDefaultFocus();
                            }
                        }

                        ImGui.EndCombo();
                    }

                    if (_editObjectHandle?.Destroyed == false)
                    {
                        if (oldObjectHandle != _editObjectHandle)
                        {
                            SetupNewObject(_editObjectHandle);
                        }

                        if (_editObjectHandle != null)
                        {
                            isValid = true;

                            ImGui.Text(_editObjectHandle.ToString());

                            var componentIndex = 0;

                            foreach (var componentData in _editableComponents)
                            {
                                if (ImGui.CollapsingHeader(componentData.Component.ToString()))
                                {
                                    ImGui.InputText($"Invoke method ## {componentIndex}", ref componentData.InvokeBuffer, InvokeMaxLength, ImGuiInputTextFlags.None, null);

                                    ImGui.SameLine();

                                    if (ImGui.Button($"Invoke ## {componentIndex}") && componentData.InvokeBuffer.Length > 0)
                                    {
                                        componentData.Component.Invoke(componentData.InvokeBuffer);
                                    }

                                    //Display all properties
                                    foreach (var member in componentData.EditableMembers)
                                    {
                                        member.Display(_editObjectHandle, componentData.Accessor);
                                    }
                                }

                                ++componentIndex;
                            }
                        }
                    }
                }

                if (!isValid)
                {
                    SetupNewObject(null);
                }

                ImGui.End();
            }
        }
    }
}
