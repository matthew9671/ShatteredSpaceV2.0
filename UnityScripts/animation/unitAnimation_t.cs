using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unitAnimation_t : MonoBehaviour 
{
	public static unitAnimation_t pool;
	public GameObject turret1;
	public GameObject goal;
	public GameObject blasterAddOn;
	public GameObject antibody;
	public GameObject explosion1;

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
		Action<GameObject> play = delegate(GameObject obj) {
			StartCoroutine (move(obj, toPosition, frames));
		};
		Action stop = delegate {
			StopCoroutine("move");
		};
		// In the case of the movement the delay is equal to duration
		// For testing we make delay somewhat bigger
		return new animation_t("Unit moves to " + toPosition.ToString(), play, stop, frames, frames);
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

	public animation_t get_hit_animation(damage_t dmg)
	{
		int frames = 10;
		Action<GameObject> play = delegate(GameObject obj) {
			StartCoroutine (get_hit(obj, frames));
		};
		Action stop = delegate {
			StopCoroutine("get_hit");
		};
		// In the case of the movement the delay is equal to duration
		// For testing we make delay somewhat bigger
		return new animation_t("Unit gets hit", play, stop, frames, frames);
	}

	public IEnumerator get_hit(GameObject obj, int frames)
	{
		// We might have to change this
		// Since there can be multiple renderer in children
		// Who knows?
		Renderer rend = obj.GetComponentInChildren<Renderer>();
		Color originalColor = rend.material.color;
		Color hitColor = Color.red;
		rend.material.color = hitColor;
		for (int i = 1; i < frames + 1; i++)
		{
			if (obj == null) break;
			rend.material.color = Color.Lerp(hitColor, originalColor, (float)i / frames);
			yield return new WaitForFixedUpdate();
		}
	}

	public animation_t get_exit_animation()
	{
		int frames = 20;
		Action<GameObject> play = delegate(GameObject obj) {
			StartCoroutine (exit(obj, frames));
		};
		Action stop = delegate {
			StopCoroutine("exit");
		};
		// In the case of the movement the delay is equal to duration
		// For testing we make delay somewhat bigger
		return new animation_t("Unit exits", play, stop, frames, frames);
	}

	public IEnumerator exit(GameObject obj, int frames)
	{
		Vector3 start = obj.transform.localScale;
		Vector3 end = Vector3.zero;
		for (int i = 1; i < frames + 1; i++)
		{
			if (obj == null) break;
			obj.transform.localScale = Vector3.Lerp(start, end, (float)i / frames);
			yield return new WaitForFixedUpdate();
		}
	}

	public animation_t get_destroyed_animation()
	{
		int duration = 20;
		GameObject explosion;
		Action<GameObject> play = delegate(GameObject obj) {
			obj.SetActive(false);
		};
		Action stop = delegate {
		};
		// In the case of the movement the delay is equal to duration
		// For testing we make delay somewhat bigger
		return new prefabAnimation_t(explosion1, "Unit is destroyed", play, stop, 0, duration);
	}
}

public class prefabAnimation_t : animation_t
{
	GameObject prefab;
	GameObject instantiated;

	public prefabAnimation_t(GameObject prefab, string name, 
		Action<GameObject> play, Action stop, int delay, int duration):base(name, play, stop, delay, duration)
	{
		this.prefab = prefab;
	}

	public override void play_animation (GameObject obj)
	{
		instantiated = GameObject.Instantiate(prefab, obj.transform.position, Quaternion.identity);
		base.play_animation(obj);
	}

	public override void stop_animation ()
	{
		GameObject.Destroy(instantiated);
	}
}