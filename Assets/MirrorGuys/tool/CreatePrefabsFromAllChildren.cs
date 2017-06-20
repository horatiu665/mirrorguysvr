
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CreatePrefabsFromAllChildren : MonoBehaviour
{
    public string path;

#if UNITY_EDITOR
    [Kaae.DebugButton]
    void CreatePrefabsFromChildren()
    {
        var children = transform.GetChildren();
        foreach (Transform t in children) {
            Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/" + path + "/" + t.gameObject.name + ".prefab");
            PrefabUtility.ReplacePrefab(t.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
        }
    }

    // http://answers.unity3d.com/questions/172601/how-do-i-apply-prefab-in-script-.html
    [Kaae.DebugButton]
    void ApplyAllChildrenPrefabs()
    {
        var children = transform.GetChildren();
        foreach (Transform t in children) {
            var instanceRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(t.gameObject);
            var targetPrefab = UnityEditor.PrefabUtility.GetPrefabParent(instanceRoot);
            PrefabUtility.ReplacePrefab(
                             instanceRoot,
                             targetPrefab,
                             ReplacePrefabOptions.ConnectToPrefab
                             );
        }
    }
#endif

}
