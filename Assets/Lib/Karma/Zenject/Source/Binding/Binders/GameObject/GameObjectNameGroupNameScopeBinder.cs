#if !NOT_UNITY3D

namespace Zenject
{
    public class GameObjectNameGroupNameScopeBinder : GameObjectGroupNameScopeBinder
    {
        public GameObjectNameGroupNameScopeBinder(
            BindInfo bindInfo,
            GameObjectCreationParameters gameObjectInfo)
            : base(bindInfo, gameObjectInfo)
        {
        }

        public GameObjectGroupNameScopeBinder WithGameObjectName(string gameObjectName)
        {
            GameObjectInfo.Name = gameObjectName;
            return this;
        }
    }
}

#endif
