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
// public enum inputMode_t {FINISHED, ATTACK, MOVE, SPMOVE, WEAPON};

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
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

// UI Stuff
// After we fully transition to Unity we will move these to gameManager.cs
public enum inputMode_t {FINISHED, ATTACK, MOVE, SPMOVE, WEAPON, IN_TURN, FREE};
//public delegate void animation_t(GameObject obj);

public class animation_t
{
	protected Action<GameObject> play;
	protected Action stop;
	public int delay;
	public int duration;
	public string name;
	public bool isPlaying;
	// If an animation is active, then it plays when time moves forward and stops when time stops 
	public bool isActive;

	public static animation_t DO_NOTHING = new animation_t("Do nothing", delegate(GameObject obj){}, delegate(){}, 0, 0);
	public static animation_t HALT = new animation_t("Halt", delegate(GameObject obj)
	{
		obj.GetComponent<animController_t>().stop();
		gameManager_t.GM.animation_halted();
	}, delegate(){}, 0, 0);

	public animation_t(string name, Action<GameObject> play, Action stop, int delay, int duration)
	{
		this.name = name;
		this.play = play;
		this.stop = stop;
		this.delay = delay;
		this.duration = duration;
		isPlaying = false;
		isActive = false;
	}

	public virtual void play_animation(GameObject obj)
	{
		//		UnityEngine.Debug.Log("Playing animation!");
		//		UnityEngine.Debug.Assert(!isPlaying);
		if (!isPlaying)
		{
			isPlaying = true;
			play(obj);
		}
	}

	public virtual void stop_animation()
	{
		//		UnityEngine.Debug.Assert(isPlaying);
		isPlaying = false;
		stop();
	}

	public virtual void destroy()
	{
		stop_animation();
		return;
	}

	public virtual animation_t copy()
	{
		animation_t result = new animation_t(name, play, stop, delay, duration);
		result.isActive = isActive;
		return result;
	}

	public virtual void set_active(bool b)
	{}
}

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

	public static bool operator ==(tileMode_t t1, tileMode_t t2)
	{
		return (t1.isDangerous == t2.isDangerous &&
			t1.stepsToDamage == t2.stepsToDamage &&
			t1.isOutOfRange == t2.isOutOfRange &&
			t1.isValidMove == t2.isValidMove &&
			t1.isValidTarget == t2.isValidTarget);
	}

	public static bool operator !=(tileMode_t t1, tileMode_t t2)
	{
		return !(t1.isDangerous == t2.isDangerous &&
			t1.stepsToDamage == t2.stepsToDamage &&
			t1.isOutOfRange == t2.isOutOfRange &&
			t1.isValidMove == t2.isValidMove &&
			t1.isValidTarget == t2.isValidTarget);
	}
}

public class tile_t : MonoBehaviour
{
    tileMode_t mode;
    public Vector2 position;
	Renderer rend;
	bool flashing = false;
	bool rightHold = false;

	Color flashingColor = Color.green;
	Color validMoveColor = Color.green;
	Color validAtkColor = Color.red;
	Color outOfRangeColor = Color.gray;
    
    // ######################################################
    // Unity methods
	void Awake()
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
		gameManager_t.GM.mouse_enter(position);
    }

    void OnMouseExit()
    {
		gameManager_t.GM.mouse_exit();
    }

    void OnMouseUp()
    // Triggered when the user releases the left mouse button
    // Doesn't work on the right mouse button
    {
		gameManager_t.GM.add_input();
    }

	void OnMouseOver(){
		if (Input.GetButton ("Fire2")) {   // Detect right MouseUp event manually
			rightHold = true;
		} else {
			if(rightHold){
				gameManager_t.GM.cancel_input ();
				rightHold = false;
			}
		}
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

	void OnGUI() {
		return;
		Texture2D tex = gameManager_t.GM.testTexture;
		Vector3 guiPosition = Camera.main.WorldToScreenPoint(transform.position);
		guiPosition.y = Screen.height - guiPosition.y;
		Rect rect = new Rect(guiPosition.x - tex.width/2f, guiPosition.y - tex.height/2f, tex.width, tex.height);
		GUI.DrawTexture(rect, tex);
	}
}

public class gameManager_t : MonoBehaviour
{
	// Black magic learned from
	//http://answers.unity3d.com/questions/323195/how-can-i-have-a-static-class-i-can-access-from-an.html
	public static gameManager_t GM;

	// -------------------
	// Important constants
	// -------------------
	// The temporary board is a duplicate board object with only one player
	public int playerCount = 1;
	// The playerId of the player on this client
	// Set to 0 for convenience of testing
	public int playerId = 0;
	// The spacing of the gameTiles
	public float spacing = 1f;
	// The maximum number of steps per turn
	// Notice that the player is unable to move in the first step
	// And that when the player doesn't attack the player gets an extra step (not implemented yet)
	// TODO: Implement the extra step mechanic
	public int stepsPerTurn = 3;

    // Input is a component in the action: weaponId, attack, movement or spMovement
    // inputMode tells us which kind of input we want from the user right now
    // This line is commented out because it is defined in game.cs
    // enum inputMode_t {NONE, ATTACK, MOVE, SPMOVE, WEAPON};
    public inputMode_t inputMode;

	public Transform boardHolder;
    public tile_t[,] tiles;
	// The one and only real virtual board
    public board_t board;
	public Stack<board_t> tempBoards;
    // The player is also a temporary duplicate
    public player_t tempPlayer;
    // The position of the TILE that the mouse is currently on
    // NOT the position of the mouse on the screen!
    public Vector2 mousePos;
	// The default value for mousePos
	public Vector2 outOfBoard;
    // The action stack of the player we are generating
    public Stack<action_t> actions;
	public int stepNumber;

	// Prefabs linked in unity editor
	public GameObject gameTile;
    // UI stuff linked in unity editor
    public List<Button> playerBtns;

	// For testing
	Text output;
	public int testNum = 42;

	// Animation stuff
	public Stack<List<animController_t>> animControllers = new Stack<List<animController_t>>();
	public int haltedControllers = 0;
	public const float timeScale = 5f;
	public const int stepFrames = 20;

	// UI stuff
	public List<GameObject> attackNodes = new List<GameObject>();
	public List<GameObject> movementBars = new List<GameObject>();
	public GameObject attackSpline;
	public Texture2D testTexture;
	public Button playButton;

	// Level stuff
	public TextAsset[] levelInfos;
	level_t currLevel;

	// Black magic
	void Awake()
	{
		if (GM != null) 
		{
			GameObject.Destroy (GM);
		} 
		else 
		{
			GM = this;
		}
		DontDestroyOnLoad(this);
	}
		
	//---------------
	// Initialization
	//---------------

    public void init_game()
    // Called when the game starts
    {
		load_level(0);
	}

	void reset_scene()
	{
		tiles = null;
		stop_animation();
		animControllers.Clear();
	}

	public void init_board(int mapW, int mapH, bool[,] tileLayout, List<object_t> objs)
	// Called by scene_t
	{
		// Init the virtual board
		board = new board_t(mapW:mapW, mapH:mapH, tileLayout:tileLayout, objs:objs);
		// Init the physical board
		if (boardHolder == null) boardHolder = new GameObject ("Board").transform;
        // Generate the physical board
		tiles = new tile_t[board.mapH, board.mapW];
		for (int row = 0; row < board.mapH; row++) {
			for (int col = 0; col < board.mapW; col++) {
				int boardX = col - board.centerCol;
				int boardY = board.centerRow - row;
				if (board.is_in_board (new Vector2 (boardX, boardY))) {
					float x = boardX * spacing + boardY * 0.5f * spacing;
					float y = boardY * spacing * Mathf.Sqrt (3) / 2;
					GameObject instance = GameObject.Instantiate (gameTile, new Vector3 (x, y, 0), Quaternion.identity) as GameObject;
					tile_t tile = instance.AddComponent<tile_t> ();
					tile.position = new Vector2 (boardX, boardY);
					tiles [row, col] = tile;
					instance.transform.SetParent (boardHolder);
				}
			}
		}
		// TODO: Change this to generate all units
        // Generate objects on the physical board
		objs = board.get_objects();
		List<animController_t> animCtrlList = new List<animController_t> ();
		foreach (object_t obj in objs)
        {
            GameObject instance = GameObject.Instantiate 
				(obj.get_model(), SS.board_to_world(obj.get_pos()), Quaternion.identity) as GameObject;
			if (obj is unit_t)
			// Currently only units can have animations
			{
				animController_t aCtrl = instance.AddComponent<animController_t> ();
				obj.objectId = animCtrlList.Count;
				aCtrl.objectId = animCtrlList.Count;
				animCtrlList.Add (aCtrl);

			}
			instance.transform.SetParent (boardHolder);
		}
		animControllers.Push (animCtrlList);
		// Initialize the UI elements
		outOfBoard = new Vector2(board.centerCol + 1, board.centerRow + 1);
		tempPlayer = board.get_players()[playerId];
    }

	void init_UI()
	// Initialize the UI elements
	{
		for (int i = 0; i < stepsPerTurn; i++)
		{
			attackNodes.Add(GameObject.Find("Attack Node " + i.ToString()));
			movementBars.Add(GameObject.Find("Movement Bar " + i.ToString()));
		}
		attackNodes.Add(GameObject.Find("Attack Node EOT"));
	}

    public void init_planning()
    // Called when a turn ends and the planning phase starts
    // Creates a temporary duplicate of the player and board, 
    // removes the opponent from the temporary board
    {
		Debug.Log("Init planning!");
		// For testing
		if (!tempPlayer.weapons.Exists(x => x is shockCannon_t)) tempPlayer.build_weapon (new shockCannon_t ());
		if (!tempPlayer.weapons.Exists(x => x is recoilCannon_t)) tempPlayer.build_weapon (new recoilCannon_t ());
		init_UI();
		tempBoards = new Stack<board_t>();
        // Get the copy of the board
		board_t tempBoard = objectCopier.clone<board_t>(board);
		tempBoards.Push (tempBoard);
		actions = new Stack<action_t>();
		actions.Push(new action_t());
		stepNumber = 0;
		mousePos = outOfBoard;
		foreach (animController_t anim in animControllers.Peek())
		{
			anim.reset();
		}
		// Planning always starts with choosing a weapon
		inputMode = inputMode_t.WEAPON;
		// Shift the camera to look cool
		StartCoroutine(rotate_camera(Quaternion.identity));
    }

	//-----------------
	// Called by tile_t
	//-----------------

    public void mouse_exit()
    // Called by tile_t
    // Triggers when the mouse exits a tile
    {
		// Set mouse position to outside of the board
		// There might be a better way to do this
		set_mouse_position(outOfBoard);
		// Hide the attack spline
		attackSpline.GetComponent<SplineDecorator>().show_curve(false);
    }
		
    public void mouse_enter(Vector2 tilePos)
    // Called by tile_t
    // Triggers when the mouse first enters that tile
    {
        set_mouse_position(tilePos);
    }

    void update_tiles()
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
        // Also updates the player menu
        int numBtns = playerBtns.Count;
        for (int i = 0; i < numBtns; i++)
        {
            if (inputMode == inputMode_t.WEAPON)
            {
				// If the button corresponds to a weapon
                if (i < numBtns - 1)
                {
					weapon_t weapon = tempPlayer.get_weapon (i);
					if (weapon == null)
                    {
						playerBtns[i].interactable = false;
                    }
                    else
                    {
						if (weapon.can_be_fired()) 
						{
							playerBtns[i].interactable = true;
						} 
						else 
						{
							playerBtns[i].interactable = false;
						}
                    } 
                }
                else
                {
                    // The last button is the "no attack" button
                    playerBtns[i].interactable = true;
                }
            }
            else
            {
                playerBtns[i].interactable = false;
            }
        }
    }

    tileMode_t get_tile_mode(Vector2 tilePos, Vector2 playerPos)
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
		board_t tempBoard = get_board();

		// Should use switch instead...
		switch (inputMode)
		{
		case inputMode_t.WEAPON:
			tile.isOutOfRange = true;
			tile.isValidMove = false;
			tile.isValidTarget = false;
			break;
		case inputMode_t.MOVE:
			if (SS.distance(tilePos, playerPos) <= 1){
				tile.isOutOfRange = false;
			}
			else tile.isOutOfRange = true;
			tile.isValidMove = false;
			if (mousePos == tilePos && !tile.isOutOfRange){
				tile.isValidMove = true;
			}
			tile.isValidTarget = false;
			break;
		case inputMode_t.FREE:
			if (SS.distance(tilePos, playerPos) <= 1){
				tile.isOutOfRange = false;
			}
			else tile.isOutOfRange = true;
			tile.isValidMove = false;
			if (mousePos == tilePos && !tile.isOutOfRange){
				tile.isValidMove = true;
			}
			tile.isValidTarget = false;
			break;
		case inputMode_t.FINISHED:
			tile.isOutOfRange = true;
			tile.isValidMove = false;
			tile.isValidTarget = false;
			break;
		case inputMode_t.IN_TURN:
			tile.isOutOfRange = false;
			tile.isValidMove = false;
			tile.isValidTarget = false;
			break;
		default:
			weapon_t weapon = tempPlayer.get_weapon(actions.Peek().wpnId);
			// ATTACK or SPMOVE
			tile = weapon.get_tile_mode(tilePos, playerPos, mousePos, inputMode,
				tempBoard, tempPlayer);
			break;
		}
		tempBoard.set_dangerous(tilePos, tile);
		return tile;
    }

	//-------------------------
	// Add input & cancel input
	//-------------------------

    public bool add_input()
    // Triggered when a tile is clicked on or when the player weapon menu is pressed
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
//		Debug.Log ("Add input!");
//		print_mode (inputMode);
		tileMode_t cur;
		inputMode_t newMode = new inputMode_t();
		// The current action we are editing
		action_t action;
		if (actions != null) 
		{
			action = actions.Peek();
		}
		else
		{
			action = new action_t();
		}
		weapon_t weapon;
		switch (inputMode)
		{
			case inputMode_t.FINISHED:
				return false;
			case inputMode_t.IN_TURN:
				return false;
			case inputMode_t.FREE:
				cur = get_tile_mode(mousePos, tempPlayer.get_pos());
				if (!cur.isValidMove) return false;
				
				Vector2 diff = mousePos - tempPlayer.get_pos (); 
				action = new action_t ();
				action.movement = diff;

				tempPlayer.add_action (action);
				bool enteringBattle = game_t.in_battle(board);
				// Execute a step without making a copy of the board
				game_t.execute_step (board);
				// Let animations play
				play_animation();
				// First set mode to IN_TURN
				// Then when the animation is finished, init_planning would be called
				if (enteringBattle) set_mode(inputMode_t.IN_TURN);
				return true;
			case inputMode_t.WEAPON:
				copy_current_state();
				// Light up the corresponding attack node
				// TODO: We will do something fancier in later stages of the game
				UIAnimation.UIA.activate_attack_node(attackNodes[stepNumber], true);
				weapon = tempPlayer.get_weapon(action.wpnId);
				if (weapon == null || weapon.passive)
				{
					inputMode = inputMode_t.ATTACK;
					add_input();
					return true;
				}
				newMode = inputMode_t.ATTACK;
				break;
			case inputMode_t.ATTACK:
				// Creates a new tempBoard on top of the tempBoards stack
				// and changes tempPlayer to the player in the new copy
				// Always copy first before making changes!
				copy_current_state();
				weapon = tempPlayer.get_weapon(action.wpnId);
				if (weapon == null)
				{
					newMode = inputMode_t.MOVE;
				}
				else
				{
					cur = weapon.get_tile_mode(mousePos, tempPlayer.get_pos(), mousePos, inputMode,
						get_board(), tempPlayer);
					if (!cur.isValidTarget)
					{
						revert_to_prev_state();
						return false;
					}
					// The newMode is usually MOVE but it can also be ATTACK or SPMOVE
					newMode = weapon.generate_action (action, tempPlayer.get_pos (), mousePos, inputMode);
				}
				tempPlayer.add_action (action);
				game_t.execute_attack(get_board());
				// Let time move forward
				play_animation();
				start_time();
				// Create a time stop effect
				Invoke("stop_time", 0.1f);
				break;
			case inputMode_t.SPMOVE:
				copy_current_state();
				weapon = tempPlayer.get_weapon(action.wpnId);
				// This should only be MOVE
				newMode = weapon.generate_action (action, tempPlayer.get_pos (), mousePos, inputMode);

				copy_current_state();

				tempPlayer.add_action (action);
				game_t.execute_sp_movement(get_board());
				play_animation();
				start_time();
				break;
			case inputMode_t.MOVE:
				cur = get_tile_mode(mousePos, tempPlayer.get_pos());
				if (!cur.isValidMove) return false;
				copy_current_state();
				// UI Animation
				UIAnimation.UIA.activate_movement_bar(movementBars[stepNumber], true);
				diff = mousePos - tempPlayer.get_pos (); 
				action = new action_t ();
				action.movement = diff;

				tempPlayer.add_action (action);
				// Execute a step of the copy of the game
				game_t.execute_movement (get_board());
				// Let time move forward
				play_animation();
				start_time();

				actions.Push (action);
				newMode = inputMode_t.WEAPON;
				break;
			default:
				Debug.Assert(false);
				return false;
		}
		if (newMode == inputMode_t.MOVE)
		{
			// Update the board so that other interesting stuff can happen
			// Like turrets targeting the player and so on
			game_t.resolve_step(get_board());
			stepNumber += 1;
			if (stepNumber == stepsPerTurn)
			{
				newMode = inputMode_t.FINISHED;
			}
		}
		set_mode (newMode);
		return true;
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
		Debug.Log("cancel input");
		inputMode_t newMode = new inputMode_t();
		if (actions.Count == 0) return false;
		action_t prevAct = actions.Peek ();
		// TODO: Revise this when all animations stuff are settled
		weapon_t weapon = tempPlayer.get_weapon(prevAct.wpnId);
		switch (inputMode)
		{
			case inputMode_t.IN_TURN:
				return false;
			case inputMode_t.FREE:
				return false;
			case inputMode_t.WEAPON:
				if (actions.Count == 1)
					// We are at the bottom of the stack
				{
					Debug.Log("Reached bottom of the stack!");
					return false;
				}
				revert_to_prev_state ();
				actions.Pop();
				newMode = inputMode_t.MOVE;
				break;
			case inputMode_t.ATTACK:
				revert_to_prev_state ();
				if (prevAct.wpnId == -1) 
				{
					newMode = inputMode_t.MOVE;
				} else 
				{
					newMode = weapon.cancel_action (prevAct, inputMode);
				}
				break;
			case inputMode_t.SPMOVE:
				revert_to_prev_state ();
				Debug.Assert(weapon != null);
				{
					newMode = weapon.cancel_action (prevAct, inputMode);
				}
				break;
			case inputMode_t.MOVE:
				revert_to_prev_state ();

				if (prevAct.wpnId == -1) 
				{
					newMode = inputMode_t.WEAPON;
				} else 
				{
					weapon = tempPlayer.get_weapon(prevAct.wpnId);
					newMode = weapon.cancel_action (prevAct, inputMode);
				}
				stepNumber -= 1;
				break;
			case inputMode_t.FINISHED:
				revert_to_prev_state ();

				if (prevAct.wpnId == -1) 
				{
					newMode = inputMode_t.WEAPON;
				} else 
				{
					weapon = tempPlayer.get_weapon(prevAct.wpnId);
					newMode = weapon.cancel_action (prevAct, inputMode);
				}
				stepNumber -= 1;
				break;
			default:
				Debug.Assert(false);
				return false;
		}
		if (newMode == inputMode_t.MOVE)
		{
			UIAnimation.UIA.activate_movement_bar(movementBars[stepNumber], false);
		}
		else if (newMode == inputMode_t.WEAPON)
		{
			UIAnimation.UIA.activate_attack_node(attackNodes[stepNumber], false);
		}
		set_mode (newMode);
		return true;
    }

	public void get_ready()
	// For the case where playerCount == 1 this starts the game 
	{
		while(tempBoards.Count != 1)
		{
			revert_to_prev_state();
		}
		set_mode(inputMode_t.IN_TURN);
		List<List<action_t>> input = new List<List<action_t>>();
		List<action_t> actionList = new List<action_t>();
		foreach (action_t action in actions.ToArray())
		{
			actionList.Insert(0, action);
		}
		// TODO: This is only for testing
		input.Add(actionList);
		input.Add(new List<action_t>());
		stop_animation();
		game_t.execute_turn(board, input);
		StartCoroutine(rotate_camera(Quaternion.Euler(-45f, 0f, 0f)));
		//playButton.interactable = true;
		play_animation();
		start_time();
	}

	void copy_current_state()
	// Generate a copy of the current board and make that the current temp board
	// Also changes temp player and pushes a new list of animation controllers
	{
		// Copy the virtual board
		board_t newTempBoard = objectCopier.clone (get_board ());
		// As well as the physical one
		List<animController_t> animCtrlList = animControllers.Peek();
		List<animController_t> newAnimCtrlList = new List<animController_t> ();
		for (int i = 0; i < animCtrlList.Count; i++) 
		{
			GameObject copy = GameObject.Instantiate (animCtrlList [i].gameObject) as GameObject;
			animController_t aCtrl = copy.GetComponent<animController_t>();
			// Copy every animation over
			// For some animations with extra game objects, those game objects will also be copied
			aCtrl.copy_from(animCtrlList [i]);
			newAnimCtrlList.Add(aCtrl);
			animCtrlList[i].set_active (false);
		}
		// Add the new list of the animation controllers
		animControllers.Push (newAnimCtrlList);
		tempPlayer = newTempBoard.get_players () [playerId];
		tempBoards.Push (newTempBoard);
	}

	void revert_to_prev_state()
	// Reverts both the physical board and the virtual board to the original state
	// By popping the destroying the topmost item on the stack of copies
	{
		Debug.Log("Revert_to_prev_state!");
		tempBoards.Pop ();
		tempPlayer = get_board ().get_players () [playerId];
		foreach (animController_t animCtrl in animControllers.Pop())
		{
			animCtrl.destroy();
		}
		foreach (animController_t animCtrl in animControllers.Peek())
		{
			animCtrl.set_active (true);
		}
	}

	//--------------------------
	// All the animation methods
	//--------------------------

	void stop_time()
	{
		Time.timeScale = 0f;
	}

	void start_time()
	{
		Time.timeScale = 1f;
	}

	void stop_animation()
	{
		foreach(animController_t aCtrl in animControllers.Peek())
		{
			aCtrl.stop();
		}
	}

	public void play_animation()
	{
		haltedControllers = 0;
		foreach(animController_t aCtrl in animControllers.Peek())
		{
			Debug.Assert(aCtrl.gameObject != null);
			aCtrl.play();
		}
		playButton.interactable = false;
	}

	public void animation_halted()
	// Called exclusively by HALT animations
	{
		haltedControllers += 1;
		if (haltedControllers == animControllers.Peek().Count)
			// All animation controllers are done with their animations for the step
		{
			// Start the animations for the next step
			play_animation();
		}
	}

	public void animation_finished()
	// Called exclusively by animation controllers when they run out of animations to play
	{
		// See if turn has ended
		bool turnEnded = true;
		foreach (animController_t anim in animControllers.Peek())
		{
			if (anim.index < anim.animations.Count)
			{
				turnEnded = false;
			}
		}
		if (inputMode == inputMode_t.IN_TURN && turnEnded)
		{
			init_planning();
		}
	}

	public void send_animation(List<animation_t> animSequence, int objectId)
	{
		animControllers.Peek()[objectId].animations.AddRange(animSequence);
	}

	public void warp_time(float scale)
	{
		if (Time.timeScale > timeScale * scale) Time.timeScale = timeScale * scale;
		Debug.Log(Time.timeScale);
	}

	public void reset_time()
	{
		Debug.Log("Timescale reset!");
		Time.timeScale = timeScale;
		// If we are in planning
		//stop_animation();
	}

	public void set_spline(Vector2 start, Vector2 end)
	//
	{
		BezierSpline spline = attackSpline.GetComponent<BezierSpline>();
		spline.points[0] = SS.board_to_world(start);
		spline.points[1] = SS.board_to_world(Vector2.Lerp(start, end, 0.25f)) + Vector3.back;
		spline.points[2] = SS.board_to_world(Vector2.Lerp(start, end, 0.75f)) + Vector3.back * 2;
		spline.points[3] = SS.board_to_world(end);
		attackSpline.GetComponent<SplineDecorator>().decorate();
	}

	public IEnumerator move_camera(Vector3 target)
	{
		int frames = 40;
		Transform camera = GameObject.Find("Main Camera").transform;
		Vector3 start = camera.position;
		for (int i = 1; i < frames + 1; i++)
		{
			camera.position = Vector3.Lerp(start, target, (float)i / frames);
			yield return new WaitForFixedUpdate();
		}
	}

	public IEnumerator rotate_camera(Quaternion target)
	{
		int frames = 80;
		Transform camera = GameObject.Find("Main Camera").transform;
		Quaternion start = camera.rotation;
		for (int i = 1; i < frames + 1; i++)
		{
			camera.rotation = Quaternion.Lerp(start, target, (float)i / frames);
			yield return new WaitForFixedUpdate();
		}
	}

	//-------------------------------
	// Game and level related methods
	//-------------------------------

	public void load_level(int levelId)
	// load the level indexed by levelId
	{
		currLevel = new level_t(levelInfos[levelId].text);
	}

	public void next_scene()
	{
		reset_scene();
		currLevel.load_next_scene();
	}

	//--------------
	// Get-set functions
	//--------------

	board_t get_board()
	// Returns the board that we are currently playing on
	{
		if (inputMode == inputMode_t.FREE || inputMode == inputMode_t.IN_TURN)
			// We are in free mode
		{
			return board;
		}
		else
			// We are in planning mode
		{
			return tempBoards.Peek();
		}
	}

	public void set_mode(inputMode_t mode)
	{
		inputMode = mode;
		Debug.Log ("Set_mode:");
		print_mode (mode);
		if (tiles != null) update_tiles();
		// Update the spline
		if (mode == inputMode_t.ATTACK)
		{
			//			attackSpline.GetComponent<SplineDecorator>().show_curve(true);
		}
		else
		{
			attackSpline.GetComponent<SplineDecorator>().clear();
		}
	}

	public void set_weapon(int wpnId)
	{
		actions.Peek().wpnId = wpnId;
		add_input();
	}

	public void print_mode(inputMode_t mode)
	{
		Debug.Log(mode.ToString());
	}

	public void set_mouse_position(Vector2 pos)
	{
		mousePos = pos;
		update_tiles();
	}
}
