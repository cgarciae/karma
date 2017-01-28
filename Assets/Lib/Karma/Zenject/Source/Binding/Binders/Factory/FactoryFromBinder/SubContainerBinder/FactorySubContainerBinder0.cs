using System;

namespace Zenject
{
    public class FactorySubContainerBinder<TContract>
        : FactorySubContainerBinderBase<TContract>
    {
        public FactorySubContainerBinder(
            BindInfo bindInfo, Type factoryType,
            BindFinalizerWrapper finalizerWrapper, object subIdentifier)
            : base(bindInfo, factoryType, finalizerWrapper, subIdentifier)
        {
        }

        public ConditionBinder ByMethod(Action<DiContainer> installerMethod)
        {
            SubFinalizer = CreateFinalizer(
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByMethod(
                        container, installerMethod)));

            return new ConditionBinder(BindInfo);
        }

#if !NOT_UNITY3D

        public GameObjectNameGroupNameBinder ByPrefab(UnityEngine.Object prefab)
        {
            BindingUtil.AssertIsValidPrefab(prefab);

            var gameObjectInfo = new GameObjectCreationParameters();

            SubFinalizer = CreateFinalizer(
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByPrefab(
                        container,
                        new PrefabProvider(prefab),
                        gameObjectInfo)));

            return new GameObjectNameGroupNameBinder(BindInfo, gameObjectInfo);
        }

        public GameObjectNameGroupNameBinder ByPrefabResource(string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);

            var gameObjectInfo = new GameObjectCreationParameters();

            SubFinalizer = CreateFinalizer(
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByPrefab(
                        container,
                        new PrefabProviderResource(resourcePath),
                        gameObjectInfo)));

            return new GameObjectNameGroupNameBinder(BindInfo, gameObjectInfo);
        }
#endif
    }
}
