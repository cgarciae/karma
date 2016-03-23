#if !ZEN_NOT_UNITY3D

using System;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoBindAttribute : Attribute
    {
    }

    public class AutoBindInstaller : Installer
    {
        GameObject _root;

        public override void InstallBindings()
        {
            Assert.IsNotNull(_root);

            foreach (var monoBehaviour in _root.GetComponentsInChildren<MonoBehaviour>())
            {
                if (monoBehaviour != null
                    && monoBehaviour.GetType().HasAttribute<AutoBindAttribute>())
                {
                    Container.Bind(monoBehaviour.GetType()).ToInstance(monoBehaviour);
                }
            }
        }

        [PostInject]
        public void Initialize(CompositionRoot compRoot)
        {
            _root = compRoot.gameObject;
        }
    }
}

#endif