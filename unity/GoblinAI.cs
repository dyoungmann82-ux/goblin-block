using UnityEngine;
using UnityEngine.AI;

// GOBLIN BLOCK — zombie/goblin AI. Needs NavMeshAgent + Animator on the same object.
// Shambler = slow relentless; Runner = approach → flank → charge → lunge.
// Mirrors the Three.js state machine and limb-damage model.
[RequireComponent(typeof(NavMeshAgent))]
public class GoblinAI : MonoBehaviour
{
    public enum Kind { Shambler, Runner }
    public enum State { Idle, Approach, Flank, Charge, Lunge, Attack, Dead }

    [Header("Type")]
    public Kind kind = Kind.Runner;
    public float maxHealth = 30f;
    public float shamblerSpeed = 1.3f;
    public float runnerSpeed = 3.2f;
    public float chargeMultiplier = 1.75f;

    [Header("Combat")]
    public float attackRange = 1.8f;
    public float attackDamage = 8f;
    public float attackInterval = 0.9f;
    public float flankRadius = 4f;

    [Header("Refs")]
    public Animator animator;            // params: Speed(float), Attack(trig), Hit(trig), Die(trig)
    public Dismemberment limbs;          // optional, for 4-point dismemberment

    State state = State.Idle;
    float hp, stateTimer, attackTimer, idleTime;
    int orbitDir;
    Transform player;
    PlayerHealth playerHp;
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (limbs == null) limbs = GetComponent<Dismemberment>();
        hp = maxHealth;
        idleTime = Random.Range(0.6f, 1.8f);
        orbitDir = Random.value < 0.5f ? 1 : -1;
        agent.speed = kind == Kind.Shambler ? shamblerSpeed : runnerSpeed;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) { player = p.transform; playerHp = p.GetComponent<PlayerHealth>(); }
    }

    void Update()
    {
        if (state == State.Dead || player == null) return;
        stateTimer += Time.deltaTime;
        attackTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);
        Vector3 toPlayer = (player.position - transform.position).normalized;

        switch (state)
        {
            case State.Idle:
                agent.isStopped = true;
                if (stateTimer > idleTime) Switch(State.Approach);
                break;

            case State.Approach:
                agent.isStopped = false;
                if (kind == Kind.Shambler)
                {
                    // drunken weave toward the player
                    Vector3 weave = Quaternion.Euler(0, Mathf.Sin(Time.time * 1.4f) * 20f, 0) * toPlayer;
                    agent.SetDestination(transform.position + weave * 3f);
                }
                else
                {
                    agent.SetDestination(player.position);
                    if (dist < 12f) Switch(State.Flank);
                }
                if (dist < attackRange) Switch(State.Attack);
                break;

            case State.Flank:
                agent.isStopped = false;
                Vector3 perp = Vector3.Cross(Vector3.up, toPlayer) * orbitDir;
                Vector3 ring = player.position - toPlayer * flankRadius + perp * flankRadius;
                agent.SetDestination(ring);
                if (Random.value < Time.deltaTime * 0.4f) orbitDir = -orbitDir;
                if (stateTimer > Random.Range(0.8f, 3f)) Switch(State.Charge);
                if (dist > 18f) Switch(State.Approach);
                if (dist < attackRange) Switch(State.Attack);
                break;

            case State.Charge:
                agent.isStopped = false;
                agent.speed = runnerSpeed * chargeMultiplier;
                agent.SetDestination(player.position);
                if (dist < 2.9f && dist > 1.6f && attackTimer <= 0f) Switch(State.Lunge);
                if (dist < attackRange) Switch(State.Attack);
                if (stateTimer > 3f) { agent.speed = runnerSpeed; Switch(State.Approach); }
                break;

            case State.Lunge:
                agent.velocity = toPlayer * runnerSpeed * 3f;
                if (stateTimer > 0.15f && dist < 2f) { Hit(); }
                if (stateTimer > 0.42f) { agent.speed = runnerSpeed; Switch(State.Approach); }
                break;

            case State.Attack:
                agent.isStopped = true;
                FaceTarget(toPlayer);
                if (attackTimer <= 0f) { Hit(); attackTimer = attackInterval; if (animator) animator.SetTrigger("Attack"); }
                if (dist > attackRange * 1.4f) Switch(State.Approach);
                break;
        }

        if (animator) animator.SetFloat("Speed", agent.isStopped ? 0f : agent.velocity.magnitude);
    }

    void Hit()
    {
        if (playerHp) playerHp.Damage((limbs && limbs.BothArmsGone) ? attackDamage * 0.5f : attackDamage);
    }

    void Switch(State s) { state = s; stateTimer = 0f; }
    void FaceTarget(Vector3 dir)
    {
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 8f);
    }

    // Called by FirstPersonShooter. collider may be a tagged limb (Head/ArmL/ArmR/Legs).
    public void TakeHit(Collider part, float dmg, Vector3 dir)
    {
        if (state == State.Dead) return;
        float mult = 1f;
        string tag = part ? part.tag : "Body";
        if (tag == "Head") mult = 2.3f;
        else if (tag == "ArmL" || tag == "ArmR" || tag == "Legs") mult = 0.8f;

        hp -= dmg * mult;
        if (animator) animator.SetTrigger("Hit");
        if (limbs) limbs.RegisterHit(tag, dmg, dir, this);   // may sever a limb / make crawler

        if (hp <= 0f) Die();
    }

    public void Cripple() { runnerSpeed *= 0.45f; shamblerSpeed *= 0.45f; agent.speed *= 0.45f; }

    void Die()
    {
        state = State.Dead;
        agent.enabled = false;
        if (animator) animator.SetTrigger("Die");
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        // corpse persists — no Destroy (matches the spec's accumulating corpses)
        WaveSpawner.NotifyDeath(this);
        enabled = false;
    }
}
