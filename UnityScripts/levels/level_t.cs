using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class level_t
{
	int sceneId;
	List<scene_t> scenes = new List<scene_t>();

	public level_t(string levelInfo)
	{
		init_level(levelInfo);
	}

	void init_level(string levelInfo)
	{
		string[] temp = levelInfo.Split(new string[] {"<scene>"}, StringSplitOptions.RemoveEmptyEntries);
		for (int i=0; i < temp.Length; i++)
		{
			scenes.Add(new scene_t(temp[i]));
		}
		sceneId = 0;
		scenes[0].load_scene();
	}

	public bool load_next_scene()
	{
		sceneId ++;
		if (sceneId >= scenes.Count)
		{
			return false;
		}
		scenes[sceneId].load_scene();
		return true;
	} 
}

public class scene_t
{
	int mapH;
	int mapW;
	bool[,] tileLayout;
	List<Type> objTypes;
	List<object_t> objs;

	public scene_t(string mapInfo)
	{
		objTypes = new List<Type>();
		objTypes.Add(typeof(player_t));
		objTypes.Add(typeof(turret_t));
		objTypes.Add(typeof(sceneExit_t));
		init_scene(mapInfo);
	}

	public void init_scene(string mapInfo)
	// Read the layout of the map by parsing mapinfo
	{
		mapInfo = Regex.Replace(mapInfo, @"\s+", "");
		string[] temp = mapInfo.Split(new string[] {"<size>", "<map>", "<object>"}, StringSplitOptions.None);
		mapH = int.Parse(temp[0].Split(',')[0]);
		mapW = int.Parse(temp[0].Split(',')[1]);
		string map = temp[1];
		string[] objects = temp[2].Split(new char[]{'|'}, StringSplitOptions.RemoveEmptyEntries);
		tileLayout = new bool[mapH, mapW];
		using(StringReader str = new StringReader(map))
		{
			for (int row = 0; row < mapH; row++)
			{
				for (int col = 0; col < mapW; col++)
				{
					char[] buf = new char[1];
					str.Read(buf, 0, 1);
					tileLayout[row, col] = (buf[0] == '1');
				}
			}
		}
		objs = new List<object_t>();
		foreach (string objStr in objects)
		{
			string[] objInfo = objStr.Split(',');
			int typeId = int.Parse(objInfo[0]);
			int row = int.Parse(objInfo[1]);
			int col = int.Parse(objInfo[2]);
			object_t obj = Activator.CreateInstance(objTypes[typeId]) as object_t;
			if (obj is player_t)
			{
				// We are assuming a 1 player level
				(obj as player_t).playerId = 0;
			}
			obj.set_pos(board_t.array_to_pos(row, col, mapH, mapW));
			objs.Add(obj);
		}
	}

	public void load_scene()
	{
		if (GameObject.Find("Board") != null)
		// If this is not the first scene, move everything back
		{
			Transform boardHolder = GameObject.Find("Board").transform;
			Transform camera = GameObject.Find("Main Camera").transform;
			Vector3 offset = new Vector3(10f, 10f, 0f);
			boardHolder.position -= offset;
			Vector3 target = camera.position;
			camera.position -= offset;
			gameManager_t.GM.StartCoroutine(gameManager_t.GM.move_camera(target));
		}
		gameManager_t.GM.init_board(mapW:mapW, mapH:mapH, tileLayout:tileLayout, objs:objs);
		// Test only
		gameManager_t.GM.set_mode(inputMode_t.FREE);
	}
}