#if !ZEN_NOT_UNITY3D
using UnityEngine;

namespace Zenject
{
    public abstract class CompositionRoot : MonoBehaviour
    {
        public abstract DiContainer Container
        {
            get;
        }
    }
}

#endif

