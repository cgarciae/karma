using System;

namespace Zenject
{
    public class FromBinderNonGeneric : FromBinder
    {
        public FromBinderNonGeneric(
            BindInfo bindInfo,
            BindFinalizerWrapper finalizerWrapper)
            : base(bindInfo, finalizerWrapper)
        {
        }

        public ScopeArgBinder FromFactory<TConcrete, TFactory>()
            where TFactory : IFactory<TConcrete>
        {
            return FromFactoryBase<TConcrete, TFactory>();
        }

        public ScopeArgBinder FromMethod<TConcrete>(Func<InjectContext, TConcrete> method)
        {
            return FromMethodBase<TConcrete>(method);
        }

        public ScopeBinder FromResolveGetter<TObj, TContract>(Func<TObj, TContract> method)
        {
            return FromResolveGetter<TObj, TContract>(null, method);
        }

        public ScopeBinder FromResolveGetter<TObj, TContract>(object identifier, Func<TObj, TContract> method)
        {
            return FromResolveGetterBase<TObj, TContract>(identifier, method);
        }

        public ScopeBinder FromInstance(object instance)
        {
            return FromInstance(instance, false);
        }

        public ScopeBinder FromInstance(object instance, bool allowNull)
        {
            return FromInstanceBase(instance, allowNull);
        }
    }
}

