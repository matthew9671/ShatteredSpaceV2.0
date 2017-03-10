#define DEBUG
using System;
//using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

public class wenzeTest
{
    public static void test_weapons()
    {
        test_shock_cannon();
    }
    
    public static void test_shock_cannon()
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

public class dirAttack_t : attack_t
{
    public int dir;
    public dirAttack_t():base(null)
    {}
}

public class shockDamage_t : damage_t
{

    player_t player;

    public override void on_collision(object_t other){
        if (other is player_t && player == null)
        {
            player = other as player_t;
        }
    }

    public override void step_update(board_t board)
    {
        if (player == null)
        {
            // Self destruct
            board.remove_later(this);
        }
    }

    public override void end_turn(board_t board){
        if (player != null){
            player.take_damage(player.get_hp()/2);
        }
    }
}

public class shockCannon_t : weapon_t
{
    static int[] modules = {0, 0, 1, 2};
    int dir;

    public shockCannon_t():base(range:2, damage:0, delay:0, modules:modules)
    {}

    Vector2[,] pos = {{new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)},
                    {new Vector2(0,1), new Vector2(-1,2), new Vector2(-1,1)},
                    {new Vector2(-1,1), new Vector2(-2,1), new Vector2(-1,0)},
                    {new Vector2(-1,0), new Vector2(-1,-1), new Vector2(0,-1)},
                    {new Vector2(0,-1), new Vector2(1,-2), new Vector2(1,-1)},
                    {new Vector2(1,-1), new Vector2(2,-1), new Vector2(1,0)}};

    public override bool fire(attack_t attack, board_t board, unit_t master){
        dirAttack_t dirAttack = attack as dirAttack_t;
        int dir = dirAttack.dir;
        Vector2 targetPos;
        int delay = get_delay(master);
        for (int i = 0; i < 3; i++){
            Vector2 vec = pos[dir, i];
            targetPos = vec + master.get_pos();
            shockDamage_t dmg = new shockDamage_t();
            dmg.set_params(0, delay);
            dmg.set_pos(targetPos);
            dmg.stepLife = -1;
            dmg.turnLife = 1;
            board.create_damage(dmg);
        }
        return true;
    }

}