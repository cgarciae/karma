#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    public class ResourceProvider : ProviderBase
    {
        readonly Type _concreteType;
        readonly string _resourcePath;

        public ResourceProvider(Type concreteType, string resourcePath)
        {
            _concreteType = concreteType;
            _resourcePath = resourcePath;
        }

        public override Type GetInstanceType()
        {
            return _concreteType;
        }

        public override object GetInstance(InjectContext context)
        {
            var obj = Resources.Load(_resourcePath, _concreteType);

            Assert.IsNotNull(obj,
                "Could not find resource at path '{0}' with type '{1}'", _resourcePath, _concreteType);

            return obj;
        }

        Type GetTypeToInstantiate(Type contractType)
        {
            Assert.That(_concreteType.DerivesFromOrEqual(contractType));
            return _concreteType;
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            yield break;
        }
    }
}

#endif
