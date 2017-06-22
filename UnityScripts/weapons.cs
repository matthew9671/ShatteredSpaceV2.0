using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;

[Serializable]
public class weapon_t
{
	// All kinds of weapons are its subclasses
	public bool passive;
	// 0 momentum 1 explosive 2 particle 3 field (4 overheated particle ?)
	// The build cost of the weapon
	// Doesn't change once the weapon is created
	public int[] modules;

	// Number of shots fired in planning
	protected int fireCount;
	// Maximum number of shots that can be fired in planning
	const int maxFire = 1;

	// Any delay <0 would let the damage be generated at end of turn
	protected int delay_base;
	protected int damage_base;
	protected int range_base;

	public weapon_t(int range, int damage, int delay, int[] modules, bool passive = false)
	{
		this.range_base = range;
		this.damage_base = damage;
		this.delay_base = delay;
		this.modules = modules;
		this.passive = passive;
	}

	// #########################################################################
	// User interface related methods
	public virtual void refresh()
	// Refresh the weapon so that it can fire again in the next turn.
	{
		fireCount = 0;
	}

	public virtual bool can_be_fired()
	{
		return fireCount < maxFire;
	}

	public virtual inputMode_t generate_action(action_t action, 
		Vector2 playerPos, Vector2 mousePos, inputMode_t inputMode)
	// Change the action based on user input and return the next inputMode
	{
		// This is the most general case
		// So we assume that the attack is not generated
		// And we are not doing a special movement
		System.Diagnostics.Debug.Assert(action.attack == null);
		System.Diagnostics.Debug.Assert(maxFire > fireCount);
		System.Diagnostics.Debug.Assert(inputMode == inputMode_t.ATTACK);
		// Add the attack to the action
		action.attack = new attack_t(mousePos);
		fireCount += 1;
		return inputMode_t.MOVE;
	}

	public virtual inputMode_t cancel_action(action_t action, inputMode_t inputMode)
	{
		if (inputMode == inputMode_t.ATTACK)
			return inputMode_t.WEAPON;
		else if (inputMode == inputMode_t.MOVE || inputMode == inputMode_t.FINISHED) 
		{
			action.target = Vector2.zero;
			// Get rid of all permanent states
			//            fireCount -= 1;
			return inputMode_t.ATTACK;
		}
		else {
			return inputMode_t.MOVE;
		}
	}

	public virtual tileMode_t get_tile_mode(Vector2 tilePos, Vector2 playerPos, 
		Vector2 mousePos, inputMode_t inputMode, board_t board, unit_t master)
	// Returns the tile mode of the tile at tilePos
	// Generally speaking, when inputMode is ATTACK: 
	// tile.isOutOfRange = true if it is out of range from playerPos;
	// is validAttack if it is in range and have the mouse over it.
	{
		System.Diagnostics.Debug.Assert(inputMode == inputMode_t.ATTACK);
		tileMode_t result = new tileMode_t();
		if (!is_in_range(playerPos, tilePos, master, board))
		{
			result.isOutOfRange = true;
		}
		else if (mousePos == tilePos)
		{
			result.isValidTarget = true;
			gameManager_t.GM.set_spline(playerPos, tilePos);
		}
		return result;
	}

	public virtual bool is_in_range(Vector2 playerPos, Vector2 targetPos,
		unit_t master, board_t board)
	// Returns true if targetPos is within attack range from playerPos
	{
		int d = SS.distance(playerPos, targetPos);
		return (d > 0) && (d <= get_range(master)) 
			&& board.get_blocked(playerPos, targetPos) == null;
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

	public virtual animation_t fire(attack_t attack, board_t board, unit_t master)
	// Create pending damage on the board, 
	// return false if this is an invalid attack. 
	// May have some special effects on the master.
	{
		System.Diagnostics.Debug.Assert(attack != null);
		Vector2 pos = attack.target;
		int amount = get_damage_amount(master);
		int delay = get_delay(master);
		damage_t dmg = new damage_t();
		dmg.set_params(amount, delay);
		dmg.set_pos(pos);
		dmg.stepLife = 1;
		board.create_damage(dmg);
		return get_fire_animation(attack, master);
	}

	public virtual animation_t get_fire_animation(attack_t attack, unit_t master)
	{
		return animation_t.DO_NOTHING;
	}
}

[Serializable]
// The most basic weapon
// There should be a weaker version of it that only uses one momentum module
public class blaster_t : weapon_t
{
	new static int[] modules = {2, 0, 0, 0};
	public blaster_t():base(range:5, damage:5, delay:1, modules:modules)
	{}

	public override animation_t get_fire_animation (attack_t attack, unit_t master)
	{
		int frames = 20;
		return new blasterAnimation_t(attack.target, frames);
	}
}

[Serializable]
// A variation of the blaster that the turrets use
public class turretGun_t : weapon_t
{
	new static int[] modules = {100, 100, 100, 100};
	public turretGun_t():base(range:4, damage:5, delay:1, modules:modules)
	{}

	public override animation_t get_fire_animation (attack_t attack, unit_t master)
	{
		int frames = 20;
		return new blasterAnimation_t(attack.target, frames);
	}
}

[Serializable]
// The basic weapon explosive (bomb-based) weapon
// There should be a weaker version of it that only uses one explosive module
public class grenadeLauncher_t : weapon_t
{
	new static int[] modules = {0, 2, 0, 0};
	const int SPLASH_DAMAGE = 2;
	public grenadeLauncher_t():base(range:5, damage:4, delay:-1, modules:modules)
	{}

	public int get_splash_damage(unit_t master)
	// There might be upgrades that increases the splash damage
	{
		return SPLASH_DAMAGE;
	}

	public override animation_t fire(attack_t attack, board_t board, unit_t master)
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
		System.Diagnostics.Debug.Assert(pos != null);
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

// A special kind of damage that pushes players away
[Serializable]
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
			action_t action = new action_t();
			action.movement = direction;
			player.add_action(action);
		}
	}
}

