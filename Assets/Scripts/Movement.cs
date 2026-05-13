using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 5f;
    public float sensitivity = 2f; // How fast the camera rotates
    public float clampAngle = 80f; // Limits the up and down rotation

    private float rotationX = 0f; // Current X rotation (up/down)
    private float rotationY = 0f; // Current Y rotation (left/right)
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        rotationY += mouseX * sensitivity;
        rotationX -= mouseY * sensitivity;
        rotationX = Mathf.Clamp(rotationX, -clampAngle, clampAngle);
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
    }
}