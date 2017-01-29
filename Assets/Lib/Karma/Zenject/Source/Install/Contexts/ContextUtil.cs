#if !NOT_UNITY3D

using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject.Internal;

namespace Zenject
{
    public static class ContextUtil
    {
        public static IEnumerable<Component> GetInjectableComponents(Scene scene)
        {
            foreach (var gameObject in GetRootGameObjects(scene))
            {
                foreach (var component in GetInjectableComponents(gameObject))
                {
                    yield return component;
                }
            }

            yield break;
        }

        public static IEnumerable<Component> GetInjectableComponents(GameObject gameObject)
        {
            foreach (var component in ZenUtilInternal.GetInjectableComponentsBottomUp(gameObject, true))
            {
                if (component == null)
                {
                    // This warning about fiBackupSceneStorage appears in normal cases so just ignore
                    // Not sure what it is
                    if (gameObject.name != "fiBackupSceneStorage")
                    {
                        Log.Warn("Zenject: Found null component on game object '{0}'.  Possible missing script.", gameObject.name);
                    }
                    continue;
                }

                if (component.GetType().DerivesFrom<MonoInstaller>())
                {
                    // Do not inject on installers since these are always injected before they are installed
                    continue;
                }

                yield return component;
            }
        }

        public static IEnumerable<GameObject> GetRootGameObjects(Scene scene)
        {
            // Note: We can't use activeScene.GetRootObjects() here because that apparently fails with an exception
            // about the scene not being loaded yet when executed in Awake
            // We also can't use GameObject.FindObjectsOfType<Transform>() because that does not include inactive game objects
            // So we use Resources.FindObjectsOfTypeAll, even though that may include prefabs.  However, our assumption here
            // is that prefabs do not have their "scene" property set correctly so this should work
            //
            // It's important here that we only inject into root objects that are part of our scene, to properly support
            // multi-scene editing features of Unity 5.x
            //
            // Also, even with older Unity versions, if there is an object that is marked with DontDestroyOnLoad, then it will
            // be injected multiple times when another scene is loaded
            //
            // We also make sure not to inject into the project root objects which are injected by ProjectContext.
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(x => x.transform.parent == null
                    && x.GetComponent<ProjectContext>() == null
                    && x.scene == scene);
        }
    }
}

#endif
