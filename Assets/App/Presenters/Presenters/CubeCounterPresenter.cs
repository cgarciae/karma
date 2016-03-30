using UnityEngine;
using System.Collections;
using Karma;
using System;
using Karma.Metadata;
using Zenject;
using UnityEngine.UI;

[Presenter(path)]
public class CubeCounterPresenter : MVCPresenter
{
    public const string path = "cube-counter";

    public Transform cubeOrigin;
    public Text numberOfCubes;

    public IRouter router { get; private set; }
    public MVCPresenter app { get; private set; }
    public DiContainer container { get; private set; }
    public CubeCounterController controller { get; private set; }

    [PostInject]
    public void PostConstructor(IRouter router, ICApp _app, DiContainer container, CubeCounterController controller)
    {
        this.router = router;
        this.app = (MVCPresenter)_app;
        this.container = container;
        this.controller = controller;

        controller.PostConstructor(this);

        app.RegisterOn(app, C.cubeDestroyed, controller.CubeDestroyed);
    }

    internal void SetCubeNumber(int cubes)
    {
        numberOfCubes.text = cubes.ToString();
    }

    public void CreateCube()
    {
        print("Creating cube");

        var cube = container.Resolve<CubePresenter>();

        cube.ResetTransformUnder(cubeOrigin);

        controller.CubeCreated();
    }

    public override void OnPresenterDestroy()
    {
        
    }
}
