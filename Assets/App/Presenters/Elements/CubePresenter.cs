using UnityEngine;
using System.Collections;
using Karma;
using System;
using Karma.Metadata;
using Zenject;
using UnityEngine.EventSystems;

[Element(path)]
public class CubePresenter : MVCPresenter, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public const string path = "cube";

    public MVCPresenter app { get; private set; }

    [PostInject]
    public void PostConstructor(ICApp _app)
    {
        this.app = (MVCPresenter)_app;
    }

    public override void OnPresenterDestroy()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
        BroadcastOn(app, C.cubeDestroyed, null);

        Destroy(gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }
}
