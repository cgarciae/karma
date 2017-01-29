using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using ModestTree;
using Assert = ModestTree.Assert;

namespace Zenject
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ValidateOnlyAttribute : Attribute
    {
    }

    public abstract class ZenjectIntegrationTestFixture
    {
        SceneContext _sceneContext;

        bool _hasStarted;
        bool _isValidating;

        protected DiContainer Container
        {
            get
            {
                return _sceneContext.Container;
            }
        }

        [SetUp]
        public void SetUp()
        {
            ClearScene();
            _hasStarted = false;
            _isValidating = CurrentTestHasAttribute<ValidateOnlyAttribute>();

            ProjectContext.ValidateOnNextRun = _isValidating;

            _sceneContext = new GameObject("SceneContext").AddComponent<SceneContext>();
            _sceneContext.ParentNewObjectsUnderRoot = true;
            // This creates the container but does not resolve the roots yet
            _sceneContext.Install();
        }

        public void Initialize()
        {
            Assert.That(!_hasStarted);
            _hasStarted = true;

            _sceneContext.Resolve();

            // This allows them to make very common bindings fields for use in any of the tests
            Container.Inject(this);

            if (_isValidating)
            {
                Container.ValidateIValidatables();
            }
            else
            {
                _sceneContext.gameObject.GetComponent<SceneKernel>().Start();
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Status == TestStatus.Passed)
            {
                // If we expected an exception then initialize would normally not be called
                // Unless the initialize method itself is what caused the exception
                if (!CurrentTestHasAttribute<ExpectedExceptionAttribute>())
                {
                    Assert.That(_hasStarted, "ZenjectIntegrationTestFixture.Initialize was not called by current test");
                }
            }

            ClearScene();
        }

        bool CurrentTestHasAttribute<T>()
            where T : Attribute
        {
            var fullMethodName = TestContext.CurrentContext.Test.FullName;
            var name = fullMethodName.Substring(fullMethodName.LastIndexOf(".")+1);

            return this.GetType().GetMethod(name).GetCustomAttributes(true)
                .Cast<Attribute>().OfType<T>().Any();
        }

        void ClearScene()
        {
            var scene = EditorSceneManager.GetActiveScene();

            // This is the temp scene that unity creates for EditorTestRunner
            Assert.IsEqual(scene.name, "");

            // This will include ProjectContext which is what we want, to ensure no test
            // affects any other test
            foreach (var gameObject in scene.GetRootGameObjects())
            {
                GameObject.DestroyImmediate(gameObject);
            }
        }
    }
}
