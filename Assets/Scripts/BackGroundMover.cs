using UnityEngine;

public class BackGroundMover : MonoBehaviour
{
    Camera mainCamera;
    private float parallaxFactor = 0.6f; // 背景の追従スピード（0に近いほどゆっくり動く）

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

        // 視差効果を適用した移動距離
        float dist = cameraTravel * parallaxFactor;

        transform.position = new Vector3(startPosX + dist, transform.position.y, transform.position.z);

        float temp = cameraTravel * (1 - parallaxFactor);

        // 背景ループ処理
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