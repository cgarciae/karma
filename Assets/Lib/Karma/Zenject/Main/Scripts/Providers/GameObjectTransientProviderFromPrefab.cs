#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    public class GameObjectTransientProviderFromPrefab : ProviderBase
    {
        readonly Type _concreteType;
        readonly DiContainer _container;
        readonly GameObject _template;

        public GameObjectTransientProviderFromPrefab(Type concreteType, DiContainer container, GameObject template)
        {
            // Don't do this because it might be an interface
            //Assert.That(typeof(T).DerivesFrom<Component>());

            _concreteType = concreteType;
            _container = container;
            _template = template;
        }

        public override Type GetInstanceType()
        {
            return _concreteType;
        }

        public override object GetInstance(InjectContext context)
        {
            Assert.That(_concreteType.DerivesFromOrEqual(context.MemberType));
            return _container.InstantiatePrefabForComponent(_concreteType, _template);
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            return BindingValidator.ValidateObjectGraph(_container, _concreteType, context, null);
        }
    }
}

#endif
