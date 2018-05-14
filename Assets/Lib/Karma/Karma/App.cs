using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Zenject;
using Atoms;
using ModestTree.Util;
using System.Reflection;
using Karma.Metadata;
using JsonFx.Json;
using System.Text;
using RSG;
using System.Linq;

namespace Karma {
    public abstract class App : MVCPresenter, IApplication, IRouter
    {
        public Dictionary<string, ValuePair<IRoute,Type>> presentersMap { get; private set; }
        public Dictionary<string, ValuePair<IRoute, Type>> layoutsMap { get; private set; }
        private DiContainer container { get; set; }
        public bool useHistoryOnBackButton { get; private set; }
        public IApplication app { get; private set; }
        public bool useLayout { get; private set; }

        MVCPresenter current;
        MVCPresenter currentLayout;
        MVCPresenter currentPresenter;
        public Transform root;
        public bool useZenjectContext = false;
        
        Stack<IRequest> history = new Stack<IRequest>();
        
        public void Awake()
        {
            if (root == null)
                root = this.transform;

            app = this;
            presentersMap = new Dictionary<string, ValuePair<IRoute, Type>>();
            layoutsMap = new Dictionary<string, ValuePair<IRoute, Type>>();

            // On a strictly optional basis, use an existing Zenject context/installer system
            if (useZenjectContext) 
            {
                SceneContext context = FindObjectOfType<SceneContext>();
                if (context != null) 
                {
                    container = context.Container;
                }
                else if (ProjectContext.HasInstance) 
                {
                    container = ProjectContext.Instance.Container;
                } 
                else // We need to create a context to work in, here... 
                {
                    CreateZenjectContext();
                }
            } 
            else // ...or here 
            {
                CreateZenjectContext();
            }

            container.BindInterfacesAndSelfTo<App>().FromInstance(this).AsCached();
            SetupDI();

            Configure(this, container);
        }

        
        public void Start()
        {
            Init(this, container);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && useHistoryOnBackButton)
            {
                Debug.Log("Escape Pressed");
                Back();
            }
        }

        protected void CreateZenjectContext()
        {
            GameObject context = new GameObject("ZenjectSceneContext");
            context.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave | HideFlags.NotEditable;
            Context zenContext = context.AddComponent<SceneContext>();
            container = new DiContainer();
            container.BindInterfacesAndSelfTo<Context>().FromInstance(zenContext).AsCached();
        }
    
        public DiContainer SetupDI()
        {
            
            //Register presenters
            foreach (var tuple in GetAnnotatedTypes<Presenter>())
            {
                RegisterPresenter(tuple.Second, tuple.First, presentersMap);
            }

            //Register laytous
            foreach (var tuple in GetAnnotatedTypes<Layout>())
            {
                RegisterPresenter(tuple.Second, tuple.First, layoutsMap);
            }

            //Register component
            foreach (var tuple in GetAnnotatedTypes<Element>())
            {
                RegisterPresenter(tuple.Second, tuple.First);
            }

            //Register controllers
            foreach (var tuple in GetAnnotatedTypes<Controller>())
            {
                RegisterController(container, tuple.First);
            }

            //Register services
            foreach (var tuple in GetAnnotatedTypes<Service>())
            {
                RegisterService(container, tuple.First);
            }

            return container;
        }

        public abstract void Configure(IApplication app, DiContainer container);
        public abstract void Init(IRouter router, DiContainer container);

        public IApplication RegisterPresenter(IRoute route, Type type, Dictionary<string, ValuePair<IRoute, Type>> dict = null)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(type))
                throw new Exception(string.Format("Type '{0}' is not a MonoBehaviour", type));

			container.Bind(type).FromComponentInNewPrefabResource(route.fullPath).AsTransient();
            if (dict != null)
            {
                dict[route.path] = ValuePair.New(route, type);
            }
            return this;
        }

        
        
        public IApplication RegisterController(Type type)
        {
            RegisterController(container, type);
            return this;
        }

        public static void RegisterController(DiContainer container, Type type)
        {
            container.Bind(type).AsTransient();
        }

        public IApplication RegisterService(Type type)
        {
            RegisterService(container, type);
            return this;
        }

        public static void RegisterService(DiContainer container, Type type)
        {
            container.Bind(type).AsSingle();
        }
        /*
        public IApplication RegisterRoute<T>(DiContainer container, string url) where T : MonoBehaviour
        {
            RegisterPresenter(container, presentersMap, new Presenter(url) {
                type = typeof(T)
            });
            return this;
        }
        */

        public void Back()
        {
            if (history.Count > 1)
            {
                var currentRequest = history.Pop();
                var previousRequest = history.Peek();
                GoTo(previousRequest, saveInHistory: false);
            }
            else
            {
                Application.Quit();
            }
        }

        

        public void CorrectView(MVCPresenter newView, MVCPresenter layout, MVCPresenter presenter)
        {
            if (layout != null)
            {
                layout.SetChild(presenter);
            }

            newView.ResetRectTransformUnder(root.RectTransform());
        }

        
        public void GoTo(string path, Dictionary<string, object> parameters, object body = null, string layoutPath = "master", bool saveInHistory = true)
        {

            var request = new Request()
            {
                path = path,
                parameters = parameters != null? parameters: new Dictionary<string, object>(),
                body = body,
                layoutPath = layoutPath
            };

            GoTo(request, saveInHistory);
        }

        public void ClearCurrentView()
        {
            ClearView(current);
        }

        private void ClearView(MonoBehaviour view)
        {
            Atom.WaitFrames(1).Then(() =>
            {
                if (view != null)
                    Destroy(view.gameObject);
            })
            .Start(this);
        }

        public static IEnumerable<ValuePair<Type, MetadataType>> GetAnnotatedTypes<MetadataType>() where MetadataType : Attribute
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (var route in type.GetCustomAttributes(true))
                {
                    if (route is MetadataType)
                        yield return ValuePair.New(type, route as MetadataType);
                }
            }
        }

        public A SetEnv<A>()
        {
            var textAsset = (TextAsset)Resources.Load("config");
            var map = Deserialize<Dictionary<string, object>>(textAsset.text);

            var env_type = (string)map["__env__"];
            var general_env = (Dictionary<string, object>)map["__general__"];
            var current_env = (Dictionary<string, object>)map[env_type];

            foreach (var item in current_env)
            {
                general_env[item.Key] = item.Value;
            }

            A env = Cast<A>(general_env);

            container.Bind<A>().FromInstance(env);

            return env;
        }

        

        public static A Deserialize<A>(string s)
        {
            JsonReaderSettings readerSettings = new JsonReaderSettings();
            readerSettings.TypeHintName = "__type__";
            return new JsonReader(s, readerSettings).Deserialize<A>();
        }

        public static object Deserialize(string s, Type type)
        {
            JsonReaderSettings readerSettings = new JsonReaderSettings();
            readerSettings.TypeHintName = "__type__";
            return new JsonReader(s, readerSettings).Deserialize(type);
        }

        public static IEnumerable<A> DeserializeList<A>(string s)
        {
            JsonReaderSettings readerSettings = new JsonReaderSettings();
            readerSettings.TypeHintName = "__type__";
            JsonReader reader = new JsonReader(s, readerSettings);
            return new JsonReader(s, readerSettings).Deserialize<A[]>();
        }

        public static string Serialize<A>(A a)
        {
            JsonWriterSettings writerSettings = new JsonWriterSettings();
            writerSettings.TypeHintName = "__type__";
            StringBuilder json = new StringBuilder();
            JsonWriter writer = new JsonWriter(json, writerSettings);
            writer.Write(a);
            return json.ToString();
        }

        public static A Cast<A>(object b)
        {
            return Deserialize<A>(Serialize(b));
        }

        object Cast(object b, Type type)
        {
            return Deserialize(Serialize(b), type);
        }

        
        /*
        public IApplication RegisterRoute<A>(string url) where A : MonoBehaviour
        {
            return RegisterRoute<A>(container, url);
        }
        */

        public IApplication BackInHistoryOnEscapeButton(bool value)
        {
            this.useHistoryOnBackButton = value;
            return this;
        }

        public IApplication UseLayout(bool value)
        {
            this.useLayout = value;
            return this;
        }

        List<Middleware> _middlewares = new List<Middleware>();
        internal IPromise<IResponse> CreatePresenters(IRequest request, Func<IRequest, IPromise<IResponse>> next)
        {
            return next(request)
                .Then(resp => 
                {
                    container.Rebind<IRequest>().FromInstance(request);

                    MVCPresenter layout = useLayout && resp.layoutType != null ?
                        (MVCPresenter)container.Resolve(resp.layoutType) :
                        null;
                    var presenter = (MVCPresenter)container.Resolve(resp.type);
                    
                    var newView = layout != null ?
                        layout :
                        presenter;

                    CorrectView(newView, layout, presenter);
                    ClearView(current);

                    current = newView;
                    return resp;
                });
        }


        private IPromise<IResponse> CreateResponse(IRequest request, Func<IRequest, IPromise<IResponse>> next)
        {
            if (request.path == null)
            {
                throw new Exception("Path cannot be null");
            }

            if (!presentersMap.ContainsKey(request.path)) {
                StringBuilder sb = new StringBuilder("Karma is not aware of a presenter called ");
                sb.Append(request.path);
                throw new Exception(sb.ToString());
            }

            var tuple = presentersMap[request.path];
            var route = tuple.First;

            var response = new Response()
            {
                type = tuple.Second,
                request = request
            };

            if (!useLayout)
            {
                //Do nothing
            }
            else if (request.layoutPath != null)
            {
                response.layoutType = layoutsMap[request.layoutPath].Second;
            }
            else if (route.layoutPath != null)
            {
                response.layoutType = layoutsMap[route.layoutPath].Second;
            }

            var prom = new Promise<IResponse>();
            prom.Resolve(response);
            return prom;
        }

        IEnumerable<Middleware> middlewares
        {
            get
            {
                return
                    new List<Middleware>() { CreatePresenters }
                    .Concat(_middlewares)
                    .Concat(new List<Middleware>() { CreateResponse });
            }
        }
        public IPromise<IResponse> Goto(IRequest request)
        {
            var chain = middlewares
                .Reverse()
                .Aggregate(default(Func<IRequest, IPromise<IResponse>>), (next, middleware) => 
                {
                    return (req) => middleware(req, next);
                });

            return chain(request);
        }

        public void GoTo(IRequest request, bool saveInHistory = true)
        {
            Goto(request)
                .Catch(e => 
                {
                    Debug.Log(e.ToString());
                });
        }

        internal IApplication AddMiddleware(Middleware middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }
    }

    public interface ICRootApp
    {

    }
}
