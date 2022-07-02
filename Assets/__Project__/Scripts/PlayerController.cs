using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public enum ControllerTypes
    {
        OnWheels,
        Flying,
        Autopilot
    }

    [ReadOnly] public ControllerTypes ControllerType;
    [BoxGroup("Player Script Setup"), SerializeField] private Player _player;

    [BoxGroup("Controller Setup"), SerializeField] private Rigidbody _rigidbody;
    [BoxGroup("Controller Setup"), SerializeField] private Slider _acceleratorPedal;
    [BoxGroup("Controller Setup"), SerializeField] private FloatingJoystick _floatingJoystick;
    [BoxGroup("Controller Setup"), SerializeField] private Transform _aircraftModel;

    private RaycastHit _hitGround;
    private bool _isGravityActive;

    private void FixedUpdate()
    {
        switch (ControllerType)
        {
            case ControllerTypes.OnWheels:
                TakeOffAircraft();
                break;
            case ControllerTypes.Flying:
                FlyingAircraft();
                break;
            case ControllerTypes.Autopilot:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Update()
    {
        switch (ControllerType)
        {
            case ControllerTypes.OnWheels:
                AccelerationPedal();
                RotateAircraftOnWheels();
                Gravity();
                RaycastGround();
                break;
            case ControllerTypes.Flying:
                AccelerationPedal();
                Gravity();
                RotateAircraftFlying();
                PreventAircraftFlip();
                RaycastGround();
                break;
            case ControllerTypes.Autopilot:
                RaycastGround();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void PulledUpPedal()
    {
        GameManager.Instance.Gameplay();
        if (!_player.JetSound.isPlaying)
        {
            _player.JetSound.Play();
        }
    }

    private void AccelerationPedal()
    {
        if (_player.WheelSpeed < 0)
        {
            _player.WheelSpeed = 0;
            _player.JetSound.Stop();
        }
        else
        {
            if (_player.WheelSpeed < 2000)
            {
                _player.WheelSpeed += _acceleratorPedal.value * _player.MultiplySpeedPerSecond * Time.deltaTime;
            }
            else
            {
                _player.WheelSpeed = 2000f + _acceleratorPedal.value;
            }
        }


        if (_player.FlyingSpeed < 0)
        {
            _player.FlyingSpeed = 0;
            _player.JetSound.Stop();
        }
        else
        {
            if (_player.FlyingSpeed < 3000)
            {
                _player.FlyingSpeed += _acceleratorPedal.value * _player.MultiplySpeedPerSecond * Time.deltaTime;
            }
            else
            {
                _player.FlyingSpeed = 3000f + _acceleratorPedal.value;
            }
        }
    }

    private void TakeOffAircraft()
    {
        if (_player.WheelSpeed > 1)
        {
            _rigidbody.velocity = Vector3.forward * (_player.WheelSpeed * Time.fixedDeltaTime);
        }

        if (_rigidbody.velocity.magnitude > _player.TakeOffSpeed)
        {
            ControllerType = ControllerTypes.Flying;
        }
    }

    private void FlyingAircraft()
    {
        if (_player.FlyingSpeed > 1)
        {
            _rigidbody.velocity = transform.forward * (_player.FlyingSpeed * Time.fixedDeltaTime);
        }
    }

    private void RotateAircraftFlying()
    {
        transform.Rotate(((Vector3.up * (_floatingJoystick.Direction.x)) + (-Vector3.right * _floatingJoystick.Direction.y)) * _player.RotateSpeed); // Rotating Parent Object According To Joystick
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0), Time.deltaTime); // Rotation.Z is resetting smoothly for parent object
        _aircraftModel.Rotate((-Vector3.forward * _floatingJoystick.Direction.x));
        _aircraftModel.localRotation = Quaternion.Lerp(_aircraftModel.localRotation, Quaternion.Euler(_aircraftModel.localEulerAngles.x, _aircraftModel.localEulerAngles.y, 0), Time.deltaTime); // Rotation.Z is resetting smoothly
    }

    private void RotateAircraftOnWheels()
    {
        transform.Rotate(-Vector3.right * (_floatingJoystick.Direction.y * _player.RotateSpeed)); // Rotating Parent Object According To Joystick
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.eulerAngles.y, 0), Time.deltaTime * _player.RotateSpeed * 2); // Rotation.Z is resetting smoothly for parent object
    }

    private void PreventAircraftFlip()
    {
        // Fixed Rotation.Z for parent object
        if (transform.eulerAngles.z > 45 && transform.eulerAngles.z < 180)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 45);
        }

        if (transform.eulerAngles.z < 315 && transform.eulerAngles.z > 180)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 315);
        }

        // Fixed Rotation.X for parent object
        if (transform.eulerAngles.x > 45 && transform.eulerAngles.x < 180)
        {
            transform.eulerAngles = new Vector3(45, transform.eulerAngles.y, transform.eulerAngles.z);
        }

        if (transform.eulerAngles.x < 315 && transform.eulerAngles.x > 180)
        {
            transform.eulerAngles = new Vector3(315, transform.eulerAngles.y, transform.eulerAngles.z);
        }

        // Fixed Rotation.Z for child object
        if (_aircraftModel.localEulerAngles.z > 45 && _aircraftModel.localEulerAngles.z < 180)
        {
            _aircraftModel.localEulerAngles = new Vector3(_aircraftModel.localEulerAngles.x, _aircraftModel.localEulerAngles.y, 45);
        }

        if (_aircraftModel.localEulerAngles.z < 315 && _aircraftModel.localEulerAngles.z > 180)
        {
            _aircraftModel.localEulerAngles = new Vector3(_aircraftModel.localEulerAngles.x, _aircraftModel.localEulerAngles.y, 315);
        }
    }

    private void Gravity()
    {
        if (_isGravityActive)
        {
            if (_player.FlyingSpeed < 500)
            {
                _player.GravitySpeedPerSecond += Time.deltaTime; // Time is passing
                transform.Translate((new Vector3(0, transform.up.y, -transform.forward.z) * -_player.GravitySpeedPerSecond) / 20f, Space.World); // Gravity Force by time
                _aircraftModel.localRotation = Quaternion.Lerp(_aircraftModel.localRotation, Quaternion.Euler(45, 0, 0), Time.deltaTime * 0.5f); // Aircraft is rotating down while falling
            }
            else
            {
                _aircraftModel.localRotation = Quaternion.Lerp(_aircraftModel.localRotation, Quaternion.Euler(10, 0, 0), Time.deltaTime * 0.5f); // Aircraft is rotating little down while flying forward
                _player.GravitySpeedPerSecond = 0; // Reset Timer
            }
        }
    }

    private void RaycastGround()
    {
        Physics.Raycast(transform.position + new Vector3(0, -0.5f, 0), transform.forward, out _hitGround, 5f, LayerMask.GetMask("Ground"));
        Physics.Raycast(transform.position + new Vector3(0, 1, 0), -transform.up, out _hitGround, 5f, LayerMask.GetMask("Ground"));

        if (_hitGround.transform)
        {
            _isGravityActive = false;
            //transform.position = Vector3.Lerp(transform.position,new Vector3(transform.position.x, _hitGround.transform.position.y, transform.position.z),Time.deltaTime * 2); // Get position same with road
            _aircraftModel.localRotation = Quaternion.Lerp(_aircraftModel.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 2); // Rotation.Z is resetting smoothly
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.eulerAngles.y, 0), Time.deltaTime * 2); // Rotation.Z is resetting smoothly for parent object
        }
        else
        {
            _isGravityActive = true;
        }
    }

    public void GameEnded()
    {
        _rigidbody.isKinematic = true;
    }
}