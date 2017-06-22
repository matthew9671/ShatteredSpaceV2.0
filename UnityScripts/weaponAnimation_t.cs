using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponAnimation_t : MonoBehaviour {

	public static weaponAnimation_t pool;
	public GameObject blasterProjectile;

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
