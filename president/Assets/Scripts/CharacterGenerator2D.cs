using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterGenerator2D : MonoBehaviour
{
    [Header("Folders")]
    [SerializeField] private string facesFolderPath = "Assets/Sprites/faces";
    [SerializeField] private string clothesFolderPath = "Assets/Sprites/clothes";
    [SerializeField] private string headsFolderPath = "Assets/Sprites/head";

    [Header("Loaded sprites")]
    [SerializeField] private Sprite[] faces;
    [SerializeField] private Sprite[] clothes;
    [SerializeField] private Sprite[] heads;

    [Header("Generation")]
    [SerializeField] private int charactersToGenerate = 4;
    [SerializeField] private bool generateOnStart = false;
    [SerializeField] private float horizontalSpacing = 3.5f;
    [SerializeField] private bool clearOldCharactersBeforeGenerate = true;

    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int faceOrder = 0;
    [SerializeField] private int clothesOrder = 1;
    [SerializeField] private int headOrder = 2;

    public int FaceCount => faces != null ? faces.Length : 0;
    public int ClothesCount => clothes != null ? clothes.Length : 0;
    public int HeadCount => heads != null ? heads.Length : 0;

    private void Start()
    {
        if (generateOnStart)
            GenerateUniqueCharacters();
    }

    [ContextMenu("Generate 4 Unique Characters")]
    public void GenerateUniqueCharacters()
    {
        if (!HasEnoughSprites())
            return;

        if (clearOldCharactersBeforeGenerate)
            ClearGeneratedCharacters();

        List<Sprite> pickedFaces = PickUniqueRandomSprites(faces, charactersToGenerate);
        List<Sprite> pickedClothes = PickUniqueRandomSprites(clothes, charactersToGenerate);
        List<Sprite> pickedHeads = PickUniqueRandomSprites(heads, charactersToGenerate);

        for (int i = 0; i < charactersToGenerate; i++)
        {
            Transform characterRoot = GetOrCreateCharacterRoot(i);
            characterRoot.localPosition = GetCharacterLocalPosition(i);
            characterRoot.localRotation = Quaternion.identity;
            characterRoot.localScale = Vector3.one;

            SpriteRenderer faceRenderer = GetOrCreateLayerRenderer(characterRoot, "Face");
            SpriteRenderer clothesRenderer = GetOrCreateLayerRenderer(characterRoot, "Clothes");
            SpriteRenderer headRenderer = GetOrCreateLayerRenderer(characterRoot, "Head");

            faceRenderer.sprite = pickedFaces[i];
            clothesRenderer.sprite = pickedClothes[i];
            headRenderer.sprite = pickedHeads[i];

            ApplySorting(faceRenderer, clothesRenderer, headRenderer);
        }
    }

    public void GenerateUniqueCharacters(int count)
    {
        charactersToGenerate = Mathf.Max(1, count);
        GenerateUniqueCharacters();
    }

    [ContextMenu("Reload Sprites From Folders")]
    public void ReloadFromFolders()
    {
#if UNITY_EDITOR
        faces = LoadSpritesFromFolder(facesFolderPath);
        clothes = LoadSpritesFromFolder(clothesFolderPath);
        heads = LoadSpritesFromFolder(headsFolderPath);

        EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Clear Generated Characters")]
    public void ClearGeneratedCharacters()
    {
        List<GameObject> toDelete = new List<GameObject>();

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Character_", StringComparison.Ordinal))
                toDelete.Add(child.gameObject);
        }

        for (int i = 0; i < toDelete.Count; i++)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(toDelete[i]);
            else
                Destroy(toDelete[i]);
#else
            Destroy(toDelete[i]);
#endif
        }
    }

    private bool HasEnoughSprites()
    {
        if (faces == null || faces.Length < charactersToGenerate)
        {
            Debug.LogWarning(
                $"[{name}] Недостаточно face-спрайтов. Нужно минимум {charactersToGenerate}, сейчас {FaceCount}.",
                this
            );
            return false;
        }

        if (clothes == null || clothes.Length < charactersToGenerate)
        {
            Debug.LogWarning(
                $"[{name}] Недостаточно clothes-спрайтов. Нужно минимум {charactersToGenerate}, сейчас {ClothesCount}.",
                this
            );
            return false;
        }

        if (heads == null || heads.Length < charactersToGenerate)
        {
            Debug.LogWarning(
                $"[{name}] Недостаточно head-спрайтов. Нужно минимум {charactersToGenerate}, сейчас {HeadCount}.",
                this
            );
            return false;
        }

        return true;
    }

    private Transform GetOrCreateCharacterRoot(int index)
    {
        string childName = $"Character_{index + 1}";
        Transform child = transform.Find(childName);

        if (child == null)
        {
            GameObject go = new GameObject(childName);
            go.transform.SetParent(transform);
            child = go.transform;
        }

        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;

        return child;
    }

    private SpriteRenderer GetOrCreateLayerRenderer(Transform parent, string layerName)
    {
        Transform child = parent.Find(layerName);

        if (child == null)
        {
            GameObject go = new GameObject(layerName);
            go.transform.SetParent(parent);
            child = go.transform;
        }

        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;

        SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = child.gameObject.AddComponent<SpriteRenderer>();

        return renderer;
    }

    private Vector3 GetCharacterLocalPosition(int index)
    {
        float offset = (charactersToGenerate - 1) * 0.5f;
        float x = (index - offset) * horizontalSpacing;
        return new Vector3(x, 0f, 0f);
    }

    private void ApplySorting(
        SpriteRenderer faceRenderer,
        SpriteRenderer clothesRenderer,
        SpriteRenderer headRenderer)
    {
        if (faceRenderer != null)
        {
            faceRenderer.sortingLayerName = sortingLayerName;
            faceRenderer.sortingOrder = faceOrder;
        }

        if (clothesRenderer != null)
        {
            clothesRenderer.sortingLayerName = sortingLayerName;
            clothesRenderer.sortingOrder = clothesOrder;
        }

        if (headRenderer != null)
        {
            headRenderer.sortingLayerName = sortingLayerName;
            headRenderer.sortingOrder = headOrder;
        }
    }

    private List<Sprite> PickUniqueRandomSprites(Sprite[] source, int count)
    {
        List<Sprite> pool = new List<Sprite>(source);
        Shuffle(pool);
        return pool.Take(count).ToList();
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        charactersToGenerate = Mathf.Max(1, charactersToGenerate);
        horizontalSpacing = Mathf.Max(0f, horizontalSpacing);
    }

    private Sprite[] LoadSpritesFromFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            Debug.LogWarning($"[{name}] Путь к папке пустой.");
            return Array.Empty<Sprite>();
        }

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning($"[{name}] Папка не найдена: {folderPath}");
            return Array.Empty<Sprite>();
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        List<Sprite> result = new List<Sprite>();
        HashSet<string> unique = new HashSet<string>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            Sprite mainSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (mainSprite != null)
            {
                string key = assetPath + "|" + mainSprite.name;
                if (unique.Add(key))
                    result.Add(mainSprite);
            }

            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            foreach (UnityEngine.Object subAsset in subAssets)
            {
                if (subAsset is Sprite sprite)
                {
                    string key = assetPath + "|" + sprite.name;
                    if (unique.Add(key))
                        result.Add(sprite);
                }
            }
        }

        return result
            .OrderBy(s => GetNamePrefix(s.name))
            .ThenBy(s => GetTrailingNumber(s.name))
            .ThenBy(s => s.name)
            .ToArray();
    }

    private string GetNamePrefix(string value)
    {
        Match match = Regex.Match(value, @"^(.*?)(\d+)?$");
        return match.Success ? match.Groups[1].Value : value;
    }

    private int GetTrailingNumber(string value)
    {
        Match match = Regex.Match(value, @"(\d+)$");
        if (match.Success && int.TryParse(match.Value, out int number))
            return number;

        return int.MaxValue;
    }
#endif
}