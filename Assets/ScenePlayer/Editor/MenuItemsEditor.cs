using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ScenePlayer
{
    
    [System.Serializable]
    public class MenuItemsSetting
    {
        [System.Serializable]
        public struct MenuItemData
        {
            public string name;
            public string scenePath;
        }
        
        public List<MenuItemData> menuItems = new List<MenuItemData>();
    }
    
    public class MenuItemsEditor : EditorWindow
    {
        #region Default Menu Items

        [MenuItem("Scene Player/Play 1st Scene _F5", true)]
        private static bool ValidatePlayFirstScene() => Utility.IsValidPlay() && EditorBuildSettings.scenes.Length > 0;

        [MenuItem("Scene Player/Play 1st Scene _F5", false, 0)]
        private static void PlayFirstScene() => Utility.Play(EditorBuildSettings.scenes[0].path);

        [MenuItem("Scene Player/Open Player", false, 100)]
        private static void OpenPlayer()
        {
            var editor = GetWindow(typeof(PlayerEditor), true, "Scene Player");
            editor.Show();
        }

        [MenuItem("Scene Player/Open Menu Items Editor", false, 101)]
        private static void OpenMenuItemsEditor()
        {
            var editor = GetWindow(typeof(MenuItemsEditor), true, "Scene Player Menu Items Editor");
            editor.Show();
        }

        #endregion


        private const string MENUITEMS_CODE_PATH = "Assets/ScenePlayer/Editor/Generated/MenuItems.cs";
        private const string MENUITEMS_SETTING_PATH = "Assets/ScenePlayer/Editor/Generated/MenuItemsSetting.json";

        private const string MENUITEMS_CODE_TEMPLATE = @"using UnityEditor;
namespace ScenePlayer
{
    public class ScenePlayerMenuItems
    {
%MENUITEMS%
    }
}
";
        private const string MENUITEMS_MENU_CODE_TEMPLATE = @"
        [MenuItem(""Scene Player/[%INDEX%] Play %NAME% Scene"", true)]
        private static bool ValidatePlayScene_%INDEX%() => Utility.IsValidPlay();
        [MenuItem(""Scene Player/[%INDEX%] Play %NAME% Scene"", false, %PRIORITY%)]
        private static void PlayScene_%INDEX%() => Utility.Play(""%PATH%"");";

        public MenuItemsSetting setting;
        public Vector2 scroll;

        private Dictionary<string, SceneAsset> m_CacnedSceneAssets = new Dictionary<string, SceneAsset>();
        
        private void OnGUI()
        {
            if (setting == null)
            {
                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(MENUITEMS_SETTING_PATH);
                if (textAsset != null)
                    setting = JsonUtility.FromJson<MenuItemsSetting>(textAsset.text);
                else
                    setting = new MenuItemsSetting();
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            OnGUI_MenuItems();
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save"))
            {
                var directoryPath = System.IO.Path.GetDirectoryName(MENUITEMS_SETTING_PATH);
                if (string.IsNullOrEmpty(directoryPath) == false && System.IO.Directory.Exists(directoryPath) == false)
                    System.IO.Directory.CreateDirectory(directoryPath);
                
                System.IO.File.WriteAllText(MENUITEMS_SETTING_PATH, JsonUtility.ToJson(setting, true), System.Text.Encoding.UTF8);

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
                    System.Text.Encoding.UTF8);
                
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
                setting.menuItems.Add(new MenuItemsSetting.MenuItemData());
            }
        }
    }
}
