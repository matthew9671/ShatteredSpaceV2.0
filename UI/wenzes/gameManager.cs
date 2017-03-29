// Shi Wenze's version of the gameManager_t
// Will eventually be combined with others' versions
// This would only work in Unity

public struct tileMode_t
{
    // True if we know some damage with positive amount is going to hit the tile in the future
    bool isDangerous;
    // Meaningful only when isDangerous is true
    // Usually -1, 0 or 1
    // If it's -1, the damage falls at end of turn
    // 0 means that the damage is on the tile right now
    // 1 means that the damage will be put on the tile a step later
    int stepsToDamage;
    bool isOutOfRange;
    bool isValidMove;
    bool isValidTarget;
}

public class tile_t : MonoBehavior
{
    tileMode_t mode;
    Vector2 position;
    
    // ######################################################
    // Unity methods
    void Update()
    {

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
    // Updates the tile's appearence according to the current mode
    {

    }
}

public static class gameManager_t
{
    // Input is a component in the action: weaponId, attack, movement or spMovement
    // inputMode tells us which kind of input we want from the user right now
    enum inputMode_t {ATTACK, MOVE, SPMOVE, WEAPON};
    public static inputMode_t inputMode;
    // The temporary board is a duplicate board object with only one player
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

    // Set functions
    public static void set_mode(tileMode_t mode)
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

    }

    public static void init_planning()
    // Called when a turn ends and the planning phase starts
    // Creates a temporary duplicate of the player and board, 
    // removes the opponent from the temporary board
    {

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
    }

    public bool add_input()
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
        // if in ATTACK or SPMOVE, pass the current action, player position and mouseClick
        // position to the generate_action method in the weapon
    }

    public bool cancel_input()
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
    }
}