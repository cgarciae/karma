using System;
using System.Collections.Generic;

namespace Zenject
{
    // Derived class should contain all dependencies
    // for the the given run configuration
    public interface IDependencyRoot
    {
        void Initialize();
    }
}
