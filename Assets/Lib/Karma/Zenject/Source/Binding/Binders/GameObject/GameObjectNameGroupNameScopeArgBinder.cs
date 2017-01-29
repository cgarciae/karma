#if !NOT_UNITY3D

namespace Zenject
{
    public class GameObjectNameGroupNameScopeArgBinder : GameObjectGroupNameScopeArgBinder
    {
        public GameObjectNameGroupNameScopeArgBinder(
            BindInfo bindInfo,
            GameObjectCreationParameters gameObjectInfo)
            : base(bindInfo, gameObjectInfo)
        {
        }

        public GameObjectGroupNameScopeArgBinder WithGameObjectName(string gameObjectName)
        {
            GameObjectInfo.Name = gameObjectName;
            return this;
        }
    }
}

#endif
