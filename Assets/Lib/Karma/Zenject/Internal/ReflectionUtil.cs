using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModestTree.Util
{
    public static class ReflectionUtil
    {
        public static bool IsGenericList(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public static bool IsGenericList(Type type, out Type contentsType)
        {
            if (IsGenericList(type))
            {
                contentsType = type.GetGenericArguments().Single();
                return true;
            }

            contentsType = null;
            return false;
        }

        public static IList CreateGenericList(Type elementType, object[] contentsAsObj)
        {
            var genericType = typeof(List<>).MakeGenericType(elementType);

            var list = (IList)Activator.CreateInstance(genericType);

            foreach (var obj in contentsAsObj)
            {
                if (obj != null)
                {
                    Assert.That(elementType.IsAssignableFrom(obj.GetType()),
                        "Wrong type when creating generic list, expected something assignable from '"+ elementType +"', but found '" + obj.GetType() + "'");
                }

                list.Add(obj);
            }

            return list;
        }

        public static IDictionary CreateGenericDictionary(
            Type keyType, Type valueType, object[] keysAsObj, object[] valuesAsObj)
        {
            Assert.That(keysAsObj.Length == valuesAsObj.Length);

            var genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

            var dictionary = (IDictionary)Activator.CreateInstance(genericType);

            for (int i = 0; i < keysAsObj.Length; i++)
            {
                dictionary.Add(keysAsObj[i], valuesAsObj[i]);
            }

            return dictionary;
        }

        public static object DowncastList<TFrom, TTo>(IEnumerable<TFrom> fromList) where TTo : class, TFrom
        {
            var toList = new List<TTo>();

            foreach (var obj in fromList)
            {
                toList.Add(obj as TTo);
            }

            return toList;
        }
    }
}
