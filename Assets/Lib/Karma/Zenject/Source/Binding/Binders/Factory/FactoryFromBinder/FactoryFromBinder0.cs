using System;
using System.Collections.Generic;

namespace Zenject
{
    public class FactoryFromBinder<TContract> : FactoryFromBinderBase<TContract>
    {
        public FactoryFromBinder(
            BindInfo bindInfo,
            Type factoryType,
            BindFinalizerWrapper finalizerWrapper)
            : base(bindInfo, factoryType, finalizerWrapper)
        {
        }

        public ConditionBinder FromResolveGetter<TObj>(Func<TObj, TContract> method)
        {
            return FromResolveGetter<TObj>(null, method);
        }

        public ConditionBinder FromResolveGetter<TObj>(
            object subIdentifier, Func<TObj, TContract> method)
        {
            SubFinalizer = CreateFinalizer(
                (container) => new GetterProvider<TObj, TContract>(subIdentifier, method, container));

            return this;
        }

        public ConditionBinder FromMethod(Func<DiContainer, TContract> method)
        {
            SubFinalizer = CreateFinalizer(
                (container) => new MethodProviderWithContainer<TContract>(method));

            return this;
        }

        public ConditionBinder FromInstance(object instance)
        {
            BindingUtil.AssertInstanceDerivesFromOrEqual(instance, AllParentTypes);

            SubFinalizer = CreateFinalizer(
                (container) => new InstanceProvider(ContractType, instance));

            return this;
        }

        public ConditionBinder FromFactory<TSubFactory>()
            where TSubFactory : IFactory<TContract>
        {
            SubFinalizer = CreateFinalizer(
                (container) => new FactoryProvider<TContract, TSubFactory>(container, new List<TypeValuePair>()));

            return this;
        }

        public FactorySubContainerBinder<TContract> FromSubContainerResolve()
        {
            return FromSubContainerResolve(null);
        }

        public FactorySubContainerBinder<TContract> FromSubContainerResolve(object subIdentifier)
        {
            return new FactorySubContainerBinder<TContract>(
                BindInfo, FactoryType, FinalizerWrapper, subIdentifier);
        }

#if !NOT_UNITY3D

        public ConditionBinder FromResource(string resourcePath)
        {
            BindingUtil.AssertDerivesFromUnityObject(ContractType);

            SubFinalizer = CreateFinalizer(
                (container) => new ResourceProvider(resourcePath, ContractType));

            return this;
        }
#endif
    }
}
