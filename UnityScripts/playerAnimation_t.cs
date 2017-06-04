using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerAnimation_t : MonoBehaviour 
{
	public static playerAnimation_t pool;

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

	public animation_t move(Vector3 v)
	{
		return delegate(GameObject obj) {
			obj.transform.position += v;
		};
	}
}
