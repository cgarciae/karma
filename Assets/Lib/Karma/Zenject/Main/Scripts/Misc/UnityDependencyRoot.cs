#if !ZEN_NOT_UNITY3D

using System.Collections.Generic;
using ModestTree;
using ModestTree.Util.Debugging;
using UnityEngine;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public sealed class UnityDependencyRoot : MonoBehaviour, IDependencyRoot
    {
        [Inject]
        TickableManager _tickableManager = null;

        [Inject]
        InitializableManager _initializableManager = null;

        [Inject]
        DisposableManager _disposablesManager = null;

        bool _disposed;

        // For cases where you have game objects that aren't referenced anywhere but still want them to be
        // created on startup
        [InjectOptional]
        public List<object> _initialObjects = null;

        public void Initialize()
        {
            _initializableManager.Initialize();
        }

        public void OnApplicationQuit()
        {
            // In some cases we have monobehaviour's that are bound to IDisposable, and who have
            // also been set with Application.DontDestroyOnLoad so that the Dispose() is always
            // called instead of OnDestroy.  This is nice because we can actually reliably predict the
            // order Dispose() is called in which is not the case for OnDestroy.
            // However, when the user quits the app, OnDestroy is called even for objects that
            // have been marked with Application.DontDestroyOnLoad, and so the destruction order
            // changes.  So to address this case, dispose before the OnDestroy event below (OnApplicationQuit
            // is always called before OnDestroy) and then don't call dispose in OnDestroy
            Assert.That(!_disposed);
            _disposed = true;
            _disposablesManager.Dispose();
        }

        public void OnDestroy()
        {
            if (!_disposed)
            {
                _disposablesManager.Dispose();
            }
        }

        public void Update()
        {
            _tickableManager.Update();
        }

        public void FixedUpdate()
        {
            _tickableManager.FixedUpdate();
        }

        public void LateUpdate()
        {
            _tickableManager.LateUpdate();
        }
    }
}

#endif
