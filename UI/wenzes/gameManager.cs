// Shi Wenze's version of the gameManager_t
// Will eventually be combined with others' versions
// This would only work in Unity

// #############################################################################
// ###########
// ##READ ME##
// ###########

// The board tiles is the board that the players see in game. 
// In the planning phase, players click on the board tiles to plan their 
// movements and attacks. In Shattered Space V1.0 movement and attack are 
// planned altogether - there is no hint that tells you what you should do 
// and in what order. Now we are going to change that.

// Now in the planning phase there are several input modes: 
// ATTACK, MOVE, SPMOVE and WEAPON. In each input mode the relevant tiles are 
// highlighted while the others turns gray. When the player inputs a command by 
// clicking on a valid tile, the input mode changes to ask the player for 
// another input.

// How the tiles are highlighted (which is to say the mode of each tile) 
// depend on the input mode, the player's position, the board arrangement, 
// the mouse's position and the weapon that the player is using. For example, 
// in MOVE mode, only the tile at the player's position and 6 tiles right next 
// to the player should light up. The other tiles should turn gray.

// #############################################################################

// Relevant stuff defined in game.cs
// Reproduced here for your convenience

// After we fully transition to Unity we will move these to gameManager.cs
// public enum inputMode_t {NONE, ATTACK, MOVE, SPMOVE, WEAPON};

// public struct tileMode_t
// {
//     // True if we know some damage with positive amount is going to hit the tile in the future
//     public bool isDangerous;
//     // Meaningful only when isDangerous is true
//     // Usually -1, 0 or 1
//     // If it's -1, the damage falls at end of turn
//     // 0 means that the damage is on the tile right now
//     // 1 means that the damage will be put on the tile a step later
//     public int stepsToDamage;
//     public bool isOutOfRange;
//     public bool isValidMove;
//     public bool isValidTarget;
// }

// Some relevant methods in the weapon_t class 

//     public virtual inputMode_t generate_action(action_t action, 
//         Vector2 playerPos, Vector2 mousePos, inputMode_t inputMode)
//     // Change the action based on user input and return the next inputMode
//     {
//         // This is the most general case
//         // So we assume that the attack is not generated
//         // And we are not doing a special movement
//         Debug.Assert(action.attack == null);
//         Debug.Assert(inputMode == inputMode_t.ATTACK);
//         // Add the attack to the action
//         action.attack = new attack_t(mousePos);
//         return inputMode_t.MOVE;
//     }

//     public virtual tileMode_t get_tile_mode(Vector2 tilePos, Vector2 playerPos, 
//         Vector2 mousePos, inputMode_t inputMode, board_t board, unit_t master)
//     // Returns the tile mode of the tile at tilePos
//     // Generally speaking, when inputMode is ATTACK: 
//     // tile.isOutOfRange = true if it is out of range from playerPos;
//     // is validAttack if it is in range and have the mouse over it.
//     {
//         Debug.Assert(inputMode == inputMode_t.ATTACK);
//         tileMode_t result = new tileMode_t();
//         if (!is_in_range(playerPos, tilePos, master))
//         {
//             result.isOutOfRange = true;
//         }
//         else if (mousePos == tilePos)
//         {
//             result.isValidTarget = true;
//         }
//         return result;
//     }
// #############################################################################

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class tile_t : MonoBehaviour
{
    tileMode_t mode;
    public Vector2 position;
    Renderer rend;
    bool flashing = false;

    Color flashingColor = Color.green;
    Color validMoveColor = Color.green;
    Color validAtkColor = Color.red;
    Color outOfRangeColor = Color.gray;
    
    // ######################################################
    // Unity methods
    void Start()
    {
        rend = GetComponent<Renderer> ();
    }

    void Update()
    // This function gets called every frame
    // Useful for animations
    {
        Color maxColor = Color.white;
        if (flashing) {
            rend.material.color = Color.Lerp (flashingColor, maxColor, Mathf.PingPong (Time.time, 0.7f));
        }
    }

    void OnMouseEnter()
    {
        gameManager_t.mouse_enter(position);
    }

    void OnMouseExit()
    {
        gameManager_t.mouse_exit();
    }

    void OnMouseUp()
    // Triggered when the user releases the left mouse button
    // Doesn't work on the right mouse button
    {
        gameManager_t.add_input();
    }

    // End of Unity methods
    // ######################################################

    public void update_tile_mode(tileMode_t newMode)
    // Updates the tile mode and changes the appearence of the tile
    {
        if (mode != newMode)
        {
            mode = newMode;
            update_appearence();
        }
    }

    void update_appearence()
    // Updates the tile's appearence according to the current mode\
    // Turns gray if isOutOfRange
    // Flashes green if isValidMove
    // Flashes red if isValidAttack
    {
        rend.material.color = Color.white;
        flashing = false;
        if (mode.isValidMove) {
            rend.material.color = validMoveColor;
            flashing = true;
            flashingColor = validMoveColor;
        } else if (mode.isOutOfRange) {
            rend.material.color = outOfRangeColor;
        } else if (mode.isValidTarget) {
            rend.material.color = validAtkColor;
            flashing = true;
            flashingColor = validAtkColor;
        }
    }
}

public static class gameManager_t
{
    // Input is a component in the action: weaponId, attack, movement or spMovement
    // inputMode tells us which kind of input we want from the user right now
    // This line is commented out because it is defined in game.cs
    // enum inputMode_t {NONE, ATTACK, MOVE, SPMOVE, WEAPON};
    public static inputMode_t inputMode;
    // The temporary board is a duplicate board object with only one player
    public static int playerCount = 2;
    public static tile_t[,] tiles;
    public static board_t board;
    public static board_t tempBoard;
    public static weapon_t weapon;
    // The player is also a temporary duplicate
    public static player_t tempPlayer;
    // The position of the TILE that the mouse is currently on
    // NOT the position of the mouse on the screen!
    public static Vector2 mousePos;
    // The current action we are writing to 
    public static action_t action;
    // The action stack of the player we are generating
    public static Stack<action_t> actions;
    public static GameObject gameTile;
    public static int spacing = 1;

    // Set functions
    public static void set_mode(inputMode_t mode)
    {
        inputMode = mode;
        update_tiles();
    }

    public static void set_weapon(weapon_t wpn){weapon = wpn;}

    public static void set_mouse_position(Vector2 pos)
    {
        mousePos = pos;
        update_tiles();
    }

    public static void init_game()
    // Called when the game starts
    {
        Transform boardHolder = new GameObject ("Board").transform;
        gameTile = GameObject.Find ("Quad");
        gameTile.AddComponent<tile_t> ();
        board = new board_t(0, playerCount);
        tiles = new tile_t[board.mapH, board.mapW];
        // TODO: Generate the physical board (an array of tile_t's) here
        for (int row = 0; row < board.mapH; row++) {
            for (int col = 0; col < board.mapW; col++) {
                int boardX = col - board.centerCol;
                int boardY = board.centerRow - row;
                if (board.is_in_board (new Vector2 (boardX, boardY))) {
                    float x = boardX * spacing + boardY * 0.5f * spacing;
                    float y = boardY * spacing * Mathf.Sqrt (3) / 2;
                    GameObject instance = GameObject.Instantiate (gameTile, new Vector3 (x, y, 0), Quaternion.identity) as GameObject;
                    tile_t tile = instance.GetComponent<tile_t> ();
                    tile.position = new Vector2 (boardX, boardY);
                    tiles [row, col] = tile;
                    instance.transform.SetParent (boardHolder);
                }
            }
        }
        GameObject.Destroy (gameTile);
        init_planning ();
    }

    public static void init_planning()
    // Called when a turn ends and the planning phase starts
    // Creates a temporary duplicate of the player and board, 
    // removes the opponent from the temporary board
    {
        // Get the copy of the board that has only one player
        tempBoard = board_t.solo_copy(board);
        tempPlayer = tempBoard.get_players()[0];
        // Planning always starts with choosing a weapon
        inputMode = inputMode_t.WEAPON;
        action = new action_t(Vector2.zero);
        actions = new Stack<action_t>();
        actions.Push(action);
    }

    public static void mouse_exit()
    // Called by tile_t
    // Triggers when the mouse exits a tile
    {
        set_mouse_position(Vector2.zero);
    }

    public static void mouse_enter(Vector2 tilePos)
    // Called by tile_t
    // Triggers when the mouse first enters that tile
    {
        set_mouse_position(tilePos);
    }

    static void update_tiles()
    // Called whenever the user moves the mouse in/out of a tile,
    // left clicks on a tile (adds input) or right clicks anywhere on the board (cancels input)
    // Updates the appearance of all the tiles
    {
        // Call get_tile_mode on every tile position and call update_tile_mode on the tile_t
        for (int row = 0; row < board.mapH; row++) {
            for (int col = 0; col < board.mapW; col++) {
                int boardX = col - board.centerCol;
                int boardY = board.centerRow - row;
                tileMode_t cur = get_tile_mode (new Vector2 (boardX, boardY), tempPlayer.get_pos());
                if (tiles [row, col] != null) {
                    tiles [row, col].update_tile_mode (cur);
                }
            }
        }
    }

    static tileMode_t get_tile_mode(Vector2 tilePos, Vector2 playerPos)
    // Return the tileMode of the tile on tilePos given playerPos and mousePos
    {
        // Get the stepsToDamage
        // If in WEAPON mode, make all tiles isOutOfRange 
        // (since we are clicking on the weapon selection menu instead of the board)
        // If in MOVE mode, make all tiles isOutOfRange except for tiles within one distance from playerPos (even if it is occupied by a solid object!)
        // If the mouse happens to be on the tile and it is within movement range, 
        // make it isValidMove (same for attack)
        // If in ATTACK mode, call get_tile_mode on the weapon
        // If in SPMOVE mode, also consult the weapon
        // In addition to the steps above, we also need to set the isDangerous and stepsToDamage
        // by consulting tempBoard
        tileMode_t tile = new tileMode_t();
        if (inputMode == inputMode_t.WEAPON){
            tile.isOutOfRange = true;
            tile.isValidMove = false;
            tile.isValidTarget = false;
        }
        else if (inputMode == inputMode_t.MOVE){
            if (SS.distance(tilePos, playerPos) <= 1){
                tile.isOutOfRange = false;
            }
            else tile.isOutOfRange = true;
            tile.isValidMove = false;
            if (mousePos == tilePos && !tile.isOutOfRange){
                tile.isValidMove = true;
            }
            tile.isValidTarget = false;
        }

        else{
            tile = weapon.get_tile_mode(tilePos, playerPos, mousePos, inputMode,
                tempBoard, tempPlayer);
        }
        tempBoard.set_dangerous(tilePos, tile);
        return tile;
    }

    public static bool add_input()
    // Triggered when a tile is clicked on
    // If the mouse is clicking on a valid tile then add an input, change the input mode and return true;
    // If not, ignore it and return false.
    // ###########################################################
    // Some cases of the input change we want to keep in mind
    // Input mode change (General case): WEAPON -> ATTACK -> MOVE
    // Choose not to attack/no weapons to choose/weapon don't need targeting: 
    // WEAPON -> MOVE
    // Multiple attack weapons (Force field, Teleport bomb):
    // WEAPON -> ATTACK (choose first target) -> ATTACK (choose second target) -> MOVE
    // Special movement weapons (Combustion thruster, Recoil cannon):
    // WEAPON -> ATTACK(or skip it in the case of combustion thruster) -> SPMOVE -> MOVE
    // ###########################################################
    {
        // If the tile clicked on is invalid, return false;
        // If valid,
        // We have to be in the ATTACK, MOVE, or SPMOVE modes:
        // if in MOVE, add the movement to action and push it on the action stack
        // and transition to WEAPON if there are still steps left, otherwise finish;
        // if in ATTACK or SPMOVE, pass all the required parameters 
        // to the generate_action method in the weapon.
        // The generate_action method changes the current action and returns the 
        // input mode to transition to (usually MOVE)
        // We do not consider number of maximum steps for the moment
        tileMode_t cur = get_tile_mode(mousePos, tempPlayer.get_pos());
        if (!cur.isValidMove) return false;
        if (inputMode == inputMode_t.WEAPON)
            return false;
        else if (inputMode == inputMode_t.ATTACK || inputMode == inputMode_t.SPMOVE) {
            inputMode = weapon.generate_action (action, tempPlayer.get_pos (), mousePos, inputMode);
        } else {
            Vector2 diff = mousePos - tempPlayer.get_pos (); 
            action = new action_t (diff);
            actions.Push (action);
            inputMode = inputMode_t.WEAPON;
            Debug.Log ("added action: ");
            Debug.Log (diff.ToString ());
        }
        update_tiles();
        return true;
    }

    public static bool cancel_input()
    // Triggered when right mouse button is clicked
    // If there are inputs to cancel,
    // cancel it by changing the current action or poping the action from the stack,
    // revert inputMode by consult the weapon and return true;
    // Otherwise, do nothing and return false.
    {
        // Basically this function erases your last input
        // and reverts the state of the inputManager to before you made the input
        // Basically if you call add_input() followed by cancel_input() nothing should change.
        // So you should be able to figure out what to put in here
        // after you finish add_input()
        return false;
    }

    public static void test()
    {
        Debug.Log ("This is a test");

        tileMode_t newMode = new tileMode_t ();
        set_mode (inputMode_t.MOVE);
        //newMode.isValidMove = true;

        //tiles [0, 0].update_tile_mode (newMode);  
    }
}
