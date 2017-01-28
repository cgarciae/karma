using System;
using ModestTree;

namespace Zenject
{
    public class FactorySubContainerBinderWithParams<TContract> : FactorySubContainerBinderBase<TContract>
    {
        public FactorySubContainerBinderWithParams(
            BindInfo bindInfo, Type factoryType,
            BindFinalizerWrapper finalizerWrapper, object subIdentifier)
            : base(bindInfo, factoryType, finalizerWrapper, subIdentifier)
        {
        }

#if !NOT_UNITY3D

        public GameObjectNameGroupNameBinder ByPrefab<TInstaller>(UnityEngine.Object prefab)
            where TInstaller : IInstaller
        {
            return ByPrefab(typeof(TInstaller), prefab);
        }

        public GameObjectNameGroupNameBinder ByPrefab(Type installerType, UnityEngine.Object prefab)
        {
            BindingUtil.AssertIsValidPrefab(prefab);

            Assert.That(installerType.DerivesFrom<MonoInstaller>(),
                "Invalid installer type given during bind command.  Expected type '{0}' to derive from 'MonoInstaller'", installerType.Name());

            var gameObjectInfo = new GameObjectCreationParameters();

            SubFinalizer = CreateFinalizer(
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByPrefabWithParams(
                        installerType,
                        container,
                        new PrefabProvider(prefab),
                        gameObjectInfo)));

            return new GameObjectNameGroupNameBinder(BindInfo, gameObjectInfo);
        }

        public GameObjectNameGroupNameBinder ByPrefabResource<TInstaller>(string resourcePath)
            where TInstaller : IInstaller
        {
            return ByPrefabResource(typeof(TInstaller), resourcePath);
        }

        public GameObjectNameGroupNameBinder ByPrefabResource(
            Type installerType, string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);

            var gameObjectInfo = new GameObjectCreationParameters();

            SubFinalizer = CreateFinalizer(
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByPrefabWithParams(
                        installerType,
                        container,
                        new PrefabProviderResource(resourcePath),
                        gameObjectInfo)));

            return new GameObjectNameGroupNameBinder(BindInfo, gameObjectInfo);
        }
#endif
    }
}
