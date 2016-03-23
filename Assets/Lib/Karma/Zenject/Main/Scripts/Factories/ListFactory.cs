using System;
using System.Collections.Generic;
using Zenject;
using System.Linq;
using ModestTree;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class ListFactory<T>
    {
        readonly DiContainer _container;
        List<Type> _implTypes;

        public ListFactory(
            List<Type> implTypes, DiContainer container)
        {
            foreach (var type in implTypes)
            {
                Assert.That(type.DerivesFromOrEqual<T>());
            }

            _implTypes = implTypes;
            _container = container;
        }

        public static void Bind()
        {
        }

        public List<T> Create(params object[] constructorArgs)
        {
            var list = new List<T>();

            foreach (var type in _implTypes)
            {
                list.Add(_container.Instantiate<T>(type, constructorArgs));
            }

            return list;
        }
    }
}

