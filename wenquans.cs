#define DEBUG
using System;
//using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

public class wenquanTest
{
    public static void test_weapons()
    {
        test_force_barrier();
    }

    public static void test_force_barrier()
    {
        // This is a sample test
        // Write your own test with this as a template
        List<Stack<action_t>> input = new List<Stack<action_t>>();
        // Generate the action stack for player1
        Stack<action_t> actions = new Stack<action_t>();
        // Generate one action
        // #############################
        action_t action = new action_t();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = 5;
        action.attack = new doubleAttack_t(new Vector2(0, 0),
        								   new Vector2(0, 1));
        actions.Push(action);
        // #############################
        input.Add(actions);
        // Generate the action stack for player2
        actions = new Stack<action_t>();
        // #############################
        action = new action_t();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = -1;
        actions.Push(action);
        action = new action_t();
        action.movement = Vector2
        // #############################
        input.Add(actions);
        Console.WriteLine("Testing force barrier...");
        game_t.execute_turn(input);
        Console.WriteLine("...Passed!");
    }
}

public class doubleAttack_t : attack_t
{
	public Vector2 target1;
	public Vector2 target2;
	public doubleAttack_t(Vector2 tgt1, Vector2 tgt2):base(null)
    {
    	this.target1 = tgt1;
    	this.target2 = tgt2;
    }

}

public class barrier_t : unit_t
{
	// Barriers are destoryed upon receiving any damage
	public static int hp = 1;

	public barrier_t():base("barrier", hp)
	{
		this.exists = false;
	}

	public override void end_turn(board_t board)
	{
		base.end_turn(board);
		this.exists = true;
		if !board.is_free(pos) board.remove_later(this);
	}
}

public class forceField_t : weapon_t
{
    static int[] modules = {0, 0, 0, 1};
    public forceField_t():base(range:5, damage:5, delay:-1, modules:modules)
    {}
    
}

public class forceBarrier_t : weapon_t
{
    static int[] modules = {0, 0, 0, 1};
    public forceBarrier_t():base(range:5, damage:0, delay:-1, modules:modules)
    {
    }
  
  	public override bool fire(attack_t attack, board_t board, unit_t master)
    // Create pending damage on the board, 
    // return false if this is an invalid attack. 
    // May have some special effects on the master.
    {
        Debug.Assert(attack != null);
        doubleAttack_t dbAttack = attack as doubleAttack_t;
        Vector2 pos1 = dbAttack.target1;
        Vector2 pos2 = dbAttack.target2;
        Debug.Assert(pos1 != null);
        Debug.Assert(pos2 != null);
        int delay = get_delay(master);
        barrier_t barrier = new barrier_t();
        barrier.stepLife = -1;
        barrier.turnLife = 3;
        board.put_object(pos1, barrier);
        barrier = new barrier_t();
        barrier.stepLife = -1;
        barrier.turnLife = 3;
        board.put_object(pos2, barrier);
        return true;
    }
}