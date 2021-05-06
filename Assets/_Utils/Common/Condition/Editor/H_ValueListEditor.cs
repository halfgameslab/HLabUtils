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
    public class H_ValEditor
    {
        private H_ValueEditor _weightEditor;
        private H_ValueEditor _valueEditor;

        public bool Draw(Rect rect, H_Val value, Type refType)
        {
            bool result = false;
            object v = value.Value;
            H_EValueType t = value.ValueType;
            
            if (value.Weight != null)
            {
                // cant pass properties as ref so we copy the data and swap the data after the edition
                object w = value.Weight.Value;
                t = value.Weight.ValueType;
                if (_weightEditor == null)
                    _weightEditor = new H_ValueEditor();

                result = _weightEditor.Draw(rect, "Weight", typeof(float), ref t, ref w);
                value.Weight.Value = w;
                value.Weight.ValueType = t;
                t = value.ValueType;
                rect.y += rect.height;
            }

            if (_valueEditor == null)
                _valueEditor = new H_ValueEditor();

            result = _valueEditor.Draw(rect, "Value", refType, ref t, ref v) || result;
            value.Value = v;
            value.ValueType = t;

            return result;
        }
    }

    public class H_ValueEditor
    {
        H_ValueCVarEditor _editor = new H_ValueCVarEditor();

        public bool Draw(Rect rect, string label, Type refType, ref H_EValueType type, ref object value)
        {
            rect.width = (rect.width) / 5;

            if (label != string.Empty)
            {
                EditorGUI.LabelField(rect, label);
                rect.x += rect.width;
            }
            H_EValueType auxType = type;
            type = (H_EValueType)EditorGUI.EnumPopup(rect, type);

            if (auxType != type)
            {
                if (type == H_EValueType.CVAR)
                {
                    value = string.Concat(refType!=null?refType.Name:"String",".global.","undefined");
                }
                else if (type == H_EValueType.VALUE)
                {
                    value = H_ValueListEditor.GetNewDefaultValue(refType != null ? refType.Name : "Single", 0.0f);
                }
                else
                    value = "MethodName";

                return true;
            }
                        
            if (type == H_EValueType.CVAR)
            {
                rect.x += rect.width + 4;
                rect.width = rect.width * 5;
                return _editor.Draw(rect, rect, refType, ref value);
            }
            else if (type == H_EValueType.VALUE)
            {
                string[] varTypes = CVarSystem.AllowedTypes;
                rect.x += rect.width;

                int index = Array.IndexOf(varTypes, refType != null ? refType.Name : value.GetType().Name);

                EditorGUI.BeginDisabledGroup(refType != null);
                int newIndex = EditorGUI.Popup(rect, index, varTypes);
                EditorGUI.EndDisabledGroup();

                if (index != newIndex)
                {
                    value = H_ValueListEditor.GetNewDefaultValue(varTypes[newIndex], 0.0f);
                    return true;
                }

                rect.x += rect.width + 4;

                EditorGUI.BeginChangeCheck();
                if (value == null)
                    value = 0;

                value = DrawFieldByType(rect, value);

                return EditorGUI.EndChangeCheck();
            }
            else
            {
                rect.x += rect.width;
                rect.width *= 2;

                EditorGUI.BeginChangeCheck();
                if (value == null)
                    value = "MethodName";

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

    }

    public class H_ValueCVarEditor
    {
        private Color _missingColor = new Color(0.9f, 0.35f, 0.35f);

        //private H_EValueMode _mode;
        private Type _varType;

        private bool _editCVarByText = false;
        private string _auxString = null;
        public bool Draw(Rect rect, Rect origin, Type varType, ref object varFullname)
        {
            _varType = varType;
            string fullname = string.Concat((varType ?? typeof(int)).Name, ".global.undefined");//(string)((H_Val)_valuesReorderableList.list[index])?.Value ?? "Int32.global.undefined";

            if (varFullname == null)
                varFullname = fullname;//string.Concat((_varType??typeof(int)).Name, ".global.undefined");

            string[] param0 = ((string)varFullname).Split('.');

            if (varType != null && param0[0] != varType.Name)
                param0[0] = varType.Name;

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
                param0 = new string[] { (varType ?? typeof(int)).Name, "global", "undefined" };
            }

            rect.width = (origin.width) / 5;

            rect.width = 18;
            _editCVarByText = EditorGUI.Toggle(rect, _editCVarByText);
            rect.x += rect.width;
            rect.width = (origin.width) / 5 - 6;

            if (!_editCVarByText)
            {
                return DrawCVarAsPopup(rect, param0, ref varFullname);
            }
            else
            {
                return DrawCVarAsText(rect, param0, ref varFullname);
            }
        }

        private bool DrawCVarAsPopup(Rect rect, string[] param0, ref object varFullname)
        {
            string[] groupsNames = CVarSystem.GetGroups().Select(e => e.Name).ToArray();
            string[] varTypes = CVarSystem.AllowedTypes;

            int i = Array.FindIndex(varTypes, 0, varTypes.Length, e => e == param0[0]);//param0[0]);
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

            string fullname = CVarSystem.GetFullName(param0[2], param0[0], param0[1]);

            if ((string)varFullname != fullname)
            {
                varFullname = fullname;
                return true;
            }
            
            return false;
        }
        
        private bool DrawCVarAsText(Rect rect, string[] param0, ref object varFullname)
        {
            rect.width = rect.width * 3;
         
            if (_auxString == null)
            {
                rect.width -= 20;
                EditorGUI.BeginDisabledGroup(true);
                varFullname = EditorGUI.TextField(rect, (string)varFullname);
                EditorGUI.EndDisabledGroup();

                rect.x += rect.width;
                rect.width = 20;
                //if (EditorGUI.EndChangeCheck())
                if (GUI.Button(rect, "E"))
                {
                    _auxString = (string)varFullname;
                }

                return false;
            }
            else
            {
                rect.width -= 40;
                EditorGUI.BeginDisabledGroup(false);
                _auxString = EditorGUI.TextField(rect, _auxString);
                EditorGUI.EndDisabledGroup();

                rect.x += rect.width;
                rect.width = 20;
                if (GUI.Button(rect, "V"))
                {
                    if ((string)varFullname == _auxString)
                    {
                        _auxString = null;
                        GUI.FocusControl("");
                        return false;
                    }

                    varFullname = _auxString;
                    param0 = ((string)varFullname).Split('.');

                    // complete the name with the type using the first character as reference
                    if (!CVarSystem.ValidateType(param0[0]) || _varType != null)
                    {
                        if (param0[0].Length >= 1)
                        {
                            char firstC = param0[0].ToLower()[0];

                            if (_varType != null)
                                param0[0] = _varType.Name;
                            else if (firstC == 'i')
                                param0[0] = CVarSystem.GetTypeName<int>();
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
                            varFullname = string.Concat(varFullname, ".undefined");

                        param0 = ((string)varFullname).Split('.');
                    }

                    param0[1] = ObjectNamesManager.RemoveForbiddenCharacters(param0[1]);
                    param0[2] = ObjectNamesManager.RemoveForbiddenCharacters(param0[2]);

                    varFullname = CVarSystem.GetFullName(param0[2], param0[0], param0[1]);
                    _auxString = null;
                    GUI.FocusControl("");
                    return true;
                }
                rect.x += 20;
                if (GUI.Button(rect, "X"))
                {
                    _auxString = null;
                    GUI.FocusControl("");
                }
            } 
            return false;
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

    public class H_ValueListEditor
    {
        private Color _missingColor = new Color(0.9f, 0.35f, 0.35f);

        private ReorderableList _valuesReorderableList;

        private H_EValueMode _mode;
        private Type _varType;
        private List<H_ValEditor> _valueEditorList = new List<H_ValEditor>();
        public float Height { get { return _valuesReorderableList != null ? _valuesReorderableList.GetHeight() : 0; } }

        public Type VarType 
        {
            get
            {
                return _varType;
            }
            set
            {
                if (value != _varType)
                {
                    _varType = value;
                    foreach(H_Val val in _valuesReorderableList.list)
                    {
                        if(val.ValueType == H_EValueType.VALUE && val.Value.GetType() != _varType)
                        {
                            val.Value = GetNewDefaultValue(_varType.Name, 0.0f);
                        }
                        else if(val.ValueType == H_EValueType.CVAR)
                        {
                            val.Value = CVarSystem.ChangeFullnameType((string)val.Value, _varType.Name);
                        }
                    }
                    this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ((List<H_Val>)_valuesReorderableList.list).ToArray());
                }
            } 
        }

        public static object GetNewDefaultValue(string type, object def = null)
        {
            if (type == typeof(string).Name)
                return "value";
            else if (type == typeof(int).Name)
                return 0;
            else if (type == typeof(float).Name)
                return 0.0f;
            else if (type == typeof(bool).Name)
                return false;
            else if (type == typeof(Vector3).Name)
                return Vector3.zero;

            return def;
        }

        public void Start(H_Val[] values, H_EValueMode mode, Type varType)
        {
            List<H_Val> hl = new List<H_Val>();

            hl.AddRange(values);

            _mode = mode;
            _varType = varType;
            
            for(int i = 0; i < values.Length; i++)
            {
                _valueEditorList.Add(new H_ValEditor());
            }

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
                this.DispatchEvent(ES_Event.ON_CHANGE, new object[] { _mode, ((List<H_Val>)_valuesReorderableList.list).ToArray() });
            }


            EditorGUI.EndDisabledGroup();
        }

        private void OnAddElementHandler(ReorderableList list)
        {
            H_Val val = new H_Val()
            {
                ValueType = H_EValueType.VALUE,
                Value = _varType != null ? GetNewDefaultValue(_varType.Name, 0.0f) : 0.0f
            };

            if (list.index > 0)
            {
                _valuesReorderableList.list.Insert(list.index + 1, val);
                _valueEditorList.Insert(list.index + 1, new H_ValEditor());
                list.index++;
            }
            else
            {
                _valuesReorderableList.list.Add(val);
                _valueEditorList.Add(new H_ValEditor());
                list.index = list.count - 1;
            }

            ValidateModeWeightAndDispatchEvent();

            //ValidateWeight(list);

            //this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ((List<H_Val>)_valuesReorderableList.list).ToArray());
        }

        private void OnRemoveElementHandler(ReorderableList list)
        {
            _valueEditorList.RemoveAt(list.index);
            list.list.RemoveAt(list.index);
            list.index--;

            ValidateModeWeightAndDispatchEvent();

            //ValidateWeight(list);

            //this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ((List<H_Val>)_valuesReorderableList.list).ToArray());
        }

        private void OnReorderListHandler(ReorderableList list, int oldIndex, int newIndex)
        {
            H_ValEditor aux = _valueEditorList[oldIndex];
            _valueEditorList.RemoveAt(oldIndex);
            _valueEditorList.Insert(newIndex, aux);

            this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ((List<H_Val>)_valuesReorderableList.list).ToArray());
        }

        private void ValidateModeWeightAndDispatchEvent()
        {
            _mode = ValidateMode(_mode);

            ValidateWeight(_valuesReorderableList);

            this.DispatchEvent(ES_Event.ON_CHANGE, new object[] { _mode, ((List<H_Val>)_valuesReorderableList.list).ToArray() });
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
            rect.height = EditorGUIUtility.singleLineHeight;
            if(_valueEditorList[index].Draw(rect, (H_Val)_valuesReorderableList.list[index], _varType))
            {   
                this.DispatchEvent(ES_Event.ON_VALUE_CHANGE, ((List<H_Val>)_valuesReorderableList.list).ToArray());
            }
        }

        public void Draw(Rect rect)
        {
            _valuesReorderableList.DoList(rect);
        }
    }
}