using Cysharp.Threading.Tasks;
using R3;
using System.Threading;

namespace KONDO.StateMachine
{
	/// <summary>
	/// StateBase
	/// StateMachine用のStateのベースクラス
	/// </summary>
	public abstract class StateBase<T> where T : new()
	{
		// State内でのみ有効なDisposable
		private CompositeDisposable _stateDisposable = new CompositeDisposable();

		// State内でのみ有効なCancellationTokenSource
		private CancellationTokenSource _stateCancellationTokenSource = new CancellationTokenSource();

		private bool _started = false;

		// 経過時間
		public float ElapsedTime { get; private set; }

		/// <summary>
		/// OnInitialize
		/// 初期化時
		/// </summary>
		public virtual async UniTask OnInitializeAsync(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnStart
		/// 開始時
		/// </summary>
		public virtual async UniTask OnStartAsync(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnUpdate
		/// 更新
		/// </summary>
		public virtual void OnUpdate(StateMachineBase<T> stateMachine, float deltaTime) { }

		/// <summary>
		/// OnEnd
		/// 終了時
		/// </summary>
		public virtual async UniTask OnEndAsync(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnDestroy
		/// 破棄時
		/// </summary>
		public virtual async UniTask OnDestroyAsync(StateMachineBase<T> stateMachine) { }

		/// <summary>
		/// OnInitializeInternal
		/// 内部用初期化処理
		/// </summary>
		internal virtual async UniTask OnInitializeInternalAsync(StateMachineBase<T> stateMachine)
		{
			// 先にこのクラスの初期化をする

			// 継承先の初期化は最後
			await OnInitializeAsync(stateMachine);
		}

		/// <summary>
		/// OnStartInternal
		/// 内部用開始処理
		/// </summary>
		internal virtual async UniTask OnStartInternalAsync(StateMachineBase<T> stateMachine)
		{
			// 使用する可能性があるので先に生成
			_stateCancellationTokenSource = new CancellationTokenSource();

			// 経過時間をリセット
			ElapsedTime = 0.0f;

			await OnStartAsync(stateMachine);

			_started = true;
		}

		/// <summary>
		/// OnUpdateInternal
		/// 内部処理更新処理
		/// </summary>
		internal virtual void OnUpdateInternal(StateMachineBase<T> stateMachine, float deltaTime)
		{
			if (!_started)
			{
				return;
			}

			// 経過時間の更新
			ElapsedTime += deltaTime;

			OnUpdate(stateMachine, deltaTime);
		}

		/// <summary>
		/// OnEndInternal
		/// 内部用終了処理
		/// </summary>
		internal virtual async UniTask OnEndInternalAsync(StateMachineBase<T> stateMachine)
		{
			// 終了処理は継承先から
			await OnEndAsync(stateMachine);

			// 使いまわせるようにClear
			_stateDisposable.Clear();

			// キャンセルして破棄する
			_stateCancellationTokenSource.Cancel();
			_stateCancellationTokenSource = null;

			_started = false;
		}

		/// <summary>
		/// OnDestroyInternal
		/// 内部用破棄処理
		/// </summary>
		internal virtual async UniTask OnDestroyInternalAsync(StateMachineBase<T> stateMachine)
		{
			// 破棄処理も継承先から
			await OnDestroyAsync(stateMachine);

			// Disposeする
			_stateDisposable.Dispose();
			_stateCancellationTokenSource?.Dispose();
		}

		/// <summary>
		/// ChangeState
		/// Stateの変更
		/// </summary>
		protected async UniTask ChangeStateAsync<TState>(StateMachineBase<T> stateMachine) where TState : StateBase<T>
		{
			await stateMachine.ChangeStateAsync<TState>();
		}
	}
}
