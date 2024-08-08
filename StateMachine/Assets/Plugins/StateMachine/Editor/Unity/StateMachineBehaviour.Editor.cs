using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KONDO.StateMachine.Unity
{
	/// <summary>
	/// MainStateMachineBehaviourEditor
	/// MainStateMachineBehaviour�p��CustomEditor
	/// </summary>
	[CustomEditor(typeof(MainStateMachineBehaviour))]
	public class MainStateMachineBehaviourEditor : Editor
	{
		// MainStateMachine
		private MainStateMachineBehaviour _mainStateMachineBehaviour;

		// SerializedProperty
		private SerializedProperty _mainStateNamesProperty;
		private SerializedProperty _firstMainStateNameProperty;

		/// <summary>
		/// OnEnable
		/// �L����
		/// </summary>
		private void OnEnable()
		{
			// target���L���X�g
			_mainStateMachineBehaviour = target as MainStateMachineBehaviour;

			// SerializedProperty�̎擾
			_mainStateNamesProperty = serializedObject.FindProperty("_mainStateNames");
			_firstMainStateNameProperty = serializedObject.FindProperty("_firstMainStateName");

			// �ĕ`��p�ɍX�V��ǉ�
			EditorApplication.update += OnUpdate;
		}

		/// <summary>
		/// OnDisable
		/// ������
		/// </summary>
		private void OnDisable()
		{
			// �������ɍ폜
			EditorApplication.update -= OnUpdate;
		}

		/// <summary>
		/// OnUpdate
		/// �X�V����
		/// </summary>
		private void OnUpdate()
		{
			Repaint();
		}

		/// <summary>
		/// OnInspectorGUI
		/// �C���X�y�N�^�[�̕`��
		/// </summary>
		public override void OnInspectorGUI()
		{
			// SerializedObject�̍X�V
			serializedObject.Update();

			// �����^�C�����̕`��
			DrawRuntimeInformation();

			// MainState�ݒ�̕`��
			DrawMainStateSettings();

			// SerializedProperty�̕ύX��K�p
			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// DrawRuntimeInformation
		/// �����^�C�����̕`��
		/// </summary>
		private void DrawRuntimeInformation()
		{
			// ���s���̂�
			if (Application.isPlaying)
			{
				EditorGUILayout.LabelField("Runtime Information");

				EditorGUI.indentLevel++;

				var currentState = _mainStateMachineBehaviour.MainStateMachine.CurrentState;
				if (currentState != null)
				{
					// ���s���̃X�e�[�g�ƌo�ߎ��Ԃ�\��
					var stringBuilder = new StringBuilder();
					stringBuilder.AppendFormat("CurrentState�F{0}", currentState.GetType().Name);
					stringBuilder.AppendFormat("\t�o�ߎ��ԁF{0:F2}", currentState.ElapsedTime);
					EditorGUILayout.LabelField(stringBuilder.ToString());
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.BeginHorizontal("Box", GUILayout.MaxHeight(1), GUILayout.ExpandWidth(true));
				EditorGUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// DrawMainStateSettings
		/// MainState�ݒ�̕`��
		/// </summary>
		private void DrawMainStateSettings()
		{
			EditorGUILayout.LabelField("MainState Settings");

			EditorGUI.indentLevel++;

			// MainState���p�����Ă��邷�ׂẴN���X���擾
			var states = GetMainStateSubClass();

			// �L����MainState��I��
			SelectAvailableMainState(states);

			// ��ԏ��߂Ɏ��s����MainState��I��
			SelectStartState(states);

			EditorGUI.indentLevel--;
		}

		/// <summary>
		/// GetMainStateSubClass
		/// MainState���p�����Ă��邷�ׂẴN���X���擾
		/// </summary>
		private Dictionary<Type, string> GetMainStateSubClass()
		{
			var states = new Dictionary<Type, string>();

			// AssemblyDefinition���擾
			var adfGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new string[] { "Assets/" });

			foreach (var adfGuid in adfGuids)
			{
				// GUI����p�X�ɕϊ����ăt�@�C�����݂̂��擾
				var assemblyName = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(adfGuid));

				// Assembly��ǂݍ���
				var assembly = Assembly.Load(assemblyName);

				if (assembly == null)
				{
					continue;
				}

				// �ǂݍ���Assembly����MainState���p�����Ă���N���X�����𒊏o
				foreach (var mainState in assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(MainStateBase))))
				{
					states.TryAdd(mainState, assemblyName);
				};
			}

			var defaultAssemblyName = "Assembly-CSharp";

			// Assembly��ǂݍ���
			var defaultAssembly = Assembly.Load(defaultAssemblyName);

			if (defaultAssembly != null)
			{
				// �ǂݍ���Assembly����MainState���p�����Ă���N���X�����𒊏o
				foreach (var mainState in defaultAssembly.GetTypes().Where(x => x.IsSubclassOf(typeof(MainStateBase))))
				{
					states.TryAdd(mainState, defaultAssemblyName);
				};
			}

			return states;
		}

		/// <summary>
		/// SelectAvailableMainState
		/// ���p�\��MainState��I��
		/// </summary>
		private void SelectAvailableMainState(Dictionary<Type, string> states)
		{
			if (states.Count == 0)
			{
				EditorGUILayout.LabelField("Not Found MainState Sub Class");
				return;
			}

			EditorGUILayout.LabelField("States");

			EditorGUILayout.BeginVertical("Box");
			{
				foreach (var state in states)
				{
					var index = -1;
					var arraySize = _mainStateNamesProperty.arraySize;

					// ��Assembly����ł��ǂݍ��߂�悤��Assembly�����܂߂�
					var stateNameWithAssemblyName = state.Key.FullName + $",{state.Value}";

					for (var i = 0; i < arraySize; i++)
					{
						// �z��̒��g���擾
						var prop = _mainStateNamesProperty.GetArrayElementAtIndex(i);

						// ���ɓo�^�ς݂Ȃ�
						if (prop.stringValue == stateNameWithAssemblyName)
						{
							index = i;
						}
					}

					var contains = index != -1;

					var newContains = EditorGUILayout.ToggleLeft(state.Key.FullName, contains);

					// �����ɂȂ����Ƃ�
					if (contains && !newContains)
					{
						_mainStateNamesProperty.DeleteArrayElementAtIndex(index);
					}

					// �L���ɂ����Ƃ�
					if (!contains && newContains)
					{
						_mainStateNamesProperty.InsertArrayElementAtIndex(arraySize);
						var prop = _mainStateNamesProperty.GetArrayElementAtIndex(arraySize);

						// �l�̍X�V
						prop.stringValue = stateNameWithAssemblyName;
					}
				}
			}
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// SelectStartState
		/// �J�n����State�̑I��
		/// </summary>
		private void SelectStartState(Dictionary<Type, string> states)
		{
			if (states.Count == 0)
			{
				return;
			}

			var stateNames = states.Select(x => x.Key.FullName).ToArray();

			// ��Assembly����ł��ǂݍ��߂�悤��Assembly�����܂߂�
			var stateNamesWithAssemblyName = states.Select(x => x.Key.FullName + $",{x.Value}").ToArray();

			// �I���ς݂�State��T��
			var index = Array.FindIndex(stateNamesWithAssemblyName, x => x == _firstMainStateNameProperty.stringValue);

			// �Ȃ���ΐ擪��I��
			if (index < 0)
			{
				index = 0;
			}

			index = EditorGUILayout.Popup(new GUIContent("FirstMainState"), index, stateNames);

			// �X�V
			_firstMainStateNameProperty.stringValue = stateNamesWithAssemblyName[index];
		}
	}
}