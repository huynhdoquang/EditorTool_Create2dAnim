using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Create animation from sprites
/// sprites name: "Pose" + "Direction"
/// </summary>
public class EditorCreateSpriteAnim4BlendTree : EditorWindow
{
    public struct AnimPoseAndTrigger
    {
        public string Pose;
        public string Trigger;
        public AnimPoseAndTrigger(string pose, string trigger)
        {
            this.Pose = pose;
            this.Trigger = trigger;
        }
    }

    public struct AnimDirectionInfo
    {
        public Vector2 Position;
        public string Name;
        public AnimDirectionInfo(string name, Vector2 pos)
        {
            this.Name = name;
            this.Position = pos;
        }
    }

    string TextureURL = "";
    string saveURL = "";
    int Samples;

    private List<AnimDirectionInfo> _directions;
    private List<AnimDirectionInfo> directions
    {
        get
        {
            if (_directions == null)
            {
                _directions = new List<AnimDirectionInfo>();
                _directions.Add(new AnimDirectionInfo("down_left", new Vector2(-1, -1)));
                _directions.Add(new AnimDirectionInfo("down_right", new Vector2(1, -1)));
                _directions.Add(new AnimDirectionInfo("up_left", new Vector2(-1, 1)));
                _directions.Add(new AnimDirectionInfo("up_right", new Vector2(1, 1)));
                _directions.Add(new AnimDirectionInfo("left", new Vector2(-1, 0)));
                _directions.Add(new AnimDirectionInfo("right", new Vector2(1, 0)));
                _directions.Add(new AnimDirectionInfo("up", new Vector2(0, 1)));
                _directions.Add(new AnimDirectionInfo("down", new Vector2(0, -1)));
            }
            return _directions;
        }
    }

    private List<AnimPoseAndTrigger> _poses;
    private List<AnimPoseAndTrigger> poses
    {
        get
        {
            if (_poses == null)
            {
                _poses = new List<AnimPoseAndTrigger>();
                _poses.Add(new AnimPoseAndTrigger("run", "walk"));
                _poses.Add(new AnimPoseAndTrigger("idle", "idle"));
            }
            return _poses;
        }
    }

    [MenuItem("LineTool/Animations/BlendTreeAnimations")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        EditorCreateSpriteAnim4BlendTree window = (EditorCreateSpriteAnim4BlendTree)EditorWindow.GetWindow(typeof(EditorCreateSpriteAnim4BlendTree));
        window.Show();
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(TextureURL))
        {
            saveURL = EditorPrefs.GetString(LineTool.GetProjectKey("AnimationSaveURL"));
            TextureURL = EditorPrefs.GetString(LineTool.GetProjectKey("AnimationTextureURL"));
            Samples = EditorPrefs.GetInt(LineTool.GetProjectKey("AnimationSamples"));
        }

        //Select folder contain textures
        EditorGUILayout.BeginHorizontal();
        TextureURL = EditorGUILayout.TextField("Textures URL", TextureURL);
        if (GUILayout.Button("Browser...", GUILayout.Width(100)))
        {
            TextureURL = EditorUtility.OpenFolderPanel("Choose Textures File", TextureURL, "");
            EditorPrefs.SetString(LineTool.GetProjectKey("AnimationTextureURL"), TextureURL);
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
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Samples", EditorStyles.boldLabel);
            Samples = EditorGUILayout.IntField(Samples);
            EditorPrefs.SetInt(LineTool.GetProjectKey("AnimationSamples"), Samples);
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.Space(10);

        //Generate animation
        GUILayout.BeginArea(new Rect((position.width - 100) / 2, 100, 100, 30));
        if (GUILayout.Button("Generate", GUILayout.Width(100), GUILayout.Height(30)))
        {
            string existURL = string.IsNullOrEmpty(saveURL) ? Application.dataPath : saveURL;
            saveURL = EditorUtility.SaveFilePanel("Save Animation File", existURL, "Frames", "controller");
            if (saveURL.Contains(dataPath))
            {
                saveURL = saveURL.Replace(dataPath, "Assets");
            }
            EditorPrefs.SetString(LineTool.GetProjectKey("AnimationSaveURL"), saveURL);

            //Create new animator controller
            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(saveURL);
            if (controller != null)
            {
                //Add parameter
                controller.AddParameter("idle", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("walk", AnimatorControllerParameterType.Trigger);
                //Parameter for determine what anim to play
                controller.AddParameter("speed", AnimatorControllerParameterType.Float);
                controller.AddParameter("faceX", AnimatorControllerParameterType.Float);
                controller.AddParameter("faceY", AnimatorControllerParameterType.Float);
            }

            List<Sprite> sprites = new List<Sprite>();
            sprites.AddRange(allSprites);

            var rootStateMachine = controller.layers[0].stateMachine;
            Dictionary<string, UnityEditor.Animations.AnimatorState> animStates = new Dictionary<string, UnityEditor.Animations.AnimatorState>();
            var emptyState = rootStateMachine.AddState("Empty");

            foreach (var poseandTrigger in poses)
            {
                var pose = poseandTrigger.Pose.ToLower();
                var trigger = poseandTrigger.Trigger;

                //Change sprite name to lower to determine anim by name
                if (sprites.Any(xx => xx.name.ToLower().Contains(pose) || (xx.name.ToLower().Replace("_", "")).Contains(pose)))
                {
                    UnityEditor.Animations.BlendTree blendTree;
                    var currentState = controller.CreateBlendTreeInController(pose, out blendTree, 0);
                    blendTree.name = pose;
                    blendTree.blendType = UnityEditor.Animations.BlendTreeType.SimpleDirectional2D;
                    blendTree.blendParameter = "faceX";
                    blendTree.blendParameterY = "faceY";
                    foreach (var dir in directions)
                    {
                        var sameDir = dir.Name.Replace("_", "");
                        var foundSprite = sprites.FindAll(xx => (xx.name.ToLower().Contains(pose) || (xx.name.ToLower().Replace("_", "")).Contains(pose)) && (xx.name.ToLower().Contains(dir.Name) || xx.name.ToLower().Contains(sameDir)));

                        if (foundSprite == null || foundSprite.Count == 0)
                            continue;
                        string clipName = controller.name + "_" + pose + "_" + dir.Name;

                        bool isLoop = trigger.Equals("idle") || trigger.Equals("walk");
                        Debug.Log(pose + " loop: " + isLoop);
                        var clip = createAnimationClip(foundSprite, clipName, isLoop);

                        AssetDatabase.AddObjectToAsset(clip, controller);
                        //AssetDatabase.CreateAsset(clip, saveURL + clipName+ ".anim");
                        blendTree.AddChild(clip, dir.Position);
                        if (!pose.Contains("fall"))
                            sprites.RemoveAll(xx => foundSprite.Contains(xx));
                        Debug.Log("Sprites Count: " + sprites.Count);
                    }
                    animStates.Add(pose, currentState);
                }
            }

            //Make Transition
            UnityEditor.Animations.AnimatorState idle = null;

            if (animStates.ContainsKey("idle"))
                idle = animStates["idle"];

            foreach (var animDict in animStates)
            {
                var pose = animDict.Key;
                var poseTrigger = poses.Find(x => x.Pose.ToLower().Equals(pose.ToLower())).Trigger;
                var animState = animDict.Value;

                if (pose.Equals("run"))
                {
                    var walk = animDict.Value;
                    var transStop = walk.AddTransition(idle);
                    transStop.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.001f, "speed");
                    transStop.duration = 0;
                    transStop.hasExitTime = true;
                    var transWalk = idle.AddTransition(walk);
                    transWalk.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.001f, "speed");
                    transWalk.duration = 0;
                    transWalk.hasExitTime = false;

                    var trans = rootStateMachine.AddAnyStateTransition(animStates[pose]);
                    trans.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, poseTrigger);
                    trans.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.001f, "speed");
                    trans.duration = 0;
                }
                else if (poses.Any(x => x.Pose.ToLower().Equals(pose)))
                {
                    //var poseTrigger = pose.Equals("attack") || pose.Equals("spell") ? "attack" : pose;
                    controller.AddParameter(poseTrigger, AnimatorControllerParameterType.Trigger);

                    var trans = rootStateMachine.AddAnyStateTransition(animState);
                    trans.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, poseTrigger);
                    trans.duration = 0;
                }
            }
            if (!animStates.ContainsKey("run") && idle != null)
            {
                var trans = rootStateMachine.AddAnyStateTransition(idle);
                trans.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "walk");
                trans.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.001f, "speed");
                trans.duration = 0;
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
