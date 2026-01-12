using System;
using UnityEngine;
using Random = UnityEngine.Random;

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

    [SerializeField] private GameObject steeringWheel;

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
    private WheelCollider[] rearWheels;
    private WheelCollider[] frontWheels;
    private WheelCollider[] allWheels;

    // Debug
    [SerializeField] private float frontLeftSlip;
    [SerializeField] private float frontRightSlip;
    [SerializeField] private float rearLeftSlip;
    [SerializeField] private float rearRightSlip;


    [Space]
    [Header("Work In Progress")]
    // RPM
    [SerializeField] private float rPM;
    [SerializeField] private float idleRPM;
    [SerializeField] private float maxRPM;
    [SerializeField] private RPMGuage rpmGuage;
    [SerializeField] private float[] gearRatios;
    [SerializeField] private int currentGear;
    [SerializeField] private float differentialRatio;
    [SerializeField] private float currentTorque;
    [SerializeField] private float clutch;
    [SerializeField] private float wheelRPM;
    [SerializeField] private AnimationCurve hpToRPMCurve;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();

        rearWheels = new WheelCollider[]
        {
            wheelColliders.rearLeftWheelCollider,
            wheelColliders.rearRightWheelCollider
        };

        frontWheels = new WheelCollider[]
        {
            wheelColliders.frontLeftWheelCollider,
            wheelColliders.frontRightWheelCollider
        };

        allWheels = new WheelCollider[]
        {
            wheelColliders.frontLeftWheelCollider,
            wheelColliders.frontRightWheelCollider,
            wheelColliders.rearLeftWheelCollider,
            wheelColliders.rearRightWheelCollider
        };
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
        GetClutchValue();
        rpmGuage.UpdateGuageVisual(rPM, maxRPM);
        Accelerate();
        Steer();
        Brake();
        CheckWheelSkid();
        UpdateAllWheels();
    }

    private void GetInput()
    {
        gasInput = GameInput.Instance.CarMovementInputNormalized().y;
        steeringInput = GameInput.Instance.CarMovementInputNormalized().x;
    }

    private void GetClutchValue()
    {
        clutch = GameInput.Instance.IsClutchApplied() ? 0 :
                 clutch = Mathf.Lerp(clutch, 1, Time.deltaTime);
    }

    private void Accelerate()
    {
        switch (driveMode)
        {
            default:
            case DriveMode.Rear_Wheel_Drive:
                AccelerateWheel(rearWheels);
                break;
            case DriveMode.Front_Wheel_Drive:
                AccelerateWheel(frontWheels);
                break;
            case DriveMode.All_Wheel_Drive:
                AccelerateWheel(allWheels);
                break;
        }
    }

    private void AccelerateWheel(WheelCollider[] wheelColliders)
    {
        currentTorque = CalculateTorque(wheelColliders);
        foreach (WheelCollider wheelCollider in wheelColliders)
        {
            wheelCollider.motorTorque = currentTorque * gasInput;
        }
    }

    private float CalculateTorque(WheelCollider[] wheelColliders)
    {
        float torque = 0;
        // TODO : Implement engine running check.
        if(clutch < 0.1f)
        {
            rPM = Mathf.Lerp(rPM, Mathf.Max(idleRPM, maxRPM * gasInput + Random.Range(-50, 50)), Time.deltaTime);
        }
        else
        {
            float sumWheelRPM = 0;
            foreach(WheelCollider wheelCollider in wheelColliders)
            {
                sumWheelRPM += wheelCollider.rpm;
            }
            wheelRPM = Mathf.Abs(sumWheelRPM / wheelColliders.Length) * gearRatios[currentGear] * differentialRatio;
            rPM = Mathf.Lerp(rPM, Mathf.Max(idleRPM - 100, wheelRPM), Time.deltaTime * 3);
        }

        torque = (hpToRPMCurve.Evaluate(rPM / maxRPM) * motorPower / rPM) *
                gearRatios[currentGear] * differentialRatio * 5252f * clutch;

        return torque;
    }

    private void Steer()
    {
        float steeringAngle = speedVsAngleCurve.Evaluate(speed) * steeringInput;
        wheelColliders.frontLeftWheelCollider.steerAngle = steeringAngle;
        wheelColliders.frontRightWheelCollider.steerAngle = steeringAngle;
        steeringWheel.transform.localRotation = Quaternion.Euler(0, steeringAngle, 0);
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
