using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class XRCalibrationManager : MonoBehaviour
{
    public GameObject virtualDataEnvironment;  // Parent GameObject of the environment
    public GameObject virtualEnvironment;  // Parent GameObject of the environment
    public GameObject showcases;  // Parent GameObject of the environment

    // Input action references for joystick-based calibration with two modifiers
    public InputActionReference positionCalibrationAction;  // Left joystick with two modifiers for position calibration
    public InputActionReference rotationCalibrationAction;  // Right joystick with two modifiers for rotation calibration

    public Image uiBorder;                  // Reference to the UI image border for visual feedback
    public Color positionActiveColor = Color.green; // Color when position calibration is active
    public Color rotationActiveColor = Color.blue;  // Color when rotation calibration is active
    public Color inactiveColor = Color.gray;        // Default color when neither action is active

    public float positionSpeed = 0.1f;      // Speed multiplier for position adjustments
    public float rotationSpeed = 30f;       // Speed multiplier for rotation adjustments

    void Update()
    {
        if (virtualEnvironment != null)
        {
            // Check if position calibration action is triggered with modifiers active
            Vector2 positionInput = positionCalibrationAction.action.ReadValue<Vector2>();
            if (positionInput != Vector2.zero)  // Only apply movement if there is joystick input
            {
                // Set UI border color for position calibration
                SetBorderColor(positionActiveColor);

                Vector3 positionOffset = new Vector3(positionInput.x, 0, positionInput.y) * positionSpeed * Time.deltaTime;
                virtualEnvironment.transform.position += positionOffset;
                virtualDataEnvironment.transform.position += positionOffset;
                showcases.transform.position += positionOffset;
            }

            // Check if rotation calibration action is triggered with modifiers active
            Vector2 rotationInput = rotationCalibrationAction.action.ReadValue<Vector2>();
            if (rotationInput != Vector2.zero)  // Only apply rotation if there is joystick input
            {
                // Set UI border color for rotation calibration
                SetBorderColor(rotationActiveColor);

                float rotationAmount = rotationInput.x * rotationSpeed * Time.deltaTime;
                virtualEnvironment.transform.Rotate(Vector3.up, rotationAmount, Space.World);
                virtualDataEnvironment.transform.Rotate(Vector3.up, rotationAmount, Space.World);
                showcases.transform.Rotate(Vector3.up, rotationAmount, Space.World);
            }

            // Revert to inactive color if neither action has input
            if (positionInput == Vector2.zero && rotationInput == Vector2.zero)
            {
                SetBorderColor(inactiveColor);
            }
        }
    }

    // Method to set the UI border color
    private void SetBorderColor(Color color)
    {
        if (uiBorder != null)
        {
            uiBorder.color = color;
        }
    }
}
