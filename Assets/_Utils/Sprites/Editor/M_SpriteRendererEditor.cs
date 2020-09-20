using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mup.M_Sprites
{
    [CustomEditor(typeof(M_SpriteRendererExtension))]
    //[CanEditMultipleObjects]
    public class M_SpriteRendererEditor : Editor
    {
        private static readonly string MISSING_STRING = "<< missing >>";
        private static readonly string[] EMPTY_STRING_ARRAY = new string[] { "<< Empty >>" };

        private M_SpriteRendererExtension _currentTarget;

        private SerializedProperty _spriteIndexField;
        private string[] _displayedOptions;
        
        private void OnEnable()
        {
            _currentTarget = ((M_SpriteRendererExtension)target);
            Sprite currentSprite = _currentTarget.GetComponent<SpriteRenderer>().sprite;
            _displayedOptions = new string[M_SpriteLoader.Instance.ArrayLength];

            if (_displayedOptions.Length > 0)
            {
                Sprite sprite;

                for (int i = 0; i < _displayedOptions.Length; i++)
                {
                    sprite = M_SpriteLoader.Instance.GetSpriteByIndex(i);

                    if (sprite)
                    {
                        _displayedOptions[i] = sprite.name;
                        if(currentSprite && currentSprite.name == sprite.name)
                        {
                            _currentTarget.SpriteIndex = i;
                        }
                    }
                    else
                        _displayedOptions[i] = MISSING_STRING;
                }
            }
            else
            {
                _displayedOptions = EMPTY_STRING_ARRAY;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_displayedOptions.Length > 1)
            {
                _currentTarget.SpriteIndex = EditorGUILayout.Popup("Sprite:", _currentTarget.SpriteIndex, _displayedOptions);
                serializedObject.FindProperty("_spriteIndex").intValue = _currentTarget.SpriteIndex;
            }
            else
                EditorGUILayout.Popup(0, _displayedOptions);

            serializedObject.ApplyModifiedProperties();
        }
    }
}