using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace KONDO.StateMachine
{
	/// <summary>
	/// StateMachineBase
	/// StateMachine�̃x�[�X�N���X
	/// </summary>
	public abstract class StateMachineBase<T> where T : new()
	{
		// State��Owner
		public T Owner => _stateOwner;

		// ���݂̃X�e�[�g
		public StateBase<T> CurrentState => _currentState;

		// �J�ڂł���X�e�[�g
		private Dictionary<Type, StateBase<T>> _states = new();

		// ���݂̃X�e�[�g
		private StateBase<T> _currentState;

		// State��Owner
		private T _stateOwner;

		// State�����������ꂽ��
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
		/// �X�V����
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
		/// State�̊J�n
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
		/// State�̊J�n
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
		/// State�̊J�n
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
		/// State�̒ǉ�
		/// </summary>
		public async UniTask AddStateAsync(StateBase<T> state)
		{
			if (_states.TryAdd(state.GetType(), state))
			{
				// �ǉ��o�����珉����
				await state.OnInitializeInternalAsync(this);
			}
		}

		/// <summary>
		/// AddState
		/// State�̒ǉ�
		/// </summary>
		public async UniTask AddStatesAsync(IEnumerable<StateBase<T>> states)
		{
			foreach (var state in states)
			{
				if (_states.TryAdd(state.GetType(), state))
				{
					// �ǉ��o�����珉����
					await state.OnInitializeInternalAsync(this);
				}
			}
		}

		/// <summary>
		/// ChangeState
		/// State�̕ύX
		/// </summary>
		internal async UniTask ChangeStateAsync<TState>() where TState : StateBase<T>
		{
			_isInitializedState = false;

			// ��Ɍ��݂�State���I��������
			await _currentState.OnEndInternalAsync(this);

			if (_states.TryGetValue(typeof(TState), out var newState))
			{
				// State��������ΊJ�n����
				_currentState = newState;
				await _currentState.OnStartInternalAsync(this);
			}

			_isInitializedState = true;
		}
	}
}
