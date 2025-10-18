using UnityEngine;

public class BackGroundMover : MonoBehaviour
{
    Camera mainCamera;
    private float parallaxFactor = 0.6f; // �w�i�̒Ǐ]�X�s�[�h�i0�ɋ߂��قǂ�����蓮���j

    private float spriteWidth;
    private float startPosX;

    void Start()
    {
        mainCamera = Camera.main;

        startPosX = transform.position.x;
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void FixedUpdate()
    {
        if (mainCamera == null) return;

        Parallax();
    }

    private void Parallax()
    {
        float cameraTravel = mainCamera.transform.position.x;

        // �������ʂ�K�p�����ړ�����
        float dist = cameraTravel * parallaxFactor;

        transform.position = new Vector3(startPosX + dist, transform.position.y, transform.position.z);

        float temp = cameraTravel * (1 - parallaxFactor);

        // �w�i���[�v����
        if (temp > startPosX + spriteWidth)
        {
            startPosX += spriteWidth * 2f;
        }
        else if (temp < startPosX - spriteWidth)
        {
            startPosX -= spriteWidth * 2f;
        }
    }
}