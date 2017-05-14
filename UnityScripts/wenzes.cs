#define DEBUG
using System;
using UnityEngine;
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
        board_t board = new board_t(0, 2);
        player_t player = board.get_players()[0];
        player.build_weapon(new shockCannon_t());
        // This is a sample test
        // Write your own test with this as a template
        List<List<action_t>> input = new List<List<action_t>>();
        // Generate the action List for player1
        List<action_t> actions = new List<action_t>();
        // Generate one action
        // #############################
        action_t action;
        // player 1 moves
        for (int i = 0; i<5; i++)
        {
            action = new action_t();
            action.spMovement = Vector2.zero;
            action.movement = new Vector2(1,0);
            action.wpnId = -1;
            actions.Add(action);
        }
        // player 1 attacks
        action = new action_t();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.attack = new dirAttack_t(3);
        action.wpnId = 1;
        actions.Add(action);
        // player 1 moves
        for (int i = 0; i<5; i++)
        {
            action = new action_t();
            action.spMovement = Vector2.zero;
            action.movement = new Vector2(-1,0);
            action.wpnId = -1;
            actions.Add(action);
        }
        // add player 1 actions to input
        input.Add(actions);
        // Generate the action List for player2
        actions = new List<action_t>();
        // player 2 does nothing
        action = new action_t();
        action.spMovement = Vector2.zero;
        action.movement = Vector2.zero;
        action.wpnId = -1;
        actions.Add(action);
        // add player 2 actions to input
        input.Add(actions);
        Console.WriteLine("Testing Shock Cannon...");
        game_t.execute_turn(board, input);
        Console.WriteLine("...Passed!");

        input = new List<List<action_t>>();
        actions = new List<action_t>();
        for (int i = 0; i<3; i++)
        {
            action = new action_t();
            action.spMovement = Vector2.zero;
            action.movement = new Vector2(0,-1);
            action.wpnId = -1;
            actions.Add(action);
        }
        input.Add(actions);
        actions = new List<action_t>();
        for (int i = 0; i<3; i++)
        {
            action = new action_t();
            action.spMovement = Vector2.zero;
            action.movement = new Vector2(0,-1);
            action.wpnId = -1;
            actions.Add(action);
        }
        input.Add(actions);
        Console.WriteLine("Testing Shock Cannon new round...");
        game_t.execute_turn(board, input);
        Console.WriteLine("...passed!");
    }
}

public class dirAttack_t : attack_t
{
    public int dir;
    public dirAttack_t(int dir):base(Vector2.zero)
    {
        this.dir = dir;
    }
}

public class shockDamage_t : damage_t
{

    player_t player;

    Vector2[] piecePos = {new Vector2(1,0), new Vector2(2,0), new Vector2(1,1),
                    new Vector2(0,1), new Vector2(0,2), new Vector2(-1,1),
                    new Vector2(-1,2), new Vector2(-1,0), new Vector2(-2,0),
                    new Vector2(-2,2), new Vector2(-2,1), new Vector2(-1,-1),
                    new Vector2(0,-1), new Vector2(0,-2), new Vector2(1,-1),
                    new Vector2(2,-1), new Vector2(1,-2), new Vector2(2,-2)};

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
        base.end_turn(board);
        if (player != null){
            int numOfPieces = player.get_hp()/2;
            player.take_damage(player.get_hp()/2);

            //creating pieces of energy that can be picked up by players
            damage_t piece;
            int randomPos;
            Vector2 targetPos;
            System.Random rnd = new System.Random();

            for (int i = 0; i < numOfPieces; i++) {
                randomPos = rnd.Next(18);
                Vector2 vec = piecePos[randomPos];
                targetPos = vec + player.get_pos();
                piece = new damage_t();
                piece.set_params(-1, 0);
                piece.set_pos(targetPos);
                piece.stepLife = -1;
                piece.turnLife = -1;
                board.put_later(targetPos, piece);
            }
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

    int get_direction(Vector2 mousePos, Vector2 playerPos) {
        if (SS.distance(mousePos, playerPos) == 1){
            for (int i = 0; i < 6; i++){
                if (mousePos - playerPos == pos[i, 2])  return i;
            }
        }
        else if (SS.distance(mousePos, playerPos) == 2){
            for (int i = 0; i < 6; i++){
                if (mousePos - playerPos == pos[i, 1])  return i;
            }
        }
        return -1;
    }

    public override inputMode_t generate_action(action_t action, 
        Vector2 playerPos, Vector2 mousePos, inputMode_t inputMode)
    // Change the action based on user input and return the next inputMode
    {
        // This is the most general case
        // So we assume that the attack is not generated
        // And we are not doing a special movement
        System.Diagnostics.Debug.Assert(action.attack == null);
        System.Diagnostics.Debug.Assert(inputMode == inputMode_t.ATTACK);
        // Add the attack to the action
        action.attack = new dirAttack_t(get_direction(mousePos, playerPos));
        return inputMode_t.MOVE;
    }
        
    public override inputMode_t cancel_action(action_t action, inputMode_t inputMode)
    {
        if (inputMode == inputMode_t.ATTACK)
            return inputMode_t.WEAPON;
        else if (inputMode == inputMode_t.MOVE) {
            action.target = Vector2.zero;
            return inputMode_t.ATTACK;
        } 
        else {
            return inputMode_t.MOVE;
        }
    }

    bool is_valid_attack(Vector2 tilePos, Vector2 playerPos, Vector2 mousePos)
    {
        int dir = get_direction(mousePos, playerPos);
        if (dir == -1)
            return false;
        for (int i = 0 ; i < 3; i++){
            if (tilePos - playerPos == pos[dir, i])  return true;
        }
        return false;
    }

    public override tileMode_t get_tile_mode(Vector2 tilePos, Vector2 playerPos, 
        Vector2 mousePos, inputMode_t inputMode, board_t board, unit_t master)
    // Returns the tile mode of the tile at tilePos
    // Generally speaking, when inputMode is ATTACK: 
    // tile.isOutOfRange = true if it is out of range from playerPos;
    // is validAttack if it is in range and have the mouse over it.
    {
        System.Diagnostics.Debug.Assert(inputMode == inputMode_t.ATTACK);
        tileMode_t result = new tileMode_t();
        if (!is_in_range(playerPos, tilePos, master, board)) {
            result.isOutOfRange = true;
        }
        else {
            result.isValidTarget = is_valid_attack(tilePos, playerPos, mousePos);
        }
        return result;
    }

    public override bool is_in_range(Vector2 playerPos, Vector2 targetPos,
        unit_t master, board_t board)
    // Returns true if targetPos is within attack range from playerPos
    // Ignoring obstacles.
    {
        for (int i = 0; i < 6; i++) {
            for (int j = 0; j < 3; j++) {
                if (targetPos - playerPos == pos [i, j]) {
                    return true;
                }
            }
        }
        return false;
    }

}