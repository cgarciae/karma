using System;
using ModestTree;

namespace Zenject
{
    public class FactorySubContainerBinderBase<TContract>
    {
        readonly BindFinalizerWrapper _finalizerWrapper;

        public FactorySubContainerBinderBase(
            BindInfo bindInfo, Type factoryType,
            BindFinalizerWrapper finalizerWrapper, object subIdentifier)
        {
            SubIdentifier = subIdentifier;
            BindInfo = bindInfo;
            FactoryType = factoryType;

            _finalizerWrapper = finalizerWrapper;

            // Reset so we get errors if we end here
            finalizerWrapper.SubFinalizer = null;
        }

        protected Type FactoryType
        {
            get;
            private set;
        }

        protected BindInfo BindInfo
        {
            get;
            private set;
        }

        protected object SubIdentifier
        {
            get;
            private set;
        }

        protected Type ContractType
        {
            get
            {
                return typeof(TContract);
            }
        }

        protected IBindingFinalizer SubFinalizer
        {
            set
            {
                _finalizerWrapper.SubFinalizer = value;
            }
        }

        protected IBindingFinalizer CreateFinalizer(Func<DiContainer, IProvider> providerFunc)
        {
            return new DynamicFactoryBindingFinalizer<TContract>(
                BindInfo, FactoryType, providerFunc);
        }

        public ConditionBinder ByInstaller<TInstaller>()
            where TInstaller : InstallerBase
        {
            return ByInstaller(typeof(TInstaller));
        }

        public ConditionBinder ByInstaller(Type installerType)
        {
            Assert.That(installerType.DerivesFrom<InstallerBase>(),
                "Invalid installer type given during bind command.  Expected type '{0}' to derive from 'Installer<>'", installerType.Name());

            SubFinalizer = CreateFinalizer(
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByInstaller(
                        container, installerType)));

            return new ConditionBinder(BindInfo);
        }
    }
}
