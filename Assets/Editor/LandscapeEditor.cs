using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Landscape))]
public class LandscapeEditor : Editor
{
    Landscape landscape;

    public override void OnInspectorGUI()
    {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                if (check.changed)
                {
                    landscape.GenerateLandscape();
                }
            }

            if (GUILayout.Button("Generate Landscape"))
            {
                landscape.GenerateLandscape();
            }
        
    }


    private void OnEnable()
    {
        landscape = (Landscape)target;
    }
}
