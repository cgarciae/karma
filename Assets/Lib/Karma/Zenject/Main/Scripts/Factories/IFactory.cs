using System;

namespace Zenject
{
    // The difference between a factory and a provider:
    // Factories create new instances, providers might return an existing instance
    public interface IFactory<T>
    {
        T Create();
    }

    public interface IFactory<TParam1, T>
    {
        T Create(TParam1 param);
    }

    public interface IFactory<TParam1, TParam2, T>
    {
        T Create(TParam1 param1, TParam2 param2);
    }

    public interface IFactory<TParam1, TParam2, TParam3, T>
    {
        T Create(TParam1 param1, TParam2 param2, TParam3 param3);
    }

    public interface IFactory<TParam1, TParam2, TParam3, TParam4, T>
    {
        T Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);
    }

    public interface IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, T>
    {
        T Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5);
    }

    public interface IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, T>
    {
        T Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6);
    }

    public interface IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, T>
    {
        T Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7);
    }

    public interface IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, T>
    {
        T Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8);
    }

    public interface IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, T>
    {
        T Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9);
    }

    public interface IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, T>
    {
        T Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10);
    }
}
