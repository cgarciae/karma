#if !NOT_UNITY3D

using System;
using UnityEngine;
namespace Zenject
{
    public class GameObjectGroupNameScopeBinder : ScopeBinder
    {
        public GameObjectGroupNameScopeBinder(
            BindInfo bindInfo,
            GameObjectCreationParameters gameObjectInfo)
            : base(bindInfo)
        {
            GameObjectInfo = gameObjectInfo;
        }

        protected GameObjectCreationParameters GameObjectInfo
        {
            get;
            private set;
        }

        public ScopeBinder UnderTransform(Transform parent)
        {
            GameObjectInfo.ParentTransform = parent;
            return this;
        }

        public ScopeBinder UnderTransform(Func<DiContainer, Transform> parentGetter)
        {
            GameObjectInfo.ParentTransformGetter = parentGetter;
            return this;
        }

        public ScopeBinder UnderTransformGroup(string transformGroupname)
        {
            GameObjectInfo.GroupName = transformGroupname;
            return this;
        }
    }
}

#endif
