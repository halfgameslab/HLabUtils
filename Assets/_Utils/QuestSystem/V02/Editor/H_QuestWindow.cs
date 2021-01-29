using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class H_QuestWindow : EditorWindow
{
    QuestEditor e = new QuestEditor();

    [MenuItem("HLab/Quest")]
    public static void ShowWindow()
    {
        H_QuestWindow ow = null;

        if (HasOpenInstances<H_QuestWindow>())
            ow = GetWindow<H_QuestWindow>(false, "Quests", true);

        H_QuestWindow editorWindow = CreateWindow<H_QuestWindow>("Quests");

        if (ow != null)
            editorWindow.position = new Rect(ow.position.x + 20f, ow.position.y + 20f, ow.position.width, ow.position.height);

        ConfigureWindow(editorWindow);
    }

    public void OnEnable()
    {
        e.Start();
    }

    public static void ConfigureWindow(H_QuestWindow window)
    {
        window.autoRepaintOnSceneChange = true;
        window.minSize = new Vector2(450, window.minSize.y);
        window.Show();
    }

    private bool DefaultTextureButton(string textureName, string hint, float w = 50, float h = 40)
    {
        return GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture(textureName), hint), GUILayout.Width(w), GUILayout.Height(h));
    }

    public void OnGUI()
    {
        float firstCollumWidth = EditorGUIUtility.currentViewWidth / 3f;

        //EditorGUI.DrawRect(new Rect(0, 0, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight*2), Color.blue);

        EditorGUILayout.BeginHorizontal();
        DefaultTextureButton("d_Collab.FileAdded", "Create New Group");
        DefaultTextureButton("d_Refresh@2x", "Reload");
        DefaultTextureButton(CVarSystem.IsEditModeActived ? "d_PlayButton@2x" : "d_PauseButton@2x", CVarSystem.IsEditModeActived ? "Disable Edit Mode" : "Enable Edit Mode");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Popup("Group (id)", 0, new string[] { "group a", "group b", "group c" });
        GUILayout.Button("E", GUILayout.Width(18));
        GUILayout.Button("-", GUILayout.Width(18));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Popup("Persistent Type", 0, new string[] { "SHARED", "PER_SCENE", "CUSTOM" });
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.MinWidth(firstCollumWidth));

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Quest", GUILayout.MinWidth(firstCollumWidth/4));
        GUIStyle searchStyle = GUI.skin.FindStyle("ToolbarSeachTextField");
        
        EditorGUILayout.TextField("", searchStyle);

        GUILayout.Button("+", GUILayout.MinWidth(18));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        GUILayout.BeginScrollView(Vector2.zero);
        
        //EditorGUI.DrawRect(new Rect(0, EditorGUIUtility.singleLineHeight * 2, firstCollumWidth, EditorGUIUtility.singleLineHeight), Color.green);
        EditorGUILayout.LabelField("", GUILayout.MinWidth(firstCollumWidth+18), GUILayout.MaxWidth(firstCollumWidth + 18), GUILayout.Height(1));

        for (int i = 0; i < 40; i++)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(firstCollumWidth));
            GUILayout.Button("quest", GUILayout.MinWidth(firstCollumWidth - 40));
            GUILayout.Button("+", GUILayout.MinWidth(18));
            GUILayout.Button("-", GUILayout.MinWidth(18));
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUILayout.Width((firstCollumWidth*2)));

        //EditorGUI.DrawRect(new Rect(firstCollumWidth, EditorGUIUtility.singleLineHeight * 2, firstCollumWidth*2, EditorGUIUtility.singleLineHeight), Color.red);

        

        e.Draw();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }
}

public class QuestInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
}


public class QuestEditor
{
    public List<QuestInfo> Infos { get; set; } = new List<QuestInfo>();

    private ReorderableList _infoList;

    private int _currentOption = 0;

    ConditionEditor _start = new ConditionEditor();
    ConditionEditor _goals = new ConditionEditor();
    ConditionEditor _fails = new ConditionEditor();

    RewardListEditor _rewardList = new RewardListEditor();

    Vector2 _scroolPosition;

    public void Start()
    {
        _infoList = new ReorderableList(Infos, typeof(QuestInfo));

        _infoList.drawHeaderCallback = OnDrawHeaderHandler;
        _infoList.drawElementCallback = OnDrawElementHandler;
        _infoList.elementHeightCallback = OnElementHeightHandler;

        _start.Start(new Condition() { Type = "Condition" }, "Start Conditions");
        _goals.Start(new Condition() { Type = "Condition" }, "Goals/Tasks");
        _fails.Start(new Condition() { Type = "Condition" }, "Fails Conditions");
        _rewardList.Start();
    }

    private float OnElementHeightHandler(int index)
    {
        return EditorGUIUtility.singleLineHeight * 2+5f;
    }

    public void Draw()
    {
        float w = (((EditorGUIUtility.currentViewWidth / 3f) * 2f) / 4f)-3.5f;

        _scroolPosition = EditorGUILayout.BeginScrollView(_scroolPosition);

        EditorGUILayout.Space();
        /*
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("Quest (id)","@name_identifier");
        GUILayout.Button("-", GUILayout.MinWidth(18), GUILayout.MaxWidth(18));
        //EditorGUILayout.Toggle(false, GUILayout.MinWidth(18), GUILayout.MaxWidth(18));// persistent
        EditorGUILayout.EndHorizontal();

        //EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("Description","@quest_identifier");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();*/

        //EditorGUILayout.LabelField("Tasks/Goals");
        _infoList.DoLayoutList();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(_currentOption == 0);
        if(GUILayout.Button("Start", GUILayout.MinWidth(w), GUILayout.MaxWidth(w)))
            _currentOption = 0;
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(_currentOption == 1);
        if(GUILayout.Button("Tasks", GUILayout.MinWidth(w), GUILayout.MaxWidth(w)))
            _currentOption = 1;
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(_currentOption == 2);
        if(GUILayout.Button("Fails", GUILayout.MinWidth(w), GUILayout.MaxWidth(w)))
            _currentOption = 2;
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(_currentOption == 3);
        if(GUILayout.Button("Rewards", GUILayout.MinWidth(w), GUILayout.MaxWidth(w)))
            _currentOption = 3;
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        if(_currentOption == 0)
            _start.Draw();
        else if (_currentOption == 1)
            _goals.Draw();
        else if (_currentOption == 2)
            _fails.Draw();
        else if (_currentOption == 3)
            _rewardList.Draw();

        EditorGUILayout.EndScrollView();
    }

    void OnDrawHeaderHandler(Rect rect)
    {
        EditorGUI.LabelField(rect, "Quest Info (id)");
    }
    void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
    {
        rect.height = EditorGUIUtility.singleLineHeight;
        Infos[index].Name = EditorGUI.TextField(rect,"Name:", Infos[index].Name);
        rect.y += rect.height;
        Infos[index].Description = EditorGUI.TextField(rect, "Description:", Infos[index].Description);
    }
}

public enum ConditionOperation
{ 
    AND,
    OR
}


public class Condition
{ 
    public string ID { get; set; }
    public string Type { get; set; } = "CheckVar";

    public ConditionOperation Operation { get; set; } = ConditionOperation.AND;

    public object[] _params;

    public List<Condition> Conditions { get; set; }

    public bool IsLeaf { get { return Conditions == null; } }
}


public class ConditionEditor
{
    string _title = string.Empty;

    Condition _condition = new Condition() { Type = "Condition" };

    ReorderableList reorderableList;
    /*List<Condition> list = new List<Condition>() 
    { 
        new Condition() { Type = "CheckVar" },
        new Condition() { Type = "CheckVar" },
        new Condition() { Type = "CheckVar" }
    };*/

    List<ConditionEditor> _conditions = new List<ConditionEditor>();

    public void Start(Condition condition, string title = "Conditions")
    {
        //save title
        _title = title;
        //save condition
        _condition = condition;

        if (_condition.IsLeaf)
        {
            //create the reorderableList
            reorderableList = new ReorderableList(_conditions, typeof(ConditionEditor))
            {
                drawHeaderCallback = OnDrawHeaderHandler,
                drawElementCallback = OnDrawElementHandler,
                elementHeightCallback = OnElementHeightHandler,
                onAddCallback = OnAddElementHandler,
                onRemoveCallback = OnRemoveElementHandler
            };
        }
    }

    private void OnRemoveElementHandler(ReorderableList list)
    {
        _conditions.RemoveAt(list.index);
    }

    private void OnAddElementHandler(ReorderableList list)
    {
        ConditionEditor c = new ConditionEditor();
        c.Start(new Condition() { Type = "CheckVar" });
        _conditions.Add(c);
    }

    private float OnElementHeightHandler(int index)
    {
        if (_conditions[index]._condition.Type == "Condition")
            return _conditions[index].reorderableList.GetHeight() + EditorGUIUtility.singleLineHeight*1.5f;//(_conditions[index]._conditions.Count+6) * EditorGUIUtility.singleLineHeight;

        return EditorGUIUtility.singleLineHeight * 2 + 10;
    }

    public void Draw()
    {
        /*EditorGUILayout.LabelField(label);*/

        //EditorGUILayout.Popup("Mode", 0, new string[] { "AND", "OR" });

        reorderableList.DoLayoutList();
                
        //reorderableList.DoLayoutList();

        /*EditorGUILayout.Space();
        EditorGUILayout.Space();
        GUILayout.Button(string.Format("Create On {0} Complete Handler", _title));
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.Space();*/


        //EditorGUILayout.Space();
    }

    void OnDrawHeaderHandler(Rect rect)
    {
        rect.width = rect.width / 2;
        EditorGUI.LabelField(rect, _title);
        rect.x += rect.width;
        rect.width = rect.width / 2;
        rect.x += rect.width;
        _condition.Operation = (ConditionOperation)EditorGUI.EnumPopup(rect, _condition.Operation);
    }
    void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
    {
        //EditorGUI.LabelField(rect, list[index]);
        //EditorGUI.LabelField(rect, list[index]);

        Rect origin = rect;
        string[] types = new string[] { "Condition", "CheckVar", "OnChangeVar" };

        rect.width = rect.width / 2f;
        rect.height = EditorGUIUtility.singleLineHeight;
        _conditions[index]._condition.Type = types[EditorGUI.Popup(rect, Array.IndexOf(types, _conditions[index]._condition.Type), types)];
        //rect.x += rect.width;
        //EditorGUI.Popup(rect, 0, new string[] { "AND", "OR" });

        //rect.x = origin.x;

        

        rect.y += EditorGUIUtility.singleLineHeight;

        if (!_conditions[index]._condition.Type.Equals("Condition"))
        {
            rect.width = 18;
            EditorGUI.Toggle(rect, false);
            rect.x += 18;
            rect.width = (origin.width - 18) / 3;
            EditorGUI.Popup(rect, 0, new string[] { "Global", "Other Group" });
            rect.x += rect.width;
            EditorGUI.Popup(rect, 0, new string[] { "String", "Int", "Float", "Boolean", "Vector3" });
            rect.x += rect.width;
            EditorGUI.TextField(rect, "value");
        }
        else
        {
            rect.width = origin.width;
            
            _conditions[index].reorderableList.DoList(rect);
            
        }
        //EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _title+" "+list[index]);
        //EditorGUI.LabelField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight), list[index]);
    }

}

public class RewardListEditor
{
    ReorderableList reorderableList;
    List<string> list = new List<string>() { "reward a", "reward b", "reward c" };

    public void Start()
    {
        reorderableList = new ReorderableList(list, typeof(string))
        {
            drawHeaderCallback = OnDrawHeaderHandler,
            drawElementCallback = OnDrawElementHandler
        };
    }

    public void Draw(string label = "Conditions")
    {
        /*EditorGUILayout.LabelField(label);*/

        reorderableList.DoLayoutList();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        //EditorGUILayout.Space();
    }

    void OnDrawHeaderHandler(Rect rect)
    {
        string name = "Rewards";
        EditorGUI.LabelField(rect, name);
    }
    void OnDrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
    {
        EditorGUI.LabelField(rect, list[index]);
    }
}

public class RewardEditor
{ 
    public void Draw()
    {
        EditorGUILayout.LabelField("Reward field");
    }
}


public class QuestList
{

}

