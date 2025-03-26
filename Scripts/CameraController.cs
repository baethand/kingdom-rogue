using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] private Transform player;
    [SerializeField] private float cameraSpeed = 1.0f;
    [SerializeField] private float xOffsetParam = 0.0f;
    [SerializeField] private float yOffsetParam = 0.0f;

    private Rigidbody2D heroRb;

    private Vector3 pos;
    private float xOffset = 0.0f;
    private float yOffset = 0.0f;

    private void Awake() {
        if (!player){
            player = FindAnyObjectByType<CharacterController>().transform;
        }

        heroRb = player.GetComponent<Rigidbody2D>();
    }
    
    void Update() {

        if (player.GetComponent<CharacterController>().moveInput > 0) 
            xOffset = xOffsetParam;
        else if (player.GetComponent<CharacterController>().moveInput < 0)
            xOffset = xOffsetParam * (-1);

        if (Input.GetAxis("Vertical") > 0) 
            yOffset = yOffsetParam;
        else if (Input.GetAxis("Vertical") < 0)
            yOffset = yOffsetParam * (-1);
        

        pos = player.position  + new Vector3(xOffset, yOffsetParam, 0);
        pos.z = -10f;

        if (cameraSpeed < 0.1f) {
            cameraSpeed = 0.1f;
        }

        transform.position = Vector3.Lerp(transform.position, pos, cameraSpeed * Time.deltaTime);
    }
}
