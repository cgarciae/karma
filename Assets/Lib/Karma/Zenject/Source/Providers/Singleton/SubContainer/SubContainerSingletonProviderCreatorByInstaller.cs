using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using Zenject.Internal;

namespace Zenject
{
    public class SubContainerSingletonProviderCreatorByInstaller
    {
        readonly SingletonMarkRegistry _markRegistry;
        readonly DiContainer _container;
        readonly Dictionary<InstallerSingletonId, ISubContainerCreator> _subContainerCreators =
            new Dictionary<InstallerSingletonId, ISubContainerCreator>();

        public SubContainerSingletonProviderCreatorByInstaller(
            DiContainer container,
            SingletonMarkRegistry markRegistry)
        {
            _markRegistry = markRegistry;
            _container = container;
        }

        public IProvider CreateProvider(
            Type resultType, string concreteIdentifier, Type installerType, object identifier)
        {
            _markRegistry.MarkSingleton(
                resultType, concreteIdentifier,
                SingletonTypes.ToSubContainerInstaller);

            var subContainerSingletonId = new InstallerSingletonId(
                concreteIdentifier, installerType);

            ISubContainerCreator subContainerCreator;

            if (!_subContainerCreators.TryGetValue(subContainerSingletonId, out subContainerCreator))
            {
                subContainerCreator = new SubContainerCreatorCached(
                    new SubContainerCreatorByInstaller(
                        _container, installerType));

                _subContainerCreators.Add(subContainerSingletonId, subContainerCreator);
            }

            return new SubContainerDependencyProvider(
                resultType, identifier, subContainerCreator);
        }

        class InstallerSingletonId : IEquatable<InstallerSingletonId>
        {
            public readonly string ConcreteIdentifier;
            public readonly Type InstallerType;

            public InstallerSingletonId(string concreteIdentifier, Type installerType)
            {
                ConcreteIdentifier = concreteIdentifier;
                InstallerType = installerType;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    hash = hash * 29 + (this.ConcreteIdentifier == null ? 0 : this.ConcreteIdentifier.GetHashCode());
                    hash = hash * 29 + this.InstallerType.GetHashCode();
                    return hash;
                }
            }

            public override bool Equals(object other)
            {
                if (other is InstallerSingletonId)
                {
                    InstallerSingletonId otherId = (InstallerSingletonId)other;
                    return otherId == this;
                }
                else
                {
                    return false;
                }
            }

            public bool Equals(InstallerSingletonId that)
            {
                return this == that;
            }

            public static bool operator ==(InstallerSingletonId left, InstallerSingletonId right)
            {
                return object.Equals(left.InstallerType, right.InstallerType)
                    && object.Equals(left.ConcreteIdentifier, right.ConcreteIdentifier);
            }

            public static bool operator !=(InstallerSingletonId left, InstallerSingletonId right)
            {
                return !left.Equals(right);
            }
        }
    }
}
