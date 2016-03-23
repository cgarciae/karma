using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class InstanceProvider : ProviderBase
    {
        readonly object _instance;
        readonly Type _instanceType;

        public InstanceProvider(Type instanceType, object instance)
        {
            Assert.That(instance == null || instance.GetType().DerivesFromOrEqual(instanceType));

            _instance = instance;
            _instanceType = instanceType;
        }

        public override Type GetInstanceType()
        {
            return _instanceType;
        }

        public override object GetInstance(InjectContext context)
        {
            // _instance == null during validation sometimes
            Assert.That(_instance == null || _instance.GetType().DerivesFromOrEqual(context.MemberType));
            return _instance;
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            return Enumerable.Empty<ZenjectResolveException>();
        }
    }
}
