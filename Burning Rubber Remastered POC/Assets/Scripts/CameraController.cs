using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float speed;

    private Rigidbody playerRb;

    private void Awake()
    {
        playerRb = player.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        FocusPlayer();
    }

    private void FocusPlayer()
    {
        Vector3 playerForward = (playerRb.velocity + player.forward).normalized;
        transform.position = Vector3.Lerp(transform.position,
            player.position + player.TransformVector(offset) +
            playerForward * (-5f),
            speed * Time.deltaTime);
        transform.LookAt(player);
    }



}
