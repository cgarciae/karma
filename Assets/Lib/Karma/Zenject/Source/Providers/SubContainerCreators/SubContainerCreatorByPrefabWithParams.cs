#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public class SubContainerCreatorByPrefabWithParams : ISubContainerCreator
    {
        readonly DiContainer _container;
        readonly IPrefabProvider _prefabProvider;
        readonly Type _installerType;
        readonly GameObjectCreationParameters _gameObjectBindInfo;

        public SubContainerCreatorByPrefabWithParams(
            Type installerType, DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo)
        {
            _gameObjectBindInfo = gameObjectBindInfo;
            _prefabProvider = prefabProvider;
            _container = container;
            _installerType = installerType;
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        DiContainer CreateTempContainer(List<TypeValuePair> args)
        {
            var tempSubContainer = Container.CreateSubContainer();

            foreach (var argPair in args)
            {
                tempSubContainer.Bind(argPair.Type)
                    .FromInstance(argPair.Value, true).WhenInjectedInto(_installerType);
            }

            return tempSubContainer;
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args)
        {
            Assert.That(!args.IsEmpty());

            var prefab = _prefabProvider.GetPrefab();
            var gameObject = CreateTempContainer(args).InstantiatePrefab(
                prefab, new object[0], _gameObjectBindInfo);

            var context = gameObject.GetComponent<GameObjectContext>();

            Assert.IsNotNull(context,
                "Expected prefab with name '{0}' to container a component of type 'GameObjectContext'", prefab.name);

            return context.Container;
        }
    }
}

#endif

