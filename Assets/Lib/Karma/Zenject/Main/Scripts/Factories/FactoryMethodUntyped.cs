using System;
using System.Collections.Generic;

namespace Zenject
{
    // Instantiate using a delegate
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryMethodUntyped<TContract> : IFactoryUntyped<TContract>
    {
        readonly DiContainer _container;
        readonly Func<DiContainer, object[], TContract> _method;

        public FactoryMethodUntyped(
            DiContainer container, Func<DiContainer, object[], TContract> method)
        {
            _container = container;
            _method = method;
        }

        public TContract Create(params object[] constructorArgs)
        {
            return _method(_container, constructorArgs);
        }

        public IEnumerable<ZenjectResolveException> Validate(params Type[] extras)
        {
            yield break;
        }
    }
}
