using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;

#if !ZEN_NOT_UNITY3D
using UnityEngine;
#endif

namespace Zenject
{
    public interface IResolver
    {
        IList ResolveAll(InjectContext context);

        List<Type> ResolveTypeAll(InjectContext context);

        object Resolve(InjectContext context);

        void InjectExplicit(
            object injectable, IEnumerable<TypeValuePair> extraArgs,
            bool shouldUseAll, ZenjectTypeInfo typeInfo, InjectContext context, string concreteIdentifier);

#if !ZEN_NOT_UNITY3D
        // Inject dependencies into child game objects
        void InjectGameObject(
            GameObject gameObject, bool recursive, bool includeInactive);

        void InjectGameObject(
            GameObject gameObject, bool recursive);

        void InjectGameObject(
            GameObject gameObject);

        void InjectGameObject(
            GameObject gameObject,
            bool recursive, bool includeInactive, IEnumerable<object> extraArgs);

        void InjectGameObject(
            GameObject gameObject,
            bool recursive, bool includeInactive, IEnumerable<object> extraArgs, InjectContext context);
#endif

        void Inject(object injectable);
        void Inject(object injectable, IEnumerable<object> additional);
        void Inject(object injectable, IEnumerable<object> additional, bool shouldUseAll);
        void Inject(
            object injectable, IEnumerable<object> additional, bool shouldUseAll, InjectContext context);
        void Inject(
            object injectable,
            IEnumerable<object> additional, bool shouldUseAll, InjectContext context, ZenjectTypeInfo typeInfo);

        void InjectExplicit(object injectable, List<TypeValuePair> additional);
        void InjectExplicit(object injectable, List<TypeValuePair> additional, InjectContext context);

        List<Type> ResolveTypeAll(Type type);

        TContract Resolve<TContract>();
        TContract Resolve<TContract>(string identifier);

        TContract TryResolve<TContract>()
            where TContract : class;
        TContract TryResolve<TContract>(string identifier)
            where TContract : class;

        object TryResolve(Type contractType);
        object TryResolve(Type contractType, string identifier);

        object Resolve(Type contractType);
        object Resolve(Type contractType, string identifier);

        TContract Resolve<TContract>(InjectContext context);

        List<TContract> ResolveAll<TContract>();
        List<TContract> ResolveAll<TContract>(bool optional);
        List<TContract> ResolveAll<TContract>(string identifier);
        List<TContract> ResolveAll<TContract>(string identifier, bool optional);
        List<TContract> ResolveAll<TContract>(InjectContext context);

        IList ResolveAll(Type contractType);
        IList ResolveAll(Type contractType, string identifier);
        IList ResolveAll(Type contractType, bool optional);
        IList ResolveAll(Type contractType, string identifier, bool optional);
    }
}
