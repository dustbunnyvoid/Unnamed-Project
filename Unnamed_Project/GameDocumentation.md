# Team Documentation


## Table of Contents
1. [Scene Hierarchy](#scene-hierarchy)
2. [Required Tags](#required-tags)
3. [Adding a New Room](#adding-a-new-room)
4. [RoomTrigger, Wave Configuration](#roomtrigger-wave-configuration)
5. [Enemy Prefab Requirements](#enemy-prefab-requirements)
6. [Enemy Types](#enemy-types)
7. [Combat, HealthComponent & PlayerAttack](#combat-healthcomponent--playerattack)
8. [Lock-On System](#lock-on-system)
9. [Player](#player)
10. [Win & Death Conditions](#win--death-conditions)
11. [What Isn't Finished Yet](#what-isnt-finished-yet)
12. [Conventions](#conventions)

---

# Scene Hierarchy

```
TestScene
├── Player
│   ├── PlayerHandler       has PlayerMovement, LockOnSystem, Rigidbody, PlayerAttack
│   │   └── PlayerCollider  has CapsuleCollider, tagged "Player"
│   └── CameraTarget        has LockOnCameraTarget, drives the Cinemachine target
├── Cameras
│   ├── Main Camera
│   └── VirtualCameraOne    has Cinemachine camera, follows CameraTarget
├── Lighting
│   └── Directional Light
├── HUD                     has Canvas with all UI panels
├── GameManager             has Death/win screens and scene reload
├── EventSystem
└── Level
    ├── Hallway_Start       has Entry corridor before Room 1
    ├── Room_1              has Self-contained: geometry, walls, door, trigger, spawns
    │   ├── Wall_Room1_Left / Wall_Room1_Right
    │   ├── Hallway_1
    │   ├── Door_Room1
    │   ├── RoomTrigger_1
    │   └── Spawns_Room1
    ├── Room_2              Same as Room_1
    ├── Room_3              ditto
    └── ExitTrigger         If the player touches it then it triggers "You Win"
```

Each room is self-contained. geometry, exit walls, door, trigger, and spawn points all live under the room's GameObject. When you build a new room, please follow this same grouping so the hierarchy doesn't become a mess.

---

# Required Tags

These tags are hard-coded into scripts.

| Tag | Applied to | Why |
|---|---|---|
| `Player` | `PlayerCollider` (child of PlayerHandler) | RoomTrigger detects room entry, RusherAI/SwingerAI find the player, ExitTrigger fires win |
| `Enemy` | Every enemy GameObject | LockOnSystem's detection sphere filters by this tag, PlayerAttack hits filter by this too |


---

# Adding a New Room

Rooms are ProBuilder meshes (but you can also manually make them in the scene if you want). Each room needs: geometry, exit wall panels, a door, a trigger, and spawn points.

1. Build the room geometry. Keep the room mesh open on the entrance side (no wall). The exit side also needs to be open, the door GameObject handles that opening, not the mesh itself.
    - If you create the exit wall in ProBuilder and seal it, the player will never be able to leave even after combat ends. ...ask me how I know...

2. Add exit wall panels (the wall on either side of the door opening). Create two thin cubes covering the wall area left and right of the 3-unit-wide doorway. Give each a MeshCollider.
    - e.g. for a 10-wide room: left panel is 3.5 wide, right panel is 3.5 wide, door covers the center 3 units.

3. Create a Door, a box-shaped GameObject with a `BoxCollider` and a visible mesh. Size it to exactly match the doorway opening (3 units wide × 3.5 units tall in the existing rooms). This is what `RoomTrigger` toggles to close and open the exit.
    - Assign it to `RoomTrigger.door` in the inspector.
    - The door starts active (visible and blocking). When all waves are cleared, the trigger calls `SetActive(false)` to open it.

4. Add spawn points, empty GameObjects positioned where enemies will appear. Group them under a `Spawns_RoomN` parent. Assign the individual spawn point Transforms to `RoomTrigger.spawnPoints` (one per wave, in order).

5. Add a RoomTrigger, create an empty GameObject with a `BoxCollider` (set `Is Trigger = true`) and add the `RoomTrigger` script.
    - Position it just inside the room entrance so it fires as soon as the player steps in (not in the middle of the room).
    - A size of about (4, 2, 2) works for the existing 3-unit-wide hallways.

6. Add a connecting hallway between this room's exit and the next room's entrance. The existing hallways are 3 units wide × 3.5 units tall and have no end walls (so they connect flush to the rooms).

7. Parent everything under the room's root GameObject in the hierarchy so it stays organized.

8. Bake the NavMesh. Both enemy AI types require a NavMesh to pathfind. Go to `Window > AI > Navigation` and bake after placing geometry. Enemies won't move without it. We learned how to do this in the personal project but I also like using this video as a refresher: https://www.youtube.com/watch?v=SMWxCpLvrcc&pp=ygUNdW5pdHkgbmF2bWVzaA%3D%3D

---

# RoomTrigger, Wave Configuration

`RoomTrigger` is on `Assets/Scripts/Level/RoomTrigger.cs`. It handles the full room encounter: sealing the exit, spawning enemies wave by wave, showing intro cards, and opening the door when everything's dead.

**Fields to set in the inspector:**

| Field | What it does |
|---|---|
| `Waves` | List of `EnemyWave` entries, run in order |
| `Spawn Points` | One Transform per wave, where that wave's enemies spawn from |
| `Door` | The door GameObject to toggle |
| `Intro Duration` | How long the intro card stays on screen (default 3.5s) |
| `Intro Fade Time` | Fade in/out duration for the intro card (default 0.5s) |

**EnemyWave fields:**

| Field | What it does |
|---|---|
| `Prefab` | The enemy prefab to spawn |
| `Count` | How many of this enemy to spawn sequentially |
| `Display Name` | Title shown on the intro card for the first enemy of this wave |
| `Description` | Subtitle shown on the intro card |

- Enemies within a wave spawn **one at a time**, the next spawns only after the previous one dies. If you want multiple enemies alive at once, use separate waves or rework the coroutine.
- The first enemy of each wave freezes the player briefly and shows the intro card before the fight starts. Subsequent enemies in the same wave skip the intro.
- On each spawn, the lock-on system is automatically force-targeted to the new enemy. The player doesn't need to press T.
- If `Display Name` is left empty, the intro card is skipped and the player is just given the freeze delay.

---

# Enemy Prefab Requirements

Any prefab spawned by `RoomTrigger` **must** have:

- `HealthComponent`, the room trigger listens to `OnDeath` to know when to move to the next spawn. Without it, the room assumes the enemy is already dead and skips immediately.
- Either `RusherAI` **or** `SwingerAI`, the trigger calls `WakeUp()` on whichever is present after the intro card. Without a WakeUp call, the enemy stands still forever.
- `NavMeshAgent`, required by both AI types (enforced via `[RequireComponent]`). Enemies won't compile without it.
- `Rigidbody`, also required. The AI sets it to kinematic internally, so you don't need to configure it.
- Tagged `"Enemy"`, the collider on the enemy must have this tag, otherwise the lock-on system and player attacks won't see it.

The enemy **does not need a separate death script**, both AI types call `Destroy(gameObject, 0.1f)` on their own when `OnDeath` fires.

---

# Enemy Types

## RusherAI
Charges directly at the player and deals contact damage.

| Field | Default | Notes |
|---|---|---|
| `Move Speed` | 3.5 | NavMeshAgent speed |
| `Stop Range` | 1.0 | Distance at which it stops pursuing (prevents clipping) |
| `Contact Range` | 1.2 | Radius for dealing damage |
| `Contact Damage` | 10 | Per hit |
| `Damage Interval` | 0.6 | Seconds between damage ticks |

## SwingerAI
Approaches, stops at melee range, winds up, then swings in an arc.

| Field | Default | Notes |
|---|---|---|
| `Move Speed` | 2.5 | NavMeshAgent speed |
| `Approach Stop Distance` | 2.5 | Distance at which it stops and begins windup |
| `Attack Range` | 2.8 | Radius of the swing OverlapSphere |
| `Attack Damage` | 25 | Damage per swing |
| `Swing Arc Angle` | 100 | Total arc angle in degrees (player must be within 50° of forward) |
| `Windup Duration` | 1.2 | Seconds of telegraph before the swing |
| `Swing Duration` | 0.25 | Seconds the swing hitbox is active |
| `Cooldown Duration` | 1.8 | Seconds before it approaches again |

Both types expose a `WakeUp()` method and do nothing until it's called, this is intentional so enemies don't pathfind during the intro card.

---

# Combat, HealthComponent & PlayerAttack

## HealthComponent
`Assets/Scripts/Combat/HealthComponent.cs`, attach this to anything that can take damage (players and enemies both use it).

**Public events:**
- `OnDeath`, fires once when health hits 0
- `OnDamageTaken`, fires on any damage (used by DamageFlash UI)
- `OnHealthChanged`, fires with normalized health (0,1), used by health bar UI

**Public method:**
- `TakeDamage(float amount)`, call this to deal damage. Safe to call multiple times; ignores calls once dead.

**Public property:**
- `NormalizedHealth`, current health as a 0,1 float

## PlayerAttack
`Assets/Scripts/Player/PlayerAttack.cs`, lives on PlayerHandler. LMB fires an OverlapSphere in front of the player.

| Field | Default |
|---|---|
| `Attack Range` | 1.8 |
| `Attack Damage` | 34 |
| `Attack Cooldown` | 0.5s |

The attack hits all colliders tagged `"Enemy"` within the sphere and calls `TakeDamage` on their `HealthComponent`. A burst particle effect spawns on each hit via `FXHelper.SpawnBurst`.

---

# Lock-On System

`Assets/Scripts/Player/LockOnSystem.cs`, lives on PlayerHandler.

**Player controls:**
- `T`: cycle to next closest enemy (or start locking on)
- `Y`: release lock-on

**Inspector fields:**

| Field | Default | Notes |
|---|---|---|
| `Cast Radius` | 5 | Detection sphere radius for manual cycling |
| `Vision Mode` | false | If true, only detects enemies within the vision cone |
| `Vision Angle` | 45° | Half-angle of the cone (only used in vision mode) |
| `Debug Visible` | true | Draws gizmos in the editor for the detection sphere and active lock-on |

**For code that needs to interact with the lock-on:**

`LockOnSystem.TargetEnemy`, the currently locked-on GameObject, or `null`. Read-only from outside the class, but `ForceTarget` is the approved way to set it:

```csharp
lockOnSystem.ForceTarget(someEnemyGameObject);
```

`ForceTarget` bypasses the distance check entirely, the target won't be dropped until the enemy is destroyed or the player presses Y. This is what `RoomTrigger` uses when a new enemy spawns. If you're building something that needs to direct the player's attention to a specific target, use this.

Setting `ForceTarget(null)` clears the lock-on.

When a target is active, `PlayerMovement` automatically rotates the player to face it and `LockOnCameraTarget` lerps the camera toward a midpoint between the player and the enemy.

---

# Player

`Assets/Scripts/Player/PlayerMovement.cs`, lives on PlayerHandler.

Movement is camera-relative (WASD moves relative to where the camera is facing, not the world). Physics are Rigidbody-based, Y velocity is preserved so gravity works normally.

| Field | Default |
|---|---|
| `Move Speed` | 2 |
| `Rotation Speed` | 10 |

**IsFrozen**, set `player.IsFrozen = true` to stop all player input and zero out horizontal velocity. `RoomTrigger` uses this during the intro card sequence. Setting it back to false re-enables movement. Don't leave it set to true or the player will be stuck permanently.

PlayerHandler needs a `Rigidbody` with `Collision Detection` set to `Continuous Dynamic`, this is set in code on `Start`, but if you're duplicating the prefab and things feel jittery, check that the Rigidbody is on the same GameObject as `PlayerMovement` and `LockOnSystem`.

---

# Win & Death Conditions

## Death
`GameManager` (`Assets/Scripts/Core/GameManager.cs`) listens to the player's `HealthComponent.OnDeath` event. When it fires, it shows the death screen and freezes time (`Time.timeScale = 0`). The restart button calls `GameManager.Restart()` which reloads the current scene.

## Win
Place an `ExitTrigger` (`Assets/Scripts/Level/ExitTrigger.cs`) at the end of the level. When the player's collider enters it, it calls `GameManager.Instance.TriggerWin()`, which shows the win screen and freezes time.

- `GameManager` is a singleton (`GameManager.Instance`). There should only ever be one in the scene.
- Both win and death states set `_gameOver = true` internally, so if somehow both trigger at the same time, only the first one counts.

---

# What Isn't Finished Yet

Being upfront about this so you don't spend time trying to use things that don't exist yet.

- **Multiple enemies alive simultaneously**, the wave system spawns enemies sequentially (one at a time). Parallel spawning isn't supported by the current `RoomTrigger` coroutine.
- **BPM / mid-level events**, no event/director system. Scripted moments beyond the wave intro card aren't wired up.
- **Cinemachine**, `VirtualCameraOne` is in the scene and Cinemachine 3.1.6 is installed, but the virtual camera is not fully driving gameplay yet. The camera is primarily controlled by `LockOnCameraTarget` on `CameraTarget`. Again if you don't remember, this was used in your personal rpoject.
- **NavMesh baking**, you have to bake the NavMesh manually every time geometry changes. There's no auto-bake step.
- **Death animations**, enemies are destroyed 0.1 seconds after `OnDeath` fires. There's no animation or ragdoll system yet.

---

# MISC STUFF

- **Tags over layers**, detection is tag-based, not layer-based. `"Enemy"` and `"Player"` are the only tags that matter right now.
- **No input abstraction**. input is read directly via `Input.GetAxisRaw`, `Input.GetKeyDown`, and `Input.GetButtonDown`. There's no InputSystem or input action asset. If you want to remap controls, change the key in the script.
- **Singletons**, `GameManager.Instance` and `RoomIntroUI.Instance` are singletons. One of each per scene. The intro UI is on the HUD canvas, don't duplicate it.

- **Prototype materials**, visual materials are from Gridbox Prototype Materials in `Assets/Thirdparty/`. New ProBuilder shapes default to `ProBuilderDefault.mat`. You'll need to apply a Gridbox material manually if visual consistency matters for a test.
- **Room naming**, follow the existing pattern: `Room_N`, `Hallway_N`, `Door_RoomN`, `RoomTrigger_N`, `Spawns_RoomN`. It's not enforced anywhere but it keeps the hierarchy readable.
