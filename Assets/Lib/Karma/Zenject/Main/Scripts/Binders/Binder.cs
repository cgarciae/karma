using System;
using ModestTree;
#if !ZEN_NOT_UNITY3D
using UnityEngine;
#endif

namespace Zenject
{
    public class Binder
    {
        readonly Type _contractType;
        readonly DiContainer _container;
        readonly string _bindIdentifier;
        readonly SingletonProviderMap _singletonMap;

        public Binder(
            DiContainer container,
            Type contractType,
            string bindIdentifier,
            SingletonProviderMap singletonMap)
        {
            _container = container;
            _contractType = contractType;
            _bindIdentifier = bindIdentifier;
            _singletonMap = singletonMap;
        }

        public Type ContractType
        {
            get
            {
                return _contractType;
            }
        }

        public BindingConditionSetter ToTransient()
        {
#if !ZEN_NOT_UNITY3D
            if (_contractType.DerivesFrom(typeof(Component)))
            {
                throw new ZenjectBindException(
                    "Should not use ToTransient for Monobehaviours (when binding type '{0}'), you probably want either ToLookup or ToTransientFromPrefab"
                    .Fmt(_contractType.Name()));
            }
#endif

            return ToProvider(new TransientProvider(_container, _contractType));
        }

        public BindingConditionSetter ToTransient(Type concreteType)
        {
#if !ZEN_NOT_UNITY3D
            if (concreteType.DerivesFrom(typeof(Component)))
            {
                throw new ZenjectBindException(
                    "Should not use ToTransient for Monobehaviours (when binding type '{0}'), you probably want either ToLookup or ToTransientFromPrefab"
                    .Fmt(concreteType.Name()));
            }
#endif

            return ToProvider(new TransientProvider(_container, concreteType));
        }

        public BindingConditionSetter ToSingle()
        {
            return ToSingle((string)null);
        }

        public BindingConditionSetter ToSingle(string concreteIdentifier)
        {
#if !ZEN_NOT_UNITY3D
            if (_contractType.DerivesFrom(typeof(Component)))
            {
                throw new ZenjectBindException(
                    "Should not use ToSingle for Monobehaviours (when binding type '{0}'), you probably want either ToLookup or ToSinglePrefab or ToSingleGameObject"
                    .Fmt(_contractType.Name()));
            }
#endif

            return ToProvider(_singletonMap.CreateProviderFromType(concreteIdentifier, _contractType));
        }

        public BindingConditionSetter ToSingle(Type concreteType)
        {
            return ToSingle(null, concreteType);
        }

        public BindingConditionSetter ToSingle(string concreteIdentifier, Type concreteType)
        {
            if (!concreteType.DerivesFromOrEqual(_contractType))
            {
                throw new ZenjectBindException(
                    "Invalid type given during bind command.  Expected type '{0}' to derive from type '{1}'".Fmt(concreteType.Name(), _contractType.Name()));
            }

            return ToProvider(_singletonMap.CreateProviderFromType(concreteIdentifier, concreteType));
        }

        public BindingConditionSetter ToSingle(Type concreteType, string concreteIdentifier)
        {
            if (!concreteType.DerivesFromOrEqual(_contractType))
            {
                throw new ZenjectBindException(
                    "Invalid type given during bind command.  Expected type '{0}' to derive from type '{1}'".Fmt(concreteType.Name(), _contractType.Name()));
            }

#if !ZEN_NOT_UNITY3D
            if (concreteType.DerivesFrom(typeof(Component)))
            {
                throw new ZenjectBindException(
                    "Should not use ToSingle for Monobehaviours (when binding type '{0}' to '{1}'), you probably want either ToLookup or ToSinglePrefab or ToSinglePrefabResource or ToSingleGameObject"
                    .Fmt(_contractType.Name(), concreteType.Name()));
            }
#endif

            return ToProvider(_singletonMap.CreateProviderFromType(concreteIdentifier, concreteType));
        }

        public virtual BindingConditionSetter ToProvider(ProviderBase provider)
        {
            _container.RegisterProvider(
                provider, new BindingId(_contractType, _bindIdentifier));

            if (_contractType.IsValueType)
            {
                var nullableType = typeof(Nullable<>).MakeGenericType(_contractType);

                // Also bind to nullable primitives
                // this is useful so that we can have optional primitive dependencies
                _container.RegisterProvider(
                    provider, new BindingId(nullableType, _bindIdentifier));
            }

            return new BindingConditionSetter(provider);
        }

#if !ZEN_NOT_UNITY3D

        // Note that concreteType here could be an interface as well
        public BindingConditionSetter ToSinglePrefab(
            Type concreteType, string concreteIdentifier, GameObject prefab)
        {
            if (!concreteType.DerivesFromOrEqual(_contractType))
            {
                throw new ZenjectBindException(
                    "Invalid type given during bind command.  Expected type '{0}' to derive from type '{1}'".Fmt(concreteType.Name(), _contractType.Name()));
            }

            if (ZenUtil.IsNull(prefab))
            {
                throw new ZenjectBindException(
                    "Received null prefab while binding type '{0}'".Fmt(concreteType.Name()));
            }

            var prefabSingletonMap = _container.Resolve<PrefabSingletonProviderMap>();
            return ToProvider(
                prefabSingletonMap.CreateProvider(concreteIdentifier, concreteType, prefab, null));
        }

        public BindingConditionSetter ToTransientPrefab(Type concreteType, GameObject prefab)
        {
            if (!concreteType.DerivesFromOrEqual(_contractType))
            {
                throw new ZenjectBindException(
                    "Invalid type given during bind command.  Expected type '{0}' to derive from type '{1}'".Fmt(concreteType.Name(), _contractType.Name()));
            }

            // We have to cast to object otherwise we get SecurityExceptions when this function is run outside of unity
            if (ZenUtil.IsNull(prefab))
            {
                throw new ZenjectBindException("Received null prefab while binding type '{0}'".Fmt(concreteType.Name()));
            }

            return ToProvider(new GameObjectTransientProviderFromPrefab(concreteType, _container, prefab));
        }

        public BindingConditionSetter ToSingleGameObject()
        {
            return ToSingleGameObject(_contractType.Name());
        }

        // Creates a new game object and adds the given type as a new component on it
        // NOTE! The string given here is just a name and not a singleton identifier
        public BindingConditionSetter ToSingleGameObject(string name)
        {
            if (!_contractType.IsSubclassOf(typeof(Component)))
            {
                throw new ZenjectBindException("Expected UnityEngine.Component derived type when binding type '{0}'".Fmt(_contractType.Name()));
            }

            return ToProvider(new GameObjectSingletonProvider(_contractType, _container, name));
        }

        // Creates a new game object and adds the given type as a new component on it
        // NOTE! The string given here is just a name and not a singleton identifier
        public BindingConditionSetter ToSingleGameObject(Type concreteType, string name)
        {
            if (!concreteType.DerivesFromOrEqual(_contractType))
            {
                throw new ZenjectBindException(
                    "Invalid type given during bind command.  Expected type '{0}' to derive from type '{1}'".Fmt(concreteType.Name(), _contractType.Name()));
            }

            return ToProvider(new GameObjectSingletonProvider(concreteType, _container, name));
        }

        public BindingConditionSetter ToTransientPrefabResource(string resourcePath)
        {
            return ToTransientPrefabResource(_contractType, resourcePath);
        }

        public BindingConditionSetter ToTransientPrefabResource(Type concreteType, string resourcePath)
        {
            Assert.IsNotNull(resourcePath);
            return ToProvider(new GameObjectTransientProviderFromPrefabResource(concreteType, _container, resourcePath));
        }

        public BindingConditionSetter ToSinglePrefabResource(Type concreteType, string concreteIdentifier, string resourcePath)
        {
            Assert.That(concreteType.DerivesFromOrEqual(_contractType));
            Assert.IsNotNull(resourcePath);

            var prefabSingletonMap = _container.Resolve<PrefabSingletonProviderMap>();
            return ToProvider(
                prefabSingletonMap.CreateProvider(concreteIdentifier, concreteType, null, resourcePath));
        }

        public BindingConditionSetter ToSinglePrefabResource(string resourcePath)
        {
            return ToSinglePrefabResource(null, resourcePath);
        }

        public BindingConditionSetter ToSinglePrefabResource(string identifier, string resourcePath)
        {
            return ToSinglePrefabResource(_contractType, identifier, resourcePath);
        }

        public BindingConditionSetter ToTransientPrefab(GameObject prefab)
        {
            return ToTransientPrefab(_contractType, prefab);
        }

        public BindingConditionSetter ToSinglePrefab(GameObject prefab)
        {
            return ToSinglePrefab(null, prefab);
        }

        public BindingConditionSetter ToSinglePrefab(string identifier, GameObject prefab)
        {
            return ToSinglePrefab(_contractType, identifier, prefab);
        }

#endif
        protected BindingConditionSetter ToSingleMethodBase<TConcrete>(string concreteIdentifier, Func<InjectContext, TConcrete> method)
        {
            return ToProvider(_singletonMap.CreateProviderFromMethod(concreteIdentifier, method));
        }

        protected BindingConditionSetter ToMethodBase<T>(Func<InjectContext, T> method)
        {
            if (!typeof(T).DerivesFromOrEqual(_contractType))
            {
                throw new ZenjectBindException(
                    "Invalid type given during bind command.  Expected type '{0}' to derive from type '{1}'".Fmt(typeof(T), _contractType.Name()));
            }

            return ToProvider(new MethodProvider<T>(method));
        }

        protected BindingConditionSetter ToLookupBase<TConcrete>(string identifier)
        {
            return ToMethodBase<TConcrete>((ctx) => ctx.Container.Resolve<TConcrete>(
                new InjectContext(
                    ctx.Container, typeof(TConcrete), identifier,
                    false, ctx.ObjectType, ctx.ObjectInstance, ctx.MemberName, ctx, null, ctx.FallBackValue)));
        }

        protected BindingConditionSetter ToGetterBase<TObj, TResult>(string identifier, Func<TObj, TResult> method)
        {
            return ToMethodBase((ctx) => method(ctx.Container.Resolve<TObj>(
                new InjectContext(
                    ctx.Container, typeof(TObj), identifier,
                    false, ctx.ObjectType, ctx.ObjectInstance, ctx.MemberName, ctx, null, ctx.FallBackValue))));
        }

        public BindingConditionSetter ToInstance(Type concreteType, object instance)
        {
            if (ZenUtil.IsNull(instance) && !_container.AllowNullBindings)
            {
                string message;

                if (_contractType == concreteType)
                {
                    message = "Received null instance during Bind command with type '{0}'".Fmt(_contractType.Name());
                }
                else
                {
                    message =
                        "Received null instance during Bind command when binding type '{0}' to '{1}'".Fmt(_contractType.Name(), concreteType.Name());
                }

                throw new ZenjectBindException(message);
            }

            if (!ZenUtil.IsNull(instance) && !instance.GetType().DerivesFromOrEqual(_contractType))
            {
                throw new ZenjectBindException(
                    "Invalid type given during bind command.  Expected type '{0}' to derive from type '{1}'".Fmt(concreteType.Name(), _contractType.Name()));
            }

            return ToProvider(new InstanceProvider(concreteType, instance));
        }

        protected BindingConditionSetter ToSingleInstance(Type concreteType, string concreteIdentifier, object instance)
        {
            if (!concreteType.DerivesFromOrEqual(_contractType))
            {
                throw new ZenjectBindException(
                    "Invalid type given during bind command.  Expected type '{0}' to derive from type '{1}'".Fmt(concreteType.Name(), _contractType.Name()));
            }

            if (ZenUtil.IsNull(instance) && !_container.AllowNullBindings)
            {
                string message;

                if (_contractType == concreteType)
                {
                    message = "Received null singleton instance during Bind command with type '{0}'".Fmt(_contractType.Name());
                }
                else
                {
                    message =
                        "Received null singleton instance during Bind command when binding type '{0}' to '{1}'".Fmt(_contractType.Name(), concreteType.Name());
                }

                throw new ZenjectBindException(message);
            }

            return ToProvider(_singletonMap.CreateProviderFromInstance(concreteIdentifier, concreteType, instance));
        }

#if !ZEN_NOT_UNITY3D

        protected BindingConditionSetter ToSingleMonoBehaviourBase<TConcrete>(GameObject gameObject)
        {
            return ToProvider(new MonoBehaviourSingletonProvider(typeof(TConcrete), _container, gameObject));
        }

        public BindingConditionSetter ToResource(string resourcePath)
        {
            return ToResource(_contractType, resourcePath);
        }

        public BindingConditionSetter ToResource(Type concreteType, string resourcePath)
        {
            if (!concreteType.DerivesFromOrEqual(_contractType))
            {
                throw new ZenjectBindException(
                    "Invalid type given during bind command.  Expected type '{0}' to derive from type '{1}'".Fmt(concreteType.Name(), _contractType.Name()));
            }

            return ToProvider(new ResourceProvider(concreteType, resourcePath));
        }
#endif
    }
}
