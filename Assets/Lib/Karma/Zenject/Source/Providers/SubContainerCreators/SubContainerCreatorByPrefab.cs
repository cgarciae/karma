#if !NOT_UNITY3D

using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public class SubContainerCreatorByPrefab : ISubContainerCreator
    {
        readonly GameObjectCreationParameters _gameObjectBindInfo;
        readonly IPrefabProvider _prefabProvider;
        readonly DiContainer _container;

        public SubContainerCreatorByPrefab(
            DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo)
        {
            _gameObjectBindInfo = gameObjectBindInfo;
            _prefabProvider = prefabProvider;
            _container = container;
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args)
        {
            Assert.That(args.IsEmpty());

            var prefab = _prefabProvider.GetPrefab();
            var gameObject = _container.InstantiatePrefab(
                prefab, new object[0], _gameObjectBindInfo);

            var context = gameObject.GetComponent<GameObjectContext>();

            Assert.IsNotNull(context,
                "Expected prefab with name '{0}' to container a component of type 'GameObjectContext'", prefab.name);

            return context.Container;
        }
    }
}

#endif
