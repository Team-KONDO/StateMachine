using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace KONDO.StateMachine
{
	/// <summary>
	/// StateMachineBase
	/// StateMachineのベースクラス
	/// </summary>
	public abstract class StateMachineBase<T> where T : new()
	{
		// StateのOwner
		public T Owner => _stateOwner;

		// 現在のステート
		public StateBase<T> CurrentState => _currentState;

		// 遷移できるステート
		private Dictionary<Type, StateBase<T>> _states = new();

		// 現在のステート
		private StateBase<T> _currentState;

		// StateのOwner
		private T _stateOwner;

		// Stateが初期化されたか
		private bool _isInitializedState = false;

		// ----- Constructor -----
		public StateMachineBase()
		{
			_stateOwner = new T();
		}


		// ----- Constructor -----
		public StateMachineBase(T owner)
		{
			_stateOwner = owner ?? new T();
		}

		/// <summary>
		/// Update
		/// 更新処理
		/// </summary>
		public void Update(float deltaTime)
		{
			if (!_isInitializedState)
			{
				return;
			}

			_currentState.OnUpdateInternal(this, deltaTime);
		}

		/// <summary>
		/// StartState
		/// Stateの開始
		/// </summary>
		public async UniTask StartStateAsync<TState>() where TState : StateBase<T>
		{
			_isInitializedState = false;

			if (_states.TryGetValue(typeof(TState), out var newState))
			{
				_currentState = newState;
				await _currentState.OnStartInternalAsync(this);
			}

			_isInitializedState = true;
		}

		/// <summary>
		/// StartState
		/// Stateの開始
		/// </summary>
		public async UniTask StartStateAsync(Type stateType)
		{
			_isInitializedState = false;

			if (_states.TryGetValue(stateType, out var newState))
			{
				_currentState = newState;
				await _currentState.OnStartInternalAsync(this);
			}

			_isInitializedState = true;
		}

		/// <summary>
		/// StartState
		/// Stateの開始
		/// </summary>
		public async UniTask EndStateAsync()
		{
			await _currentState.OnEndInternalAsync(this);

			foreach (var state in _states)
			{
				await state.Value.OnDestroyInternalAsync(this);
			}

			_states.Clear();
		}

		/// <summary>
		/// AddState
		/// Stateの追加
		/// </summary>
		public async UniTask AddStateAsync(StateBase<T> state)
		{
			if (_states.TryAdd(state.GetType(), state))
			{
				// 追加出来たら初期化
				await state.OnInitializeInternalAsync(this);
			}
		}

		/// <summary>
		/// AddState
		/// Stateの追加
		/// </summary>
		public async UniTask AddStatesAsync(IEnumerable<StateBase<T>> states)
		{
			foreach (var state in states)
			{
				if (_states.TryAdd(state.GetType(), state))
				{
					// 追加出来たら初期化
					await state.OnInitializeInternalAsync(this);
				}
			}
		}

		/// <summary>
		/// ChangeState
		/// Stateの変更
		/// </summary>
		internal async UniTask ChangeStateAsync<TState>() where TState : StateBase<T>
		{
			_isInitializedState = false;

			// 先に現在のStateを終了させる
			await _currentState.OnEndInternalAsync(this);

			if (_states.TryGetValue(typeof(TState), out var newState))
			{
				// Stateが見つかれば開始する
				_currentState = newState;
				await _currentState.OnStartInternalAsync(this);
			}

			_isInitializedState = true;
		}
	}
}
