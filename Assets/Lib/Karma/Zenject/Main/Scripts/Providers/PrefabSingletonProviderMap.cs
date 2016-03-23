#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class PrefabSingletonProviderMap
    {
        Dictionary<PrefabSingletonId, PrefabSingletonLazyCreator> _creators = new Dictionary<PrefabSingletonId, PrefabSingletonLazyCreator>();
        DiContainer _container;

        public PrefabSingletonProviderMap(DiContainer container)
        {
            _container = container;
        }

        public IEnumerable<PrefabSingletonLazyCreator> Creators
        {
            get
            {
                return _creators.Values;
            }
        }

        internal void RemoveCreator(PrefabSingletonId id)
        {
            bool success = _creators.Remove(id);
            Assert.That(success);
        }

        PrefabSingletonLazyCreator AddCreator(PrefabSingletonId id)
        {
            PrefabSingletonLazyCreator creator;

            if (!_creators.TryGetValue(id, out creator))
            {
                creator = new PrefabSingletonLazyCreator(_container, this, id);
                _creators.Add(id, creator);
            }

            creator.IncRefCount();
            return creator;
        }

        public ProviderBase CreateProvider(
            string identifier, Type concreteType, GameObject prefab, string resourcePath)
        {
            return new PrefabSingletonProvider(
                _container, concreteType, AddCreator(new PrefabSingletonId(identifier, prefab, resourcePath)));
        }
    }
}
#endif
