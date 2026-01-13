using UnityEngine;
using TMPro;

public class RPMGuage : Guage
{
    [SerializeField] private TextMeshProUGUI rpmText = null;
    [SerializeField] private TextMeshProUGUI gearText = null;

    public void UpdateGuageVisual(float currentRPM, float maxRPM, int currentGear = 0)
    {
        SetNeedleRotation(currentRPM, maxRPM, rpmText);
        if (gearText) UpdateGearVisual(currentGear);
    }

    private void UpdateGearVisual(int currentGear)
    {
        gearText.text = (currentGear + 1).ToString();
    }
}
