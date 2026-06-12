using UnityEngine;

// GOBLIN BLOCK — player controller + pump shotgun + bat fallback.
// Put on the player camera (with a CharacterController on the parent capsule,
// or assign one). Mirrors the Three.js fire()/reload/melee logic.
[RequireComponent(typeof(CharacterController))]
public class FirstPersonShooter : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 8.5f;
    public float sprintSpeed = 13.5f;
    public float crouchSpeed = 4.8f;
    public float jumpSpeed = 7f;
    public float gravity = 22f;
    public float lookSensitivity = 2.2f;
    public float standHeight = 1.7f;
    public float crouchHeight = 0.95f;

    [Header("Shotgun")]
    public int pellets = 8;
    public float spreadDegrees = 3.2f;
    public float range = 90f;
    public float damagePerPellet = 12f;
    public int magSize = 8;
    public int reserveMax = 48;
    public float fireCooldown = 0.6f;     // fire + pump cycle
    public float reloadPerShell = 0.55f;
    public LayerMask hitMask = ~0;
    public Transform muzzle;              // empty at barrel tip
    public ParticleSystem muzzleFlash;
    public GameObject impactDecal;        // optional blood/scorch quad

    [Header("Bat")]
    public float batDamage = 30f;
    public float batRange = 2.4f;
    public float batCooldown = 0.5f;

    [Header("Refs")]
    public Camera cam;
    public Animator weaponAnimator;       // optional: "Fire","Pump","Reload","Swing","ToBat","ToGun"

    int shells, reserve;
    float pitch, cooldown;
    bool usingBat;
    CharacterController cc;
    float vy;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (cam == null) cam = GetComponentInChildren<Camera>();
        shells = magSize; reserve = 24;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Look();
        Move();
        cooldown -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0)) TryFire();
        if (Input.GetKeyDown(KeyCode.R)) StartReload();
    }

    void Look()
    {
        float mx = Input.GetAxis("Mouse X") * lookSensitivity;
        float my = Input.GetAxis("Mouse Y") * lookSensitivity;
        transform.Rotate(0f, mx, 0f);
        pitch = Mathf.Clamp(pitch - my, -85f, 85f);
        cam.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    void Move()
    {
        bool crouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        bool sprint = !crouch && Input.GetKey(KeyCode.LeftShift);
        cc.height = Mathf.Lerp(cc.height, crouch ? crouchHeight : standHeight, Time.deltaTime * 9f);

        float speed = crouch ? crouchSpeed : sprint ? sprintSpeed : walkSpeed;
        Vector3 dir = (transform.forward * Input.GetAxisRaw("Vertical") +
                       transform.right   * Input.GetAxisRaw("Horizontal"));
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        if (cc.isGrounded)
        {
            vy = -1f;
            if (Input.GetKeyDown(KeyCode.Space)) vy = jumpSpeed;
        }
        vy -= gravity * Time.deltaTime;
        Vector3 vel = dir * speed + Vector3.up * vy;
        cc.Move(vel * Time.deltaTime);
    }

    void TryFire()
    {
        if (cooldown > 0f) return;
        if (usingBat) { Swing(); return; }
        if (shells <= 0) { StartReload(); return; }

        shells--;
        cooldown = fireCooldown;
        if (muzzleFlash) muzzleFlash.Play();
        if (weaponAnimator) weaponAnimator.SetTrigger("Fire");

        for (int i = 0; i < pellets; i++)
        {
            Vector3 dir = ConeDirection(cam.transform.forward, spreadDegrees);
            if (Physics.Raycast(cam.transform.position, dir, out RaycastHit hit, range, hitMask))
            {
                float falloff = Mathf.Max(0.4f, 1f - hit.distance / 60f);
                var dmg = hit.collider.GetComponentInParent<GoblinAI>();
                if (dmg) dmg.TakeHit(hit.collider, damagePerPellet * falloff, dir);
                if (impactDecal) Instantiate(impactDecal, hit.point + hit.normal * 0.01f,
                    Quaternion.LookRotation(hit.normal));
            }
        }

        // recoil kick
        pitch = Mathf.Clamp(pitch - (2f + Random.value), -85f, 85f);
        if (shells <= 0 && reserve <= 0) SwitchToBat();
    }

    void StartReload()
    {
        if (usingBat || shells >= magSize || reserve <= 0 || cooldown > 0f) return;
        StartCoroutine(ReloadRoutine());
    }

    System.Collections.IEnumerator ReloadRoutine()
    {
        if (weaponAnimator) weaponAnimator.SetTrigger("Reload");
        while (shells < magSize && reserve > 0)
        {
            yield return new WaitForSeconds(reloadPerShell);
            shells++; reserve--;
        }
    }

    void Swing()
    {
        cooldown = batCooldown;
        if (weaponAnimator) weaponAnimator.SetTrigger("Swing");
        foreach (var g in Object.FindObjectsByType<GoblinAI>(FindObjectsSortMode.None))
        {
            Vector3 to = g.transform.position - cam.transform.position;
            if (to.magnitude < batRange && Vector3.Dot(to.normalized, cam.transform.forward) > 0.35f)
                g.TakeHit(null, batDamage, cam.transform.forward);
        }
    }

    void SwitchToBat()  { usingBat = true;  if (weaponAnimator) weaponAnimator.SetTrigger("ToBat"); }
    void SwitchToGun()  { usingBat = false; if (weaponAnimator) weaponAnimator.SetTrigger("ToGun"); }

    Vector3 ConeDirection(Vector3 forward, float degrees)
    {
        float r = Mathf.Tan(degrees * Mathf.Deg2Rad);
        Vector2 c = Random.insideUnitCircle * r;
        return (forward + cam.transform.right * c.x + cam.transform.up * c.y).normalized;
    }

    public void AddAmmo(int n)  { reserve = Mathf.Min(reserveMax, reserve + n); if (usingBat) SwitchToGun(); }
    public int Shells => shells;
    public int Reserve => reserve;
    public bool UsingBat => usingBat;
}
