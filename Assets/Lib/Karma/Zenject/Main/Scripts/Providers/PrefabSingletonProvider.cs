#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    // NOTE: we need the provider seperate from the creator because
    // if we return the same provider multiple times then the condition
    // will get over-written
    internal class PrefabSingletonProvider : ProviderBase
    {
        PrefabSingletonLazyCreator _creator;
        DiContainer _container;
        Type _concreteType;

        public PrefabSingletonProvider(
            DiContainer container, Type concreteType, PrefabSingletonLazyCreator creator)
        {
            _creator = creator;
            _container = container;
            _concreteType = concreteType;
        }

        public override void Dispose()
        {
            _creator.DecRefCount();
        }

        public override Type GetInstanceType()
        {
            return _concreteType;
        }

        public override object GetInstance(InjectContext context)
        {
            return _creator.GetComponent(_concreteType, context);
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            if (!_creator.ContainsComponent(_concreteType))
            {
                yield return new ZenjectResolveException(
                    "Could not find component of type '{0}' in prefab with name '{1}' \nObject graph:\n{2}"
                    .Fmt(_concreteType.Name(), _creator.Prefab.name, context.GetObjectGraphString()));
                yield break;
            }

            foreach (var err in BindingValidator.ValidateObjectGraph(_container, _concreteType, context, null))
            {
                yield return err;
            }
        }
    }
}

#endif
