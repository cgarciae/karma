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
    internal static class InstantiateUtil
    {
        public static List<TypeValuePair> CreateTypeValueList(IEnumerable<object> args)
        {
            Assert.That(!args.Contains(null));
            return args.Select(x => new TypeValuePair(x.GetType(), x)).ToList();
        }

        public static TypeValuePair CreateTypePair<T>(T param)
        {
            // Use the most derived type that we can find here
            return new TypeValuePair(
                param == null ? typeof(T) : param.GetType(), param);
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
