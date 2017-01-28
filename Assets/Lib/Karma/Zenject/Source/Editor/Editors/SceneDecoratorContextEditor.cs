using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ModestTree;

namespace Zenject
{
    [CustomEditor(typeof(SceneDecoratorContext))]
    public class SceneDecoratorContextEditor : ContextEditor
    {
        SerializedProperty _decoratedContractNameProperty;

        public override void OnEnable()
        {
            base.OnEnable();

            _decoratedContractNameProperty = serializedObject.FindProperty("_decoratedContractName");
        }

        protected override void OnGui()
        {
            base.OnGui();

            EditorGUILayout.PropertyField(_decoratedContractNameProperty);
        }
    }
}
