using System;
using System.Collections.Generic;
using Zenject;
using ModestTree;
using ModestTree.Util;

namespace Zenject
{
    public class FixedTickablePrioritiesInstaller : Installer
    {
        List<Type> _tickables;

        public FixedTickablePrioritiesInstaller(
            List<Type> tickables)
        {
            _tickables = tickables;
        }

        public override void InstallBindings()
        {
            int priorityCount = 1;

            foreach (var tickableType in _tickables)
            {
                BindPriority(Container, tickableType, priorityCount);
                priorityCount++;
            }
        }

        public static void BindPriority(
            DiContainer container, Type tickableType, int priorityCount)
        {
            Assert.That(tickableType.DerivesFrom<IFixedTickable>(),
                "Expected type '{0}' to derive from IFixedTickable", tickableType.Name());

            container.Bind<ModestTree.Util.Tuple<Type, int>>("Fixed").ToInstance(
                ModestTree.Util.Tuple.New(tickableType, priorityCount)).WhenInjectedInto<TickableManager>();
        }
    }
}

