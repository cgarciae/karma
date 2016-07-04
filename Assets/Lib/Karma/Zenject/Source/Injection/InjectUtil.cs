using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class TypeValuePair
    {
        public Type Type;
        public object Value;

        public TypeValuePair(Type type, object value)
        {
            Type = type;
            Value = value;
        }
    }

    [System.Diagnostics.DebuggerStepThrough]
    public static class InjectUtil
    {
        public static List<TypeValuePair> CreateArgList(IEnumerable<object> args)
        {
            Assert.That(!args.ContainsItem(null),
                "Cannot include null values into Zenject method.  Must use the Explicit form if need to do this.");
            return args.Select(x => new TypeValuePair(x.GetType(), x)).ToList();
        }

        public static TypeValuePair CreateTypePair<T>(T param)
        {
            // Use the most derived type that we can find here
            return new TypeValuePair(
                param == null ? typeof(T) : param.GetType(), param);
        }

        public static List<TypeValuePair> CreateArgListExplicit<T>(T param)
        {
            return new List<TypeValuePair>()
            {
                CreateTypePair(param),
            };
        }

        public static List<TypeValuePair> CreateArgListExplicit<TParam1, TParam2>(TParam1 param1, TParam2 param2)
        {
            return new List<TypeValuePair>()
            {
                CreateTypePair(param1),
                CreateTypePair(param2),
            };
        }

        public static List<TypeValuePair> CreateArgListExplicit<TParam1, TParam2, TParam3>(
            TParam1 param1, TParam2 param2, TParam3 param3)
        {
            return new List<TypeValuePair>()
            {
                CreateTypePair(param1),
                CreateTypePair(param2),
                CreateTypePair(param3),
            };
        }

        public static List<TypeValuePair> CreateArgListExplicit<TParam1, TParam2, TParam3, TParam4>(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
        {
            return new List<TypeValuePair>()
            {
                CreateTypePair(param1),
                CreateTypePair(param2),
                CreateTypePair(param3),
                CreateTypePair(param4),
            };
        }

        public static List<TypeValuePair> CreateArgListExplicit<TParam1, TParam2, TParam3, TParam4, TParam5>(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
        {
            return new List<TypeValuePair>()
            {
                CreateTypePair(param1),
                CreateTypePair(param2),
                CreateTypePair(param3),
                CreateTypePair(param4),
                CreateTypePair(param5),
            };
        }

        public static List<TypeValuePair> CreateArgListExplicit<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
        {
            return new List<TypeValuePair>()
            {
                CreateTypePair(param1),
                CreateTypePair(param2),
                CreateTypePair(param3),
                CreateTypePair(param4),
                CreateTypePair(param5),
                CreateTypePair(param6),
            };
        }

        // Find the first match with the given type and remove it from the list
        // Return true if it was removed
        public static bool PopValueWithType(
            List<TypeValuePair> extraArgMap, Type injectedFieldType, out object value)
        {
            var match = extraArgMap
                .Where(x => x.Type.DerivesFromOrEqual(injectedFieldType))
                .FirstOrDefault();

            if (match != null)
            {
                // Note that this will only remove the first element which is what we want
                extraArgMap.RemoveWithConfirm(match);
                value = match.Value;
                return true;
            }

            value = injectedFieldType.GetDefaultValue();
            return false;
        }
    }
}
