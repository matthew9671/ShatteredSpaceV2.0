#define DEBUG
using System;
//using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

public class luTest
{
    public static void test_weapons()
    {
        test_cluster_bomb();
    }

    public static void test_cluster_bomb()
    {
        // This is a sample test
        // Write your own test with this as a template
        List<Stack<action_t>> input = new List<Stack<action_t>>();
        // Generate the action stack for player1
        Stack<action_t> actions = new Stack<action_t>();
        // Generate one action
        // #############################
        action_t action1 = new action_t();
        action1.spMovement = Vector2.zero;
        action1.movement = Vector2.zero;
        action1.wpnId = 4;
        action1.attack = new attack_t(new Vector2(2, 2));
        actions.Push(action1);

        // #############################
        input.Add(actions);
        // Generate the action stack for player2
        actions = new Stack<action_t>();
        // #############################
        // action = new action_t();
        // action.spMovement = Vector2.zero;
        // action.movement = Vector2.zero;
        // action.wpnId = -1;
        // actions.Push(action);
        // #############################
        input.Add(actions);
        Console.WriteLine("Testing cluster bomb...");
        game_t.execute_turn(input);

        input = new List<Stack<action_t>>();
        // Generate the action stack for player1
        actions = new Stack<action_t>();
        // Generate one action
        // #############################
        action1 = new action_t();
        action1.spMovement = Vector2.zero;
        action1.movement = Vector2.zero;
        action1.wpnId = -1;
        actions.Push(action1);
        action1 = new action_t();
        action1.spMovement = Vector2.zero;
        action1.movement = Vector2.zero;
        action1.wpnId = -1;
        actions.Push(action1);
        action1 = new action_t();
        action1.spMovement = Vector2.zero;
        action1.movement = Vector2.zero;
        action1.wpnId = -1;
        actions.Push(action1);
        // #############################
        input.Add(actions);
        // Generate the action stack for player2
        actions = new Stack<action_t>();
        // #############################
        // action = new action_t();
        // action.spMovement = Vector2.zero;
        // action.movement = Vector2.zero;
        // action.wpnId = -1;
        // actions.Push(action);
        // #############################
        input.Add(actions);
        game_t.execute_turn(input);
        Console.WriteLine("...Passed!");
    }
}

public class clusterDamage_t : damage_t
{

    int subDamage;
    int subSplash;
    int bombCount;
    int delay = 1;

    public clusterDamage_t(int subDamage, int subSplash, int bombCount)
    {
        this.subDamage = subDamage;
        this.subSplash = subSplash;
        this.bombCount = bombCount;
    }

    Random _random = new Random();
    public T[] Shuffle<T>(T[] array)
    {
        var random = _random;
        for (int i = array.Length; i > 1; i--)
        {
            // Pick random element to swap.
            int j = random.Next(i); // 0 <= j <= i-1
            // Swap.
            T tmp = array[j];
            array[j] = array[i - 1];
            array[i - 1] = tmp;
        }
        return array;
    }

    public override void end_turn(board_t board)
    {
        base.end_turn(board);

        SS.dbg_log("End of turn for clusterBomb!");
        // TODO: Generate 3 small bombs
        Vector2 pos = this.get_pos();
        Vector2[] randDir = Shuffle<Vector2>(SS.DIRECTIONS);
        Vector2[] subDamDir = {pos+randDir[0], pos+randDir[1], pos+randDir[2]};
        SS.dbg_log("random"+subDamDir[0]);
        SS.dbg_log("random"+subDamDir[1]);
        SS.dbg_log("random"+subDamDir[2]);


        // Generate the damage in the center

        int amount = subDamage;
        damage_t dmg = new damage_t();
        dmg.set_params(amount, delay);
        dmg.set_pos(pos);
        dmg.stepLife = 1;
        board.create_damage(dmg);
        generate_splash_damage(pos, board, null);

        damage_t dmg0 = new damage_t();
        dmg0.set_params(amount, delay);
        dmg0.set_pos(subDamDir[0]);
        dmg0.stepLife = 1;
        board.create_damage(dmg0);
        generate_splash_damage(subDamDir[0], board, null);

        damage_t dmg1 = new damage_t();
        dmg1.set_params(amount, delay);
        dmg1.set_pos(subDamDir[1]);
        dmg1.stepLife = 1;
        board.create_damage(dmg1);
        generate_splash_damage(subDamDir[1], board, null);


    }

    void generate_splash_damage(Vector2 pos, board_t board, unit_t master)
    {
        Debug.Assert(pos != null);
        int amount = subSplash;
        foreach (Vector2 dir in SS.DIRECTIONS)
        {
            damage_t dmg = new damage_t();
            dmg.set_params(amount, delay);
            dmg.set_pos(pos + dir);
            dmg.stepLife = 1;
            board.create_damage(dmg);
        }
    }
}

public class clusterBomb_t : weapon_t
{
    static int[] modules = {0, 3, 0, 0};
    const int SPLASH_DAMAGE = 2;
    const int SUB_DAMAGE = 3;
    const int SUB_SPLASH = 2;
    const int BOMB_COUNT = 3;
    public clusterBomb_t():base(range:5, damage:5, delay:-1, modules:modules)
    {}

    public int get_splash_damage(unit_t master)
    // There might be upgrades that increases the splash damage
    {
        return SPLASH_DAMAGE;
    }

    public override bool fire(attack_t attack, board_t board, unit_t master)
    // Create pending damage on the board, 
    // return false if this is an invalid attack. 
    // May have some special effects on the master.
    {
        Vector2 pos = attack.target;
        generate_splash_damage(pos, board, master);
        // Generate the special damage in the center
        Debug.Assert(attack != null);
        int amount = get_damage_amount(master);
        int delay = get_delay(master);
        clusterDamage_t dmg = new clusterDamage_t(SUB_DAMAGE, SUB_SPLASH, 
            BOMB_COUNT);
        dmg.set_params(amount, delay);
        dmg.set_pos(pos);
        dmg.stepLife = -1;
        dmg.turnLife = 1;
        board.create_damage(dmg);
        return board.is_in_board(pos);
    }

    void generate_splash_damage(Vector2 pos, board_t board, unit_t master)
    {
        Debug.Assert(pos != null);
        int amount = get_splash_damage(master);
        int delay = get_delay(master);
        foreach (Vector2 dir in SS.DIRECTIONS)
        {
            blastWave_t dmg = new blastWave_t(dir);
            dmg.set_params(amount, delay);
            dmg.set_pos(pos + dir);
            dmg.stepLife = 1;
            board.create_damage(dmg);
        }
    }
}