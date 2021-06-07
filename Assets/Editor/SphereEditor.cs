using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TriangulatedSphere))]
public class SphereEditor : Editor
{
    TriangulatedSphere sphere;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                sphere.Generate();
            }
        }

        if (GUILayout.Button("Generate Landscape"))
        {
            sphere.Generate();
        }
    }


    private void OnEnable()
    {
        sphere = (TriangulatedSphere)target;
    }
}
