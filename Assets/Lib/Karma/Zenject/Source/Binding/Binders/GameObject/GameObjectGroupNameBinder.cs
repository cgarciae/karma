#if !NOT_UNITY3D

using System;
using UnityEngine;
namespace Zenject
{
    public class GameObjectGroupNameBinder : ConditionBinder
    {
        public GameObjectGroupNameBinder(BindInfo bindInfo, GameObjectCreationParameters gameObjInfo)
            : base(bindInfo)
        {
            GameObjectInfo = gameObjInfo;
        }

        protected GameObjectCreationParameters GameObjectInfo
        {
            get;
            private set;
        }

        public ConditionBinder UnderTransform(Transform parent)
        {
            GameObjectInfo.ParentTransform = parent;
            return this;
        }

        public ConditionBinder UnderTransform(Func<DiContainer, Transform> parentGetter)
        {
            GameObjectInfo.ParentTransformGetter = parentGetter;
            return this;
        }

        public ConditionBinder UnderTransformGroup(string transformGroupname)
        {
            GameObjectInfo.GroupName = transformGroupname;
            return this;
        }
    }
}

#endif
