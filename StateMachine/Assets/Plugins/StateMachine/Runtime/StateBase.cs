using R3;
using System.Threading;

namespace KONDO.StateMachine
{
	/// <summary>
	/// StateBase
	/// StateMachine�p��State�̃x�[�X�N���X
	/// </summary>
	public abstract class StateBase<T> where T : new()
	{
		// State���ł̂ݗL����Disposable
		private CompositeDisposable _stateDisposable = new CompositeDisposable();

		// State���ł̂ݗL����CancellationTokenSource
		private CancellationTokenSource _stateCancellationTokenSource = new CancellationTokenSource();

		// �o�ߎ���
		public float ElapsedTime { get; private set; }

		/// <summary>
		/// OnInitialize
		/// ��������
		/// </summary>
		public virtual void OnInitialize(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnStart
		/// �J�n��
		/// </summary>
		public virtual void OnStart(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnUpdate
		/// �X�V
		/// </summary>
		public virtual void OnUpdate(StateMachineBase<T> stateMachine, float deltaTime) { }

		/// <summary>
		/// OnEnd
		/// �I����
		/// </summary>
		public virtual void OnEnd(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnDestroy
		/// �j����
		/// </summary>
		public virtual void OnDestroy(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnInitializeInternal
		/// �����p����������
		/// </summary>
		internal virtual void OnInitializeInternal(StateMachineBase<T> stateMachine)
		{
			// ��ɂ��̃N���X�̏�����������

			// �p����̏������͍Ō�
			OnInitialize(stateMachine);
		}

		/// <summary>
		/// OnStartInternal
		/// �����p�J�n����
		/// </summary>
		internal virtual void OnStartInternal(StateMachineBase<T> stateMachine)
		{
			// �g�p����\��������̂Ő�ɐ���
			_stateCancellationTokenSource = new CancellationTokenSource();

			// �o�ߎ��Ԃ����Z�b�g
			ElapsedTime = 0.0f;

			OnStart(stateMachine);
		}

		/// <summary>
		/// OnUpdateInternal
		/// ���������X�V����
		/// </summary>
		internal virtual void OnUpdateInternal(StateMachineBase<T> stateMachine, float deltaTime)
		{
			// �o�ߎ��Ԃ̍X�V
			ElapsedTime += deltaTime;

			OnUpdate(stateMachine, deltaTime);
		}

		/// <summary>
		/// OnEndInternal
		/// �����p�I������
		/// </summary>
		internal virtual void OnEndInternal(StateMachineBase<T> stateMachine)
		{
			// �I�������͌p���悩��
			OnEnd(stateMachine);

			// �g���܂킹��悤��Clear
			_stateDisposable.Clear();

			// �L�����Z�����Ĕj������
			_stateCancellationTokenSource.Cancel();
			_stateCancellationTokenSource = null;
		}

		/// <summary>
		/// OnDestroyInternal
		/// �����p�j������
		/// </summary>
		internal virtual void OnDestroyInternal(StateMachineBase<T> stateMachine)
		{
			// �j���������p���悩��
			OnDestroy(stateMachine);

			// Dispose����
			_stateDisposable.Dispose();
			_stateCancellationTokenSource?.Dispose();
		}

		/// <summary>
		/// ChangeState
		/// State�̕ύX
		/// </summary>
		protected void ChangeState<TState>(StateMachineBase<T> stateMachine) where TState : StateBase<T>
		{
			stateMachine.ChangeState<TState>();
		}
	}
}
