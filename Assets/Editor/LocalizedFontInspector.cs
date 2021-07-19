using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedFont))]
public class LocalizedFontInspector : Editor {
    
    private LocalizedFont targetScript;
    private SerializedObject serializedScript;
    private SerializedProperty lines;
    
    //--------------------------------------------------------------------------------------------------------
    private void OnEnable() {
        targetScript = (LocalizedFont)target;
        serializedScript = new SerializedObject(targetScript);
        lines = serializedScript.FindProperty("Lines");
    }

    //--------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI(){
        serializedScript.Update();
        targetScript.Lines ??= new List<LocalizedFontLine>();
        
        for(int i = 0; i < lines.arraySize; i++){
            SerializedProperty line = lines.GetArrayElementAtIndex(i);
            SerializedProperty language = line.FindPropertyRelative("Language");
            SerializedProperty fontAsset = line.FindPropertyRelative("FontAsset");
            SerializedProperty fontMaterial = line.FindPropertyRelative("FontMaterial");
            
            EditorGUI.indentLevel = 0;
            GUILayout.BeginHorizontal();
            language.enumValueIndex = (int)(LocalizedLanguage)EditorGUILayout.EnumPopup(
                targetScript.Lines[i].Language, GUILayout.Width(100));
            fontAsset.objectReferenceValue = EditorGUILayout.ObjectField(
                fontAsset.objectReferenceValue, typeof(TMP_FontAsset));
            fontMaterial.objectReferenceValue = EditorGUILayout.ObjectField(
                fontMaterial.objectReferenceValue, typeof(Material));
            if(GUILayout.Button("Set", GUILayout.Width(40))){
                targetScript.SetLanguage((LocalizedLanguage)language.enumValueIndex);
            }
            if(GUILayout.Button("Del", GUILayout.Width(40))){
                targetScript.RemoveLineAt(i);
            }
            GUILayout.EndHorizontal();
        }
        
        // "Add" button
        if(GUILayout.Button("Add Language")){
            targetScript.AddLanguageLine();
        }
        
        // apply changes
        serializedScript.ApplyModifiedProperties();
    }
}
