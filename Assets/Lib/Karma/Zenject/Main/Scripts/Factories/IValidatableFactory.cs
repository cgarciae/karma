using System;
using System.Collections.Generic;

namespace Zenject
{
    public interface IValidatableFactory
    {
        Type ConstructedType
        {
            get;
        }

        Type[] ProvidedTypes
        {
            get;
        }
    }
}
