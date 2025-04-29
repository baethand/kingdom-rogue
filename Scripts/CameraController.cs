using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] private Transform player;
    [SerializeField] private float cameraSpeed = 1.0f;

    private Vector3 pos;

    private void Awake() {
        if (!player){
            player = FindAnyObjectByType<MainHero>().transform;
        }
    }
    
    void Update() {
        pos = player.position;
        pos.z = -10f;

        if (cameraSpeed < 0.1f) {
            cameraSpeed = 0.1f;
        }

        transform.position = Vector3.Lerp(transform.position, pos, cameraSpeed * Time.deltaTime);
    }
}
