using System;
using System.Collections.Generic;
using System.Reflection;

namespace Zenject
{
    // The difference between a factory and a provider:
    // Factories create new instances, providers might return an existing instance
    // This is the same as IFactory<> except it does not have compile-time checking
    // of parameters
    public interface IFactoryUntyped<T>
    {
        // Note that we lose some type safety here when passing the arguments
        // We are trading compile time checks for some flexibility
        T Create(params object[] constructorArgs);

        IEnumerable<ZenjectResolveException> Validate(params Type[] extraType);
    }
}
