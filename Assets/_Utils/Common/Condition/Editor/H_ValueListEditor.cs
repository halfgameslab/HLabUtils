using H_Misc;
using Mup.EventSystem.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace HLab.H_Common.H_Editor
{
    public class H_ValueListEditor
    {
        private Color _missingColor = new Color(0.9f, 0.35f, 0.35f);

        private ReorderableList _valuesReorderableList;

        private H_EValueMode _mode;
        private Type _varType;
        public float Height { get { return _valuesReorderableList != null ? _valuesReorderableList.GetHeight() : 0; } }

        public void Start(H_Val[] values, H_EValueMode mode, Type varType)
        {
            List<H_Val> hl = new List<H_Val>();

            hl.AddRange(values);

            _mode = mode;
            _varType = varType;

            _valuesReorderableList = new ReorderableList(hl, typeof(H_Val))
            {
                elementHeightCallback = OnElementHeightHandler,
                drawHeaderCallback = OnDrawElementHeaderHandler,
                drawElementCallback = OnDrawElementHandler,
                onAddCallback = OnAddElementHandler,
                onRemoveCallback = OnRemoveElementHandler,
                onReorderCallbackWithDetails = OnReorderListHandler
            };
        }

        private float OnElementHeightHandler(int index)
        {
            if(_mode == H_EValueMode.RANDOM_VALUE)
                return EditorGUIUtility.singleLineHeight * 2 +4f;

            return EditorGUIUtility.singleLineHeight + 4f;
        }

        private void OnDrawElementHeaderHandler(Rect rect)
        {
            EditorGUI.LabelField(rect, "Values");
            rect.x += rect.width - (rect.width / 4f);
            rect.width = rect.width / 4f;
            EditorGUI.BeginDisabledGroup(_valuesReorderableList.list.Count <= 1 || _valuesReorderableList.list.Count > 2);

            H_EValueMode _oldMode = _mode;

            _mode = (H_EValueMode)EditorGUI.EnumPopup(rect, _mode);

            _mode = ValidateMode(_mode);

            if(_oldMode != _mode)
            {
                ValidateWeight(_valuesReorderableList);
                this.DispatchEvent(ES_Event.ON_CHANGE, _mode);
            }


            EditorGUI.EndDisabledGroup();
        }

        private void OnAddElementHandler(ReorderableList list)
        {
            H_Val val = new H_Val()
            {
                ValueType = H_EValueType.VALUE,
                Value = 0.0f
            };

            if (list.index > 0)
            {
                _valuesReorderableList.list.Insert(list.index + 1, val);
                list.index++;
            }
            else
            {
                _valuesReorderableList.list.Add(val);
                list.index = list.count - 1;
            }

            ValidateModeAndDispatchEvent();

            ValidateWeight(list);

            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ((List<H_Val>)_valuesReorderableList.list).ToArray());
        }

        private void OnRemoveElementHandler(ReorderableList list)
        {
            list.list.RemoveAt(list.index);
            list.index--;

            ValidateModeAndDispatchEvent();

            ValidateWeight(list);

            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ((List<H_Val>)_valuesReorderableList.list).ToArray());
        }

        private void OnReorderListHandler(ReorderableList list, int oldIndex, int newIndex)
        {
            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ((List<H_Val>)_valuesReorderableList.list).ToArray());
        }

        private void ValidateModeAndDispatchEvent()
        {
            _mode = ValidateMode(_mode);
            this.DispatchEvent(ES_Event.ON_CHANGE, _mode);
        }

        private void ValidateWeight(ReorderableList list)
        {
            foreach (H_Val v in list.list)
            {
                v.Weight = v.Weight == null && _mode == H_EValueMode.RANDOM_VALUE ? new H_Val()
                {
                    ValueType = H_EValueType.VALUE,
                    Value = 0.0f
                } : _mode == H_EValueMode.RANDOM_VALUE ? v.Weight : null;
            }
        }

        private H_EValueMode ValidateMode(H_EValueMode mode)
        {
            if (_valuesReorderableList.list.Count <= 1)
            {
                if (mode != H_EValueMode.SINGLE_VALUE)
                {
                    Debug.LogWarning("Mode updated to SINGLE_VALUE. Add values to unlock RANDOM options.");
                    return H_EValueMode.SINGLE_VALUE;
                }
            }
            else if (mode != H_EValueMode.RANDOM_VALUE)
            {
                if (_valuesReorderableList.list.Count > 2)
                {
                    if (mode == H_EValueMode.SINGLE_VALUE)
                        Debug.LogWarning("Mode updated to RANDOM_VALUE. Leave only one value to use the option SINGLE_VALUE.");
                    else
                        Debug.LogWarning("Mode updated to RANDOM_VALUE. Leave only two values to use the option RANDOM_INTERVAL.");

                    return H_EValueMode.RANDOM_VALUE;
                }
                else if (mode == H_EValueMode.SINGLE_VALUE)
                {
                    Debug.LogWarning("Mode updated to RANDOM_VALUE. Leave only one value to use the option SINGLE_VALUE.");
                    return H_EValueMode.RANDOM_VALUE;
                }
            }

            return mode;
        }

        private void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {   
            string varUID = "1";
            Rect origin = rect;

            rect.height = EditorGUIUtility.singleLineHeight;

            object value = null;
            H_Val currentHValue = (H_Val)_valuesReorderableList.list[index];
            H_EValueType type;
            if (_mode == H_EValueMode.RANDOM_VALUE)
            {
                if (currentHValue.Weight != null)
                {
                    type = currentHValue.Weight.ValueType;
                    value = currentHValue.Weight.Value;
                }
                else
                {
                    type = H_EValueType.VALUE;
                    value = 0.0f;
                }

                if (DrawVarCondition(rect, origin, "Weight", index, ref type, ref value, ref varUID))
                {
                    if (currentHValue.Weight == null)
                        currentHValue.Weight = new H_Val();

                    currentHValue.Weight.Value = value;
                    currentHValue.Weight.ValueType = type;
                    this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ((List<H_Val>)_valuesReorderableList.list).ToArray());
                }
                rect.y += rect.height;
            }


            value = currentHValue.Value;
            type = currentHValue.ValueType;
            if (DrawVarCondition(rect, origin, "Value", index, ref type, ref value, ref varUID))
            {
                currentHValue.Value = value;
                currentHValue.ValueType = type;
                Debug.Log("value " + value);
                this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ((List<H_Val>)_valuesReorderableList.list).ToArray());
            }
        }

        public void Draw(Rect rect)
        {
            _valuesReorderableList.DoList(rect);
        }

        private bool DrawCVar(Rect rect, Rect origin, int index, ref object varFullname, ref string varUID)
        {
            string fullname = string.Concat((_varType ?? typeof(int)).Name, ".global.undefined");//(string)((H_Val)_valuesReorderableList.list[index])?.Value ?? "Int32.global.undefined";

            if (varFullname == null)
                varFullname = fullname;//string.Concat((_varType??typeof(int)).Name, ".global.undefined");


            string[] groupsNames = CVarSystem.GetGroups().Select(e => e.Name).ToArray();
            string[] varTypes = CVarSystem.AllowedTypes;

            string[] param0 = ((string)varFullname).Split('.');
            if (param0.Length == 2)
            {
                param0 = new string[] { param0[0], param0[1], "undefined" };
            }
            else if (param0.Length == 1)
            {
                param0 = new string[] { param0[0], "global", "undefined" };
            }
            else if (param0.Length == 0)
            {
                param0 = new string[] { (_varType ?? typeof(int)).Name, "global", "undefined" };
            }

            rect.width = (origin.width) / 5;

            rect.width = 18;
            EditorGUI.Toggle(rect, true);
            rect.x += rect.width;
            rect.width = (origin.width) / 5 - 6;

            if (true)
            {
                int i = Array.FindIndex(varTypes, 0, varTypes.Length, e => e == param0[0]);
                if (i < 0)
                {
                    param0[0] = varTypes[0];
                    i = 0;
                }
                EditorGUI.BeginDisabledGroup(_varType != null);
                if (CheckablePopup(rect, i, varTypes, out i))
                {
                    param0[0] = varTypes[i];
                }
                EditorGUI.EndDisabledGroup();

                rect.x += rect.width;

                i = Array.FindIndex(groupsNames, 0, groupsNames.Length, e => e == param0[1]);
                if (i < 0)
                {
                    List<string> l = new List<string>();
                    l.Add("<missing>" + param0[1]);
                    l.AddRange(groupsNames);
                    groupsNames = l.ToArray();
                    i = 0;
                    GUI.contentColor = _missingColor;
                }

                if (CheckablePopup(rect, i, groupsNames, out i))
                {
                    param0[1] = groupsNames[i];
                }

                rect.x += rect.width;
                GUI.contentColor = Color.white;

                string[] vars = CVarSystem.GetVarNamesByType(param0[0], param0[1]);
                i = Array.FindIndex(vars, 0, vars.Length, e => e == param0[2]);
                if (i < 0)
                {
                    List<string> l = new List<string>();
                    l.Add("<missing>" + param0[2]);
                    l.AddRange(vars);
                    vars = l.ToArray();
                    i = 0;
                    GUI.contentColor = _missingColor;
                }

                if (CheckablePopup(rect, i, vars, out i))
                {
                    param0[2] = vars[i];
                }

                GUI.contentColor = Color.white;

                fullname = CVarSystem.GetFullName(param0[2], param0[0], param0[1]);

                if ((string)varFullname != fullname)
                {
                    varFullname = fullname;
                    return true;
                }
            }
            else
            {
                rect.width = rect.width * 3f;

                //string varFullname = (string)_conditions[index]._condition.Params[0];//CVarSystem.GetFullName((string)_conditions[index]._condition.Params[2], (string)_conditions[index]._condition.Params[1], (string)_conditions[index]._condition.Params[0]);
                EditorGUI.BeginChangeCheck();
                varFullname = EditorGUI.TextField(rect, (string)varFullname);
                if (EditorGUI.EndChangeCheck())
                {
                    param0 = ((string)varFullname).Split('.');

                    // complete the name with the type using the first character as reference
                    if (!CVarSystem.ValidateType(param0[0]))
                    {
                        if (param0[0].Length >= 1)
                        {
                            char firstC = param0[0].ToLower()[0];

                            if (firstC == 'i')
                            {
                                param0[0] = CVarSystem.GetTypeName<int>();
                            }
                            else if ((firstC == 's' && param0[0].Length >= 2 && param0[0].ToLower()[1] == 'i') || firstC == 'f')
                                param0[0] = CVarSystem.GetTypeName<float>();
                            else if (firstC == 'b')
                                param0[0] = CVarSystem.GetTypeName<bool>();
                            else if (firstC == 'v')
                                param0[0] = CVarSystem.GetTypeName<Vector3>();
                            else
                                param0[0] = CVarSystem.GetTypeName<string>();
                        }
                        else
                            param0[0] = CVarSystem.GetTypeName<string>();

                        Debug.LogWarning(string.Format("Condition Editor: Invalid type. Type changed for {0}", param0[0]));

                        varFullname = string.Join(".", param0);
                    }

                    if (param0.Length < 3)
                    {
                        for (int i = param0.Length; i < 3; i++)
                            varFullname = string.Concat(varFullname, ".");

                        param0 = ((string)varFullname).Split('.');
                    }

                    param0[1] = ObjectNamesManager.RemoveForbiddenCharacters(param0[1]);
                    param0[2] = ObjectNamesManager.RemoveForbiddenCharacters(param0[2]);

                    varFullname = CVarSystem.GetFullName(param0[2], param0[0], param0[1]);

                    return true;
                    //_conditions[index]._condition.UpdateParam(0, fullname);

                }

                rect.width = rect.width * 3f;

                //string varFullname = (string)_conditions[index]._condition.Params[0];//CVarSystem.GetFullName((string)_conditions[index]._condition.Params[2], (string)_conditions[index]._condition.Params[1], (string)_conditions[index]._condition.Params[0]);
                EditorGUI.BeginChangeCheck();
                varFullname = EditorGUI.TextField(rect, (string)varFullname);
                if (EditorGUI.EndChangeCheck())
                {
                    param0 = ((string)varFullname).Split('.');

                    // complete the name with the type using the first character as reference
                    if (!CVarSystem.ValidateType(param0[0]))
                    {
                        if (param0[0].Length >= 1)
                        {
                            char firstC = param0[0].ToLower()[0];

                            if (firstC == 'i')
                            {
                                param0[0] = CVarSystem.GetTypeName<int>();
                            }
                            else if ((firstC == 's' && param0[0].Length >= 2 && param0[0].ToLower()[1] == 'i') || firstC == 'f')
                                param0[0] = CVarSystem.GetTypeName<float>();
                            else if (firstC == 'b')
                                param0[0] = CVarSystem.GetTypeName<bool>();
                            else if (firstC == 'v')
                                param0[0] = CVarSystem.GetTypeName<Vector3>();
                            else
                                param0[0] = CVarSystem.GetTypeName<string>();
                        }
                        else
                            param0[0] = CVarSystem.GetTypeName<string>();

                        Debug.LogWarning(string.Format("Condition Editor: Invalid type. Type changed for {0}", param0[0]));

                        varFullname = string.Join(".", param0);
                    }

                    if (param0.Length < 3)
                    {
                        for (int i = param0.Length; i < 3; i++)
                            varFullname = string.Concat(varFullname, ".");

                        param0 = ((string)varFullname).Split('.');
                    }

                    param0[1] = ObjectNamesManager.RemoveForbiddenCharacters(param0[1]);
                    param0[2] = ObjectNamesManager.RemoveForbiddenCharacters(param0[2]);

                    varFullname = CVarSystem.GetFullName(param0[2], param0[0], param0[1]);

                    return true;
                    //_conditions[index]._condition.UpdateParam(0, fullname);

                }
            }
            return false;
        }

        private bool DrawVarCondition(Rect rect, Rect origin, string label,int index, ref H_EValueType type, ref object value, ref string varUID)
        {
            //type = ((H_Val)_valuesReorderableList.list[index]).ValueType;

            rect.width = (origin.width) / 5;

            EditorGUI.LabelField(rect, label);
            rect.x += rect.width;

            H_EValueType auxType = type;
            type = (H_EValueType)EditorGUI.EnumPopup(rect, type);

            if (auxType != type)
            {
                if(type == H_EValueType.CVAR)
                {
                    value = string.Empty;
                }
                else
                {
                    value = 0.0f;
                }

                return true;
            }

            rect.x += rect.width+4;

            if (type == H_EValueType.CVAR)
            {
                return DrawCVar(rect, origin, index, ref value, ref varUID);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                if (value == null)
                    value = 0;

                value = DrawFieldByType(rect, value);
                
                return EditorGUI.EndChangeCheck();
            }
        }

        private static object DrawFieldByType(Rect rect, object value)
        {
            if (value is string)
                return EditorGUI.TextField(rect, (string)value);
            else if (value is int)
                return EditorGUI.IntField(rect, (int)value);
            else if (value is float)
                return EditorGUI.FloatField(rect, (float)value);
            else if (value is bool)
                return EditorGUI.Toggle(rect, (bool)value);
            else if (value is Vector3)
                return EditorGUI.Vector3Field(rect, "", (Vector3)value);

            return null;
        }

        public bool CheckablePopup(Rect rect, int selectedIndex, string[] displayableOptions, out int index)
        {
            index = EditorGUI.Popup(rect, selectedIndex, displayableOptions);

            if (selectedIndex != index)
            {
                return true;
            }

            return false;
        }
    }
}