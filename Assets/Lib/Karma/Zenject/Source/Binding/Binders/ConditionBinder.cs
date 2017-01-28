using System;
using System.Linq;
using ModestTree;

namespace Zenject
{
    public class ConditionBinder : CopyIntoSubContainersBinder
    {
        public ConditionBinder(BindInfo bindInfo)
            : base(bindInfo)
        {
        }

        public CopyIntoSubContainersBinder When(BindingCondition condition)
        {
            BindInfo.Condition = condition;
            return this;
        }

        public CopyIntoSubContainersBinder WhenInjectedIntoInstance(object instance)
        {
            BindInfo.Condition = r => ReferenceEquals(r.ObjectInstance, instance);
            return this;
        }

        public CopyIntoSubContainersBinder WhenInjectedInto(params Type[] targets)
        {
            BindInfo.Condition = r => targets.Where(x => r.ObjectType != null && r.ObjectType.DerivesFromOrEqual(x)).Any();
            return this;
        }

        public CopyIntoSubContainersBinder WhenInjectedInto<T>()
        {
            BindInfo.Condition = r => r.ObjectType != null && r.ObjectType.DerivesFromOrEqual(typeof(T));
            return this;
        }

        public CopyIntoSubContainersBinder WhenNotInjectedInto<T>()
        {
            BindInfo.Condition = r => r.ObjectType == null || !r.ObjectType.DerivesFromOrEqual(typeof(T));
            return this;
        }
    }
}
