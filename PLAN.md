# PROJECT GENESIS — Consolidated Build Plan (v4 + v5 PDFs + chat directives)
Status: EXECUTED 2026-06-11. All phases shipped & smoke-tested (live run, zero exceptions).
Game: C:\Users\dkyun\Claude\goblin-block\index.html · serve port 8741 · ?demo for testing

## Phase 1 — Mesh/Geometry pipeline — DONE
- [x] Terrain heightmap: displaced road meshes, crowned street, noise on sidewalks; camera/goblin physics follow surfaceY()
- [x] Porches fully traversable: continuous sidewalk→yard→porch elevation ramps; knee-wall/retaining-wall/pier colliders gate access to the step openings
- [x] Unique houses: per-house cloned photo crop (zoom/offset jitter), roofline height jitter, setback jitter, 55% protruding bay windows
- [x] Mesh detail on cars (hood/trunk/bumpers/mirrors/lights), trees (branches, layered canopy), pickups
- [x] Interior collision: walk-rect union + prop circles (deli, alleys)

## Phase 2 — Combat — DONE
- [x] Realistic 870-pattern shotgun: receiver/ejection port/vent rib/bead/mag tube/ribbed walnut forend/trigger guard/butt pad
- [x] Gun physics: pitch recoil kick into shoulder, weapon inertia sway on turns, strafe lean, ejected brass with bounce + tink (persistent ~25s)
- [x] 4-point dismemberment: head / L-arm / R-arm / legs — limbs detach, tumble, land, persist; head = kill; legs = crawler (drags by arms, .45×); both arms = weak bite
- [x] Bat fallback: ammo 0 → bare hand + ash bat viewmodel; swing arc, frontal cone hit (30 dmg + knockback), whoosh/thud; auto re-equips gun on ammo
- [x] Crouch (C / Ctrl): eye height + speed reduction
- [x] Realistic pickups: latched white medkit, stenciled olive ammo can

## Phase 3 — AI / Nav — DONE
- [x] Dijkstra flow-field grid (1.5 m cells, all walkable zones incl. porches/alleys/deli), rebuilt every .4 s — goblins path through structures
- [x] State machine: Idle → Shambling_Path / Runner(approach-flank-charge) → Attack_Lunge (leap + hit frame)

## Phase 4 — Gore & persistence — DONE
- [x] Corpses persist (cap 45), settle to terrain, register static colliders
- [x] Blood pools permanent (cap 110); ground spatter decals from droplets (cap 250); severed limbs persist (cap 40)

## Phase 5 — Atmosphere — DONE
- [x] Rowhouse-skyline silhouette band ringing the horizon dome (street-end vistas)
- [x] Burning trash barrel @ Master corner: flickering fire light + additive flame particles
- [x] Deli sign worn-ballast flicker
- [x] Human corpse set dressing ×4 with blood pools
- [x] 4096 PCF-soft shadow map (CSM evaluated — skipped: high breakage risk for marginal gain at this scene scale)

## Known deviations
- CSM replaced by tight single-cascade 4096 shadows (see above)
- Skeletal bone-linking approximated by per-mesh zone tagging (equivalent behavior, no skinned rig)
