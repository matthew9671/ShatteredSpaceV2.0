using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(animController_t))]
public class animationEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		animController_t myScript = (animController_t)target;
		if(GUILayout.Button("Print animations"))
		{
			myScript.print_animations();
		}
	}
}
