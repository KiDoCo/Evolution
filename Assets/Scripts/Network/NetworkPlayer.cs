using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayer : NetworkBehaviour {

    public Rigidbody rb { get; set; }

    [SerializeField] private float MovSpeed = 10f;            // Max player speed
    [SerializeField] private float SmoothSpeed = 1f;          // How fast player gets to max speed
    [SerializeField] private GameObject playerCamera = null;

    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        LocalStart();
    }

    private void Update()
    {
        // Moving is done only in local player. Position data is updated to other clients through Network Transform component
        if (isLocalPlayer)
        {
            Move();
        }
    }

    protected void Move()
    {
        Vector3 moveDirection = Vector3.Normalize(new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"))) * MovSpeed;
        rb.MovePosition(Vector3.SmoothDamp(rb.position, rb.position + transform.TransformDirection(moveDirection), ref currentVelocity, SmoothSpeed, Mathf.Infinity, Time.deltaTime));
    }

    /// <summary> Start configurations that are done only in local player </summary> 
    protected void LocalStart()
    {
        if (isLocalPlayer)
        {
            GameObject camera = Instantiate(playerCamera);
            //camera.GetComponent<CameraMovement>().FollowedPlayer = this.gameObject;
        }
    }
}