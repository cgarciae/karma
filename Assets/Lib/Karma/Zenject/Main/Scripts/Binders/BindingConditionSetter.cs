using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    public delegate bool BindingCondition(InjectContext c);

    [System.Diagnostics.DebuggerStepThrough]
    public class BindingConditionSetter
    {
        readonly ProviderBase _provider;

        public BindingConditionSetter(ProviderBase provider)
        {
            _provider = provider;
        }

        public void When(BindingCondition condition)
        {
            _provider.Condition = condition;
        }

        public void WhenInjectedIntoInstance(object instance)
        {
            _provider.Condition = r => ReferenceEquals(r.ObjectInstance, instance);
        }

        public void WhenInjectedInto(params Type[] targets)
        {
            _provider.Condition = r => targets.Where(x => r.ObjectType != null && r.ObjectType.DerivesFromOrEqual(x)).Any();
        }

        public void WhenInjectedInto<T>()
        {
            _provider.Condition = r => r.ObjectType != null && r.ObjectType.DerivesFromOrEqual(typeof(T));
        }

        public void WhenNotInjectedInto<T>()
        {
            _provider.Condition = r => r.ObjectType == null || !r.ObjectType.DerivesFromOrEqual(typeof(T));
        }
    }
}
