using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace PavoStudio.MAE
{
	class PSMuscleEditorWindow : EditorWindow
	{
		public const string Version = "Muscle Animation Editor 2.2";

		public enum TAB
		{
			MUSCLE = 0,
			OPTIONS = 1
		}

		public PSTab curTab;
		public PSBaseReflection reflection;
		public AnimationClip clip;
		public float time;
		public int frame;
		public GameObject target;
		public PSTabMuscle tabMuscle;
		public PSTabOption tabOption;
		public PSMuscleHandle muscleHandle;
		public Vector2 scrollPos;

		private bool onFocus;
		private int skipCount;

		[MenuItem ("Window/Muscle Animation Editor")]
		static void InitWindow ()
		{
			PSMuscleEditorWindow window = (PSMuscleEditorWindow)EditorWindow.GetWindow (typeof(PSMuscleEditorWindow));
			window.Show ();
		}

		public void Initialize ()
		{
			if (tabMuscle == null) {
				tabMuscle = new PSTabMuscle ();
				tabMuscle.Init ();
			}

			if (tabOption == null)
				tabOption = new PSTabOption ();
			if (muscleHandle == null)
				muscleHandle = new PSMuscleHandle ();

			muscleHandle.setTabs (tabMuscle, tabOption);
			if (curTab == null)
				curTab = tabMuscle;
		}

		void OnInspectorUpdate ()
		{
			this.Repaint ();
			//UpdateValue (true);
		}

		void OnFocus ()
		{
			//Debug.Log("OnFocus");
			onFocus = true;
		}

		void OnLostFocus ()
		{
			//Debug.Log("OnLostFocus");
			onFocus = false;
		}

		public void OnEnable ()
		{
			this.titleContent = new GUIContent ("MAEditor");
			Initialize ();
			SceneView.onSceneGUIDelegate += ShowHandles;
			EditorApplication.update += UpdateTarget;
		}

		public void OnDisable ()
		{
			SceneView.onSceneGUIDelegate -= ShowHandles;
			EditorApplication.update -= UpdateTarget;
		}

		void UpdateTarget ()
		{
			// Update target every 10 frames
			if (skipCount < 10) {
				skipCount++;
				return;
			}

			skipCount = 0;
			_UpdateTarget ();
		}

		private void _UpdateTarget ()
		{
			if (reflection == null)
				reflection = PSReflectionFactory.GetReflection ();	

			if (reflection == null)
				return;

			GameObject newTarget = reflection.GetRootObject ();
			if (newTarget == null) {
				target = null;
				return;
			}

			if (newTarget != target) {
				if (!IsValidTarget (newTarget)) {
					target = null;
					return;
				}
				target = newTarget;
				tabMuscle.SetTarget (target);
				tabOption.SetTarget (target);
				muscleHandle.SetTarget (target);
			}

			clip = reflection.GetClip ();
			time = reflection.GetTime ();
			frame = reflection.GetFrame ();

			tabMuscle.SetClip (clip);
			tabMuscle.SetTimeFrame (time, frame);

			if (AnimationMode.InAnimationMode () && !onFocus) {
				tabMuscle.OnUpdateValue ();
				//tabMuscle.ResampleAnimation();
			}
		}

		void ShowHandles (SceneView sceneview)
		{
			if (muscleHandle != null && target != null && AnimationMode.InAnimationMode ())
				muscleHandle.ShowHandles ();
		}

		/***********************************************
	 *  GUI METHODS
	 ***********************************************/

		void OnGUI ()
		{
			if (reflection == null) {
				reflection = PSReflectionFactory.GetReflection ();	
			}

			if (target == null) {
				EditorGUILayout.HelpBox ("No humanoid target found, please select humanoid gameobject in the hierarchy.", MessageType.None);
				if (GUILayout.Button ("Refresh", EditorStyles.miniButton, GUILayout.Width (100))) {
					reflection.Init ();
				}
				return;
			}

			this.InfoGUI ();

			EditorGUILayout.Space ();
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Toggle (curTab == tabMuscle, "MUSCLE", EditorStyles.toolbarButton))
				curTab = tabMuscle;

			if (GUILayout.Toggle (curTab == tabOption, "OPTIONS", EditorStyles.toolbarButton))
				curTab = tabOption;
			EditorGUILayout.EndHorizontal ();

			this.TabGUI ();

			EditorGUILayout.BeginHorizontal (EditorStyles.helpBox);
			EditorGUILayout.LabelField (Version);
			EditorGUILayout.LabelField ("Email: pavostudio@hotmail.com", GUILayout.Width (190));
			EditorGUILayout.EndHorizontal ();
		}

		protected void TabGUI ()
		{
			scrollPos = EditorGUILayout.BeginScrollView (scrollPos);

			EditorGUI.BeginDisabledGroup (!AnimationMode.InAnimationMode ());

			curTab.OnTabGUI ();

			EditorGUI.EndDisabledGroup ();

			EditorGUILayout.EndScrollView ();

		}

		protected void InfoGUI ()
		{
			EditorGUILayout.BeginHorizontal (EditorStyles.toolbar);
			EditorGUILayout.LabelField ("Information");
			if (GUILayout.Button ("Refresh", EditorStyles.toolbarButton, GUILayout.Width (80))) {
				reflection.Init ();
			}
			EditorGUILayout.EndHorizontal ();

			Color tmpColor = GUI.color;
			GUI.color = Color.yellow;
			EditorGUILayout.HelpBox ("This tool will overwrite animation clip file directly, you should back up your clip files before using it!", MessageType.Warning);
			GUI.color = tmpColor;

			EditorGUILayout.HelpBox (string.Format ("Target: {0}\nClip: {1}\nTime: {2}s\nFrame: {3}", target, clip, time, frame), MessageType.None);

		}

		/***********************************************
	 * 
	 ***********************************************/

		private bool IsValidTarget (GameObject obj)
		{
			if (obj == null)
				return false;
			
			Animator animator = obj.GetComponent (typeof(Animator)) as Animator;
			if (animator == null)
				return false;
			
			Avatar avatar = animator.avatar;
			if (avatar != null && avatar.isValid && avatar.isHuman) {
				return true;
			}

			return false;
		}

	}
}