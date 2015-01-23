using UnityEditor;
using UnityEngine;

namespace LevelEngine
{

    [CustomEditor(typeof(LevelManager))]
    public class LevelManager_Editor : Editor
    {
        LevelManager manager;

        SerializedProperty Parts;

        bool initiated = false;

        void OnEnable()
        {
            manager = (LevelManager)target;

            initiated = false;
            Parts = serializedObject.FindProperty("Parts");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if(!initiated)
            {
                initiated = true;

            }

            DrawInspector();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(manager);
            }
        }

        public void DrawInspector()
        {
            if(GUILayout.Button("Add LevelPart"))
            {
                GameObject go = new GameObject(string.Format("LevelPart_"));
                AddPart(go.AddComponent<LevelPart>());
            }
        }

        public void AddPart(LevelPart part)
        {
            Undo.RecordObject(manager, "Before Add Part");
        }
        public void RemovePart(LevelPart part)
        {
            Undo.RecordObject(manager, "Before Remove Part");
        }
    }
}