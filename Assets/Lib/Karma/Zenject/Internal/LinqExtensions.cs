using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace ModestTree
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> first, T item)
        {
            foreach (T t in first)
            {
                yield return t;
            }

            yield return item;
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> first, T item)
        {
            yield return item;

            foreach (T t in first)
            {
                yield return t;
            }
        }

        // Return the first item when the list is of length one and otherwise returns default
        public static TSource OnlyOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var results = source.Take(2).ToArray();
            return results.Length == 1 ? results[0] : default(TSource);
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            foreach (T t in second)
            {
                yield return t;
            }

            foreach (T t in first)
            {
                yield return t;
            }
        }

        // These are more efficient than Count() in cases where the size of the collection is not known
        public static bool HasAtLeast<T>(this IEnumerable<T> enumerable, int amount)
        {
            return enumerable.Take(amount).Count() == amount;
        }

        public static bool HasMoreThan<T>(this IEnumerable<T> enumerable, int amount)
        {
            return enumerable.HasAtLeast(amount+1);
        }

        public static bool HasLessThan<T>(this IEnumerable<T> enumerable, int amount)
        {
            return enumerable.HasAtMost(amount-1);
        }

        public static bool HasAtMost<T>(this IEnumerable<T> enumerable, int amount)
        {
            return enumerable.Take(amount + 1).Count() <= amount;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }

        public static IEnumerable<T> GetDuplicates<T>(this IEnumerable<T> list)
        {
            return list.GroupBy(x => x).Where(x => x.Skip(1).Any()).Select(x => x.Key);
        }

        public static IEnumerable<T> ReplaceOrAppend<T>(
            this IEnumerable<T> enumerable, Predicate<T> match, T replacement)
        {
            bool replaced = false;

            foreach (T t in enumerable)
            {
                if (match(t))
                {
                    replaced = true;
                    yield return replacement;
                }
                else
                {
                    yield return t;
                }
            }

            if (!replaced)
            {
                yield return replacement;
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return (T)enumerator.Current;
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }

        // This is more efficient than just Count() < x because it will end early
        // rather than iterating over the entire collection
        public static bool IsLength<T>(this IEnumerable<T> enumerable, int amount)
        {
            return enumerable.Take(amount + 1).Count() == amount;
        }

        public static T GetSingle<T>(this object[] objectArray, bool required)
        {
            if (required)
            {
                return objectArray.Where(x => x is T).Cast<T>().Single();
            }
            else
            {
                return objectArray.Where(x => x is T).Cast<T>().SingleOrDefault();
            }
        }

        public static IEnumerable<T> OfType<T>(this IEnumerable<T> source, Type type)
        {
            Assert.That(type.DerivesFromOrEqual<T>());
            return source.Where(x => x.GetType() == type);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return source.DistinctBy(keySelector, null);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");
            return DistinctByImpl(source, keySelector, comparer);
        }

        static IEnumerable<TSource> DistinctByImpl<TSource, TKey>(IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var knownKeys = new HashSet<TKey>(comparer);
            foreach (var element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static T Second<T>(this IEnumerable<T> list)
        {
            return list.Skip(1).First();
        }

        public static T SecondOrDefault<T>(this IEnumerable<T> list)
        {
            return list.Skip(1).FirstOrDefault();
        }

        public static int RemoveAll<T>(this LinkedList<T> list, Func<T, bool> predicate)
        {
            int numRemoved = 0;

            var currentNode = list.First;
            while (currentNode != null)
            {
                if (predicate(currentNode.Value))
                {
                    var toRemove = currentNode;
                    currentNode = currentNode.Next;
                    list.Remove(toRemove);
                    numRemoved++;
                }
                else
                {
                    currentNode = currentNode.Next;
                }
            }

            return numRemoved;
        }
    }
}
