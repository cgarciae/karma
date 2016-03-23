using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;
using ModestTree.Util.Debugging;

namespace Zenject
{
    public class TickableManager
    {
        [InjectOptional]
        readonly List<ITickable> _tickables = null;

        [InjectOptional]
        readonly List<IFixedTickable> _fixedTickables = null;

        [InjectOptional]
        readonly List<ILateTickable> _lateTickables = null;

        [InjectOptional]
        readonly List<ModestTree.Util.Tuple<Type, int>> _priorities = null;

        [InjectOptional("Fixed")]
        readonly List<ModestTree.Util.Tuple<Type, int>> _fixedPriorities = null;

        [InjectOptional("Late")]
        readonly List<ModestTree.Util.Tuple<Type, int>> _latePriorities = null;

        [Inject]
        readonly SingletonInstanceHelper _singletonInstanceHelper = null;

        TaskUpdater<ITickable> _updater;
        TaskUpdater<IFixedTickable> _fixedUpdater;
        TaskUpdater<ILateTickable> _lateUpdater;

        [InjectOptional]
        bool _warnForMissing = false;

        [PostInject]
        public void Initialize()
        {
            InitTickables();
            InitFixedTickables();
            InitLateTickables();

            if (_warnForMissing)
            {
                WarnForMissingBindings();
            }
        }

        void WarnForMissingBindings()
        {
            var boundTypes = _tickables.Select(x => x.GetType()).Distinct();

            var unboundTypes = _singletonInstanceHelper.GetActiveSingletonTypesDerivingFrom<ITickable>(boundTypes);

            foreach (var objType in unboundTypes)
            {
                Log.Warn("Found unbound ITickable with type '" + objType.Name() + "'");
            }
        }

        void InitFixedTickables()
        {
            _fixedUpdater = new TaskUpdater<IFixedTickable>(UpdateFixedTickable);

            foreach (var type in _fixedPriorities.Select(x => x.First))
            {
                Assert.That(type.DerivesFrom<IFixedTickable>(),
                    "Expected type '{0}' to drive from IFixedTickable while checking priorities in TickableHandler", type.Name());
            }

            foreach (var tickable in _fixedTickables)
            {
                // Note that we use zero for unspecified priority
                // This is nice because you can use negative or positive for before/after unspecified
                var matches = _fixedPriorities.Where(x => tickable.GetType().DerivesFromOrEqual(x.First)).Select(x => x.Second).ToList();
                int priority = matches.IsEmpty() ? 0 : matches.Single();

                _fixedUpdater.AddTask(tickable, priority);
            }
        }

        void InitTickables()
        {
            _updater = new TaskUpdater<ITickable>(UpdateTickable);

            foreach (var type in _priorities.Select(x => x.First))
            {
                Assert.That(type.DerivesFrom<ITickable>(),
                    "Expected type '{0}' to drive from ITickable while checking priorities in TickableHandler", type.Name());
            }

            foreach (var tickable in _tickables)
            {
                // Note that we use zero for unspecified priority
                // This is nice because you can use negative or positive for before/after unspecified
                var matches = _priorities.Where(x => tickable.GetType().DerivesFromOrEqual(x.First)).Select(x => x.Second).ToList();
                int priority = matches.IsEmpty() ? 0 : matches.Single();

                _updater.AddTask(tickable, priority);
            }
        }

        void InitLateTickables()
        {
            _lateUpdater = new TaskUpdater<ILateTickable>(UpdateLateTickable);

            foreach (var type in _latePriorities.Select(x => x.First))
            {
                Assert.That(type.DerivesFrom<ILateTickable>(),
                    "Expected type '{0}' to drive from ILateTickable while checking priorities in TickableHandler", type.Name());
            }

            foreach (var tickable in _lateTickables)
            {
                // Note that we use zero for unspecified priority
                // This is nice because you can use negative or positive for before/after unspecified
                var matches = _latePriorities.Where(x => tickable.GetType().DerivesFromOrEqual(x.First)).Select(x => x.Second).ToList();
                int priority = matches.IsEmpty() ? 0 : matches.Single();

                _lateUpdater.AddTask(tickable, priority);
            }
        }

        void UpdateLateTickable(ILateTickable tickable)
        {
#if PROFILING_ENABLED
            using (ProfileBlock.Start("{0}.LateTick()".Fmt(tickable.GetType().Name())))
#endif
            {
                tickable.LateTick();
            }
        }

        void UpdateFixedTickable(IFixedTickable tickable)
        {
#if PROFILING_ENABLED
            using (ProfileBlock.Start("{0}.FixedTick()".Fmt(tickable.GetType().Name())))
#endif
            {
                tickable.FixedTick();
            }
        }

        void UpdateTickable(ITickable tickable)
        {
#if PROFILING_ENABLED
            using (ProfileBlock.Start("{0}.Tick()".Fmt(tickable.GetType().Name())))
#endif
            {
                tickable.Tick();
            }
        }

        public void Add(ITickable tickable, int priority)
        {
            _updater.AddTask(tickable, priority);
        }

        public void Add(ITickable tickable)
        {
            Add(tickable, 0);
        }

        public void AddLate(ILateTickable tickable, int priority)
        {
            _lateUpdater.AddTask(tickable, priority);
        }

        public void AddLate(ILateTickable tickable)
        {
            AddLate(tickable, 0);
        }

        public void AddFixed(IFixedTickable tickable, int priority)
        {
            _fixedUpdater.AddTask(tickable, priority);
        }

        public void AddFixed(IFixedTickable tickable)
        {
            _fixedUpdater.AddTask(tickable, 0);
        }

        public void Remove(ITickable tickable)
        {
            _updater.RemoveTask(tickable);
        }

        public void RemoveLate(ILateTickable tickable)
        {
            _lateUpdater.RemoveTask(tickable);
        }

        public void RemoveFixed(IFixedTickable tickable)
        {
            _fixedUpdater.RemoveTask(tickable);
        }

        public void Update()
        {
            _updater.OnFrameStart();
            _updater.UpdateAll();
        }

        public void FixedUpdate()
        {
            _fixedUpdater.OnFrameStart();
            _fixedUpdater.UpdateAll();
        }

        public void LateUpdate()
        {
            _lateUpdater.OnFrameStart();
            _lateUpdater.UpdateAll();
        }
    }
}
