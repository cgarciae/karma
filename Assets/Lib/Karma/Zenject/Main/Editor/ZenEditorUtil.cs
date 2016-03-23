#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ModestTree;

namespace Zenject
{
    public static class ZenEditorUtil
    {
        public static DiContainer GetContainerForCurrentScene()
        {
            var compRoot = GameObject.FindObjectsOfType<SceneCompositionRoot>().OnlyOrDefault();

            if (compRoot == null)
            {
                throw new ZenjectException(
                    "Unable to find SceneCompositionRoot in current scene.");
            }

            return compRoot.Container;
        }

        public static List<ZenjectResolveException> ValidateAllActiveScenes(int maxErrors)
        {
            var activeScenes = UnityEditor.EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.ToString()).ToList();
            return ValidateScenes(activeScenes, maxErrors);
        }

        // This can be called by build scripts using batch mode unity for continuous integration testing
        public static void ValidateAllScenesFromScript()
        {
            var activeScenes = UnityEditor.EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.ToString()).ToList();
            ValidateScenes(activeScenes, 25, true);
        }

        public static List<ZenjectResolveException> ValidateScenes(List<string> sceneNames, int maxErrors)
        {
            return ValidateScenes(sceneNames, maxErrors, false);
        }

        public static List<ZenjectResolveException> ValidateScenes(List<string> sceneNames, int maxErrors, bool exitAfter)
        {
            var errors = new List<ZenjectResolveException>();
            var activeScenes = sceneNames
                .Select(x => new { Name = x, Path = GetScenePath(x) }).ToList();

            foreach (var sceneInfo in activeScenes)
            {
                Log.Trace("Validating Scene '{0}'", sceneInfo.Path);
                EditorApplication.OpenScene(sceneInfo.Path);

                errors.AddRange(ValidateCurrentScene().Take(maxErrors - errors.Count));

                if (errors.Count >= maxErrors)
                {
                    break;
                }
            }

            if (errors.IsEmpty())
            {
                Log.Trace("Successfully validated all {0} scenes", activeScenes.Count);

                if (exitAfter)
                {
                    // 0 = no errors
                    EditorApplication.Exit(0);
                }
            }
            else
            {
                Log.Error("Zenject Validation failed!  Found {0} errors.", errors.Count);

                foreach (var err in errors)
                {
                    Log.ErrorException(err);
                }

                if (exitAfter)
                {
                    // 1 = errors occurred
                    EditorApplication.Exit(1);
                }
            }

            return errors;
        }

        static string GetScenePath(string sceneName)
        {
            var namesToPaths = UnityEditor.EditorBuildSettings.scenes.ToDictionary(
                x => Path.GetFileNameWithoutExtension(x.path), x => x.path);

            if (!namesToPaths.ContainsKey(sceneName))
            {
                throw new Exception(
                    "Could not find scene with name '" + sceneName + "'");
            }

            return namesToPaths[sceneName];
        }

        public static IEnumerable<ZenjectResolveException> ValidateCurrentScene()
        {
            var compRoot = GameObject.FindObjectsOfType<SceneCompositionRoot>().OnlyOrDefault();

            if (compRoot == null || compRoot.Installers.IsEmpty())
            {
                return Enumerable.Empty<ZenjectResolveException>();
            }

            return ZenEditorUtil.ValidateInstallers(compRoot);
        }

        public static IEnumerable<ZenjectResolveException> ValidateInstallers(SceneCompositionRoot compRoot)
        {
            var globalContainer = GlobalCompositionRoot.CreateContainer(true, null);
            var container = compRoot.CreateContainer(true, globalContainer, new List<IInstaller>());

            foreach (var error in container.ValidateResolve(new InjectContext(container, typeof(IDependencyRoot), null)))
            {
                yield return error;
            }

            // Also make sure we can fill in all the dependencies in the built-in scene
            foreach (var curTransform in compRoot.GetComponentsInChildren<Transform>())
            {
                foreach (var monoBehaviour in curTransform.GetComponents<MonoBehaviour>())
                {
                    if (monoBehaviour == null)
                    {
                        Log.Warn("Found null MonoBehaviour on " + curTransform.name);
                        continue;
                    }

                    foreach (var error in container.ValidateObjectGraph(monoBehaviour.GetType()))
                    {
                        yield return error;
                    }
                }
            }

            foreach (var installer in globalContainer.InstalledInstallers.Concat(container.InstalledInstallers))
            {
                if (installer is IValidatable)
                {
                    foreach (var error in ((IValidatable)installer).Validate())
                    {
                        yield return error;
                    }
                }
            }

            foreach (var error in container.ValidateValidatables())
            {
                yield return error;
            }
        }

        public static void OutputObjectGraphForCurrentScene(
            DiContainer container, IEnumerable<Type> ignoreTypes, IEnumerable<Type> contractTypes)
        {
            string dotFilePath = EditorUtility.SaveFilePanel("Choose the path to export the object graph", "", "ObjectGraph", "dot");

            if (!dotFilePath.IsEmpty())
            {
                ObjectGraphVisualizer.OutputObjectGraphToFile(
                    container, dotFilePath, ignoreTypes, contractTypes);

                var dotExecPath = EditorPrefs.GetString("Zenject.GraphVizDotExePath", "");

                if (dotExecPath.IsEmpty() || !File.Exists(dotExecPath))
                {
                    EditorUtility.DisplayDialog(
                        "GraphViz", "Unable to locate GraphViz.  Please select the graphviz 'dot.exe' file which can be found at [GraphVizInstallDirectory]/bin/dot.exe.  If you do not have GraphViz you can download it at http://www.graphviz.org", "Ok");

                    dotExecPath = EditorUtility.OpenFilePanel("Please select dot.exe from GraphViz bin directory", "", "exe");

                    EditorPrefs.SetString("Zenject.GraphVizDotExePath", dotExecPath);
                }

                if (!dotExecPath.IsEmpty())
                {
                    RunDotExe(dotExecPath, dotFilePath);
                }
            }
        }

        static void RunDotExe(string dotExePath, string dotFileInputPath)
        {
            var outputDir = Path.GetDirectoryName(dotFileInputPath);
            var fileBaseName = Path.GetFileNameWithoutExtension(dotFileInputPath);

            var proc = new System.Diagnostics.Process();

            proc.StartInfo.FileName = dotExePath;
            proc.StartInfo.Arguments = "-Tpng {0}.dot -o{0}.png".Fmt(fileBaseName);
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = outputDir;

            proc.Start();
            proc.WaitForExit();

            var errorMessage = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (errorMessage.IsEmpty())
            {
                EditorUtility.DisplayDialog(
                    "Success!", "Successfully created files {0}.dot and {0}.png".Fmt(fileBaseName), "Ok");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Error", "Error occurred while generating {0}.png".Fmt(fileBaseName), "Ok");

                Log.Error("Zenject error: Failure during object graph creation: " + errorMessage);

                // Do we care about STDOUT?
                //var outputMessage = proc.StandardOutput.ReadToEnd();
                //Log.Error("outputMessage = " + outputMessage);
            }

        }
    }
}
#endif
