using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    public class TransientProvider : ProviderBase
    {
        readonly Type _concreteType;
        readonly DiContainer _container;

        public TransientProvider(DiContainer container, Type concreteType)
        {
            _container = container;
            _concreteType = concreteType;
        }

        public override Type GetInstanceType()
        {
            return _concreteType;
        }

        public override object GetInstance(InjectContext context)
        {
            var obj = _container.InstantiateExplicit(
                GetTypeToInstantiate(context.MemberType), new List<TypeValuePair>(), context, null, true);
            Assert.That(obj != null);
            return obj;
        }

        Type GetTypeToInstantiate(Type contractType)
        {
            if (_concreteType.IsOpenGenericType())
            {
                Assert.That(!contractType.IsAbstract);
                Assert.That(contractType.GetGenericTypeDefinition() == _concreteType);
                return contractType;
            }

            Assert.That(_concreteType.DerivesFromOrEqual(contractType));
            return _concreteType;
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            return BindingValidator.ValidateObjectGraph(_container, _concreteType, context, null);
        }
    }
}
