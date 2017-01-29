#if !NOT_UNITY3D

using System.Collections.Generic;
using UnityEngine;

namespace Zenject
{
    public interface IPrefabInstantiator
    {
        List<TypeValuePair> ExtraArguments
        {
            get;
        }

        GameObjectCreationParameters GameObjectCreationParameters
        {
            get;
        }

        IEnumerator<GameObject> Instantiate(List<TypeValuePair> args);

        UnityEngine.Object GetPrefab();
    }
}

#endif
