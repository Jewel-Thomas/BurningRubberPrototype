using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private InputActions inputActions;
    private Vector2 inputVector;
    private float smoothTime = 10;
    private bool isClutchApplied;

    private enum InputSystemType
    {
        OLD_INPUT_SYSTEM,
        NEW_INPUT_SYSTEM
    }

    [SerializeField] private InputSystemType inputSystemType;

    private void Awake()
    {
        Instance = this;
        inputActions = new InputActions();

        inputActions.Car.Enable();
    }

    private void Update()
    {
        UpdateClutchApplied();
    }

    public void UpdateClutchApplied()
    {
        isClutchApplied = inputActions.Car.Clutch.IsPressed();
    }

    public bool IsClutchApplied()
    {
        return isClutchApplied;
    }

    public Vector2 CarMovementInputNormalized()
    {
        switch(inputSystemType)
        {
            default:
            case InputSystemType.OLD_INPUT_SYSTEM:
                float verticalInput = Input.GetAxis("Vertical");
                float horizontalInput = Input.GetAxis("Horizontal");
                inputVector = new Vector2(horizontalInput, verticalInput);
                return inputVector;
            case InputSystemType.NEW_INPUT_SYSTEM:
                Vector2 inputVectorRaw = inputActions.Car.Move.ReadValue<Vector2>();
                inputVector = Vector2.Lerp(inputVector, inputVectorRaw, Time.deltaTime * smoothTime);
                if (inputVector.sqrMagnitude < 0.0001f)
                    inputVector = Vector2.zero;
                return inputVector;
        }
    }
}
