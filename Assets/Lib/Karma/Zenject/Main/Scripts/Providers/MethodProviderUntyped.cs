using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class MethodProviderUntyped : ProviderBase
    {
        readonly Func<InjectContext, object> _method;
        readonly Type _returnType;

        public MethodProviderUntyped(Type returnType, Func<InjectContext, object> method)
        {
            _method = method;
            _returnType = returnType;
        }

        public override Type GetInstanceType()
        {
            return _returnType;
        }

        public override object GetInstance(InjectContext context)
        {
            Assert.That(_returnType.DerivesFromOrEqual(context.MemberType));

            var obj = _method(context);

            Assert.That(obj != null, () =>
                "Method provider returned null when looking up type '{0}'. \nObject graph:\n{1}".Fmt(_returnType.Name(), context.GetObjectGraphString()));

            return obj;
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            return Enumerable.Empty<ZenjectResolveException>();
        }
    }
}
