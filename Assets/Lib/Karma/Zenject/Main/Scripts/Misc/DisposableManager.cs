using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;

namespace Zenject
{
    public class DisposableManager : IDisposable
    {
        readonly SingletonInstanceHelper _singletonInstanceHelper;
        List<DisposableInfo> _disposables = new List<DisposableInfo>();
        bool _disposed;

        public DisposableManager(
            [InjectOptional]
            List<IDisposable> disposables,
            [InjectOptional]
            List<ModestTree.Util.Tuple<Type, int>> priorities,
            SingletonInstanceHelper singletonInstanceHelper)
        {
            _singletonInstanceHelper = singletonInstanceHelper;

            foreach (var disposable in disposables)
            {
                // Note that we use zero for unspecified priority
                // This is nice because you can use negative or positive for before/after unspecified
                var matches = priorities.Where(x => disposable.GetType().DerivesFromOrEqual(x.First)).Select(x => x.Second).ToList();
                int priority = matches.IsEmpty() ? 0 : matches.Single();

                _disposables.Add(new DisposableInfo(disposable, priority));
            }

            Log.Debug("Loaded {0} IDisposables to DisposablesHandler", _disposables.Count());
        }

        public void Dispose()
        {
            Assert.That(!_disposed);
            _disposed = true;

            _disposables = _disposables.OrderBy(x => x.Priority).ToList();

            //WarnForMissingBindings();

            foreach (var disposable in _disposables.Select(x => x.Disposable).GetDuplicates())
            {
                Assert.That(false, "Found duplicate IDisposable with type '{0}'".Fmt(disposable.GetType()));
            }

            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable.Disposable.Dispose();
                }
                catch (Exception e)
                {
                    throw new ZenjectException(
                        "Error occurred while disposing IDisposable with type '{0}'".Fmt(disposable.Disposable.GetType().Name()), e);
                }
            }

            Log.Debug("Disposed of {0} disposables in DisposablesHandler", _disposables.Count());
        }

        void WarnForMissingBindings()
        {
            var ignoredTypes = new Type[] { typeof(DisposableManager) };
            var boundTypes = _disposables.Select(x => x.Disposable.GetType()).Distinct();

            var unboundTypes = _singletonInstanceHelper.GetActiveSingletonTypesDerivingFrom<IDisposable>(boundTypes.Concat(ignoredTypes));

            foreach (var objType in unboundTypes)
            {
                Log.Warn("Found unbound IDisposable with type '" + objType.Name() + "'");
            }
        }

        class DisposableInfo
        {
            public IDisposable Disposable;
            public int Priority;

            public DisposableInfo(IDisposable disposable, int priority)
            {
                Disposable = disposable;
                Priority = priority;
            }
        }
    }
}
