#define DEBUG
using System;
//using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

// This is where we define the global constants for testing 
// and other convenient stuff
public class SS
{
    public const int TECH_COUNT = 4;
    public const int MOMENTUM = 0;
    public const int EXPLOSIVE = 1;
    public const int PARTICLE = 2;
    public const int FIELD = 3;
    public const int PARTICLE_OVERHEATED = 4;
    public const bool DBG = true;
    public const bool VERBOSE = false;
    // This is the randomized seed for all random events in the game
    // Created by one client and shared across all clients
    // So that the outcomes are identical across clients
    public const int RND_SEED = 42;
    public static readonly Vector2[] DIRECTIONS =
        {new Vector2(1, 0),
         new Vector2(-1, 0),
         new Vector2(0, 1),
         new Vector2(0, -1),
         new Vector2(1, -1),
         new Vector2(-1, 1)
        };
    // Only output if verbosity is turned on
    public static void dbg_log(string message)
    {
        if (VERBOSE) Console.WriteLine(message);
    }

    public static int distance(Vector2 v1, Vector2 v2)
    // Returns the length of shortest path in tiles from v1 to v2
    // On a hexagonal board, ofcourse.
    { 
        int x2 = (int)(v2.x - v1.x);
        int y2 = (int)(v2.y - v1.y);
        int z = 0 - x2 - y2;
        int dist = Math.Abs(x2) + Math.Abs(y2) + Math.Abs(z);
        dist = dist/2;
        return dist;
    }
}

// From stackoverflow
// Fixes Debug.Assert so that it prints the stack frame 
// and terminates the program
public class DumpStackTraceListener : TraceListener
{
  public override void Write( string message )
  {
     Console.Write( message );
  }

  public override void WriteLine(string message)
  {
     Console.WriteLine( message );
  }

  public override void Fail(string message)
  {
     Fail( message, String.Empty );
  }

  public override void Fail(string message1, string message2)
  {
     if (null == message2)
        message2 = String.Empty;

     Console.WriteLine( "{0}: {1}", message1, message2 );
     Console.WriteLine( "Stack Trace:" );

     StackTrace trace = new StackTrace( true );
     foreach (StackFrame frame in trace.GetFrames())
     {
        MethodBase frameClass = frame.GetMethod();
        Console.WriteLine( "  {2}.{3} {0}:{1}", 
                           frame.GetFileName(),
                           frame.GetFileLineNumber(),
                           frameClass.DeclaringType,
                           frameClass.Name );
     }

    #if DEBUG
     Console.WriteLine( "Exiting because Fail" );
     Environment.Exit( 1 );
    #endif
  }
}

// This is defined in Unity but we're not running the prgram in Unity for now
public class Vector2
{
    public int x;
    public int y;
    public static readonly Vector2 zero = new Vector2(0,0);

    public Vector2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Vector2 operator +(Vector2 v1, Vector2 v2) 
    {
        return new Vector2(v1.x + v2.x, v1.y + v2.y);
    }

    public static Vector2 operator -(Vector2 v1, Vector2 v2) 
    {
        return new Vector2(v1.x - v2.x, v1.y - v2.y);
    }

    public static Vector2 operator *(Vector2 v1, int c) 
    {
        return new Vector2(v1.x * c, v1.y * c);
    }

    public static Vector2 operator *(int c, Vector2 v1) 
    {
        return new Vector2(v1.x * c, v1.y * c);
    }

    public static bool operator ==(Vector2 v1, Vector2 v2) 
    {
        if ((System.Object)v2 == null)
        {
            return (System.Object)v1 == null;
        }
        return (v1.x == v2.x) && (v1.y == v2.y);
    }

    public static bool operator !=(Vector2 v1, Vector2 v2) 
    { 
        return !(v1 == v2);
    }

    public string ToString()
    {
        return "(" + this.x + ", " + this.y + ")";
    }
}

// Something we need in order for Vector2 to work
public class vecComp : IEqualityComparer<Vector2>
{
    public int GetHashCode(Vector2 v)
    {
        return v.x * 10 + v.y;
    }

    public bool Equals(Vector2 v1, Vector2 v2)
    {
        return v1 == v2;
    }
}

public class attack_t
{
    public Vector2 target;
    public attack_t(Vector2 target)
    {
        this.target = target;
    }
}

// An action is a single step that a player executes
public struct action_t
{
    public Vector2 movement;
    // Special movement caused by recoil or combustion thruster
    public Vector2 spMovement;
    public attack_t attack;
    public Vector2 target;
    public int wpnId;

    public action_t(Vector2 movement)
    {
        this.movement = movement;
        this.spMovement = Vector2.zero;
        this.target = Vector2.zero;
        this.attack = new attack_t(Vector2.zero);
        this.wpnId = -1;
    }
}

// An abstract instance of a game of shattered space
public static class game_t
{   
    // Execute one time step at a time until both players run out of actions, 
    // then execute end_turn.
    // The input comes from user input
    public static void execute_turn(board_t board, List<Stack<action_t>> actions)
    {
        int playerCount = board.playerCount;
        List<player_t> players = board.get_players();
        for (int i = 0; i < playerCount; i++)
        {
            players[i].set_actions(actions[i]);
        }
        // Execute the actions one step at a time
        while(!turn_finished(players))
        {
            SS.dbg_log("Executing step...");
            execute_step(board);
            board.print_board();
        }
        // Do end of turn stuff
        end_turn(board);
    }
    
    static bool turn_finished(List<player_t> players)
    // Returns true if all players run out of actions
    // and all remaining pending damage has non-positive delay
    {
        int playerCount = players.Count;
        // Check player actions
        for (int i = 0; i < playerCount; i++)
        {
            if (!players[i].no_moves_left())
            {
                return false;
            }
        }
        // Check pending damage
        // Or not?
        return true;
    }
        
    static List<Vector2>[,] execute_step(board_t board)
    // Execute one time step of the game.  
    // Pop off an action from each player;
    // all players fire their weapons;
    // move the players and deal with solid collisions recursively. 
    // Calls update board when finished.
    {
        int playerCount = board.playerCount;
        List<player_t> players = board.get_players();
        Vector2[] spvs = new Vector2[playerCount];
        Vector2[] vs = new Vector2[playerCount];
        List<Vector2>[,] result = new List<Vector2>[playerCount,2];
        // Calculate special and normal movements separately
        // with the same helper
        for (int i = 0; i < playerCount; i++)
        {
            action_t action = players[i].pop_action();
            spvs[i] = action.spMovement;
            vs[i] = action.movement;
            // Every player fire their weapons
            // Note that some weapons aren't "fired" in here
            // They just gets turned on and continue to take effect
            players[i].attack(action.attack, action.wpnId, board);
        }
        // Move the players
        SS.dbg_log("Doing special movement...");
        List<Vector2>[] spMovements = move_players(board, players, spvs);
        SS.dbg_log("Doing normal movement...");
        List<Vector2>[] movements = move_players(board, players, vs);
        for (int i = 0; i < playerCount; i++)
        {
            result[i,0] = spMovements[i];
            result[i,1] = movements[i];
        }
        // Update the board, deal with collisions
        // This is also where all the objects gets updated
        board.step_update();
        return result;
    }

    static List<Vector2>[] move_players(board_t board,
        List<player_t> players, Vector2[] vs)
    // Move the players around with vs as their initial velocities
    // This is more complicated than it seems
    // Since we have to take care of collisions in a sensible and fun way
    // For two and potentially more players
    {
        int playerCount = board.playerCount;
        List<Vector2>[] result = new List<Vector2>[playerCount];
        // A map from positions on the board to a list of player indices
        Dictionary<Vector2, List<int>> posToPlayer = 
            new Dictionary<Vector2, List<int>>(new vecComp());
        bool isMoving = true;
        // Temporarily remove all players from the board
        for (int i = 0; i < playerCount; i++)
        {
            board.remove_object(players[i]);
        }
        while(isMoving)
        {
            posToPlayer.Clear();
            // Add all the velocities to the result
            for (int i = 0; i < playerCount; i++)
            {
                if (result[i] == null)
                {
                    result[i] = new List<Vector2>();
                }
                // We only record non-zero movements for now
                // Because it's easier to process
                if (vs[i] != Vector2.zero)
                {
                    SS.dbg_log("Adding velocity: "+ vs[i].ToString());
                    result[i].Add(vs[i]);
                }
            }
            // First check for special case where two players switch positions
            for (int i = 0; i < playerCount; i++)
            {
                for (int j = i + 1; j < playerCount; j++)
                {
                    Vector2 pos1 = players[i].get_pos();
                    Vector2 pos2 = players[j].get_pos();
                    Vector2 v1 = vs[i];
                    Vector2 v2 = vs[j];
                    if ((pos1 + v1 == pos2) && (pos2 + v2 == pos1))
                    {
                        // We caught our special case!
                        // Add additional movements to undo the movements
                        result[i].Add(-1 * vs[i]);
                        result[j].Add(-1 * vs[j]);
                        vs[i] = Vector2.zero;
                        vs[j] = Vector2.zero;
                        SS.dbg_log("We have a special case!");
                    }
                }
            }
            // Build the map from position to player
            // We have to assume that the original positions
            // don't have two stationary players
            // occupying the same tile
            // Since that may generate a infinite loop and 
            // cause the game to crash
            for (int i = 0; i < playerCount; i++)
            {
                Vector2 pos;
                pos = players[i].get_pos();
                pos += vs[i];
                if (posToPlayer.ContainsKey(pos))
                {
                    posToPlayer[pos].Add(i);
                }
                else
                {
                    List<int> newValue = new List<int>();
                    newValue.Add(i);
                    posToPlayer.Add(pos, newValue);
                }
                // Record the new position in the player object
                players[i].set_pos(pos);
            }
            // Solve for the velocities in the next round of collision checking
            foreach (Vector2 pos in posToPlayer.Keys)
            {
                List<int> pIds = posToPlayer[pos];
                Debug.Assert(pIds != null);
                Debug.Assert(pIds.Count != 0);
                if (pIds.Count == 1)
                {
                    SS.dbg_log("Player " + pIds[0].ToString() + " at position: "
                        + pos.ToString());
                    int pId = pIds[0];
                    // Check board
                    if (board.is_free(pos))
                    {
                        // Valid movement for now!
                        vs[pId] = Vector2.zero;                    
                    }
                    else
                    {
                        // Reverse the velocity
                        vs[pId] = -1 * vs[pId];
                    }
                }
                else if (pIds.Count == 2)
                // Special case: if there are exactly two players,
                // and one is stationary, the moving one will nudge the other
                // away.
                {
                    SS.dbg_log("2 players at position");
                    int pId1 = pIds[0];
                    int pId2 = pIds[1];
                    if (vs[pId1] == Vector2.zero)
                    {
                        Debug.Assert(vs[pId2] != Vector2.zero);
                        vs[pId1] = vs[pId2];
                        vs[pId2] = Vector2.zero;
                    }
                    else if(vs[pId2] == Vector2.zero)
                    {
                        Debug.Assert(vs[pId1] != Vector2.zero);
                        vs[pId2] = vs[pId1];
                        vs[pId1] = Vector2.zero;
                    }
                    else
                    {
                        // Neither is valid
                        // Reverse the velocity
                        vs[pId1] = -1 * vs[pId1];
                        vs[pId2] = -1 * vs[pId2];
                    }
                }
                else
                {
                    SS.dbg_log("Multiple players at position");
                    // All of the movements are invalid
                    foreach(int pId in pIds)
                    {
                        vs[pId] = -1 * vs[pId];
                    }
                }
            }
            // Check if any one of the players is still moving
            isMoving = false;
            for(int i = 0; i < playerCount; i++)
            {
                if (vs[i] != Vector2.zero)
                {
                    isMoving = true;
                    break;
                }
            }
        }
        // Put all players back to the board
        for (int i = 0; i < playerCount; i++)
        {
            Vector2 pos = players[i].get_pos();
            Debug.Assert(board.is_free(pos));
            board.put_object(pos, players[i]);
        }
        return result;
    }
        
    // Generate all remaining damage, clear the action stacks again 
    // then call end_turn on all objects on the board.
    static void end_turn(board_t board)
    {
        if (SS.DBG) Console.WriteLine("End of turn...");
        // First generate all end-of-turn damage
        board.step_update();
        board.clear_pending_damage();
        board.print_board();
        board.step_update();
        // Attempt to move the players again
        // since there might be blast wave effects that pushes players around
        while(!turn_finished(board.get_players()))
        {
            //if (SS.DBG) Console.WriteLine("Executing step...");
            execute_step(board);
            board.print_board();
        }
	   board.turn_update();
    }
} 

// An opaque interface of the game board
public class board_t
{
    int mapW;
    int mapH;
    int centerRow;
    int centerCol;
    int mapId;
    public int playerCount;
    List<object_t> objects;
    List<object_t>[,] board;
    // Should use a priority queue but it's too much trouble
    List<damage_t> pending;
    List<object_t> objsToRemove = new List<object_t>();
    // For debugging
    const string player_symbol = "O";
    const string turret_symbol = "T";
    const string tile_symbol = ".";
    const string damage_symbol = "*";
    const string other_symbol = "?";

    public board_t(int mapId, int playerCount)
    {
        this.playerCount = playerCount;
        this.mapId = mapId;
        this.init();
    }

    // Initialize map with mapId
    // We need to store the map data somewhere...
    // but I haven't figured out how
    void init()
    {
        // Right now mapId doesn't do anything
        // We are always generating a hexagonal board for testing
        mapW = 13;
        mapH = 13;
        board = new List<object_t>[mapH,mapW];
        objects = new List<object_t>();
        pending = new List<damage_t>();

        centerRow = mapH / 2;
        centerCol = mapW / 2;
        // Initialize the board
        // Null is not a valid position (out of the map)
        // Empty list represents an empty tile
        for (int i = 0; i < mapH; i++)
        {
            for (int j = 0; j < mapW; j++)
            {
                // The board is just a parallelogram with two corners removed
                if (i >= j - centerCol && i - centerRow <= j)
                {
                    // We are in the hexagon
                    board[i,j] = new List<object_t>();
                }
                else
                {
                    // It's always a good habit to initialize an array
                    board[i,j] = null;
                }
            }
        } 
        // Add the players
        // Hard coding FTW!!!!
        player_t p1 = new player_t();
        Vector2 pos = new Vector2(3, 0);
        p1.playerId = 0;
        put_object(pos, p1);
        if (playerCount == 2)
        {
            player_t p2 = new player_t();
            pos = new Vector2(-3, 0);
            p2.playerId = 1;
            put_object(pos, p2);
        }
    }

    public bool is_in_board(Vector2 pos)
    //Determines if the given position is in the board.
    {
        return get_tile(pos) != null;
    }

    public bool is_free(Vector2 pos)
    //Determines if the given position is not occupied by any solid object.
    {
        List<object_t> tile = get_tile(pos);
        if (tile == null)
        {
            return false;
        }
        else
        {
            foreach(object_t obj in tile)
            {
                if (obj.solid) return false;
            }
            return true;
        }
    }

    object_t get_blocked(Vector2 pos1, Vector2 pos2)
    // Returns the first solid object that blocks the ray from pos1 to pos2. 
    // Returns null if no such objects exist.
    {
        Debug.Assert(false);
        return null;
    }

    public void create_damage(damage_t dmg)
    // Insert the dmg object into the pending list. 
    // Return false if the dmg is not generated.
    {
        this.pending.Add(dmg);
    }

    public bool remove_object(object_t obj)
    // Remove object from the board. 
    // Returns false if the object doesn't exist.
    {
        // Remove the object from the object list
        if(!objects.Remove(obj))
        {
            return false;
        }
        // Remove the object from the board
        List<object_t> tile = this.get_tile(obj.get_pos());
        if (tile == null)
        {
            return false;
        }
        return tile.Remove(obj);
    }

    List<object_t> get_tile(Vector2 pos)
    // Returns the a list of objects at position pos on the board
    {
        int row = centerRow - pos.y;
        int col = centerCol + pos.x;
        // Return null if out of bounds
        if (row < 0 || col < 0 || row >= mapH || col >= mapW)
        {
            return null;
        }
        else
        {
            return board[row, col];
        }
    }

    public bool put_object(Vector2 pos, object_t obj)
    // Put object on the board at pos. Returns false if pos is invalid.
    {
        List<object_t> tile = get_tile(pos);
        // If pos is invalid or the object already exists
        if (tile == null || objects.Find(x => x == obj) != null)
        {
            return false;
        }  
        else
        {
            obj.set_pos(pos);
            tile.Add(obj);
            objects.Add(obj);
            return true;
        }
    }

    public void remove_later(object_t obj)
    // Add obj to toRemove
    {
        objsToRemove.Add(obj);
    }

    public void step_update()
    // Update the board and all the objects in it
    // update all objects->update_damage->check_collisions
    {
        // Update all objects
        foreach (object_t obj in this.objects)
        {
            obj.step_update(this);
        }
        // Note that sometimes damage objects are created in the update function
        // So it is necessary that update_damage comes after the update.
        this.update_damage();
        this.check_collisions();
        this.remove_destroyed();
        this.empty_remove_list();
    }

    public void remove_destroyed()
    // Remove all unit_t with hp <= 0
    // and trigger all their dying effects
    {
        foreach (object_t obj in this.objects)
        {
            if (obj is unit_t)
            {
                // Cast the object into a unit object
                unit_t unit = obj as unit_t;
                if (unit.get_hp() <= 0)
                {
                    unit.on_destroyed(this);
                    // Add the unit object to the to remove list
                    remove_later(unit);
                }
            }
        }
    }
    
    public void turn_update()
    // Update the board and all the objects in it
    // Call end_turn on all objects on the board
    {
        // Update all objects
        foreach (object_t obj in this.objects)
        {
            obj.end_turn(this);
        }
    }

    void empty_remove_list()
    // Empty toRemove and remove every object in it from the board
    {
        foreach(object_t obj in objsToRemove)
        {
            this.remove_object(obj);
        }
        objsToRemove.Clear();
    }

    public void clear_pending_damage()
    // This only happens at end of turn
    // And, despite the name suggests, it doesn't necessarily clear
    // the pending damage list
    // since some attacks take effect over two turns
    {
        List<damage_t> toRemove = new List<damage_t>();
        foreach(damage_t dmg in pending)
        {
            // It has to be end-of-turn damage
            //Debug.Assert(dmg.delay < 0);
            // TODO: Later we should add a condition here
            // Specifically to implement the homing missile (or something...)
            if (true){
                put_object(dmg.get_pos(), dmg);
                toRemove.Add(dmg);
            }
        }
        // Clear the toRemove list from the pending damage list
        foreach (damage_t dmg in toRemove)
        {
            pending.Remove(dmg);
        }
    }

    void update_damage()
    // Generate damage objects scheduled for the time step.
    // Does not call update on the damage objects
    {
        // If the delay is 0, create it
        // else, decrease the delay by one
        List<damage_t> toRemove = new List<damage_t>();
        foreach (damage_t dmg in pending)
        {
            if (dmg.delay == 0)
            {
                put_object(dmg.get_pos(), dmg);
                toRemove.Add(dmg);
            }
            else
            {
                dmg.delay -= 1;
            }
        }
        // Clear the toRemove list from the pending damage list
        foreach (damage_t dmg in toRemove)
        {
            pending.Remove(dmg);
        }
    }

    public bool has_pending_damage()
    // Returns true if any pending damage has non-negative delay
    {
        foreach (damage_t dmg in pending)
        {
            if (dmg.delay >= 0) return true;
        }
        return false;
    }

    void check_collisions()
    // Update the board, check collisions for all objects on the board.
    // This only works when one of the two objects is solid
    // The merging of damage/energy is a special case
    // that should be taken care of somewhere else
    // So in practice this method only applies to getting hit by damage
    // and picking up energy
    {
        foreach (List<object_t> tile in board)
        {
            if (tile != null)
            {
                int n = tile.Count;
                for (int i = 0; i < n; i++)
                {
                    for (int j = i + 1; j < n; j++)
                    {
                        // Call on_collision on both objects
                        // This could lead to problems due to 
                        // the order of execution
                        tile[i].on_collision(tile[j]);
                        tile[j].on_collision(tile[i]);
                    }
                }
            }
        }
    }

    public List<player_t> get_players()
    //Returns a list of players ordered by playerId.
    {
        List<object_t> player_objs = objects.FindAll(x => x is player_t);
        List<player_t> players = new List<player_t>();
        // Convert all the "object_t*" to "player_t*"
        for (int i = 0; i < playerCount; i++)
        {
            players.Add(player_objs[i] as player_t);
        }
        Debug.Assert(players.Count == playerCount);
        // Sort the players by playerId
        players.Sort((x, y) => x.playerId.CompareTo(y.playerId));
        return players;
    }

    public void print_board()
    //Print out all the objects on the board for debugging purposes.
    {
        // Only print under debug mode
        if (!SS.DBG) 
        {
            return;
        }
        for (int i = 0; i < mapH; i++)
        {
            // Print offset spaces
            for (int k = 0; k < mapH - i - 1; k++)
            {
                Console.Write(" ");
            }
            for (int j = 0; j < mapW; j++)
            {
                List<object_t> tile = board[i, j];
                // Print the contents of the tile
                if (tile != null)
                {
                    if (tile.Count == 0)
                    {
                        Console.Write(tile_symbol);
                    }
                    else if (tile.Count == 1)
                    {
                        object_t obj = tile[0];
                        if (obj is player_t)
                        {
                            Console.Write(player_symbol);
                        }
                        else if (obj is turret_t)
                        {
                            Console.Write(turret_symbol);
                        }
                        else if (obj is damage_t)
                        {
                            Console.Write(damage_symbol);
                        }
                        else
                        {
                            Console.Write(other_symbol);
                        }
                    }
                    else
                    {
                        // If there are multiple objects 
                        // we print the number of objects
                        Console.Write(tile.Count.ToString());
                    }
                    Console.Write(" ");
                }
                else
                {
                    Console.Write("  ");
                }
            }
            Console.Write("\n");
        } 
    }
}

public class object_t
{
    // The object does not render and interact with other objects 
    // if this is false
    public bool exists = true;
    public string name;
    public bool solid;
    public int stepLife;
    public int turnLife;
    // Position is too important to be public
    Vector2 position;

    public object_t(string name, bool solid)
    {
        this.name = name;
        this.solid = solid;
    }

    public virtual void on_collision(object_t other){}
    // Triggered when some other object collides with it 
    // (One of the two has to be solid). This may lead to inefficiency.

    public virtual void step_update(board_t board)
    // Update the object by one timestep.
    {
        this.stepLife -= 1;
        if (this.stepLife == 0)
        {
            // Remove from board
            // Removing the object at once will result in an error
            // since we are still looping through the list of objects
            board.remove_later(this);
        }
    }

    public virtual void end_turn(board_t board)
    // Decrement the turnLife of the object. 
    // Remove object when turnLife goes to 0.
    {
        this.turnLife -= 1;
        if (this.turnLife == 0)
        {
            // Remove from board
            // Removing the object at once will result in an error
            // since we are still looping through the list of objects
            board.remove_later(this);
        }
    }

    public Vector2 get_pos()
    // The only way to access the position of an object
    {
        if (SS.DBG && this.position == null)
        {
            Console.WriteLine(
                "Error: Uninitialized position for object " + this.name);
            System.Environment.Exit(1);
        }
        return this.position;
    }

    public void set_pos(Vector2 pos)
    // Only meant to be called by board_t
    {
        this.position = pos;
    }
}

// A unit object is one that has a health bar and can interact with
// damage objects
public class unit_t : object_t
{
    int hp;
    public List<weapon_t> weapons = new List<weapon_t>();

    public unit_t(string name, int hp):base(name, solid:true)
    {
        this.hp = hp;   
    }

    public virtual bool attack(attack_t attack, int wpnId, board_t board)
    // Fire the weapon with the parameters specified in attack 
    // Returns false if this is an invalid attack.
    {
        //Debug.Assert(pos != null);
        if (wpnId < 0 || wpnId >= weapons.Count)
        {
            return false;
        }
        weapons[wpnId].fire(attack, board, this);
        return true;
    }

    public bool get_hit(damage_t dmg)
    // Only called by the dmg object
    // Hit the unit with dmg. Calls take_damage. 
    // Returns false if the unit is destroyed.
    {
        int amount = dmg.amount;
        return take_damage(amount);
    }

    public virtual bool take_damage(int amount)
    // Take damage specified by amount. 
    // Returns false if the unit is destroyed.
    {
        SS.dbg_log(this.name + " takes " + amount.ToString() + " damage!");
        this.hp -= amount;
        return this.hp > 0;
    }

    public virtual void on_destroyed(board_t board)
    // Do something when the unit is destroyed
    // Like deathrattle in Hearthstone
    {}

    public int get_hp()
    // Get the hit points of the unit object
    {
        return hp;
    }
}

public class player_t : unit_t
{
    const int WEAPONSLOTS = 4;
    const int starting_hp = 10;
    // This is the id given at the start of game which is unique for each player 
    // and identical across clients.
    public int playerId;
    Stack<action_t> actions;
    int[] upgrades;
    // The free weapon modules the player now has
    // 0 momentum 1 explosive 2 particle 3 field 4 overheated particle
    // modules[0] stores the number of free momentum modules and so on
    // For testing purposes we have lots of every module
    int[] modules = {10, 10, 10, 10, 10};

    public player_t():base(name:"Player", hp:starting_hp)
    {
        actions = new Stack<action_t>();
        // Temporary
        // TODO: Change this
        this.weapons.Add(new blaster_t());
    }

    void refresh_weapons()
    //Call refresh() on all of the weapons.
    {
        Debug.Assert(false);
    }

    public action_t pop_action()
    // Pop an element off the stack and return it. 
    // If the stack is empty then return an action with stationary movement.
    {
        Debug.Assert(actions != null);
        if (actions.Count == 0)
        {
            SS.dbg_log("No actions left!");
            return new action_t(Vector2.zero);
        }
        else
        {
            return actions.Pop();
        }
    }

    public bool no_moves_left()
    // Returns true if the player has no moves left
    // returns false otherwise
    {
        return actions.Count == 0;
    }

    void gain_exp(int amount)
    // Gain experience points equal to amount.
    {
    }

    public bool build_weapon(weapon_t weapon)
    // Build weapon and add it to the weapons list
    // Returns false if the player's missing some required modules
    // Returns true otherwise
    {
        // Make a copy of modules since an overheated particle module 
        // is still a particle module
        int[] temp_modules = new int[SS.TECH_COUNT];
        Array.Copy(this.modules, temp_modules, SS.TECH_COUNT);
        //temp_modules[SS.PARTICLE] += modules[SS.PARTICLE_OVERHEATED];
        for (int i = 0; i < SS.TECH_COUNT; i++)
        {
            if(weapon.modules[i] > temp_modules[i])
            {
                return false;
            }
        }
        // We can build the weapon
        // ...so build it!
        // Remove the modules from the list of free modules
        for (int i = 0; i < SS.TECH_COUNT; i++)
        {
            modules[i] -= weapon.modules[i];
        }
        // Add the weapon to the list
        // TODO: This should actually be a copy
        this.weapons.Add(weapon);
        return true;
    }

    public void set_actions(Stack<action_t> actions)
    // Set the action stack of the player to be actions.
    {
        this.actions = actions;
    }

    public void push_action(action_t action)
    // Push action onto the action stack
    // Should only be called by a blast_wave_t object
    {
        this.actions.Push(action);
    }
}

public class turret_t : unit_t
{
    const int hp = 10;
    int reward;
    weapon_t weapon;
    public turret_t():base("Turret", hp){}
}

public class damage_t : object_t
{
    public int amount;
    string effect;
    object_t creator;
    public int delay;
    public damage_t():base("dmg", false){}

    public void set_params(int amount, int delay)
    // Set the damage amount and delay of the damage object
    {
        this.amount = amount;
        this.delay = delay;
    }

    public override void on_collision(object_t other)
    // Calls get_hit on the other object 
    {
        if (other is unit_t)
        {
            unit_t target = other as unit_t;
            target.get_hit(this);
            // Maybe remove the damage object from the board?
        }
    }
}

// A special kind of damage that pushes players away
public class blastWave_t : damage_t
{
    Vector2 direction;
    public blastWave_t(Vector2 direction):base()
    {
        this.direction = direction;
    }

    public override void on_collision(object_t other)
    // Calls get_hit on the other object 
    {
        base.on_collision(other);
        if (other is player_t)
        {
            player_t player = other as player_t;
            action_t action = new action_t(direction);
            player.push_action(action);
        }
    }
}

public class weapon_t
{
    // All kinds of weapons are its subclasses
    bool defensive;
    // 0 momentum 1 explosive 2 particle 3 field (4 overheated particle ?)
    // The build cost of the weapon
    // Doesn't change once the weapon is created
    public int[] modules;
    int fireCount;
    // Any delay <0 would let the damage be generated at end of turn
    int delay_base;
    int damage_base;
    int range_base;

    public weapon_t(int range, int damage, int delay, int[] modules)
    {
        this.range_base = range;
        this.damage_base = damage;
        this.delay_base = delay;
        this.modules = modules;
    }

    // #########################################################################
    // User interface related methods
    public virtual void refresh()
    // Refresh the weapon so that it can fire again in the next turn.
    {}

    public virtual inputMode_t generate_action(action_t action, 
        Vector2 playerPos, Vector2 mousePos, inputMode_t inputMode)
    // Change the action based on user input and return the next inputMode
    {
        // This is the most general case
        // So we assume that the attack is not generated
        // And we are not doing a special movement
        Debug.Assert(action.attack == null);
        Debug.Assert(inputMode == inputMode_t.ATTACK);
        // Add the attack to the action
        action.attack = new attack_t(mousePos);
        return inputMode_t.MOVE;
    }

    public virtual tileMode_t get_tile_mode(Vector2 tilePos, Vector2 playerPos, 
        Vector2 mousePos, inputMode_t inputMode, board_t board, unit_t master)
    // Returns the tile mode of the tile at tilePos
    // Generally speaking, when inputMode is ATTACK: 
    // tile.isOutOfRange = true if it is out of range from playerPos;
    // is validAttack if it is in range and have the mouse over it.
    {
        Debug.Assert(inputMode == inputMode_t.ATTACK);
        tileMode_t result = new tileMode_t();
        if (!is_in_range(playerPos, tilePos, master))
        {
            result.isOutOfRange = true;
        }
        else if (mousePos == tilePos)
        {
            result.isValidTarget = true;
        }
        return result;
    }

    public virtual bool is_in_range(Vector2 playerPos, Vector2 targetPos,
        unit_t master)
    // Returns true if targetPos is within attack range from playerPos
    // Ignoring obstacles.
    {
        int d = SS.distance(playerPos, targetPos);
        return (d > 0) && (d <= get_range(master));
    }

    public virtual int get_range(unit_t master)
    // Returns the weapon's attack range
    // usually it is base range plus something depending on the upgrades
    {
        return range_base;
    }
    // #########################################################################

    public virtual int get_damage_amount(unit_t master)
    // Returns the weapon's damage amount
    // usually it is base damage plus something depending on the upgrades
    {
        return damage_base;
    }

    public virtual int get_delay(unit_t master)
    // Returns the weapon's delay
    // usually it is just the base delay but it might change 
    // according to the upgrades
    {
        return delay_base;
    }

    public virtual bool fire(attack_t attack, board_t board, unit_t master)
    // Create pending damage on the board, 
    // return false if this is an invalid attack. 
    // May have some special effects on the master.
    {
        Debug.Assert(attack != null);
        Vector2 pos = attack.target;
        int amount = get_damage_amount(master);
        int delay = get_delay(master);
        damage_t dmg = new damage_t();
        dmg.set_params(amount, delay);
        dmg.set_pos(pos);
        dmg.stepLife = 1;
        board.create_damage(dmg);
        return board.is_in_board(pos);
    }
}

// The most basic weapon
// There should be a weaker version of it that only uses one momentum module
public class blaster_t : weapon_t
{
    static int[] modules = {2, 0, 0, 0};
    public blaster_t():base(range:5, damage:5, delay:1, modules:modules)
    {}
}

// The basic weapon explosive (bomb-based) weapon
// There should be a weaker version of it that only uses one explosive module
public class grenadeLauncher_t : weapon_t
{
    static int[] modules = {0, 2, 0, 0};
    const int SPLASH_DAMAGE = 2;
    public grenadeLauncher_t():base(range:5, damage:4, delay:-1, modules:modules)
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
        return base.fire(attack, board, master);
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

// UI Stuff
// After we fully transition to Unity we will move these to gameManager.cs
public enum inputMode_t {NONE, ATTACK, MOVE, SPMOVE, WEAPON};

public struct tileMode_t
{
    // True if we know some damage with positive amount is going to hit the tile in the future
    public bool isDangerous;
    // Meaningful only when isDangerous is true
    // Usually -1, 0 or 1
    // If it's -1, the damage falls at end of turn
    // 0 means that the damage is on the tile right now
    // 1 means that the damage will be put on the tile a step later
    public int stepsToDamage;
    public bool isOutOfRange;
    public bool isValidMove;
    public bool isValidTarget;
}