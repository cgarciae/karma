using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Futures;
using ModestTree.Util;

namespace Atoms {
	public static class Atom {
	
		public static IEnumerable Wait {get {
			while (true)
				yield return null;
		}}
		
		public static IEnumerable Do (Action f)
		{
			f();
			yield return null;
		}

        public static IEnumerable Do(Func<IEnumerable> f)
        {
            return f();
        }

        public static IEnumerable KeepDoing (Action f)
		{
			while (true) {
				f();
				yield return null;
			}
		}

        public static IEnumerable Limit(this IEnumerable e, int times)
        {
            var enu = e.GetEnumerator();

            while (times-- > 0 && enu.MoveNext())
                yield return enu.Current;
        }
		
		public static void Nothing (){}
		public static IEnumerable DoNothing = Do ((Action)Nothing);

        public static Func<bool> Not(Func<bool> cond)
        {
            return () => !cond();
        }

		public static IEnumerable If (Func<bool> cond, IEnumerable ifTrue) {
			return If (cond, ifTrue, DoNothing);
		}
		
		public static IEnumerable If (Func<bool> cond, IEnumerable ifTrue, IEnumerable ifFalse)
		{
			IEnumerable seleted = cond()? ifTrue: ifFalse;
			
			var ator = seleted.GetEnumerator();
			while (ator.MoveNext())
				yield return ator.Current;
		}

        public static IEnumerable If(this IEnumerable e, Func<bool> cond, IEnumerable ifTrue, IEnumerable ifFalse)
        {
            return e.Then(If(cond, ifTrue, ifFalse));
        }

        public static IEnumerable If(this IEnumerable e, Func<bool> cond, IEnumerable ifTrue)
        {
            return e.Then(If(cond, ifTrue));
        }

        public static void ForEach<A>(this IEnumerable<A> e, Action<A> f) {
            foreach (var elem in e)
                f(elem);
        }
		
		public static IEnumerable WaitWhile (Func<bool> cond)
		{
			while (cond()) {
				yield return null;
			}
		}

        public static IEnumerable WaitWhile(this IEnumerable e, Func<bool> cond)
        {
            return e.Then(WaitWhile(cond));
        }

        public static IEnumerable While (Func<bool> cond, IEnumerable e)
		{
			var ator = e.GetEnumerator();
			while (cond()) {
				ator.MoveNext();
				yield return ator.Current;
			}
		}

        public static IEnumerable While(this IEnumerable e1, Func<bool> cond, IEnumerable e2)
        {
            return e1.Then(While(cond, e2));
        }

        public static IEnumerable While(Func<bool> cond, Action f)
        {
            return While(cond, KeepDoing(f));
        }

        public static IEnumerable While(this IEnumerable e, Func<bool> cond, Action f)
        {
            return e.Then(While(cond, f));
        }

        public static IEnumerable Until(Func<bool> cond, IEnumerable e)
        {
            return While(() => !cond(), e);
        }

        public static IEnumerable Until(this IEnumerable e, Func<bool> cond)
        {
            return Until(cond, e);
        }

        public static IEnumerable Then (this IEnumerable a, IEnumerable b)
		{
			var atorA = a.GetEnumerator();
			var atorB = b.GetEnumerator();
			
			while (atorA.MoveNext())
				yield return atorA.Current;
				
			while (atorB.MoveNext())
				yield return atorB.Current;
		}
		
		public static IEnumerable WaitSeconds (float t)
		{
			yield return new UnityEngine.WaitForSeconds (t);
		}

        public static IEnumerable WaitSeconds(this IEnumerable e, float t)
        {
            return e.Then(WaitSeconds(t));
        }

        public static IEnumerable WaitFrames(int frames) {
            while (frames-- > 0)
                yield return null;
        }

        public static IEnumerable WaitFrames(this IEnumerable e, int frames) {
            return e.Then(WaitFrames(frames));
        }
		
		public static IEnumerable DelayN (int n)
		{
			while (n-- > 0)
			{
				yield return null;
			}
		}
		
		public static IEnumerable Delay1 {get{
			yield return null;
		}}
		
		public static IEnumerable Then<A> (this IEnumerable e, Action<A> f)
		{
			var ator = e.GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
			f ((A)ator.Current);
		}
		
		public static IEnumerable Then<A> (this IEnumerable e, Func<A> f)
		{
			var ator = e.GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
			yield return f ();
		}

        public static IEnumerable Then<A,B>(this IEnumerable e, Func<A,B> f)
        {
            var ator = e.GetEnumerator();

            while (ator.MoveNext())
            {
                yield return ator.Current;
                Debug.Log(ator.Current);
            }
            
            yield return f((A)ator.Current);
        }

        public static IEnumerable Then (this IEnumerable e, Action f)
		{
			var ator = e.GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
			f ();
		}
		
		public static IEnumerable Then<A> (this IEnumerable e, Func<A,IEnumerable> f)
		{
			var ator = e.GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
			
			ator = f ((A)ator.Current).GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
		}
		
		public static IEnumerable Then (this IEnumerable e, Func<IEnumerable> f)
		{
			var ator = e.GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
			
			ator = f ().GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
		}
		
		public static IEnumerable Expand<A> (this IEnumerable<A> e, Func<A,IEnumerable> f)
		{
			foreach (A a in e)
				foreach (var _ in f(a))
					yield return _;
		}
		
		public static IEnumerable<B> Expand<A,B> (this IEnumerable<A> e, Func<A,IEnumerable<B>> f)
		{
			foreach (A a in e)
				foreach (B b in f(a))
					yield return b;
		}
		
		
		
		public static Coroutine Start (this IEnumerable e, MonoBehaviour m)
		{
            e = e.CatchError<Exception>(er => Debug.Log(er.ToString()));
			return m.StartCoroutine (e.GetEnumerator());
		}

		public static Coroutine Start (this IEnumerator e, MonoBehaviour m)
		{
			return m.StartCoroutine (e);
		}
		
		public static IEnumerable Cycle (this IEnumerable e)
		{
			while (true)
				foreach (var _ in e)
					yield return _;
		}

        public static IEnumerable CycleEvery(this IEnumerable e, float seconds)
        {
            return
                e
                .WaitSeconds(seconds)
                .Cycle();
        }

        public static IEnumerable<A> Cycle<A>(this IEnumerable<A> e)
        {
            while (true)
                foreach (var a in e)
                    yield return a;
        }

        public static IEnumerable When(Func<bool> cond, IEnumerable whenTrue, IEnumerable whenFalse = null, Func<bool> stopCond = null)
        {
            whenFalse = whenFalse != null ? 
                whenFalse :
                KeepDoing(Nothing);

            return SimpleStateMachine(stopCond,
                Tuple.New(cond, whenTrue),
                Tuple.New(Not(cond), whenFalse)
            );
        }

        public static IEnumerable When(Func<bool> cond, Action whenTrue, Action whenFalse = null, Func<bool> stopCond = null)
        {
            whenFalse = whenFalse != null ?
                whenFalse :
                Nothing;

            return When(cond, KeepDoing(whenTrue), KeepDoing(whenFalse), stopCond);
        }

        public static IEnumerable When(this IEnumerable e, Func<bool> cond, Action whenTrue, Action whenFalse = null, Func<bool> stopCond = null)
        {
            return e.Then(When(cond, whenTrue, whenFalse, stopCond));
        }
        public static IEnumerable When(this IEnumerable e, Func<bool> cond, IEnumerable whenTrue, IEnumerable whenFalse = null, Func<bool> stopCond = null)
        {
            return e.Then(When(cond, whenTrue, whenFalse, stopCond));
        }

        public static IEnumerable SimpleStateMachine(Func<bool> stopCond, params Tuple<Func<bool>, IEnumerable>[] states)
        {
            var enums = states.Select(s => new
            {
                cond = s.First,
                enu = s.Second.GetEnumerator()
            })
            .ToList();

            while (stopCond == null || ! stopCond())
            {
                var current = enums.FirstOrDefault(state => state.cond());

                if (current != null)
                {
                    current.enu.MoveNext();
                    yield return current.enu.Current;
                }
                else
                {
                    yield break;
                }
            }
        }

        public static IEnumerable SimpleStateMachine(params Tuple<Func<bool>, IEnumerable>[] states)
        {
            return SimpleStateMachine(null, states);
        }
        public static IEnumerable CatchError<E> (this IEnumerable e, Action<E> errorHandler) where E : Exception {
            var enu = e.GetEnumerator();
            var moveNext = true;

            while (moveNext) {
                try
                {
                    moveNext = enu.MoveNext();
                }
                catch (E ex)
                {
                    errorHandler(ex);
                    yield break;
                }

                if (moveNext)
                    yield return enu.Current;
            }

        }

        public static IEnumerable WhenComplete(this IEnumerable e, Action f) {
            return e
                .CatchError<Exception>((Exception ex) => { })
                .Then(f);
        }

        public static IEnumerable WhenComplete<A>(this IEnumerable e, Func<A> f)
        {
            return e
                .CatchError<Exception>((Exception ex) => { })
                .Then(f);
        }

        public static Future<A> GetFuture<A>(this IEnumerable e, MonoBehaviour m) {
            var future = new Completer<A>();

            e.Then<A>((Action<A>)future.Complete);

            return future;
        }

        public static void Run(this IEnumerable e)
        {
            var enu = e.GetEnumerator();
            while (enu.MoveNext()) { }
        }

        public static IEnumerable ForEachDo(this IEnumerable e, Action f)
        {
            foreach (var n in e)
            {
                f();
                yield return n;
            }
        }
	}

    public static class AtomQ
    {
        public static IEnumerable<A> Iterate<A>(this A a, Func<A, A> f)
        {
            while (true)
            {
                yield return a;
                a = f(a);
            }
        }

        public static IEnumerable<int> To(this int n, int number)
        {
            return n
                .Iterate(x => x + 1)
                .Take(number - n + 1);
        }

        public static B Do<A, B>(this A obj, Func<A, B> f)
        {
            return f(obj);
        }

        public static void Do<A>(this A obj, Action<A> f)
        {
            f(obj);
        }

        public static void MaybeDo<A>(this A obj, Action<A> f)
        {
            if (obj != null)
                Do(obj, f);
        }
    }
}

