#if !NOT_UNITY3D

namespace Zenject
{
    public class GameObjectNameGroupNameBinder : GameObjectGroupNameBinder
    {
        public GameObjectNameGroupNameBinder(
            BindInfo bindInfo, GameObjectCreationParameters gameObjectInfo)
            : base(bindInfo, gameObjectInfo)
        {
        }

        public GameObjectGroupNameBinder WithGameObjectName(string gameObjectName)
        {
            GameObjectInfo.Name = gameObjectName;
            return this;
        }
    }
}

#endif
