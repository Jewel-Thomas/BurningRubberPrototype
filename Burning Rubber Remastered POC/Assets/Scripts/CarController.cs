using System;
using UnityEngine;

public class CarController : MonoBehaviour
{

    private Rigidbody playerRb;

    [SerializeField] private WheelColliders wheelColliders;
    [SerializeField] private WheelMeshes wheelMeshes;
    [SerializeField] private WheelSmoke wheelSmoke;
    [SerializeField] private Transform smokePrefabTransform;

    [SerializeField] private AnimationCurve speedVsAngleCurve;
    [SerializeField] private bool isReversing;
    [SerializeField] private float slipAllowance = 0.1f;

    private float gasInput;
    private float steeringInput;
    private float brakeInput;
    private float speed;
    private float wheelRadius;

    private enum DriveMode
    {
        Rear_Wheel_Drive,
        Front_Wheel_Drive,
        All_Wheel_Drive
    }


    [SerializeField] private float motorPower;
    [SerializeField] private float brakePower;
    [SerializeField] private DriveMode driveMode;

    // Debug
    [SerializeField] private float frontLeftSlip;
    [SerializeField] private float frontRightSlip;
    [SerializeField] private float rearLeftSlip;
    [SerializeField] private float rearRightSlip;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        wheelRadius = wheelColliders.frontLeftWheelCollider.radius;
        InstantiateSmoke();
    }

    private void Update()
    {
        speed = playerRb.velocity.magnitude;
        GetInput();
        isReversing = IsReversing();
        Accelerate();
        Steer();
        Brake();
        CheckWheelSkid();
        UpdateAllWheels();
    }

    // TODO: Get input using the new Input System. Doing this only for initiating functionality.
    private void GetInput()
    {
        gasInput = GameInput.Instance.CarMovementInputNormalized().y;
        steeringInput = GameInput.Instance.CarMovementInputNormalized().x;
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
            else
            {
                brakeInput = 0;
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

    private void CheckWheelSkid()
    {
        ApplySmoke(wheelColliders.frontLeftWheelCollider, wheelSmoke.frontLeftSmoke, slipAllowance);
        ApplySmoke(wheelColliders.frontRightWheelCollider, wheelSmoke.frontRightSmoke, slipAllowance);
        ApplySmoke(wheelColliders.rearLeftWheelCollider, wheelSmoke.rearLeftSmoke, slipAllowance);
        ApplySmoke(wheelColliders.rearRightWheelCollider, wheelSmoke.rearRightSmoke, slipAllowance);
    }


    private void ApplySmoke(WheelCollider wheelCollider, ParticleSystem wheelSmoke, float slipAllowance)
    {
        if(wheelCollider.GetGroundHit(out WheelHit wheelHit))
        {
            if (Mathf.Abs(wheelHit.sidewaysSlip) + Mathf.Abs(wheelHit.forwardSlip) > slipAllowance)
            {
                if (!wheelSmoke.isPlaying)
                    wheelSmoke.Play();
            }
            else
            {
                if (wheelSmoke.isPlaying)
                    wheelSmoke.Stop();
            }
        }
        else
        {
            if (wheelSmoke.isPlaying)
                wheelSmoke.Stop();
        }
    }

    private void UpdateAllWheels()
    {
        UpdateWheel(wheelColliders.frontLeftWheelCollider, wheelMeshes.frontLeftWheelMesh);
        UpdateWheel(wheelColliders.frontRightWheelCollider, wheelMeshes.frontRightWheelMesh);
        UpdateWheel(wheelColliders.rearLeftWheelCollider, wheelMeshes.rearLeftWheelMesh);
        UpdateWheel(wheelColliders.rearRightWheelCollider, wheelMeshes.rearRightWheelMesh);
    }

    private void InstantiateSmoke()
    {
        wheelSmoke.frontLeftSmoke = Instantiate(smokePrefabTransform.gameObject, wheelColliders.frontLeftWheelCollider.transform.position - Vector3.up*wheelRadius,
            Quaternion.identity, wheelColliders.frontLeftWheelCollider.transform).GetComponent<ParticleSystem>();
        wheelSmoke.frontRightSmoke = Instantiate(smokePrefabTransform.gameObject, wheelColliders.frontRightWheelCollider.transform.position - Vector3.up * wheelRadius,
            Quaternion.identity, wheelColliders.frontRightWheelCollider.transform).GetComponent<ParticleSystem>();
        wheelSmoke.rearLeftSmoke = Instantiate(smokePrefabTransform.gameObject, wheelColliders.rearLeftWheelCollider.transform.position - Vector3.up * wheelRadius,
            Quaternion.identity, wheelColliders.rearLeftWheelCollider.transform).GetComponent<ParticleSystem>();
        wheelSmoke.rearRightSmoke = Instantiate(smokePrefabTransform.gameObject, wheelColliders.rearRightWheelCollider.transform.position - Vector3.up * wheelRadius,
            Quaternion.identity, wheelColliders.rearRightWheelCollider.transform).GetComponent<ParticleSystem>();
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
public class WheelSmoke
{
    public ParticleSystem frontLeftSmoke;
    public ParticleSystem frontRightSmoke;
    public ParticleSystem rearLeftSmoke;
    public ParticleSystem rearRightSmoke;
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
