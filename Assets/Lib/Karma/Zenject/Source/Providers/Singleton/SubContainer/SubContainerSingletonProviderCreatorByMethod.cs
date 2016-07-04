using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using Zenject.Internal;

namespace Zenject
{
    public class SubContainerSingletonProviderCreatorByMethod
    {
        readonly SingletonMarkRegistry _markRegistry;
        readonly DiContainer _container;
        readonly Dictionary<MethodSingletonId, ISubContainerCreator> _subContainerCreators =
            new Dictionary<MethodSingletonId, ISubContainerCreator>();

        public SubContainerSingletonProviderCreatorByMethod(
            DiContainer container,
            SingletonMarkRegistry markRegistry)
        {
            _markRegistry = markRegistry;
            _container = container;
        }

        public IProvider CreateProvider(
            Type resultType, string concreteIdentifier,
            Action<DiContainer> installMethod, object identifier)
        {
            _markRegistry.MarkSingleton(
                new SingletonId(resultType, concreteIdentifier),
                SingletonTypes.ToSubContainerMethod);

            ISubContainerCreator subContainerCreator;

            var subContainerSingletonId = new MethodSingletonId(
                concreteIdentifier, installMethod);

            if (!_subContainerCreators.TryGetValue(subContainerSingletonId, out subContainerCreator))
            {
                subContainerCreator = new SubContainerCreatorCached(
                    new SubContainerCreatorByMethod(
                        _container, installMethod));

                _subContainerCreators.Add(subContainerSingletonId, subContainerCreator);
            }

            return new SubContainerDependencyProvider(
                resultType, identifier, subContainerCreator);
        }

        class MethodSingletonId : IEquatable<MethodSingletonId>
        {
            public readonly string ConcreteIdentifier;
            public readonly Delegate InstallerDelegate;

            public MethodSingletonId(string concreteIdentifier, Delegate installerMethod)
            {
                ConcreteIdentifier = concreteIdentifier;
                InstallerDelegate = installerMethod;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    hash = hash * 29 + (this.ConcreteIdentifier == null ? 0 : this.ConcreteIdentifier.GetHashCode());
                    hash = hash * 29 + this.InstallerDelegate.Target.GetHashCode();
                    hash = hash * 29 + this.InstallerDelegate.Method().GetHashCode();
                    return hash;
                }
            }

            public override bool Equals(object other)
            {
                if (other is MethodSingletonId)
                {
                    MethodSingletonId otherId = (MethodSingletonId)other;
                    return otherId == this;
                }
                else
                {
                    return false;
                }
            }

            public bool Equals(MethodSingletonId that)
            {
                return this == that;
            }

            public static bool operator ==(MethodSingletonId left, MethodSingletonId right)
            {
                return object.Equals(left.InstallerDelegate.Target, right.InstallerDelegate.Target)
                    && object.Equals(left.InstallerDelegate.Method(), right.InstallerDelegate.Method())
                    && object.Equals(left.ConcreteIdentifier, right.ConcreteIdentifier);
            }

            public static bool operator !=(MethodSingletonId left, MethodSingletonId right)
            {
                return !left.Equals(right);
            }
        }
    }
}
