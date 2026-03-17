using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterGeneratorUI : MonoBehaviour
{
    [Header("Cards in UI")]
    [SerializeField] private CharacterCardUI[] cards;

    [Header("Folders")]
    [SerializeField] private string facesFolderPath = "Assets/Sprites/faces";
    [SerializeField] private string clothesFolderPath = "Assets/Sprites/clothes";
    [SerializeField] private string headsFolderPath = "Assets/Sprites/head";

    [Header("Loaded sprites")]
    [SerializeField] private Sprite[] faces;
    [SerializeField] private Sprite[] clothes;
    [SerializeField] private Sprite[] heads;

    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool reloadSpritesBeforeGenerate = false;
    [SerializeField] private int skillsPerCharacter = 4;
    [SerializeField] private int actionsPerDropdown = 3;

    [Header("Content pools")]
    [SerializeField] private string[] possibleNames =
    {
        "Mara", "Iris", "Noah", "Ava",
        "Leon", "Mika", "Rin", "Troy"
    };

    [SerializeField] private string[] possibleSkills =
    {
        "Negotiation", "Medicine", "Stealth", "Analysis",
        "Leadership", "Engineering", "Empathy", "Tactics",
        "Deception", "Observation"
    };

    [SerializeField] private ActionOption[] possiblePlayerActions =
    {
        new ActionOption { title = "Talk", description = "Try to negotiate and reduce tension." },
        new ActionOption { title = "Attack", description = "Use direct force and accept the risk." },
        new ActionOption { title = "Hide", description = "Avoid contact and wait for a better moment." },
        new ActionOption { title = "Trade", description = "Offer resources to gain advantage." },
        new ActionOption { title = "Scout", description = "Gather more information before acting." }
    };

    [SerializeField] private ActionOption[] possibleAiActions =
    {
        new ActionOption { title = "Observe", description = "The AI studies your behavior before moving." },
        new ActionOption { title = "Pressure", description = "The AI increases stress and forces a reaction." },
        new ActionOption { title = "Retreat", description = "The AI pulls back and changes position." },
        new ActionOption { title = "Ambush", description = "The AI prepares a sudden attack." },
        new ActionOption { title = "Manipulate", description = "The AI tries to bait the player into a mistake." }
    };

    private void Start()
    {
        if (generateOnStart)
            GenerateCharactersForUI();
    }

    [ContextMenu("Generate Characters For UI")]
    public void GenerateCharactersForUI()
    {
#if UNITY_EDITOR
        if (reloadSpritesBeforeGenerate)
            ReloadFromFolders();
#endif

        int count = cards != null ? cards.Length : 0;
        if (count == 0)
        {
            Debug.LogWarning("Cards array is empty.", this);
            return;
        }

        if (!HasEnoughSprites(count))
            return;

        List<Sprite> pickedFaces = PickUniqueRandomSprites(faces, count);
        List<Sprite> pickedClothes = PickUniqueRandomSprites(clothes, count);
        List<Sprite> pickedHeads = PickUniqueRandomSprites(heads, count);

        List<string> pickedNames = PickRandomStrings(possibleNames, count, allowRepeatsIfNeeded: true);

        for (int i = 0; i < count; i++)
        {
            if (cards[i] == null)
                continue;

            CharacterData data = new CharacterData
            {
                face = pickedFaces[i],
                clothes = pickedClothes[i],
                head = pickedHeads[i],
                characterName = pickedNames[i],
                skills = PickRandomStrings(possibleSkills, skillsPerCharacter, allowRepeatsIfNeeded: false).ToArray(),
                playerActions = PickRandomActions(possiblePlayerActions, actionsPerDropdown),
                aiActions = PickRandomActions(possibleAiActions, actionsPerDropdown),
                hp = UnityEngine.Random.Range(1, 4),
                insanity = UnityEngine.Random.Range(1, 4),
                age = UnityEngine.Random.Range(1, 4)
            };

            cards[i].Apply(data);
        }
    }

    private bool HasEnoughSprites(int requiredCount)
    {
        if (faces == null || faces.Length < requiredCount)
        {
            Debug.LogWarning($"Not enough face sprites. Need {requiredCount}, have {faces?.Length ?? 0}.", this);
            return false;
        }

        if (clothes == null || clothes.Length < requiredCount)
        {
            Debug.LogWarning($"Not enough clothes sprites. Need {requiredCount}, have {clothes?.Length ?? 0}.", this);
            return false;
        }

        if (heads == null || heads.Length < requiredCount)
        {
            Debug.LogWarning($"Not enough head sprites. Need {requiredCount}, have {heads?.Length ?? 0}.", this);
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

    private List<string> PickRandomStrings(string[] source, int count, bool allowRepeatsIfNeeded)
    {
        List<string> result = new List<string>();

        if (source == null || source.Length == 0)
        {
            for (int i = 0; i < count; i++)
                result.Add($"Character {i + 1}");

            return result;
        }

        if (!allowRepeatsIfNeeded && source.Length >= count)
        {
            List<string> pool = new List<string>(source);
            Shuffle(pool);
            return pool.Take(count).ToList();
        }

        for (int i = 0; i < count; i++)
            result.Add(source[UnityEngine.Random.Range(0, source.Length)]);

        return result;
    }

    private ActionOption[] PickRandomActions(ActionOption[] source, int count)
    {
        if (source == null || source.Length == 0)
            return Array.Empty<ActionOption>();

        List<ActionOption> pool = new List<ActionOption>(source);
        Shuffle(pool);
        return pool.Take(Mathf.Min(count, pool.Count)).ToArray();
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
    [ContextMenu("Reload Sprites From Folders")]
    public void ReloadFromFolders()
    {
        faces = LoadSpritesFromFolder(facesFolderPath);
        clothes = LoadSpritesFromFolder(clothesFolderPath);
        heads = LoadSpritesFromFolder(headsFolderPath);

        EditorUtility.SetDirty(this);
    }

    private Sprite[] LoadSpritesFromFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            Debug.LogWarning("Folder path is empty.", this);
            return Array.Empty<Sprite>();
        }

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning($"Folder not found: {folderPath}", this);
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