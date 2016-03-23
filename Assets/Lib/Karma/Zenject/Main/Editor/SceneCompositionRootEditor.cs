using System.Collections.Generic;
using System.Linq;
using Zenject;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
using ModestTree;

namespace Zenject
{
    [CustomEditor(typeof(SceneCompositionRoot))]
    public class SceneCompositionRootEditor : UnityInspectorListEditor
    {
        protected override string[] PropertyNames
        {
            get
            {
                return new string[]
                {
                    "Installers"
                };
            }
        }

        protected override string[] PropertyDescriptions
        {
            get
            {
                return new string[]
                {
                    ""
                };
            }
        }
    }
}


