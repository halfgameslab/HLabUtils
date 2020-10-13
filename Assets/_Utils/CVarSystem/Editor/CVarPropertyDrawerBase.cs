﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

[CustomPropertyDrawer(typeof(CVar<>), true)]
public class CVarPropertyDrawerBase : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.type.Contains("String"))
            DrawProperty<string>(position, property, label);
        else if (property.type.Contains("Int"))
            DrawProperty<int>(position, property, label);
        else if (property.type.Contains("Float") || property.type.Contains("Single"))
            DrawProperty<float>(position, property, label);
        else if (property.type.Contains("Bool"))
            DrawProperty<bool>(position, property, label);
        else
            EditorGUILayout.HelpBox("Type not suported", MessageType.Warning);
        
    }
    public static void DrawProperty<T>(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        //Debug.Log(property.type);

        // Draw label
        //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        string key = property.FindPropertyRelative("_name").stringValue;
        string groupName = property.FindPropertyRelative("_groupName").stringValue;

        int address = property.FindPropertyRelative("_address").intValue;

        EditorGUI.LabelField(new Rect(position.x, position.y, position.width * 0.3f, position.height), label);

        //property.FindPropertyRelative("_groupName").stringValue = 
        string[] groups = CVarSystem.GetGroups().Select(x => x.Name).ToArray();
        int indexOfCurrentGroup = Array.IndexOf(groups, groupName);
        int selectedGroup = EditorGUI.Popup(new Rect(position.x + position.width * 0.12f, position.y, position.width * 0.18f, position.height), indexOfCurrentGroup, groups);
        
        if (selectedGroup != indexOfCurrentGroup)
        {
            groupName = groups[selectedGroup];
            property.FindPropertyRelative("_groupName").stringValue = groupName;
            address = property.FindPropertyRelative("_address").intValue = CVarSystem.GetAddress<T>(key, groupName);
            property.serializedObject.ApplyModifiedProperties();
        }

        DrawNamePopup<T>(new Rect(position.x + position.width * 0.3f, position.y, position.width * 0.3f, position.height), key, address, groupName, property);

        T value;

        if (CVarSystem.ContainsVarAt(address))
        {
            EditorGUI.BeginDisabledGroup(CVarSystem.GetLockedAt<T>(address));
            value = (T)DrawFieldByType(new Rect(position.x + position.width * 0.60f, position.y, position.width * 0.4f -20, position.height), CVarSystem.GetValueAt<T>(address, (T)GetDefault<T>()));
            EditorGUI.EndDisabledGroup();

            // draw Edit button at the end of line
            if (GUI.Button(new Rect(position.x + position.width -17, position.y, 17, position.height), "E"))
            {
                CVarWindow.ShowWindow(groupName);
            }

            if (value != null)
            {
                CVarSystem.SetValueAt<T>(address, value);
                //SetPropertyValue<T>(property, value);
            }
        }
        else if (CVarSystem.ContainsVar<T>(key, groupName))
        {
            EditorGUI.BeginDisabledGroup(CVarSystem.GetLocked<T>(key, groupName));
            value = (T)DrawFieldByType(new Rect(position.x + position.width * 0.60f, position.y, position.width * 0.4f -20, position.height), CVarSystem.GetValue<T>(key, (T)GetDefault<T>(), groupName));
            EditorGUI.EndDisabledGroup();

            // draw Edit button at the end of line
            if (GUI.Button(new Rect(position.x + position.width - 17, position.y, 17, position.height), "E"))
            {
                CVarWindow.ShowWindow(groupName);
            }

            if (value != null)
            {
                CVarSystem.SetValue<T>(key, value, groupName);
                //SetPropertyValue<T>(property, value);
            }
        }
        else if (key != null && key.Length > 0)
            if (GUI.Button(new Rect(position.x + position.width * 0.60f, position.y, position.width * 0.4f, position.height), new GUIContent("Fix", "Click to create var at selected group.")))
            {
                CVarSystem.SetValue<T>(key, (T)GetDefault<T>(), groupName);
            }

        

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
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

        return null;
    }

    private static object GetDefault<T>()
    {
        if (typeof(T) == typeof(string))
            return string.Empty;
        else if (typeof(T) == typeof(int))
            return 0;
        else if (typeof(T) == typeof(float))
            return 0.0f;
        else if (typeof(T) == typeof(bool))
            return false;

        return null;
    }
    
    /*private static void SetPropertyValue<T>(SerializedProperty property, object value)
    {
        if (typeof(T) == typeof(string))
            property.FindPropertyRelative("_value").stringValue = (string)value;
        else if (typeof(T) == typeof(int))
            property.FindPropertyRelative("_value").intValue = (int)value;
        else if (typeof(T) == typeof(float))
            property.FindPropertyRelative("_value").floatValue = (float)value;
        else if (typeof(T) == typeof(bool))
            property.FindPropertyRelative("_value").boolValue = (bool)value;

        property.serializedObject.ApplyModifiedProperties();
    }*/

    public static void DrawNamePopup<T>(Rect rect, string name, int address, string groupName, SerializedProperty property)
    {
        List<string> names = new List<string>(CVarSystem.GetVarNamesByType<T>(groupName));
        int currentSelecion = 0;

        // if there is some var at the current address
        if (CVarSystem.ContainsVarAt(address))
        {
            currentSelecion = names.IndexOf(CVarSystem.GetNameAt<T>(address, groupName)); // get the position on vector for the var
        }
        else if (CVarSystem.ContainsVar<T>(name, groupName)) // else if the address are unsinging and the name still valid
        {
            currentSelecion = names.IndexOf(name);
            //names.Insert(0, "<moved>." + name); // if last name still valid but the current address not give user the hability to rebind
        }
        else if (name.Length > 0)
        {
            names.Insert(0, "<missing>." + name); // if name, address and name that was setup once are invalid show missing message
        }
        else
        {
            names.Insert(0, "<undefined>");// if name wasnt setup
        }

        names.Add("");
        names.Add("Add CVar");

        int selection = EditorGUI.Popup(rect, currentSelecion, names.ToArray());

        if (selection != currentSelecion)
        {
            if (CVarSystem.ContainsVar<T>(names[selection], groupName) && selection != names.Count - 1)
            {
                OnClickNameButtonHandler<T>(new HelpProperty { Property = property, SelectedName = names[selection], GroupName = groupName });
            }
            else if (selection == names.Count - 1)
            {
                OnAddNewClickHandler();
            }
        }
    }

    private static void OnAddNewClickHandler()
    {
        CVarWindow.ShowWindow();
    }

    private static void OnClickNameButtonHandler<T>(HelpProperty obj)
    {
        HelpProperty p = (HelpProperty)obj;

        p.Property.FindPropertyRelative("_name").stringValue = p.SelectedName;
        //p.Property.
        p.Property.FindPropertyRelative("_address").intValue = CVarSystem.GetAddress<T>(p.SelectedName, p.GroupName);

        p.Property.serializedObject.ApplyModifiedProperties();
    }

}

internal struct HelpProperty
{
    public string SelectedName { get; set; }

    public string GroupName { get; set; }
    public SerializedProperty Property { get; set; }
}