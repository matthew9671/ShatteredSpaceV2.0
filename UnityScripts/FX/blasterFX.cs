using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blasterFX : MonoBehaviour {

	Vector2 toPosition;
	int frames;

	public IEnumerator move () {
		Vector3 start = this.transform.position;
		Vector3 end = SS.board_to_world(toPosition);
		for (int i = 1; i < frames + 1; i++)
		{
//			yield return new WaitUntil(() => this.gameObject.activeSelf);
			this.transform.position = Vector3.Lerp(start, end, (float)i / frames);
			yield return new WaitForFixedUpdate();
//			yield return new WaitUntil(() => this.gameObject.activeSelf);
		}
		this.gameObject.SetActive(false);
	}

	public void play(Vector2 target, int f)
	{
		toPosition = target;
		frames = f;
		StartCoroutine("move");
	}

	public void stop()
	{
		StopCoroutine("move");
	}
		
}

public class blasterAnimation_t : animation_t
{
	public GameObject projectile;
	Vector2 target;

	public blasterAnimation_t(Vector2 target, int frames):base("blaster shot", null, null, 0, frames)
	{
		this.target = target;
	}

	public override void play_animation(GameObject obj)
	{
		Debug.Log("Playing blaster shot animation");
		if (projectile == null)
		{
			projectile = GameObject.Instantiate(weaponAnimation_t.pool.blasterProjectile, 
				obj.transform.position, Quaternion.identity) as GameObject;
		}
		projectile.GetComponent<blasterFX>().play(target, this.duration);
	}

	public override void stop_animation()
	{
		//UnityEngine.Debug.Log("Blaster animation stopped!");
		if (projectile != null) projectile.GetComponent<blasterFX>().stop();
		GameObject.Destroy (projectile);
	}

	public override animation_t copy()
	{
		blasterAnimation_t result = new blasterAnimation_t(target, this.duration);
		if (projectile == null)
		{
			result.projectile = null;
		}
		else
		{
			result.projectile = GameObject.Instantiate(projectile) as GameObject;
		}
		result.isPlaying = this.isPlaying;
		result.projectile.GetComponent<blasterFX>().stop();
		result.projectile.GetComponent<blasterFX>().play(target, this.duration);
		return result as animation_t;
	}

	public override void set_active(bool b)
	{
		if (projectile != null) projectile.SetActive(b);
	}
}