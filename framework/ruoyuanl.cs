#define DEBUG
using System;
//using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

public class ruoyuanTest
{
    public static void test_weapons()
    {
        test_plasma_cutter();
    }

    public static void test_plasma_cutter()
    {
        return;
        // This is a sample test
        // Write your own test with this as a template
        List<List<action_t>> input = new List<List<action_t>>();
        // Generate the action List for player1
        List<action_t> actions = new List<action_t>();
        // Generate one action
        // #############################
        action_t action = new action_t();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = 1;
        action.attack = new attack_t(new Vector2(-2, 0));
        actions.Add(action);
        // #############################
        input.Add(actions);
        // Generate the action List for player2
        actions = new List<action_t>();
        // #############################
        action = new action_t();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = -1;
        actions.Add(action);
        // #############################
        input.Add(actions);
        Console.WriteLine("Testing grenade...");
        game_t.execute_turn(null, input);
        Console.WriteLine("...Passed!");
    }
}

public class plasmaCutter_t : weapon_t
{
    static int[] modules = {2, 0, 0, 0};
    public plasmaCutter_t():base(range:5, damage:5, delay:1, modules:modules)
    {}
}