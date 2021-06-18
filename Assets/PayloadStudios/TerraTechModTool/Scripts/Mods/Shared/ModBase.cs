using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------
// <SHARED CLASS> This is shared between TerraTech and TTModDesigner
// -----------------------------------------------------------------
public abstract class ModBase
{
	public ModBase() { }

	// Called before any game systems are initialized. For advanced use cases.
	// If you want this to be called, you must return true in HasEarlyInit
	public virtual void EarlyInit() { }
	// Return true if your mod needs the EarlyInit hook before the game boots. If this is added
	// mid-way through a session, the game will prompt the player to restart.
	public virtual bool HasEarlyInit() { return false; }

	// Standard initialization function called when entering a gamemode with this mod active
	// Note that if the player joins a server not running this mod, this will not be called
	public abstract void Init();
	// Standard de-initialization function called when leaving a gamemode with this mod active
	// You should endevour to cleanup any changes your mod has made in case the player moves
	// to another session (probably in MP) that does not use this mod
	public abstract void DeInit();


	// Very similar to Unity's Update method, will be called once per frame by the mod loader
	public virtual void Update() { }
	// Very similar to Unity's FixedUpdate method, will be called once per physics frame by the mod loader
	public virtual void FixedUpdate() { }
}
