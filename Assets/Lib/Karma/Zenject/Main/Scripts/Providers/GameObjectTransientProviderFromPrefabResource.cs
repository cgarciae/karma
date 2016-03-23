#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    public class GameObjectTransientProviderFromPrefabResource : ProviderBase
    {
        readonly DiContainer _container;
        readonly string _resourcePath;
        readonly Type _concreteType;

        public GameObjectTransientProviderFromPrefabResource(
            Type concreteType, DiContainer container, string resourcePath)
        {
            // Don't do this because it might be an interface
            //Assert.That(_concreteType.DerivesFrom<Component>());

            _concreteType = concreteType;
            _container = container;
            _resourcePath = resourcePath;
        }

        public override Type GetInstanceType()
        {
            return _concreteType;
        }

        public override object GetInstance(InjectContext context)
        {
            Assert.That(_concreteType.DerivesFromOrEqual(context.MemberType));

            return _container.InstantiatePrefabResourceForComponent(_concreteType, _resourcePath);
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            return BindingValidator.ValidateObjectGraph(_container, _concreteType, context, null);
        }
    }
}

#endif

