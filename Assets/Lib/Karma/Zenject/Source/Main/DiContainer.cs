using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;
using Zenject.Internal;

#if !NOT_UNITY3D
using UnityEngine;
#endif

namespace Zenject
{
    public delegate bool BindingCondition(InjectContext c);

    // Responsibilities:
    // - Expose methods to configure object graph via Bind() methods
    // - Build object graphs via Resolve() method
    public class DiContainer : IInstantiator, IResolver, IBinder
    {
        public const string DependencyRootIdentifier = "DependencyRoot";

        readonly Dictionary<BindingId, List<ProviderInfo>> _providers = new Dictionary<BindingId, List<ProviderInfo>>();
        readonly HashSet<Type> _installedInstallers = new HashSet<Type>();
        readonly Stack<Type> _installsInProgress = new Stack<Type>();
        readonly DiContainer _parentContainer;
        readonly Stack<LookupId> _resolvesInProgress = new Stack<LookupId>();

        readonly SingletonProviderCreator _singletonProviderCreator;
        readonly SingletonMarkRegistry _singletonMarkRegistry;

        readonly Queue<IBindingFinalizer> _currentBindings = new Queue<IBindingFinalizer>();
        readonly List<IBindingFinalizer> _processedBindings = new List<IBindingFinalizer>();

        bool _isValidating;
        bool _isInstalling;
        bool _hasDisplayedInstallWarning;

        public DiContainer(bool isValidating)
        {
            _isValidating = isValidating;

            _singletonMarkRegistry = new SingletonMarkRegistry();
            _singletonProviderCreator = new SingletonProviderCreator(this, _singletonMarkRegistry);

            // We can't simply call Bind<DiContainer>().FromInstance(this) here because
            // we don't want these bindings to be included in the Clone() below
            // So just directly add to the provider map instead
            var thisProvider = new InstanceProvider(typeof(DiContainer), this);
            var thisContracts = new Type[]
            {
                typeof(DiContainer), typeof(IBinder), typeof(IResolver), typeof(IInstantiator)
            };

            foreach (var contractType in thisContracts)
            {
                var infoList = new List<ProviderInfo>()
                {
                    new ProviderInfo(thisProvider, null)
                };

                var bindingId = new BindingId(contractType, null);

                _providers.Add(bindingId, infoList);
            }
        }

        public DiContainer()
            : this(false)
        {
        }

        public DiContainer(DiContainer parentContainer, bool isValidating)
            : this(isValidating)
        {
            _parentContainer = parentContainer;

            if (parentContainer != null)
            {
                parentContainer.FlushBindings();

#if !NOT_UNITY3D
                DefaultParent = parentContainer.DefaultParent;
#endif
                foreach (var binding in parentContainer._processedBindings
                        .Where(x => x.InheritInSubContainers))
                {
                    _currentBindings.Enqueue(binding);
                }

                FlushBindings();
            }
        }

        public DiContainer(DiContainer parentContainer)
            : this(parentContainer, false)
        {
        }

        // When true, this will throw exceptions whenever we create new game objects
        // This is helpful when used in places like EditorWindowKernel where we can't
        // assume that there is a "scene" to place objects
        public bool AssertOnNewGameObjects
        {
            get;
            set;
        }

        public SingletonMarkRegistry SingletonMarkRegistry
        {
            get
            {
                return _singletonMarkRegistry;
            }
        }

        public SingletonProviderCreator SingletonProviderCreator
        {
            get
            {
                return _singletonProviderCreator;
            }
        }

#if !NOT_UNITY3D

        public Transform DefaultParent
        {
            get;
            set;
        }
#endif

        public DiContainer ParentContainer
        {
            get
            {
                return _parentContainer;
            }
        }

        public bool ChecksForCircularDependencies
        {
            get
            {
#if ZEN_MULTITHREADING
                // When multithreading is supported we can't use a static field to track the lookup
                // TODO: We could look at the inject context though
                return false;
#else
                return true;
#endif
            }
        }

        public IEnumerable<Type> InstalledInstallers
        {
            get
            {
                return _installedInstallers;
            }
        }

        // See comment in IBinder.cs for description
        public bool IsValidating
        {
            get
            {
                return _isValidating;
            }
        }

        // When this is true, it will log warnings when Resolve or Instantiate
        // methods are called
        // Used to ensure that Resolve and Instantiate methods are not called
        // during bind phase.  This is important since Resolve and Instantiate
        // make use of the bindings, so if the bindings are not complete then
        // unexpected behaviour can occur
        public bool IsInstalling
        {
            get
            {
                return _isInstalling;
            }
            set
            {
                _isInstalling = value;
            }
        }

        public IEnumerable<BindingId> AllContracts
        {
            get
            {
                FlushBindings();
                return _providers.Keys;
            }
        }

        // DO not run this within Unity!
        // This is only really useful if you are not using any of the Unity bind methods such as
        // FromGameObject, FromPrefab, etc.
        // If you are using those, and you call this method, then it will have side effects like
        // creating game objects
        // Otherwise, it should be safe to call since all the fake instances will be limited to
        // within a cloned copy of the DiContainer and should not have any side effects
        public void Validate()
        {
            var container = CloneForValidate();

            Assert.That(container.IsValidating);

            // It's tempting here to iterate over all the BindingId's in _providers
            // and make sure they can be resolved but that fails once we start
            // using complex conditionals, so this is the best we can do
            container.ResolveDependencyRoots();

            container.ValidateIValidatables();
        }

        public List<object> ResolveDependencyRoots()
        {
            var context = new InjectContext(
                this, typeof(object), DependencyRootIdentifier);
            context.SourceType = InjectSources.Local;
            context.Optional = true;

            return ResolveAll(context).Cast<object>().ToList();
        }

        DiContainer CloneForValidate()
        {
            FlushBindings();

            DiContainer container;

            if (this.ParentContainer == null)
            {
                container = new DiContainer(null, true);
            }
            else
            {
                // Need to clone all parents too
                container = new DiContainer(
                    this.ParentContainer.CloneForValidate(), true);
            }

            // Validating shouldn't have side effects, so assert if this occurs
            container.AssertOnNewGameObjects = true;

            foreach (var binding in _processedBindings)
            {
                container._currentBindings.Enqueue(binding);
            }

            container.FlushBindings();
            return container;
        }

        public void ValidateIValidatables()
        {
            Assert.That(IsValidating);

            foreach (var pair in _providers.ToList())
            {
                var bindingId = pair.Key;
                var providers = pair.Value;

                // Validate all IValidatable's
                List<ProviderInfo> validatableProviders;

                var injectContext = new InjectContext(
                    this, bindingId.Type, bindingId.Identifier);

                if (bindingId.Type.DerivesFrom<IValidatable>())
                {
                    validatableProviders = providers;
                }
                else
                {
                    validatableProviders = providers
                        .Where(x => x.Provider.GetInstanceType(injectContext)
                                .DerivesFrom<IValidatable>()).ToList();
                }

                foreach (var provider in validatableProviders)
                {
                    var validatable = (IValidatable)provider.Provider.GetInstance(injectContext);

                    validatable.Validate();
                }
            }
        }

        public DiContainer CreateSubContainer()
        {
            return CreateSubContainer(_isValidating);
        }

        public DiContainer CreateSubContainer(bool isValidating)
        {
            return new DiContainer(this, isValidating);
        }

        public void RegisterProvider(
            BindingId bindingId, BindingCondition condition, IProvider provider)
        {
            var info = new ProviderInfo(provider, condition);

            if (_providers.ContainsKey(bindingId))
            {
                _providers[bindingId].Add(info);
            }
            else
            {
                _providers.Add(bindingId, new List<ProviderInfo> { info });
            }
        }

        // Wrap IEnumerable<> to avoid LINQ mistakes
        internal List<IProvider> GetAllProviderMatches(InjectContext context)
        {
            Assert.IsNotNull(context);
            return GetProviderMatchesInternal(context).Select(x => x.ProviderInfo.Provider).ToList();
        }

        // Be careful with this method since it is a coroutine
        IEnumerable<ProviderPair> GetProviderMatchesInternal(InjectContext context)
        {
            Assert.IsNotNull(context);
            return GetProvidersForContract(context.GetBindingId(), context.SourceType)
                .Where(x => x.ProviderInfo.Condition == null || x.ProviderInfo.Condition(context));
        }

        IEnumerable<ProviderPair> GetProvidersForContract(BindingId bindingId, InjectSources sourceType)
        {
            FlushBindings();

            switch (sourceType)
            {
                case InjectSources.Local:
                {
                    return GetLocalProviders(bindingId).Select(x => new ProviderPair(x, this));
                }
                case InjectSources.Any:
                {
                    var localPairs = GetLocalProviders(bindingId).Select(x => new ProviderPair(x, this));

                    if (_parentContainer == null)
                    {
                        return localPairs;
                    }

                    return localPairs.Concat(
                        _parentContainer.GetProvidersForContract(bindingId, InjectSources.Any));
                }
                case InjectSources.AnyParent:
                {
                    if (_parentContainer == null)
                    {
                        return Enumerable.Empty<ProviderPair>();
                    }

                    return _parentContainer.GetProvidersForContract(bindingId, InjectSources.Any);
                }
                case InjectSources.Parent:
                {
                    if (_parentContainer == null)
                    {
                        return Enumerable.Empty<ProviderPair>();
                    }

                    return _parentContainer.GetProvidersForContract(bindingId, InjectSources.Local);
                }
            }

            throw Assert.CreateException("Invalid source type");
        }

        List<ProviderInfo> GetLocalProviders(BindingId bindingId)
        {
            List<ProviderInfo> localProviders;

            if (_providers.TryGetValue(bindingId, out localProviders))
            {
                return localProviders;
            }

            // If we are asking for a List<int>, we should also match for any localProviders that are bound to the open generic type List<>
            // Currently it only matches one and not the other - not totally sure if this is better than returning both
            if (bindingId.Type.IsGenericType() && _providers.TryGetValue(new BindingId(bindingId.Type.GetGenericTypeDefinition(), bindingId.Identifier), out localProviders))
            {
                return localProviders;
            }

            return new List<ProviderInfo>();
        }

        // See comment in IResolver.cs for description of this method
        public IList ResolveAll(InjectContext context)
        {
            Assert.IsNotNull(context);
            // Note that different types can map to the same provider (eg. a base type to a concrete class and a concrete class to itself)

            FlushBindings();
            CheckForInstallWarning(context);

            var matches = GetProviderMatchesInternal(context).ToList();

            if (matches.Any())
            {
                var instances = matches.SelectMany(x => SafeGetInstances(x.ProviderInfo.Provider, context)).ToArray();

                if (IsValidating)
                {
                    instances = instances.Select(x => x is ValidationMarker ? context.MemberType.GetDefaultValue() : x).ToArray();
                }

                return ReflectionUtil.CreateGenericList(context.MemberType, instances);
            }

            Assert.That(context.Optional,
                "Could not find required dependency with type '{0}' \nObject graph:\n {1}", context.MemberType.Name(), context.GetObjectGraphString());

            return ReflectionUtil.CreateGenericList(context.MemberType, new object[] {});
        }

        void CheckForInstallWarning(InjectContext context)
        {
            Assert.IsNotNull(context);
#if DEBUG || UNITY_EDITOR
            if (!_isInstalling)
            {
                return;
            }

            if (_hasDisplayedInstallWarning)
            {
                return;
            }

            if (context == null)
            {
                // No way to tell whether this is ok or not so just assume ok
                return;
            }

            var rootContext = context.ParentContextsAndSelf.Last();

            if (rootContext.MemberType.DerivesFrom<IInstaller>())
            {
                // Resolving/instantiating/injecting installers is valid during install phase
                return;
            }

#if !NOT_UNITY3D
            if (rootContext.MemberType.DerivesFrom<DecoratorInstaller>())
            {
                return;
            }
#endif

            _hasDisplayedInstallWarning = true;
            // Feel free to comment this out if you are comfortable with this practice
            Log.Warn("Zenject Warning: It is bad practice to call Inject/Resolve/Instantiate before all the Installers have completed!  This is important to ensure that all bindings have properly been installed in case they are needed when injecting/instantiating/resolving.  Detected when operating on type '{0}'", rootContext.MemberType.Name());
#endif
        }

        // See comment in IResolver.cs for description of this method
        public List<Type> ResolveTypeAll(InjectContext context)
        {
            Assert.IsNotNull(context);

            FlushBindings();
            CheckForInstallWarning(context);

            var bindingId = context.GetBindingId();

            if (_providers.ContainsKey(bindingId))
            {
                return _providers[bindingId].Select(x => x.Provider.GetInstanceType(context)).Where(x => x != null).ToList();
            }

            return new List<Type> {};
        }

        // See comment in IBinder.cs for description for this method
        public void Install(IEnumerable<IInstaller> installers)
        {
            foreach (var installer in installers)
            {
                Assert.IsNotNull(installer, "Tried to install a null installer");

                if (installer.IsEnabled)
                {
                    Install(installer);
                }
            }
        }

        // See comment in IBinder.cs for description for this method
        public void InstallExplicit(IInstaller installer, List<TypeValuePair> extraArgs)
        {
            Assert.That(installer.IsEnabled);

            FlushBindings();

            InjectExplicit(installer, extraArgs);
            InstallInstallerInternal(installer);
        }

        // See comment in IBinder.cs for description for this method
        public void Install(IInstaller installer)
        {
            Install(installer, new object[0]);
        }

        public void Install(IInstaller installer, IEnumerable<object> extraArgs)
        {
            Assert.That(installer.IsEnabled);

            FlushBindings();

            try
            {
                Inject(installer, extraArgs);
            }
            catch (Exception e)
            {
                throw new ZenjectException(e,
                    "While installing installer '{0}'", installer.GetType().Name());
            }

            InstallInstallerInternal(installer);
        }

        // See comment in IBinder.cs for description for this method
        public void Install<T>()
            where T : Installer
        {
            Install<T>(new object[0]);
        }

        public void Install<T>(IEnumerable<object> extraArgs)
            where T : Installer
        {
            Install(typeof(T), extraArgs);
        }

        // See comment in IBinder.cs for description of this method
        public void Install(Type installerType)
        {
            Install(installerType, new object[0]);
        }

        public void Install(Type installerType, IEnumerable<object> extraArgs)
        {
            InstallExplicit(installerType, InjectUtil.CreateArgList(extraArgs));
        }

        // See comment in IBinder.cs for description of this method
        public void InstallExplicit(Type installerType, List<TypeValuePair> extraArgs)
        {
            Assert.That(installerType.DerivesFrom<Installer>());

            FlushBindings();

            var installer = (IInstaller)InstantiateExplicit(installerType, extraArgs);
            InstallInstallerInternal(installer);
        }

#if !NOT_UNITY3D

        // See comment in IBinder.cs for description for this method
        public void InstallPrefabResource<T>()
            where T : MonoInstaller
        {
            InstallPrefabResource<T>(GetDefaultInstallerPrefabResourcePath<T>());
        }

        // See comment in IBinder.cs for description for this method
        public void InstallPrefabResource<T>(string resourcePath)
            where T : MonoInstaller
        {
            InstallPrefabResource<T>(resourcePath, new object[0]);
        }

        public void InstallPrefabResource<T>(IEnumerable<object> extraArgs)
            where T : MonoInstaller
        {
            InstallPrefabResource<T>(GetDefaultInstallerPrefabResourcePath<T>(), extraArgs);
        }

        public void InstallPrefabResource<T>(string resourcePath, IEnumerable<object> extraArgs)
            where T : MonoInstaller
        {
            InstallPrefabResource(typeof(T), resourcePath, extraArgs);
        }

        // See comment in IBinder.cs for description of this method
        public void InstallPrefabResource(Type installerType)
        {
            InstallPrefabResource(
                installerType, GetDefaultInstallerPrefabResourcePath(installerType));
        }

        // See comment in IBinder.cs for description of this method
        public void InstallPrefabResource(Type installerType, string resourcePath)
        {
            InstallPrefabResource(installerType, resourcePath, new object[0]);
        }

        public void InstallPrefabResource(Type installerType, IEnumerable<object> extraArgs)
        {
            InstallPrefabResource(
                installerType, GetDefaultInstallerPrefabResourcePath(installerType), extraArgs);
        }

        public void InstallPrefabResource(Type installerType, string resourcePath, IEnumerable<object> extraArgs)
        {
            InstallPrefabResourceExplicit(installerType, resourcePath, InjectUtil.CreateArgList(extraArgs));
        }

        string GetDefaultInstallerPrefabResourcePath<T>()
            where T : MonoInstaller
        {
            return GetDefaultInstallerPrefabResourcePath(typeof(T));
        }

        string GetDefaultInstallerPrefabResourcePath(Type installerType)
        {
            Assert.That(installerType.DerivesFrom<MonoInstaller>());
            return "Installers/" + installerType.Name();
        }

        public void InstallPrefabResourceExplicit(
            Type installerType, string resourcePath, List<TypeValuePair> extraArgs)
        {
            Assert.That(installerType.DerivesFrom<MonoInstaller>());

            var gameObj = CreateAndParentPrefabResource(resourcePath);

            var installer = gameObj.GetComponentInChildren<MonoInstaller>();

            InjectExplicit(installer, extraArgs);
            InstallInstallerInternal(installer);
        }

        // See comment in IBinder.cs for description for this method
        public void InstallScriptableObject<T>()
            where T : ScriptableObjectInstaller
        {
            InstallScriptableObject<T>(new object[0]);
        }

        public void InstallScriptableObject<T>(IEnumerable<object> extraArgs)
            where T : ScriptableObjectInstaller
        {
            InstallScriptableObject(typeof(T), extraArgs);
        }

        // See comment in IBinder.cs for description of this method
        public void InstallScriptableObject(Type installerType)
        {
            InstallScriptableObject(installerType, new object[0]);
        }

        public void InstallScriptableObject(Type installerType, IEnumerable<object> extraArgs)
        {
            InstallScriptableObjectExplicit(installerType, InjectUtil.CreateArgList(extraArgs));
        }

        public void InstallScriptableObjectExplicit(Type installerType, List<TypeValuePair> extraArgs)
        {
            Assert.That(installerType.DerivesFrom<ScriptableObjectInstaller>());

            var resourcePath = "Installers/" + installerType.Name();
            var installer = (ScriptableObjectInstaller)Resources.Load(resourcePath);

            Assert.IsNotNull(installer, "Expected to find Scriptable Object Installer at resource path '{0}'", resourcePath);

            InjectExplicit(installer, extraArgs);
            InstallInstallerInternal(installer);
        }
#endif

        // See comment in IBinder.cs for description of this method
        public bool HasInstalled<T>()
            where T : IInstaller
        {
            return HasInstalled(typeof(T));
        }

        // See comment in IBinder.cs for description of this method
        public bool HasInstalled(Type installerType)
        {
            FlushBindings();
            return _installedInstallers.Where(x => x == installerType).Any();
        }

        void InstallInstallerInternal(IInstaller installer)
        {
            var installerType = installer.GetType();

            Log.Debug("Installing installer '{0}'", installerType);

            Assert.That(!_installsInProgress.Contains(installerType),
                "Potential infinite loop detected while installing '{0}'", installerType.Name());

            Assert.That(!_installedInstallers.Contains(installerType),
                "Tried installing installer '{0}' twice", installerType.Name());

            _installedInstallers.Add(installerType);
            _installsInProgress.Push(installerType);

            try
            {
                installer.InstallBindings();
                FlushBindings();
            }
            catch (Exception e)
            {
                // This context information is really helpful when bind commands fail
                throw new Exception(
                    "Error occurred while running installer '{0}'".Fmt(installer.GetType().Name()), e);
            }
            finally
            {
                Assert.That(_installsInProgress.Peek().Equals(installerType));
                _installsInProgress.Pop();
            }
        }

        // Try looking up a single provider for a given context
        // Note that this method should not throw zenject exceptions
        internal ProviderLookupResult TryGetUniqueProvider(
            InjectContext context, out IProvider provider)
        {
            Assert.IsNotNull(context);

            // Note that different types can map to the same provider (eg. a base type to a concrete class and a concrete class to itself)
            var providers = GetProviderMatchesInternal(context).ToList();

            if (providers.IsEmpty())
            {
                provider = null;
                return ProviderLookupResult.None;
            }

            if (providers.Count > 1)
            {
                // If we find multiple providers and we are looking for just one, then
                // try to intelligently choose one from the list before giving up

                // First try picking the most 'local' dependencies
                // This will bias towards bindings for the lower level specific containers rather than the global high level container
                // This will, for example, allow you to just ask for a DiContainer dependency without needing to specify [Inject(Source = InjectSources.Local)]
                // (otherwise it would always match for a list of DiContainer's for all parent containers)
                var sortedProviders = providers.Select(x => new { Pair = x, Distance = GetContainerHeirarchyDistance(x.Container) }).OrderBy(x => x.Distance).ToList();

                sortedProviders.RemoveAll(x => x.Distance != sortedProviders[0].Distance);

                if (sortedProviders.Count == 1)
                {
                    // We have one match that is the closest
                    provider = sortedProviders[0].Pair.ProviderInfo.Provider;
                }
                else
                {
                    // Try choosing the one with a condition before giving up and throwing an exception
                    // This is nice because it allows us to bind a default and then override with conditions
                    provider = sortedProviders.Where(x => x.Pair.ProviderInfo.Condition != null).Select(x => x.Pair.ProviderInfo.Provider).OnlyOrDefault();

                    if (provider == null)
                    {
                        return ProviderLookupResult.Multiple;
                    }
                }
            }
            else
            {
                provider = providers.Single().ProviderInfo.Provider;
            }

            Assert.IsNotNull(provider);
            return ProviderLookupResult.Success;
        }

        // See comment in IResolver.cs for description of this method
        public object Resolve(InjectContext context)
        {
            Assert.IsNotNull(context);

            IProvider provider;

            FlushBindings();
            CheckForInstallWarning(context);

            var result = TryGetUniqueProvider(context, out provider);

            Assert.That(result != ProviderLookupResult.Multiple,
                "Found multiple matches when only one was expected for type '{0}'{1}. \nObject graph:\n {2}",
                context.MemberType.Name(),
                (context.ObjectType == null ? "" : " while building object with type '{0}'".Fmt(context.ObjectType.Name())),
                context.GetObjectGraphString());

            if (result == ProviderLookupResult.None)
            {
                // If it's a generic list then try matching multiple instances to its generic type
                if (ReflectionUtil.IsGenericList(context.MemberType))
                {
                    var subType = context.MemberType.GenericArguments().Single();

                    var subContext = context.Clone();
                    subContext.MemberType = subType;

                    return ResolveAll(subContext);
                }

                if (context.Optional)
                {
                    return context.FallBackValue;
                }

                throw Assert.CreateException("Unable to resolve type '{0}'{1}. \nObject graph:\n{2}",
                    context.MemberType.Name() + (context.Identifier == null ? "" : " with ID '{0}'".Fmt(context.Identifier.ToString())),
                    (context.ObjectType == null ? "" : " while building object with type '{0}'".Fmt(context.ObjectType.Name())),
                    context.GetObjectGraphString());
            }

            Assert.That(result == ProviderLookupResult.Success);
            Assert.IsNotNull(provider);

            var instances = SafeGetInstances(provider, context);

            Assert.That(!instances.IsEmpty(), "Provider returned zero instances when one was expected!");
            Assert.That(instances.Count() == 1, "Provider returned multiple instances when one was expected!");

            return instances.First();
        }

        IEnumerable<object> SafeGetInstances(IProvider provider, InjectContext context)
        {
            Assert.IsNotNull(context);

            if (ChecksForCircularDependencies)
            {
                var lookupId = new LookupId(provider, context.GetBindingId());

                // Allow one before giving up so that you can do circular dependencies via postinject or fields
                Assert.That(_resolvesInProgress.Where(x => x.Equals(lookupId)).Count() <= 1,
                    "Circular dependency detected! \nObject graph:\n {0}", context.GetObjectGraphString());

                _resolvesInProgress.Push(lookupId);
                try
                {
                    return provider.GetAllInstances(context);
                }
                finally
                {
                    Assert.That(_resolvesInProgress.Peek().Equals(lookupId));
                    _resolvesInProgress.Pop();
                }
            }
            else
            {
                return provider.GetAllInstances(context);
            }
        }

        int GetContainerHeirarchyDistance(DiContainer container)
        {
            return GetContainerHeirarchyDistance(container, 0);
        }

        int GetContainerHeirarchyDistance(DiContainer container, int depth)
        {
            if (container == this)
            {
                return depth;
            }

            Assert.IsNotNull(_parentContainer);
            return _parentContainer.GetContainerHeirarchyDistance(container, depth + 1);
        }

        public IEnumerable<Type> GetDependencyContracts<TContract>()
        {
            return GetDependencyContracts(typeof(TContract));
        }

        public IEnumerable<Type> GetDependencyContracts(Type contract)
        {
            FlushBindings();

            foreach (var injectMember in TypeAnalyzer.GetInfo(contract).AllInjectables)
            {
                yield return injectMember.MemberType;
            }
        }

        public T InstantiateExplicit<T>(List<TypeValuePair> extraArgs)
        {
            return (T)InstantiateExplicit(typeof(T), extraArgs);
        }

        public object InstantiateExplicit(Type concreteType, List<TypeValuePair> extraArgs)
        {
            bool autoInject = true;

            return InstantiateExplicit(
                concreteType,
                autoInject,
                new InjectArgs()
                {
                    ExtraArgs = extraArgs,
                    Context = new InjectContext(this, concreteType, null),
                    ConcreteIdentifier = null,
                    UseAllArgs = true,
                });
        }

        // See comment in IInstantiator.cs for description of this method
        public object InstantiateExplicit(Type concreteType, bool autoInject, InjectArgs args)
        {
#if PROFILING_ENABLED
            using (ProfileBlock.Start("Zenject.Instantiate({0})", concreteType))
#endif
            {
                return InstantiateInternal(concreteType, autoInject, args);
            }
        }

        public static bool CanCreateOrInjectDuringValidation(Type type)
        {
            // During validation, do not instantiate or inject anything except for
            // Installers, IValidatable's, or types marked with attribute ZenjectAllowDuringValidation
            // You would typically use ZenjectAllowDuringValidation attribute for data that you
            // inject into factories
            return type.DerivesFrom<IInstaller>()
                || type.DerivesFrom<IValidatable>()
#if !NOT_UNITY3D
                || type.DerivesFrom<Context>()
                || type.DerivesFrom<DecoratorInstaller>()
#endif
#if !(UNITY_WSA && ENABLE_DOTNET)
                || type.HasAttribute<ZenjectAllowDuringValidationAttribute>()
#endif
            ;
        }

        object InstantiateInternal(Type concreteType, bool autoInject, InjectArgs args)
        {
#if !NOT_UNITY3D
            Assert.That(!concreteType.DerivesFrom<UnityEngine.Component>(),
                "Error occurred while instantiating object of type '{0}'. Instantiator should not be used to create new mono behaviours.  Must use InstantiatePrefabForComponent, InstantiatePrefab, or InstantiateComponent.", concreteType.Name());
#endif

            Assert.That(!concreteType.IsAbstract(), "Expected type 'type' to be non-abstract", concreteType);

            FlushBindings();
            CheckForInstallWarning(args.Context);

            var typeInfo = TypeAnalyzer.GetInfo(concreteType);

            object newObj;

#if !NOT_UNITY3D
            if (concreteType.DerivesFrom<ScriptableObject>())
            {
                Assert.That( typeInfo.ConstructorInjectables.IsEmpty(),
                    "Found constructor parameters on ScriptableObject type '{0}'.  This is not allowed.  Use an [Inject] method or fields instead.");

                if (!IsValidating || CanCreateOrInjectDuringValidation(concreteType))
                {
                    newObj = ScriptableObject.CreateInstance(concreteType);
                }
                else
                {
                    newObj = new ValidationMarker(concreteType);
                }
            }
            else
#endif
            {
                Assert.IsNotNull(typeInfo.InjectConstructor,
                    "More than one (or zero) constructors found for type '{0}' when creating dependencies.  Use one [Inject] attribute to specify which to use.", concreteType);

                // Make a copy since we remove from it below
                var paramValues = new List<object>();

                foreach (var injectInfo in typeInfo.ConstructorInjectables)
                {
                    object value;

                    if (!InjectUtil.PopValueWithType(
                        args.ExtraArgs, injectInfo.MemberType, out value))
                    {
                        value = Resolve(injectInfo.CreateInjectContext(
                            this, args.Context, null, args.ConcreteIdentifier));
                    }

                    if (value is ValidationMarker)
                    {
                        Assert.That(IsValidating);
                        paramValues.Add(injectInfo.MemberType.GetDefaultValue());
                    }
                    else
                    {
                        paramValues.Add(value);
                    }
                }

                if (!IsValidating || CanCreateOrInjectDuringValidation(concreteType))
                {
                    //Log.Debug("Zenject: Instantiating type '{0}'", concreteType.Name());
                    try
                    {
#if PROFILING_ENABLED
                        using (ProfileBlock.Start("{0}.{0}()", concreteType))
#endif
                        {
                            newObj = typeInfo.InjectConstructor.Invoke(paramValues.ToArray());
                        }
                    }
                    catch (Exception e)
                    {
                        throw Assert.CreateException(
                            e, "Error occurred while instantiating object with type '{0}'", concreteType.Name());
                    }
                }
                else
                {
                    newObj = new ValidationMarker(concreteType);
                }
            }

            if (autoInject)
            {
                InjectExplicit(newObj, concreteType, args);
            }
            else if (args.UseAllArgs && !args.ExtraArgs.IsEmpty())
            {
                throw Assert.CreateException(
                    "Passed unnecessary parameters when injecting into type '{0}'. \nExtra Parameters: {1}\nObject graph:\n{2}",
                    newObj.GetType().Name(), String.Join(",", args.ExtraArgs.Select(x => x.Type.Name()).ToArray()), args.Context.GetObjectGraphString());
            }

            return newObj;
        }

        public void InjectExplicit(object injectable, List<TypeValuePair> extraArgs)
        {
            Type injectableType;

            if (injectable is ValidationMarker)
            {
                injectableType = ((ValidationMarker)injectable).MarkedType;
            }
            else
            {
                injectableType = injectable.GetType();
            }

            InjectExplicit(
                injectable,
                injectableType,
                new InjectArgs()
                {
                    ExtraArgs = extraArgs,
                    UseAllArgs = true,
                    Context = new InjectContext(this, injectableType, null),
                    ConcreteIdentifier = null,
                });
        }

        // See comment in IResolver.cs for description of this method
        public void InjectExplicit(
            object injectable, Type injectableType, InjectArgs args)
        {
            Assert.That(injectable != null);

            // Installers are the only things that we instantiate/inject on during validation
            bool isDryRun = IsValidating && !CanCreateOrInjectDuringValidation(injectableType);

            if (!isDryRun)
            {
                Assert.IsEqual(injectable.GetType(), injectableType);
            }

#if !NOT_UNITY3D
            Assert.That(injectableType != typeof(GameObject),
                "Use InjectGameObject to Inject game objects instead of Inject method");
#endif

            FlushBindings();
            CheckForInstallWarning(args.Context);

            var typeInfo = TypeAnalyzer.GetInfo(injectableType);

            foreach (var injectInfo in typeInfo.FieldInjectables.Concat(
                typeInfo.PropertyInjectables))
            {
                object value;

                if (InjectUtil.PopValueWithType(args.ExtraArgs, injectInfo.MemberType, out value))
                {
                    if (!isDryRun)
                    {
                        if (value is ValidationMarker)
                        {
                            Assert.That(IsValidating);
                        }
                        else
                        {
                            injectInfo.Setter(injectable, value);
                        }
                    }
                }
                else
                {
                    value = Resolve(
                        injectInfo.CreateInjectContext(
                            this, args.Context, injectable, args.ConcreteIdentifier));

                    if (injectInfo.Optional && value == null)
                    {
                        // Do not override in this case so it retains the hard-coded value
                    }
                    else
                    {
                        if (!isDryRun)
                        {
                            if (value is ValidationMarker)
                            {
                                Assert.That(IsValidating);
                            }
                            else
                            {
                                injectInfo.Setter(injectable, value);
                            }
                        }
                    }
                }
            }

            foreach (var method in typeInfo.PostInjectMethods)
            {
#if PROFILING_ENABLED
                using (ProfileBlock.Start("{0}.{1}()", injectableType, method.MethodInfo.Name))
#endif
                {
                    var paramValues = new List<object>();

                    foreach (var injectInfo in method.InjectableInfo)
                    {
                        object value;

                        if (!InjectUtil.PopValueWithType(args.ExtraArgs, injectInfo.MemberType, out value))
                        {
                            value = Resolve(
                                injectInfo.CreateInjectContext(this, args.Context, injectable, args.ConcreteIdentifier));
                        }

                        if (value is ValidationMarker)
                        {
                            Assert.That(IsValidating);
                            paramValues.Add(injectInfo.MemberType.GetDefaultValue());
                        }
                        else
                        {
                            paramValues.Add(value);
                        }
                    }

                    if (!isDryRun)
                    {
                        method.MethodInfo.Invoke(injectable, paramValues.ToArray());
                    }
                }
            }

            if (args.UseAllArgs && !args.ExtraArgs.IsEmpty())
            {
                throw Assert.CreateException(
                    "Passed unnecessary parameters when injecting into type '{0}'. \nExtra Parameters: {1}\nObject graph:\n{2}",
                    injectableType.Name(), String.Join(",", args.ExtraArgs.Select(x => x.Type.Name()).ToArray()), args.Context.GetObjectGraphString());
            }
        }

#if !NOT_UNITY3D

        public GameObject InstantiatePrefabResourceExplicit(
            string resourcePath, List<TypeValuePair> extraArgs)
        {
            return InstantiatePrefabResourceExplicit(
                resourcePath, extraArgs, null);
        }

        public GameObject InstantiatePrefabResourceExplicit(
            string resourcePath, List<TypeValuePair> extraArgs, string groupName)
        {
            return InstantiatePrefabResourceExplicit(
                resourcePath, extraArgs, groupName, true);
        }

        public GameObject InstantiatePrefabResourceExplicit(
            string resourcePath, List<TypeValuePair> extraArgs,
            string groupName, bool useAllArgs)
        {
            var prefab = (GameObject)Resources.Load(resourcePath);
            Assert.IsNotNull(prefab, "Could not find prefab at resource location '{0}'".Fmt(resourcePath));

            return InstantiatePrefabExplicit(
                prefab, extraArgs, groupName, useAllArgs);
        }

        public GameObject InstantiatePrefabExplicit(
            UnityEngine.Object prefab, List<TypeValuePair> extraArgs)
        {
            return InstantiatePrefabExplicit(
                prefab, extraArgs, null);
        }

        public GameObject InstantiatePrefabExplicit(
            UnityEngine.Object prefab, List<TypeValuePair> extraArgs,
            string groupName)
        {
            return InstantiatePrefabExplicit(
                prefab, extraArgs, groupName, true);
        }

        public GameObject InstantiatePrefabExplicit(
            UnityEngine.Object prefab, List<TypeValuePair> extraArgs,
            string groupName, bool useAllArgs)
        {
            FlushBindings();

            var gameObj = CreateAndParentPrefab(prefab, groupName);

            InjectGameObjectExplicit(
                gameObj, true, extraArgs, useAllArgs);

            return gameObj;
        }

        // Don't use this unless you know what you're doing
        // You probably want to use InstantiatePrefab instead
        // This one will only create the prefab and will not inject into it
        public GameObject CreateAndParentPrefabResource(string resourcePath)
        {
            return CreateAndParentPrefabResource(resourcePath, null);
        }

        // Don't use this unless you know what you're doing
        // You probably want to use InstantiatePrefab instead
        // This one will only create the prefab and will not inject into it
        public GameObject CreateAndParentPrefabResource(string resourcePath, string groupName)
        {
            var prefab = (GameObject)Resources.Load(resourcePath);

            Assert.IsNotNull(prefab,
                "Could not find prefab at resource location '{0}'".Fmt(resourcePath));

            return CreateAndParentPrefab(prefab, groupName);
        }

        GameObject GetPrefabAsGameObject(UnityEngine.Object prefab)
        {
            if (prefab is GameObject)
            {
                return (GameObject)prefab;
            }

            Assert.That(prefab is Component, "Invalid type given for prefab. Given object name: '{0}'", prefab.name);
            return ((Component)prefab).gameObject;
        }

        // Don't use this unless you know what you're doing
        // You probably want to use InstantiatePrefab instead
        // This one will only create the prefab and will not inject into it
        public GameObject CreateAndParentPrefab(UnityEngine.Object prefab, string groupName)
        {
            Assert.That(!AssertOnNewGameObjects,
                "Given DiContainer does not support creating new game objects");

            FlushBindings();

            var prefabAsGameObject = GetPrefabAsGameObject(prefab);

            GameObject gameObj;

            if (IsValidating)
            {
                // We need to avoid triggering any Awake() method during validation
                // so temporarily disable the prefab so that the object gets
                // instantiated as disabled
                bool wasActive = prefabAsGameObject.activeSelf;

                prefabAsGameObject.SetActive(false);

                try
                {
                    gameObj = (GameObject)GameObject.Instantiate(prefabAsGameObject);
                }
                finally
                {
                    prefabAsGameObject.SetActive(wasActive);
                }
            }
            else
            {
                gameObj = (GameObject)GameObject.Instantiate(prefabAsGameObject);
            }

            gameObj.transform.SetParent(GetTransformGroup(groupName), false);
            return gameObj;
        }

        public GameObject CreateEmptyGameObject(string name)
        {
            return CreateEmptyGameObject(name, null);
        }

        public GameObject CreateEmptyGameObject(string name, string groupName)
        {
            Assert.That(!AssertOnNewGameObjects,
                "Given DiContainer does not support creating new game objects");

            FlushBindings();

            var gameObj = new GameObject(name);
            gameObj.transform.SetParent(GetTransformGroup(groupName), false);
            return gameObj;
        }

        public T InstantiatePrefabForComponentExplicit<T>(
            UnityEngine.Object prefab, List<TypeValuePair> extraArgs)
        {
            return (T)InstantiatePrefabForComponentExplicit(
                typeof(T), prefab, extraArgs);
        }

        // Note: Any arguments that are used will be removed from extraArgs
        public object InstantiatePrefabForComponentExplicit(
            Type componentType, UnityEngine.Object prefab, List<TypeValuePair> extraArgs)
        {
            return InstantiatePrefabForComponentExplicit(
                componentType, prefab, extraArgs, null);
        }

        public object InstantiatePrefabForComponentExplicit(
            Type componentType, UnityEngine.Object prefab, List<TypeValuePair> extraArgs,
            string groupName)
        {
            return InstantiatePrefabForComponentExplicit(
                componentType, prefab, groupName,
                new InjectArgs()
                {
                    ExtraArgs = extraArgs,
                    Context = new InjectContext(this, componentType, null),
                    ConcreteIdentifier = null,
                    UseAllArgs = true,
                });
        }

        // Note: Any arguments that are used will be removed from extraArgs
        public object InstantiatePrefabForComponentExplicit(
            Type componentType, UnityEngine.Object prefab, string groupName, InjectArgs args)
        {
            Assert.That(!AssertOnNewGameObjects,
                "Given DiContainer does not support creating new game objects");

            FlushBindings();

            Assert.That(prefab != null, "Null prefab found when instantiating game object");

            Assert.That(componentType.IsInterface() || componentType.DerivesFrom<Component>(),
                "Expected type '{0}' to derive from UnityEngine.Component", componentType.Name());

            var gameObj = (GameObject)GameObject.Instantiate(GetPrefabAsGameObject(prefab));

            gameObj.transform.SetParent(GetTransformGroup(groupName), false);
            gameObj.SetActive(true);

            return InjectGameObjectForComponentExplicit(
                gameObj, componentType, args);
        }

        Transform GetTransformGroup(string groupName)
        {
            Assert.That(!AssertOnNewGameObjects,
                "Given DiContainer does not support creating new game objects");

            if (DefaultParent == null)
            {
                if (groupName == null)
                {
                    return null;
                }

                return (GameObject.Find("/" + groupName) ?? new GameObject(groupName)).transform;
            }

            if (groupName == null)
            {
                return DefaultParent;
            }

            foreach (Transform child in DefaultParent)
            {
                if (child.name == groupName)
                {
                    return child;
                }
            }

            var group = new GameObject(groupName).transform;
            group.SetParent(DefaultParent, false);
            return group;
        }

#endif

        public T Instantiate<T>()
        {
            return Instantiate<T>(new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public T Instantiate<T>(IEnumerable<object> extraArgs)
        {
            var result = Instantiate(typeof(T), extraArgs);

            if (IsValidating && !(result is T))
            {
                Assert.That(result is ValidationMarker);
                return default(T);
            }

            return (T)result;
        }

        public object Instantiate(Type concreteType)
        {
            return Instantiate(concreteType, new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public object Instantiate(
            Type concreteType, IEnumerable<object> extraArgs)
        {
            Assert.That(!extraArgs.ContainsItem(null),
                "Null value given to factory constructor arguments when instantiating object with type '{0}'. In order to use null use InstantiateExplicit", concreteType);

            return InstantiateExplicit(
                concreteType, InjectUtil.CreateArgList(extraArgs));
        }

#if !NOT_UNITY3D
        // See comment in IInstantiator.cs for description of this method
        public TContract InstantiateComponent<TContract>(GameObject gameObject)
            where TContract : Component
        {
            return InstantiateComponent<TContract>(gameObject, new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public TContract InstantiateComponent<TContract>(
            GameObject gameObject, IEnumerable<object> extraArgs)
            where TContract : Component
        {
            return (TContract)InstantiateComponent(typeof(TContract), gameObject, extraArgs);
        }

        public Component InstantiateComponent(
            Type componentType, GameObject gameObject)
        {
            return InstantiateComponent(componentType, gameObject, new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public Component InstantiateComponent(
            Type componentType, GameObject gameObject, IEnumerable<object> extraArgs)
        {
            return InstantiateComponentExplicit(
                componentType, gameObject, InjectUtil.CreateArgList(extraArgs));
        }

        public Component InstantiateComponentExplicit(
            Type componentType, GameObject gameObject, List<TypeValuePair> extraArgs)
        {
            Assert.That(componentType.DerivesFrom<Component>());

            FlushBindings();

            var monoBehaviour = (Component)gameObject.AddComponent(componentType);
            InjectExplicit(monoBehaviour, extraArgs);
            return monoBehaviour;
        }

        public GameObject InstantiatePrefab(UnityEngine.Object prefab)
        {
            return InstantiatePrefab(prefab, new object[0]);
        }

        public GameObject InstantiatePrefab(
            UnityEngine.Object prefab, IEnumerable<object> extraArgs)
        {
            return InstantiatePrefab(
                prefab, extraArgs, null);
        }

        public GameObject InstantiatePrefab(
            UnityEngine.Object prefab, IEnumerable<object> extraArgs, string groupName)
        {
            return InstantiatePrefabExplicit(
                prefab, InjectUtil.CreateArgList(extraArgs),
                groupName);
        }

        // See comment in IInstantiator.cs for description of this method
        public GameObject InstantiatePrefabResource(string resourcePath)
        {
            return InstantiatePrefabResource(resourcePath, new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public GameObject InstantiatePrefabResource(
            string resourcePath, IEnumerable<object> extraArgs)
        {
            return InstantiatePrefabResource(
                resourcePath, extraArgs, null);
        }

        // See comment in IInstantiator.cs for description of this method
        public GameObject InstantiatePrefabResource(
            string resourcePath, IEnumerable<object> extraArgs, string groupName)
        {
            return InstantiatePrefabResourceExplicit(
                resourcePath, InjectUtil.CreateArgList(extraArgs),
                groupName);
        }

        /////////////// InstantiatePrefabForComponent

        public T InstantiatePrefabForComponent<T>(UnityEngine.Object prefab)
        {
            return InstantiatePrefabForComponent<T>(prefab, new object[0]);
        }

        public T InstantiatePrefabForComponent<T>(
            UnityEngine.Object prefab, IEnumerable<object> extraArgs)
        {
            return (T)InstantiatePrefabForComponent(
                typeof(T), prefab, extraArgs);
        }

        public object InstantiatePrefabForComponent(
            Type concreteType, UnityEngine.Object prefab, IEnumerable<object> extraArgs)
        {
            return InstantiatePrefabForComponentExplicit(
                concreteType, prefab,
                InjectUtil.CreateArgList(extraArgs));
        }

        /////////////// InstantiatePrefabForComponent

        // See comment in IInstantiator.cs for description of this method
        public T InstantiatePrefabResourceForComponent<T>(string resourcePath)
        {
            return InstantiatePrefabResourceForComponent<T>(resourcePath, new object[0]);
        }

        // See comment in IInstantiator.cs for description of this method
        public T InstantiatePrefabResourceForComponent<T>(
            string resourcePath, IEnumerable<object> extraArgs)
        {
            return (T)InstantiatePrefabResourceForComponent(typeof(T), resourcePath, extraArgs);
        }

        // See comment in IInstantiator.cs for description of this method
        public object InstantiatePrefabResourceForComponent(
            Type concreteType, string resourcePath, IEnumerable<object> extraArgs)
        {
            Assert.That(!extraArgs.ContainsItem(null),
            "Null value given to factory constructor arguments when instantiating object with type '{0}'. In order to use null use InstantiatePrefabForComponentExplicit", concreteType);

            return InstantiatePrefabResourceForComponentExplicit(
                concreteType, resourcePath, InjectUtil.CreateArgList(extraArgs));
        }

        public T InstantiatePrefabResourceForComponentExplicit<T>(
            string resourcePath, List<TypeValuePair> extraArgs)
        {
            return (T)InstantiatePrefabResourceForComponentExplicit(
                typeof(T), resourcePath, extraArgs);
        }

        // Note: Any arguments that are used will be removed from extraArgs
        public object InstantiatePrefabResourceForComponentExplicit(
            Type componentType, string resourcePath, List<TypeValuePair> extraArgs)
        {
            return InstantiatePrefabResourceForComponentExplicit(
                componentType, resourcePath, null,
                new InjectArgs()
                {
                    ExtraArgs = extraArgs,
                    Context = new InjectContext(this, componentType, null),
                    ConcreteIdentifier = null,
                    UseAllArgs = true,
                });
        }

        // Note: Any arguments that are used will be removed from extraArgs
        public object InstantiatePrefabResourceForComponentExplicit(
            Type componentType, string resourcePath, string groupName, InjectArgs args)
        {
            var prefab = (GameObject)Resources.Load(resourcePath);
            Assert.IsNotNull(prefab,
                "Could not find prefab at resource location '{0}'".Fmt(resourcePath));
            return InstantiatePrefabForComponentExplicit(
                componentType, prefab, groupName, args);
        }

#endif

        ////////////// Convenience methods for IResolver ////////////////

#if !NOT_UNITY3D
        // See comment in IResolver.cs for description of this method
        public void InjectGameObject(
            GameObject gameObject)
        {
            InjectGameObject(gameObject, true);
        }

        // See comment in IResolver.cs for description of this method
        public void InjectGameObject(
            GameObject gameObject, bool recursive)
        {
            InjectGameObject(gameObject, recursive, new object[0]);
        }

        // See comment in IResolver.cs for description of this method
        public void InjectGameObject(
            GameObject gameObject, bool recursive, IEnumerable<object> extraArgs)
        {
            InjectGameObjectExplicit(
                gameObject, recursive,
                InjectUtil.CreateArgList(extraArgs));
        }

        public void InjectGameObjectExplicit(
            GameObject gameObject, bool recursive,
            List<TypeValuePair> extraArgs)
        {
            // We have to pass in a null InjectContext here because we aren't
            // asking for any particular type
            InjectGameObjectExplicit(
                gameObject, recursive, extraArgs, true);
        }

        // See comment in IResolver.cs for description of this method
        public void InjectGameObjectExplicit(
            GameObject gameObject, bool recursive,
            List<TypeValuePair> extraArgs, bool useAllArgs)
        {
            FlushBindings();

            // Inject on the children first since the parent objects are more likely to use them in their post inject methods
            foreach (var component in ZenUtilInternal.GetInjectableComponentsBottomUp(
                gameObject, recursive).ToList())
            {
                if (component == null)
                {
                    Log.Warn("Found null component while injecting game object '{0}'.  Possible missing script.", gameObject.name);
                    continue;
                }

                if (component.GetType().DerivesFrom<MonoInstaller>())
                {
                    // Do not inject on installers since these are always injected before they are installed
                    continue;
                }

                InjectExplicit(component, component.GetType(),
                    new InjectArgs()
                    {
                        ExtraArgs = extraArgs,
                        Context = new InjectContext(this, component.GetType()),
                        ConcreteIdentifier = null,
                        UseAllArgs = false,
                    });
            }

            if (useAllArgs && !extraArgs.IsEmpty())
            {
                throw Assert.CreateException(
                    "Passed unnecessary parameters when injecting into game object '{0}'. \nExtra Parameters: {1}",
                    gameObject.name, string.Join(",", extraArgs.Select(x => x.Type.Name()).ToArray()));
            }
        }

        public Component InjectGameObjectForComponentExplicit(
            GameObject gameObject, Type componentType,
            InjectArgs args)
        {
            Component requestedScript = null;

            // Inject on the children first since the parent objects are more likely to use them in their post inject methods
            foreach (var component in ZenUtilInternal.GetInjectableComponentsBottomUp(
                gameObject, true).ToList())
            {
                if (component == null)
                {
                    Log.Warn("Found null component while injecting into game object '{0}'.  Possible missing script.", gameObject.name);
                    continue;
                }

                if (component.GetType().DerivesFrom<MonoInstaller>())
                {
                    // Do not inject on installers since these are always injected before they are installed
                    continue;
                }

                if (component.GetType().DerivesFromOrEqual(componentType))
                {
                    Assert.IsNull(requestedScript,
                        "Found multiple matches with type '{0}' when injecting into game object '{1}'", componentType, gameObject.name);
                    requestedScript = component;

                    InjectExplicit(component, component.GetType(), args);
                }
                else
                {
                    Inject(component);
                }
            }

            if (requestedScript == null)
            {
                throw Assert.CreateException(
                    "Could not find component with type '{0}' when instantiating new game object", componentType);
            }

            return requestedScript;
        }
#endif

        // See comment in IResolver.cs for description of this method
        public void Inject(object injectable)
        {
            Inject(injectable, new object[0]);
        }

        // See comment in IResolver.cs for description of this method
        public void Inject(object injectable, IEnumerable<object> extraArgs)
        {
            InjectExplicit(
                injectable, InjectUtil.CreateArgList(extraArgs));
        }

        // See comment in IResolver.cs for description of this method
        public List<Type> ResolveTypeAll(Type type)
        {
            return ResolveTypeAll(new InjectContext(this, type, null));
        }

        // See comment in IResolver.cs for description of this method
        public TContract Resolve<TContract>()
        {
            return Resolve<TContract>((string)null);
        }

        // See comment in IResolver.cs for description of this method
        public TContract Resolve<TContract>(object identifier)
        {
            return Resolve<TContract>(new InjectContext(this, typeof(TContract), identifier));
        }

        // See comment in IResolver.cs for description of this method
        public TContract TryResolve<TContract>()
            where TContract : class
        {
            return TryResolve<TContract>((string)null);
        }

        // See comment in IResolver.cs for description of this method
        public TContract TryResolve<TContract>(object identifier)
            where TContract : class
        {
            return (TContract)TryResolve(typeof(TContract), identifier);
        }

        // See comment in IResolver.cs for description of this method
        public object TryResolve(Type contractType)
        {
            return TryResolve(contractType, null);
        }

        // See comment in IResolver.cs for description of this method
        public object TryResolve(Type contractType, object identifier)
        {
            return Resolve(new InjectContext(this, contractType, identifier, true));
        }

        // See comment in IResolver.cs for description of this method
        public object Resolve(Type contractType)
        {
            return Resolve(new InjectContext(this, contractType, null));
        }

        // See comment in IResolver.cs for description of this method
        public object Resolve(Type contractType, object identifier)
        {
            return Resolve(new InjectContext(this, contractType, identifier));
        }

        // See comment in IResolver.cs for description of this method
        public TContract Resolve<TContract>(InjectContext context)
        {
            Assert.IsNotNull(context);

            Assert.IsEqual(context.MemberType, typeof(TContract));
            return (TContract) Resolve(context);
        }

        // See comment in IResolver.cs for description of this method
        public List<TContract> ResolveAll<TContract>()
        {
            return ResolveAll<TContract>((string)null);
        }

        // See comment in IResolver.cs for description of this method
        public List<TContract> ResolveAll<TContract>(bool optional)
        {
            return ResolveAll<TContract>(null, optional);
        }

        // See comment in IResolver.cs for description of this method
        public List<TContract> ResolveAll<TContract>(object identifier)
        {
            return ResolveAll<TContract>(identifier, true);
        }

        // See comment in IResolver.cs for description of this method
        public List<TContract> ResolveAll<TContract>(object identifier, bool optional)
        {
            var context = new InjectContext(this, typeof(TContract), identifier, optional);
            return ResolveAll<TContract>(context);
        }

        // See comment in IResolver.cs for description of this method
        public List<TContract> ResolveAll<TContract>(InjectContext context)
        {
            Assert.IsNotNull(context);
            Assert.IsEqual(context.MemberType, typeof(TContract));
            return (List<TContract>) ResolveAll(context);
        }

        // See comment in IResolver.cs for description of this method
        public IList ResolveAll(Type contractType)
        {
            return ResolveAll(contractType, null);
        }

        // See comment in IResolver.cs for description of this method
        public IList ResolveAll(Type contractType, object identifier)
        {
            return ResolveAll(contractType, identifier, true);
        }

        // See comment in IResolver.cs for description of this method
        public IList ResolveAll(Type contractType, bool optional)
        {
            return ResolveAll(contractType, null, optional);
        }

        // See comment in IResolver.cs for description of this method
        public IList ResolveAll(Type contractType, object identifier, bool optional)
        {
            var context = new InjectContext(this, contractType, identifier, optional);
            return ResolveAll(context);
        }

        ////////////// IBinder ////////////////

        public void UnbindAll()
        {
            FlushBindings();
            _providers.Clear();
        }

        public bool Unbind<TContract>()
        {
            return Unbind<TContract>(null);
        }

        public bool Unbind<TContract>(object identifier)
        {
            FlushBindings();

            var bindingId = new BindingId(typeof(TContract), identifier);

            return _providers.Remove(bindingId);
        }

        // See comment in IBinder.cs for description of this method
        public bool HasBinding(InjectContext context)
        {
            Assert.IsNotNull(context);

            FlushBindings();

            List<ProviderInfo> providers;

            if (!_providers.TryGetValue(context.GetBindingId(), out providers))
            {
                return false;
            }

            return providers.Where(x => x.Condition == null || x.Condition(context)).HasAtLeast(1);
        }

        // See comment in IBinder.cs for description of this method
        public bool HasBinding<TContract>()
        {
            return HasBinding<TContract>(null);
        }

        // See comment in IBinder.cs for description of this method
        public bool HasBinding<TContract>(object identifier)
        {
            return HasBinding(
                new InjectContext(this, typeof(TContract), identifier));
        }

        // Do not use this - it is for internal use only
        public void FlushBindings()
        {
            while (!_currentBindings.IsEmpty())
            {
                var binding = _currentBindings.Dequeue();

                binding.FinalizeBinding(this);

                _processedBindings.Add(binding);
            }
        }

        public BindFinalizerWrapper StartBinding()
        {
            FlushBindings();

            var bindingFinalizer = new BindFinalizerWrapper();
            _currentBindings.Enqueue(bindingFinalizer);
            return bindingFinalizer;
        }

        public ConcreteBinderGeneric<TContract> Rebind<TContract>()
        {
            return Rebind<TContract>(null);
        }

        public ConcreteBinderGeneric<TContract> Rebind<TContract>(object identifier)
        {
            Unbind<TContract>(identifier);
            return Bind<TContract>().WithId(identifier);
        }

        public ConcreteIdBinderGeneric<TContract> Bind<TContract>()
        {
            Assert.That(!typeof(TContract).DerivesFrom<IDynamicFactory>(),
                "You should not use Container.Bind for factory classes.  Use Container.BindFactory instead.");

            var bindInfo = new BindInfo(typeof(TContract));

            return new ConcreteIdBinderGeneric<TContract>(
                bindInfo, StartBinding());
        }

        public ConcreteIdBinderNonGeneric Bind(params Type[] contractTypes)
        {
            return Bind((IEnumerable<Type>)contractTypes);
        }

        public ConcreteIdBinderNonGeneric Bind(IEnumerable<Type> contractTypes)
        {
            var contractTypesList = contractTypes.ToList();
            Assert.That(contractTypesList.All(x => !x.DerivesFrom<IDynamicFactory>()),
                "You should not use Container.Bind for factory classes.  Use Container.BindFactory instead.");

            var bindInfo = new BindInfo(contractTypesList);
            return new ConcreteIdBinderNonGeneric(bindInfo, StartBinding());
        }

#if !(UNITY_WSA && ENABLE_DOTNET)
        public ConcreteIdBinderNonGeneric Bind(
            Action<ConventionSelectTypesBinder> generator)
        {
            var conventionBindInfo = new ConventionBindInfo();
            generator(new ConventionSelectTypesBinder(conventionBindInfo));

            var contractTypesList = conventionBindInfo.ResolveTypes();

            Assert.That(contractTypesList.All(x => !x.DerivesFrom<IDynamicFactory>()),
                "You should not use Container.Bind for factory classes.  Use Container.BindFactory instead.");

            var bindInfo = new BindInfo(contractTypesList);

            // This is nice because it allows us to do things like Bind(all interfaces).To<Foo>()
            // (though of course it would be more efficient to use BindAllInterfaces in this case)
            bindInfo.InvalidBindResponse = InvalidBindResponses.Skip;

            return new ConcreteIdBinderNonGeneric(bindInfo, StartBinding());
        }
#endif

        // This is equivalent to calling NonLazy() at the end of your bind statement
        // It's only in rare cases where you need to call this instead of NonLazy()
        public void BindRootResolve<TContract>()
        {
            BindRootResolve<TContract>(null);
        }

        public void BindRootResolve<TContract>(object identifier)
        {
            BindRootResolve(identifier, new Type[] { typeof(TContract) });
        }

        public void BindRootResolve(IEnumerable<Type> rootTypes)
        {
            BindRootResolve(null, rootTypes);
        }

        public void BindRootResolve(object identifier, IEnumerable<Type> rootTypes)
        {
            Bind<object>().WithId(DependencyRootIdentifier).To(rootTypes).FromResolve(identifier);
        }

        public ConcreteIdBinderNonGeneric BindAllInterfaces<T>()
        {
            return BindAllInterfaces(typeof(T));
        }

        public ConcreteIdBinderNonGeneric BindAllInterfaces(Type type)
        {
            // We must only have one dependency root per container
            // We need this when calling this with a GameObjectContext
            return Bind(type.Interfaces().ToArray());
        }

        public ConcreteIdBinderNonGeneric BindAllInterfacesAndSelf<T>()
        {
            return BindAllInterfacesAndSelf(typeof(T));
        }

        public ConcreteIdBinderNonGeneric BindAllInterfacesAndSelf(Type type)
        {
            // We must only have one dependency root per container
            // We need this when calling this with a GameObjectContext
            return Bind(
                type.Interfaces().Append(type).ToArray());
        }

        public IdScopeBinder BindInstance<TContract>(TContract instance)
        {
            return BindInstance<TContract>(instance, false);
        }

        public IdScopeBinder BindInstance<TContract>(TContract instance, bool allowNull)
        {
            if (!allowNull)
            {
                Assert.That(!ZenUtilInternal.IsNull(instance),
                    "Found null instance with type '{0}' in BindInstance method", typeof(TContract).Name());
            }

            var bindInfo = new BindInfo(typeof(TContract));
            var binding = StartBinding();

            binding.SubFinalizer = new ScopableBindingFinalizer(
                bindInfo, SingletonTypes.ToInstance, instance,
                (_, type) => new InstanceProvider(type, instance));

            return new IdScopeBinder(bindInfo);
        }

        public FactoryToChoiceIdBinder<TContract> BindIFactory<TContract>()
        {
            var bindInfo = new BindInfo(typeof(IFactory<TContract>));
            return new FactoryToChoiceIdBinder<TContract>(
                bindInfo, typeof(Factory<TContract>), StartBinding());
        }

        public FactoryToChoiceIdBinder<TContract> BindFactory<TContract, TFactory>()
            where TFactory : Factory<TContract>
        {
            var bindInfo = new BindInfo(typeof(TFactory));
            return new FactoryToChoiceIdBinder<TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        public FactoryToChoiceIdBinder<TParam1, TContract> BindIFactory<TParam1, TContract>()
        {
            var bindInfo = new BindInfo(typeof(IFactory<TParam1, TContract>));
            return new FactoryToChoiceIdBinder<TParam1, TContract>(
                bindInfo, typeof(Factory<TParam1, TContract>), StartBinding());
        }

        public FactoryToChoiceIdBinder<TParam1, TContract> BindFactory<TParam1, TContract, TFactory>()
            where TFactory : Factory<TParam1, TContract>
        {
            var bindInfo = new BindInfo(typeof(TFactory));
            return new FactoryToChoiceIdBinder<TParam1, TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TContract> BindIFactory<TParam1, TParam2, TContract>()
        {
            var bindInfo = new BindInfo(typeof(IFactory<TParam1, TParam2, TContract>));
            return new FactoryToChoiceIdBinder<TParam1, TParam2, TContract>(
                bindInfo, typeof(Factory<TParam1, TParam2, TContract>), StartBinding());
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TContract> BindFactory<TParam1, TParam2, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TContract>
        {
            var bindInfo = new BindInfo(typeof(TFactory));
            return new FactoryToChoiceIdBinder<TParam1, TParam2, TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TContract> BindIFactory<TParam1, TParam2, TParam3, TContract>()
        {
            var bindInfo = new BindInfo(typeof(IFactory<TParam1, TParam2, TParam3, TContract>));
            return new FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TContract>(
                bindInfo, typeof(Factory<TParam1, TParam2, TParam3, TContract>), StartBinding());
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TContract> BindFactory<TParam1, TParam2, TParam3, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TParam3, TContract>
        {
            var bindInfo = new BindInfo(typeof(TFactory));
            return new FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindIFactory<TParam1, TParam2, TParam3, TParam4, TContract>()
        {
            var bindInfo = new BindInfo(typeof(IFactory<TParam1, TParam2, TParam3, TParam4, TContract>));
            return new FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TContract>(
                bindInfo, typeof(Factory<TParam1, TParam2, TParam3, TParam4, TContract>), StartBinding());
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TContract> BindFactory<TParam1, TParam2, TParam3, TParam4, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TParam3, TParam4, TContract>
        {
            var bindInfo = new BindInfo(typeof(TFactory));
            return new FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract> BindIFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>()
        {
            var bindInfo = new BindInfo(typeof(IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>));
            return new FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>(
                bindInfo, typeof(Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>), StartBinding());
        }

        public FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract> BindFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract, TFactory>()
            where TFactory : Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>
        {
            var bindInfo = new BindInfo(typeof(TFactory));
            return new FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>(
                bindInfo, typeof(TFactory), StartBinding());
        }

        ////////////// Types ////////////////

        class ProviderPair
        {
            public ProviderPair(
                ProviderInfo providerInfo,
                DiContainer container)
            {
                ProviderInfo = providerInfo;
                Container = container;
            }

            public ProviderInfo ProviderInfo
            {
                get;
                private set;
            }

            public DiContainer Container
            {
                get;
                private set;
            }
        }

        public enum ProviderLookupResult
        {
            Success,
            Multiple,
            None
        }

        struct LookupId
        {
            public readonly IProvider Provider;
            public readonly BindingId BindingId;

            public LookupId(
                IProvider provider, BindingId bindingId)
            {
                Provider = provider;
                BindingId = bindingId;
            }
        }

        public class ProviderInfo
        {
            public ProviderInfo(IProvider provider, BindingCondition condition)
            {
                Provider = provider;
                Condition = condition;
            }

            public IProvider Provider
            {
                get;
                private set;
            }

            public BindingCondition Condition
            {
                get;
                private set;
            }
        }
    }
}
