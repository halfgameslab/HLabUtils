using H_Misc;
using HLab.H_DataSystem;
using HLab.H_QuestSystem;
using Mup.EventSystem.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Util.InstanceSystem.Editor;

namespace HLab.H_Common.H_Editor
{
    public class H_ConditionEditor
    {
        private string _title = string.Empty;

        private H_Condition _condition;

        private ReorderableList _conditionReorderableList;

        private H_ValueListEditor _valueListEditor;

        private List<H_ConditionEditor> _conditions = new List<H_ConditionEditor>();

        private bool _showStringFullname = false;
        private H_ValueEditor _valueEditor;

        private UnityEngine.Object _objTarget;

        private IS_InstanceSceneManager InstanceManager;// renomear

        private Color _missingColor = new Color(0.9f, 0.35f, 0.35f);

        public void Start(H_Condition condition, string title = "Conditions")
        {
            //save title
            _title = title;
            //save condition
            _condition = condition;
            _conditions.Clear();
            if (_condition.Type == H_EConditionType.CONDITION)
            {
                foreach (H_Condition c in condition.Conditions)
                {
                    H_ConditionEditor ce = new H_ConditionEditor();
                    _conditions.Add(ce);
                    ce.Start(c);
                }

                //create the reorderableList
                _conditionReorderableList = new ReorderableList(_conditions, typeof(H_ConditionEditor))
                {
                    drawHeaderCallback = OnDrawHeaderHandler,
                    drawElementCallback = OnDrawElementHandler,
                    elementHeightCallback = OnElementHeightHandler,
                    onAddCallback = OnAddElementHandler,
                    onRemoveCallback = OnRemoveElementHandler,
                    onReorderCallbackWithDetails = OnReorderListHandler
                };
            }
            else
            {
                _conditionReorderableList = null;
                if(_condition.Type == H_EConditionType.CHECK_VAR
                || _condition.Type == H_EConditionType.ON_CHANGE_VAR)
                {
                    _valueListEditor = new H_ValueListEditor();

                    List<H_Val> values = new List<H_Val>();

                    if (_condition.Params != null)
                    {
                        // get H_Val data from generic objects inside Params array
                        for (int i = 3; i < _condition.Params.Length; i += 2)
                        {
                            H_Val v = new H_Val { ValueType = (H_EValueType)_condition.Params[i], Value = _condition.Params[i + 1] };

                            if ((H_EValueMode)_condition.Params[2] != H_EValueMode.SINGLE_VALUE)
                            {
                                v.Weight = new H_Val { ValueType = (H_EValueType)_condition.Params[i + 2], Value = _condition.Params[i + 3] };
                                i += 2;
                            }
                            
                            values.Add(v);
                        }

                        Type type = GetTypeByValue(_condition.Params[0]);
                        // verify type and pass to value list to match with the list
                        _valueListEditor.Start(values.ToArray(), (H_EValueMode)_condition.Params[2], type);
                    }
                    else
                    {
                        _valueListEditor.Start(values.ToArray(), H_EValueMode.SINGLE_VALUE, typeof(int));
                    }
                    
                    if(!_valueListEditor.HasEventListener(ES_Event.ON_VALUE_CHANGE, OnValueListEditorChangeHandler))
                        _valueListEditor.AddEventListener(ES_Event.ON_VALUE_CHANGE, OnValueListEditorChangeHandler);
                    if(!_valueListEditor.HasEventListener(ES_Event.ON_CHANGE, OnValueListModeChangeHandler))
                        _valueListEditor.AddEventListener(ES_Event.ON_CHANGE, OnValueListModeChangeHandler);
                }
            }
        }

        private Type GetTypeByValue(object value)
        {
            Type type = null;

            if(value is string stringValue)
            {
                string[] values = stringValue.Split('.');

                if(values[0].Equals("Int32"))
                {
                    type = typeof(int);
                }
                else if (values[0].Equals("Single"))
                {
                    type = typeof(float);
                }
                else if (values[0].Equals("Boolean"))
                {
                    type = typeof(bool);
                }
                else if (values[0].Equals("Vector3"))
                {
                    type = typeof(Vector3);
                }
                else
                {
                    type = typeof(string);
                }
            }
            else
            {
                type = value.GetType();
            }

            return type;
        }

        private void OnValueListModeChangeHandler(ES_Event ev)
        {
            //H_EValueMode mode = (H_EValueMode)ev.Data;
            _condition.UpdateParam(2, ev.Data);
        }

        private void OnValueListEditorChangeHandler(ES_Event e)
        {
            List<object> param = new List<object>();

            param.Add(_condition.Params[0]);
            param.Add(_condition.Params[1]);
            param.Add(_condition.Params[2]);

            foreach(H_Val v in (H_Val[])e.Data)
            {
                param.Add(v.ValueType);
                
                param.Add(v.Value);

                if ((H_EValueMode)_condition.Params[2] == H_EValueMode.RANDOM_VALUE)
                {
                    param.Add(v.Weight.ValueType);
                    param.Add(v.Weight.Value);
                }

            }
            
            _condition.UpdateParams(param.ToArray());
        }

        public void Draw()
        {
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleLeft;
            style.richText = true;
            EditorGUILayout.SelectableLabel(string.Format("<color=#dddddd>Global UName:</color> <b><color=#81B4FF>{0}</color></b>", _condition.GlobalUName), style, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight+3f));//EditorStyles.linkLabel);

            _conditionReorderableList?.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Button(string.Format("Create On {0} Complete Scene Handler", _title));
        }

        public void Clear()
        {
            _title = string.Empty;
            _condition = null;
            _conditionReorderableList = null;
            _conditions.Clear();
        }

        private void OnRemoveElementHandler(ReorderableList list)
        {
            _condition.RemoveCondition(list.index);
            _conditions.RemoveAt(list.index);

            // select the up next element to facilitate delete operation
            if (list.index != 0)
                list.index = list.index - 1;
        }

        private void OnAddElementHandler(ReorderableList list)
        {
            H_ConditionEditor c = new H_ConditionEditor();
            H_Condition condition = new H_Condition() { Type = H_EConditionType.CHECK_VAR };
            if (list.index < 0 || list.count == 0)
            {
                //condition.UID = ObjectNamesManager.GetUniqueName(_condition.Conditions.Select(e=>e.UID).ToArray(), string.Concat(_condition.UID, ".c(0)"), "");//string.Concat(_condition.UID, ".c", 0);
                condition.UName = ObjectNamesManager.GetUniqueName(_condition.Conditions.Select(e=>e.UName).ToArray(), "c(0)", "");//string.Concat(_condition.UID, ".c", 0);
                //condition.ParentGlobalUID = _condition.GlobalUID;//.GlobalUID = string.Format("{0}.{1}", _condition.GlobalUID, condition.UID);
                c._condition = condition;
                _conditions.Add(c);
                _condition.AddCondition(condition);
                list.index = 0;
            }
            else
            {
                //condition.UID = ObjectNamesManager.GetUniqueName(_condition.Conditions.Select(e => e.UID).ToArray(), string.Concat(_condition.UID, ".c(0)"), "");//string.Concat(_condition.UID, ".c", list.index);
                condition.UName = ObjectNamesManager.GetUniqueName(_condition.Conditions.Select(e => e.UName).ToArray(), "c(0)", "");//string.Concat(_condition.UID, ".c", list.index);
                c._condition = condition;
                //condition.ParentGlobalUID = _condition.GlobalUID;
                _conditions.Insert(list.index + 1, c);
                _condition.InsertCondition(list.index + 1, condition);
                list.index++;
            }
            c.Start(condition);
        }


        private void OnReorderListHandler(ReorderableList list, int oldIndex, int newIndex)
        {
            _condition.SwapConditions(oldIndex, newIndex);
        }

        private float OnElementHeightHandler(int index)
        {
            if (_conditions[index]._condition.Type == H_EConditionType.CONDITION)
                return _conditions[index]._conditionReorderableList.GetHeight() + EditorGUIUtility.singleLineHeight * 2.5f;//(_conditions[index]._conditions.Count+6) * EditorGUIUtility.singleLineHeight;
            else if (_conditions[index]._condition.Type == H_EConditionType.ON_EVENT_DISPATCH
                || _conditions[index]._condition.Type == H_EConditionType.LISTEN_QUEST
                || _conditions[index]._condition.Type == H_EConditionType.TIMER)
                return EditorGUIUtility.singleLineHeight * 4 + 10;
            else if (_conditions[index]._condition.Type == H_EConditionType.CHECK_VAR
                || _conditions[index]._condition.Type == H_EConditionType.ON_CHANGE_VAR)
                return EditorGUIUtility.singleLineHeight * 3 + 10 + _conditions[index]._valueListEditor.Height;

            return EditorGUIUtility.singleLineHeight * 3 + 10;
        }

        void OnDrawHeaderHandler(Rect rect)
        {
            float w = rect.width / 2;

            rect.width = w;
            EditorGUI.LabelField(rect, _title);

            rect.x += rect.width;
            rect.width = GUI.skin.label.CalcSize(new GUIContent("repeat")).x + 9;
            EditorGUI.LabelField(rect, "Repeat:");

            rect.x += rect.width;
            rect.width = (w / 2) - rect.width;
            _condition.RepeatCount = EditorGUI.IntField(rect, _condition.RepeatCount);

            rect.x += rect.width;
            rect.width = w / 2;
            _condition.Operation = (H_EConditionOperation)EditorGUI.EnumPopup(rect, _condition.Operation);
        }

        void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
        {
            // just a safe condition to avoid null references
            if (_conditions[index]._condition.Type != H_EConditionType.CONDITION && _conditions[index]._condition.Params == null)
                _conditions[index]._condition.CreateParamsByType(_conditions[index]._condition.Type);

            // create a top padding distance
            //rect.y += 4;

            Rect origin = rect;
            float halfWidth = (origin.width / 2f);

            // draw the line between cells
            if (index < _conditions.Count - 1)
            {
                rect.y += rect.height - 2;
                rect.height = 2;

                EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f));
                rect = origin;
            }

            rect.height = EditorGUIUtility.singleLineHeight;

            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleLeft;
            style.richText = true;
            EditorGUI.SelectableLabel(rect, string.Format("<color=#dddddd>Global UName:</color> <b><color=#81B4FF>{0}</color></b>", _conditions[index]._condition.GlobalUName), style);//EditorStyles.linkLabel);
            
            rect.width = halfWidth;

            rect.y += EditorGUIUtility.singleLineHeight;

            //EditorGUI.BeginDisabledGroup(true);
            string newUID = EditorGUI.TextField(rect, _conditions[index]._condition.UName);

            if(newUID != _conditions[index]._condition.UName)
            {
                if (newUID.Replace(" ", string.Empty).Length != 0)
                    newUID = ObjectNamesManager.RemoveForbiddenCharacters(newUID);
                else
                    newUID = "c(0)";
                _conditions[index]._condition.UName = ObjectNamesManager.GetUniqueName(_condition.Conditions.Where(e=>e!= _conditions[index]._condition).Select(e => e.UName).ToArray(), newUID, "");
            }
            //EditorGUI.EndDisabledGroup();
            

            rect.x += rect.width;
            /*rect.width = 18f;
            GUI.Button(rect, "E");*/

            rect.width = halfWidth / 2;
            //rect.x += 18f;
            EditorGUI.BeginDisabledGroup(index == 0);
            _conditions[index]._condition.CombineOperation = (H_ECombineOperation)EditorGUI.EnumPopup(rect, _conditions[index]._condition.CombineOperation);
            if (index == 0 && _conditions[index]._condition.CombineOperation != H_ECombineOperation.JOIN)
            {
                _conditions[index]._condition.CombineOperation = H_ECombineOperation.JOIN;
            }
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width;

            EditorGUI.BeginChangeCheck();
            _conditions[index]._condition.Type = (H_EConditionType)EditorGUI.EnumPopup(rect, _conditions[index]._condition.Type);//types[EditorGUI.Popup(rect, Array.IndexOf(types, _conditions[index]._condition.Type), types)];

            if (EditorGUI.EndChangeCheck())
            {
                _conditions[index].Start(_conditions[index]._condition);
                //_conditions[index]._condition.CreateParamsByType(_conditions[index]._condition.Type);

            }

            rect.x = origin.x;

            rect.y += EditorGUIUtility.singleLineHeight;

            DrawParamsByConditionType(rect, origin, index, _conditions[index]._condition.Type);
        }
        
        private void DrawParamsByConditionType(Rect rect, Rect origin, int index, H_EConditionType type)
        {
            if (type == H_EConditionType.CHECK_VAR || type == H_EConditionType.ON_CHANGE_VAR)
            {
                DrawVarCondition(rect, origin, index);
            }
            else if (type == H_EConditionType.ON_EVENT_DISPATCH)
            {
                DrawEventDispatch(rect, origin, index);
            }
            else if (type == H_EConditionType.CONDITION)
            {
                DrawCondition(rect, origin, index);
            }
            else if (type == H_EConditionType.LISTEN_QUEST)
            {
                DrawListenQuest(rect, origin, index);
            }
            else if (type == H_EConditionType.TIMER)
            {
                DrawTimerOption(rect, origin, index);
            }
        }

        private void DrawVarCondition(Rect rect, Rect origin, int index)
        {
            if (_conditions[index]._valueEditor == null)
                _conditions[index]._valueEditor = new H_ValueEditor();

            object param0 = _conditions[index]._condition.Params[0];
            CVarCommands conditionOperation = (CVarCommands)_conditions[index]._condition.Params[1];
            H_EValueType type = (H_EValueType)_conditions[index]._condition.Params[2];

            rect.width = origin.width;
            rect.height = EditorGUIUtility.singleLineHeight;

            if(_conditions[index]._valueEditor.Draw(rect, string.Empty, null, ref type, ref param0))
            {
                _conditions[index]._condition.UpdateParam(0, param0);
                _conditions[index]._condition.UpdateParam(2, type);
            }

            //string[] groupsNames = CVarSystem.GetGroups().Select(e => e.Name).ToArray();
            //string[] varTypes = CVarSystem.AllowedTypes;

            //string[] param0 = ((string)_conditions[index]._condition.Params[0]).Split('.');
            //CVarCommands param3 = (CVarCommands)_conditions[index]._condition.Params[1];

            //rect.width = (origin.width-18) / 5;

            //EditorGUI.Popup(rect, 0, new string[] { "CVar", "Value", "Method" });

            //rect.x += rect.width;

            //rect.width = 18;
            //_conditions[index]._showStringFullname = EditorGUI.Toggle(rect, _conditions[index]._showStringFullname);
            //rect.x += 18;

            //rect.width = (origin.width-18) / 5;

            //if (!_conditions[index]._showStringFullname)
            //{
            //    int i = Array.FindIndex(varTypes, 0, varTypes.Length, e => e == param0[0]);
            //    int newValue = CheckableOptionPopup(rect, index, 0, varTypes);

            //    if (newValue != i)
            //    {
            //        _conditions[index]._condition.UpdateParam(2, GetNewDefaultValue(varTypes[newValue]));
            //    }

            //    rect.x += rect.width;

            //    CheckableOptionPopup(rect, index, 1, groupsNames);

            //    rect.x += rect.width;

            //    CheckableOptionPopup(rect, index, 2, CVarSystem.GetVarNamesByType(param0[0], param0[1]));


            //}
            //else
            //{
            //    rect.width = rect.width * 3f;

            //    string fullname = (string)_conditions[index]._condition.Params[0];//CVarSystem.GetFullName((string)_conditions[index]._condition.Params[2], (string)_conditions[index]._condition.Params[1], (string)_conditions[index]._condition.Params[0]);
            //    EditorGUI.BeginChangeCheck();
            //    fullname = EditorGUI.TextField(rect, fullname);
            //    if (EditorGUI.EndChangeCheck())
            //    {
            //        param0 = fullname.Split('.');

            //        // complete the name with the type using the first character as reference
            //        if (!CVarSystem.ValidateType(param0[0]))
            //        {
            //            if (param0[0].Length >= 1)
            //            {
            //                char firstC = param0[0].ToLower()[0];

            //                if (firstC == 'i')
            //                {
            //                    param0[0] = CVarSystem.GetTypeName<int>();
            //                }
            //                else if ((firstC == 's' && param0[0].Length >= 2 && param0[0].ToLower()[1] == 'i') || firstC == 'f')
            //                    param0[0] = CVarSystem.GetTypeName<float>();
            //                else if (firstC == 'b')
            //                    param0[0] = CVarSystem.GetTypeName<bool>();
            //                else if (firstC == 'v')
            //                    param0[0] = CVarSystem.GetTypeName<Vector3>();
            //                else
            //                    param0[0] = CVarSystem.GetTypeName<string>();
            //            }
            //            else
            //                param0[0] = CVarSystem.GetTypeName<string>();

            //            Debug.LogWarning(string.Format("Condition Editor: Invalid type. Type changed for {0}", param0[0]));

            //            fullname = string.Join(".", param0);
            //        }

            //        if (param0.Length < 3)
            //        {
            //            for (int i = param0.Length; i < 3; i++)
            //                fullname = string.Concat(fullname, ".");

            //            param0 = fullname.Split('.');
            //        }

            //        param0[1] = RemoveForbiddenCharacters(param0[1]);
            //        param0[2] = RemoveForbiddenCharacters(param0[2]);

            //        fullname = CVarSystem.GetFullName(param0[2], param0[0], param0[1]);

            //        _conditions[index]._condition.UpdateParam(0, fullname);

            //    }
            //}

            rect.width = (origin.width / 5);
            rect.x += rect.width*4+5;
            rect.width -= 5;
            EditorGUI.BeginChangeCheck();

            string paraType;

            if (type == H_EValueType.CVAR)
                paraType = ((string)param0).Split('.')[0];
            else
                paraType = param0.GetType().Name;   

            if (paraType == CVarSystem.GetTypeName<int>() || paraType == CVarSystem.GetTypeName<float>())
                conditionOperation = (CVarCommands)EditorGUI.EnumPopup(rect, conditionOperation);
            else
            {
                if (conditionOperation != CVarCommands.EQUAL && conditionOperation != CVarCommands.NOT_EQUAL)
                {
                    conditionOperation = CVarCommands.NOT_EQUAL;
                    _conditions[index]._condition.UpdateParam(1, conditionOperation);
                }

                int op = conditionOperation == CVarCommands.EQUAL ? 0 : 1;
                op = EditorGUI.Popup(rect, op, new string[] { "EQUAL", "NOT_EQUAL" });
                conditionOperation = op == 0 ? CVarCommands.EQUAL : CVarCommands.NOT_EQUAL;
            }
            if (EditorGUI.EndChangeCheck())
                _conditions[index]._condition.UpdateParam(1, conditionOperation);//*/

            //rect.x += rect.width;

            //EditorGUI.Popup(rect, 0, new string[] { "fix value","random value"  });

            rect.x = origin.x;
            rect.width = origin.width;
            rect.y += EditorGUIUtility.singleLineHeight;

            _conditions[index]._valueListEditor?.Draw(rect);
            //EditorGUI.BeginChangeCheck();
            //object param4 = DrawFieldByType(rect, _conditions[index]._condition.Params[2]);
            //if (EditorGUI.EndChangeCheck())
            //    _conditions[index]._condition.UpdateParam(2, param4);
        }

        private string RemoveForbiddenCharacters(string name)
        {
            return ObjectNamesManager.RemoveForbiddenCharacters(name);
        }

        private void DrawCondition(Rect rect, Rect origin, int index)
        {
            rect.width = origin.width;

            _conditions[index]._conditionReorderableList.DoList(rect);
        }

        private void DrawEventDispatch(Rect rect, Rect origin, int index)
        {
            if (!InstanceManager)
                InstanceManager = GameObject.FindObjectOfType<IS_InstanceSceneManager>();
            //rect.width = 18;
            //EditorGUI.Toggle(rect, false);
            //rect.x += 18;
            float targetSize = GUI.skin.label.CalcSize(new GUIContent("Target")).x;
            float identifierSize = GUI.skin.label.CalcSize(new GUIContent("Identifier")).x;
            float fieldSize = (origin.width - 18 - targetSize - identifierSize) / 3f;

            rect.width = targetSize;
            EditorGUI.LabelField(rect, "Target:");

            rect.x += rect.width + 9;
            rect.width = fieldSize;
            string param0 = (string)_conditions[index]._condition.Params[0];

            EditorGUI.BeginChangeCheck();
            _conditions[index]._objTarget = EditorGUI.ObjectField(rect, _conditions[index]._objTarget, typeof(UnityEngine.Object), true);

            if (_conditions[index]._objTarget)
            {
                param0 = (_conditions[index]._objTarget.GetEditorInstanceName() == string.Empty) ? _conditions[index]._objTarget.SetEditorInstanceName("__instance__") : _conditions[index]._objTarget.GetEditorInstanceName(false);
            }
            else
            {
                _conditions[index]._objTarget = GetObjectByString(param0);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (!_conditions[index]._objTarget)
                    param0 = string.Empty;

                _conditions[index]._condition.UpdateParam(0, param0);
            }

            rect.x += rect.width;

            if (_conditions[index]._objTarget)
            {
                Component[] cs = null;
                string[] s = null;

                if (_conditions[index]._objTarget is Component ob)
                {
                    cs = ob.GetComponents<Component>();
                    s = new string[cs.Length + 1];
                }
                else if (_conditions[index]._objTarget is GameObject ob2)
                {
                    cs = ob2.GetComponents<Component>();
                    s = new string[cs.Length + 1];
                }

                if (cs != null)
                {
                    int ind = 0;
                    int newIndex = 0;

                    s[0] = "GameObject";
                    for (int i = 0; i < cs.Length; i++)
                    {
                        s[i + 1] = string.Format("{0}. {1}", i, cs[i].GetType().Name);
                        if (cs[i] == _conditions[index]._objTarget)
                            ind = i + 1;
                    }

                    newIndex = EditorGUI.Popup(rect, ind, s);
                    //newIndex = EditorGUILayout.Popup(index, s);

                    if (newIndex != ind)
                    {
                        if (newIndex != 0)
                        {
                            _conditions[index]._objTarget = cs[newIndex - 1];
                        }
                        else
                        {
                            if (_conditions[index]._objTarget is Component)
                            {
                                _conditions[index]._objTarget = (_conditions[index]._objTarget as Component).gameObject;
                            }
                            else if (_conditions[index]._objTarget is GameObject)
                            {
                                _conditions[index]._objTarget = (_conditions[index]._objTarget as GameObject);
                            }
                        }

                        param0 = (_conditions[index]._objTarget.GetEditorInstanceName() == string.Empty) ? _conditions[index]._objTarget.SetEditorInstanceName("__instance__") : _conditions[index]._objTarget.GetEditorInstanceName(false);
                        _conditions[index]._condition.UpdateParam(0, param0);
                        //_id.stringValue = (_objTarget.GetEditorInstanceName() == string.Empty) ? _objTarget.SetEditorInstanceName("__instance__") : _objTarget.GetEditorInstanceName(false);
                    }
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.Popup(rect, 0, new string[] { "none" });
                EditorGUI.EndDisabledGroup();
            }

            rect.x += rect.width;
            rect.width = identifierSize;
            EditorGUI.LabelField(rect, "Identifier:");

            rect.x += rect.width + 9;
            rect.width = fieldSize;
            EditorGUI.BeginChangeCheck();
            param0 = EditorGUI.TextField(rect, param0);
            if (EditorGUI.EndChangeCheck())
            {
                if (InstanceManager != null)
                {
                    _conditions[index]._objTarget = GetObjectByString(param0);
                }
                _conditions[index]._condition.UpdateParam(0, param0);
            }

            rect.x = origin.x;
            rect.width = origin.width;
            rect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            string param1 = (string)_conditions[index]._condition.Params[1];
            param1 = EditorGUI.TextField(rect, "Event Name:", param1);
            if (EditorGUI.EndChangeCheck())
            {
                _conditions[index]._condition.UpdateParam(1, param1);
            }
                //EditorGUI.Popup(rect, 0, new string[] { "ON_TUTORIAL_COMPLETE", "ON_GAME_OVER" });
        }

        private UnityEngine.Object GetObjectByString(string objID)
        {
            if (InstanceManager && InstanceManager.ContainsValue(objID))
            {
                return (UnityEngine.Object)InstanceManager.GetKeyByValue(objID);
            }
            else
            {
                return null;
            }
        }

        private void DrawListenQuest(Rect rect, Rect origin, int index)
        {
            rect.width = origin.width;
            EditorGUI.BeginChangeCheck();
            bool param2 = EditorGUI.ToggleLeft(rect, "Start Quest If Deactivated", (bool)_conditions[index]._condition.Params[2]);
            if (EditorGUI.EndChangeCheck())
            {
                _conditions[index]._condition.UpdateParam(2, param2);
            }

            rect.y += EditorGUIUtility.singleLineHeight;

            rect.width = 18;
            _conditions[index]._showStringFullname = EditorGUI.Toggle(rect, _conditions[index]._showStringFullname);
            rect.x += 18;

            if (!_conditions[index]._showStringFullname)
            {
                string[] param0 = ((string)_conditions[index]._condition.Params[0]).Split('.');

                rect.width = (origin.width - 18) / 3;
                List<string> groups = new List<string>();
                groups.AddRange(H_QuestManager.Instance.QuestGroups.GetGroups().Select(e => e.Name).ToArray());
                
                int option = groups.IndexOf(param0[0]);

                if(option == -1)
                {
                    GUI.contentColor = _missingColor;
                    groups.Insert(0, string.Concat("<missing>", param0[0]));
                    option = 0;
                }

                if (CheckablePopup(rect, option, groups.ToArray(), out option))
                {
                    _conditions[index]._condition.UpdateParam(0, string.Concat(groups[option], ".", param0[1]));
                }
                //
                rect.x += rect.width;
                string[] quests;

                H_DataGroup<H_Quest, H_PersistentQuestData> group = H_QuestManager.Instance.QuestGroups.GetGroupByName(groups[option]);

                int option2 = -1;
                if (group != null)
                {
                    GUI.contentColor = Color.white;
                    quests = group.Data.Select(e => e.UName).ToArray();
                    option2 = Array.IndexOf(quests, param0[1]);
                    if(option2 == -1)
                    {
                        GUI.contentColor = _missingColor;
                        quests = new string[] { string.Concat("<missing>", param0[1]) }.Concat(quests).ToArray();
                        option2 = 0;
                    }
                }
                else
                {
                    GUI.contentColor = _missingColor;
                    quests = new string[] { string.Concat("<missing>", param0[1]) };
                    option2 = 0;
                }

                //lastOption = option2;
                //option2 = EditorGUI.Popup(rect, option2, quests);
                if (CheckablePopup(rect, option2, quests, out option2))
                {
                    _conditions[index]._condition.UpdateParam(0, string.Concat(groups[option], ".", quests[option2]));
                }
                rect.x += rect.width;
                GUI.contentColor = Color.white;
            }
            else
            {
                rect.width = (origin.width - 18) / 3 * 2;
                EditorGUI.BeginChangeCheck();
                string aux = EditorGUI.TextField(rect, (string)_conditions[index]._condition.Params[0]);
                if (EditorGUI.EndChangeCheck())
                {
                    string[] parts = aux.Split('.');
                    if(parts.Length < 2)
                    {
                        parts = new string[] { aux, string.Empty };
                    }

                    parts[0] = RemoveForbiddenCharacters(parts[0]);
                    parts[1] = RemoveForbiddenCharacters(parts[1]);

                    aux = string.Join(".", parts[0], parts[1]);

                    _conditions[index]._condition.UpdateParam(0, aux);
                }

                rect.x += rect.width;
                rect.width = (origin.width - 18) / 3;
            }

            EditorGUI.Popup(rect, 0, new string[] { "ON_COMPLETE", "ON_GOAL_UPDATE", "ON_FAIL" });
        }

        private void DrawTimerOption(Rect rect, Rect origin, int index)
        {
            bool OnUnloadScene = (bool)_conditions[index]._condition.Params[3];
            OnUnloadScene = EditorGUI.ToggleLeft(rect, "Reset On Unload Scene", OnUnloadScene);
            if(OnUnloadScene != (bool)_conditions[index]._condition.Params[3])
                _conditions[index]._condition.UpdateParam(3, OnUnloadScene);

            rect.y += EditorGUIUtility.singleLineHeight;

            //rect.width = 18;
            //EditorGUI.Toggle(rect, false);
            //
            //rect.x += 18;
            rect.width = (origin.width) / 3;
            string aux = (string)_conditions[index]._condition.Params[0];
            EditorGUI.BeginChangeCheck();
            aux = EditorGUI.TextField(rect, aux);
            if (EditorGUI.EndChangeCheck())
            {   
                _conditions[index]._condition.UpdateParam(0, RemoveForbiddenCharacters(aux));
            }
            //
            rect.x += rect.width;
            EditorGUI.BeginChangeCheck();
            H_ETimeMode timerMode = (H_ETimeMode)EditorGUI.EnumPopup(rect, (H_ETimeMode)_conditions[index]._condition.Params[1]);
            if (EditorGUI.EndChangeCheck())
            {
                _conditions[index]._condition.UpdateParam(1, timerMode);
            }
            //
            rect.x += rect.width;
            float time = (float)_conditions[index]._condition.Params[2];
            EditorGUI.BeginChangeCheck();
            time = EditorGUI.FloatField(rect, time);
            if (EditorGUI.EndChangeCheck())
            {
                _conditions[index]._condition.UpdateParam(2, time);
            }
        }

        private static object GetNewDefaultValue(string type)
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

            return null;
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

        public int CheckableOptionPopup(Rect rect, int conditionIndex, int paramIndex, string[] displayableOptions)
        {
            string[] param0 = ((string)_conditions[conditionIndex]._condition.Params[0]).Split('.');

            List<string> displayableOptionsWithAdd = new List<string>();

            int oldIndex = Array.FindIndex(displayableOptions, 0, displayableOptions.Length, (e) => e == param0[paramIndex]);

            if (oldIndex == -1)
            {
                displayableOptionsWithAdd.Add(string.Concat("<missing>", param0[paramIndex]));
                displayableOptionsWithAdd.InsertRange(1, displayableOptions);
                oldIndex = 0;
                GUI.contentColor = _missingColor;
            }
            else
            {
                displayableOptionsWithAdd.InsertRange(0, displayableOptions);
            }

            if (CheckablePopup(rect, oldIndex, displayableOptionsWithAdd.ToArray(), out int index))
            {
                GUI.contentColor = Color.white;
                string s = string.Empty;
                for (int i = 0; i < param0.Length; i++)
                {
                    if (i != paramIndex)
                        s += param0[i];
                    else
                        s += displayableOptionsWithAdd[index];

                    if (i < param0.Length - 1)
                        s += ".";
                }
                _conditions[conditionIndex]._condition.UpdateParam(0, s);
                return index;
            }
            GUI.contentColor = Color.white;
            return oldIndex;
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