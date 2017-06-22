using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unitAnimation_t : MonoBehaviour 
{
	public static unitAnimation_t pool;
	public GameObject turret1;

	// Black magic
	void Awake()
	{
		if (pool != null) 
		{
			GameObject.Destroy (pool);
		} 
		else 
		{
			pool = this;
		}
		DontDestroyOnLoad(this);
	}

	public animation_t get_move_animation(Vector2 toPosition, int frames)
	{
		Action<GameObject> play = delegate(GameObject obj) {
			StartCoroutine (move(obj, toPosition, frames));
		};
		Action stop = delegate {
			StopCoroutine("move");
		};
		// In the case of the movement the delay is equal to duration
		// For testing we make delay somewhat bigger
		return new animation_t(play, stop, frames, frames);
	}

	public IEnumerator move(GameObject obj, Vector2 toPosition, int frames)
	{
		Vector3 start = obj.transform.position;
		Vector3 end = SS.board_to_world(toPosition);
		for (int i = 1; i < frames + 1; i++)
		{
			if (obj == null) break;
			obj.transform.position = Vector3.Lerp(start, end, (float)i / frames);
			yield return new WaitForFixedUpdate();
		}
	}
}
