using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

public class RemovePolybrush : OdinEditorWindow
{
    [SerializeField]
    private List<GameObject> objects;


    [MenuItem("Window/RemovePolybrush")]
    static void Init()
    {
        RemovePolybrush window = CreateInstance<RemovePolybrush>();
        window.Show();
    }

    //UnityEngine.Polybrush.PolybrushMesh
    [Sirenix.OdinInspector.Button]
    private void Run()
    {
        foreach (var obj in objects)
        {
            var components = obj.GetComponents<Component>();
            if (components.Length < 4)
                continue;
            
            if (components[3].GetType().ToString().Contains("Polybrush"))
            {
                var type = components[3].GetType();
                var sourceMeshField = type.GetField("m_OriginalMeshObject", BindingFlags.NonPublic | BindingFlags.Instance);
                var method = type.GetMethod("SetMesh", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(components[3], new object[] { sourceMeshField.GetValue(components[3]) });

                DestroyImmediate(components[3]);
            }

            //var type = TestHierarchySelect.GetType2("UnityEngine.Polybrush.PolybrushMesh");
            //var instance = obj.GetComponent(type);
            //if (instance)
            //{
            //    DestroyImmediate(instance);
            //}
        }
    }

}