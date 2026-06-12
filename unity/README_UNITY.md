# GOBLIN BLOCK — Unity Port Kit

This folder gives you a real Unity starting point for the game: the 3D scene
(as a GLB) plus C# gameplay scripts that mirror the Three.js version.

## A. Get the scene into Unity
1. Open the web game in Chrome (`https://dyoungmann82-ux.github.io/goblin-block/`)
   and press **G** — it downloads `goblin-block.glb` (the whole street, houses,
   props, terrain). Or open `…/?export` and it auto-downloads after load.
2. In Unity: drag `goblin-block.glb` into `Assets/`. It imports as a model with
   all meshes + the baked photo textures. Drop it into the scene at origin.
   - This is a **blockout** — accurate proportions and layout. Swap individual
     houses/cars for Asset-Store / Sketchfab PBR models when you want full realism.
3. Select the GLB → Inspector → **Generate Colliders** (or add Mesh Colliders) so
   the player and zombies can walk on it.

## B. Project setup
- Unity **6 LTS**, **3D (URP)** template (or HDRP for max fidelity).
- Asset Store → import **Starter Assets - First Person** (free) for the player rig,
  OR use the included `FirstPersonShooter.cs` on your own capsule + camera.
- Window → AI → **Navigation (NavMeshSurface)**: add a NavMeshSurface to the scene
  root and **Bake** so zombies can pathfind. (Install "AI Navigation" via Package Manager.)
- Zombies: download a rigged model + walk/run/attack/death clips from **Mixamo**
  (free). Put an `Animator` on it with those states; `GoblinAI.cs` drives them.

## C. Scripts (drop into Assets/Scripts/)
| File | Put it on | Does |
|---|---|---|
| `FirstPersonShooter.cs` | Player camera | WASD move, mouse look, raycast shotgun, recoil, reload, bat fallback, crouch |
| `GoblinAI.cs` | Each zombie prefab (needs NavMeshAgent + Animator) | Idle→Shamble/Run→Flank→Charge→Attack state machine, limb damage |
| `WaveSpawner.cs` | An empty GameObject | Escalating waves from spawn points |
| `PlayerHealth.cs` | Player | HP, damage, death, heal |
| `Pickup.cs` | Ammo can / medkit prefabs | Bob/spin, grant ammo or health on trigger |
| `Dismemberment.cs` | Zombie prefab | 4-point limb detach (head/L-arm/R-arm/legs) |

## D. Wire-up checklist
1. Player capsule + child Camera. Add `FirstPersonShooter`, `PlayerHealth`.
2. Make a zombie prefab: Mixamo model + `NavMeshAgent` + `Animator` + `GoblinAI` +
   `Dismemberment`. Tag child colliders: Head, ArmL, ArmR, Legs, Body.
3. Empty GameObject + `WaveSpawner`; assign the zombie prefab and 4–10 spawn-point
   Transforms placed at the intersections.
4. Bake NavMesh. Press Play.

Everything below is plain Unity C# — no paid packages required except the models
you choose for realism.
