using System;
using System.Collections.Generic;

namespace Zenject
{
    // We don't bother implementing IValidatable here because it is assumed that the nested
    // factory handles that for us

    // Zero parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryNested<TContract, TConcrete> : IFactory<TContract>
        where TConcrete : TContract
    {
        readonly IFactory<TConcrete> _concreteFactory;

        public FactoryNested(IFactory<TConcrete> concreteFactory)
        {
            _concreteFactory = concreteFactory;
        }

        public virtual TContract Create()
        {
            return _concreteFactory.Create();
        }
    }

    // One parameter
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryNested<TParam1, TContract, TConcrete> : IFactory<TParam1, TContract>
        where TConcrete : TContract
    {
        readonly IFactory<TParam1, TConcrete> _concreteFactory;

        public FactoryNested(IFactory<TParam1, TConcrete> concreteFactory)
        {
            _concreteFactory = concreteFactory;
        }

        public virtual TContract Create(TParam1 param)
        {
            return _concreteFactory.Create(param);
        }
    }

    // Two parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryNested<TParam1, TParam2, TContract, TConcrete> : IFactory<TParam1, TParam2, TContract>
        where TConcrete : TContract
    {
        readonly IFactory<TParam1, TParam2, TConcrete> _concreteFactory;

        public FactoryNested(IFactory<TParam1, TParam2, TConcrete> concreteFactory)
        {
            _concreteFactory = concreteFactory;
        }


        public virtual TContract Create(TParam1 param1, TParam2 param2)
        {
            return _concreteFactory.Create(param1, param2);
        }
    }

    // Three parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryNested<TParam1, TParam2, TParam3, TContract, TConcrete> : IFactory<TParam1, TParam2, TParam3, TContract>
        where TConcrete : TContract
    {
        readonly IFactory<TParam1, TParam2, TParam3, TConcrete> _concreteFactory;

        public FactoryNested(IFactory<TParam1, TParam2, TParam3, TConcrete> concreteFactory)
        {
            _concreteFactory = concreteFactory;
        }

        public virtual TContract Create(TParam1 param1, TParam2 param2, TParam3 param3)
        {
            return _concreteFactory.Create(param1, param2, param3);
        }
    }

    // Four parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryNested<TParam1, TParam2, TParam3, TParam4, TContract, TConcrete> :
        IFactory<TParam1, TParam2, TParam3, TParam4, TContract>
        where TConcrete : TContract
    {
        readonly IFactory<TParam1, TParam2, TParam3, TParam4, TConcrete> _concreteFactory;

        public FactoryNested(IFactory<TParam1, TParam2, TParam3, TParam4, TConcrete> concreteFactory)
        {
            _concreteFactory = concreteFactory;
        }

        public virtual TContract Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
        {
            return _concreteFactory.Create(param1, param2, param3, param4);
        }
    }

    // Five parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryNested<TParam1, TParam2, TParam3, TParam4, TParam5, TContract, TConcrete> :
        IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>
        where TConcrete : TContract
    {
        readonly IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TConcrete> _concreteFactory;

        public FactoryNested(IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TConcrete> concreteFactory)
        {
            _concreteFactory = concreteFactory;
        }

        public virtual TContract Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
        {
            return _concreteFactory.Create(param1, param2, param3, param4, param5);
        }
    }

    // Six parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryNested<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TContract, TConcrete> :
        IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TContract>
        where TConcrete : TContract
    {
        readonly IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TConcrete> _concreteFactory;

        public FactoryNested(IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TConcrete> concreteFactory)
        {
            _concreteFactory = concreteFactory;
        }

        public virtual TContract Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
        {
            return _concreteFactory.Create(param1, param2, param3, param4, param5, param6);
        }
    }

    // Seven parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryNested<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TContract, TConcrete> :
        IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TContract>
        where TConcrete : TContract
    {
        readonly IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TConcrete> _concreteFactory;

        public FactoryNested(IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TConcrete> concreteFactory)
        {
            _concreteFactory = concreteFactory;
        }

        public virtual TContract Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7)
        {
            return _concreteFactory.Create(param1, param2, param3, param4, param5, param6, param7);
        }
    }

    // Eigth parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryNested<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TContract, TConcrete> :
        IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TContract>
        where TConcrete : TContract
    {
        readonly IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TConcrete> _concreteFactory;

        public FactoryNested(IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TConcrete> concreteFactory)
        {
            _concreteFactory = concreteFactory;
        }

        public virtual TContract Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8)
        {
            return _concreteFactory.Create(param1, param2, param3, param4, param5, param6, param7, param8);
        }
    }

    // Nine parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryNested<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TContract, TConcrete> :
        IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TContract>
        where TConcrete : TContract
    {
        readonly IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TConcrete> _concreteFactory;

        public FactoryNested(IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TConcrete> concreteFactory)
        {
            _concreteFactory = concreteFactory;
        }

        public virtual TContract Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9)
        {
            return _concreteFactory.Create(param1, param2, param3, param4, param5, param6, param7, param8, param9);
        }
    }

    // Ten parameters
    [System.Diagnostics.DebuggerStepThrough]
    public class FactoryNested<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TContract, TConcrete> :
        IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TContract>
        where TConcrete : TContract
    {
        readonly IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TConcrete> _concreteFactory;

        public FactoryNested(IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TConcrete> concreteFactory)
        {
            _concreteFactory = concreteFactory;
        }

        public virtual TContract Create(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10)
        {
            return _concreteFactory.Create(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);
        }
    }
}


