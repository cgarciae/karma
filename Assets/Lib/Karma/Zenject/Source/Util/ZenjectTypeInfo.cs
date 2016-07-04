using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;
using ModestTree;

namespace Zenject
{
    public class PostInjectableInfo
    {
        readonly MethodInfo _methodInfo;
        readonly List<InjectableInfo> _injectableInfo;

        public PostInjectableInfo(
            MethodInfo methodInfo, List<InjectableInfo> injectableInfo)
        {
            _methodInfo = methodInfo;
            _injectableInfo = injectableInfo;
        }

        public MethodInfo MethodInfo
        {
            get
            {
                return _methodInfo;
            }
        }

        public IEnumerable<InjectableInfo> InjectableInfo
        {
            get
            {
                return _injectableInfo;
            }
        }
    }

    public class ZenjectTypeInfo
    {
        readonly List<PostInjectableInfo> _postInjectMethods;
        readonly List<InjectableInfo> _constructorInjectables;
        readonly List<InjectableInfo> _fieldInjectables;
        readonly List<InjectableInfo> _propertyInjectables;
        readonly ConstructorInfo _injectConstructor;
        readonly Type _typeAnalyzed;

        public ZenjectTypeInfo(
            Type typeAnalyzed,
            List<PostInjectableInfo> postInjectMethods,
            ConstructorInfo injectConstructor,
            List<InjectableInfo> fieldInjectables,
            List<InjectableInfo> propertyInjectables,
            List<InjectableInfo> constructorInjectables)
        {
            _postInjectMethods = postInjectMethods;
            _fieldInjectables = fieldInjectables;
            _propertyInjectables = propertyInjectables;
            _constructorInjectables = constructorInjectables;
            _injectConstructor = injectConstructor;
            _typeAnalyzed = typeAnalyzed;
        }

        public Type Type
        {
            get
            {
                return _typeAnalyzed;
            }
        }

        public IEnumerable<PostInjectableInfo> PostInjectMethods
        {
            get
            {
                return _postInjectMethods;
            }
        }

        public IEnumerable<InjectableInfo> AllInjectables
        {
            get
            {
                return _constructorInjectables.Concat(_fieldInjectables).Concat(_propertyInjectables).Concat(_postInjectMethods.SelectMany(x => x.InjectableInfo));
            }
        }

        public IEnumerable<InjectableInfo> FieldInjectables
        {
            get
            {
                return _fieldInjectables;
            }
        }

        public IEnumerable<InjectableInfo> PropertyInjectables
        {
            get
            {
                return _propertyInjectables;
            }
        }

        public IEnumerable<InjectableInfo> ConstructorInjectables
        {
            get
            {
                return _constructorInjectables;
            }
        }

        // May be null
        public ConstructorInfo InjectConstructor
        {
            get
            {
                return _injectConstructor;
            }
        }
    }
}
