#define DEBUG
using System;
//using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

public class ryanTest
{
    public static void test_weapons()
    {
        test_tracking_mine();
    }

    public static void test_tracking_mine()
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

// ###################################################################
// Write your weapon class here
// ###################################################################

// public class trackingMine_t : weapon_t
// {

// }

// ###################################################################
// To build a class in C# you need to have a constructor
// Here is an example of the simplest weapon class to get you started
// ###################################################################

// public class blaster_t : weapon_t
// {
//     // From left to right, the entries represent 
//     // number of momentum, explosive, particle and field modules.
//     static int[] modules = {2, 0, 0, 0};
//     public blaster_t():base(range:5, damage:5, delay:1, modules:modules)
//     {}
// }