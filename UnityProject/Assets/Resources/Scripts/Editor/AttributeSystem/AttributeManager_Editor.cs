using Rotorz.ReorderableList;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AttributeSystem
{
    [CustomEditor(typeof(AttributeManager))]
    public class AttributeManager_Editor : Editor
    {
        private int AttributeMarginX = 20;
        public GUIStyle labelMargin, textMargin, buttonMargin, miniButton, boldLabel, boldFoldOut, boldMiniButton;

        AttributeManager manager;

        SerializedProperty Attributes_FoldOut;
        SerializedProperty NameList_FoldOut;
        SerializedProperty Attributes_Debug;
        SerializedProperty Attributes;

        bool initiated = false;

        void OnEnable()
        {
            initiated = false;

            Attributes = serializedObject.FindProperty("Attributes");
            Attributes_FoldOut = serializedObject.FindProperty("Attributes_FoldOut");
            NameList_FoldOut = serializedObject.FindProperty("NameList_FoldOut");
            Attributes_Debug = serializedObject.FindProperty("Attributes_Debug");
        }

        Attribute removeAttribute = null;
        int moveFrom = 0, moveTo = 0;

        public void ListIterator(SerializedProperty listProperty, ref SerializedProperty visible)
        {
            Attributes_Debug.boolValue = EditorGUILayout.Toggle(new GUIContent("Debug Mode", "Enables editing attribute points"), Attributes_Debug.boolValue);

            EditorGUILayout.BeginHorizontal();
            visible.boolValue = EditorGUILayout.Foldout(visible.boolValue, listProperty.name, boldFoldOut);
            if (Button("+", miniButton, GUILayout.Width(20)))
            {
                Undo.RecordObject(manager, "Before Attribute Added");
                manager.AddAttribute(new Attribute() { Name = string.Format("Attribute{0}", manager.Attributes.Count) });
            }
            EditorGUILayout.EndHorizontal();

            if (visible.boolValue)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    SerializedProperty attribute = listProperty.GetArrayElementAtIndex(i);
                    DrawAttribute(attribute, i);
                }
                EditorGUI.indentLevel--;
            }

            if (removeAttribute != null)
            {
                Undo.RecordObject(manager, "Before Attribute Removed");
                manager.RemoveAttribute(removeAttribute);
                removeAttribute = null;
            }
            if(moveFrom >= 0 && moveTo >= 0 && moveFrom != moveTo)
            {
                Undo.RecordObject(manager, "Before Attribute Move");
                Attribute temp = manager.Attributes[moveTo];
                manager.Attributes[moveTo] = manager.Attributes[moveFrom];
                manager.Attributes[moveFrom] = temp;
            }
            moveFrom = -1;
            moveTo = -1;
        }

        public void DrawAttribute(SerializedProperty attribute, int index)
        {
            Attribute currentAttribute = manager.Attributes[index];
            
            bool enabledBefore = GUI.enabled;

            SerializedProperty foldOut = attribute.FindPropertyRelative("FoldOut");
            SerializedProperty point_FoldOut = attribute.FindPropertyRelative("Point_FoldOut");
            SerializedProperty value_FoldOut = attribute.FindPropertyRelative("Value_FoldOut");

            //AttributeSettings
            SerializedProperty name = attribute.FindPropertyRelative("Name");
            SerializedProperty enabled = attribute.FindPropertyRelative("Enabled");
            SerializedProperty locked = attribute.FindPropertyRelative("Locked");

            //PointSettings
            SerializedProperty points = attribute.FindPropertyRelative("Points");
            SerializedProperty startPoints = attribute.FindPropertyRelative("StartPoints");
            SerializedProperty maxPoints = attribute.FindPropertyRelative("MaxPoints");

            //ValueSettings
            SerializedProperty valueInfo = attribute.FindPropertyRelative("ValueInfo");

            SerializedProperty value = valueInfo.FindPropertyRelative("Value");
            SerializedProperty startValue = valueInfo.FindPropertyRelative("StartValue");
            SerializedProperty valuePerPoint = valueInfo.FindPropertyRelative("ValuePerPoint");
            SerializedProperty valuePerPointMultipliedByCurrentPoints = valueInfo.FindPropertyRelative("ValuePerPointMultipliedByCurrentPoints");

            float progress = (points.intValue - startPoints.intValue) / (float)maxPoints.intValue;
            if(maxPoints.intValue == 0)
                progress = 1f;

            EditorGUILayout.BeginHorizontal();
            foldOut.boolValue = EditorGUILayout.Foldout(foldOut.boolValue, new GUIContent(string.Format("Attribute_<color=#5522aa>{0:00}</color>: <color=#008800>{1}</color>", index, name.stringValue)), boldFoldOut);
            
            GUI.enabled = index != 0;
            if (Button("⤴", miniButton, GUILayout.Width(20)))
            {
                moveFrom = index;
                moveTo = index - 1;
            }
            GUI.enabled = index + 1 < Attributes.arraySize;
            if (Button("⤵", miniButton, GUILayout.Width(20)))
            {
                moveFrom = index;
                moveTo = index + 1;
            }
            GUI.enabled = true;
            if (Button("-", miniButton, GUILayout.Width(20)))
                removeAttribute = currentAttribute;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = enabled.boolValue;
            ProgressBar(enabled.boolValue ? progress : 0f, string.Format("Progress: ({0}/{1}) = {2:0.00}", points.intValue, maxPoints.intValue, value.floatValue), 20, 0, Attributes_Debug.boolValue ? 18 : 5);

            if (Attributes_Debug.boolValue)
            {
                if (GUI.enabled)
                    GUI.enabled = !locked.boolValue;
                if (maxPoints.intValue == 0)
                    GUI.enabled = false;
                if (!enabled.boolValue)
                {
                    if (Button("Disabled", miniButton, GUILayout.Width(107)))
                    {
                    }
                }
                else if (locked.boolValue)
                {
                    if (Button("Locked", miniButton, GUILayout.Width(107)))
                    {
                    }
                }
                else
                {
                    if (Button("-", miniButton, GUILayout.Width(20)))
                    {
                        Undo.RecordObject(manager, "Before Attribute RemovePoint");
                        currentAttribute.RemovePoint();
                    }
                    if (Button("+", miniButton, GUILayout.Width(20)))
                    {
                        Undo.RecordObject(manager, "Before Attribute AddPoint");
                        currentAttribute.AddPoint();
                    }
                    if (Button("0", miniButton, GUILayout.Width(20)))
                    {
                        Undo.RecordObject(manager, "Before Attribute ResetPoint");
                        currentAttribute.ResetPoints();
                    }
                    if (Button("max", miniButton, GUILayout.Width(35)))
                    {
                        Undo.RecordObject(manager, "Before Attribute MaximizePoints");
                        currentAttribute.MaximizePoints();
                    }
                }
            }
            GUI.enabled = true;


            EditorGUILayout.EndHorizontal();

            if (foldOut.boolValue)
            {
                string tempName = EditorGUILayout.TextField(new GUIContent("Name", "The Name of this Attribute"), name.stringValue);
                if (tempName != name.stringValue)
                {
                    name.stringValue = tempName;
                }

                enabled.boolValue = EditorGUILayout.Toggle(new GUIContent("Enabled", "Enables or Disables this Attribute in Game"), enabled.boolValue);
                locked.boolValue = EditorGUILayout.Toggle(new GUIContent("Locked", "A locked Attribute can't be changed in Editor or Game"), locked.boolValue);

                point_FoldOut.boolValue = EditorGUILayout.Foldout(point_FoldOut.boolValue, new GUIContent(string.Format("Points(<color=#2277ff>{0}</color>):", points.intValue)), boldFoldOut);

                if (point_FoldOut.boolValue)
                {
                    //AttributePoints
                    ProgressBar(progress, string.Format("Current Points: {0}/{1}", points.intValue, maxPoints.intValue, value.floatValue), 20, 0, 18);

                    int newStartPoints = EditorGUILayout.IntField(new GUIContent("Start Points:", ""), startPoints.intValue);
                    if (points.intValue == startPoints.intValue)
                        points.intValue = newStartPoints;
                    startPoints.intValue = newStartPoints;

                    maxPoints.intValue = EditorGUILayout.IntField(new GUIContent("Max Points:", ""), maxPoints.intValue);
                }

                if (startPoints.intValue < 0)
                    startPoints.intValue = 0;

                if (maxPoints.intValue < startPoints.intValue)
                    maxPoints.intValue = startPoints.intValue;

                if (points.intValue < startPoints.intValue)
                    points.intValue = startPoints.intValue;
                if (points.intValue > maxPoints.intValue)
                    points.intValue = maxPoints.intValue;

                //AttributeValue
                value_FoldOut.boolValue = EditorGUILayout.Foldout(value_FoldOut.boolValue, new GUIContent(string.Format("Value(<color=#aa5522>{0:0.00}</color>):", value.floatValue)), boldFoldOut);

                if (value_FoldOut.boolValue)
                {
                    EditorGUILayout.LabelField(new GUIContent(string.Format("Calculation: {0} + {1}x{2} + {3}x({4} -> {5}) = {6}", startValue.floatValue, points.intValue, valuePerPoint.floatValue, valuePerPointMultipliedByCurrentPoints.floatValue, 1, points.intValue, value.floatValue)));

                    startValue.floatValue = EditorGUILayout.FloatField(new GUIContent("Start Value:", ""), startValue.floatValue);
                    valuePerPoint.floatValue = EditorGUILayout.FloatField(new GUIContent("Value per Point:", ""), valuePerPoint.floatValue);
                    valuePerPointMultipliedByCurrentPoints.floatValue = EditorGUILayout.FloatField(new GUIContent("Value per Point (multiplied by current Points):", ""), valuePerPointMultipliedByCurrentPoints.floatValue);
                }
            }

            currentAttribute.RecalculateValue();
            GUI.enabled = enabledBefore;
        }

        public void ProgressBar(float value, string label, float moveX = 0f, float moveY = 0f, float height = 0) 
        {
            if (height == 0)
                height = 18;
            Rect rect = GUILayoutUtility.GetRect(18, height);
            rect.x += moveX;
            rect.width -= moveX;
            rect.y += moveY;
            rect.height -= moveY;

		    EditorGUI.ProgressBar (rect, value, label);
	    }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            manager = (AttributeManager)target;

            if (!initiated)
            {
                initiated = true;
                labelMargin = new GUIStyle(EditorStyles.label);
                textMargin = new GUIStyle(EditorStyles.textField);
                buttonMargin = new GUIStyle(GUI.skin.button);
                miniButton = new GUIStyle(EditorStyles.miniButton);
                boldLabel = new GUIStyle(EditorStyles.boldLabel);
                boldFoldOut = new GUIStyle(EditorStyles.foldout);
                boldMiniButton = new GUIStyle(EditorStyles.miniButton);

                labelMargin.margin = new RectOffset(AttributeMarginX, 0, 0, 0);
                textMargin.margin = new RectOffset(AttributeMarginX, 0, 0, 0);
                buttonMargin.margin = new RectOffset(AttributeMarginX, 0, 0, 0);

                boldFoldOut.fontStyle = FontStyle.Bold;
                boldFoldOut.richText = true;

                boldMiniButton.fontStyle = FontStyle.Bold;
                boldMiniButton.alignment = TextAnchor.MiddleCenter;
                boldMiniButton.padding = new RectOffset(0, 2, 0, 2);
                boldMiniButton.fontSize = 12;

                miniButton.alignment = TextAnchor.MiddleCenter;
                miniButton.padding = new RectOffset(0, 2, 0, 2);
                miniButton.fontSize = 12;
            }

            ListIterator(Attributes, ref Attributes_FoldOut);


            NameList_FoldOut.boolValue = EditorGUILayout.Foldout(NameList_FoldOut.boolValue, "NameList:", boldFoldOut);
            if(NameList_FoldOut.boolValue)
                ReorderableListGUI.ListField(manager.AttributeNames, DrawString, ReorderableListFlags.HideAddButton | ReorderableListFlags.HideRemoveButtons | ReorderableListFlags.DisableReordering);

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                manager.UpdateAttributeNamesList();
                EditorUtility.SetDirty(manager);
            }
        }

        public string DrawString(Rect rect, string value)
        {
            EditorGUI.LabelField(rect, value);
            return value;
        }

        public void Label(string label, params GUILayoutOption[] options)
        {
            Label(label, GUI.skin.label, options);
        }
        public void Label(string label, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label(label, style, options);
        }
        public void TextField(string label, ref string value, params GUILayoutOption[] options)
        {
            TextField(label, ref value, GUI.skin.textField, options);
        }
        public void TextField(string label, ref string value, GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.TextField(label, value, style, options);
        }
        public bool Button(string label, params GUILayoutOption[] options)
        {
            return Button(label, GUI.skin.button, options);
        }
        public bool MiniButton(string label, params GUILayoutOption[] options)
        {
            return Button(label, EditorStyles.miniButton, options);
        }
        public bool Button(string label, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayout.Button(label, style, options);
        }
        public void IntField(string label, ref int value, params GUILayoutOption[] options)
        {
            IntField(label, ref value, GUI.skin.textField, options);
        }
        public void IntField(string label, ref int value, GUIStyle style, params GUILayoutOption[] options)
        {
            value = EditorGUILayout.IntField(label, value, style, options);
        }
        public bool FloatField(GUIContent label, ref float value, params GUILayoutOption[] options)
        {
            return FloatField(label, ref value, GUI.skin.textField, options);
        }
        public bool FloatField(GUIContent label, ref float value, GUIStyle style, params GUILayoutOption[] options)
        {
            float temp = EditorGUILayout.FloatField(label, value, style, options);
            if (temp != value)
            {
                value = temp;
                return true;
            }
            return false;
        }
    }
}
