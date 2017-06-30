using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class sceneExit_t : object_t
{
	public sceneExit_t():base("Scene exit", false)
	{}

	public override void on_collision (object_t other)
	{
		if (other is player_t)
		{
			gameManager_t.GM.next_scene();
		}
	}

	public override GameObject get_model()
	{
		return unitAnimation_t.pool.goal;
	}
}
