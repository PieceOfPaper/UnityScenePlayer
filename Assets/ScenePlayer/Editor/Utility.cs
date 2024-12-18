using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ScenePlayer
{
    public static class Utility
    {
        public static bool IsValidPlay()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
                return false;

            if (EditorApplication.isCompiling == true)
                return false;

            return true;
        }


        public static void Open(string scenePath)
        {
            for (int i = 0; i < SceneManager.sceneCount; i ++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded == false) continue;

                if (scene.isDirty == true)
                {
                    var option = EditorUtility.DisplayDialogComplex("Scene Player",
                        $"{scene.name} Scene has been modified.\nDo you want to save the changes you made before playing?",
                        "Save",
                        "Cancel",
                        "Don't Save");

                    if (option == 0)
                    {
                        //Save
                        EditorSceneManager.SaveScene(scene);
                    }
                    else if (option == 1)
                    {
                        //Cancel
                        return;
                    }
                    else if (option == 2)
                    {
                        //Don't Save
                    }
                }
            }

            EditorSceneManager.OpenScene(scenePath);
        }

        public static void Play(string scenePath)
        {
            if (IsValidPlay() == false)
                return;
            
            Open(scenePath);
            EditorApplication.isPlaying = true;
        }
    }
}
