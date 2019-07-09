using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace PavoStudio.MAE
{
	class PSReflection : PSBaseReflection
	{
		private object animationWindowState;
		private PropertyInfo timeProperty;
		private PropertyInfo clipProperty;
		private PropertyInfo frameProperty;
		private PropertyInfo rootObjectProperty;

		private int retryCount = 0;

		public override void Init ()
		{
			if (UNITYEDITOR_DLL == null)
				return;

			if (retryCount > 10) {
				Debug.LogError ("Failed to get some internal properties, please report it to developer.");
				return;
			}

			retryCount++;

			System.Type animationWindowType = Assembly.LoadFile (UNITYEDITOR_DLL).GetType ("UnityEditor.AnimationWindow");
			object animationWindow = EditorWindow.GetWindow (animationWindowType);
			if (animationWindow == null)
				return;
			//animationWindowState = animationWindow.GetType ().GetProperty ("state").GetValue (animationWindow, null);
			object animEditor = animationWindow.GetType ().GetField ("m_AnimEditor", getBindingFlags ()).GetValue (animationWindow);
			if (animEditor == null)
				return;

			animationWindowState = animEditor.GetType ().GetField ("m_State", getBindingFlags ()).GetValue (animEditor);
			if (animationWindowState == null)
				return;

			timeProperty = animationWindowState.GetType ().GetProperty ("currentTime");
			clipProperty = animationWindowState.GetType ().GetProperty ("activeAnimationClip");
			frameProperty = animationWindowState.GetType ().GetProperty ("currentFrame");
			rootObjectProperty = animationWindowState.GetType ().GetProperty ("activeRootGameObject");
		}

		public override float GetTime ()
		{
			if (timeProperty == null)
				Init ();
			return timeProperty == null ? 0.0f : (float)timeProperty.GetValue (animationWindowState, null);
		}

		public override int GetFrame ()
		{
			if (frameProperty == null)
				Init ();
			return frameProperty == null ? 0 : (int)frameProperty.GetValue (animationWindowState, null);
		}

		public override GameObject GetRootObject ()
		{
			if (rootObjectProperty == null)
				Init ();
			return rootObjectProperty == null ? null : (GameObject)rootObjectProperty.GetValue (animationWindowState, null);
		}

		public override AnimationClip GetClip ()
		{
			if (clipProperty == null)
				Init ();
			return clipProperty == null ? null : (AnimationClip)clipProperty.GetValue (animationWindowState, null);
		}

		private BindingFlags getBindingFlags ()
		{
			return BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
		}
	}
}

