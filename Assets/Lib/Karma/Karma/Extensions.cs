using System;
using UnityEngine;

using Zenject;
using RektTransform;

namespace Karma
{
    public static class MiddlewareExtensions
    {
        public static IApplication UseMVC(this App app)
        {
            return app.AddMiddleware(app.CreatePresenters);
        }
    }
    public static class Extensions
    {
        
        

        public static void ResetTransform(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localScale = Vector3.one;
            t.localRotation = Quaternion.identity;
        }

        public static void ResetRectTransform(this RectTransform t)
        {
            t.ResetTransform();

            t.SetTopEdge(0f);
            t.SetLeftEdge(0f);
            t.SetRightEdge(0f);
            t.SetBottomEdge(0f);
        }

        public static void ResetTransformUnder(this Transform t, Transform parent)
        {
            t.SetParent(parent);
            t.ResetTransform();
        }

        public static void ResetRectTransformUnder(this RectTransform t, RectTransform parent)
        {
            t.SetParent(parent);
            t.ResetRectTransform();
        }

        public static void ResetTransformUnder(this MonoBehaviour m, Transform parent)
        {
            ResetTransformUnder(m.transform, parent);
        }

        public static void ResetRectTransformUnder(this MonoBehaviour m, RectTransform parent)
        {
            ResetRectTransformUnder(m.RectTransform(), parent);
        }

        public static void CopyTransform(this Transform t, Transform other)
        {
            var parent = t.parent;
            t.ResetTransformUnder(other);
            t.parent = parent;
        }

        public static RectTransform RectTransform(this Transform t)
        {
            return (RectTransform)t;
        }

        public static RectTransform RectTransform(this MonoBehaviour m)
        {
            return m.transform.RectTransform();
        }

        public static RectTransform RectTransform(this Behaviour m)
        {
            return m.transform.RectTransform();
        }
    }
}
