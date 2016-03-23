using System;
using System.Collections.Generic;

namespace Zenject
{
    // Zero parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class Factory<T> : IValidatableFactory, IFactory<T>
    {
        // Use inject rather than a constructor parameter so that
        // derived classes aren't also forced to create a constructor
        [Inject]
        DiContainer _container = null;

        public Type ConstructedType
        {
            get { return typeof(T); }
        }

        public Type[] ProvidedTypes
        {
            get { return new Type[0]; }
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual T Create()
        {
            return _container.Instantiate<T>();
        }
    }

    // One parameter
    [System.Diagnostics.DebuggerStepThrough]
    public class Factory<TParam1, TValue> : IValidatableFactory, IFactory<TParam1, TValue>
    {
        [Inject]
        DiContainer _container = null;

        public Type ConstructedType
        {
            get { return typeof(TValue); }
        }

        public Type[] ProvidedTypes
        {
            get { return new Type[] { typeof(TParam1) }; }
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual TValue Create(TParam1 param)
        {
            return _container.InstantiateExplicit<TValue>(
                new List<TypeValuePair>()
                {
                    InstantiateUtil.CreateTypePair(param),
                });
        }
    }

    // Two parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class Factory<TParam1, TParam2, TValue> : IValidatableFactory, IFactory<TParam1, TParam2, TValue>
    {
        [Inject]
        DiContainer _container = null;

        public Type ConstructedType
        {
            get { return typeof(TValue); }
        }

        public Type[] ProvidedTypes
        {
            get { return new Type[] { typeof(TParam1), typeof(TParam2) }; }
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual TValue Create(TParam1 param1, TParam2 param2)
        {
            return _container.InstantiateExplicit<TValue>(
                new List<TypeValuePair>()
                {
                    InstantiateUtil.CreateTypePair(param1),
                    InstantiateUtil.CreateTypePair(param2),
                });
        }
    }

    // Three parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class Factory<TParam1, TParam2, TParam3, TValue> : IValidatableFactory, IFactory<TParam1, TParam2, TParam3, TValue>
    {
        [Inject]
        DiContainer _container = null;

        public Type ConstructedType
        {
            get { return typeof(TValue); }
        }

        public Type[] ProvidedTypes
        {
            get { return new Type[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }; }
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual TValue Create(TParam1 param1, TParam2 param2, TParam3 param3)
        {
            return _container.InstantiateExplicit<TValue>(
                new List<TypeValuePair>()
                {
                    InstantiateUtil.CreateTypePair(param1),
                    InstantiateUtil.CreateTypePair(param2),
                    InstantiateUtil.CreateTypePair(param3),
                });
        }
    }

    // Four parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class Factory<TParam1, TParam2, TParam3, TParam4, TValue> :
        IValidatableFactory, IFactory<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        [Inject]
        DiContainer _container = null;

        public Type ConstructedType
        {
            get { return typeof(TValue); }
        }

        public Type[] ProvidedTypes
        {
            get { return new Type[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4) }; }
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual TValue Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
        {
            return _container.InstantiateExplicit<TValue>(
                new List<TypeValuePair>()
                {
                    InstantiateUtil.CreateTypePair(param1),
                    InstantiateUtil.CreateTypePair(param2),
                    InstantiateUtil.CreateTypePair(param3),
                    InstantiateUtil.CreateTypePair(param4),
                });
        }
    }

    // Five parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> :
        IValidatableFactory, IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        [Inject]
        DiContainer _container = null;

        public Type ConstructedType
        {
            get { return typeof(TValue); }
        }

        public Type[] ProvidedTypes
        {
            get { return new Type[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5) }; }
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual TValue Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
        {
            return _container.InstantiateExplicit<TValue>(
                new List<TypeValuePair>()
                {
                    InstantiateUtil.CreateTypePair(param1),
                    InstantiateUtil.CreateTypePair(param2),
                    InstantiateUtil.CreateTypePair(param3),
                    InstantiateUtil.CreateTypePair(param4),
                    InstantiateUtil.CreateTypePair(param5),
                });
        }
    }

    // Six parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> :
        IValidatableFactory, IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>
    {
        [Inject]
        DiContainer _container = null;

        public Type ConstructedType
        {
            get { return typeof(TValue); }
        }

        public Type[] ProvidedTypes
        {
            get { return new Type[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6) }; }
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual TValue Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
        {
            return _container.InstantiateExplicit<TValue>(
                new List<TypeValuePair>()
                {
                    InstantiateUtil.CreateTypePair(param1),
                    InstantiateUtil.CreateTypePair(param2),
                    InstantiateUtil.CreateTypePair(param3),
                    InstantiateUtil.CreateTypePair(param4),
                    InstantiateUtil.CreateTypePair(param5),
                    InstantiateUtil.CreateTypePair(param6),
                });
        }
    }

    // Seven parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> :
        IValidatableFactory, IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>
    {
        [Inject]
        DiContainer _container = null;

        public Type ConstructedType
        {
            get { return typeof(TValue); }
        }

        public Type[] ProvidedTypes
        {
            get { return new Type[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7) }; }
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual TValue Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7)
        {
            return _container.InstantiateExplicit<TValue>(
                new List<TypeValuePair>()
                {
                    InstantiateUtil.CreateTypePair(param1),
                    InstantiateUtil.CreateTypePair(param2),
                    InstantiateUtil.CreateTypePair(param3),
                    InstantiateUtil.CreateTypePair(param4),
                    InstantiateUtil.CreateTypePair(param5),
                    InstantiateUtil.CreateTypePair(param6),
                    InstantiateUtil.CreateTypePair(param7),
                });
        }
    }

    // Eigth parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> :
        IValidatableFactory, IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>
    {
        [Inject]
        DiContainer _container = null;

        public Type ConstructedType
        {
            get { return typeof(TValue); }
        }

        public Type[] ProvidedTypes
        {
            get { return new Type[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8) }; }
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual TValue Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8)
        {
            return _container.InstantiateExplicit<TValue>(
                new List<TypeValuePair>()
                {
                    InstantiateUtil.CreateTypePair(param1),
                    InstantiateUtil.CreateTypePair(param2),
                    InstantiateUtil.CreateTypePair(param3),
                    InstantiateUtil.CreateTypePair(param4),
                    InstantiateUtil.CreateTypePair(param5),
                    InstantiateUtil.CreateTypePair(param6),
                    InstantiateUtil.CreateTypePair(param7),
                    InstantiateUtil.CreateTypePair(param8),
                });
        }
    }

    // Nine parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TValue> :
        IValidatableFactory, IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TValue>
    {
        [Inject]
        DiContainer _container = null;

        public Type ConstructedType
        {
            get { return typeof(TValue); }
        }

        public Type[] ProvidedTypes
        {
            get { return new Type[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9) }; }
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual TValue Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9)
        {
            return _container.InstantiateExplicit<TValue>(
                new List<TypeValuePair>()
                {
                    InstantiateUtil.CreateTypePair(param1),
                    InstantiateUtil.CreateTypePair(param2),
                    InstantiateUtil.CreateTypePair(param3),
                    InstantiateUtil.CreateTypePair(param4),
                    InstantiateUtil.CreateTypePair(param5),
                    InstantiateUtil.CreateTypePair(param6),
                    InstantiateUtil.CreateTypePair(param7),
                    InstantiateUtil.CreateTypePair(param8),
                    InstantiateUtil.CreateTypePair(param9),
                });
        }
    }

    // Ten parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class Factory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TValue> :
        IValidatableFactory, IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TValue>
    {
        [Inject]
        DiContainer _container = null;

        public Type ConstructedType
        {
            get { return typeof(TValue); }
        }

        public Type[] ProvidedTypes
        {
            get { return new Type[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4), typeof(TParam5), typeof(TParam6), typeof(TParam7), typeof(TParam8), typeof(TParam9), typeof(TParam10) }; }
        }

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual TValue Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10)
        {
            return _container.InstantiateExplicit<TValue>(
                new List<TypeValuePair>()
                {
                    InstantiateUtil.CreateTypePair(param1),
                    InstantiateUtil.CreateTypePair(param2),
                    InstantiateUtil.CreateTypePair(param3),
                    InstantiateUtil.CreateTypePair(param4),
                    InstantiateUtil.CreateTypePair(param5),
                    InstantiateUtil.CreateTypePair(param6),
                    InstantiateUtil.CreateTypePair(param7),
                    InstantiateUtil.CreateTypePair(param8),
                    InstantiateUtil.CreateTypePair(param9),
                    InstantiateUtil.CreateTypePair(param10),
                });
        }
    }
}
