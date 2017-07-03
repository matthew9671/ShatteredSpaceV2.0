using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animController_t : MonoBehaviour {

	// This is the list of animation "triggers"
	public List<animation_t> animations = new List<animation_t>();
	public int objectId;
	public bool isPlaying = true;
	public int index = 0;
	// For debugging
	public int animCount;

	public void reset()
	// Reset is called by init_planning
	{
		index = 0;
		foreach(animation_t anim in animations)
		{
			anim.destroy();
		}
		animations.Clear();
	}

	// Affected by timescale
	void FixedUpdate () 
	{
		animCount = animations.Count;
		// Play the animation(s) 
		while (isPlaying && animations.Count > index) 
		{
			animation_t animation = animations[index];
			if (animation.delay == 0)
			{
				index++;
			}
			if (!animation.isPlaying)
			{
				// Every animation should only get played once
				animation.play_animation(this.gameObject);
			}
			if (animation.delay > 0)
			{
				animation.delay -= 1;
				break;
			}
		}
		// Check if all animations finished playing
		bool all_finished = true;
		// Update the duration counter on each playing animation
		foreach (animation_t animation in animations)
		{
			if (animation.duration > 0) all_finished = false;
			if (animation.isPlaying)
			{
				if (animation.duration == 0)
				{
					// Hopefully all the animations only gets stopped once
					animation.stop_animation();
				}
				else
				{
					animation.duration -= 1;
				}
			}
		}
		if (animations.Count <= index && all_finished)
		{
			stop();
			gameManager_t.GM.animation_finished();
		}
	}

	public void copy_from(animController_t other)
	{
		animations = new List<animation_t>();
		objectId = other.objectId;
		isPlaying = other.isPlaying;
		index = other.index;
		foreach(animation_t anim in other.animations)
		{
			animations.Add(anim.copy());
		}
	}

	public void play()
	{
		//Debug.Log("Animation started!");
		isPlaying = true;
//		foreach (animation_t animation in animations)
//		{
//			if (animation.isActive) animation.play_animation(this.gameObject);
//		}
	}
		
	public void stop()
	// Should only be called by HALT
	{
		//Debug.Log("Animation stopped!");
		isPlaying = false;
	}

	public void pop_animation()
	{
		if ( animations.Count > 0)
			 animations.RemoveAt( animations.Count - 1);
		//Debug.Log ("Popping animation! " +  animations.Count.ToString() + "animations remaining.");
	}

	public void destroy()
	{
		foreach(animation_t anim in animations)
		{
			anim.destroy();
		}
		Destroy(this.gameObject);
		Destroy(this);
	}

	public void set_active(bool b)
	{
		foreach(animation_t anim in animations)
		{
			anim.set_active(b);
		}
		if (b)
		{
			this.gameObject.SetActive(true);
		}
		else
		{
			this.gameObject.SetActive(false);
		}
	}

	public void print_animations()
	{
		foreach (animation_t anim in animations)
		{
			Debug.Log(anim.name);
		}
	}
}
