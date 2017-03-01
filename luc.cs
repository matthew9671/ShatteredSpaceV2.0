#define DEBUG
using System;
//using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

public class luTest
{
    public static void test_cluster_bomb()
    // Make the two players shoot each other with grenade launchers
    {
        return;
        List<Stack<action_t>> input = new List<Stack<action_t>>();
        // Generate the action stack for player1
        action_t action = new action_t();
        Stack<action_t> actions = new Stack<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = 1;
        action.target = new Vector2(-2, 0);
        actions.Push(action);
        input.Add(actions);
        // Generate the action stack for player2
        action = new action_t();
        actions = new Stack<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = 1;
        action.target = new Vector2(2, 0);
        actions.Push(action);
        input.Add(actions);
        Console.WriteLine("Testing grenade...");
        game_t.execute_turn(input);
        Console.WriteLine("...Passed!");
    }
}

public class clusterBomb_t : weapon_t
{
    static int[] modules = {0, 3, 0, 0};
    const int SPLASH_DAMAGE = 2;
    const int SUB_DAMAGE = 3;
    const int SUB_SPLASH = 2;
    public clusterBomb_t():base(range:5, damage:5, delay:-1, modules:modules)
    {}
}