using System;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private WheelColliders wheelColliders;
    [SerializeField] private WheelMeshes wheelMeshes;

    private void Update()
    {
        UpdateAllWheels();
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
