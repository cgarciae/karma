using System;
using System.Collections.Generic;
using ModestTree;
using System.Linq;

namespace Zenject
{
    public class FactoryFromBinder<TParam1, TParam2, TParam3, TContract> : FactoryFromBinderWithParams<TContract>
    {
        public FactoryFromBinder(BindInfo bindInfo, Type factoryType, BindFinalizerWrapper finalizerWrapper)
            : base(bindInfo, factoryType, finalizerWrapper)
        {
        }

        public ConditionBinder FromMethod(Func<DiContainer, TParam1, TParam2, TParam3, TContract> method)
        {
            SubFinalizer = CreateFinalizer(
                (container) => new MethodProviderWithContainer<TParam1, TParam2, TParam3, TContract>(method));

            return this;
        }

        public ConditionBinder FromFactory<TSubFactory>()
            where TSubFactory : IFactory<TParam1, TParam2, TParam3, TContract>
        {
            SubFinalizer = CreateFinalizer(
                (container) => new FactoryProvider<TParam1, TParam2, TParam3, TContract, TSubFactory>(container, new List<TypeValuePair>()));

            return this;
        }

        public FactorySubContainerBinder<TParam1, TParam2, TParam3, TContract> FromSubContainerResolve()
        {
            return FromSubContainerResolve(null);
        }

        public FactorySubContainerBinder<TParam1, TParam2, TParam3, TContract> FromSubContainerResolve(object subIdentifier)
        {
            return new FactorySubContainerBinder<TParam1, TParam2, TParam3, TContract>(
                BindInfo, FactoryType, FinalizerWrapper, subIdentifier);
        }
    }
}

