#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public static class CompositionRootHelper
    {
        public static void InstallSceneInstallers(
            DiContainer container, IEnumerable<IInstaller> installers)
        {
            foreach (var installer in installers)
            {
                Assert.IsNotNull(installer, "Found null installer in composition root");

                if (installer.IsEnabled)
                {
                    container.Install(installer);
                }
            }
        }
    }
}

#endif
