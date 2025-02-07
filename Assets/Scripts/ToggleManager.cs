using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleManager : MonoBehaviour
{
    // A struct to pair each toggle with a game object
    [System.Serializable]
    public struct TogglePair
    {
        public Toggle toggle;          // The UI toggle
        public GameObject targetObject; // The game object to activate/deactivate
    }

    // List of pairs to assign in the Inspector
    public List<TogglePair> togglePairs;

    private void Start()
    {
        // Loop through each pair to initialize the toggle listeners
        foreach (var pair in togglePairs)
        {
            if (pair.toggle != null && pair.targetObject != null)
            {
                // Initialize the game object's active state to match the toggle's current state
                pair.targetObject.SetActive(pair.toggle.isOn);

                // Add listener to handle toggle state changes
                pair.toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(pair.toggle, isOn));
            }
        }
    }

    // Method to handle the toggle state change
    private void OnToggleValueChanged(Toggle changedToggle, bool isOn)
    {
        // Find the associated GameObject and set its active state
        foreach (var pair in togglePairs)
        {
            if (pair.toggle == changedToggle)
            {
                pair.targetObject.SetActive(isOn);
                break;
            }
        }
    }

    private void OnDestroy()
    {
        // Remove listeners when the script is destroyed to avoid memory leaks
        foreach (var pair in togglePairs)
        {
            if (pair.toggle != null)
            {
                pair.toggle.onValueChanged.RemoveListener((isOn) => OnToggleValueChanged(pair.toggle, isOn));
            }
        }
    }
}
