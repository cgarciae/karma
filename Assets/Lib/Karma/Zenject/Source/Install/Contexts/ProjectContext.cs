#if !NOT_UNITY3D

using ModestTree;

using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Zenject
{
    public class ProjectContext : Context
    {
        public const string ProjectContextResourcePath = "ProjectContext";
        public const string ProjectContextResourcePathOld = "ProjectCompositionRoot";

        static ProjectContext _instance;

        DiContainer _container;

        readonly List<object> _dependencyRoots = new List<object>();

        public override DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public static bool HasInstance
        {
            get
            {
                return _instance != null;
            }
        }

        public static ProjectContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = InstantiateNewRoot();

                    // Note: We use Initialize instead of awake here in case someone calls
                    // ProjectContext.Instance while ProjectContext is initializing
                    _instance.Initialize();
                }

                return _instance;
            }
        }

#if UNITY_EDITOR
        public static bool ValidateOnNextRun
        {
            get;
            set;
        }
#endif

        public static GameObject TryGetPrefab()
        {
            var prefab = (GameObject)Resources.Load(ProjectContextResourcePath);

            if (prefab == null)
            {
                prefab = (GameObject)Resources.Load(ProjectContextResourcePathOld);
            }

            return prefab;
        }

        public static ProjectContext InstantiateNewRoot()
        {
            Assert.That(GameObject.FindObjectsOfType<ProjectContext>().IsEmpty(),
                "Tried to create multiple instances of ProjectContext!");

            ProjectContext instance;

            var prefab = TryGetPrefab();

            if (prefab == null)
            {
                instance = new GameObject("ProjectContext")
                    .AddComponent<ProjectContext>();
            }
            else
            {
                instance = GameObject.Instantiate(prefab).GetComponent<ProjectContext>();

                Assert.IsNotNull(instance,
                    "Could not find ProjectContext component on prefab 'Resources/{0}.prefab'", ProjectContextResourcePath);
            }

            return instance;
        }

        public void EnsureIsInitialized()
        {
            // Do nothing - Initialize occurs in Instance property
        }

        void Initialize()
        {
            Log.Debug("Initializing ProjectContext");

            Assert.IsNull(_container);

            if (Application.isPlaying)
            // DontDestroyOnLoad can only be called when in play mode and otherwise produces errors
            // ProjectContext is created during design time (in an empty scene) when running validation
            // and also when running unit tests
            // In these cases we don't need DontDestroyOnLoad so just skip it
            {
                DontDestroyOnLoad(gameObject);
            }

            bool isValidating = false;

#if UNITY_EDITOR
            isValidating = ValidateOnNextRun;

            // Reset immediately to ensure it doesn't get used in another run
            ValidateOnNextRun = false;
#endif

            _container = new DiContainer(
                StaticContext.Container, isValidating);

            _container.LazyInstanceInjector.AddInstances(
                GetInjectableComponents().Cast<object>());

            _container.IsInstalling = true;

            try
            {
                InstallBindings();
            }
            finally
            {
                _container.IsInstalling = false;
            }

            _container.LazyInstanceInjector.LazyInjectAll();

            Assert.That(_dependencyRoots.IsEmpty());

            _dependencyRoots.AddRange(_container.ResolveDependencyRoots());
        }

        protected override IEnumerable<Component> GetInjectableComponents()
        {
            return ContextUtil.GetInjectableComponents(this.gameObject);
        }

        void InstallBindings()
        {
            _container.DefaultParent = this.transform;

            _container.Bind(typeof(TickableManager), typeof(InitializableManager), typeof(DisposableManager))
                .ToSelf().AsSingle().CopyIntoAllSubContainers();

            _container.Bind<Context>().FromInstance(this);

            _container.Bind<ProjectKernel>().FromComponent(this.gameObject).AsSingle().NonLazy();

            InstallSceneBindings();

            InstallInstallers();
        }
    }
}

#endif
