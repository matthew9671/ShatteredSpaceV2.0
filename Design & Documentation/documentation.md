Types:

struct action
    Vector2 movement
    Vector2 spMovement
    Vector2 attack
    int wpnId

class game_t
    global (or something...) PLAYER_CNT = 2
    board_t board
    
    void execute_turn()
        Execute one time step at a time until both players run out of actions, then execute end_turn.
    
    void step()
        Execute one time step of the game. 
        step(fire weapons)->generate damage(create damage objects)->move players->update board(check collisions)

    void move_players(players, actions)
        Move the players and deal with solid collisions recursively. Calls update board when finished.

    void end_turn()
        Generate all remaining damage, clear the action stacks again and then call end_turn on all objects on the board.

class board_t
    List<object_t> objects
    List<object_t>[] board
    Dict<List<damage_t>, int> pending // Should use a priority queue but it's too much trouble

    bool is_in_board(Vector2 pos)
        Determines if the given position is in the board.

    bool is_free(Vector2 pos)
        Determines if the given position is not occupied by any solid object.

    object_t get_blocked(Vector2 pos1, Vector2 pos2)
        Returns the first solid object that blocks the ray from pos1 to pos2. Returns false if no such objects exist.

    bool create_damage(Vector2 pos, damage_t dmg, int delay)
        Insert the dmg object into the pending with delay as key. Return false if the dmg is not generated.

    bool remove_object(object_t object)
        Remove object from the board. Returns false if the object doesn't exist.

    bool put_object(Vector2 pos, object_t object)
        Put object on the board at pos. Returns false if pos is invalid.

    void update_damage()
        Generate damage objects scheduled for the time step.

    void check_collisions()
        Update the board, check collisions for all objects on the board.

    List<player_t> get_players()
        Returns a list of players arranged by playerId.

    void print_board()
        Print out all the objects on the board for debugging purposes.

class object_t
    string name
    bool solid
    int life
    Vector2 pos

    void on_collision(other)
        Triggered when some other object collides with it (whether or not it is solid). This may lead to inefficiency.

    void step(board_t board)
        Update the object by one timestep.

    void end_turn()
        Decrement the life of the object. Remove object when life goes to 0.

class unit_t : object_t
    int hp

    bool shoot(Vector2 pos, weapon_t weapon, board_t board)
        Fire the weapon at a position on the board. Returns false if this is an invalid attack.

    bool get_hit(damage_t dmg)
        Hit the unit with dmg. Calls take_damage. Returns false if the unit is destroyed.

    bool take_damage(int amount)
        Take damage specified by amount. Returns false if the unit is destroyed.

class player_t : unit_t
    const int WEAPONSLOTS = 4
    int playerId
        This is the id given at the start of game which is unique for each player and identical across clients.
    List<weapon_t> weapons
        The list can have more than 4 elements, but only the first 4 can be used in the same turn. The rest of the weapons has to be single-moduled.
    Stack<action> actions
    int[] upgrades

    void refresh_weapons()
        Call refresh() on all of the weapons.

    action get_action(board_t board)
        Take a peek of the stack and return it. If the stack is empty then return an action with stationary movement.

    void gain_exp(int amount)
        Gain experience points equal to amount.

    void set_actions(Stack<action> actions)
        Set the action stack of the player to be actions.

class turret_t : unit_t
    int reward
    weapon_t weapon

class damage_t : object_t
    int value
    string effect
    object_t creator

class weapon_t
    (All kinds of weapons are its subclass)
    bool defensive
    List<int> modules
        0 momentum 1 explosive 2 particle 3 field 4 overheated particle
    int fireCount

    void refresh()
        Refresh the weapon so that it can fire again in the next turn.

    bool fire_at(Vector2 pos, board_t board, unit_t master)
        Create damage objects on the board, return false if this is an invalid attack. May have some special effects on the master.
