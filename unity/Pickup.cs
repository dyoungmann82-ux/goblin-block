using UnityEngine;

// GOBLIN BLOCK — ammo can / medkit pickup. Put on the pickup prefab with a
// trigger Collider. Bobs and spins; grants on player overlap.
[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    public enum Type { Health, Ammo }
    public Type type = Type.Ammo;
    public int healthAmount = 25;
    public int ammoAmount = 8;
    public float bobHeight = 0.12f;
    public float bobSpeed = 2.4f;
    public float spinSpeed = 90f;
    public AudioSource pickupSound;

    Vector3 basePos;
    float t;

    void Start()
    {
        basePos = transform.position;
        GetComponent<Collider>().isTrigger = true;
        t = Random.value * 6f;
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.position = basePos + Vector3.up * Mathf.Sin(t * bobSpeed) * bobHeight;
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (type == Type.Health)
        {
            var hp = other.GetComponent<PlayerHealth>();
            if (hp == null || hp.IsFull) return;     // leave it if full
            hp.Heal(healthAmount);
        }
        else
        {
            var s = other.GetComponent<FirstPersonShooter>();
            if (s == null) return;
            s.AddAmmo(ammoAmount);
        }

        if (pickupSound) pickupSound.Play();
        Destroy(gameObject);
    }
}
