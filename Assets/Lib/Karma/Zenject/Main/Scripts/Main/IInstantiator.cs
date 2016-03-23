using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

#if !ZEN_NOT_UNITY3D
using UnityEngine;
#endif

namespace Zenject
{
    public interface IInstantiator
    {
        // Use this method to create any non-monobehaviour
        T Instantiate<T>(params object[] extraArgs);
        object Instantiate(Type concreteType, params object[] extraArgs);

        // This is used instead of Instantiate to support specifying null values
        T InstantiateExplicit<T>(List<TypeValuePair> extraArgMap);

        object InstantiateExplicit(Type concreteType, List<TypeValuePair> extraArgMap);

        // For most cases you can pass in currentContext and concreteIdentifier as null
        object InstantiateExplicit(
            Type concreteType, List<TypeValuePair> extraArgMap, InjectContext currentContext, string concreteIdentifier, bool autoInject);

#if !ZEN_NOT_UNITY3D

        // Create a new game object from a given prefab
        // Without returning any particular monobehaviour
        // If you want to retrieve a specific monobehaviour use InstantiatePrefabForComponent
        GameObject InstantiatePrefabExplicit(
            GameObject prefab, IEnumerable<object> extraArgMap, InjectContext currentContext);

        // Instantiate the given prefab, inject on all MonoBehaviours, then return the instance of 'componentType'
        // Any arguments supplied are assumed to be used as extra parameters into 'componentType'
        object InstantiatePrefabForComponentExplicit(
            Type componentType, GameObject prefab, List<TypeValuePair> extraArgMap, InjectContext currentContext);

        // Instantiate an empty game object and then add a component to it of type 'componentType'
        object InstantiateComponentOnNewGameObjectExplicit(
            Type componentType, string name, List<TypeValuePair> extraArgMap, InjectContext currentContext);

        // Add new monobehaviour to existing game object and fill in its dependencies
        // NOTE: Gameobject here is not a prefab prototype, it is an instance
        Component InstantiateComponent(
            Type componentType, GameObject gameObject, params object[] extraArgMap);

        TContract InstantiateComponent<TContract>(
            GameObject gameObject, params object[] args)
            where TContract : Component;

        // Create a new empty game object under the root transform
        GameObject InstantiateGameObject(string name);

        // Create a new game object from a prefab and fill in dependencies for all children
        GameObject InstantiatePrefab(
            GameObject prefab, params object[] args);

        // Create a new game object from a resource path and fill in dependencies for all children
        GameObject InstantiatePrefabResource(
            string resourcePath, params object[] args);

        /////////////// InstantiatePrefabForComponent

        // Same as InstantiatePrefab but returns a component after it's initialized

        T InstantiatePrefabForComponent<T>(
            GameObject prefab, params object[] extraArgs);

        object InstantiatePrefabForComponent(
            Type concreteType, GameObject prefab, params object[] extraArgs);

        // This is used instead of Instantiate to support specifying null values
        T InstantiatePrefabForComponentExplicit<T>(
            GameObject prefab, List<TypeValuePair> extraArgMap);

        object InstantiatePrefabForComponentExplicit(
            Type concreteType, GameObject prefab, List<TypeValuePair> extraArgMap);

        /////////////// InstantiatePrefabResourceForComponent

        T InstantiatePrefabResourceForComponent<T>(
            string resourcePath, params object[] extraArgs);

        object InstantiatePrefabResourceForComponent(
            Type concreteType, string resourcePath, params object[] extraArgs);

        // This is used instead of Instantiate to support specifying null values
        T InstantiatePrefabResourceForComponentExplicit<T>(
            string resourcePath, List<TypeValuePair> extraArgMap);

        object InstantiatePrefabResourceForComponentExplicit(
            Type concreteType, string resourcePath, List<TypeValuePair> extraArgMap);

        /////////////// InstantiateComponentOnNewGameObject
        // Create a new game object, and add the given component to it, and fill in dependencies

        T InstantiateComponentOnNewGameObject<T>(
            string name, params object[] extraArgs);

        object InstantiateComponentOnNewGameObject(
            Type concreteType, string name, params object[] extraArgs);

        // This is used instead of Instantiate to support specifying null values
        T InstantiateComponentOnNewGameObjectExplicit<T>(
            string name, List<TypeValuePair> extraArgMap);

        object InstantiateComponentOnNewGameObjectExplicit(
            Type concreteType, string name, List<TypeValuePair> extraArgMap);
#endif
    }
}
