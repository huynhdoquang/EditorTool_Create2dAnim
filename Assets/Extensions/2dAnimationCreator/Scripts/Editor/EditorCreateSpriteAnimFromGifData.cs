using Assets.GifAssets.PowerGif;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Line;


class EmojiGif2AnimationData
{
    public EmojiGif2AnimationData(string path)
    {
        if (Borders == null)
            Borders = new Dictionary<string, string>();
        var lines = System.IO.File.ReadAllLines(path);
        foreach (var l in lines)
        {
            var row = l.Split('|');
            if (row.Length >= 2)
                Borders.Add(row[0], row[1]);
        }
    }
    /// <summary>
    /// first string: name of emoji
    /// second string: array of delay time
    /// </summary>
    public Dictionary<string, string> Borders;

    public string ToString()
    {
        string result = "";
        foreach (var b in Borders)
            result += b.Key + " " + b.Value + "\n";
        return result;

    }
}

public class EditorCreateSpriteAnimFromGifData : EditorWindow
{
    string TextureURL = "";
    string BorderURL = "";
    string saveURL = "";
    string GifDataURL = "";

    [MenuItem("LineTool/Animations/Animation From Gif Data")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        EditorCreateSpriteAnimFromGifData window = (EditorCreateSpriteAnimFromGifData)EditorWindow.GetWindow(typeof(EditorCreateSpriteAnimFromGifData));
        window.Show();
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(TextureURL))
        {
            saveURL = EditorPrefs.GetString(LineTool.GetProjectKey("AnimationSaveURL_FromGif"));
            TextureURL = EditorPrefs.GetString(LineTool.GetProjectKey("AnimationTextureURL_FromGif"));
            BorderURL = EditorPrefs.GetString(LineTool.GetProjectKey("AnimationBorderURL_FromGif"));
            GifDataURL = EditorPrefs.GetString(LineTool.GetProjectKey("AnimationGifDataURLURL_FromGif"));
        }

        //Select folder contain gif data
        EditorGUILayout.BeginHorizontal();
        GifDataURL = EditorGUILayout.TextField("GifData URL ", GifDataURL);
        if (GUILayout.Button("Browser...", GUILayout.Width(100)))
        {
            GifDataURL = EditorUtility.OpenFolderPanel("Choose GifData File", GifDataURL, "");
            EditorPrefs.SetString(LineTool.GetProjectKey("AnimationGifDataURLURL_FromGif"), GifDataURL);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //gif part

        List<Gif> gifs = new List<Gif>();

        if (!string.IsNullOrEmpty(GifDataURL))
        {
            string[] folder_paths = System.IO.Directory.GetDirectories(GifDataURL);
            foreach (string folder_path in folder_paths)
            {
                //Ignor FX folder cuz this file not right config name
                if (Path.GetFileName(folder_path) == "FX" || Path.GetFileName(folder_path) == "items")
                    continue;

                var files = System.IO.Directory.GetFiles(folder_path, "*.gif");

                foreach (var f in files)
                {
                    var bytes = File.ReadAllBytes(f);
                    var _gif = Gif.Decode(bytes);
                    _gif.Name = Path.GetFileName(folder_path) + "_" + Path.GetFileName(f);

                    gifs.Add(_gif);
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextArea("gif Count: " + gifs.Count);
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        BorderURL = EditorGUILayout.TextField("BorderURL ", BorderURL);
        if (GUILayout.Button("Browser...", GUILayout.Width(100)))
        {
            if (string.IsNullOrEmpty(BorderURL))
                BorderURL = EditorUtility.OpenFilePanel("Choose Border File Path", Application.dataPath, "txt");
            else
                BorderURL = EditorUtility.OpenFilePanel("Choose Border File Path", BorderURL, "txt");

            EditorPrefs.SetString(LineTool.GetProjectKey("AnimationBorderURL_FromGif"), BorderURL);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Get Info", GUILayout.Width(100), GUILayout.Height(30)))
        {
            string changeBorderContent = "";
            foreach (var g in gifs)
            {
                var s = "";
                s += g.Name + "|";
                foreach (var f in g.Frames)
                    s += f.Delay + " ";

                changeBorderContent += s + "\n";
            }

            //Save borderData
            if (!string.IsNullOrEmpty(BorderURL) && !string.IsNullOrEmpty(changeBorderContent))
            {
                System.IO.File.WriteAllText(BorderURL, changeBorderContent);
                EditorUtility.DisplayDialog("save gif info", "save Success", "OK");
            }
            else EditorUtility.DisplayDialog("Error", "pls select boder file path", "Gotta");

        }
        GUILayout.EndHorizontal();

        //======== end read data from gif

        EditorGUILayout.Space(); EditorGUILayout.Space();
        EditorGUILayout.LabelField("======================================");
        EditorGUILayout.Space(); EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        TextureURL = EditorGUILayout.TextField("Textures URL", TextureURL);
        if (GUILayout.Button("Browser...", GUILayout.Width(100)))
        {
            TextureURL = EditorUtility.OpenFolderPanel("Choose Textures File", TextureURL, "");
            EditorPrefs.SetString(LineTool.GetProjectKey("AnimationTextureURL_FromGif"), TextureURL);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();


        var dataPath = Application.dataPath;
        //New one
        List<string> animatorNames = new List<string>();

        List<string> fxTriggers = new List<string>();
        List<Sprite> sprites = new List<Sprite>();

        if (!string.IsNullOrEmpty(TextureURL))
        {
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
                sprites.AddRange(_sprites);
            }
            //Thêm trigger cho anim
            foreach (var s in sprites)
            {
                // "name" + "-" + "trigger"

                var spriteName = s.name;
                var spriteName1 = spriteName.Replace('-', '_');
                var strlist = spriteName1.Split('_');

                if (!animatorNames.Contains(strlist[0].ToLower()))
                {
                    animatorNames.Add(strlist[0].ToLower());
                }

                if (strlist.Length >= 2)
                {
                    var triggerName = strlist[1].ToLower();
                    if (!fxTriggers.Contains(triggerName))
                        fxTriggers.Add(triggerName);
                }
            }
            EditorGUILayout.TextArea("Animator Count: " + animatorNames.Count);
            EditorGUILayout.TextArea("Sprite Count: " + sprites.Count);
            EditorGUILayout.TextArea("Trigger Count: " + fxTriggers.Count);
        }

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate", GUILayout.Width(100), GUILayout.Height(30)))
        {
            if (string.IsNullOrEmpty(BorderURL))
            {
                EditorUtility.DisplayDialog("Error", "pls select boder file path", "Gotta");
                return;
            }

            EmojiGif2AnimationData borderData = new EmojiGif2AnimationData(BorderURL);

            string existURL = string.IsNullOrEmpty(saveURL) ? Application.dataPath : saveURL;
            saveURL = EditorUtility.OpenFolderPanel("Choose Animator Save Folder", existURL, "");
            //saveURL = EditorUtility.SaveFilePanel("Save Animation File", existURL, "EffectSprite2DName", "controller");
            if (saveURL.Contains(dataPath))
            {
                saveURL = saveURL.Replace(dataPath, "Assets");
            }
            EditorPrefs.SetString(LineTool.GetProjectKey("AnimationSaveURL_FromGif"), saveURL);

            foreach (var a in animatorNames)
            {
                var newSaveURL = saveURL + "/" + a + ".controller";
                var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(newSaveURL);
                if (controller != null)
                {
                    var rootStateMachine = controller.layers[0].stateMachine;
                    var emptyState = rootStateMachine.AddState("empty");
                    foreach (var trigger in fxTriggers)
                    {
                        var foundSprites = sprites.FindAll(xx => xx.name.ToLower().Contains(trigger) && xx.name.ToLower().Contains(a));
                        if (foundSprites == null || foundSprites.Count == 0)
                            continue;
                        string clipName = trigger;

                        //select info from gif
                        string info = "";
                        var gif_name = a + "_" + trigger + ".gif";
                        if (borderData.Borders.ContainsKey(gif_name))
                        {
                            info = borderData.Borders[gif_name];
                            Debug.Log("info " + info);

                            controller.AddParameter(trigger, AnimatorControllerParameterType.Trigger);

                            var clip = createAnimationClip(foundSprites, clipName, info, true);

                            AssetDatabase.AddObjectToAsset(clip, controller);
                            var newState = rootStateMachine.AddState(trigger);
                            newState.motion = clip;
                            var trans = rootStateMachine.AddAnyStateTransition(newState);
                            trans.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, trigger);
                            trans.duration = 0;
                            trans.hasExitTime = false;
                            sprites.RemoveAll(xx => foundSprites.Contains(xx));
                        }
                        else
                            continue;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Close();
        }
        GUILayout.EndHorizontal();
    }

    AnimationClip createAnimationClip(List<Sprite> sources, string animName, string info, bool isLoop = false)
    {
        sources.Sort((x, y) =>
        {
            return x.name.CompareTo(y.name);
        });
        AnimationClip animClip = new AnimationClip();
        animClip.name = animName;
        animClip.frameRate = 60;   // it will is default 

        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        //spriteBinding.type = typeof(UnityEngine.UI.Image);
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        //spriteBinding.propertyName = "m_Sprite";
        spriteBinding.propertyName = "m_Sprite";
        ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sources.Count + 1];

        var listDelay = info.Split(' ');

        float timePerframe = 1 / (float)60;

        float stop_time = 0;
        for (int i = 0; i < (sources.Count); i++)
        {
            spriteKeyFrames[i] = new ObjectReferenceKeyframe();
            if (listDelay.Length >= sources.Count)
            {
                //Tính tổng frame
                float totalDelayTime = 0;
                for (int j = 0; j <= i; j++)
                    totalDelayTime += j == 0 ? 0 : listDelay[j - 1].ToFloat();

                var totalFrame = totalDelayTime / timePerframe;

                spriteKeyFrames[i].time = totalFrame * (1 / (float)animClip.frameRate);
            }
            spriteKeyFrames[i].value = sources[i];

            if (i == sources.Count - 1)
            {
                if (i <= listDelay.Length)
                {
                    //Tính tổng frame
                    float totalDelayTime = 0;
                    for (int j = 0; j <= i + 1; j++)
                        totalDelayTime += j == 0 ? 0 : listDelay[j - 1].ToFloat();

                    var totalFrame = totalDelayTime / timePerframe;

                    stop_time = totalFrame * (1 / (float)animClip.frameRate);

                    spriteKeyFrames[i + 1].time = stop_time;
                    spriteKeyFrames[i + 1].value = sources[i];
                }
            }
        }

        AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);
        var settings = AnimationUtility.GetAnimationClipSettings(animClip);
        settings.loopTime = isLoop;
        settings.stopTime = stop_time;
        AnimationUtility.SetAnimationClipSettings(animClip, settings);
        return animClip;

    }

}
