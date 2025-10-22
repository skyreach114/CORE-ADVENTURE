using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private PlayerController pc;
    private Vector3 initPos;

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            pc = GameManager.Instance.playerController;
        }

        initPos = transform.position;
    }

    void LateUpdate()
    {
        if (pc != null)
        {
            FollowPlayer();
        }
    }

    private void FollowPlayer()
    {
        float x = pc.transform.position.x;
        x = Mathf.Clamp(x, initPos.x, Mathf.Infinity);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}
