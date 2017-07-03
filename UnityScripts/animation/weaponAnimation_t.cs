using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponAnimation_t : MonoBehaviour {

	public static weaponAnimation_t pool;
	public GameObject blasterProjectile;
	public Transform laserBeam;

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
		
}

public class laserAnimation_t : animation_t
{
	public Transform beam;
	Vector2 target;

	public laserAnimation_t(Vector2 target, int frames):base("Laser shot", null, null, 0, frames)
	{
		this.target = target;
	}

	public override void play_animation(GameObject obj)
	{
		Vector3 targetDir = SS.board_to_world(target) - obj.transform.position - Vector3.back;
		Transform laserBeam = weaponAnimation_t.pool.laserBeam;
		beam = GameObject.Instantiate(laserBeam, obj.transform.position + Vector3.back, Quaternion.LookRotation(targetDir)); 
		//Forge3D.F3DAudioController.instance.PlasmaBeamLoop(transform.position, transform.parent);
		beam.gameObject.GetComponent<Forge3D.SSBeam>().StartCoroutine("despawn_after_frames", duration);
	}

	public override void stop_animation()
	{
		//UnityEngine.Debug.Log("Blaster animation stopped!");
	}

}