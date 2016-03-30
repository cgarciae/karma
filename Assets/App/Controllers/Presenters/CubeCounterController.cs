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

    public void PostConstructor(CubeCounterPresenter presenter)
    {
        this.presenter = presenter;

        presenter.SetCubeNumber(cubes);
    }

    public void OnDestroy()
    {
        
    }

    internal void CubeDestroyed(object _)
    {
        cubes -= 1;

        presenter.SetCubeNumber(cubes);
    }

    internal void CubeCreated()
    {
        cubes += 1;

        presenter.SetCubeNumber(cubes);
    }
}
