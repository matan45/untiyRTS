using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RTS.UI;

[InitializeOnLoad]
public class AddButtonLayoutFixer
{
    static AddButtonLayoutFixer()
    {
        EditorApplication.delayCall += AddComponent;
    }

    static void AddComponent()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid()) return;

        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
            Transform container = FindButtonContainer(root.transform);
            if (container != null)
            {
                // Check if component already exists
                if (container.GetComponent<ButtonLayoutFixer>() == null)
                {
                    container.gameObject.AddComponent<ButtonLayoutFixer>();
                    EditorUtility.SetDirty(container.gameObject);
                    EditorSceneManager.MarkSceneDirty(scene);
                    Debug.Log("[AddButtonLayoutFixer] ✅ Added ButtonLayoutFixer to ButtonContainer");
                }
                else
                {
                    Debug.Log("[AddButtonLayoutFixer] ButtonLayoutFixer already exists");
                }
                return;
            }
        }
    }

    static Transform FindButtonContainer(Transform parent)
    {
        if (parent.name == "ButtonContainer")
            return parent;

        foreach (Transform child in parent)
        {
            var result = FindButtonContainer(child);
            if (result != null) return result;
        }

        return null;
    }

    [MenuItem("RTS/Add Button Layout Fixer")]
    public static void ManualAdd()
    {
        AddComponent();
    }
}
