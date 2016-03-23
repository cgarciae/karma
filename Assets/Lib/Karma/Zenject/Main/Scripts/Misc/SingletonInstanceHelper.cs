using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class SingletonInstanceHelper
    {
#if !ZEN_NOT_UNITY3D
        [Inject]
        readonly PrefabSingletonProviderMap _prefabSingletonProviderMap = null;
#endif

        [Inject]
        readonly SingletonProviderMap _singletonProviderMap = null;

        public IEnumerable<Type> GetActiveSingletonTypesDerivingFrom<T>(
            IEnumerable<Type> ignoreTypes)
        {
            var unboundTypes = _singletonProviderMap.Creators
                .Select(x => x.GetInstanceType())
                .Where(x => x.DerivesFrom<T>())
                .Where(x => !ignoreTypes.Contains(x));

#if ZEN_NOT_UNITY3D
            return unboundTypes.Distinct();
#else
            var unboundPrefabTypes = _prefabSingletonProviderMap.Creators
                .SelectMany(x => x.GetAllComponentTypes())
                .Where(x => x.DerivesFrom<T>())
                .Where(x => !ignoreTypes.Contains(x));

            return unboundTypes.Concat(unboundPrefabTypes).Distinct();
#endif
        }
    }
}
