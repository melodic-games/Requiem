using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PavoStudio.MAE
{
	[System.Serializable]
	class PSTabMuscle : PSTab
	{

		public bool[] muscleToggle;
		public string[] muscleName;
		public float[] muscleValue;
		public float[] muscleMasterValue;
		public float[] muscleFingerValue;
		public bool[] muscleBodyGroupToggle;
		public EditorCurveBinding[] curveBindings;
		public string[] propertyMuscleName;
		public int muscleCount;
		public int curSelectGroup;
		public int curSelectMasterGroup;
		public bool mirror;
		public bool rootT;
		public bool rootQ;
		public float[] rootValue;
		public bool[] rootToggle;
		public bool showRootGUI = true;
		public bool showMuscleGroupGUI = true;
		public bool showMuscleGUI = true;

		private bool resample = false;
		private List<EditorCurveBinding> bindingList = new List<EditorCurveBinding> ();

		public PSTabMuscle ()
		{
			//Init ();
		}

		public void Init ()
		{
			int len = PSMuscleDefine.muscle.Length;
			this.muscleBodyGroupToggle = new bool[len];
			for (int i = 0; i < len; i++) {
				this.muscleBodyGroupToggle [i] = false;
			}
			this.muscleName = PSMuscleDefine.GetMuscleName ();

			this.propertyMuscleName = PSMuscleDefine.GetPropertyMuscleName ();

			this.muscleCount = HumanTrait.MuscleCount;
			this.muscleToggle = new bool[this.muscleCount];
			this.muscleValue = new float[this.muscleCount];
			for (int k = 0; k < this.muscleCount; k++) {
				this.muscleToggle [k] = false;
				this.muscleValue [k] = 0f;
			}

			len = PSMuscleDefine.masterMuscle.Length;
			this.muscleMasterValue = new float[len];
			for (int m = 0; m < len; m++) {
				this.muscleMasterValue [m] = 0f;
			}

			len = PSMuscleDefine.fingerMuscle.Length;
			this.muscleFingerValue = new float[len];
			for (int m = 0; m < len; m++) {
				this.muscleFingerValue [m] = 0f;
			}

			len = PSMuscleDefine.rootProperty.Length;
			rootValue = new float[len];
			rootToggle = new bool[len];

			Undo.undoRedoPerformed += UndoCallback;
		}

		void UndoCallback ()
		{
			resample = true;
			OnUpdateValue ();
		}

		/***********************************************
	 *  override 
	 ***********************************************/

		public override void OnTargetChange ()
		{
			if (target == null) {
				PSLogger.Log ("Target is null");
				return;
			}

			EditorCurveBinding[] bindings = AnimationUtility.GetAnimatableBindings (target, target);
			bindingList.Clear ();

			for (int i = 0; i < this.propertyMuscleName.Length; i++) {
				for (int j = 0; j < bindings.Length; j++) {
					EditorCurveBinding binding = bindings [j];
					if (binding.type.Equals (typeof(Animator)) && binding.propertyName == this.propertyMuscleName [i]) {
						bindingList.Add (binding);
						break;
					}
				}
			}	
			curveBindings = bindingList.ToArray ();
		}

		public override void OnUpdateValue ()
		{
			if (target == null || clip == null) {
				PSLogger.Log ("Target or clip is null");
				return;
			}

			this.muscleToggle = new bool[this.muscleCount];
			this.muscleValue = new float[this.muscleCount];
			rootValue = new float[PSMuscleDefine.rootProperty.Length];
			rootToggle = new bool[PSMuscleDefine.rootProperty.Length];
			rootT = false;
			rootQ = false;

			foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip)) {
				if (!binding.type.Equals (typeof(Animator)))
					continue;
				//Debug.Log(binding.propertyName);
				for (int i = 0, len = PSMuscleDefine.rootProperty.Length; i < len; i++) {
					if (binding.propertyName == PSMuscleDefine.rootProperty [i]) {
						AnimationUtility.GetFloatValue (target, binding, out rootValue [i]);
						rootToggle [i] = true;
						if (i < 3)
							rootT = true;
						else
							rootQ = true;
						break;
					}
				}

				for (int i = 0, len = this.propertyMuscleName.Length; i < len; i++) {
					if (binding.propertyName == this.propertyMuscleName [i]) {
						AnimationUtility.GetFloatValue (target, binding, out muscleValue [i]);
						muscleToggle [i] = true;
						break;
					}
				}
			}
		}

		public override void OnTabGUI ()
		{
			this.RootGUI ();
			EditorGUILayout.Space ();
			this.MuscleGroupGUI ();
			EditorGUILayout.Space ();
			this.MuscleGUI ();
			EditorGUILayout.Space ();

			ResampleAnimation ();
		}

		/***********************************************
	 *  GUI
	 ***********************************************/

		protected void RootGUI ()
		{
			EditorGUILayout.BeginHorizontal (EditorStyles.toolbar);
			showRootGUI = EditorGUILayout.Foldout (showRootGUI, "Root Transform");

			if (GUILayout.Button ("Reset", EditorStyles.toolbarButton, GUILayout.Width (80))) {
				for (int i = 0; i < PSMuscleDefine.rootProperty.Length; i++) {
					rootValue [i] = 0;
					rootToggle [i] = false;
					rootT = false;
					rootQ = false;
					WritePropertyValue (PSMuscleDefine.rootProperty [i], rootValue [i], rootToggle [i]);
				}

			}
			EditorGUILayout.EndHorizontal ();

			if (!showRootGUI)
				return;

			RootTGUI ();
			RootQGUI ();
		}

		private void RootTGUI ()
		{
			bool toggle = EditorGUILayout.BeginToggleGroup ("RootT", rootT);
			bool toggleChange = toggle != rootT;
			rootT = toggle;

			EditorGUI.indentLevel++;
			for (int i = 0; i < 3; i++) {
				float oldValue = rootValue [i];
				if (toggleChange) {
					rootToggle [i] = toggle;
					WritePropertyValue (PSMuscleDefine.rootProperty [i], oldValue, toggle);
				}

				EditorGUILayout.BeginHorizontal ();

				float value = EditorGUILayout.FloatField (PSMuscleDefine.rootT [i], oldValue);
				if (oldValue != value) {
					WritePropertyValue (PSMuscleDefine.rootProperty [i], value, true);

					rootValue [i] = value;
					rootToggle [i] = true;
				}

				EditorGUILayout.EndHorizontal ();
			}
			EditorGUI.indentLevel--;

			EditorGUILayout.EndToggleGroup ();

		}

		private void RootQGUI ()
		{
			bool toggle = EditorGUILayout.BeginToggleGroup ("RootQ", rootQ);
			if (toggle != rootQ) {
				rootToggle [3] = toggle;
				rootToggle [4] = toggle;
				rootToggle [5] = toggle;
				rootToggle [6] = toggle;

				// init values
				if (rootValue [6] == 0) {
					Quaternion quat = Quaternion.identity;
					rootValue [3] = quat.x;
					rootValue [4] = quat.y;
					rootValue [5] = quat.z;
					rootValue [6] = quat.w;
				}

				WritePropertyValue (PSMuscleDefine.rootProperty [3], rootValue [3], rootToggle [3]);
				WritePropertyValue (PSMuscleDefine.rootProperty [4], rootValue [4], rootToggle [4]);
				WritePropertyValue (PSMuscleDefine.rootProperty [5], rootValue [5], rootToggle [5]);
				WritePropertyValue (PSMuscleDefine.rootProperty [6], rootValue [6], rootToggle [6]);

				rootQ = toggle;
			}

			EditorGUI.indentLevel++;

			Quaternion q = new Quaternion (rootValue [3], rootValue [4], rootValue [5], rootValue [6]);
			float x = EditorGUILayout.FloatField (PSMuscleDefine.rootQ [0], q.eulerAngles.x);
			float y = EditorGUILayout.FloatField (PSMuscleDefine.rootQ [1], q.eulerAngles.y);
			float z = EditorGUILayout.FloatField (PSMuscleDefine.rootQ [2], q.eulerAngles.z);

			if (x != q.eulerAngles.x || y != q.eulerAngles.y || z != q.eulerAngles.z) {
				q.eulerAngles = new Vector3 (x, y, z);
				rootValue [3] = q.x;
				rootValue [4] = q.y;
				rootValue [5] = q.z;
				rootValue [6] = q.w;
				WritePropertyValue (PSMuscleDefine.rootProperty [3], q.x, rootToggle [3]);
				WritePropertyValue (PSMuscleDefine.rootProperty [4], q.y, rootToggle [4]);
				WritePropertyValue (PSMuscleDefine.rootProperty [5], q.z, rootToggle [5]);
				WritePropertyValue (PSMuscleDefine.rootProperty [6], q.w, rootToggle [6]);
			}

			EditorGUI.indentLevel--;

			EditorGUILayout.EndToggleGroup ();

		}

		protected void MuscleGroupGUI ()
		{
			EditorGUILayout.BeginHorizontal (EditorStyles.toolbar);
			showMuscleGroupGUI = EditorGUILayout.Foldout (showMuscleGroupGUI, "Muscle Group");

			if (GUILayout.Button ("Reset Sliders", EditorStyles.toolbarButton, GUILayout.Width (100))) {
				for (int k = 0, len = PSMuscleDefine.masterMuscle.Length; k < len; k++) {
					muscleMasterValue [k] = TabSlider (PSMuscleDefine.muscleTypeGroup [k], 0);
				}
				for (int k = 0, len = PSMuscleDefine.fingerMuscle.Length; k < len; k++) {
					muscleFingerValue [k] = TabSlider (PSMuscleDefine.muscleFingerGroup [k], 0);
				}
			}
			//EditorGUILayout.LabelField ("Muscle Group");
			EditorGUILayout.EndHorizontal ();

			if (!showMuscleGroupGUI)
				return;

			EditorGUILayout.BeginHorizontal ();
			for (int i = 0, len = PSMuscleDefine.muscleTabGroup.Length; i < len; i++) {
				if (GUILayout.Toggle (curSelectMasterGroup == i, PSMuscleDefine.muscleTabGroup [i], EditorStyles.toolbarButton)) {
					curSelectMasterGroup = i;
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.HelpBox ("This window affects multiple muscles’ value at the same time. Note however, that the changes made here will affect the ‘Muscle Body Group’ as well. Changes made in Muscle Body Group will not affect the Muscle Group properties.", MessageType.None);

			if (curSelectMasterGroup == 0) {
				for (int k = 0, len = PSMuscleDefine.masterMuscle.Length; k < len; k++) {
					float oldValue = muscleMasterValue [k];
					float value = TabSlider (PSMuscleDefine.muscleTypeGroup [k], oldValue);

					if (oldValue == value)
						continue;

					muscleMasterValue [k] = value;
					int[] array = PSMuscleDefine.masterMuscle [k];
					for (int l = 0, len2 = array.Length; l < len2; l++) {
						int index = array [l];

						if (index != -1) {
							muscleValue [index] = value;
							muscleToggle [index] = true;
							WriteMuscleValue (index, muscleValue [index]);
						}
					}
				}
			} else if (curSelectMasterGroup == 1) {
				for (int k = 0, len = PSMuscleDefine.fingerMuscle.Length; k < len; k++) {
					float oldValue = muscleFingerValue [k];
					float value = TabSlider (PSMuscleDefine.muscleFingerGroup [k], oldValue);
					if (oldValue == value)
						continue;
					
					muscleFingerValue [k] = value;
					int[] array = PSMuscleDefine.fingerMuscle [k];
					for (int l = 0, len2 = array.Length; l < len2; l++) {
						int index = array [l];

						if (index != -1) {
							muscleValue [index] = value;
							muscleToggle [index] = true;
							WriteMuscleValue (index, muscleValue [index]);
						}
					}
				}
			}
		}

		protected void MuscleGUI ()
		{
			EditorGUILayout.BeginHorizontal (EditorStyles.toolbar);
			showMuscleGUI = EditorGUILayout.Foldout (showMuscleGUI, "Muscle Body Group");

			mirror = GUILayout.Toggle (mirror, "Mirror", EditorStyles.toolbarButton, GUILayout.Width (80));

			if (GUILayout.Button ("Add All", EditorStyles.toolbarButton, GUILayout.Width (80))) {
				for (int j = 0; j < muscleValue.Length; j++) {
					muscleToggle [j] = true;
				}
				WriteAllMuscleValue ();
			}

			if (GUILayout.Button ("Remove All", EditorStyles.toolbarButton, GUILayout.Width (80))) {
				for (int j = 0; j < muscleValue.Length; j++) {
					muscleValue [j] = 0f;
					muscleToggle [j] = false;
				}
				WriteAllMuscleValue ();
			}
			EditorGUILayout.EndHorizontal ();

			if (!showMuscleGUI)
				return;

			int groupLen = muscleBodyGroupToggle.Length;
			EditorGUILayout.BeginHorizontal ();
			for (int i = 0; i < groupLen; i++) {
				if (GUILayout.Toggle (curSelectGroup == i, PSMuscleDefine.muscleBodyGroup [i], EditorStyles.toolbarButton)) {
					curSelectGroup = i;
				}
			}

			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.Space ();

			for (int i = 0; i < groupLen; i++) {
				if (curSelectGroup != i)
					continue;
				
				GUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField (PSMuscleDefine.muscleBodyGroup [i], GUILayout.MinWidth (100));

				if (GUILayout.Button ("Add All", EditorStyles.miniButtonLeft, GUILayout.Width (70))) {
					for (int j = 0, len = PSMuscleDefine.muscle [i].Length; j < len; j++) {
						WriteToggleValue (i, j, true);
					}
				}

				if (GUILayout.Button ("Remove All", EditorStyles.miniButtonRight, GUILayout.Width (70))) {
					for (int j = 0, len = PSMuscleDefine.muscle [i].Length; j < len; j++) {
						WriteToggleValue (i, j, false);
					}
				}

				GUILayout.EndHorizontal ();
				EditorGUILayout.Space ();

				int[] array = PSMuscleDefine.muscle [i];
				for (int j = 0, len = array.Length; j < len; j++) {
					int index = array [j];

					GUILayout.BeginHorizontal ();

					bool toggle = EditorGUILayout.ToggleLeft (GUIContent.none, muscleToggle [index], GUILayout.Width (10));

					if (toggle != muscleToggle [index]) {
						WriteToggleValue (i, j, toggle);
					}

					EditorGUI.BeginDisabledGroup (!muscleToggle [index]);

					float value = TabSlider (this.muscleName [index], muscleValue [index]);

					if (muscleValue [index] != value) {
						muscleValue [index] = value;

						if (mirror)
							WriteMirrorMuscleValue (i, j);

						WriteMuscleValue (index, value);
					}

					EditorGUI.EndDisabledGroup ();
					GUILayout.EndHorizontal ();
				}

				EditorGUILayout.Space ();

				if (GUILayout.Button ("Copy values to the mirror side", EditorStyles.miniButton, GUILayout.Width (180))) {
					for (int j = 0, len = PSMuscleDefine.muscle [i].Length; j < len; j++) {
						WriteMirrorMuscleValue (i, j);
					}
				}

				// MCV
				if (GUILayout.Button ("Flip values Vertically", EditorStyles.miniButton, GUILayout.Width (180))) {
					for (int j = 0, len = PSMuscleDefine.muscle [i].Length; j < len; j++) {
						WriteFlipVerticalMuscleValue (i, j);
					}
				}

			}

		}


		/***********************************************
	 *  Write Values
	 ***********************************************/

		public void WriteToggleValue (int i, int j, bool toggle)
		{
			int index = PSMuscleDefine.muscle [i] [j];

			muscleToggle [index] = toggle;
			int i2 = PSMuscleDefine.mirrorMuscle [i];

			if (mirror && i2 != i) {
				int mirrorIndex = PSMuscleDefine.muscle [i2] [j];
				muscleToggle [mirrorIndex] = toggle;
				WriteMuscleValue (mirrorIndex, muscleValue [mirrorIndex]);
			}

			WriteMuscleValue (index, muscleValue [index]);
		}

		public void WriteMirrorMuscleValue (int i, int j)
		{
			int index = PSMuscleDefine.muscle [i] [j];

			int i2 = PSMuscleDefine.mirrorMuscle [i];
			if (i2 == i)
				return;
			
			int mirrorIndex = PSMuscleDefine.muscle [i2] [j];
			muscleValue [mirrorIndex] = muscleValue [index];
			muscleToggle [mirrorIndex] = muscleToggle [index];
			WriteMuscleValue (mirrorIndex, muscleValue [mirrorIndex]);
		}

		//MCV - FlipVertical
		// This should switch the Left & Right sides to invert the pose
		public void WriteFlipVerticalMuscleValue (int i, int j)
		{
			int index = PSMuscleDefine.muscle [i] [j];
			int i2 = PSMuscleDefine.mirrorMuscle [i];
			if (i2 == i)
				return;
			
			int mirrorIndex = PSMuscleDefine.muscle [i2] [j];
			float tempMuscleValue = muscleValue [mirrorIndex];         // Save the current mirrored location value.
			bool tempMuscleToggle = muscleToggle [mirrorIndex];
			muscleValue [mirrorIndex] = muscleValue [index];      
			muscleToggle [mirrorIndex] = muscleToggle [index];
			WriteMuscleValue (mirrorIndex, muscleValue [mirrorIndex]);

			// Copy the temp to the original side and save.
			muscleValue [index] = tempMuscleValue;
			muscleToggle [index] = tempMuscleToggle;
			WriteMuscleValue (index, muscleValue [index]);
		}

		public void WritePropertyValue (string propertyName, float value, bool enable)
		{
			if (clip == null)
				return;
			
			EditorCurveBinding binding = EditorCurveBinding.FloatCurve ("", typeof(Animator), propertyName);
			Undo.RecordObject (clip, clip.name);

			if (enable)
				this.SetEditorCurve (binding, value);
			else
				AnimationUtility.SetEditorCurve (clip, binding, null);

			resample = true;
		}

		public void WriteMuscleValue (int index, float value)
		{
			if (curveBindings == null) {
				OnTargetChange ();
			}

			if (clip == null || curveBindings == null) {
				PSLogger.Error ("Clip or curveBindings is null");
				return;
			}

			if (index >= curveBindings.Length) {
				PSLogger.Error ("Array index is out of range");
				return;
			}

			Undo.RecordObject (clip, clip.name);

			if (muscleToggle [index]) {
				this.SetEditorCurve (curveBindings [index], value);
			} else {
				AnimationUtility.SetEditorCurve (clip, curveBindings [index], null);
			}

			resample = true;
		}

		public void WriteAllMuscleValue ()
		{
			if (curveBindings == null) {
				OnTargetChange ();
			}

			if (clip == null || curveBindings == null) {
				PSLogger.Error ("Clip or curveBindings is null");
				return;
			}

			if (this.propertyMuscleName.Length > curveBindings.Length) {
				PSLogger.Error ("CurveBindings length and propertyMuscleName length are not equal");
				return;
			}

			Undo.RecordObject (clip, clip.name);

			for (int i = 0, len = this.propertyMuscleName.Length; i < len; i++) {
				if (muscleToggle [i]) {
					this.SetEditorCurve (curveBindings [i], muscleValue [i]);
				} else {
					AnimationUtility.SetEditorCurve (clip, curveBindings [i], null);
				}
			}

			resample = true;
		}

		private void SetEditorCurve (EditorCurveBinding binding, float value)
		{
			AnimationCurve curve = AnimationUtility.GetEditorCurve (clip, binding);

			if (curve == null)
				curve = new AnimationCurve ();

			bool found = false;
			Keyframe[] keys = curve.keys;
			for (int i = 0; i < keys.Length; i++) {
				if (keys [i].time == time) {
					keys [i].value = value;
					found = true;
					break;
				}
			}

			if (found)
				curve.keys = keys;
			else
				curve.AddKey (new Keyframe (time, value));

			AnimationUtility.SetEditorCurve (clip, binding, curve);
		}

		public void ResampleAnimation ()
		{
			if (!resample)
				return;

			resample = false;
			if (AnimationMode.InAnimationMode ()) {
				AnimationMode.BeginSampling ();
				AnimationMode.SampleAnimationClip (target, clip, time);
				AnimationMode.EndSampling ();
			}

			UnityEditorInternal.InternalEditorUtility.RepaintAllViews ();
		}

		private static float TabSlider (string label, float val)
		{
			if (label == null)
				val = EditorGUILayout.Slider (GUIContent.none, val, -1f, 1f);
			else
				val = EditorGUILayout.Slider (label, val, -1f, 1f);
			return val;
		}
	}
}
