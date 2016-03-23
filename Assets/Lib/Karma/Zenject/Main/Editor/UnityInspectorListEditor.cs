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
    public abstract class UnityInspectorListEditor : UnityEditor.Editor
    {
        List<ReorderableList> _installersLists;
        List<SerializedProperty> _installersProperties;

        protected abstract string[] PropertyNames
        {
            get;
        }

        protected abstract string[] PropertyDescriptions
        {
            get;
        }

        protected virtual bool DisplayAllProperties
        {
            get
            {
                return true;
            }
        }

        public virtual void OnEnable()
        {
            _installersProperties = new List<SerializedProperty>();
            _installersLists = new List<ReorderableList>();

            var descriptions = PropertyDescriptions;
            var names = PropertyNames;

            Assert.IsEqual(descriptions.Length, names.Length);

            var infos = Enumerable.Range(0, names.Length).Select(i => new { Name = names[i], Description = descriptions[i] }).ToList();

            foreach (var info in infos)
            {
                var installersProperty = serializedObject.FindProperty(info.Name);
                _installersProperties.Add(installersProperty);

                ReorderableList installersList = new ReorderableList(serializedObject, installersProperty, true, true, true, true);
                _installersLists.Add(installersList);

                var closedName = info.Name;
                var closedDesc = info.Description;

                installersList.drawHeaderCallback += rect =>
                {
                    GUI.Label(rect,
                    new GUIContent(closedName, closedDesc));
                };
                installersList.drawElementCallback += (rect, index, active, focused) =>
                {
                    rect.width -= 40;
                    rect.x += 20;
                    EditorGUI.PropertyField(rect, installersProperty.GetArrayElementAtIndex(index), GUIContent.none, true);
                };
            }
        }

        public override void OnInspectorGUI()
        {
            if (DisplayAllProperties)
            {
                base.OnInspectorGUI();
            }

            serializedObject.Update();

            if (Application.isPlaying)
            {
                GUI.enabled = false;
            }

            foreach (var list in _installersLists)
            {
                list.DoLayoutList();
            }

            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();
        }
    }
}

