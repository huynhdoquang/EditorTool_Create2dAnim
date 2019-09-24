using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SliceSpritesAutomatically : EditorWindow
{
    string TextureURL = "";
    string BorderURL = "";

    [MenuItem("LineTool/SpriteEditor/Slice Sprites Auto")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SliceSpritesAutomatically window = (SliceSpritesAutomatically)EditorWindow.GetWindow(typeof(SliceSpritesAutomatically));
        window.Show();
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(TextureURL))
        {
            BorderURL = EditorPrefs.GetString(LineTool.GetProjectKey("AnimationBorderURL_SliceSprites"));
            TextureURL = EditorPrefs.GetString(LineTool.GetProjectKey("AnimationTextureURL_SliceSprites"));
        }


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("TO DO!");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //Select folder contain textures
        EditorGUILayout.BeginHorizontal();
        TextureURL = EditorGUILayout.TextField("Textures URL", TextureURL);
        if (GUILayout.Button("Browser...", GUILayout.Width(100)))
        {
            TextureURL = EditorUtility.OpenFilePanel("Choose Textures File", TextureURL, "");
            EditorPrefs.SetString(LineTool.GetProjectKey("AnimationTextureURL_SliceSprites"), TextureURL);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        BorderURL = EditorGUILayout.TextField("Border URL", BorderURL);
        if (GUILayout.Button("Browser...", GUILayout.Width(100)))
        {
            BorderURL = EditorUtility.OpenFolderPanel("Choose Boder File", BorderURL, "");
            EditorPrefs.SetString(LineTool.GetProjectKey("AnimationBorderURL_SliceSprites"), BorderURL);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }
    static void SetSpriteNames()
    {
        //Texture2D myTexture = (Texture2D)Resources.LoadAssetAtPath<Texture2D>("Assets/Sprites/MyTexture.png");

        //string path = AssetDatabase.GetAssetPath(myTexture);
        //TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        //ti.isReadable = true;

        //List<SpriteMetaData> newData = new List<SpriteMetaData>();

        //int SliceWidth = 16;
        //int SliceHeight = 16;

        //for (int i = 0; i < myTexture.width; i += SliceWidth)
        //{
        //    for (int j = myTexture.height; j > 0; j -= SliceHeight)
        //    {
        //        SpriteMetaData smd = new SpriteMetaData();
        //        smd.pivot = new Vector2(0.5f, 0.5f);
        //        smd.alignment = 9;
        //        smd.name = (myTexture.height - j) / SliceHeight + ", " + i / SliceWidth;
        //        smd.rect = new Rect(i, j - SliceHeight, SliceWidth, SliceHeight);

        //        newData.Add(smd);
        //    }
        //}

        //ti.spritesheet = newData.ToArray();
        //AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }
}
