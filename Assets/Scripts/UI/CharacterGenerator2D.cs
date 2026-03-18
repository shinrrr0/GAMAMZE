using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterGenerator2D : MonoBehaviour
{
    [Serializable]
    public class CardSlot
    {
        public Transform cardRoot;
        public Image faceImage;
        public Image clothesImage;
        public Image headImage;
    }

    [Header("Cards (optional)")]
    [SerializeField] private CardSlot[] cards;

    [Header("Folders")]
    [SerializeField] private string facesFolderPath = "Assets/Sprites/faces";
    [SerializeField] private string clothesFolderPath = "Assets/Sprites/clothes";
    [SerializeField] private string headsFolderPath = "Assets/Sprites/head";

    [Header("Loaded sprites")]
    [SerializeField] private Sprite[] faces;
    [SerializeField] private Sprite[] clothes;
    [SerializeField] private Sprite[] heads;

    [Header("Options")]
    [SerializeField] private bool generateOnStart = false;

    public int FaceCount => faces != null ? faces.Length : 0;
    public int ClothesCount => clothes != null ? clothes.Length : 0;
    public int HeadCount => heads != null ? heads.Length : 0;
    public int CardCount => ResolveCards().Count;

    private void Start()
    {
        if (generateOnStart)
            GenerateUICharacters();
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

    [ContextMenu("Find Card Images")]
    public void FindCardImages()
    {
        if (cards == null)
            return;

        for (int i = 0; i < cards.Length; i++)
        {
            CardSlot card = cards[i];
            if (card == null || card.cardRoot == null)
                continue;

            if (card.faceImage == null)
                card.faceImage = FindLayerImage(card.cardRoot, "FaceImage", "Face");

            if (card.clothesImage == null)
                card.clothesImage = FindLayerImage(card.cardRoot, "ClothesImage", "Clothes");

            if (card.headImage == null)
                card.headImage = FindLayerImage(card.cardRoot, "HeadImage", "Head");
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Generate UI Characters")]
    public void GenerateUICharacters()
    {
        List<CardSlot> resolvedCards = ResolveCards();

        if (resolvedCards.Count == 0)
        {
            Debug.LogWarning(
                $"[{name}] Не найдено карточек. " +
                $"Либо заполни массив Cards, либо сделай карточки прямыми детьми объекта со скриптом.",
                this
            );
            return;
        }

        if (!HasEnoughSprites(resolvedCards.Count))
            return;

        List<Sprite> pickedFaces = PickUniqueRandomSprites(faces, resolvedCards.Count);
        List<Sprite> pickedClothes = PickUniqueRandomSprites(clothes, resolvedCards.Count);
        List<Sprite> pickedHeads = PickUniqueRandomSprites(heads, resolvedCards.Count);

        for (int i = 0; i < resolvedCards.Count; i++)
        {
            SetImageSprite(resolvedCards[i].faceImage, pickedFaces[i]);
            SetImageSprite(resolvedCards[i].clothesImage, pickedClothes[i]);
            SetImageSprite(resolvedCards[i].headImage, pickedHeads[i]);
        }
    }

    [ContextMenu("Clear UI Characters")]
    public void ClearGeneratedCharacters()
    {
        List<CardSlot> resolvedCards = ResolveCards();

        for (int i = 0; i < resolvedCards.Count; i++)
        {
            SetImageSprite(resolvedCards[i].faceImage, null);
            SetImageSprite(resolvedCards[i].clothesImage, null);
            SetImageSprite(resolvedCards[i].headImage, null);
        }
    }

    private List<CardSlot> ResolveCards()
    {
        List<CardSlot> result = new List<CardSlot>();

        if (cards != null && cards.Length > 0)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                CardSlot card = cards[i];
                if (card == null || card.cardRoot == null)
                    continue;

                TryResolveCardImages(card);

                if (IsValidCard(card))
                    result.Add(card);
            }

            return result;
        }

        foreach (Transform child in transform)
        {
            CardSlot autoCard = new CardSlot
            {
                cardRoot = child,
                faceImage = FindLayerImage(child, "FaceImage", "Face"),
                clothesImage = FindLayerImage(child, "ClothesImage", "Clothes"),
                headImage = FindLayerImage(child, "HeadImage", "Head")
            };

            if (IsValidCard(autoCard))
                result.Add(autoCard);
        }

        return result;
    }

    private void TryResolveCardImages(CardSlot card)
    {
        if (card.cardRoot == null)
            return;

        if (card.faceImage == null)
            card.faceImage = FindLayerImage(card.cardRoot, "FaceImage", "Face");

        if (card.clothesImage == null)
            card.clothesImage = FindLayerImage(card.cardRoot, "ClothesImage", "Clothes");

        if (card.headImage == null)
            card.headImage = FindLayerImage(card.cardRoot, "HeadImage", "Head");
    }

    private bool IsValidCard(CardSlot card)
    {
        return card != null
            && card.cardRoot != null
            && card.faceImage != null
            && card.clothesImage != null
            && card.headImage != null;
    }

    private Image FindLayerImage(Transform root, params string[] preferredNames)
    {
        Image[] images = root.GetComponentsInChildren<Image>(true);

        for (int i = 0; i < preferredNames.Length; i++)
        {
            string wanted = preferredNames[i];

            for (int j = 0; j < images.Length; j++)
            {
                if (string.Equals(images[j].name, wanted, StringComparison.OrdinalIgnoreCase))
                    return images[j];
            }
        }

        for (int i = 0; i < preferredNames.Length; i++)
        {
            string wanted = preferredNames[i].ToLowerInvariant();

            for (int j = 0; j < images.Length; j++)
            {
                string imageName = images[j].name.ToLowerInvariant();
                if (imageName.Contains(wanted))
                    return images[j];
            }
        }

        return null;
    }

    private void SetImageSprite(Image image, Sprite sprite)
    {
        if (image == null)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            Undo.RecordObject(image, "Generate UI Character");
#endif

        image.sprite = sprite;
        image.enabled = sprite != null;
        image.preserveAspect = true;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorUtility.SetDirty(image);
#endif
    }

    private bool HasEnoughSprites(int requiredCount)
    {
        if (faces == null || faces.Length < requiredCount)
        {
            Debug.LogWarning($"[{name}] Недостаточно face-спрайтов. Нужно {requiredCount}, сейчас {FaceCount}.", this);
            return false;
        }

        if (clothes == null || clothes.Length < requiredCount)
        {
            Debug.LogWarning($"[{name}] Недостаточно clothes-спрайтов. Нужно {requiredCount}, сейчас {ClothesCount}.", this);
            return false;
        }

        if (heads == null || heads.Length < requiredCount)
        {
            Debug.LogWarning($"[{name}] Недостаточно head-спрайтов. Нужно {requiredCount}, сейчас {HeadCount}.", this);
            return false;
        }

        return true;
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
    private Sprite[] LoadSpritesFromFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            Debug.LogWarning($"[{name}] Путь к папке пустой.", this);
            return Array.Empty<Sprite>();
        }

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning($"[{name}] Папка не найдена: {folderPath}", this);
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
