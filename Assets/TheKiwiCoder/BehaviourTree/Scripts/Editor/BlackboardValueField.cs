using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


namespace TheKiwiCoder {
    public class BlackboardValueField : VisualElement {

        SerializedProperty property;

        public void BindProperty(SerializedProperty key) {

            property = key;

            BlackboardItem.Type keyType = (BlackboardItem.Type)key.FindPropertyRelative("type").enumValueIndex;

            Clear();

            switch (keyType) {
                case BlackboardItem.Type.Float:
                    FloatField floatField = new FloatField();
                    floatField.BindProperty(property.FindPropertyRelative("floatValue"));
                    Add(floatField);
                    break;
                case BlackboardItem.Type.Int:
                    IntegerField intField = new IntegerField();
                    intField.BindProperty(property.FindPropertyRelative("intValue"));
                    Add(intField);
                    break;
                case BlackboardItem.Type.Boolean:
                    Toggle toggle = new Toggle();
                    toggle.BindProperty(property.FindPropertyRelative("booleanValue"));
                    Add(toggle);
                    break;
                case BlackboardItem.Type.Vector3:
                    Vector3Field vector3Field = new Vector3Field();
                    vector3Field.BindProperty(property.FindPropertyRelative("vector3Value"));
                    Add(vector3Field);
                    break;
                case BlackboardItem.Type.Object:
                    ObjectField gameObjectField = new ObjectField();
                    gameObjectField.objectType = typeof(GameObject);
                    gameObjectField.allowSceneObjects = false;
                    gameObjectField.BindProperty(property.FindPropertyRelative("gameObjectValue"));
                    Add(gameObjectField);
                    break;
            }
        }
    }
}
