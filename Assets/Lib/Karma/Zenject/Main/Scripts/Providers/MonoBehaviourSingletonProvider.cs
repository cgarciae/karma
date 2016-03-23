#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    public class MonoBehaviourSingletonProvider : ProviderBase
    {
        Component _instance;
        DiContainer _container;
        Type _componentType;
        GameObject _gameObject;

        public MonoBehaviourSingletonProvider(
            Type componentType, DiContainer container, GameObject gameObject)
        {
            Assert.That(componentType.DerivesFrom<Component>());

            _gameObject = gameObject;
            _componentType = componentType;
            _container = container;
        }

        public override Type GetInstanceType()
        {
            return _componentType;
        }

        public override object GetInstance(InjectContext context)
        {
            Assert.That(_componentType.DerivesFromOrEqual(context.MemberType));

            if (_instance == null)
            {
                Assert.That(!_container.AllowNullBindings,
                    "Tried to instantiate a MonoBehaviour with type '{0}' during validation. Object graph: {1}", _componentType, context.GetObjectGraphString());

                _instance = _gameObject.AddComponent(_componentType);
                Assert.That(_instance != null);

                _container.Inject(_instance);
            }

            return _instance;
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            return BindingValidator.ValidateObjectGraph(_container, _componentType, context, null);
        }
    }
}

#endif
