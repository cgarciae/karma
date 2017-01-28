using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public class SubContainerCreatorByInstaller : ISubContainerCreator
    {
        readonly Type _installerType;
        readonly DiContainer _container;

        public SubContainerCreatorByInstaller(
            DiContainer container, Type installerType)
        {
            _installerType = installerType;
            _container = container;

            Assert.That(installerType.DerivesFrom<InstallerBase>(),
                "Invalid installer type given during bind command.  Expected type '{0}' to derive from 'Installer<>'", installerType.Name());
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args)
        {
            var subContainer = _container.CreateSubContainer();

            var installer = (InstallerBase)subContainer.InstantiateExplicit(
                _installerType, args);
            installer.InstallBindings();

            subContainer.ResolveDependencyRoots();

            return subContainer;
        }
    }
}
