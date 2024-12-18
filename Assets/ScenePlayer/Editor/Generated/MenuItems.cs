using UnityEditor;
namespace ScenePlayer
{
    public class ScenePlayerMenuItems
    {

        [MenuItem("Scene Player/[1] Play Sample Scene 1 Scene", true)]
        private static bool ValidatePlayScene_1() => Utility.IsValidPlay();
        [MenuItem("Scene Player/[1] Play Sample Scene 1 Scene", false, 50)]
        private static void PlayScene_1() => Utility.Play("Assets/Scenes/SampleScene 1.unity");

        [MenuItem("Scene Player/[2] Play 샘플씬2 Scene", true)]
        private static bool ValidatePlayScene_2() => Utility.IsValidPlay();
        [MenuItem("Scene Player/[2] Play 샘플씬2 Scene", false, 51)]
        private static void PlayScene_2() => Utility.Play("Assets/Scenes/SampleScene 2.unity");

        [MenuItem("Scene Player/[3] Play SC3 Scene", true)]
        private static bool ValidatePlayScene_3() => Utility.IsValidPlay();
        [MenuItem("Scene Player/[3] Play SC3 Scene", false, 52)]
        private static void PlayScene_3() => Utility.Play("Assets/Scenes/SampleScene 3.unity");

    }
}
