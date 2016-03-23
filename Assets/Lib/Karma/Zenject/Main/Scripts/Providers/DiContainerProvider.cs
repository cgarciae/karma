using System;
using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    // This provider can be used to create nested containers
    [System.Diagnostics.DebuggerStepThrough]
    public class DiContainerProvider : ProviderBase
    {
        DiContainer _container;

        public DiContainerProvider(DiContainer container)
        {
            _container = container;
        }

        public override Type GetInstanceType()
        {
            return null;
        }

        public override object GetInstance(InjectContext context)
        {
            return _container.Resolve(context);
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            return _container.ValidateResolve(context);
        }
    }
}
