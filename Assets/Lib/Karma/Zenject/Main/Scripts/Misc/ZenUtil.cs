using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using ModestTree;
using ModestTree.Util;

#if !ZEN_NOT_UNITY3D
using UnityEngine;
#endif

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class ZenUtil
    {
        // Due to the way that Unity overrides the Equals operator,
        // normal null checks such as (x == null) do not always work as
        // expected
        // In those cases you can use this function which will also
        // work with non-unity objects
        public static bool IsNull(System.Object obj)
        {
            return obj == null || obj.Equals(null);
        }

#if !ZEN_NOT_UNITY3D
        public static void LoadScene(string levelName)
        {
            LoadSceneInternal(levelName, false, null, null);
        }

        public static void LoadScene(string levelName, Action<DiContainer> preBindings)
        {
            LoadSceneInternal(levelName, false, preBindings, null);
        }

        public static void LoadScene(
            string levelName, Action<DiContainer> preBindings, Action<DiContainer> postBindings)
        {
            LoadSceneInternal(levelName, false, preBindings, postBindings);
        }

        public static void LoadSceneAdditive(string levelName)
        {
            LoadSceneInternal(levelName, true, null, null);
        }

        public static void LoadSceneAdditive(string levelName, Action<DiContainer> preBindings)
        {
            LoadSceneInternal(levelName, true, preBindings, null);
        }

        public static void LoadSceneAdditive(
            string levelName, Action<DiContainer> preBindings, Action<DiContainer> postBindings)
        {
            LoadSceneInternal(levelName, true, preBindings, postBindings);
        }

        static void LoadSceneInternal(
            string levelName, bool isAdditive, Action<DiContainer> preBindings, Action<DiContainer> postBindings)
        {
            if (preBindings != null)
            {
                SceneCompositionRoot.BeforeInstallHooks += preBindings;
            }

            if (postBindings != null)
            {
                SceneCompositionRoot.AfterInstallHooks += postBindings;
            }

            Assert.That(Application.CanStreamedLevelBeLoaded(levelName), "Unable to load level '{0}'", levelName);

            if (isAdditive)
            {
                Application.LoadLevelAdditive(levelName);
            }
            else
            {
                Log.Debug("Starting to load scene '{0}'", levelName);
                Application.LoadLevel(levelName);
                Log.Debug("Finished loading scene '{0}'", levelName);
            }
        }

        // This method can be used to load the given scene and perform injection on its contents
        // Note that the scene we're loading can have [Inject] flags however it should not have
        // its own composition root
        public static IEnumerator LoadSceneAdditiveWithContainer(
            string levelName, DiContainer parentContainer)
        {
            var rootObjectsBeforeLoad = UnityUtil.GetRootGameObjects();

            Application.LoadLevelAdditive(levelName);

            // Wait one frame for objects to be added to the scene heirarchy
            yield return null;

            var rootObjectsAfterLoad = UnityUtil.GetRootGameObjects();

            foreach (var newObject in rootObjectsAfterLoad.Except(rootObjectsBeforeLoad))
            {
                Assert.That(newObject.GetComponent<SceneCompositionRoot>() == null,
                    "LoadSceneAdditiveWithContainer does not expect a container to exist in the loaded scene");

                parentContainer.InjectGameObject(newObject);
            }
        }
#endif
    }
}
