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
        game_t.execute_turn(null, input);
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
    public void fire(attack_t attack,board_t board,unit_t master)
    {
        trackingMineAttack_t tma = (trackingMineAttack_t)attack;
        mine m = new mine(tma.getTarget(),board);
        board.put_object(master.get_pos(),m);
    }

}

public class mine:unit_t
{
    private board_t board;
    private int turn = 0;
    private unit_t target;
    private int centerDmg = 4;
    private int splashDmg = 2;
    public mine (unit_t target, board_t board):base("mine",1)
    {
        this.target = target;
        this.board = board;
        stepLife = -1;
        turnLife = 2;
        solid = false;
        exists = false;
    }
    public void end_turn()
    {
        if(stepLife<0)
        {
            stepLife = 3;
        }
    }

    public void step_update()
    {
        stepLife--;
        if(stepLife<0)
        {
            return;
        }
        if (stepLife == 0)
        {
            on_destroy();
            return;
        }
        Vector2 dif = target.get_pos() - this.get_pos();
        int dirX = Math.Abs(dif.x) / dif.x; // 1 if the target is on the right, else -1
        int dirY = Math.Abs(dif.x) / dif.y;// 1 if the the target is on the top, else -1
        Vector2 newX = this.get_pos() + new Vector2(dirX, 0); // new pos after changing x
        Vector2 newY = this.get_pos() + new Vector2(0, dirY); // new pos after changing y
        int disX = SS.distance(newX,target.get_pos()); // distance when moving mine through x axis;
        int disY = SS.distance(newY, target.get_pos()); // distance when moving mine through y axis;
        board.remove_object(this);
        //maybe don't need to check if the new location is on board
        if (disX<disY)
        {
            this.set_pos(newX);
            if (!board.is_free(newX))
            {
                on_destroy();//explode in new position? 
                return;
            }
            board.put_object(newX, this);
        }
        else if(disX>disY)
        {
            this.set_pos(newY);
            if (!board.is_free(newY))
            {
                on_destroy();//explode in new position? 
                return;
            }
            board.put_object(newY, this);
        }
        else
        {
            Random rand = new Random();
            int option = rand.Next(2);
            Vector2 newPos = this.get_pos() + new Vector2(dirX * option, dirY * (1 - option));
            this.set_pos(newPos);
            if (!board.is_free(newPos))
            {
                on_destroy();//explode in new position? 
                return;
            }
            board.put_object(newPos, this);
        }

        
    }

    public void on_destroy()
    {
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