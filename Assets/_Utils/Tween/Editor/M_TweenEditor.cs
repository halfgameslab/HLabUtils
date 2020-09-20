using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

[CustomEditor(typeof(M_Tween))]
public class M_TweenEditor :  Editor{

    private ReorderableList _actionList;
    private ReorderableList _eventList;
    private M_Tween _tweenManager;
    private void OnEnable()
    {
        _tweenManager = ((M_Tween)serializedObject.targetObject);

        UpdateSelfProperties(_tweenManager);
        SetupReorderedActionList();
        SetupReorderedEventList();
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _eventList.displayAdd = _tweenManager.EventList.Count != M_Tween.EVENT_NAMES.Length;
        _eventList.displayRemove = _tweenManager.EventList.Count != 0;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_playOnEnable"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_autoKill"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_delay"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_timeScale"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_ignoreTimeScale"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_updateType"));

        SerializedProperty repeatCount = serializedObject.FindProperty("_repeatCount");
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_repeatCount"));

        if (repeatCount.intValue != 0)
        {   
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_repeatType"));
        }
        
        _actionList.DoLayoutList();
        _eventList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void SetupReorderedEventList()
    {
        _eventList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("_eventList"),
                false, true, true, true);

        _eventList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Callbacks");
        };

        _eventList.drawElementCallback = 
        (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = _eventList.serializedProperty.GetArrayElementAtIndex(index);//get current element
            
            //show on inspector
            EditorGUI.PropertyField( new Rect(rect.x, rect.y, rect.width, rect.height), element.FindPropertyRelative("_callback"), new GUIContent(element.FindPropertyRelative("_name").stringValue));
        };


        _eventList.elementHeightCallback = (index) => {
            Repaint();
            float height = 0;

            //get the number of lines the event layout use
            height = _tweenManager.EventList[index].Callback.GetPersistentEventCount() * 2;

            height = ((EditorGUIUtility.singleLineHeight + 5.5f) * (height != 0 ? 3 + height : 5 + height ));
            
            return height;
        };
        
        _eventList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
            if (M_Tween.EVENT_NAMES.Length > _tweenManager.EventList.Count)//only populate the menu if all events wasnt created
            {
                var menu = new GenericMenu();
                
                foreach (string s in M_Tween.EVENT_NAMES)//for each possible event
                {
                    if (_tweenManager.EventList.FindIndex(obj => obj.EventName == s) == -1)//if the event wasnt created
                    {
                        //create a menu option
                        menu.AddItem(new GUIContent(s),
                        false, OnClickAddEventHandler,
                        s);
                    }
                }

                menu.ShowAsContext();//show menu
            }
            else
            {
                Debug.LogWarning("There isnt callbacks for insert!");
            }
        };

    }

    int _count = 0;
    private void SetupReorderedActionList()
    {
        _actionList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("_actions"),
                true, true, true, true);

        _actionList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Animations");
        };

        _actionList.drawElementCallback =
    (Rect rect, int index, bool isActive, bool isFocused) =>
    {
        

        SerializedProperty element = _actionList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty from;
        SerializedProperty to;

        M_TweenTypeV2 elementType = (M_TweenTypeV2)element.FindPropertyRelative("_type").enumValueIndex;
        SerializedProperty isRelativeProperty = element.FindPropertyRelative("_isRelative");

        rect.y += 2;

        if (index == 0)
            _count = 1;
        else if (element.FindPropertyRelative("_addType").enumValueIndex == 0)
        {
            _count++;
        }

        EditorGUI.LabelField(
            new Rect(rect.x, rect.y + 2, rect.width * 0.1f, EditorGUIUtility.singleLineHeight),
            String.Concat("#",_count.ToString().PadLeft(3-_count.ToString().Length, '0')));

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(
            new Rect(rect.x+ rect.width * 0.1f, rect.y + 2, rect.width * 0.2f, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("_type"), GUIContent.none);

        if (EditorGUI.EndChangeCheck())
        {
            base.serializedObject.ApplyModifiedProperties();
            UpdateTarget(_tweenManager.Actions[index]);
        }

        EditorGUI.PropertyField(
            new Rect(rect.x + rect.width * 0.3f, rect.y + 2, rect.width * 0.2f, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("_addType"), GUIContent.none);

        if (elementType == M_TweenTypeV2.ROTATE)
        {
            EditorGUI.PropertyField(
                new Rect(rect.x + rect.width * 0.5f, rect.y + 2, rect.width * 0.3f, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("_rotationMode"), GUIContent.none);
        }

        float w = rect.width / 3;

        if ((int)elementType <= 5)//only for vector3 tweens, color and fade
        {
            EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.8f, rect.y + 2, rect.width * 0.25f - 15f, EditorGUIUtility.singleLineHeight), "Additive");
            EditorGUI.PropertyField(new Rect(rect.x + (w * 3) - 15, rect.y + 2, 30, EditorGUIUtility.singleLineHeight), isRelativeProperty, GUIContent.none);
        }

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(new Rect(rect.x, rect.y + 4 + EditorGUIUtility.singleLineHeight, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("_targetType"), GUIContent.none);
        if (EditorGUI.EndChangeCheck())
        {
            base.serializedObject.ApplyModifiedProperties();
            //if target type change
            if(_tweenManager.Actions[index].TargetType == M_TweenTargetType.SELF)
            {
                _tweenManager.Actions[index].Target = _tweenManager;
            }

            UpdateTarget(_tweenManager.Actions[index]);
        }

        //get target
        GUI.enabled = element.FindPropertyRelative("_targetType").enumValueIndex != 0;
        GUI.backgroundColor = _tweenManager.Actions[index].Target != null?Color.white:Color.red;
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(new Rect(rect.x+ rect.width * 0.3f, rect.y + 4 + EditorGUIUtility.singleLineHeight, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("_target"), GUIContent.none);
        if (EditorGUI.EndChangeCheck())
        {
            base.serializedObject.ApplyModifiedProperties();
            UpdateTarget(_tweenManager.Actions[index]);
            //if target type change
        }
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;

        EditorGUI.LabelField(
        new Rect(rect.x, rect.y + (2 + EditorGUIUtility.singleLineHeight)*2, w, EditorGUIUtility.singleLineHeight),
        "Duration");

        EditorGUI.LabelField(
            new Rect(rect.x + (w), rect.y + (2 + EditorGUIUtility.singleLineHeight)*2, w, EditorGUIUtility.singleLineHeight),
            "Delay");

        EditorGUI.LabelField(
            new Rect(rect.x + (w * 2), rect.y + (2 + EditorGUIUtility.singleLineHeight)*2, w, EditorGUIUtility.singleLineHeight),
            "Use Ease");

        SerializedProperty useEase = element.FindPropertyRelative("_useEase");

        //use custom courve
        EditorGUI.PropertyField(
            new Rect(rect.x + (w * 3) - 15, rect.y + (2 + EditorGUIUtility.singleLineHeight)*2, 30, EditorGUIUtility.singleLineHeight),
            useEase, GUIContent.none);


        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 3, w - 1, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("_duration"), GUIContent.none);

        EditorGUI.PropertyField(
            new Rect(rect.x + 1 + (w), rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 3, w - 1, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("_delay"), GUIContent.none);

        if (useEase.boolValue)
        {
            EditorGUI.PropertyField(
                new Rect(rect.x + 3 + (w * 2), rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 3, w - 1, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("_ease"), GUIContent.none);
        }
        else
        {
            EditorGUI.PropertyField(
                new Rect(rect.x + 3 + (w * 2), rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 3, w - 1, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("_customEaseCourve"), GUIContent.none);
        }

        //EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 3, rect.width, EditorGUIUtility.singleLineHeight), from, new GUIContent("From"));

        if ((int)elementType <= 3 || (int)elementType >= 6)//move rotate scale
        {
            from = element.FindPropertyRelative("_fromVector3");
            to = element.FindPropertyRelative("_toVector3");
        }

        else if ((int)elementType == 4)//color
        {
            from = element.FindPropertyRelative("_fromColor");
            to = element.FindPropertyRelative("_toColor");
        }
        else//alfa
        {
            from = element.FindPropertyRelative("_fromFloat");
            to = element.FindPropertyRelative("_toFloat");
        }

        if ((int)elementType <= 5)
        {
            SerializedProperty useFrom = element.FindPropertyRelative("_useFrom");

            EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 5, 15f, EditorGUIUtility.singleLineHeight), useFrom, GUIContent.none);
            EditorGUI.LabelField(new Rect(rect.x + 15f, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 5, rect.width * 0.15f, EditorGUIUtility.singleLineHeight), "From");

            if (useFrom.boolValue)
                EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.2f, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 5, rect.width * 0.8f, EditorGUIUtility.singleLineHeight), from, GUIContent.none);
            else
                EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.2f, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 5, rect.width * 0.8f, EditorGUIUtility.singleLineHeight), "Tween will use the current property value!");
        }
        else
        {
            if ((int)elementType >= 9)
            {
                EditorGUI.LabelField(
            new Rect(rect.x, rect.y + (2 + EditorGUIUtility.singleLineHeight) * 5, w, EditorGUIUtility.singleLineHeight),
            "Vibrato");

                EditorGUI.LabelField(
                    new Rect(rect.x + (w), rect.y + (2 + EditorGUIUtility.singleLineHeight) * 5, w, EditorGUIUtility.singleLineHeight),
                    "Elasticity");


                EditorGUI.PropertyField(
                new Rect(rect.x, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 6, w - 1, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("_vibrato"), GUIContent.none);


                EditorGUI.PropertyField(
                new Rect(rect.x + 1 + (w), rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 6, w - 1, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("_elasticity"), GUIContent.none);


                EditorGUI.LabelField(
                new Rect(rect.x + (w * 2), rect.y + (2 + EditorGUIUtility.singleLineHeight) * 5, w, EditorGUIUtility.singleLineHeight),
                "Randomness");

                EditorGUI.PropertyField(
                    new Rect(rect.x + 3 + (w * 2), rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 6, w - 1, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("_randomness"), GUIContent.none);
            }
            else
            {
                EditorGUI.LabelField(
            new Rect(rect.x, rect.y + (2 + EditorGUIUtility.singleLineHeight) * 5, w / 2, EditorGUIUtility.singleLineHeight),
            "Vibrato");


                EditorGUI.PropertyField(
                new Rect(rect.x + (w / 2), rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 5, w / 2, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("_vibrato"), GUIContent.none);

                EditorGUI.LabelField(
                    new Rect(rect.x + (w), rect.y + (2 + EditorGUIUtility.singleLineHeight) * 5, w / 2, EditorGUIUtility.singleLineHeight),
                    "Strength");

                EditorGUI.PropertyField(
                new Rect(rect.x + 1 + ((w / 2) * 3), rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 5, w / 2, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("_elasticity"), GUIContent.none);
            }
        }

        if ((int)elementType <= 8)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 6, rect.width * 0.2f, EditorGUIUtility.singleLineHeight), (int)elementType < 6 ? !isRelativeProperty.boolValue ? "To" : "To Add" : "Punch");

            EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.2f, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 6, rect.width * 0.8f, EditorGUIUtility.singleLineHeight), to, GUIContent.none);
        }

        EditorGUI.LabelField(new Rect(rect.x, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 4, rect.width * 0.25f, EditorGUIUtility.singleLineHeight), "Repeat Count");
        EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.25f, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 4, rect.width * 0.25f, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("_repeatCount"), GUIContent.none);

        if (element.FindPropertyRelative("_repeatCount").intValue != 0)//if repeat is -1 or repeat is more than 0
        {
            //show repeat type selection
            EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.5f, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 4, rect.width * 0.25f, EditorGUIUtility.singleLineHeight), "Repeat Type");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.75f, rect.y + 2 + (2 + EditorGUIUtility.singleLineHeight) * 4, rect.width * 0.25f, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("_loopType"), GUIContent.none);

            if (element.FindPropertyRelative("_repeatCount").intValue < 0)
            {
                element.FindPropertyRelative("_repeatCount").intValue = 1;//cant infinite loop
                Debug.LogWarning("Can't infinite loop one tween because it will crash the sequence. If you want infinite loop try -1 on sequence repeat count or use script line tween.");
            }
        }
        if (index < _actionList.serializedProperty.arraySize - 1)
            //draw a little line to mark the end of box
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height, rect.width, 1), Color.gray);
    };


        _actionList.elementHeightCallback = (index) => {
            Repaint();
            float height = 0;

            //if(EditorGUIUtility.currentViewWidth > 345)//345 is the min size to down the vector3 objects
            height = (EditorGUIUtility.singleLineHeight + 3) * 7;
            /*else
                height = (EditorGUIUtility.singleLineHeight + 3) * 6;*/



            //height += (index % 2) * EditorGUIUtility.singleLineHeight;

            /*try
            {
                height = heights[index];
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogWarning(e.Message);
            }
            finally
            {
                float[] floats = heights.ToArray();
                Array.Resize(ref floats, prop.arraySize);
                heights = floats.ToList();
            }*/

            return height;
        };

        _actionList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
            var menu = new GenericMenu();

            foreach (M_TweenTypeV2 type in System.Enum.GetValues(typeof(M_TweenTypeV2)))
            {
                menu.AddItem(new GUIContent(type.ToString()),
                false, OnClickAddActionHandler,
                new M_TweenAction() { Target=target, TweenType = type });
            }

            menu.ShowAsContext();
        };

        //https://forum.unity.com/threads/reorderablelist-in-the-custom-editorwindow.384006/
        _actionList.onSelectCallback += (ReorderableList itemList) =>
        {
            Debug.Log(itemList.index);
        };
    }

    private T TryGetComponent<T>(UnityEngine.Object obj) where T:Component
    {
        T response = default;
        
        if (obj != null)
        {
            if (obj is GameObject)
                response = (obj as GameObject).GetComponent<T>();
            else
                response = (obj as Component).GetComponent<T>();
        }

        return response;
    }

    private void UpdateTarget(M_TweenAction action)
    {
        UnityEngine.Object target = null;

        if ((int)action.TweenType <= 3 || (int)action.TweenType >= 6)//check if type require transform
        {
            target = TryGetComponent<Transform>(action.Target);
        }
        else if((target = TryGetComponent<Renderer>(action.Target)) == null)
        {
            if ((target = TryGetComponent<Image>(action.Target)) == null)//color and fade require type that has color field
            {
                target = TryGetComponent<Text>(action.Target);
            }
        }

        action.Target = target;
    }

    /// <summary>
    /// Update the component when enable
    /// Helps if the component was copy to another
    /// </summary>
    /// <param name="tweenManager"></param>
    private void UpdateSelfProperties(M_Tween tweenManager)
    {
        foreach(M_TweenAction action in tweenManager.Actions)
        {
            if (action.TargetType == M_TweenTargetType.SELF)
            {
                action.Target = tweenManager.gameObject;
                UpdateTarget(action);
            }
        }
    }

    private void OnClickAddActionHandler(object target)
    {
        M_TweenAction action = (M_TweenAction)target;

        Undo.RecordObject(_tweenManager, "Save Before Add Action");

        _tweenManager.Actions.Add(action);
        //_actionList.serializedProperty.arraySize++;

        //_actionList.serializedProperty.InsertArrayElementAtIndex(_actionList.serializedProperty.arraySize-1);
        //SerializedProperty element = _actionList.serializedProperty.GetArrayElementAtIndex(_actionList.serializedProperty.arraySize - 1);
        //element.FindPropertyRelative("_type").enumValueIndex = (int)action.TweenType;
        
        UpdateTarget(action);

        /*int index = list.serializedProperty.arraySize;
        list.serializedProperty.arraySize++;
        list.index = index;

        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("_type").enumValueIndex = (int)action.TweenType;
        element.FindPropertyRelative("_duration").floatValue = action.Duration;
        element.FindPropertyRelative("_easy").enumValueIndex = (int)action.Easy;
        element.FindPropertyRelative("_target").objectReferenceValue = action.Target;
        //action.From = Vector3.zero;
        ((M_TweenVector3Action)action).To = Vector3.zero;*/

        serializedObject.ApplyModifiedProperties();
        /*var data = (WaveCreationParams)target;
        var index = list.serializedProperty.arraySize;
        list.serializedProperty.arraySize++;
        list.index = index;
        var element = list.serializedProperty.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("Type").enumValueIndex = (int)data.Type;
        element.FindPropertyRelative("Count").intValue =
            data.Type == MobWave.WaveType.Boss ? 1 : 20;
        element.FindPropertyRelative("Prefab").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath(data.Path, typeof(GameObject)) as GameObject;
        serializedObject.ApplyModifiedProperties();*/
    }

    private void OnClickAddEventHandler(object target)
    {
        Debug.Log((string)target);

        M_TweenUnityEvent ev = new M_TweenUnityEvent()
        {
            EventName = (string)target,
            Callback = new UnityEvent()
        };
        
        Undo.RecordObject(_tweenManager, "Save Before Add Event");

        _tweenManager.EventList.Add(ev);

        serializedObject.ApplyModifiedProperties();
    }

}
