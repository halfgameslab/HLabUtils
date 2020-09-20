using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

/// <summary>
/// SoundPlayerEditor developed by MUP studios
/// Know issues: multiple add dont start objects
/// Know issues: cant multiple edit
/// </summary>

namespace Mup.SoundSystem
{
    [CustomEditor(typeof(M_SoundPlayer))]
    public class M_SoundPlayerEditor : Editor
    {

        private static readonly string MISSING_STRING = "<< missing >>";
        private static readonly string[] EMPTY_STRING_ARRAY = new string[] { "<< Empty >>" };

        private int _choiceIndex = 0;
        //SoundPlayer soundPlayerInstance;

        //SerializedProperty _instanceAudioClipName;
        SerializedProperty _instanceAudioClipIndex;
        SerializedProperty _instancePlayOnEnable;

        M_SoundManager _soundManager;

        private void OnEnable()
        {
            //soundPlayerInstance = target as SoundPlayer;

            //propriedade selecionada [SoundManager._audioClipName]
            //_instanceAudioClipName = serializedObject.FindProperty("_audioClipName");

            //propriedade selecionada [SoundManager._playOnEnable]
            _instancePlayOnEnable = serializedObject.FindProperty("_playOnEnable");

            //propriedade selecionada [SoundManager._audioClipIndex]
            _instanceAudioClipIndex = serializedObject.FindProperty("_audioClipIndex");

            _soundManager = M_SoundManager.Instance;
            /*MethodInfo[] soundManagerMethod = typeof(SoundManager).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            //verifica se existem métodos e se algum método já foi cadastrado
            if (soundManagerMethod.Length > 0 && _instanceMethod.stringValue == string.Empty)
            {
                //cadastra um método se o objeto ainda não foi inicializado
                _instanceMethod.stringValue = soundManagerMethod[0].Name;
            }*/
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //lista todos os métodos públicos da classe
            //MethodInfo[] soundManagerMethod = typeof(SoundManager).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            string[] _listedAudioClipNames = new string[_soundManager.AudioClips.Length];//new string[soundManagerMethod.Length];
            string[] _choices;
            //SoundPlayer soundPlayerInstance = target as SoundPlayer;

            _choiceIndex = -1;
            //se a classe tem métodos
            if (_listedAudioClipNames.Length != 0)
            {
                //verifica se algum método já foi cadastrado
                if (_instanceAudioClipIndex.intValue == -1)
                {
                    //cadastra um método
                    //_instanceAudioClipName.stringValue = _soundManager.AudioClips[0].name;//_listedMethodsNames[0].name;
                    _instanceAudioClipIndex.intValue = 0;
                }

                //para cada método
                for (int i = 0; i < _listedAudioClipNames.Length; i++)
                {
                    if (_soundManager.AudioClips[i])
                    {
                        //verifica se nome do método atual é igual ao nome do método anterior
                        //if (_instanceAudioClipName.stringValue == _soundManager.AudioClips[i].name)
                        if (_instanceAudioClipIndex.intValue == i)
                        {
                            //marca o indice da escolha
                            _choiceIndex = i;
                        }

                        //adiciona o método atual a lista de métodos
                        _listedAudioClipNames[i] = _soundManager.AudioClips[i].name;
                    }
                }

                //se o método selecionado anteriormente está na lista de métodos
                if (_choiceIndex != -1)
                {
                    //copia a lista de métodos para o vetor de escolhas
                    _choices = new string[_listedAudioClipNames.Length];
                    _listedAudioClipNames.CopyTo(_choices, 0);
                }
                else
                {
                    //senão adiciona a palavra missing para o vetor
                    _choices = new string[_listedAudioClipNames.Length + 1];
                    _listedAudioClipNames.CopyTo(_choices, 0);
                    _choices[_choices.Length - 1] = MISSING_STRING;
                    _choiceIndex = _choices.Length - 1;
                }
            }
            else //senão
            {
                //escreve vazio
                _choices = EMPTY_STRING_ARRAY;
            }

            // Draw the default inspector
            //DrawDefaultInspector();
            EditorGUILayout.PropertyField(_instancePlayOnEnable);
            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Audio Clips to play in SoundManager:");
            _choiceIndex = EditorGUILayout.Popup(_choiceIndex, _choices);
            //EditorGUILayout.EndHorizontal();

            //se o vetor não está vazio ou o método não foi perdido
            if (_choiceIndex != -1 && !_choices[_choiceIndex].Contains("<<"))
            {
                // Update the selected choice in the underlying object
                //soundPlayerInstance.Method = _choices[_choiceIndex];
                _instanceAudioClipIndex.intValue = _choiceIndex;
                //_instanceAudioClipName.stringValue = _choices[_choiceIndex];
            }

            //atualiza as propriedades
            serializedObject.ApplyModifiedProperties();
            // Save the changes back to the object
            //segundo os foruns isso não é necessário mas ficou aqui só para desencargo
            EditorUtility.SetDirty(target);
        }
    }
}