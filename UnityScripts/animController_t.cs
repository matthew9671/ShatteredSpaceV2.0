using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animController_t : MonoBehaviour {

	// This is the list of animation "triggers"
	public List<animation_t> animTriggers = new List<animation_t>();
	public int frames = -1;
	public int objectId;

	void Start ()
	{
		gameManager_t.GM.stepAnimation += pop_animation;
	}

	// Update is called once per frame
	void Update () 
	{
		if (animTriggers.Count != 0) 
		{
			if (frames == 0)
			{
				// We finished one animation
				frames = -1;
				// Get ready to execute the next animation
				pop_animation();
			}
			else if (frames > 0)
			{
				// We are still in the middle of 
				frames -= 1;
			}
			// Call the topmost animation trigger
			animTriggers[animTriggers.Count - 1](this.gameObject);
		}
	}

	public static animation_t get_trigger(animation_t a, int f)
	{
		// This function replaces itself on the stack with action a and sets frames to f.
		return delegate (GameObject obj)
		{
			animController_t aCtrl = obj.GetComponent<animController_t>();
			//Debug.Log("Triggered!");
			aCtrl.pop_animation();
			aCtrl.frames = f;
			aCtrl.animTriggers.Add (a);
		};
	}

	public void pop_animation()
	{
		if (animTriggers.Count > 0)
			animTriggers.RemoveAt(animTriggers.Count - 1);
		//Debug.Log ("Popping animation! " + animTriggers.Count.ToString() + "animations remaining.");
	}
}
