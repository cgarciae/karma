using System;
using ModestTree;
using ModestTree.Util;

#if !ZEN_NOT_UNITY3D
using UnityEngine;
#endif

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class IFactoryUntypedBinder<TContract>
    {
        readonly DiContainer _container;
        readonly string _identifier;

        public IFactoryUntypedBinder(DiContainer container, string identifier)
        {
            _container = container;
            _identifier = identifier;
        }

        public BindingConditionSetter ToInstance(TContract instance)
        {
            return ToMethod((c, args) => instance);
        }

        public BindingConditionSetter ToMethod(
            Func<DiContainer, object[], TContract> method)
        {
            return _container.Bind<IFactoryUntyped<TContract>>()
                .ToMethod(c => new FactoryMethodUntyped<TContract>(c.Container, method));
        }

        public BindingConditionSetter ToCustomFactory<TFactory>()
            where TFactory : IFactoryUntyped<TContract>
        {
            return _container.Bind<IFactoryUntyped<TContract>>(_identifier)
                .ToTransient<TFactory>();
        }

        public BindingConditionSetter ToFactory()
        {
            return _container.Bind<IFactoryUntyped<TContract>>()
                .ToSingle<FactoryUntyped<TContract>>();
        }

        public BindingConditionSetter ToFactory<TConcrete>()
            where TConcrete : TContract
        {
            return _container.Bind<IFactoryUntyped<TContract>>()
                .ToSingle<FactoryUntyped<TContract, TConcrete>>();
        }
    }
}
