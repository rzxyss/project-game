using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Run Animation Multiplier")]
    public float runAnimSpeed = 1.8f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask = ~0; // default: semua layer

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogError("Animator tidak ditemukan! Pastikan komponen Animator ada di player atau child-nya.");
        else if (animator.runtimeAnimatorController == null)
            Debug.LogError("Animator Controller belum di-assign! Assign 'PersonController' ke komponen Animator.");
        else
            Debug.Log("Animator OK: " + animator.runtimeAnimatorController.name);

        // Auto-buat groundCheck tepat di bawah CharacterController
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            float bottom = controller.center.y - (controller.height / 2f) + controller.skinWidth;
            gc.transform.localPosition = new Vector3(0, bottom, 0);
            groundCheck = gc.transform;
        }

        // Fallback jika groundMask belum di-set di Inspector
        if (groundMask.value == 0)
            groundMask = ~0;
    }

    void Update()
    {
        // Gunakan isGrounded bawaan CharacterController (lebih andal)
        isGrounded = controller.isGrounded;

        // Backup: sphere check jika controller.isGrounded tidak akurat
        if (!isGrounded)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        animator.SetBool("isGrounded", isGrounded);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Input gerakan
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Arah berdasarkan kamera
        Transform cam = Camera.main.transform;
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 direction = (camForward * vertical + camRight * horizontal);

        bool isMoving = direction.magnitude > 0.1f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving;
        float speed = isRunning ? runSpeed : walkSpeed;

        // Gerak
        if (isMoving)
        {
            controller.Move(direction.normalized * speed * Time.deltaTime);

            // Rotasi karakter menghadap arah gerak
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Update animator
        animator.SetBool("isWalking", isMoving && !isRunning);
        animator.SetBool("isRunning", isRunning);
        animator.speed = 1f;

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
