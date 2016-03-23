#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using Zenject;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    //No parameters
    public class PrefabFactory<T> : IValidatable
        where T : Component
    {
        [Inject]
        protected readonly DiContainer _container;

        public T Create(GameObject prefab)
        {
            Assert.That(prefab != null,
               "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(T));

            return _container.InstantiatePrefabForComponent<T>(prefab);
        }

        public virtual T Create(string prefabResourceName)
        {
            Assert.That(!string.IsNullOrEmpty(prefabResourceName),
              "Null or empty prefab resource name given to factory create method when instantiating object with type '{0}'.", typeof(T));

            return Create((GameObject)Resources.Load(prefabResourceName));
        }

        public System.Collections.Generic.IEnumerable<ZenjectResolveException> Validate()
        {
            return _container.ValidateObjectGraph<T>();
        }
    }

    // One parameter
    public class PrefabFactory<P1, T> : IValidatable
        where T : Component
    {
        [Inject]
        protected readonly DiContainer _container;

        public virtual T Create(GameObject prefab, P1 param)
        {
            Assert.That(prefab != null,
               "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(T));

            return _container.InstantiatePrefabForComponent<T>(prefab, param);
        }

        public virtual T Create(string prefabResourceName, P1 param)
        {
            Assert.That(!string.IsNullOrEmpty(prefabResourceName),
              "Null or empty prefab resource name given to factory create method when instantiating object with type '{0}'.", typeof(T));

            return Create((GameObject)Resources.Load(prefabResourceName), param);
        }

        public System.Collections.Generic.IEnumerable<ZenjectResolveException> Validate()
        {
            return _container.ValidateObjectGraph<T>(typeof(P1));
        }
    }

    // Two parameters
    public class PrefabFactory<P1, P2, T> : IValidatable
        where T : Component
    {
        [Inject]
        protected readonly DiContainer _container;

        public virtual T Create(GameObject prefab, P1 param, P2 param2)
        {
            Assert.That(prefab != null,
               "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(T));

            return _container.InstantiatePrefabForComponent<T>(prefab, param, param2);
        }

        public virtual T Create(string prefabResourceName, P1 param, P2 param2)
        {
            Assert.That(!string.IsNullOrEmpty(prefabResourceName),
              "Null or empty prefab resource name given to factory create method when instantiating object with type '{0}'.", typeof(T));

            return Create((GameObject)Resources.Load(prefabResourceName), param, param2);
        }

        public System.Collections.Generic.IEnumerable<ZenjectResolveException> Validate()
        {
            return _container.ValidateObjectGraph<T>(typeof(P1), typeof(P2));
        }
    }

    // Three parameters
    public class PrefabFactory<P1, P2, P3, T> : IValidatable
        where T : Component
    {
        [Inject]
        protected readonly DiContainer _container;

        public virtual T Create(GameObject prefab, P1 param, P2 param2, P3 param3)
        {
            Assert.That(prefab != null,
               "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(T));

            return _container.InstantiatePrefabForComponent<T>(prefab, param, param2, param3);
        }

        public virtual T Create(string prefabResourceName, P1 param, P2 param2, P3 param3)
        {
            Assert.That(!string.IsNullOrEmpty(prefabResourceName),
              "Null or empty prefab resource name given to factory create method when instantiating object with type '{0}'.", typeof(T));

            return Create((GameObject)Resources.Load(prefabResourceName), param, param2, param3);
        }

        public System.Collections.Generic.IEnumerable<ZenjectResolveException> Validate()
        {
            return _container.ValidateObjectGraph<T>(typeof(P1), typeof(P2), typeof(P3));
        }
    }

    // Four parameters
    public class PrefabFactory<P1, P2, P3, P4, T> : IValidatable
        where T : Component
    {
        [Inject]
        protected readonly DiContainer _container;

        public virtual T Create(GameObject prefab, P1 param, P2 param2, P3 param3, P4 param4)
        {
            Assert.That(prefab != null,
               "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(T));

            return _container.InstantiatePrefabForComponent<T>(prefab, param, param2, param3, param4);
        }

        public virtual T Create(string prefabResourceName, P1 param, P2 param2, P3 param3, P4 param4)
        {
            Assert.That(!string.IsNullOrEmpty(prefabResourceName),
              "Null or empty prefab resource name given to factory create method when instantiating object with type '{0}'.", typeof(T));

            return Create((GameObject)Resources.Load(prefabResourceName), param, param2, param3, param4);
        }

        public System.Collections.Generic.IEnumerable<ZenjectResolveException> Validate()
        {
            return _container.ValidateObjectGraph<T>(typeof(P1), typeof(P2), typeof(P3), typeof(P4));
        }
    }
}

#endif
