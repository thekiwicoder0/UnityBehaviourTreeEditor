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

            string keyName = key.FindPropertyRelative("name").stringValue;
            BlackboardKey.Type keyType = (BlackboardKey.Type)key.FindPropertyRelative("type").enumValueIndex;

            Clear();

            switch (keyType) {
                case BlackboardKey.Type.Float:
                    FloatField floatField = new FloatField();
                    floatField.BindProperty(property.FindPropertyRelative("floatValue"));
                    Add(floatField);
                    break;
                case BlackboardKey.Type.Int:
                    IntegerField intField = new IntegerField();
                    intField.BindProperty(property.FindPropertyRelative("intValue"));
                    Add(intField);
                    break;
                case BlackboardKey.Type.Boolean:
                    Toggle boolField = new Toggle();
                    boolField.BindProperty(property.FindPropertyRelative("booleanValue"));
                    Add(boolField);
                    break;
                case BlackboardKey.Type.String:
                    TextField stringField = new TextField();
                    stringField.BindProperty(property.FindPropertyRelative("stringValue"));
                    Add(stringField);
                    break;
                case BlackboardKey.Type.Vector2:
                    Vector2Field vector2Field = new Vector2Field();
                    vector2Field.BindProperty(property.FindPropertyRelative("vector2Value"));
                    Add(vector2Field);
                    break;
                case BlackboardKey.Type.Vector3:
                    Vector3Field vector3Field = new Vector3Field();
                    vector3Field.BindProperty(property.FindPropertyRelative("vector3Value"));
                    Add(vector3Field);
                    break;
                case BlackboardKey.Type.GameObject:
                    ObjectField gameObjectField = new ObjectField();
                    gameObjectField.objectType = typeof(GameObject);
                    gameObjectField.allowSceneObjects = false;
                    gameObjectField.BindProperty(property.FindPropertyRelative("gameObjectValue"));
                    Add(gameObjectField);
                    break;
                case BlackboardKey.Type.Tag:
                    TagField tagField = new TagField();
                    tagField.BindProperty(property.FindPropertyRelative("stringValue"));
                    Add(tagField);
                    break;
                case BlackboardKey.Type.LayerMask:
                    LayerMaskField layerMaskField = new LayerMaskField();
                    layerMaskField.BindProperty(property.FindPropertyRelative("layerMaskValue"));
                    Add(layerMaskField);
                    break;
                default:
                    Debug.LogError($"Unhandled type '{keyType}' for key {keyName}");
                    break;
            }
        }
    }
}