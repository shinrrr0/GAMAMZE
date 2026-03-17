using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterGenerator2D))]
public class CharacterGenerator2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        CharacterGenerator2D generator = (CharacterGenerator2D)target;

        EditorGUILayout.HelpBox(
            $"Faces: {generator.FaceCount}\nClothes: {generator.ClothesCount}\nHeads: {generator.HeadCount}",
            MessageType.Info
        );

        if (GUILayout.Button("Reload Sprites From Folders", GUILayout.Height(30)))
        {
            generator.ReloadFromFolders();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Generate Unique Characters", GUILayout.Height(30)))
        {
            generator.GenerateUniqueCharacters();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Clear Generated Characters", GUILayout.Height(26)))
        {
            generator.ClearGeneratedCharacters();
            EditorUtility.SetDirty(generator);
        }
    }
}