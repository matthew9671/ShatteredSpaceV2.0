using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerAnimation_t : MonoBehaviour 
{
	public static playerAnimation_t pool;
	public GameObject playerModel;

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

	public animation_t get_move_animation(Vector2 toPosition, int frames, bool spFlag)
	{
		Action<GameObject> play;
		Action stop;
		if (!spFlag)
		{
			play = delegate(GameObject obj) {
				StartCoroutine (move(obj, toPosition, frames));
			};
			stop = delegate {
				StopCoroutine("move");
			};
		}
		else
		{
			play = delegate(GameObject obj) {
//				gameManager_t.GM.warp_time(SS.spMoveFactor);
				StartCoroutine (move(obj, toPosition, frames));
			};
			stop = delegate {
//				gameManager_t.GM.warp_time(0f);
				StopCoroutine("move");
			};
		}
		// In the case of the movement the delay is equal to duration
		// For testing we make delay somewhat bigger
		return new animation_t("Player moves to " + toPosition.ToString(), play, stop, frames, frames);
	}

	public IEnumerator move(GameObject obj, Vector2 toPosition, int frames)
	{
		Vector3 start = obj.transform.position;
		Vector3 end = SS.board_to_world(toPosition);
		yield return new WaitForSeconds(0.05f);
		for (int i = 1; i < frames + 1; i++)
		{
			if (obj == null) break;
			obj.transform.position = Vector3.Lerp(start, end, (float)i / frames);
			yield return new WaitForFixedUpdate();
		}
	}
}
