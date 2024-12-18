using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ScenePlayer
{
    public class PlayerEditor : EditorWindow
    {
        [System.Serializable]
        public class PlayerSetting
        {
            [System.Serializable]
            public struct CustomScene
            {
                public string scenePath;
            }

            public List<CustomScene> customScenes = new List<CustomScene>();
        }
        
        private const string PLAYER_SETTING_PATH = "UserSettings/ScenePlayerSetting.json";
        private static readonly string[] TAB_NAMES = new string[]
        {
            "Build Settings",
            "Custom",
        };

        public PlayerSetting setting;
        public Vector2 scroll;
        public bool isModifiedSetting = false;

        public int TabIndex
        {
            get => EditorPrefs.GetInt("ScenePlayer_PlayerEditor_TabIndex", 0);
            set => EditorPrefs.SetInt("ScenePlayer_PlayerEditor_TabIndex", value);
        }
        
        private Dictionary<string, SceneAsset> m_CacnedSceneAssets = new Dictionary<string, SceneAsset>();
        
        private void OnGUI()
        {
            if (setting == null)
            {
                if (System.IO.File.Exists(PLAYER_SETTING_PATH))
                    setting = JsonUtility.FromJson<PlayerSetting>(System.IO.File.ReadAllText(PLAYER_SETTING_PATH));
                else
                    setting = new PlayerSetting();
                isModifiedSetting = true;
            }
            
            var newTabIndex = GUILayout.Toolbar(TabIndex, TAB_NAMES);
            if (TabIndex != newTabIndex)
            {
                TabIndex = newTabIndex;
                scroll = Vector2.zero;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            switch (TabIndex)
            {
                case 0: OnGUI_BuildSettings(); break;
                case 1: OnGUI_Custom(); break;
            }
            EditorGUILayout.EndScrollView();

            if (isModifiedSetting == true)
            {
                isModifiedSetting = false;
                var directoryPath = System.IO.Path.GetDirectoryName(PLAYER_SETTING_PATH);
                if (string.IsNullOrEmpty(directoryPath) == false && System.IO.Directory.Exists(directoryPath) == false)
                    System.IO.Directory.CreateDirectory(directoryPath);
                
                System.IO.File.WriteAllText(PLAYER_SETTING_PATH, JsonUtility.ToJson(setting, true), System.Text.Encoding.UTF8);
            }
        }

        private void OnGUI_BuildSettings()
        {
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i ++)
            {
                var scene = EditorBuildSettings.scenes[i];
                SceneAsset sceneAsset = null;
                if (m_CacnedSceneAssets.ContainsKey(scene.path) == false)
                    m_CacnedSceneAssets[scene.path] = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                sceneAsset = m_CacnedSceneAssets[scene.path];

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(i.ToString(), GUILayout.Width(20f));
                EditorGUILayout.ObjectField(sceneAsset, typeof(SceneAsset), false);
                using (new EditorGUI.DisabledScope(Utility.IsValidPlay() == false))
                    if (GUILayout.Button("Open", GUILayout.ExpandWidth(false)))
                        Utility.Open(scene.path);
                using (new EditorGUI.DisabledScope(Utility.IsValidPlay() == false))
                    if (GUILayout.Button("Play", GUILayout.ExpandWidth(false)))
                        Utility.Play(scene.path);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void OnGUI_Custom()
        {
            for (int i = 0; i < setting.customScenes.Count; i ++)
            {
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(i <= 0))
                    if (GUILayout.Button("▲", GUILayout.ExpandWidth(false)))
                    {
                        (setting.customScenes[i - 1], setting.customScenes[i]) = (setting.customScenes[i], setting.customScenes[i - 1]);
                        isModifiedSetting = true;
                    }
                using (new EditorGUI.DisabledScope(i >= (setting.customScenes.Count - 1)))
                    if (GUILayout.Button("▼", GUILayout.ExpandWidth(false)))
                    {
                        (setting.customScenes[i + 1], setting.customScenes[i]) = (setting.customScenes[i], setting.customScenes[i + 1]);
                        isModifiedSetting = true;
                    }

                var data = setting.customScenes[i];

                SceneAsset sceneAsset = null;
                if (string.IsNullOrWhiteSpace(data.scenePath) == false)
                {
                    if (m_CacnedSceneAssets.ContainsKey(data.scenePath) == false)
                        m_CacnedSceneAssets[data.scenePath] = AssetDatabase.LoadAssetAtPath<SceneAsset>(data.scenePath);
                    sceneAsset = m_CacnedSceneAssets[data.scenePath];
                }
                
                var newSceneAsset = EditorGUILayout.ObjectField(sceneAsset, typeof(SceneAsset), false);
                if (sceneAsset != newSceneAsset)
                {
                    data.scenePath = AssetDatabase.GetAssetPath(newSceneAsset);
                    isModifiedSetting = true;
                }

                setting.customScenes[i] = data;
                using (new EditorGUI.DisabledScope(Utility.IsValidPlay() == false))
                    if (GUILayout.Button("Open", GUILayout.ExpandWidth(false)))
                        Utility.Open(data.scenePath);
                using (new EditorGUI.DisabledScope(Utility.IsValidPlay() == false))
                    if (GUILayout.Button("Play", GUILayout.ExpandWidth(false)))
                        Utility.Play(data.scenePath);
                if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                {
                    if (EditorUtility.DisplayDialog("Scene Player", $"Delete this item?\nPath: {data.scenePath}", "Ok", "Cancel") == true)
                    {
                        setting.customScenes.RemoveAt(i);
                        isModifiedSetting = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+"))
            {
                setting.customScenes.Add(new PlayerSetting.CustomScene());
                isModifiedSetting = true;
            }
        }
    }
}
