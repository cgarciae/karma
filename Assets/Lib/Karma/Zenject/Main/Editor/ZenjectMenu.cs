using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Debug = UnityEngine.Debug;
using ModestTree;

namespace Zenject
{
    public static class ZenjectMenu
    {
        public static void ValidateCurrentSceneThenPlay()
        {
            if (ValidateCurrentScene())
            {
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("Edit/Zenject/Create Global Composition Root")]
        public static void CreateProjectConfig()
        {
            var asset = ScriptableObject.CreateInstance<GlobalInstallerConfig>();

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Assets/Resources")))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/ZenjectGlobalCompositionRoot.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.Refresh();
        }

        // Note that you can also use ZenEditorUtil.ValidateAllActiveScenes if you want the errors back
        [MenuItem("Edit/Zenject/Validate All Active Scenes")]
        public static bool ValidateAllActiveScenes()
        {
            var startScene = EditorApplication.currentScene;

            var activeScenes = UnityEditor.EditorBuildSettings.scenes.Where(x => x.enabled)
                .Select(x => new { Name = Path.GetFileNameWithoutExtension(x.path), Path = x.path }).ToList();

            var failedScenes = new List<string>();

            foreach (var sceneInfo in activeScenes)
            {
                Log.Trace("Validating scene '{0}'...", sceneInfo.Name);

                EditorApplication.OpenScene(sceneInfo.Path);

                var compRoot = GameObject.FindObjectsOfType<SceneCompositionRoot>().OnlyOrDefault();

                // Do not validate if there is no comp root
                if (compRoot != null)
                {
                    if (!ValidateCurrentScene())
                    {
                        Log.Error("Failed to validate scene '{0}'", sceneInfo.Name);
                        failedScenes.Add(sceneInfo.Name);
                    }
                }
            }

            EditorApplication.OpenScene(startScene);

            if (failedScenes.IsEmpty())
            {
                Log.Trace("Successfully validated all {0} scenes", activeScenes.Count);
                return true;
            }
            else
            {
                Log.Error("Validated {0}/{1} scenes. Failed to validate the following: {2}",
                    activeScenes.Count - failedScenes.Count, activeScenes.Count, failedScenes.Join(", "));
                return false;
            }
        }

        [MenuItem("Edit/Zenject/Validate Current Scene #%v")]
        public static bool ValidateCurrentScene()
        {
            var startTime = DateTime.Now;
            var compRoot = GameObject.FindObjectsOfType<SceneCompositionRoot>().OnlyOrDefault();

            if (compRoot != null)
            {
                return ValidateCompRoot(compRoot, startTime);
            }

            var decoratorCompRoot = GameObject.FindObjectsOfType<SceneDecoratorCompositionRoot>().OnlyOrDefault();

            if (decoratorCompRoot == null)
            {
                Log.Error("Unable to find unique composition root in current scene");
                return false;
            }

            var sceneName = decoratorCompRoot.SceneName;
            var scenePath = UnityEditor.EditorBuildSettings.scenes.Select(x => x.path).Where(x => Path.GetFileNameWithoutExtension(x) == sceneName).SingleOrDefault();

            if (scenePath == null)
            {
                Log.Error("Could not find scene path for decorated scene '{0}'", sceneName);
                return false;
            }

            var rootObjectsBefore = GameObject.FindObjectsOfType<Transform>().Where(x => x.parent == null).ToList();

            // Use finally to ensure we clean up the data added from EditorApplication.OpenSceneAdditive
            try
            {
                EditorApplication.OpenSceneAdditive(scenePath);

                compRoot = GameObject.FindObjectsOfType<SceneCompositionRoot>().OnlyOrDefault();

                if (compRoot == null)
                {
                    Log.Error("Could not find composition root in decorated scene '{0}'", sceneName);
                    return false;
                }

                SceneCompositionRoot.BeforeInstallHooks += decoratorCompRoot.AddPreBindings;
                SceneCompositionRoot.AfterInstallHooks += decoratorCompRoot.AddPostBindings;

                return ValidateCompRoot(compRoot, startTime);
            }
            finally
            {
                decoratorCompRoot.transform.parent = null;

                var rootObjectsAfter = GameObject.FindObjectsOfType<Transform>().Where(x => x.parent == null).ToList();

                foreach (var newObject in rootObjectsAfter.Except(rootObjectsBefore).Select(x => x.gameObject))
                {
                    GameObject.DestroyImmediate(newObject);
                }
            }
        }

        static bool ValidateCompRoot(SceneCompositionRoot compRoot, DateTime startTime)
        {
            if (compRoot.Installers.IsEmpty())
            {
                Log.Warn("Could not find installers while validating current scene");
                // Return true to allow playing in this case
                return true;
            }

            // Only show a few to avoid spamming the log too much
            var resolveErrors = ZenEditorUtil.ValidateInstallers(compRoot).Take(10).ToList();

            foreach (var error in resolveErrors)
            {
                Log.ErrorException(error);
            }

            var secondsElapsed = (DateTime.Now - startTime).Milliseconds / 1000.0f;

            if (resolveErrors.Any())
            {
                Log.Error("Validation Completed With Errors, Took {0:0.00} Seconds.", secondsElapsed);
                return false;
            }

            Log.Info("Validation Completed Successfully, Took {0:0.00} Seconds.", secondsElapsed);
            return true;
        }

        [MenuItem("Edit/Zenject/Output Object Graph For Current Scene")]
        public static void OutputObjectGraphForScene()
        {
            if (!EditorApplication.isPlaying)
            {
                Log.Error("Zenject error: Must be in play mode to generate object graph.  Hit Play button and try again.");
                return;
            }

            DiContainer container;
            try
            {
                container = ZenEditorUtil.GetContainerForCurrentScene();
            }
            catch (ZenjectException e)
            {
                Log.Error("Unable to find container in current scene. " + e.Message);
                return;
            }

            var ignoreTypes = Enumerable.Empty<Type>();
            var types = container.AllConcreteTypes;

            ZenEditorUtil.OutputObjectGraphForCurrentScene(container, ignoreTypes, types);
        }
    }
}
