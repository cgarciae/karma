using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;

namespace Zenject
{
    public interface IInitializable
    {
        void Initialize();
    }
}
