using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Atoms {

	public class StateMachine<A> : IEnumerable
	{
		public A initialState;
		A _state;
		public Dictionary<A,StateBehaviour<A>> map;
		
		public StateMachine (A initialState, params StateBehaviour<A>[] behaviours)
		{
			this.initialState = this._state = initialState;
			this.map = new Dictionary<A, StateBehaviour<A>> ();
			
			foreach (var m in behaviours)
				map.Add (m.key, m);
		}
		
		public IEnumerator GetEnumerator ()
		{
			StateBehaviour<A> actual = map [initialState];
			StateBehaviour<A> nextBehaviour = null;
			IEnumerator enu = actual.GetEnumerator();
			bool move;
			
			Debug.Log (actual.key);
			
			actual.OnEnter();
            System.Action changeState = () => {
				//Debug.Log (_state);
				
				actual.OnExit();
				nextBehaviour.OnEnter();
				
				actual = nextBehaviour;
				enu = actual.GetEnumerator();
			};
			
			while ((move = enu.MoveNext()) || actual.transitive) 
			{
				if (! move)
				{
					_state = actual.onFinish();
					nextBehaviour = map [_state];
					
					changeState();
					
					continue;
				}
				
				yield return enu.Current;
				
				_state = actual.transitionFunction (_state);
				nextBehaviour = map [_state];
				
				
				if (nextBehaviour != actual)
				{
					changeState();
				}
			}
		}
		
		public A state {get {return _state;}}
		
		public static StateMachine<A> _ (A initialState, params StateBehaviour<A>[] behaviours)
		{
			return new StateMachine<A> (initialState, behaviours);	
		}
		
		
	}
	
	public class StateBehaviour<A> : IEnumerable
	{
		public A key;
		public Func<A, A> transitionFunction;
		public Func<A> onFinish;
		public bool restartOnEnter;
		
		public event System.Action onEnter;
		public event System.Action onExit;
		
		public bool transitive = false;
		
		IEnumerable behaviour;
		IEnumerator _enumerator;

        public void OnEnter()
        {
            if (onEnter != null)
                onEnter();
        }

        public void OnExit()
        {
            if (onExit != null)
                onExit();
        }

        public IEnumerator GetEnumerator()
		{
			return restartOnEnter ? behaviour.GetEnumerator() : _enumerator;
		}
		
		public StateBehaviour (A key, Func<A, A> transitionFunction, IEnumerable behaviour, bool restartOnEnter = false)
		{
			this.key = key;
			this.behaviour = behaviour;
			this.transitionFunction = transitionFunction;
			this.restartOnEnter = restartOnEnter;
			
			_enumerator = behaviour.GetEnumerator ();
		}
		
		public StateBehaviour (A key, Func<A, A> transitionFunction, IEnumerable behaviour, Func<A> onFinish, bool restartOnEnter = false) : this (key, transitionFunction, behaviour, restartOnEnter)
		{
			this.transitive = true;
			this.onFinish = onFinish;
		}
		
		public static StateBehaviour<A> _ (A key, Func<A, A> transitionFunction, IEnumerable behaviour, bool restartOnEnter = false)
		{
			return new StateBehaviour<A> (key, transitionFunction, behaviour, restartOnEnter);
		}
		
		public static StateBehaviour<A> _ (A key, Func<A, A> f, IEnumerable behaviour, Func<A> onFinish, bool restartOnEnter = false)
		{
			return new StateBehaviour<A> (key, f, behaviour, onFinish, restartOnEnter);
		}
	}
	
	public class TerminalState<A> : StateBehaviour<A>
	{
		
		public TerminalState (A key) : base (key, Id, Atom.DoNothing)
		{
			
		}

        private static A Id(A value)
        {
            return value;
        }
		
		public static TerminalState<A> _ (A key)
		{
			return new TerminalState<A> (key);	
		}
		
	}
	
	public class AbsorvingState<A> : StateBehaviour<A>
	{
		public AbsorvingState (A key, IEnumerable behaviour) : base (key, Id, behaviour) {}

        private static A Id(A value)
        {
            return value;
        }

		public static AbsorvingState<A> _ (A key, IEnumerable behaviour)
		{
			return new AbsorvingState<A> (key, behaviour);	
		}
	}
}
