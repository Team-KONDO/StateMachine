using Cysharp.Threading.Tasks;
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

		private bool _started = false;

		// �o�ߎ���
		public float ElapsedTime { get; private set; }

		/// <summary>
		/// OnInitialize
		/// ��������
		/// </summary>
		public virtual async UniTask OnInitializeAsync(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnStart
		/// �J�n��
		/// </summary>
		public virtual async UniTask OnStartAsync(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnUpdate
		/// �X�V
		/// </summary>
		public virtual void OnUpdate(StateMachineBase<T> stateMachine, float deltaTime) { }

		/// <summary>
		/// OnEnd
		/// �I����
		/// </summary>
		public virtual async UniTask OnEndAsync(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnDestroy
		/// �j����
		/// </summary>
		public virtual async UniTask OnDestroyAsync(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnInitializeInternal
		/// �����p����������
		/// </summary>
		internal virtual async UniTask OnInitializeInternalAsync(StateMachineBase<T> stateMachine)
		{
			// ��ɂ��̃N���X�̏�����������

			// �p����̏������͍Ō�
			await OnInitializeAsync(stateMachine);
		}

		/// <summary>
		/// OnStartInternal
		/// �����p�J�n����
		/// </summary>
		internal virtual async UniTask OnStartInternalAsync(StateMachineBase<T> stateMachine)
		{
			// �g�p����\��������̂Ő�ɐ���
			_stateCancellationTokenSource = new CancellationTokenSource();

			// �o�ߎ��Ԃ����Z�b�g
			ElapsedTime = 0.0f;

			await OnStartAsync(stateMachine);

			_started = true;
		}

		/// <summary>
		/// OnUpdateInternal
		/// ���������X�V����
		/// </summary>
		internal virtual void OnUpdateInternal(StateMachineBase<T> stateMachine, float deltaTime)
		{
			if (!_started)
			{
				return;
			}

			// �o�ߎ��Ԃ̍X�V
			ElapsedTime += deltaTime;

			OnUpdate(stateMachine, deltaTime);
		}

		/// <summary>
		/// OnEndInternal
		/// �����p�I������
		/// </summary>
		internal virtual async UniTask OnEndInternalAsync(StateMachineBase<T> stateMachine)
		{
			// �I�������͌p���悩��
			await OnEndAsync(stateMachine);

			// �g���܂킹��悤��Clear
			_stateDisposable.Clear();

			// �L�����Z�����Ĕj������
			_stateCancellationTokenSource.Cancel();
			_stateCancellationTokenSource = null;

			_started = false;
		}

		/// <summary>
		/// OnDestroyInternal
		/// �����p�j������
		/// </summary>
		internal virtual async UniTask OnDestroyInternalAsync(StateMachineBase<T> stateMachine)
		{
			// �j���������p���悩��
			await OnDestroyAsync(stateMachine);

			// Dispose����
			_stateDisposable.Dispose();
			_stateCancellationTokenSource?.Dispose();
		}

		/// <summary>
		/// ChangeState
		/// State�̕ύX
		/// </summary>
		protected async UniTask ChangeStateAsync<TState>(StateMachineBase<T> stateMachine) where TState : StateBase<T>
		{
			await stateMachine.ChangeStateAsync<TState>();
		}
	}
}
