using System;
using System.Collections.Generic;
using Zenject;
using ModestTree;
using ModestTree.Util;

namespace Zenject
{
    public class TickablePrioritiesInstaller : Installer
    {
        List<Type> _tickables;

        public TickablePrioritiesInstaller(List<Type> tickables)
        {
            _tickables = tickables;
        }

        public override void InstallBindings()
        {
            // All tickables without explicit priorities assigned are given priority of zero,
            // so put all of these before that (ie. negative)
            int priorityCount = -1 * _tickables.Count;

            foreach (var tickableType in _tickables)
            {
                BindPriority(Container, tickableType, priorityCount);
                priorityCount++;
            }
        }

        public static void BindPriority<T>(
            DiContainer container, int priorityCount)
            where T : ITickable
        {
            BindPriority(container, typeof(T), priorityCount);
        }

        public static void BindPriority(
            DiContainer container, Type tickableType, int priorityCount)
        {
            Assert.That(tickableType.DerivesFrom<ITickable>(),
                "Expected type '{0}' to derive from ITickable", tickableType.Name());

            container.Bind<ModestTree.Util.Tuple<Type, int>>().ToInstance(
                ModestTree.Util.Tuple.New(tickableType, priorityCount)).WhenInjectedInto<TickableManager>();
        }
    }
}
