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
            $"Cards: {generator.CardCount}\nFaces: {generator.FaceCount}\nClothes: {generator.ClothesCount}\nHeads: {generator.HeadCount}",
            MessageType.Info
        );

        GUILayout.Space(4);

        if (GUILayout.Button("Reload Sprites From Folders", GUILayout.Height(30)))
        {
            generator.ReloadFromFolders();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Find Card Images", GUILayout.Height(26)))
        {
            generator.FindCardImages();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Generate UI Characters", GUILayout.Height(30)))
        {
            generator.GenerateUICharacters();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Clear UI Characters", GUILayout.Height(26)))
        {
            generator.ClearGeneratedCharacters();
            EditorUtility.SetDirty(generator);
        }
    }
}
