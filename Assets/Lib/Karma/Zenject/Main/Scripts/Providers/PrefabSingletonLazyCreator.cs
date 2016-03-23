#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using ModestTree;
using System.Linq;
using UnityEngine;

namespace Zenject
{
    public class PrefabSingletonLazyCreator
    {
        readonly DiContainer _container;
        readonly PrefabSingletonProviderMap _owner;
        readonly PrefabSingletonId _id;

        int _referenceCount;
        GameObject _rootObj;

        public PrefabSingletonLazyCreator(
            DiContainer container, PrefabSingletonProviderMap owner,
            PrefabSingletonId id)
        {
            _container = container;
            _owner = owner;
            _id = id;

            Assert.That(id.Prefab != null || id.ResourcePath != null);
        }

        public GameObject Prefab
        {
            get
            {
                return _id.Prefab;
            }
        }

        public GameObject RootObject
        {
            get
            {
                return _rootObj;
            }
        }

        public void IncRefCount()
        {
            _referenceCount += 1;
        }

        public void DecRefCount()
        {
            _referenceCount -= 1;

            if (_referenceCount <= 0)
            {
                _owner.RemoveCreator(_id);
            }
        }

        public IEnumerable<Type> GetAllComponentTypes()
        {
            if (_id.Prefab == null)
            {
                // We don't know this when using resource path
                return Enumerable.Empty<Type>();
            }

            return _id.Prefab.GetComponentsInChildren<Component>(true).Where(x => x != null).Select(x => x.GetType());
        }

        public bool ContainsComponent(Type type)
        {
            if (_id.Prefab == null)
            {
                // We don't know this when using resource path, so just assume true
                return true;
            }

            return !_id.Prefab.GetComponentsInChildren(type, true).IsEmpty();
        }

        public object GetComponent(Type componentType, InjectContext context)
        {
            if (_rootObj == null)
            {
                Assert.That((_id.ResourcePath == null && _id.Prefab != null) || (_id.ResourcePath != null && _id.Prefab == null));

                var prefab = _id.Prefab;

                if (prefab == null)
                {
                    prefab = (GameObject)Resources.Load(_id.ResourcePath);
                    Assert.IsNotNull(prefab, "Could not find prefab at resource path '{0}'", _id.ResourcePath);
                }

                _rootObj = (GameObject)GameObject.Instantiate(prefab);

                // Default parent to comp root
                _rootObj.transform.SetParent(_container.Resolve<CompositionRoot>().transform, false);
                _rootObj.SetActive(true);

                _container.InjectGameObject(_rootObj, true, false, new object[0], context);
            }

            var component = _rootObj.GetComponentInChildren(componentType);

            if (component == null)
            {
                throw new ZenjectResolveException(
                    "Could not find component with type '{0}' in given singleton prefab".Fmt(componentType));
            }

            return component;
        }
    }
}

#endif
