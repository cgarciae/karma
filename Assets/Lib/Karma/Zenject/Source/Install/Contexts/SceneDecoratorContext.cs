#if !NOT_UNITY3D

using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Zenject
{
    public class SceneDecoratorContext : Context
    {
        [FormerlySerializedAs("SceneName")]
        [SerializeField]
        string _decoratedContractName = null;

        DiContainer _container;

        public string DecoratedContractName
        {
            get
            {
                return _decoratedContractName;
            }
        }

        public override DiContainer Container
        {
            get
            {
                Assert.IsNotNull(_container);
                return _container;
            }
        }

        public void Initialize(DiContainer container)
        {
            Assert.IsNull(_container);
            _container = container;

            container.LazyInstanceInjector
                .AddInstances(GetInjectableComponents().Cast<object>());
        }

        public void InstallDecoratorSceneBindings()
        {
            _container.Bind<SceneDecoratorContext>().FromInstance(this);
            InstallSceneBindings();
        }

        public void InstallDecoratorInstallers()
        {
            InstallInstallers();
        }

        protected override IEnumerable<Component> GetInjectableComponents()
        {
            return ContextUtil.GetInjectableComponents(this.gameObject.scene);
        }
    }
}

#endif
