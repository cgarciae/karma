using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public class InstanceProvider : IProvider
    {
        readonly object _instance;
        readonly Type _instanceType;
        readonly LazyInstanceInjector _lazyInjector;

        public InstanceProvider(
            DiContainer container, Type instanceType, object instance)
        {
            _instanceType = instanceType;
            _instance = instance;
            _lazyInjector = container.LazyInstanceInjector;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _instanceType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEmpty(args);
            Assert.IsNotNull(context);

            Assert.That(_instanceType.DerivesFromOrEqual(context.MemberType));

            _lazyInjector.OnInstanceResolved(_instance);

            yield return new List<object>() { _instance };
        }
    }
}
