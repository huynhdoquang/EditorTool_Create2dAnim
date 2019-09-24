using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorCreateSpriteAnimBasic : EditorWindow
{
    string TextureURL = "";
    string saveURL = "";
    int Samples;
    //add Anim name
    List<string> animNames = new List<string>();

    [MenuItem("LineTool/Animations/BasicAnimation")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        EditorCreateSpriteAnimBasic window = (EditorCreateSpriteAnimBasic)EditorWindow.GetWindow(typeof(EditorCreateSpriteAnimBasic));
        window.Show();
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(TextureURL))
        {
            saveURL = EditorPrefs.GetString(LineTool.GetProjectKey("AnimationSaveURL_Basic"));
            TextureURL = EditorPrefs.GetString(LineTool.GetProjectKey("AnimationTextureURL_Basic"));
            Samples = EditorPrefs.GetInt(LineTool.GetProjectKey("AnimationSamples_Basic"));
        }

        //Select folder contain textures
        EditorGUILayout.BeginHorizontal();
        TextureURL = EditorGUILayout.TextField("Textures URL", TextureURL);
        if (GUILayout.Button("Browser...", GUILayout.Width(100)))
        {
            TextureURL = EditorUtility.OpenFolderPanel("Choose Textures File", TextureURL, "");
            EditorPrefs.SetString(LineTool.GetProjectKey("AnimationTextureURL_Basic"), TextureURL);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        //Get all sprites in this folder
        var dataPath = Application.dataPath;
        List<Sprite> allSprites = new List<Sprite>();
        if (!string.IsNullOrEmpty(TextureURL))
        {
            string[] folder_paths = System.IO.Directory.GetDirectories(TextureURL);
            foreach (string folder_path in folder_paths)
            {
                var fs = System.IO.Directory.GetFiles(folder_path, "*.png");
                foreach (var f in fs)
                {
                    string shortPath = "";
                    if (f.Contains(dataPath))
                    {
                        shortPath = f.Replace(dataPath, "Assets");
                    }
                    var objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(shortPath);
                    var _sprites = objects.Where(q => q is Sprite).Cast<Sprite>();
                    allSprites.AddRange(_sprites);
                }
            }

            var files = System.IO.Directory.GetFiles(TextureURL, "*.png");
            foreach (var f in files)
            {
                string shortPath = "";
                if (f.Contains(dataPath))
                {
                    shortPath = f.Replace(dataPath, "Assets");
                }
                var objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(shortPath);
                var _sprites = objects.Where(q => q is Sprite).Cast<Sprite>();
                allSprites.AddRange(_sprites);
            }
            EditorGUILayout.TextArea("Sprite Count: " + allSprites.Count);

            animNames.Clear();
            foreach (var s in allSprites)
            {
                var spriteName = s.name;
                var strlist = spriteName.Split('_');
                //Emoji_[emoji name]_[emoji trigger]_[stt]
                if (!animNames.Contains(strlist[0].ToLower()))
                {
                    animNames.Add(strlist[0].ToLower());
                }
            }
            EditorGUILayout.TextArea("Anim find Count: " + animNames.Count);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Samples", EditorStyles.boldLabel);
            Samples = EditorGUILayout.IntField(Samples);
            EditorPrefs.SetInt(LineTool.GetProjectKey("AnimationSamples_Basic"), Samples);
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.Space(10);

        //Generate animation
        GUILayout.BeginArea(new Rect((position.width - 100) / 2, 100, 100, 30));
        if (GUILayout.Button("Generate", GUILayout.Width(100), GUILayout.Height(30)))
        {
            string existURL = string.IsNullOrEmpty(saveURL) ? Application.dataPath : saveURL;
            saveURL = EditorUtility.OpenFolderPanel("Choose Animator Save Folder", existURL, "");
            //saveURL = EditorUtility.SaveFilePanel("Save Animation File", existURL, "EffectSprite2DName", "controller");
            if (saveURL.Contains(dataPath))
            {
                saveURL = saveURL.Replace(dataPath, "Assets");
            }
            EditorPrefs.SetString(LineTool.GetProjectKey("AnimationSaveURL_Basic"), saveURL);

            foreach (var animName in animNames)
            {
                var newSaveURL = saveURL + "/" + animName + ".controller";
                var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(newSaveURL);
                if (controller != null)
                {
                    var rootStateMachine = controller.layers[0].stateMachine;
                    var emptyState = rootStateMachine.AddState("empty");

                    var foundSprites = allSprites.FindAll(xx => xx.name.ToLower().Contains(animName));
                    if (foundSprites == null || foundSprites.Count == 0)
                        continue;
                    string clipName = animName;

                    var clip = createAnimationClip(foundSprites, clipName, true);

                    //Create state
                    var newState = rootStateMachine.AddState(clipName);
                    //Add motion of state as clip we've create
                    newState.motion = clip;
                    //Set this state is defaul state
                    rootStateMachine.defaultState = newState;

                    AssetDatabase.AddObjectToAsset(clip, controller);
                    allSprites.RemoveAll(xx => foundSprites.Contains(xx));
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Close();
        }
        GUILayout.EndArea();
    }
    AnimationClip createAnimationClip(List<Sprite> sources, string animName, bool isLoop = false)
    {
        sources.Sort((x, y) =>
        {
            return x.name.CompareTo(y.name);
        });
        float samples = Samples;
        AnimationClip animClip = new AnimationClip();
        animClip.name = animName;
        animClip.frameRate = samples;   // FPS=
        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";
        ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sources.Count];
        for (int i = 0; i < (sources.Count); i++)
        {
            spriteKeyFrames[i] = new ObjectReferenceKeyframe();
            spriteKeyFrames[i].time = i * (1 / (float)samples);
            spriteKeyFrames[i].value = sources[i];
        }
        AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);
        var settings = AnimationUtility.GetAnimationClipSettings(animClip);
        settings.loopTime = isLoop;
        settings.stopTime = sources.Count * (1 / (float)samples);
        AnimationUtility.SetAnimationClipSettings(animClip, settings);
        return animClip;
    }
}
