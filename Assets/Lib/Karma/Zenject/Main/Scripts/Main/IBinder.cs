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
    public interface IBinder
    {
        bool Unbind<TContract>(string identifier);

        bool HasBinding(InjectContext context);

        BinderUntyped Bind(Type contractType, string identifier);
        BinderGeneric<TContract> Bind<TContract>(string identifier);

        BinderGeneric<TContract> Rebind<TContract>();

        IFactoryBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindIFactory<TParam1, TParam2, TParam3, TParam4, TContract>(string identifier);
        IFactoryBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindIFactory<TParam1, TParam2, TParam3, TParam4, TContract>();

        IFactoryBinder<TParam1, TParam2, TParam3, TContract> BindIFactory<TParam1, TParam2, TParam3, TContract>(string identifier);
        IFactoryBinder<TParam1, TParam2, TParam3, TContract> BindIFactory<TParam1, TParam2, TParam3, TContract>();

        IFactoryBinder<TParam1, TParam2, TContract> BindIFactory<TParam1, TParam2, TContract>(string identifier);
        IFactoryBinder<TParam1, TParam2, TContract> BindIFactory<TParam1, TParam2, TContract>();

        IFactoryBinder<TParam1, TContract> BindIFactory<TParam1, TContract>(string identifier);
        IFactoryBinder<TParam1, TContract> BindIFactory<TParam1, TContract>();

        IFactoryBinder<TContract> BindIFactory<TContract>(string identifier);
        IFactoryBinder<TContract> BindIFactory<TContract>();

        IFactoryUntypedBinder<TContract> BindIFactoryUntyped<TContract>(string identifier);
        IFactoryUntypedBinder<TContract> BindIFactoryUntyped<TContract>();

        BindingConditionSetter BindInstance<TContract>(string identifier, TContract obj);

        BindingConditionSetter BindInstance<TContract>(TContract obj);

        BinderGeneric<TContract> Bind<TContract>();

        BinderUntyped Bind(Type contractType);

        bool Unbind<TContract>();

        bool HasBinding<TContract>();

        bool HasBinding<TContract>(string identifier);

        void BindAllInterfacesToSingle<TConcrete>();

        void BindAllInterfacesToSingle(Type concreteType);

#if !ZEN_NOT_UNITY3D
        BindingConditionSetter BindGameObjectFactory<T>(
            GameObject prefab)
            // This would be useful but fails with VerificationException's in webplayer builds for some reason
            //where T : GameObjectFactory
            where T : class;
#endif
    }
}

