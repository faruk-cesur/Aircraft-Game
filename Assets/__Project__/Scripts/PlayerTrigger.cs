using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Transform _aircraftModel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            _player.CheckpointList.Remove(other.gameObject);
            Destroy(other.gameObject);
            GameManager.Instance.CollectGoldAnimation();
            _player.CheckpointParticle.Play();
            _player.CheckpointScore += 100;
            _player.CheckpointSound.Play();
            if (_player.CheckpointList.Count > 0)
            {
                _player.CheckpointList[0].SetActive(true);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain") && gameObject.CompareTag("Player") && !GameManager.Instance.IsGameEnded)
        {
            GameManager.Instance.IsGameEnded = true;
            gameObject.tag = "Untagged";
            _aircraftModel.gameObject.SetActive(false);
            _playerController.ControllerType = PlayerController.ControllerTypes.Autopilot;
            _playerController.GameEnded();
            _player.CrashParticle.Play();
            _player.CrashParticle.GetComponent<AudioSource>().Play();
            _player.PlayerUI.SetActive(false);
            GameManager.Instance.GameOver();
            _player.JetSound.Stop();
        }
    }
}