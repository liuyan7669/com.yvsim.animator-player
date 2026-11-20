using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Player = Yvsim.AnimatorPlayer.AnimatorPlayer;

namespace Yvsim.AnimatorPlayer.Samples.Editor
{
    /// <summary>使用 Unity API 生成独立的 Animator 示例资源，不覆盖现有场景或配置。</summary>
    public static class AnimatorPlaybackSampleBuilder
    {
        /// <summary>从菜单在唯一目录创建示例，保留当前打开场景和未保存内容。</summary>
        [MenuItem("Yvsim/示例/Animator Player/创建播放示例")]
        public static void Create()
        {
            string folder = AssetDatabase.GenerateUniqueAssetPath("Assets/YvsimAnimatorPlayback");
            string scenePath = BuildInto(folder);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            EditorGUIUtility.PingObject(Selection.activeObject);
            Debug.Log("Animator 播放示例已创建：" + scenePath);
        }

        /// <summary>在尚不存在的 Assets 子目录生成网格、材质、三段动画、Controller、Prefab 和场景。</summary>
        /// <param name="folder">尚不存在的 Assets 相对目录；父目录可以存在。</param>
        /// <returns>生成场景的 Assets 相对路径。</returns>
        public static string BuildInto(string folder)
        {
            folder = folder.Replace('\\', '/').TrimEnd('/');
            if (!folder.StartsWith("Assets/", StringComparison.Ordinal) || folder.Contains("..") || Directory.Exists(folder))
                throw new ArgumentException("请指定尚不存在的 Assets 子目录。", nameof(folder));
            string current = "Assets";
            foreach (string part in folder.Substring(7).Split('/'))
            {
                string next = current + "/" + part;
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, part);
                current = next;
            }

            var mesh = new Mesh { name = "Sample Cube" };
            mesh.vertices = new[] {
                new Vector3(-.5f,-.5f,-.5f), new Vector3(.5f,-.5f,-.5f), new Vector3(.5f,.5f,-.5f), new Vector3(-.5f,.5f,-.5f),
                new Vector3(-.5f,-.5f,.5f), new Vector3(.5f,-.5f,.5f), new Vector3(.5f,.5f,.5f), new Vector3(-.5f,.5f,.5f)
            };
            mesh.triangles = new[] { 0,2,1,0,3,2, 4,5,6,4,6,7, 0,1,5,0,5,4, 3,7,6,3,6,2, 0,4,7,0,7,3, 1,2,6,1,6,5 };
            mesh.RecalculateNormals();
            AssetDatabase.CreateAsset(mesh, folder + "/Sample Cube.asset");
            var material = new Material(Shader.Find("Standard")) { name = "Sample Material", color = new Color(.25f,.65f,.85f) };
            AssetDatabase.CreateAsset(material, folder + "/Sample Material.mat");
            var controller = AnimatorController.CreateAnimatorControllerAtPath(folder + "/Animator Playback.controller");
            string[] names = { "Move", "Turn", "Scale" };
            for (int i = 0; i < names.Length; i++)
            {
                var clip = new AnimationClip { name = names[i], frameRate = 30f };
                if (i == 0) SetCurve(clip, "m_LocalPosition.x", AnimationCurve.Linear(0f, 0f, 1f, 2f));
                if (i == 1) SetCurve(clip, "localEulerAnglesRaw.y", AnimationCurve.Linear(0f, 0f, 1f, 180f));
                if (i == 2)
                    foreach (string axis in new[] { "x", "y", "z" })
                        SetCurve(clip, "m_LocalScale." + axis, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(.5f, 1.5f), new Keyframe(1f, 1f)));
                AssetDatabase.CreateAsset(clip, folder + "/" + names[i] + ".anim");
                var state = controller.layers[0].stateMachine.AddState(names[i]);
                state.motion = clip;
                if (i == 0) controller.layers[0].stateMachine.defaultState = state;
            }

            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                var host = new GameObject("Animator Playback Example");
                var animator = host.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var player = host.AddComponent<Player>();
                var example = host.AddComponent<AnimatorPlaybackExample>();
                using (var serialized = new SerializedObject(example))
                {
                    serialized.FindProperty("player").objectReferenceValue = player;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                var visual = new GameObject("Visual");
                visual.transform.SetParent(host.transform, false);
                visual.AddComponent<MeshFilter>().sharedMesh = mesh;
                visual.AddComponent<MeshRenderer>().sharedMaterial = material;
                PrefabUtility.SaveAsPrefabAsset(host, folder + "/Animator Playback.prefab");

                var camera = new GameObject("Main Camera").AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.transform.position = new Vector3(3f, 2f, -6f);
                camera.transform.rotation = Quaternion.LookRotation(new Vector3(1f, 0f, 0f) - camera.transform.position);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.14f,.16f,.19f);
                var light = new GameObject("Directional Light").AddComponent<Light>();
                light.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
                string path = folder + "/Animator Playback.unity";
                if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException("保存示例场景失败。");
                AssetDatabase.SaveAssets();
                return path;
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static void SetCurve(AnimationClip clip, string property, AnimationCurve curve)
        {
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("Visual", typeof(Transform), property), curve);
        }
    }
}
