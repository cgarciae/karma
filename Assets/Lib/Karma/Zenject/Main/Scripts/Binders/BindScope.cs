using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModestTree;

namespace Zenject
{
    // This class is meant to be used the following way:
    //
    //  using (var scope = _container.CreateScope())
    //  {
    //      scope.Bind(playerWrapper);
    //      ...
    //      ...
    //      var bar = _container.Resolve<Foo>();
    //  }
    public class BindScope : IDisposable
    {
        DiContainer _container;
        List<ProviderBase> _scopedProviders = new List<ProviderBase>();
        SingletonProviderMap _singletonMap;

        internal BindScope(DiContainer container, SingletonProviderMap singletonMap)
        {
            _container = container;
            _singletonMap = singletonMap;
        }

        public BinderUntyped Bind(Type contractType)
        {
            return Bind(contractType, null);
        }

        public BinderUntyped Bind(Type contractType, string identifier)
        {
            return new CustomScopeUntypedBinder(this, contractType, identifier, _container, _singletonMap);
        }

        public BinderGeneric<TContract> Bind<TContract>()
        {
            return Bind<TContract>((string)null);
        }

        public BindingConditionSetter BindInstance<TContract>(TContract obj)
        {
            return Bind<TContract>((string)null).ToInstance(obj);
        }

        public BinderGeneric<TContract> Bind<TContract>(string identifier)
        {
            return new CustomScopeBinder<TContract>(this, identifier, _container, _singletonMap);
        }

        // This method is just an alternative way of binding to a dependency of
        // a specific class with a specific identifier
        public void BindIdentifier<TClass, TParam>(string identifier, TParam value)
        {
            Bind(typeof(TParam), identifier).ToInstance(value).WhenInjectedInto<TClass>();

            // We'd pref to do this instead but it fails on web player because Mono
            // seems to interpret TDerived : TBase to require that TDerived != TBase?
            //Bind<TParam>().To(value).WhenInjectedInto<TClass>().As(identifier);
        }

        void AddProvider(ProviderBase provider)
        {
            Assert.That(!_scopedProviders.Contains(provider));
            _scopedProviders.Add(provider);
        }

        public void Dispose()
        {
            foreach (var provider in _scopedProviders)
            {
                _container.UnregisterProvider(provider);
            }
        }

        class CustomScopeBinder<TContract> : BinderGeneric<TContract>
        {
            BindScope _owner;

            public CustomScopeBinder(
                BindScope owner, string identifier,
                DiContainer container, SingletonProviderMap singletonMap)
                : base(container, identifier, singletonMap)
            {
                _owner = owner;
            }

            public override BindingConditionSetter ToProvider(ProviderBase provider)
            {
                _owner.AddProvider(provider);
                return base.ToProvider(provider);
            }
        }

        class CustomScopeUntypedBinder : BinderUntyped
        {
            BindScope _owner;

            public CustomScopeUntypedBinder(
                BindScope owner, Type contractType, string identifier,
                DiContainer container, SingletonProviderMap singletonMap)
                : base(container, contractType, identifier, singletonMap)
            {
                _owner = owner;
            }

            public override BindingConditionSetter ToProvider(ProviderBase provider)
            {
                _owner.AddProvider(provider);
                return base.ToProvider(provider);
            }
        }
    }
}
