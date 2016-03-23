using System;
using System.Collections.Generic;
using Zenject;
using ModestTree;
using ModestTree.Util;

namespace Zenject
{
    public class DisposablePrioritiesInstaller : Installer
    {
        List<Type> _disposables;

        public DisposablePrioritiesInstaller(List<Type> disposables)
        {
            _disposables = disposables;
        }

        public override void InstallBindings()
        {
            // All disposables without explicit priorities assigned are given priority of zero,
            // so put all of these before that (ie. negative)
            int priorityCount = -1 * _disposables.Count;

            foreach (var disposableType in _disposables)
            {
                BindPriority(Container, disposableType, priorityCount);
                priorityCount++;
            }
        }

        public static void BindPriority<T>(
            DiContainer container, int priorityCount)
            where T : IDisposable
        {
            BindPriority(container, typeof(T), priorityCount);
        }

        public static void BindPriority(
            DiContainer container, Type disposableType, int priorityCount)
        {
            Assert.That(disposableType.DerivesFrom<IDisposable>(),
                "Expected type '{0}' to derive from IDisposable", disposableType.Name());

            container.Bind<ModestTree.Util.Tuple<Type, int>>().ToInstance(
                ModestTree.Util.Tuple.New(disposableType, priorityCount)).WhenInjectedInto<DisposableManager>();
        }
    }
}

