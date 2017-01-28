#if !NOT_UNITY3D

using System;
using UnityEngine;
namespace Zenject
{
    public class GameObjectGroupNameScopeArgBinder : ScopeArgBinder
    {
        public GameObjectGroupNameScopeArgBinder(
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

        public ScopeArgBinder UnderTransform(Transform parent)
        {
            GameObjectInfo.ParentTransform = parent;
            return this;
        }

        public ScopeArgBinder UnderTransform(Func<DiContainer, Transform> parentGetter)
        {
            GameObjectInfo.ParentTransformGetter = parentGetter;
            return this;
        }

        public ScopeArgBinder UnderTransformGroup(string transformGroupname)
        {
            GameObjectInfo.GroupName = transformGroupname;
            return this;
        }
    }
}

#endif
