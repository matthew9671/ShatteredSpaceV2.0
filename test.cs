#define DEBUG
using System;
//using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

public class SSTest
{
    public static void Main(string[] args)
    {
        game_t.init_game();
        // Do all the testing
        test_vector2();
        test_board();
        test_build_weapons();
        test_weapons();
        test_movement();
    }

    static void test_weapons()
    // Test shooting at each other
    // Only works for two players
    {
        Console.WriteLine("Testing weapons...");
        test_build_weapons();
        // test_blaster();
        // test_grenade();
        yueTest.test_weapons();
        ruoyuanTest.test_weapons();
        wenzeTest.test_weapons();
        luTest.test_weapons();
        Console.WriteLine("...Passed!");
    }

    static void test_build_weapons()
    // Build all the weapons for testing
    {
        List<player_t> players = game_t.board.get_players(game_t.PLAYER_CNT);
        foreach(player_t player in players)
        {
            Debug.Assert(player.build_weapon(new grenadeLauncher_t())); // 1
            Debug.Assert(player.build_weapon(new plasmaCutter_t()));    // 2
            Debug.Assert(player.build_weapon(new shockCannon_t()));     // 3
            Debug.Assert(player.build_weapon(new clusterBomb_t()));     // 4
            //Debug.Assert(player.build_weapon(new oribitMissile()));    // 5
        }
    }

    static void test_blaster()
    // Make the two players shoot each other with blasters
    {
        List<Stack<action_t>> input = new List<Stack<action_t>>();
        // Generate the action stack for player1
        action_t action = new action_t();
        Stack<action_t> actions = new Stack<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = 0;
        action.attack = new attack_t(new Vector2(-3, 0));
        actions.Push(action);
        input.Add(actions);
        // Generate the action stack for player2
        action = new action_t();
        actions = new Stack<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = 0;
        action.target = new Vector2(3, 0);
        actions.Push(action);
        input.Add(actions);
        Console.WriteLine("Testing blaster...");
        game_t.execute_turn(input);
        Console.WriteLine("...Passed!");
    }

    static void test_grenade()
    // Make the two players shoot each other with grenade launchers
    {
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

    static void test_vector2()
    // Kind of annoying...
    // We have the vector2 class in Unity
    // but not in here...
    {
        Console.Write("Testing the Vector2 class...");
        Vector2 v0 = new Vector2(0, 0);
        Vector2 v1 = new Vector2(1, 2);
        Vector2 v2 = new Vector2(1, 2);
        Vector2 v3 = new Vector2(2, 3);
        Debug.Assert(v0 == v0);
        Debug.Assert(v0 == Vector2.zero);
        Debug.Assert(v1 != v0);
        Debug.Assert(v0 != v1);
        Debug.Assert(v1 == v2);
        Debug.Assert(v2 == v1);
        Debug.Assert(v1 != v3);
        Debug.Assert(v2 != v3);
        Console.WriteLine("Passed!");
    }

    static void test_board()
    // Run some test on the board class
    {
        Console.Write("Testing remove/put...");
        // Test for inserting/removing
        player_t p1 = new player_t();
        Vector2 pos = new Vector2(1, 1);
        Debug.Assert(game_t.board.put_object(pos, p1));
        // Test for double insertion
        Debug.Assert(!game_t.board.put_object(pos, p1));
        // Test for invalid position
        pos = new Vector2(42, 24);
        Debug.Assert(!game_t.board.put_object(pos, p1));
        player_t p2 = new player_t();
        // They should not be equal since they are different pointers
        Debug.Assert(p1 != p2);
        pos = new Vector2(1, 1);
        Debug.Assert(game_t.board.put_object(pos, p2));
        Debug.Assert(game_t.board.remove_object(p2));
        Debug.Assert(game_t.board.remove_object(p1));
        // Test for removing twice
        Debug.Assert(!game_t.board.remove_object(p1));
        // Test the get_players method
        List<player_t> players1 = game_t.board.get_players(game_t.PLAYER_CNT);
        List<player_t> players2 = game_t.board.get_players(game_t.PLAYER_CNT);
        for (int i = 0; i < game_t.PLAYER_CNT; i++)
        {
            Debug.Assert(players1[i] == players2[i]);
        }
        // TODO: Position update in object
        Console.WriteLine("Passed!");
    }

    static void test_movement()
    // Test movement, special movement and player collisions
    {
        // Test normal movement
        Console.WriteLine("Testing movement...");
        game_t.board.print_board();
        // First turn moves two players to the far right of the board
        test_move_to_right();
        // Test other cases of collision
        test_collisions();
        Console.WriteLine("...Passed!");
    }

    static void test_move_to_right()
    // A simple movement test for any number of players
    // Tests map boundary collision, pushing and multi-step collision
    {
        int stepCount = 5;
        List<Stack<action_t>> input = new List<Stack<action_t>>();
        for (int i = 0; i < game_t.PLAYER_CNT; i++)
        {
            Stack<action_t> actions = new Stack<action_t>();
            for (int j = 0; j < stepCount; j++)
            {
                action_t action = new action_t();
                action.spMovement = new Vector2(1, 0);
                action.movement = new Vector2(1, 0);
                // -1 means no attack
                action.wpnId = 0;
                attack_t attack = new attack_t(new Vector2(0, 3));
                action.attack = attack;
                actions.Push(action);
            }
            input.Add(actions);
        }
        Console.WriteLine("Moving players to the far right...");
        game_t.execute_turn(input);
        Console.WriteLine("...Passed!");
    }

    static void test_collisions()
    // Tests two player collision at an angle
    // and head-on collision
    // In both cases the players should return to their original positions
    // Only works for two players
    {
        List<Stack<action_t>> input = new List<Stack<action_t>>();
        // Generate the action stack for player1
        action_t action = new action_t();
        Stack<action_t> actions = new Stack<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = new Vector2(-1, 1);
        action.wpnId = -1;
        actions.Push(action);
        input.Add(actions);
        // Generate the action stack for player2
        action = new action_t();
        actions = new Stack<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = new Vector2(0, 1);
        action.wpnId = -1;
        actions.Push(action);
        input.Add(actions);
        Console.WriteLine("Colliding two players at an angle...");
        game_t.execute_turn(input);
        Console.WriteLine("...Passed!");
        // Test for the case where two players swap positions
        input = new List<Stack<action_t>>();
        action = new action_t();
        actions = new Stack<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = new Vector2(-1, 0);
        action.wpnId = -1;
        actions.Push(action);
        input.Add(actions);

        action = new action_t();
        actions = new Stack<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = new Vector2(1, 0);
        action.wpnId = -1;
        actions.Push(action);
        input.Add(actions);
        Console.WriteLine("Colliding two players head on...");
        game_t.execute_turn(input);
        Console.WriteLine("...Passed!");
    }
}