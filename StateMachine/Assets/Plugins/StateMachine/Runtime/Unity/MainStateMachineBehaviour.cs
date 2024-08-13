using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KONDO.StateMachine.Unity
{
	/// <summary>
	/// MainStateMachineBehaviour
	/// MainStateのStateMachineを持つComponent
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class MainStateMachineBehaviour : MonoBehaviour
	{
		// 有効なMainStateの名前
		[SerializeField]
		private string[] _mainStateNames;

		// 初めに実行されるMainState名
		[SerializeField]
		private string _firstMainStateName;

		// 有効なState
		private List<MainStateBase> _mainStates = new();

		// ステートマシン
		public MainStateMachine MainStateMachine { get; private set; }

		/// <summary>
		/// UpdateStateMachine
		/// </summary>
		public void UpdateStateMachine(float deltaTime)
		{
			if (MainStateMachine != null)
			{
				MainStateMachine.Update(deltaTime);
			}
		}

		/// <summary>
		/// Awake
		/// </summary>
		private async void Awake()
		{
			// StatMachineの作成
			MainStateMachine = new MainStateMachine(this);

			// MainStateを初期化
			await InitializeMainStateAsync();

			// Stateの開始
			await MainStateMachine.StartStateAsync(Type.GetType(_firstMainStateName));
		}

		/// <summary>
		/// Update
		/// </summary>
		private void Update()
		{
			if (MainStateMachine != null)
			{
				MainStateMachine.Update(Time.deltaTime);
			}
		}

		/// <summary>
		/// OnDestroy
		/// </summary>
		private async void OnDestroy()
		{
			if (MainStateMachine != null)
			{
				await MainStateMachine.EndStateAsync();
			}
		}

		/// <summary>
		/// InitializeMainState
		/// MainStateの初期化
		/// </summary>
		private async UniTask InitializeMainStateAsync()
		{
			_mainStates.Clear();

			foreach (var stateName in _mainStateNames)
			{
				var type = Type.GetType(stateName);
				var state = (MainStateBase)Activator.CreateInstance(type);
				_mainStates.Add(state);
			}

			await MainStateMachine.AddStatesAsync(_mainStates);
		}
	}
}
