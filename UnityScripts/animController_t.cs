using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animController_t : MonoBehaviour {

	// This is the list of animation "triggers"
	public List<Action> animTriggers = new List<Action>();
	public int frames = -1;

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
			animTriggers[animTriggers.Count - 1]();
		}
	}

	public Action get_trigger(Action a, int f)
	{
		// This function replaces itself on the stack with action a and sets frames to f.
		return delegate 
		{
			pop_animation();
			frames = f;
			animTriggers.Add (a);
		};
	}

	void pop_animation()
	{
		if (animTriggers.Count > 0)
			animTriggers.RemoveAt(animTriggers.Count - 1);
	}

	public void set_anim_sequence(List<Action> s)
	{
		animTriggers = s;
	}
}
