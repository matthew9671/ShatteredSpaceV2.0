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
	{
		index = 0;
//		foreach(animation_t anim in animations)
//		{
//			anim.destroy();
//		}
		animations.Clear();
	}

	// Affected by timescale
	void FixedUpdate () 
	{
		animCount = animations.Count;
		while (isPlaying && animations.Count > index) 
		{
			animation_t animation = animations[index];
			if (animation.delay == 0)
			{
				index++;
			}
			if (!animation.isPlaying)
			{
				animation.play_animation(this.gameObject);
				animation.isActive = true;
			}
			if (animation.delay > 0)
			{
				animation.delay -= 1;
				break;
			}
		}
		if (animations.Count <= index) gameManager_t.GM.animation_finished();
		// Update the duration counter on each playing animation
		foreach (animation_t animation in animations)
		{
			if (animation != null && animation.isPlaying)
			{
				if (animation.duration == 0)
				{
					animation.stop_animation();
					animation.isActive = false;
				}
				else
				{
					animation.duration -= 1;
				}
			}
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
			if (anim != null) animations.Add(anim.copy());
		}
	}

	public void play()
	{
		//Debug.Log("Animation started!");
		isPlaying = true;
		foreach (animation_t animation in animations)
		{
			if (animation.isActive) animation.play_animation(this.gameObject);
		}
	}

	// TODO: It should be play/pause/stop instead of play/stop
	public void stop()
	{
		//Debug.Log("Animation stopped!");
		isPlaying = false;
		foreach (animation_t animation in animations)
		{
			if (animation.isActive) animation.stop_animation();
		}
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
