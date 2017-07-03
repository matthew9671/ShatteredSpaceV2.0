using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class sceneExit_t : object_t
{
	public sceneExit_t():base("Scene exit", false)
	{}

	public override void on_collision (board_t board, object_t other)
	{
		if (other is player_t)
		{
			gameManager_t.GM.endOfTurnActions += gameManager_t.GM.next_scene;
			(other as player_t).add_exit_animation();
		}
	}

	public override GameObject get_model()
	{
		return unitAnimation_t.pool.goal;
	}
}

[Serializable]
public class blasterAddOn_t : object_t
{
	public blasterAddOn_t():base("Blaster add-on", false)
	{}

	public override void on_collision (board_t board, object_t other)
	{
		if (other is player_t)
		{
			// TODO:Need to sort out problem with modules later
			(other as player_t).build_weapon(new blaster_t());
			Debug.Log("Blaster installed!");
			this.exists = false;
			// Send the exit animation
			List<animation_t> animList = new List<animation_t>();
			animList.Add(unitAnimation_t.pool.get_exit_animation());
			gameManager_t.GM.send_animation(animList, this.objectId);
		}
	}

	public override GameObject get_model()
	{
		return unitAnimation_t.pool.blasterAddOn;
	}
}

[Serializable]
public class eventTrigger_t : object_t
{
	public eventTrigger_t():base("Event trigger", false)
	{}

	public override void on_collision (board_t board, object_t other)
	{
		if (other is player_t)
		{
			gameManager_t.GM.endOfTurnActions += delegate{gameManager_t.GM.trigger_event(board, this.get_pos());};
			this.exists = false;
		}
	}
}



[Serializable]
public class antibody_t : unit_t
{
	// private board_t board;
	private int turn = 0;
	private object_t target;
	private int centerDmg = 4;
	private int splashDmg = 2;
	bool exploded = false;

	public antibody_t (object_t target):base("Floating mine",1)
	{
		this.target = target;
		stepLife = -1;
		turnLife = -1;
		solid = true;
		exists = true;
	}

	public override action_t peek_action (board_t board)
	{
		return pop_action (board);
	}

	public override action_t pop_action (board_t board)
	{
		Vector2 targetPos = target.get_pos();
		action_t action = new action_t();
		int bestDistance = -1;
		Vector2 bestDir = Vector2.zero;
		foreach (Vector2 dir in SS.DIRECTIONS)
		{
			Vector2 newPos = this.get_pos() + dir;
			if (board.is_free(newPos))
			{
				if (bestDistance == -1 || bestDistance > SS.distance(newPos, targetPos))
				{
					bestDistance = SS.distance(newPos, targetPos);
					bestDir = dir;
				}
			}
		}
		action.movement = bestDir;
		return action;
	}

	public override void on_destroyed (board_t board)
	{
		// Send the explosion animation
		List<animation_t> animList = new List<animation_t>();
		animList.Add(unitAnimation_t.pool.get_destroyed_animation());
		gameManager_t.GM.send_animation(animList, this.objectId);
	}

	public override GameObject get_model ()
	{
		return unitAnimation_t.pool.antibody;
	}
}