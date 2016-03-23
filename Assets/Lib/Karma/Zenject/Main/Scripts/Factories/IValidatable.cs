using System;
using System.Collections.Generic;

namespace Zenject
{
    public interface IValidatable
    {
        IEnumerable<ZenjectResolveException> Validate();
    }
}
