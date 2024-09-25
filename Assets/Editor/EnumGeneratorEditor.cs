using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[CustomEditor(typeof(EnumGenerator))]
public class EnumGeneratorEditor : Editor {
    private const string EnumName = "DynamicEnum";
    private const string EnumPath = "Assets/Scripts/EnumGenerator";

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        EnumGenerator enumGenerator = (EnumGenerator)target;

        if (GUILayout.Button("Generate Enum")) {
            GenerateEnum(enumGenerator.items);
            AssetDatabase.Refresh();
        }
    }

    private void GenerateEnum(EnumData items) {
        if (!Directory.Exists(EnumPath)) {
            Directory.CreateDirectory(EnumPath);
        }

        string filePath = Path.Combine(EnumPath, items.enumName + ".cs");
        using (StreamWriter streamWriter = new StreamWriter(filePath)) {
            streamWriter.WriteLine("public enum " + items.enumName);
            streamWriter.WriteLine("{");

            for (int i = 0; i < items.itemName.Count; i++) {
                streamWriter.WriteLine("    " + items.itemName[i].Replace(" ", "_") + (i < items.itemName.Count - 1 ? "," : ""));
            }

            streamWriter.WriteLine("}");
        }

        CompilationPipeline.RequestScriptCompilation();
    }
}
