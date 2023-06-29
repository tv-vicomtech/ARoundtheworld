using OrkestraLib;

#if UNITY_EDITOR

using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UnityDataViewer : EditorWindow
{
    private static Dictionary<string, ISerializable> references = new Dictionary<string, ISerializable>();

    public static UnityDataViewer Instance;

    [MenuItem("Window/OrkestraLib")]
    public static void ShowWindow()
    {
        Instance = GetWindow(typeof(UnityDataViewer)) as UnityDataViewer;
    }

    public static void Register(string key, ISerializable obj)
    {
        if (!references.ContainsKey(key))
            references.Add(key, obj);
    }

    private static void RenderSerializable(ISerializable ser, GUIStyle style)
    {
        var table = ser.Serialize();
        foreach (var k in table.Keys)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(k, style);
            EditorGUILayout.BeginVertical();
            if (table[k] is string) EditorGUILayout.LabelField((string)table[k], style);
            else if (table[k] is ISerializable) RenderSerializable(table[k] as ISerializable, style);
            else EditorGUILayout.LabelField(JsonConvert.SerializeObject(table[k]), style);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUI.indentLevel = 0;

        GUIStyle tableStyle = new GUIStyle("box");
        tableStyle.padding = new RectOffset(10, 10, 10, 10);
        tableStyle.margin.left = 32;

        GUIStyle columnStyle = new GUIStyle();
        columnStyle.fixedWidth = 65;
        columnStyle.normal.textColor = Color.cyan;

        GUIStyle rowHeaderStyle = new GUIStyle();
        rowHeaderStyle.fixedWidth = columnStyle.fixedWidth - 1;

        GUIStyle columnLabelStyle = new GUIStyle
        {
            fixedWidth = rowHeaderStyle.fixedWidth - 6,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        columnLabelStyle.normal.textColor = Color.white;

        foreach (var key in references.Keys)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(key, columnStyle);
            EditorGUILayout.BeginVertical();
            RenderSerializable(references[key], columnLabelStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}

#endif