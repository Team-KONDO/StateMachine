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
	/// MainStateMachineBehaviour用のCustomEditor
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
		/// 有効時
		/// </summary>
		private void OnEnable()
		{
			// targetをキャスト
			_mainStateMachineBehaviour = target as MainStateMachineBehaviour;

			// SerializedPropertyの取得
			_mainStateNamesProperty = serializedObject.FindProperty("_mainStateNames");
			_firstMainStateNameProperty = serializedObject.FindProperty("_firstMainStateName");

			// 再描画用に更新を追加
			EditorApplication.update += OnUpdate;
		}

		/// <summary>
		/// OnDisable
		/// 無効時
		/// </summary>
		private void OnDisable()
		{
			// 無効時に削除
			EditorApplication.update -= OnUpdate;
		}

		/// <summary>
		/// OnUpdate
		/// 更新処理
		/// </summary>
		private void OnUpdate()
		{
			Repaint();
		}

		/// <summary>
		/// OnInspectorGUI
		/// インスペクターの描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// SerializedObjectの更新
			serializedObject.Update();

			// ランタイム情報の描画
			DrawRuntimeInformation();

			// MainState設定の描画
			DrawMainStateSettings();

			// SerializedPropertyの変更を適用
			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// DrawRuntimeInformation
		/// ランタイム情報の描画
		/// </summary>
		private void DrawRuntimeInformation()
		{
			// 実行時のみ
			if (Application.isPlaying)
			{
				EditorGUILayout.LabelField("Runtime Information");

				EditorGUI.indentLevel++;

				var currentState = _mainStateMachineBehaviour.MainStateMachine.CurrentState;
				if (currentState != null)
				{
					// 実行中のステートと経過時間を表示
					var stringBuilder = new StringBuilder();
					stringBuilder.AppendFormat("CurrentState：{0}", currentState.GetType().Name);
					stringBuilder.AppendFormat("\t経過時間：{0:F2}", currentState.ElapsedTime);
					EditorGUILayout.LabelField(stringBuilder.ToString());
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.BeginHorizontal("Box", GUILayout.MaxHeight(1), GUILayout.ExpandWidth(true));
				EditorGUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// DrawMainStateSettings
		/// MainState設定の描画
		/// </summary>
		private void DrawMainStateSettings()
		{
			EditorGUILayout.LabelField("MainState Settings");

			EditorGUI.indentLevel++;

			// MainStateを継承しているすべてのクラスを取得
			var states = GetMainStateSubClass();

			// 有効なMainStateを選択
			SelectAvailableMainState(states);

			// 一番初めに実行するMainStateを選択
			SelectStartState(states);

			EditorGUI.indentLevel--;
		}

		/// <summary>
		/// GetMainStateSubClass
		/// MainStateを継承しているすべてのクラスを取得
		/// </summary>
		private Dictionary<Type, string> GetMainStateSubClass()
		{
			var states = new Dictionary<Type, string>();

			// AssemblyDefinitionを取得
			var adfGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new string[] { "Assets/" });

			foreach (var adfGuid in adfGuids)
			{
				// GUIからパスに変換してファイル名のみを取得
				var assemblyName = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(adfGuid));

				// Assemblyを読み込む
				var assembly = Assembly.Load(assemblyName);

				if (assembly == null)
				{
					continue;
				}

				// 読み込んだAssemblyからMainStateを継承しているクラスだけを抽出
				foreach (var mainState in assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(MainStateBase))))
				{
					states.TryAdd(mainState, assemblyName);
				};
			}

			var defaultAssemblyName = "Assembly-CSharp";

			// Assemblyを読み込む
			var defaultAssembly = Assembly.Load(defaultAssemblyName);

			if (defaultAssembly != null)
			{
				// 読み込んだAssemblyからMainStateを継承しているクラスだけを抽出
				foreach (var mainState in defaultAssembly.GetTypes().Where(x => x.IsSubclassOf(typeof(MainStateBase))))
				{
					states.TryAdd(mainState, defaultAssemblyName);
				};
			}

			return states;
		}

		/// <summary>
		/// SelectAvailableMainState
		/// 利用可能なMainStateを選択
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

					// 別Assemblyからでも読み込めるようにAssembly名も含める
					var stateNameWithAssemblyName = state.Key.FullName + $",{state.Value}";

					for (var i = 0; i < arraySize; i++)
					{
						// 配列の中身を取得
						var prop = _mainStateNamesProperty.GetArrayElementAtIndex(i);

						// 既に登録済みなら
						if (prop.stringValue == stateNameWithAssemblyName)
						{
							index = i;
						}
					}

					var contains = index != -1;

					var newContains = EditorGUILayout.ToggleLeft(state.Key.FullName, contains);

					// 無効になったとき
					if (contains && !newContains)
					{
						_mainStateNamesProperty.DeleteArrayElementAtIndex(index);
					}

					// 有効にしたとき
					if (!contains && newContains)
					{
						_mainStateNamesProperty.InsertArrayElementAtIndex(arraySize);
						var prop = _mainStateNamesProperty.GetArrayElementAtIndex(arraySize);

						// 値の更新
						prop.stringValue = stateNameWithAssemblyName;
					}
				}
			}
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// SelectStartState
		/// 開始するStateの選択
		/// </summary>
		private void SelectStartState(Dictionary<Type, string> states)
		{
			if (states.Count == 0)
			{
				return;
			}

			var stateNames = states.Select(x => x.Key.FullName).ToArray();

			// 別Assemblyからでも読み込めるようにAssembly名も含める
			var stateNamesWithAssemblyName = states.Select(x => x.Key.FullName + $",{x.Value}").ToArray();

			// 選択済みのStateを探す
			var index = Array.FindIndex(stateNamesWithAssemblyName, x => x == _firstMainStateNameProperty.stringValue);

			// なければ先頭を選択
			if (index < 0)
			{
				index = 0;
			}

			index = EditorGUILayout.Popup(new GUIContent("FirstMainState"), index, stateNames);

			// 更新
			_firstMainStateNameProperty.stringValue = stateNamesWithAssemblyName[index];
		}
	}
}