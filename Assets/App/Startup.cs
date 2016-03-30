using UnityEngine;
using System.Collections;
using Karma;
using System;
using Zenject;


public class Startup : App, ICApp
{
    public override void Configure(IApplication app, DiContainer container)
    {
        container.Bind<ICApp>().ToInstance(this);
    }

    public override void Init(IRouter router, DiContainer container)
    {
        router.GoTo(MenuPresenter.view);
    }

    public override void OnPresenterDestroy()
    {
        
    }
}

public interface ICApp{}