#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class PrefabEditorSpawner : MonoBehaviour
{
    [MenuItem("Tools/Instantiate Bunker")]
    public static void InstantiateMyPrefab()
    {
        var prefabPath = "Assets/Art/Prefabs/Pixel.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("Prefab not found at path: " + prefabPath);
            return;
        }

        var root = Instantiate(new GameObject("Bunker"), Vector3.zero, Quaternion.identity);

        var width = 22;
        // var height = 1;
        var height = 16;
        var pixelSpacing = 0.031f;

        var pixels = new GameObject[width, height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixel = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                pixel.transform.localPosition = new(x * pixelSpacing, y * pixelSpacing, 0);
                Undo.RegisterCreatedObjectUndo(pixel, "Instantiate Bunker Pixel");
                pixels[x, y] = pixel;
            }
        }

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixelComp = pixels[x, y].GetComponent<Pixel>();
                pixelComp.left = x > 0 ? pixels[x - 1, y].GetComponent<Pixel>() : null;
                pixelComp.right = x < width - 1 ? pixels[x + 1, y].GetComponent<Pixel>() : null;
                pixelComp.top = y < height - 1 ? pixels[x, y + 1].GetComponent<Pixel>() : null;
                pixelComp.down = y > 0 ? pixels[x, y - 1].GetComponent<Pixel>() : null;
            }
        }

        Selection.activeGameObject = root;
        DestroyImmediate(GameObject.Find("Bunker"));
        root.name = "Bunker";
    }
}
#endif
