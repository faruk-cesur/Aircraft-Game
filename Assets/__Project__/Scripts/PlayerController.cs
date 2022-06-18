using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [BoxGroup("Player Script Setup"), SerializeField] private Player _player;
    [BoxGroup("Player Script Setup"), SerializeField] private PlayerTrigger _playerTrigger;
    [BoxGroup("Controller Setup"), SerializeField] private Rigidbody _rigidbody;

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            _rigidbody.AddForce(Vector3.forward * _player.Speed * Time.deltaTime);
        }
    }
}