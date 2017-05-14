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
        // This is a sample test
        // Write your own test with this as a template
        List<List<action_t>> input = new List<List<action_t>>();
        // Generate the action List for player1
        List<action_t> actions = new List<action_t>();
        board_t board = new board_t(0, 2);
        player_t player1 = board.get_players()[0];
        player_t player2 = board.get_players()[1];
        player1.build_weapon(new trackingMine_t());
        // Generate one action
        // #############################
        // Move away
        action_t action = new action_t();
        action.spMovement = Vector2.zero;
        action.movement = new Vector2(-1, 1);
        action.wpnId = -1;
        action.attack = null;
        actions.Add(action);
        // #############################
        // Attack the other player
        action = new action_t();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = 1;
        action.attack = new trackingMineAttack_t(player2);
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
        Console.WriteLine("Testing tracking mine...");
        game_t.execute_turn(board, input);
        // Second turn
        // Two players just wait
        // #############################
        input = new List<List<action_t>>();
        int numSteps = 5;
        // Generate the action List for player1
        actions = new List<action_t>();
        for (int i = 0; i < numSteps; i++)
        {
            // #############################
            // Do nothing
            action = new action_t();
            action.spMovement = Vector2.zero;
            action.movement = Vector2.zero;
            action.wpnId = -1;
            actions.Add(action);
        }
        input.Add(actions);
        // Generate the action List for player2
        actions = new List<action_t>();
        for (int i = 0; i < numSteps; i++)
        {
            // #############################
            // Do nothing
            action = new action_t();
            action.spMovement = Vector2.zero;
            action.movement = Vector2.zero;
            action.wpnId = 0;
            action.attack = new attack_t(new Vector2(2, 0));
            actions.Add(action);
        }
        input.Add(actions);
        game_t.execute_turn(board, input);
        Console.WriteLine("...Passed!");
    }
}

// ###################################################################
// Write your weapon class here
// ###################################################################
public class trackingMineAttack_t : attack_t
{
    private unit_t target;
    public trackingMineAttack_t(unit_t target) : base(null)
    {
        this.target = target;
    }
    public unit_t getTarget()
    {
        return target;
    }
}

public class trackingMine_t : weapon_t
{
    static int[] modules = { 0, 1, 1, 0 };
    int splashDamage = 2;
    public trackingMine_t():base(range:1,damage:4,delay:-1,modules:modules)
    {       
    }
    public override bool fire(attack_t attack,board_t board,unit_t master)
    {
        trackingMineAttack_t tma = (trackingMineAttack_t)attack;
        mine m = new mine(tma.getTarget());
        board.put_object(master.get_pos(),m);
        return true;
    }

}

public class mine : unit_t
{
    // private board_t board;
    private int turn = 0;
    private unit_t target;
    private int centerDmg = 4;
    private int splashDmg = 2;
    bool exploded = false;

    public mine (unit_t target):base("mine",1)
    {
        this.target = target;
        // this.board = board;
        stepLife = -1;
        turnLife = 2;
        solid = true;
        exists = false;
    }
    public override void end_turn(board_t board)
    {
        base.end_turn(board);
        if(stepLife < 0)
        {
            stepLife = 3;
            this.exists = true;
        }
    }

    public override void step_update(board_t board)
    {
        if (!this.exists) return;
        board.remove_later(this);
        Vector2 dif = target.get_pos() - this.get_pos();
        int dirX = Math.Sign(dif.x); // 1 if the target is on the right, else -1
        int dirY = Math.Sign(dif.y); // 1 if the the target is on the top, else -1
        Vector2 newX = this.get_pos() + new Vector2(dirX, 0); // new pos after changing x
        Vector2 newY = this.get_pos() + new Vector2(0, dirY); // new pos after changing y
        int disX = SS.distance(newX, target.get_pos()); // distance when moving mine through x axis;
        int disY = SS.distance(newY, target.get_pos()); // distance when moving mine through y axis;
        //maybe don't need to check if the new location is on board 
        Vector2 newPos;
        if (disX<disY)
        {
            if (!board.is_free(newX))
            {
                this.solid = false;
                this.take_damage(1);
            }
            SS.dbg_log("Mine moving to " + newX.ToString());
            newPos = newX;
        }
        else if(disX>disY)
        {
            if (!board.is_free(newY))
            {
                this.solid = false;
                this.take_damage(1);
            }
            SS.dbg_log("Mine moving to " + newY.ToString());
            newPos = newY;
        }
        else
        {
            Random rand = new Random();
            int option = rand.Next(2);
            newPos = this.get_pos() + new Vector2(dirX * option, dirY * (1 - option));
            if (!board.is_free(newPos))
            {
                this.solid = false;
                this.take_damage(1);
            }
            SS.dbg_log("Mine moving to " + newPos.ToString());
        }
        stepLife--;
        if (stepLife == 0)
        {
            on_destroyed(board);
        }
        else
        {
            board.put_later(newPos, this);
        }
    }

    public override void on_destroyed(board_t board)
    {
        if (exploded) return;
        exploded = true;
        SS.dbg_log("Mine explode!");
        Vector2 pos = this.get_pos();
        //center damage,
        damage_t cd = new damage_t();
        cd.set_params(centerDmg, 0);
        cd.set_pos(pos);
        cd.stepLife = 1;
        board.create_damage(cd);
        //splash damage
        for(int i=0; i<6; i++)
        {
            damage_t sd = new damage_t();
            sd.set_params(splashDmg, 0);
            sd.set_pos(pos + SS.DIRECTIONS[i]);
            sd.stepLife = 1;
            board.create_damage(sd);
        }
    }
}
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