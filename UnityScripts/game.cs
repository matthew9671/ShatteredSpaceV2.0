#define DEBUG
// In theory we should be able to just turn it off while not using unity
// But unfortunately we are already working with gameObjects so 
// there's no turning back
// Still, kind of nice that I learned to do this trick
#define UNITY
#if UNITY
using UnityEngine;
#endif
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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

	// Animation constants
	public const int spMoveFrames = 15;
	public const int movePause = 5;
	public const int moveFrames = 20;
	public const float spMoveFactor = 0.1f;
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

    public static Vector3 board_to_world(Vector2 v)
    {
        Vector3 i = Vector3.right * gameManager_t.GM.spacing;
		Vector3 j = Vector3.up * gameManager_t.GM.spacing * Mathf.Sqrt (3) / 2
			+ Vector3.right * gameManager_t.GM.spacing / 2;
        return v.x * i + v.y * j;
    }
}

/// <summary>
/// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
/// Provides a method for performing a deep copy of an object.
/// Binary Serialization is used to perform the copy.
/// </summary>
public static class objectCopier
{
	/// <summary>
	/// Perform a deep Copy of the object.
	/// </summary>
	/// <typeparam name="T">The type of object being copied.</typeparam>
	/// <param name="source">The object instance to copy.</param>
	/// <returns>The copied object.</returns>
	public static T clone<T>(T source)
	{
		if (!typeof(T).IsSerializable)
		{
			if (source is GameObject) {
				return source;
			} else {
				throw new ArgumentException ("The type must be serializable.", "source");
			}
		}

		// Don't serialize a null object, simply return the default for that object
		if (ReferenceEquals(source, null))
		{
			return default(T);
		}

		IFormatter formatter = new BinaryFormatter();
		Stream stream = new MemoryStream();
		// 2. Construct a SurrogateSelector object
		SurrogateSelector ss = new SurrogateSelector();

		Vector2SerializationSurrogate v2ss = new Vector2SerializationSurrogate();
		ss.AddSurrogate(typeof(Vector2),
			new StreamingContext(StreamingContextStates.All),
			v2ss);

		// 5. Have the formatter use our surrogate selector
		formatter.SurrogateSelector = ss;
		using (stream)
		{
			formatter.Serialize(stream, source);
			stream.Seek(0, SeekOrigin.Begin);
			return (T)formatter.Deserialize(stream);
		}
	}
}

sealed class Vector2SerializationSurrogate : ISerializationSurrogate {

	// Method called to serialize a Vector2 object
	public void GetObjectData(System.Object obj,
		SerializationInfo info, StreamingContext context) {

		Vector2 v2 = (Vector2) obj;
		info.AddValue("x", v2.x);
		info.AddValue("y", v2.y);
		//Debug.Log(v2);
	}

	// Method called to deserialize a Vector2 object
	public System.Object SetObjectData(System.Object obj,
		SerializationInfo info, StreamingContext context,
		ISurrogateSelector selector) {

		Vector2 v2 = (Vector2) obj;
		v2.x = (float)info.GetValue("x", typeof(float));
		v2.y = (float)info.GetValue("y", typeof(float));
		obj = v2;
		return obj;   // Formatters ignore this return value //Seems to have been fixed!
	}
}

#if !UNITY
// From stackoverflow
// Fixes System.Diagnostics.Debug.Assert so that it prints the stack frame 
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

//Something we need in order for Vector2 to work
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
#endif

[Serializable]
public class attack_t
{
    public Vector2 target;
    public attack_t(Vector2 target)
    {
        this.target = target;
    }
}

// An action is a single step that a player executes
[Serializable]
public class action_t
{
    public Vector2 movement;
    // Special movement caused by recoil or combustion thruster
    public Vector2 spMovement;
    public attack_t attack;
    public Vector2 target;
    public int wpnId;

    public action_t()
    {
        this.movement = Vector2.zero;
        this.spMovement = Vector2.zero;
        this.target = Vector2.zero;
        this.attack = new attack_t(Vector2.zero);
        this.wpnId = -1;
    }

	public bool is_default()
	// Returns true if the action is "do nothing"
	{
		return (movement == Vector2.zero && spMovement == Vector2.zero && wpnId == -1);
	}
}

// An abstract instance of a game of shattered space
public static class game_t
{
    // Execute one time step at a time until both players run out of actions, 
    // then execute end_turn.
    // The input comes from user input
    public static void execute_turn(board_t board, List<List<action_t>> actions)
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
        
	public static void execute_step(board_t board)
    // Execute one time step of the game.  
    // Get an action from each unit;
    // all units fire their weapons;
    // move the units and deal with solid collisions recursively. 
    // Calls update board when finished.
    {
		// Execute the three components separately
		execute_movement(board);
		execute_attack(board);
		execute_sp_movement(board);
		resolve_step(board);
		// Things that happens during a real execution of the game
		List<unit_t> units = board.get_units();
		int unitCount = units.Count;
		for (int i = 0; i < unitCount; i++)
		{
			List<animation_t> halt = new List<animation_t>();
			halt.Add(animation_t.HALT);
			gameManager_t.GM.send_animation(halt, units[i].objectId);
		}
    }

	public static void resolve_step(board_t board)
	// Pop the actions of every unit
	// And update the board
	{
		List<unit_t> units = board.get_units();
		int unitCount = units.Count;
		for (int i = 0; i < unitCount; i++)
		{
			// Get rid of the action that has been executed
			units[i].pop_action();
		}
		// Update the board, deal with collisions
		// This is also where all the objects gets updated
		step_update(board);
	}

	public static void execute_movement(board_t board)
	// Execute only the movement part in one time step of the game
	{
		List<unit_t> units = board.get_units();
		List<Vector2> startPos = new List<Vector2>();
		int unitCount = units.Count;
		Vector2[] vs = new Vector2[unitCount];
		for (int i = 0; i < unitCount; i++)
		{
			startPos.Add(units[i].get_pos());
			action_t action = units[i].peek_action();
			vs[i] = action.movement;
		}
		// Move the units
		SS.dbg_log("Doing normal movement...");
		List<Vector2>[] movements = move_units(board, units, vs);
		// Add the movement animations
		for (int i = 0; i < unitCount; i++)
		{
			units[i].add_movement_animation(startPos[i], movements[i]);
		}
	}

	public static bool in_battle(board_t board)
	// Returns true if any units on the board apart from the player(s) takes a non-default action
	{
		List<unit_t> units = board.get_units();
		int unitCount = units.Count;
		for (int i = 0; i < unitCount; i++)
		{
			action_t action = units[i].peek_action();
			if (!(units[i] is player_t || action.is_default())) return true;
		}
		return false;
	}

	public static void execute_attack(board_t board)
	// Execute only the attack part in one time step of the game
	{
		List<unit_t> units = board.get_units();
		int unitCount = units.Count;
		for (int i = 0; i < unitCount; i++)
		{
			action_t action = units[i].peek_action();
			animation_t attackAnimation = units[i].attack(action.attack, action.wpnId, board);
			units [i].add_attack_animation (attackAnimation);
		}
	}

	public static void execute_sp_movement(board_t board)
	// Execute only the special movement part in one time step of the game
	{
		List<unit_t> units = board.get_units();
		List<Vector2> startPos = new List<Vector2>();
		int unitCount = units.Count;
		Vector2[] spvs = new Vector2[unitCount];
		for (int i = 0; i < unitCount; i++)
		{
			startPos.Add(units[i].get_pos());
			action_t action = units[i].peek_action();
			spvs[i] = action.spMovement;
		}
		// Move the units
		SS.dbg_log("Doing special movement...");
		List<Vector2>[] spMovements = move_units(board, units, spvs);
		// Add the movement animations
		for (int i = 0; i < unitCount; i++)
		{
			//units[i].add_movement_animation(startPos[i], spMovements[i], spFlag:true);
		}
	}

    // This is where time moves forward 1 step
    // Lots of things can be happening at the same time
    // So the order of the function calls is extremely important
    static void step_update(board_t board)
    {
        board.step_update();
        // Remove all the things that ran out of steplife
        board.empty_object_lists();
        // Note that sometimes damage objects are created in the update function
        // So it is necessary that update_damage comes after the update.
        board.update_damage();
        // Every time when new damage is generated
        // We repeat the process of checking collision 
        // and maybe creating more damage
        // Think of a chain explosion of oil cans or something
        bool dmgGenerated = true;
        while (dmgGenerated)
        {
            board.check_collisions();
            board.remove_destroyed();
            dmgGenerated = board.empty_object_lists();
        }  
    }

    static List<Vector2>[] move_units(board_t board,
        List<unit_t> units, Vector2[] vs)
    // Move the units around with vs as their initial velocities
    // This is more complicated than it seems
    // Since we have to take care of collisions in a sensible and fun way
    // For two and potentially more units
    {
        int unitCount = units.Count;
        List<Vector2>[] result = new List<Vector2>[unitCount];
        // A map from positions on the board to a list of player indices

        #if UNITY

        Dictionary<Vector2, List<int>> posToUnit = 
            new Dictionary<Vector2, List<int>>();

        #else

        Dictionary<Vector2, List<int>> posToUnit = 
            new Dictionary<Vector2, List<int>>(new vecComp());

        #endif

        bool isMoving = true;
        // Temporarily remove all units from the board
        for (int i = 0; i < unitCount; i++)
        {
            board.remove_object(units[i]);
        }
        while(isMoving)
        {
            posToUnit.Clear();
            // Add all the velocities to the result
            for (int i = 0; i < unitCount; i++)
            {
                if (result[i] == null)
                {
                    result[i] = new List<Vector2>();
                }
                // We only record non-zero movements for now
                // Because it's easier to process
                SS.dbg_log("Adding velocity: "+ vs[i].ToString());
                result[i].Add(vs[i]);
            }
            // First check for special case where two units switch positions
            for (int i = 0; i < unitCount; i++)
            {
                for (int j = i + 1; j < unitCount; j++)
                {
                    Vector2 pos1 = units[i].get_pos();
                    Vector2 pos2 = units[j].get_pos();
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
            // don't have two stationary units
            // occupying the same tile
            // Since that may generate a infinite loop and 
            // cause the game to crash
            for (int i = 0; i < unitCount; i++)
            {
                Vector2 pos;
                pos = units[i].get_pos();
                pos += vs[i];
                if (posToUnit.ContainsKey(pos))
                {
                    posToUnit[pos].Add(i);
                }
                else
                {
                    List<int> newValue = new List<int>();
                    newValue.Add(i);
                    posToUnit.Add(pos, newValue);
                }
                // Record the new position in the player object
                units[i].set_pos(pos);
            }
            // Solve for the velocities in the next round of collision checking
            foreach (Vector2 pos in posToUnit.Keys)
            {
                List<int> pIds = posToUnit[pos];
                System.Diagnostics.Debug.Assert(pIds != null);
                System.Diagnostics.Debug.Assert(pIds.Count != 0);
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
                // Special case: if there are exactly two units,
                // and one is stationary, the moving one will nudge the other
                // away.
                {
                    SS.dbg_log("2 units at position");
                    int pId1 = pIds[0];
                    int pId2 = pIds[1];
                    if (vs[pId1] == Vector2.zero)
                    {
                        System.Diagnostics.Debug.Assert(vs[pId2] != Vector2.zero);
                        vs[pId1] = vs[pId2];
                        vs[pId2] = Vector2.zero;
                    }
                    else if(vs[pId2] == Vector2.zero)
                    {
                        System.Diagnostics.Debug.Assert(vs[pId1] != Vector2.zero);
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
                    SS.dbg_log("Multiple units at position");
                    // All of the movements are invalid
                    foreach(int pId in pIds)
                    {
                        vs[pId] = -1 * vs[pId];
                    }
                }
            }
            // Check if any one of the units is still moving
            isMoving = false;
            for(int i = 0; i < unitCount; i++)
            {
                if (vs[i] != Vector2.zero)
                {
                    isMoving = true;
                    break;
                }
            }
        }
        // Put all units back to the board
        for (int i = 0; i < unitCount; i++)
        {
            Vector2 pos = units[i].get_pos();
            System.Diagnostics.Debug.Assert(board.is_free(pos));
            board.put_object(pos, units[i]);
        }
        return result;
    }
        
    // Generate all remaining damage, clear the action stacks again 
    // then call end_turn on all objects on the board.
    static void end_turn(board_t board)
    {
        if (SS.DBG) Console.WriteLine("End of turn...");
        // First generate all end-of-turn damage
        board.clear_pending_damage();
        // Some special weapons generate dmg at every step including eot
        // So we have to step_update as well
        step_update(board);
        board.print_board();
        // Attempt to move the players again
        // since there might be blast wave effects that pushes players around
        while(!turn_finished(board.get_players()))
        {
            //if (SS.DBG) Console.WriteLine("Executing step...");
            execute_step(board);
            board.print_board();
        }
        if (SS.DBG) Console.WriteLine("Turn ended!");
        board.turn_update();
        // Many objects destory themselves at end of turn!
        board.empty_object_lists();
        board.print_board();
    }
} 

// An opaque interface of the game board
[Serializable]
public class board_t
{
    public int mapW;
    public int mapH;
    public int centerRow;
    public int centerCol;
    int mapId;
    public int playerCount;
    List<object_t> objects;
    List<object_t>[,] board;
    // Should use a priority queue but it's too much trouble
    List<damage_t> pending;
    List<object_t> objsToRemove = new List<object_t>();
    List<object_t> objsToPut = new List<object_t>();
    List<Vector2> posToPut = new List<Vector2>();
    // For debugging
    const string player_symbol = "O";
    const string turret_symbol = "T";
    const string tile_symbol = ".";
    const string damage_symbol = "*";
    const string other_symbol = "?";

	public board_t(int mapId=-1, int playerCount=1, int mapW=0, int mapH=0, bool[,] tileLayout=null, List<object_t> objs=null)
    {
        this.playerCount = playerCount;
        this.mapId = mapId;
		if (mapId == 0)
		{
        	this.init();
		}
		else
		{
			this.init_with_params(playerCount:playerCount, mapW:mapW, mapH:mapH, tileLayout:tileLayout, objs:objs);
		}
    }

	void init_with_params(int playerCount, int mapW, int mapH, bool[,] tileLayout, List<object_t> objs)
	{
		board = new List<object_t>[mapH,mapW];
		objects = new List<object_t>();
		pending = new List<damage_t>();

		this.mapW = mapW;
		this.mapH = mapH;

		centerRow = mapH / 2;
		centerCol = mapW / 2;
		// Initialize the board
		// Null is not a valid position (out of the map)
		// Empty list represents an empty tile
		for (int i = 0; i < mapH; i++)
		{
			for (int j = 0; j < mapW; j++)
			{
				if (tileLayout[i,j])
				{
					// We are in the board
					board[i,j] = new List<object_t>();
				}
				else
				{
					// We are in empty space
					board[i,j] = null;
				}
			}
		} 
		foreach(object_t obj in objs)
		{
			put_object(obj.get_pos(), obj);
		}
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
		// Instantiate turrets and other units
		put_object(new Vector2(0, 3), new turret_t());
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
                if (obj.solid && obj.exists) return false;
            }
            return true;
        }
    }

    public object_t get_blocked(Vector2 pos1, Vector2 pos2)
    // Returns the first solid object that blocks the ray from pos1 to pos2,
    // NOT counting the objects at pos1 or pos2
    // Returns null if no such objects exist.
    // TODO: Implement this!
    {
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
        int row = centerRow - (int)pos.y;
        int col = centerCol + (int)pos.x;
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

	public static Vector2 array_to_pos(int row, int col, int mapH, int mapW)
	// Returns the correponding board position given the row and column number
	{
		int centerRow = mapH / 2;
		int centerCol = mapW / 2;
		return new Vector2(col - centerCol, centerRow - row);
	}

    public bool put_object(Vector2 pos, object_t obj)
    // Put object on the board at pos. Returns false if pos is invalid.
    {
        List<object_t> tile = get_tile(pos);
        // If pos is invalid or the object already exists
        if (tile == null || objects.Find(x => x == obj) != null)
        {
			UnityEngine.Debug.Log("false");
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

    public void put_later(Vector2 pos, object_t obj)
    {
        posToPut.Add(pos);
        objsToPut.Add(obj);
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
    }

    public void remove_destroyed()
    // Remove all unit_t with hp <= 0
    // and trigger all their dying effects
    {
        foreach (object_t obj in this.objects)
        {
            if (obj is unit_t && obj.exists)
            {
                // Cast the object into a unit object
                unit_t unit = obj as unit_t;
                if (unit.get_hp() <= 0)
                {
                    // Testing only
                    // If one player dies, exit the program
                    if (unit is player_t) System.Diagnostics.Debug.Assert(false);
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

    public bool empty_object_lists()
    // Empty toRemove and remove every object in it from the board
    // Returns true if any damage is generated
    {
        // Remove objects
        foreach(object_t obj in objsToRemove)
        {
            this.remove_object(obj);
        }
        objsToRemove.Clear();
        // Put objects
        bool dmgGenerated = false;
        for(int i = 0; i < objsToPut.Count; i++)
        {
            this.put_object(posToPut[i], objsToPut[i]);
            if (objsToPut[i] is damage_t) dmgGenerated = true;
        }
        objsToPut.Clear();
        posToPut.Clear();
        return dmgGenerated;
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
            //System.Diagnostics.Debug.Assert(dmg.delay < 0);
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

    public void update_damage()
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

    public void check_collisions()
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
        System.Diagnostics.Debug.Assert(players.Count == playerCount);
        // Sort the players by playerId
        players.Sort((x, y) => x.playerId.CompareTo(y.playerId));
        return players;
    }

    public List<unit_t> get_units()
    //Returns a list of units ordered arbitrarily.
    {
        return objects.FindAll(x => x is unit_t).ConvertAll<unit_t>(x => x as unit_t);
    }

	public List<object_t> get_objects()
	//Returns a list of units ordered arbitrarily.
	{
		return objects;
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

    public void set_dangerous(Vector2 tilePos, tileMode_t tileMode)
    {
        return;
    }
}

[Serializable]
public class object_t
{
	// A number that corresponds to a animation controller and gameobject
	public int objectId;
    // The object does not render and interact with other objects 
    // if this is false
    public bool exists = true;
    public string name;
    public bool solid;
    // An object by default lives forever
    public int stepLife = -1;
    public int turnLife = -1;
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

	public virtual GameObject get_model()
	{
		return null;
	}
}

// A unit object is one that has a health bar and can interact with
// damage objects
[Serializable]
public class unit_t : object_t
{
    protected int hp;
    // protected = only visible to classes that inherit this, 
    // but not to the outside world
    protected List<action_t> actions = new List<action_t>();
    public List<weapon_t> weapons = new List<weapon_t>();

    public unit_t(string name, int hp):base(name, solid:true)
    {
        this.hp = hp;   
    }

    public weapon_t get_weapon(int wpnId)
    // Returns the [wpnId]'s weapon in the unit's weapon list
    // If there is no such weapon, return null
    {
        if (wpnId < 0 || wpnId >= weapons.Count)
        {
            return null;
        }
        else
        {
            return weapons[wpnId];
        }
    }

	public virtual animation_t attack(attack_t attack, int wpnId, board_t board)
    // Fire the weapon with the parameters specified in attack 
    // Returns false if this is an invalid attack.
    {
        //System.Diagnostics.Debug.Assert(pos != null);
        if (wpnId < 0 || wpnId >= weapons.Count)
        {
			return animation_t.DO_NOTHING;
        }
		return weapons[wpnId].fire(attack, board, this);
    }

    public bool get_hit(damage_t dmg)
    // Only called by the dmg object
    // Hit the unit with dmg. Calls take_damage. 
    // Returns false if the unit is destroyed.
    {
        int amount = dmg.amount;
        bool alive = take_damage(amount);
		add_hit_animation(dmg);
        return alive;
    }

    public virtual bool take_damage(int amount)
    // Take damage specified by amount. 
    // Returns false if the unit is destroyed.
    {
        Console.WriteLine(this.name + " takes " + amount.ToString() + " damage!");
        this.hp -= amount;
        Console.WriteLine(this.name + " has " + this.hp.ToString() + " hp!");
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

	public virtual action_t peek_action()
	// Returns an action from the action LIST of the unit without removing it
	{
		if (actions.Count == 0) return new action_t();
		action_t result = pop_action ();
		actions.Insert (0, result);
		return result;
	}

    public virtual action_t pop_action()
    // Removes and returns an action from the action LIST of the unit
    {
        System.Diagnostics.Debug.Assert(actions != null);
        int len = actions.Count;
        if (len == 0)
        {
            //SS.dbg_log("No actions left!");
            return new action_t();
        }
        else
        {
            action_t result = actions[0];
            actions.RemoveAt(0);
            return result;
        }
    }

	// --------------------------
	// Methods related to visuals
	// --------------------------

	public virtual void add_movement_animation(Vector2 start, List<Vector2> vs, bool spFlag = false)
    {
		List<animation_t> animSequence = new List<animation_t>();
		int n = vs.Count;
		int frames = gameManager_t.stepFrames;
		if (spFlag) frames = (int)(frames * SS.spMoveFactor);
		for (int i = 0; i < n; i++) 
		{
			animSequence.Add (unitAnimation_t.pool.get_move_animation(vs[i] + start, frames, spFlag));
			start += vs[i];
		}
		gameManager_t.GM.send_animation(animSequence, objectId);
    }

	public virtual void add_hit_animation(damage_t dmg)
	{
		List<animation_t> animSequence = new List<animation_t>();
		animSequence.Add (unitAnimation_t.pool.get_hit_animation(dmg));
		gameManager_t.GM.send_animation(animSequence, objectId);
	}

	public virtual void add_attack_animation(animation_t attackAnimation)
	{
		List<animation_t> animSequence = new List<animation_t>();
		animSequence.Insert (0, attackAnimation);
		gameManager_t.GM.send_animation(animSequence, objectId);
	}
}

[Serializable]
public class player_t : unit_t
{
    const int WEAPONSLOTS = 4;
    const int starting_hp = 10;
    // This is the id given at the start of game which is unique for each player 
    // and identical across clients.
    public int playerId;
    int[] upgrades;
    // The free weapon modules the player now has
    // 0 momentum 1 explosive 2 particle 3 field 4 overheated particle
    // modules[0] stores the number of free momentum modules and so on
    // For testing purposes we have lots of every module
    int[] modules = {10, 10, 10, 10, 10};

    public player_t():base(name:"Player", hp:starting_hp)
    {
        // Temporary
        // TODO: Change this
        this.weapons.Add(new blaster_t());
    }

    void refresh_weapons()
    //Call refresh() on all of the weapons.
    {
        System.Diagnostics.Debug.Assert(false);
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
        this.weapons.Add(weapon);
        return true;
    }

    public void set_actions(List<action_t> actions)
    // Set the action stack of the player to be actions.
    {
        this.actions = actions;
    }

    public void add_action(action_t action)
    // Add action onto the action list
    {
		this.actions.Insert(0, action);
    }
	public override void add_movement_animation(Vector2 start, List<Vector2> vs, bool spFlag = false)
	{
		List<animation_t> animSequence = new List<animation_t>();
		int n = vs.Count;
		int frames = gameManager_t.stepFrames;
		if (spFlag) frames = (int)(frames * SS.spMoveFactor);
		for (int i = 0; i < n; i++) 
		{
			animSequence.Add (playerAnimation_t.pool.get_move_animation(vs[i] + start, frames, spFlag));
			start += vs[i];
		}
		gameManager_t.GM.send_animation(animSequence, objectId);
	}

	public override void add_attack_animation(animation_t attackAnimation)
	{
		List<animation_t> animSequence = new List<animation_t>();
		animSequence.Insert (0, attackAnimation);
		gameManager_t.GM.send_animation(animSequence, objectId);
	}

	public override GameObject get_model ()
	{
		return playerAnimation_t.pool.playerModel;
	}
}

[Serializable]
public class turret_t : unit_t
{
    new const int hp = 10;
    int reward;
    weapon_t weapon;

    public turret_t():base("Turret", hp)
    {
        // The turretGun is a weakened version of the blaster
        weapon = new turretGun_t();
        this.weapons.Add(weapon);
    }

    public override void step_update(board_t board)
    // At the each step, the turret locks onto the nearest player
    // And fires its weapon at a random position 
    // within distance 1 of that player
    {
        List<player_t> players = board.get_players();
        // A flag varible indicating if any player is in range
        bool playerInRange = false;
        int bestDistance = 0;
        int playersInTie = 0;
        Vector2 bestPos = Vector2.zero;
        Vector2 pos = this.get_pos();
        Vector2 playerPos;
        foreach (player_t player in players)
        {
            playerPos = player.get_pos();
            if (weapon.is_in_range(pos, playerPos, this, board))
            {
                int distance = SS.distance(pos, playerPos);
                if (playerInRange)
                {
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestPos = playerPos;
                        playersInTie = 0;
                    }
                    else if (distance == bestDistance)
                    {
                        // Since random.shuffle doesn't exist in C#
                        // we have to do some smart random things
                        // to make sure each player get targeted 
                        // with the same probability
                        playersInTie += 1;
                        System.Random rnd = new System.Random();
                        int randNum = rnd.Next(playersInTie);
                        if (randNum == 0)
                        {
                            bestDistance = distance;
                            bestPos = playerPos;
                        }
                    }
                }
                else
                {
                    bestDistance = distance;
                    bestPos = playerPos;
                    playerInRange = true;
                }
            }
        }
        if (playerInRange)
        {
            action_t action = new action_t();
            action.attack = new attack_t(bestPos);
            action.wpnId = 0;
            actions.Insert(0, action); 
        }
        
    }
		
	public override GameObject get_model()
	{
		return unitAnimation_t.pool.turret1;
	}
}

[Serializable]
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