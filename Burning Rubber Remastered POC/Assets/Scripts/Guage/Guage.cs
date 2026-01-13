using UnityEngine;
using TMPro;

public class Guage : MonoBehaviour
{
    [SerializeField] private GameObject needleObject;
    [SerializeField] private float minNeedleAngle;
    [SerializeField] private float maxNeedleAngle;

    /// <summary>
    ///     Rotates the gauge needle based on a value in range [0, maxMeasureValue].
    /// </summary>
    /// <param name="currentMeasureValue"></param>
    /// <param name="maxMeasureValue"></param>
    /// <param name="valueText"></param>
    public void SetNeedleRotation(float currentMeasureValue, float maxMeasureValue, TextMeshProUGUI valueText = null)
    {
        float lerpFactor = Mathf.Clamp01(currentMeasureValue / maxMeasureValue);
        needleObject.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(minNeedleAngle, maxNeedleAngle, lerpFactor));
        if (valueText) UpdateGuageValue(valueText, currentMeasureValue);
    }

    private void UpdateGuageValue(TextMeshProUGUI valueText, float currentValue)
    {
        valueText.text = Mathf.RoundToInt(currentValue).ToString("0,000");
    }
}
