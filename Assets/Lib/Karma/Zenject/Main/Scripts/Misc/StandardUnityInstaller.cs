#if !ZEN_NOT_UNITY3D

using System;
using System.Linq;
using UnityEngine;

namespace Zenject
{
    public class StandardUnityInstaller : Installer
    {
        [Inject]
        CompositionRoot _root = null;

        // Install basic functionality for most unity apps
        public override void InstallBindings()
        {
            Container.Bind<IDependencyRoot>().ToSingleMonoBehaviour<UnityDependencyRoot>(_root == null ? null : _root.gameObject);

            Container.Bind<TickableManager>().ToSingle();

            Container.Bind<InitializableManager>().ToSingle();
            Container.Bind<DisposableManager>().ToSingle();

            Container.Bind<UnityEventManager>().ToSingleGameObject();
            Container.Bind<ITickable>().ToLookup<UnityEventManager>();
        }
    }
}

#endif
