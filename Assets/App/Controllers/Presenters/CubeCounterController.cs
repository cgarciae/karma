using UnityEngine;
using System.Collections;
using Karma;
using System;
using Karma.Metadata;

[Controller]
public class CubeCounterController : IController
{
    public CubeCounterPresenter presenter { get; private set; }

    public int cubes { get; private set; }
    public CounterServices counterServices { get; private set; }

    public CubeCounterController(CounterServices counterServices)
    {
        this.counterServices = counterServices;
    }

    public void PostConstructor(CubeCounterPresenter presenter)
    {
        this.presenter = presenter;

        presenter.SetCubeNumber(cubes);
        presenter.SetMaxCubeNumber(counterServices.maxValue);
    }

    public void OnDestroy()
    {
        
    }

    internal void CubeDestroyed(object _)
    {
        UpdateCubeNumber(-1);
    }

    internal void CubeCreated()
    {
        UpdateCubeNumber(1);
    }

    internal void UpdateCubeNumber(int delta)
    {
        cubes += delta;
        presenter.SetCubeNumber(cubes);

        var max = counterServices.maxValue = Mathf.Max(cubes, counterServices.maxValue);
        presenter.SetMaxCubeNumber(max);
    }

    
}
