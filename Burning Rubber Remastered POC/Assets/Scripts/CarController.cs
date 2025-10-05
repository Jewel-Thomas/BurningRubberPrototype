using System;
using UnityEngine;

public class CarController : MonoBehaviour
{

    private Rigidbody playerRb;

    [SerializeField] private WheelColliders wheelColliders;
    [SerializeField] private WheelMeshes wheelMeshes;

    [SerializeField] private float gasInput;
    [SerializeField] private float steeringInput;
    [SerializeField] private float brakeInput;
    [SerializeField] private AnimationCurve speedVsAngleCurve;
    [SerializeField] private bool isReversing;

    private float speed;

    private enum DriveMode
    {
        Rear_Wheel_Drive,
        Front_Wheel_Drive,
        All_Wheel_Drive
    }


    [SerializeField] private float motorPower;
    [SerializeField] private float brakePower;
    [SerializeField] private DriveMode driveMode;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        speed = playerRb.velocity.magnitude;
        GetInput();
        isReversing = IsReversing();
        Accelerate();
        Steer();
        Brake();
        UpdateAllWheels();
    }


    // TODO: Get input using the new Input System. Doing this only for initiating functionality.
    private void GetInput()
    {
        gasInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
    }

    private void Accelerate()
    {
        switch(driveMode)
        {
            default:
            case DriveMode.Rear_Wheel_Drive:
                AccelerateWheel(wheelColliders.rearLeftWheelCollider);
                AccelerateWheel(wheelColliders.rearRightWheelCollider);
                break;
            case DriveMode.Front_Wheel_Drive:
                AccelerateWheel(wheelColliders.frontLeftWheelCollider);
                AccelerateWheel(wheelColliders.frontRightWheelCollider);
                break;
            case DriveMode.All_Wheel_Drive:
                AccelerateWheel(wheelColliders.rearLeftWheelCollider);
                AccelerateWheel(wheelColliders.rearRightWheelCollider);
                AccelerateWheel(wheelColliders.frontLeftWheelCollider);
                AccelerateWheel(wheelColliders.frontRightWheelCollider);
                break;
        }
    }

    private void AccelerateWheel(WheelCollider wheelCollider)
    {
        wheelCollider.motorTorque = motorPower * gasInput;
    }

    private void Steer()
    {
        float steeringAngle = speedVsAngleCurve.Evaluate(speed) * steeringInput;
        wheelColliders.frontLeftWheelCollider.steerAngle = steeringAngle;
        wheelColliders.frontRightWheelCollider.steerAngle = steeringAngle;
    }

    private bool IsReversing()
    {
        float slipAngle = Vector3.Angle(transform.forward, playerRb.velocity - transform.forward);
        if(slipAngle < 120)
        {
            if(gasInput < 0)
            {
                brakeInput = Mathf.Abs(gasInput);
                gasInput = 0;
            }
            return false;
        }
        else
        {
            brakeInput = 0;
            return true;
        }
    }

    private void Brake()
    {
        wheelColliders.frontLeftWheelCollider.brakeTorque = brakeInput * brakePower * 0.7f;
        wheelColliders.frontRightWheelCollider.brakeTorque = brakeInput * brakePower * 0.7f;
        wheelColliders.rearLeftWheelCollider.brakeTorque = brakeInput * brakePower * 0.3f;
        wheelColliders.rearRightWheelCollider.brakeTorque = brakeInput * brakePower * 0.3f;
    }

    private void UpdateAllWheels()
    {
        UpdateWheel(wheelColliders.frontLeftWheelCollider, wheelMeshes.frontLeftWheelMesh);
        UpdateWheel(wheelColliders.frontRightWheelCollider, wheelMeshes.frontRightWheelMesh);
        UpdateWheel(wheelColliders.rearLeftWheelCollider, wheelMeshes.rearLeftWheelMesh);
        UpdateWheel(wheelColliders.rearRightWheelCollider, wheelMeshes.rearRightWheelMesh);
    }

    private void UpdateWheel(WheelCollider coll, MeshRenderer mesh)
    {
        Vector3 pos;
        Quaternion quat;

        coll.GetWorldPose(out pos, out quat);
        mesh.transform.position = pos;
        mesh.transform.rotation = quat;
    }
}

[Serializable]
public class WheelColliders
{
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;
}

[Serializable]
public class WheelMeshes
{
    public MeshRenderer frontLeftWheelMesh;
    public MeshRenderer frontRightWheelMesh;
    public MeshRenderer rearLeftWheelMesh;
    public MeshRenderer rearRightWheelMesh;
}
