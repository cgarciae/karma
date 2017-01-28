#if !NOT_UNITY3D

using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    public class PrefabInstantiator : IPrefabInstantiator
    {
        readonly IPrefabProvider _prefabProvider;
        readonly DiContainer _container;
        readonly List<TypeValuePair> _extraArguments;
        readonly GameObjectCreationParameters _gameObjectBindInfo;

        public PrefabInstantiator(
            DiContainer container,
            GameObjectCreationParameters gameObjectBindInfo,
            List<TypeValuePair> extraArguments,
            IPrefabProvider prefabProvider)
        {
            _prefabProvider = prefabProvider;
            _extraArguments = extraArguments;
            _container = container;
            _gameObjectBindInfo = gameObjectBindInfo;
        }

        public GameObjectCreationParameters GameObjectCreationParameters
        {
            get
            {
                return _gameObjectBindInfo;
            }
        }

        public List<TypeValuePair> ExtraArguments
        {
            get
            {
                return _extraArguments;
            }
        }

        public UnityEngine.Object GetPrefab()
        {
            return _prefabProvider.GetPrefab();
        }

        public IEnumerator<GameObject> Instantiate(List<TypeValuePair> args)
        {
            var gameObject = _container.CreateAndParentPrefab(GetPrefab(), _gameObjectBindInfo);
            Assert.IsNotNull(gameObject);

            // Return it before inject so we can do circular dependencies
            yield return gameObject;

            _container.InjectGameObjectExplicit(
                gameObject, true, _extraArguments.Concat(args).ToList());
        }
    }
}

#endif
