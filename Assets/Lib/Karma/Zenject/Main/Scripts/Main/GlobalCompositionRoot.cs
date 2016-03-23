#if !ZEN_NOT_UNITY3D

#pragma warning disable 414
using ModestTree;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zenject
{
    public sealed class GlobalCompositionRoot : CompositionRoot
    {
        static GlobalCompositionRoot _instance;
        DiContainer _container;
        IDependencyRoot _dependencyRoot;
        bool _hasInitialized;

        public override DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public static GlobalCompositionRoot Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("Global Composition Root")
                        .AddComponent<GlobalCompositionRoot>();
                }
                return _instance;
            }
        }

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // Is this a good idea?
            //go.hideFlags = HideFlags.HideInHierarchy;

            _container = CreateContainer(false, this);
            _dependencyRoot = _container.Resolve<IDependencyRoot>();
        }

        public void InitializeIfNecessary()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                _dependencyRoot.Initialize();
            }
        }

        // If we're destroyed manually somehow handle that
        public void OnDestroy()
        {
            _instance = null;
            _dependencyRoot = null;
        }

        public static DiContainer CreateContainer(bool allowNullBindings, GlobalCompositionRoot root)
        {
            Assert.That(allowNullBindings || root != null);

            var container = new DiContainer(root == null ? null : root.transform);

            container.AllowNullBindings = allowNullBindings;

            container.Bind<GlobalCompositionRoot>().ToInstance(root);
            container.Bind<CompositionRoot>().ToInstance(root);

            container.Install<StandardUnityInstaller>();

            CompositionRootHelper.InstallSceneInstallers(container, GetGlobalInstallers());

            return container;
        }

        static IEnumerable<IInstaller> GetGlobalInstallers()
        {
            // Allow either naming convention
            var installerConfigs1 = Resources.LoadAll("ZenjectGlobalCompositionRoot", typeof(GlobalInstallerConfig));
            var installerConfigs2 = Resources.LoadAll("ZenjectGlobalInstallers", typeof(GlobalInstallerConfig));

            return installerConfigs1.Concat(installerConfigs2).Cast<GlobalInstallerConfig>().SelectMany(x => x.Installers).Cast<IInstaller>();
        }
    }
}

#endif
