using System;
using System.Collections;
using System.Collections.Generic;

using Atoms;

using Zenject;
using UnityEngine;
using ModestTree.Util;
using Karma.Metadata;
using System.Reflection;
using RSG;

namespace Karma {

    public interface IApplication
    {
        //IApplication RegisterPresenter(String url, Type type);
        //IApplication RegisterRoute<A>(String url) where A : MonoBehaviour;
        A SetEnv<A>();
        IApplication BackInHistoryOnEscapeButton(bool value);
        IApplication UseLayout(bool value);
    }



    public interface IRouter {

        IApplication app { get; }
        void Back();

        void GoTo(String url, Dictionary<string, object> parameters = null, object body = null, string layoutPath = null, bool saveInHistory = true);
        void GoTo(IRequest request, bool saveInHistory = true);
        void ClearCurrentView();
    }

    public interface IRequest
    {
        string path { get; set; }
        Dictionary<string, object> parameters { get; set; }
        object body { get; set; }
        string layoutPath { get; set; }
    }

    public interface IResponse
    {
        Type type { get; set; }
        Type layoutType { get; set; }
        string redirectPath { get; set; }
        IRequest request { get; set; }
    }


    public class Request : IRequest
    {
        public string path { get; set; }
        public object body { get; set; }
        public Dictionary<string,object> parameters { get; set; }
        public string layoutPath { get; set; }
    }

    public class Response : IResponse
    {
        public Type type { get; set; }
        public Type layoutType { get; set; }
        public string redirectPath { get; set; }
        public IRequest request { get; set; }
    }

    public delegate IPromise<IResponse> Middleware(IRequest request, Func<IRequest, IPromise<IResponse>> next);
    

    public interface IController
    {
        void OnDestroy();
    }

    public abstract class MVCPresenter : MonoBehaviour, IEnumLoader, IPresenter
    {
        public virtual Transform inner3D { get { return root3D; } }
        public virtual RectTransform innerUI { get { return rootUI; } }

        public virtual bool ready { get { return true; } }

        public virtual Transform root3D { get { return this.transform; } }
        public virtual RectTransform rootUI { get { return this.transform.RectTransform(); } }

        private Dictionary<string, Channel> channelMap = new Dictionary<string, Channel>();
        private List<ValueTriplet<MVCPresenter, string, Action<object>>> registrations = new List<ValueTriplet<MVCPresenter, string, Action<object>>>();

        public void Load(IEnumerable e)
        {
            e.Start(this);
        }

        public abstract void OnPresenterDestroy();

        public void OnDestroy()
        {
            OnPresenterDestroy();

            registrations.ForEach(ValueTriplet => 
            {
                var presenter = ValueTriplet.First;
                var topic = ValueTriplet.Second;
                var callback = ValueTriplet.Third;

                //print("Unregistering " + name + "  from " + topic + " on " + presenter.name);

                presenter.Unsubscribe(topic, callback);
            });

            //print("Destroying " + name);
        }
        

        public void SetChild(MVCPresenter child)
        {

            child.transform.SetParent(this.transform);
            
            child.root3D.ResetTransformUnder(this.root3D);
            child.rootUI.ResetRectTransformUnder(this.innerUI);
        }

        public MVCPresenter Subscribe(string topic, Action<object> callback)
        {
            MaybeCreateChannel(topic);

            var channel = channelMap[topic];
            channel.Register(callback);

            return this;
        }

        public MVCPresenter Unsubscribe(string topic, Action<object> callback)
        {
            Channel channel = null;
            if (channelMap.TryGetValue(topic, out channel))
            {
                channel.Unregister(callback);
            }

            return this;
        }

        public void RegisterOn(MVCPresenter presenter, string topic, Action<object> callback)
        {
            presenter.Subscribe(topic, callback);

            registrations
                .Add(ValueTriplet.New(presenter, topic, callback));
        }

        private void Broadcast(string topic, object msg)
        {
            channelMap[topic].Broadcast(msg);
        }

        public void BroadcastOn(MVCPresenter presenter, string topic, object msg = null)
        {
            var msgString = msg == null ? "null" : msg.ToString();

            //print(name + " broadcasting " + msgString + " on " + topic + " to " + presenter.name);
            presenter.Broadcast(topic, msg);
        }

        public bool TryBroadcast(string topic, object obj)
        {
            try
            {
                Broadcast(topic, obj);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void MaybeCreateChannel(string topic)
        {
            if (channelMap.ContainsKey(topic))
                return;

            var channel = new Channel(topic);
            channelMap.Add(topic, channel);
        }
    }

    public interface IPresenter
    {
        Transform root3D { get; }
        RectTransform rootUI { get;}
        RectTransform innerUI { get; }
        Transform inner3D { get; }

        void OnPresenterDestroy();

        
    }

    public interface ICurrentLayoutPresenter : IPresenter
    {
        
    }

    public class Channel
    {
        public readonly string topic;

        public event Action<object> actions;

        public Channel(string topic)
        {
            this.topic = topic;
        }

        public void Register(Action<object> f)
        {
            actions += f;
        }

        public void Unregister(Action<object> f)
        {
            actions -= f;
        }

        public void Broadcast(object obj)
        {
            if (actions != null)
            {
                actions(obj);
            }
        }
    }


    public interface IEnumLoader
    {
        void Load(IEnumerable e);
    }

}