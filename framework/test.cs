#define DEBUG
using System;
//using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

// As the name suggests, it is a unit that only knows to move left and right
public class zuoyouhengtiao_t : unit_t
{
    Vector2 direction = new Vector2(1, 0);
    public zuoyouhengtiao_t():base("test_unit", 5)
    {}

    public override void step_update(board_t board)
    {
        action_t action = new action_t();
        action.movement = direction;
        direction *= -1;
        // Add the new action to the bottom of the list
        actions.Insert(0, action);
    }
}

public class SSTest
{
    public static void Main(string[] args)
    {
        if (SS.DBG)
        {
            #if DEBUG
                Debug.Listeners.Add(new DumpStackTraceListener());
            #endif
            // Do all the testing
            test_vector2();
            test_board();
            test_build_weapons();
            test_weapons();
            //test_movement();
        }
    }

    static void test_weapons()
    // Test shooting at each other
    // Only works for two players
    {
        Console.WriteLine("Testing weapons...");
        test_build_weapons();
        // test_blaster();
        // test_grenade();
        //yueTest.test_weapons();
        //ruoyuanTest.test_weapons();
        wenzeTest.test_weapons();
        luTest.test_weapons();
        // TODO: Fix this weapon
        //wenquanTest.test_weapons();
        // TODO: FIx this weapon
        //ryanTest.test_weapons();
        test_units();
        Console.WriteLine("...Passed!");
    }

    static void test_build_weapons()
    // Build all the weapons for testing
    {
        board_t board = new board_t(0, 2);
        List<player_t> players = board.get_players();
        foreach(player_t player in players)
        {
            Debug.Assert(player.build_weapon(new grenadeLauncher_t())); // 1
            Debug.Assert(player.build_weapon(new plasmaCutter_t()));    // 2
            Debug.Assert(player.build_weapon(new shockCannon_t()));     // 3
            Debug.Assert(player.build_weapon(new clusterBomb_t()));     // 4
            Debug.Assert(player.build_weapon(new forceBarrier_t()));     // 5
            //Debug.Assert(player.build_weapon(new oribitMissile()));    // 6
        }
    }

    static void test_blaster()
    // Make the two players shoot each other with blasters
    {
        board_t board = new board_t(0, 2);
        List<List<action_t>> input = new List<List<action_t>>();
        // Generate the action List for player1
        action_t action = new action_t();
        List<action_t> actions = new List<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = 0;
        action.attack = new attack_t(new Vector2(-3, 0));
        actions.Add(action);
        input.Add(actions);
        // Generate the action List for player2
        action = new action_t();
        actions = new List<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = 0;
        action.target = new Vector2(3, 0);
        actions.Add(action);
        input.Add(actions);
        Console.WriteLine("Testing blaster...");
        game_t.execute_turn(board, input);
        Console.WriteLine("...Passed!");
    }

    static void test_grenade()
    // Make the two players shoot each other with grenade launchers
    {
        board_t board = new board_t(0, 2);
        List<List<action_t>> input = new List<List<action_t>>();
        // Generate the action List for player1
        action_t action = new action_t();
        List<action_t> actions = new List<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = 1;
        action.target = new Vector2(-2, 0);
        actions.Add(action);
        input.Add(actions); 
        // Generate the action List for player2
        action = new action_t();
        actions = new List<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = 1;
        action.target = new Vector2(2, 0);
        actions.Add(action);
        input.Add(actions);
        Console.WriteLine("Testing grenade...");
        game_t.execute_turn(board, input);
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
        board_t board = new board_t(0, 2);
        Debug.Assert(board.put_object(pos, p1));
        // Test for double insertion
        Debug.Assert(!board.put_object(pos, p1));
        // Test for invalid position
        pos = new Vector2(42, 24);
        Debug.Assert(!board.put_object(pos, p1));
        player_t p2 = new player_t();
        // They should not be equal since they are different pointers
        Debug.Assert(p1 != p2);
        pos = new Vector2(1, 1);
        Debug.Assert(board.put_object(pos, p2));
        Debug.Assert(board.remove_object(p2));
        Debug.Assert(board.remove_object(p1));
        // Test for removing twice
        Debug.Assert(!board.remove_object(p1));
        // Test the get_players method
        List<player_t> players1 = board.get_players();
        List<player_t> players2 = board.get_players();
        for (int i = 0; i < 2; i++)
        {
            Debug.Assert(players1[i] == players2[i]);
        }
        // TODO: Position update in object
        Console.WriteLine("Passed!");
    }

    static void test_movement()
    // Test movement, special movement and player collisions
    {
        board_t board = new board_t(0, 2);
        // Test normal movement
        Console.WriteLine("Testing movement...");
        board.print_board();
        // First turn moves two players to the far right of the board
        test_move_to_right(board);
        // Test other cases of collision
        test_collisions(board);
        Console.WriteLine("...Passed!");
    }

    static void test_move_to_right(board_t board)
    // A simple movement test for any number of players
    // Tests map boundary collision, Adding and multi-step collision
    {
        int playerCount = board.playerCount;
        int stepCount = 5;
        List<List<action_t>> input = new List<List<action_t>>();
        for (int i = 0; i < playerCount; i++)
        {
            List<action_t> actions = new List<action_t>();
            for (int j = 0; j < stepCount; j++)
            {
                action_t action = new action_t();
                action.spMovement = new Vector2(1, 0);
                action.movement = new Vector2(1, 0);
                // -1 means no attack
                action.wpnId = 0;
                attack_t attack = new attack_t(new Vector2(0, 3));
                action.attack = attack;
                actions.Add(action);
            }
            input.Add(actions);
        }
        Console.WriteLine("Moving players to the far right...");
        game_t.execute_turn(board, input);
        Console.WriteLine("...Passed!");
    }

    static void test_collisions(board_t board)
    // Tests two player collision at an angle
    // and head-on collision
    // In both cases the players should return to their original positions
    // Only works for two players
    {
        List<List<action_t>> input = new List<List<action_t>>();
        // Generate the action List for player1
        action_t action = new action_t();
        List<action_t> actions = new List<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = new Vector2(-1, 1);
        action.wpnId = -1;
        actions.Add(action);
        input.Add(actions);
        // Generate the action List for player2
        action = new action_t();
        actions = new List<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = new Vector2(0, 1);
        action.wpnId = -1;
        actions.Add(action);
        input.Add(actions);
        Console.WriteLine("Colliding two players at an angle...");
        game_t.execute_turn(board, input);
        Console.WriteLine("...Passed!");
        // Test for the case where two players swap positions
        input = new List<List<action_t>>();
        action = new action_t();
        actions = new List<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = new Vector2(-1, 0);
        action.wpnId = -1;
        actions.Add(action);
        input.Add(actions);

        action = new action_t();
        actions = new List<action_t>();
        action.spMovement = Vector2.zero;
        action.movement = new Vector2(1, 0);
        action.wpnId = -1;
        actions.Add(action);
        input.Add(actions);
        Console.WriteLine("Colliding two players head on...");
        game_t.execute_turn(board, input);
        Console.WriteLine("...Passed!");
    }

    static void test_units()
    {
        test_unit_movement();
        test_turret();
    }

    static void test_unit_movement()
    // Make a unit that moves back and forth
    // and see if it works and collides with the players correctly
    {
        board_t board = new board_t(0, 2);
        board.put_object(new Vector2(2, 0), new zuoyouhengtiao_t());
        List<List<action_t>> input = new List<List<action_t>>();
        // Generate the action List for player1
        List<action_t> actions = new List<action_t>();
        int steps = 5;
        for (int i = 0; i < steps; i++)
        {
            actions.Add(new action_t());
        }
        input.Add(actions);
        actions = new List<action_t>();
        for (int i = 0; i < steps; i++)
        {
            actions.Add(new action_t());
        }
        input.Add(actions);
        Console.WriteLine("Testing unit movement...");
        game_t.execute_turn(board, input);
        Console.WriteLine("...passed!");
    }

    static void test_turret()
    {
        board_t board = new board_t(0, 2);
        board.put_object(new Vector2(0, 3), new turret_t());
        List<List<action_t>> input = new List<List<action_t>>();
        // Generate the action List for player1
        List<action_t> actions = new List<action_t>();
        int steps = 3;
        for (int i = 0; i < steps; i++)
        {
            actions.Add(new action_t());
        }
        input.Add(actions);
        actions = new List<action_t>();
        for (int i = 0; i < steps; i++)
        {
            actions.Add(new action_t());
        }
        input.Add(actions);
        Console.WriteLine("Testing turret...");
        game_t.execute_turn(board, input);
        
        input.Clear();
        actions.Clear();
        for (int i = 0; i < steps; i++)
        {
            action_t action = new action_t();
            action.movement = new Vector2(1, -1);
            actions.Add(action);
        }
        actions.Add(new action_t());
        input.Add(actions);
        actions = new List<action_t>();
        for (int i = 0; i < steps; i++)
        {
            action_t action = new action_t();
            action.movement = new Vector2(1, 1);
            actions.Add(action);
        }
        input.Add(actions);
        game_t.execute_turn(board, input);
        // Turn 3
        input.Clear();
        actions.Clear();
        for (int i = 0; i < steps; i++)
        {
            actions.Add(new action_t());
        }
        input.Add(actions);
        actions = new List<action_t>();
        for (int i = 0; i < steps; i++)
        {
            actions.Add(new action_t());
        }
        input.Add(actions);
        game_t.execute_turn(board, input);
        Console.WriteLine("...passed!");
    }
}