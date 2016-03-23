#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModestTree.Util
{
    public enum MouseWheelScrollDirections
    {
        None,
        Up,
        Down,
    }

    public static class UnityUtil
    {
        public static bool IsAltKeyDown
        {
            get
            {
                return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            }
        }

        public static bool IsControlKeyDown
        {
            get
            {
                return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            }
        }

        public static bool IsShiftKeyDown
        {
            get
            {
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
        }

        public static bool WasShiftKeyJustPressed
        {
            get
            {
                return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            }
        }

        public static bool WasAltKeyJustPressed
        {
            get
            {
                return Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);
            }
        }

        public static MouseWheelScrollDirections CheckMouseScrollWheel()
        {
            var value = Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Approximately(value, 0.0f))
            {
                return MouseWheelScrollDirections.None;
            }

            if (value < 0)
            {
                return MouseWheelScrollDirections.Down;
            }

            return MouseWheelScrollDirections.Up;
        }

        static int GetDepthLevel(Transform transform)
        {
            if (transform == null)
            {
                return 0;
            }

            return 1 + GetDepthLevel(transform.parent);
        }

        public static IEnumerable<T> GetComponentsInChildrenTopDown<T>(GameObject gameObject, bool includeInactive)
            where T : Component
        {
            return gameObject.GetComponentsInChildren<T>(includeInactive)
                .OrderBy(x =>
                    x == null ? int.MinValue : GetDepthLevel(x.transform));
        }

        public static IEnumerable<T> GetComponentsInChildrenBottomUp<T>(GameObject gameObject, bool includeInactive)
            where T : Component
        {
            return gameObject.GetComponentsInChildren<T>(includeInactive)
                .OrderByDescending(x =>
                    x == null ? int.MinValue : GetDepthLevel(x.transform));
        }

        public static List<GameObject> GetRootGameObjects()
        {
            return GameObject.FindObjectsOfType<Transform>().Where(x => x.parent == null).Select(x => x.gameObject).ToList();
        }

        // Returns more intuitive defaults
        // eg. An empty string rather than null
        // An empty collection (eg. List<>) rather than null
        public static object GetSmartDefaultValue(Type type)
        {
            if (type == typeof(string))
            {
                return "";
            }
            else if (type == typeof(Quaternion))
            {
                return Quaternion.identity;
            }
            else if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();

                if (genericType == typeof(List<>) || genericType == typeof(Dictionary<,>))
                {
                    return Activator.CreateInstance(type);
                }
            }

            return type.GetDefaultValue();
        }
    }
}
#endif
