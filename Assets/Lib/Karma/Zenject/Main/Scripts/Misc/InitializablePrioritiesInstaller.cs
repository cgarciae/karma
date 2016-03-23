using System;
using System.Collections.Generic;
using Zenject;
using ModestTree;
using ModestTree.Util;

namespace Zenject
{
    public class InitializablePrioritiesInstaller : Installer
    {
        List<Type> _initializables;

        public InitializablePrioritiesInstaller(
            List<Type> initializables)
        {
            _initializables = initializables;
        }

        public override void InstallBindings()
        {
            // All initializables without explicit priorities assigned are given priority of zero,
            // so put all of these before that (ie. negative)
            int priorityCount = -1 * _initializables.Count;

            foreach (var initializableType in _initializables)
            {
                BindPriority(Container, initializableType, priorityCount);
                priorityCount++;
            }
        }

        public static void BindPriority<T>(
            DiContainer container, int priorityCount)
            where T : IInitializable
        {
            BindPriority(container, typeof(T), priorityCount);
        }

        public static void BindPriority(
            DiContainer container, Type initializableType, int priorityCount)
        {
            Assert.That(initializableType.DerivesFrom<IInitializable>(),
                "Expected type '{0}' to derive from IInitializable", initializableType.Name());

            container.Bind<ModestTree.Util.Tuple<Type, int>>().ToInstance(
                ModestTree.Util.Tuple.New(initializableType, priorityCount)).WhenInjectedInto<InitializableManager>();
        }
    }
}

