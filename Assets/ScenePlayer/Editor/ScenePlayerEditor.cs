using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using SceneAsset = UnityEditor.SceneAsset;

namespace ScenePlayer
{
    [System.Serializable]
    public struct ScenePlayerMenuItem
    {
        public string name;
        public string scenePath;
    }
    
    [System.Serializable]
    public class ScenePlayerSetting
    {
        public List<ScenePlayerMenuItem> menuItems = new List<ScenePlayerMenuItem>();
    }
    
    public class ScenePlayerEditor : EditorWindow
    {
        #region Default Menu Items

        [MenuItem("Scene Player/Play 1st Scene _F5", true)]
        private static bool ValidatePlayFirstScene() => IsValidPlay() && EditorBuildSettings.scenes.Length > 0;

        [MenuItem("Scene Player/Play 1st Scene _F5", false, 0)]
        private static void PlayFirstScene() => Play(EditorBuildSettings.scenes[0].path);

        [MenuItem("Scene Player/Open Editor", false, 100)]
        private static void Open()
        {
            var editor = GetWindow(typeof(ScenePlayerEditor), true, "Scene Player Editor");
            editor.Show();
        }

        #endregion


        #region Static Methods

        public static bool IsValidPlay()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
                return false;

            if (EditorApplication.isCompiling == true)
                return false;

            return true;
        }

        public static void Play(string scenePath)
        {
            if (IsValidPlay() == false)
                return;

            for (int i = 0; i < EditorSceneManager.sceneCount; i ++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
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
            EditorApplication.isPlaying = true;
        }

        #endregion


        private const string MENUITEMS_CODE_PATH = "Assets/ScenePlayer/Editor/Generated/ScenePlayerMenuItems.cs";
        private const string MENUITEMS_SETTING_PATH = "Assets/ScenePlayer/Editor/Generated/ScenePlayerSetting.json";

        private const string MENUITEMS_CODE_TEMPLATE = @"using UnityEditor;
namespace ScenePlayer
{
    public class ScenePlayerMenuItems
    {
%MENUITEMS%
    }
}";
        private const string MENUITEMS_MENU_CODE_TEMPLATE = @"
        [MenuItem(""Scene Player/[%INDEX%] Play %NAME% Scene"", true)]
        private static bool ValidatePlayScene_%INDEX%() => ScenePlayerEditor.IsValidPlay();
        [MenuItem(""Scene Player/[%INDEX%] Play %NAME% Scene"", false, %PRIORITY%)]
        private static void PlayScene_%INDEX%() => ScenePlayerEditor.Play(""%PATH%"");";

        public ScenePlayerSetting setting;
        public Vector2 scroll;
        public bool foldoutMenuItems = false;
        public bool foldoutLocalMenuItems = false;

        private Dictionary<string, SceneAsset> m_CacnedSceneAssets = new Dictionary<string, SceneAsset>();
        
        private void OnGUI()
        {
            if (setting == null)
            {
                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(MENUITEMS_SETTING_PATH);
                if (textAsset != null)
                    setting = JsonUtility.FromJson<ScenePlayerSetting>(textAsset.text);
                else
                    setting = new ScenePlayerSetting();
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foldoutMenuItems = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutMenuItems, "Menu Items");
            if (foldoutMenuItems)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10f);
                EditorGUILayout.BeginVertical();
                OnGUI_MenuItems();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save"))
            {
                var directoryPath = System.IO.Path.GetDirectoryName(MENUITEMS_SETTING_PATH);
                if (string.IsNullOrEmpty(directoryPath) == false && System.IO.Directory.Exists(directoryPath) == false)
                    System.IO.Directory.CreateDirectory(directoryPath);
                
                System.IO.File.WriteAllText(MENUITEMS_SETTING_PATH, JsonUtility.ToJson(setting, true), Encoding.UTF8);

                var strBuilder = new System.Text.StringBuilder();
                for (int i = 0; i < setting.menuItems.Count; i ++)
                {
                    strBuilder.AppendLine(MENUITEMS_MENU_CODE_TEMPLATE
                        .Replace("%INDEX%", (i + 1).ToString()) 
                        .Replace("%PRIORITY%", (i + 50).ToString())
                        .Replace("%NAME%", string.IsNullOrWhiteSpace(setting.menuItems[i].name) ? "UNKNOWN" : setting.menuItems[i].name)
                        .Replace("%PATH%", setting.menuItems[i].scenePath));
                }
                
                directoryPath = System.IO.Path.GetDirectoryName(MENUITEMS_CODE_PATH);
                if (string.IsNullOrEmpty(directoryPath) == false && System.IO.Directory.Exists(directoryPath) == false)
                    System.IO.Directory.CreateDirectory(directoryPath);
                System.IO.File.WriteAllText(MENUITEMS_CODE_PATH, 
                    MENUITEMS_CODE_TEMPLATE.Replace("%MENUITEMS%", strBuilder.ToString()),
                    Encoding.UTF8);
                
                AssetDatabase.Refresh();
            }
        }

        private void OnGUI_MenuItems()
        {
            for (int i = 0; i < setting.menuItems.Count; i ++)
            {
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(i <= 0))
                    if (GUILayout.Button("▲", GUILayout.ExpandWidth(false)))
                        (setting.menuItems[i - 1], setting.menuItems[i]) = (setting.menuItems[i], setting.menuItems[i - 1]);
                using (new EditorGUI.DisabledScope(i >= (setting.menuItems.Count - 1)))
                    if (GUILayout.Button("▼", GUILayout.ExpandWidth(false)))
                        (setting.menuItems[i + 1], setting.menuItems[i]) = (setting.menuItems[i], setting.menuItems[i + 1]);

                var data = setting.menuItems[i];
                data.name = EditorGUILayout.TextField(data.name);

                SceneAsset sceneAsset = null;
                if (string.IsNullOrWhiteSpace(data.scenePath) == false)
                {
                    if (m_CacnedSceneAssets.ContainsKey(data.scenePath) == false)
                        m_CacnedSceneAssets[data.scenePath] = AssetDatabase.LoadAssetAtPath<SceneAsset>(data.scenePath);
                    sceneAsset = m_CacnedSceneAssets[data.scenePath];
                }
                
                var newSceneAsset = EditorGUILayout.ObjectField(sceneAsset, typeof(SceneAsset), false);
                if (sceneAsset != newSceneAsset)
                    data.scenePath = AssetDatabase.GetAssetPath(newSceneAsset);

                setting.menuItems[i] = data;
                if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                {
                    setting.menuItems.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+"))
            {
                setting.menuItems.Add(new ScenePlayerMenuItem());
            }
        }
    }
}
