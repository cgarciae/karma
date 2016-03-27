# What is Karma?
##### MVC Framework for Unity3D
Karma is an MVC framework for Unity3D.

##### Based on Zenject
Its built on top of Zenject which provides Dependency Injection (DI). DI is mainly used to route the app to the desired *presenter*, it also enables us to build composable and testable systems.

##### Inspired by AngularJS and ASP vNext
Some of the basic constructs and code layout is inspired by other MVC frameworks such as AngularJS and ASP vNext.

**Note to Game Developers**
Karma uses concepts like *views* and *presenters* instead of `scenes`. The problem with scenes is that they are too heavy weight, there is no good way to comunicate between them, changing scenes is slow (specially if you are doing UI suff), and they aren't composable. 
The "Karmic" way of doing a game would be to store each level/scene in a prefab, treat it as a MVC View, store all your characters/entities in sperate prefabs, and get them instantiated onto the level's view through DI.

**Simple Routing System**
Karma has a built in routing system that enables you to create isolated views/scenes as prefabs and easily switch between them. Using an `http-like` flow, a *presenter* can `Request` the `router` to render another view.

**As Stateless as possible**
Karma aims to be as stateless as possible to try to give you the guarantee that if you entered a view once with certain `Request` parameters and reached a successful state, then you will always reach that same state if you pass the the same parameters. Unity3D doesn't provide by default a *best practice* changing from a view to another. A common way to do this is to have all posible view instatiated in the scene but only enable the current one, the problem is that you maintain state when reusing GameObjects and often end in bad states because of the many paths you need to account for. Karma keeps things simple and functional by just destroying the *current* view and instatiates a new *next* view when changing views.

**Message Passing**
In Karma state is mainly maintained through message passing, being as true as possible to Go's philosophy:

>Don't communicate by sharing memory; share memory by communicating.

The added benefit of this is that views become configurable without depending on local storage or static variables, is in turn is very important to keep a system testable.

**Folder Structure + Conventions**
As many MVC frameworks, Karma tries to keep the developers sane by stablishing conventions. Among these conventions are the folder structure, concepts (presenters, controllers, services, etc), code organization, a configuration mechanism (dev, prod, test environments).

## Parts
* Presenters
    - Handle the presentation layer
    - Extend MonoBehaviour
    - Are tied to a Prefab
    - Get instantiated on GameObjects
    - Are divided as:
        + *Plain* Presenter (Routable)
        + Elements (Reusable)
        + Layouts (Provide context)
* Controllers
    - Handle the logic layer
    - **Don't** extend MonoBehavior
    - Are 100% testable
    - Are usually coupled to a Presenter
* Services
    - Handle Resources
    - Should be Stateless
    - Singleton
    - Usually handle comunication with a server on a particular resource, local storage, specialized logic for e.g. handling certain user inputs, etc.
* Applications
    - Handle general configuration
    - Are also presenters
    - Contain the current view
    - Can be nested to create subviews
* Router
    - Belongs to a specific application
    - Tells the aplication which view to render next
    - Can store request history
* Middleware
    - Each application can configure its one pipeline
    - Each middleware can modify be `Request` and `Response`
    - Enables to e.g. create an authentication layer separate from the view itself
    - Its asyncronous so e.g. http requests can be made to the server to get extra information

# Why Karma?
**Unity3D doesn't give much structure**
While good developers do follow certain convetions, new developers struggle to keep their project in order. Karma provides conventions for both code structure and folder organization so you can keep your project clean and productive.
**Composable elements**
Mainly thanks to Zenject, with Karma you can create composable elements that you can reuses throught your application. Defining you all the components in your view/level directly in the heirarchy, with Karma you define each subcomponent in a separate prefab with their own presenter, and get them to you current view through DI. This way you avoid these common problems:
* Finding components in heirachy (you can stop abusing `GameObject.Find`)
* Having to update prefabs that are also nested in other prefabs

**Testable components**
One of the mayor goals of Karma is testability. This is achieved through these mechanisms:
1. Configurable transcient views
2. POCO `Controllers` and `Services` that are 100% mockable
3. Dependency Injection

# Getting Started
**Minimal Project**
The easiest way to get started is to clone this repository and open it with Unity3D. It contains a minimal project layout with a sample `Application`, `Presenter`, `Controller` and a `Service`. Just

```
git clone https://github.com/cgarciae/karma.git
```

and open the `karma` folder with Unity3D.

**Minimal Project**
Comming soon!

**Guides**
Comming soon!

