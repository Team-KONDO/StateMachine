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
			_currentState.OnUpdateInternal(this, deltaTime);
		}

		/// <summary>
		/// StartState
		/// State�̊J�n
		/// </summary>
		public void StartState<TState>() where TState : StateBase<T>
		{
			if (_states.TryGetValue(typeof(TState), out var newState))
			{
				_currentState = newState;
				_currentState.OnStartInternal(this);
			}
		}

		/// <summary>
		/// StartState
		/// State�̊J�n
		/// </summary>
		public void StartState(Type stateType)
		{
			if (_states.TryGetValue(stateType, out var newState))
			{
				_currentState = newState;
				_currentState.OnStartInternal(this);
			}
		}

		/// <summary>
		/// StartState
		/// State�̊J�n
		/// </summary>
		public void EndState()
		{
			_currentState.OnEndInternal(this);

			foreach (var state in _states)
			{
				state.Value.OnDestroyInternal(this);
			}

			_states.Clear();
		}

		/// <summary>
		/// AddState
		/// State�̒ǉ�
		/// </summary>
		public void AddState(StateBase<T> state)
		{
			if (_states.TryAdd(state.GetType(), state))
			{
				// �ǉ��o�����珉����
				state.OnInitializeInternal(this);
			}
		}

		/// <summary>
		/// AddState
		/// State�̒ǉ�
		/// </summary>
		public void AddStates(IEnumerable<StateBase<T>> states)
		{
			foreach (var state in states)
			{
				if (_states.TryAdd(state.GetType(), state))
				{
					// �ǉ��o�����珉����
					state.OnInitializeInternal(this);
				}
			}
		}

		/// <summary>
		/// ChangeState
		/// State�̕ύX
		/// </summary>
		internal void ChangeState<TState>() where TState : StateBase<T>
		{
			// ��Ɍ��݂�State���I��������
			_currentState.OnEndInternal(this);

			if (_states.TryGetValue(typeof(TState), out var newState))
			{
				// State��������ΊJ�n����
				_currentState = newState;
				_currentState.OnStartInternal(this);
			}
		}
	}
}
