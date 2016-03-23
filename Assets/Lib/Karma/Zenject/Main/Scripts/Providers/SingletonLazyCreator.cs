using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    internal class SingletonLazyCreator
    {
        readonly DiContainer _container;
        readonly SingletonProviderMap _owner;
        readonly Func<InjectContext, object> _createMethod;
        readonly SingletonId _id;

        int _referenceCount;
        object _instance;
        bool _hasInstance;

        public SingletonLazyCreator(
            DiContainer container, SingletonProviderMap owner,
            SingletonId id, Func<InjectContext, object> createMethod)
        {
            _container = container;
            _owner = owner;
            _id = id;
            _createMethod = createMethod;
        }

        public SingletonLazyCreator(
            DiContainer container, SingletonProviderMap owner, SingletonId id)
            : this(container, owner, id, null)
        {
        }

        public SingletonId Id
        {
            get
            {
                return _id;
            }
        }

        public bool HasCustomCreateMethod
        {
            get
            {
                return _createMethod != null;
            }
        }

        public void IncRefCount()
        {
            _referenceCount += 1;
        }

        public void DecRefCount()
        {
            _referenceCount -= 1;

            if (_referenceCount <= 0)
            {
                _owner.RemoveCreator(_id);
            }
        }

        public void SetInstance(object instance)
        {
            Assert.IsNull(_instance);
            Assert.That(instance != null || _container.AllowNullBindings);

            _instance = instance;
            // We need this flag for validation
            _hasInstance = true;
        }

        public bool HasInstance()
        {
            if (_hasInstance)
            {
                Assert.That(_container.AllowNullBindings || _instance != null);
            }

            return _hasInstance;
        }

        public Type GetInstanceType()
        {
            return _id.Type;
        }

        public object GetInstance(InjectContext context)
        {
            if (!_hasInstance)
            {
                if (_createMethod != null)
                {
                    _instance = _createMethod(context);

                    if (_instance == null)
                    {
                        throw new ZenjectResolveException(
                            "Unable to instantiate type '{0}' in SingletonLazyCreator".Fmt(context.MemberType));
                    }

                    _hasInstance = true;
                }
                else
                {
                    var concreteType = GetTypeToInstantiate(context.MemberType);

                    bool autoInject = false;
                    _instance = _container.InstantiateExplicit(
                        concreteType, new List<TypeValuePair>(), context, _id.Identifier, autoInject);

                    Assert.IsNotNull(_instance);

                    _hasInstance = true;

                    // Inject after we've instantiated and set the _hasInstance flag so that we can support circular dependencies
                    // as PostInject or field parameters
                    _container.Inject(_instance);
                }
            }

            return _instance;
        }

        Type GetTypeToInstantiate(Type contractType)
        {
            if (_id.Type.IsOpenGenericType())
            {
                Assert.That(!contractType.IsAbstract);
                Assert.That(contractType.GetGenericTypeDefinition() == _id.Type);
                return contractType;
            }

            Assert.That(_id.Type.DerivesFromOrEqual(contractType));
            return _id.Type;
        }
    }
}
