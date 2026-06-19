using System;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform target;
    public float lerpSpeed = 20;
    public float distanceFromTarget = 10;
    [Range(0,90)] public float maxRotX = 45;
    [Range(-90,0)]public float minRotX = -45;

    float rx = 0;
    float ry = 0;

    void Start()
    {
        transform.localPosition = new Vector3(0,0,-distanceFromTarget);
    }


    void Update()
    {
        Vector2 lookInput = Game.controls.Player.Look.ReadValue<Vector2>();
        rx += lookInput.y * Game.aimSense * Time.deltaTime;
        rx = Mathf.Clamp(rx,minRotX,maxRotX);
        ry += lookInput.x * Game.aimSense * Time.deltaTime;
        target.transform.localEulerAngles = new Vector3(rx,ry,0);
    }
}
