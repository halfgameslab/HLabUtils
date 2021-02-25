using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace HLab.H_DataSystem.H_Editor
{
    public class H_RuntimeFilesEditor<T, K> where T : H_Cloneable<T>, H_Processable<T>, H_Groupable<T, K> where K : H_Cloneable<K>, H_Processable<K>, H_Groupable<T, K>
    {
        private CVarWindowAction _currentAction;
        private string _editableAuxName;

        public void Draw(H_DataGroup<T, K> currentGroup, H_DataGroup<T, K>[] groups)
        {
            if (currentGroup == null)
                return;

            DrawPersistentFileManager(currentGroup);
            EditorGUILayout.Space();
        }

        protected void DrawPersistentFileManager(H_DataGroup<T, K> currentGroup)
        {
            //EditorGUI.BeginDisabledGroup(PlayerPrefs.GetInt("FilesCopied", 0) == 0);
            EditorGUI.BeginDisabledGroup(Application.isPlaying || !CVarSystem.IsEditModeActived);

            EditorGUILayout.BeginHorizontal();

            bool boolAux = CVarSystem.CanLoadRuntimeDefault;

            GUI.backgroundColor = Color.cyan;
            boolAux = EditorGUILayout.ToggleLeft("Show Runtime Default", CVarSystem.CanLoadRuntimeDefault);
            GUI.backgroundColor = Color.white;
            if (boolAux != CVarSystem.CanLoadRuntimeDefault)
            {
                if (boolAux && CVarSystem.ClearDefaultOnPlay)
                {
                    CVarSystem.DeleteRuntimeDefault(false);
                }

                CVarSystem.UnloadGroups(true);

                CVarSystem.ClearVars();

                CVarSystem.CanLoadRuntimeDefault = boolAux;
                CVarSystem.Init();

                //CVarSystem.Reload();
                //CVarSystem.LoadGroups(false);
            }
            EditorGUI.EndDisabledGroup();
            //EditorGUI.EndDisabledGroup();
            if (CVarSystem.CanLoadRuntimeDefault)
            {
                DrawDefaultFileManagerOptions
                    (
                        currentGroup,
                        CVarSystem.ParsePersistentDefaultDataPathWith(string.Concat(currentGroup.UID, ".xml")),
                        CVarSystem.ParseStreamingDefaultDataPathWith(string.Concat(currentGroup.UID, ".xml")),
                        CVarSystem.ParseStreamingDefaultDataPathWith("groups_data.xml")
                    );

            }
            else
            {
                DrawDefaultFileManagerOptions
                    (
                        currentGroup,
                        CVarSystem.ParseStreamingDefaultDataPathWith(string.Concat(currentGroup.UID, ".xml")),
                        CVarSystem.ParsePersistentDefaultDataPathWith(string.Concat(currentGroup.UID, ".xml")),
                        CVarSystem.ParsePersistentDefaultDataPathWith("groups_data.xml")
                    );
            }


            EditorGUILayout.EndHorizontal();
            /////////////////

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(Application.isPlaying || !CVarSystem.IsEditModeActived);
            boolAux = CVarSystem.CanLoadRuntimePersistent;
            GUI.backgroundColor = Color.red;
            boolAux = EditorGUILayout.ToggleLeft("Show Runtime Persistent", CVarSystem.CanLoadRuntimePersistent);
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();

            if (boolAux != CVarSystem.CanLoadRuntimePersistent)
            {
                CVarSystem.UnloadGroups();
                // change state
                CVarSystem.CanLoadRuntimePersistent = boolAux;
                CVarSystem.LoadGroups(false);
                GUI.FocusControl("");
            }

            if (currentGroup.PersistentType != CVarGroupPersistentType.SHARED)
            {
                if (_currentAction != CVarWindowAction.CREATE_PERSISTENT_FILE && _currentAction != CVarWindowAction.RENAME_PERSISTENT_FILE)
                {
                    // positive lookbehind ?<= ignore the \[ and get all between \]_ in a literal way
                    Regex pattern = new Regex(@"(?<=\[)(.*?)(?=\]_)");

                    string[] files = currentGroup.ListAllPersistentFileNames();

                    EditorGUI.BeginDisabledGroup(!CVarSystem.CanLoadRuntimePersistent);

                    if (files.Length > 0)
                    {
                        int id = Array.IndexOf(files, pattern.Match(currentGroup.PersistentPrefix).Value);//string.Format(g.PersistentPrefix.Replace("[", string.Empty).Replace("]", string.Empty).Replace("_", string.Empty)));
                        int currentId;

                        if (id >= 0 && id < files.Length)
                        {
                            currentId = EditorGUILayout.Popup(id, files);

                            if (id != currentId)
                            {
                                currentGroup?.Unload();
                                currentGroup.PersistentPrefix = string.Format("[{0}]_", files[currentId]);
                                currentGroup?.Load(false);
                            }
                        }
                        else
                        {

                            //g.PersistentPrefix = files[0];
                            currentGroup?.Unload();
                            currentGroup.PersistentPrefix = string.Format("[{0}]_", files[EditorGUILayout.Popup(0, files)]);
                            currentGroup?.Load(false);
                        }


                    }
                    else
                    {
                        EditorGUILayout.Popup(0, new string[] { "<none>" });
                    }

                    if (GUILayout.Button("+"))
                    {
                        //_currentAction = CVarWindowAction.CREATE_PERSISTENT_FILE;
                        //_editableAuxName = SceneManager.GetActiveScene().name;
                    }
                    EditorGUI.BeginDisabledGroup(files.Length == 0);
                    if (GUILayout.Button("-") && EditorUtility.DisplayDialog("Delete persistent", "Want delete persistent file?", "Delete", "Cancel"))
                    {
                        currentGroup.ResetToDefault();
                    }

                    if (GUILayout.Button("E"))
                    {
                        //rename
                        _currentAction = CVarWindowAction.RENAME_PERSISTENT_FILE;
                        _editableAuxName = pattern.Match(currentGroup.PersistentPrefix).Value;
                        /*g.ResetToDefault();
                        CVarSystem.GetGroup(_currentGroup).Save();
                        CVarSystem.GetGroup(_currentGroup).FlushPersistent();*/
                    }
                    if (GUILayout.Button("R") && EditorUtility.DisplayDialog("Reset persistent", "Want reset persistent file to default?", "Reset", "Cancel"))
                    {
                        currentGroup.ResetToDefault();
                        currentGroup.Save();
                        currentGroup.FlushPersistent();
                    }
                    EditorGUI.EndDisabledGroup();// disable files.Length == 0

                    //EditorGUI.EndDisabledGroup();
                }
                else
                {
                    _editableAuxName = EditorGUILayout.TextField(_editableAuxName);

                    if (GUILayout.Button("v"))
                    {
                        // check name consistence
                        if (ObjectNamesManager.ValidateIfNameHasntForbiddenCharacters(_editableAuxName))
                        {
                            if (!System.IO.File.Exists(CVarGroup.GetPersistentFilePath(currentGroup.Name, CVarGroup.ParsePrefixName(_editableAuxName))))
                            {
                                if (_currentAction == CVarWindowAction.CREATE_PERSISTENT_FILE && currentGroup.PersistentPrefix.Length > 0)
                                {
                                    // save the current file
                                    currentGroup?.Save();
                                    currentGroup?.FlushPersistent();
                                }
                                else if (_currentAction == CVarWindowAction.RENAME_PERSISTENT_FILE)
                                {
                                    // delete the current file
                                    currentGroup?.DeletePersistentFile();
                                }

                                // set the new persistent prefix
                                currentGroup?.SetGroupPersistentPrefix(_editableAuxName, currentGroup.PersistentType);
                                // save the new persistent file
                                currentGroup?.Save();
                                // force create file
                                currentGroup?.FlushPersistent();

                                _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
                                _editableAuxName = string.Empty;
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Invalid name", "There is another file with this name!", "OK");
                            }
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Invalid name", "Invalid name try another one!\nThe name can't have only spaces!", "OK");
                        }
                    }

                    if (GUILayout.Button("x"))
                    {
                        _currentAction = CVarWindowAction.EDIT_VAR_VALUES;
                        _editableAuxName = string.Empty;
                    }
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(!CVarSystem.CanLoadRuntimePersistent);
                if (!System.IO.File.Exists(CVarGroup.GetPersistentFilePath(currentGroup.UID, string.Empty)))
                {
                    if (GUILayout.Button("Create File"))
                    {
                        currentGroup?.Save();
                        currentGroup?.FlushPersistent();
                        EditorUtility.DisplayDialog("Success", "File Created", "Continue");
                    }
                }
                else
                {

                    if (GUILayout.Button("Revert") && EditorUtility.DisplayDialog("Revert persistent", "Want revert persistent file to default?", "Revert", "Cancel"))
                    {
                        currentGroup?.ResetToDefault();
                        currentGroup?.Save();
                        currentGroup?.FlushPersistent();
                    }

                    if (DefaultButton("-", "Delete Persistent File") && EditorUtility.DisplayDialog("Delete persistent", "Want delete persistent file?", "Delete", "Cancel"))
                    {
                        currentGroup?.ResetToDefault();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawDefaultFileManagerOptions(H_DataGroup<T, K> currentGroup, string loadedPath, string unloadedPath, string readGroupsFrom)
        {
            //if (CVarSystem.FilesHasBeenCopied)
            //{
            EditorGUI.BeginDisabledGroup(Application.isPlaying || !CVarSystem.IsEditModeActived || _currentAction != CVarWindowAction.EDIT_VAR_VALUES);

            if (CVarSystem.CanLoadRuntimeDefault && System.IO.File.Exists(loadedPath))
            {
                // If group there isn't the group in the default (group create at runtime) revert button won't be showed
                if (GUILayout.Button(new GUIContent("Revert", "Revert data to default")) && EditorUtility.DisplayDialog("Revert to default", "Want revert runtime default file with editor default data?", "Revert", "Cancel"))
                {
                    RevertGroupToDefault(
                        currentGroup,
                        unloadedPath,
                        loadedPath
                        );
                }
            }
            if (System.IO.File.Exists(loadedPath))
            {
                if (GUILayout.Button(new GUIContent(CVarSystem.CanLoadRuntimeDefault ? "Overwrite Editor" : "Overwrite Runtime", CVarSystem.CanLoadRuntimeDefault ? "Copy and overwrite default data on editor" : "Copy and overwrite default data on persistent folder"))
                    && EditorUtility.DisplayDialog("Overwrite default", "Want overwrite runtime default file?", "Ovewrite", "Cancel"))
                {
                    OverwriteData(
                        currentGroup,
                        loadedPath,
                        unloadedPath,
                        readGroupsFrom
                        );
                }
                if (DefaultButton("-", "Delete") && EditorUtility.DisplayDialog("Delete default", "Want delete default file?\nObs.: This process just delete the file and erase the data. If you want delete the whole group try the (-) symbol next to Group name.", "Delete", "Cancel"))
                {
                    currentGroup?.Unload();
                    M_XMLFileManager.Delete
                    (
                        loadedPath
                    );

                    M_XMLFileManager.Delete
                    (
                        string.Concat(loadedPath, ".meta")
                    );
                    currentGroup?.Load(false);
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent("Create File", "Create File")))
                {
                    currentGroup.Save();
                    currentGroup.Flush(true);
                }
            }

            EditorGUI.EndDisabledGroup();

        }

        private void RevertGroupToDefault(H_DataGroup<T, K> currentGroup, string origenPath, string destinationPath)
        {
            // read all groups from path
            H_DataGroup<T, K>[] groups = M_XMLFileManager.Load<H_DataGroup<T, K>[]>(CVarSystem.ParseStreamingDefaultDataPathWith("groups_data.xml"));
            // check if group exists
            H_DataGroup<T, K> group = groups.FirstOrDefault((g) => g.UID == currentGroup.UID);
            if (group != null)
            {
                //change group data
                currentGroup.Name = group.Name;
                currentGroup.PersistentType = group.PersistentType;

                currentGroup?.Unload();
                M_XMLFileManager.Copy
                (
                    origenPath,
                    destinationPath,
                    true
                );
                currentGroup?.Load(false);

                // save new group list to runtime
                ///////////////////CVarSystem.SaveGroupListToFile();
            }
            else
            {
                // add
                //groups.Add(currentGroup);
                Debug.LogWarning(string.Format("Can't revert data to default! There isn't Group with Name ({0}) and UID ({1}) on group list!", currentGroup.Name, currentGroup.UID));
            }
        }

        private void OverwriteData(H_DataGroup<T, K> currentGroup, string origenPath, string destinationPath, string readGroupsFrom)
        {
            // read all groups from path
            List<H_DataGroup<T, K>> groups = new List<H_DataGroup<T, K>>();
            groups.AddRange(M_XMLFileManager.Load<H_DataGroup<T, K>[]>(readGroupsFrom));

            // check if group exists
            H_DataGroup<T, K> group = groups.FirstOrDefault((g) => g.UID == currentGroup.UID);
            if (group != null)
            {
                //change group data
                group.Name = currentGroup.Name;
                group.PersistentType = currentGroup.PersistentType;
            }
            else
            {
                // add
                groups.Add(currentGroup);
                //Debug.LogWarning(string.Format("Can't revert data to default! There isn't Group with Name ({0}) and UID ({1}) on group list!", currentGroup.Name, currentGroup.UID));
            }

            M_XMLFileManager.Copy
                (
                    origenPath,
                    destinationPath,
                    true
                );
            M_XMLFileManager.Save(readGroupsFrom, groups.ToArray());
        }

        private bool DefaultButton(string label, string tooltip)
        {
            return GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Width(19));
        }
    }
}
