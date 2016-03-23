using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class MethodProvider<T> : ProviderBase
    {
        readonly Func<InjectContext, T> _method;

        public MethodProvider(Func<InjectContext, T> method)
        {
            _method = method;
        }

        public override Type GetInstanceType()
        {
            return typeof(T);
        }

        public override object GetInstance(InjectContext context)
        {
            Assert.That(typeof(T).DerivesFromOrEqual(context.MemberType));
            var obj = _method(context);

            Assert.That(obj != null, () =>
                "Method provider returned null when looking up type '{0}'. \nObject graph:\n{1}".Fmt(typeof(T).Name(), context.GetObjectGraphString()));

            return obj;
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            return Enumerable.Empty<ZenjectResolveException>();
        }
    }
}

