#if !ZEN_NOT_UNITY3D

using System;
using UnityEngine;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public sealed class GlobalInstallerConfig : ScriptableObject
    {
        // We can refer directly to the prefabs in this case because the properties of the installers should not change
        // You could do the same for the scene composition root installers BUT this is error prone since the prefab
        // may change at run time (for eg. if another scene injects a property into it) so it's often better
        // to copy the installer prefabs into your scene
        public MonoInstaller[] Installers;
    }
}

#endif
