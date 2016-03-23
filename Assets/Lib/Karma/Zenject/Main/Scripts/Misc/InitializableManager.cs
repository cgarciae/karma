using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;
using ModestTree.Util.Debugging;

namespace Zenject
{
    // Responsibilities:
    // - Run Initialize() on all Iinitializable's, in the order specified by InitPriority
    public class InitializableManager
    {
        readonly SingletonInstanceHelper _singletonInstanceHelper = null;

        List<InitializableInfo> _initializables = new List<InitializableInfo>();

        public InitializableManager(
            [InjectOptional]
            List<IInitializable> initializables,
            [InjectOptional]
            List<ModestTree.Util.Tuple<Type, int>> priorities,
            DiContainer container,
            SingletonInstanceHelper singletonInstanceHelper)
        {
            _singletonInstanceHelper = singletonInstanceHelper;

            WarnForMissingBindings(initializables, container);

            foreach (var initializable in initializables)
            {
                // Note that we use zero for unspecified priority
                // This is nice because you can use negative or positive for before/after unspecified
                var matches = priorities.Where(x => initializable.GetType().DerivesFromOrEqual(x.First)).Select(x => x.Second).ToList();
                int priority = matches.IsEmpty() ? 0 : matches.Single();

                _initializables.Add(new InitializableInfo(initializable, priority));
            }
        }

        void WarnForMissingBindings(List<IInitializable> initializables, DiContainer container)
        {
            var boundTypes = initializables.Select(x => x.GetType()).Distinct();
            var unboundTypes = _singletonInstanceHelper.GetActiveSingletonTypesDerivingFrom<IInitializable>(boundTypes);

            foreach (var objType in unboundTypes)
            {
                Log.Warn("Found unbound IInitializable with type '" + objType.Name() + "'");
            }
        }

        public void Initialize()
        {
            _initializables = _initializables.OrderBy(x => x.Priority).ToList();

            foreach (var initializable in _initializables.Select(x => x.Initializable).GetDuplicates())
            {
                Assert.That(false, "Found duplicate IInitializable with type '{0}'".Fmt(initializable.GetType()));
            }

            foreach (var initializable in _initializables)
            {
                //Log.Info("Initializing initializable with type '" + initializable.GetType() + "'");

                try
                {
#if PROFILING_ENABLED
                    using (ProfileBlock.Start("{0}.Initialize()", initializable.Initializable.GetType().Name()))
#endif
                    {
                        initializable.Initializable.Initialize();
                    }
                }
                catch (Exception e)
                {
                    throw new ZenjectException(
                        "Error occurred while initializing IInitializable with type '{0}'".Fmt(initializable.Initializable.GetType().Name()), e);
                }
            }
        }

        class InitializableInfo
        {
            public IInitializable Initializable;
            public int Priority;

            public InitializableInfo(IInitializable initializable, int priority)
            {
                Initializable = initializable;
                Priority = priority;
            }
        }
    }
}
