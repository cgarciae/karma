using UnityEngine;
using System.Collections;
using Karma;
using System;
using Karma.Metadata;
using Zenject;

[Presenter(view)]
public class MenuPresenter : MVCPresenter
{
    public const string view = "menu";

    public IRouter router { get; private set; }

    [PostInject]
    public void PostConstructor(IRouter router)
    {
        this.router = router;
    }

    public void GoToCubeCounterView()
    {
        router.GoTo(CubeCounterPresenter.view);
    }

    public override void OnPresenterDestroy()
    {
        
    }

    
}
