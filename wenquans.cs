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
        test_force_field();
    }

    public static void test_force_field()
    {
        return;
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
        action.wpnId = 1;
        action.attack = new attack_t(new Vector2(-2, 0));
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
        // #############################
        input.Add(actions);
        Console.WriteLine("Testing grenade...");
        game_t.execute_turn(input);
        Console.WriteLine("...Passed!");
    }
}

public class doubleAttack_t : weapon_t
{
	public doubleAttack_t()
    {}
}

public class forceField_t : weapon_t
{
    static int[] modules = {6, 0, 0, 0};
    public forceField_t():base(range:5, damage:5, delay:-1, modules:modules)
    {}
}