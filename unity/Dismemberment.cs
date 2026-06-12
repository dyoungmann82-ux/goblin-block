using UnityEngine;

// GOBLIN BLOCK — 4-point dismemberment (Head, L-Arm, R-Arm, Legs).
// Put on the zombie prefab. Assign the limb bone Transforms; tag their colliders
// "Head","ArmL","ArmR","Legs". When a zone's health hits zero the limb detaches,
// gets a Rigidbody, and tumbles free — and stays (no despawn).
public class Dismemberment : MonoBehaviour
{
    [System.Serializable]
    public class Limb { public string zone; public Transform bone; public float health = 20f; [HideInInspector] public bool gone; }

    public Limb head = new Limb { zone = "Head", health = 26f };
    public Limb armL = new Limb { zone = "ArmL", health = 20f };
    public Limb armR = new Limb { zone = "ArmR", health = 20f };
    public Limb legs = new Limb { zone = "Legs", health = 36f };
    public GameObject bloodBurst;     // optional particle prefab

    public bool BothArmsGone => armL.gone && armR.gone;

    public void RegisterHit(string tag, float dmg, Vector3 dir, GoblinAI ai)
    {
        Limb l = tag switch { "Head" => head, "ArmL" => armL, "ArmR" => armR, "Legs" => legs, _ => null };
        if (l == null || l.gone) return;
        l.health -= dmg;
        if (l.health <= 0f) Sever(l, dir, ai);
    }

    void Sever(Limb l, Vector3 dir, GoblinAI ai)
    {
        l.gone = true;
        if (l.bone)
        {
            l.bone.parent = null;                 // detach from the rig
            var rb = l.bone.gameObject.AddComponent<Rigidbody>();
            rb.AddForce(dir * 4f + Vector3.up * 3.5f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 8f, ForceMode.Impulse);
            // no Destroy — severed limbs persist on the ground
        }
        if (bloodBurst) Instantiate(bloodBurst, transform.position + Vector3.up * 1.2f, Quaternion.identity);

        if (l == head) SendMessage("Die", SendMessageOptions.DontRequireReceiver);  // headshot kill
        else if (l == legs && ai) ai.Cripple();                                     // crawler
    }
}
