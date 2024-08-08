using System;
using System.Collections.Generic;
using UnityEngine;

namespace KONDO.StateMachine.Unity
{
	/// <summary>
	/// MainStateMachineBehaviour
	/// MainState��StateMachine������Component
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class MainStateMachineBehaviour : MonoBehaviour
	{
		// �L����MainState�̖��O
		[SerializeField]
		private string[] _mainStateNames;

		// ���߂Ɏ��s�����MainState��
		[SerializeField]
		private string _firstMainStateName;

		// �L����State
		private List<MainStateBase> _mainStates = new();

		// �X�e�[�g�}�V��
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
		private void Awake()
		{
			// StatMachine�̍쐬
			MainStateMachine = new MainStateMachine(this);

			// MainState��������
			InitializeMainState();

			// State�̊J�n
			MainStateMachine.StartState(Type.GetType(_firstMainStateName));
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
		private void OnDestroy()
		{
			if (MainStateMachine != null)
			{
				MainStateMachine.EndState();
			}
		}

		/// <summary>
		/// InitializeMainState
		/// MainState�̏�����
		/// </summary>
		private void InitializeMainState()
		{
			_mainStates.Clear();

			foreach (var stateName in _mainStateNames)
			{
				var type = Type.GetType(stateName);
				var state = (MainStateBase)Activator.CreateInstance(type);
				_mainStates.Add(state);
			}

			MainStateMachine.AddStates(_mainStates);
		}
	}
}
