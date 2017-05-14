using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testBtn : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void on_click ()
	{
		gameManager_t.GM.test ();
	}

	public void on_click2 ()
	{
		gameManager_t.GM.test_weapon ();
	}

	public void on_click3 ()
	{
		gameManager_t.GM.print_stack ();
	}

	public void test_delegate()
	{
		gameManager_t.GM.test_delegate ();
	}
}
