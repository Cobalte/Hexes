using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedText))]
public class LocalizedTextInspector : Editor {
    
    private LocalizedText targetScript;
    private SerializedObject serializedScript;
    private SerializedProperty lines;
    
    //--------------------------------------------------------------------------------------------------------
    private void OnEnable() {
        targetScript = (LocalizedText)target;
        serializedScript = new SerializedObject(targetScript);
        lines = serializedScript.FindProperty("Lines");
    }

    //--------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI(){
        serializedScript.Update();
        targetScript.Lines ??= new List<LocalizedTextLine>();
        
        for(int i = 0; i < lines.arraySize; i++){
            SerializedProperty line = lines.GetArrayElementAtIndex(i);
            SerializedProperty language = line.FindPropertyRelative("Language");
            SerializedProperty text = line.FindPropertyRelative("Text");
            SerializedProperty fontAsset = line.FindPropertyRelative("FontAsset");
            SerializedProperty fontMaterial = line.FindPropertyRelative("FontMaterial");
            
            // row 1: language, font, material, and 'remove' button 
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
            
            // row 2: display string (in target language)
            EditorGUI.indentLevel = 1;
            text.stringValue = EditorGUILayout.TextField(text.stringValue);
        }
        
        // "Add" button
        if(GUILayout.Button("Add Language")){
            targetScript.AddLanguageLine();
        }
        
        // apply changes
        serializedScript.ApplyModifiedProperties();
    }
}
