using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animTest : MonoBehaviour {
	
	void Start () {
//		animController_t aCtrl = this.gameObject.GetComponent<animController_t> ();
//		List<Action> movements = new List<Action> ();
//		Vector3[] movelist = {Vector3.left * 10, Vector3.up * 10, 
//			Vector3.right * 10, Vector3.down * 10};
//		int frames = 30;
//		foreach (Vector3 movement in movelist) 
//		{
//			movements.Add(aCtrl.get_trigger(()=>move (movement/frames), frames));
//		}
//		movements.Add (wait);
//		aCtrl.set_anim_sequence (movements);
	}

	public void wait()
	{
	}

	public void move (Vector3 amount) 
	{
		this.gameObject.transform.position += amount;	
	}
}
