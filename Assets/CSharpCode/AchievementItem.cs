using TMPro;
using UnityEngine;

public class AchievementItem : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    public void SetData(string name, string description, bool unlocked)
    {
        if (unlocked)
        {
            titleText.text = name;
            descriptionText.text = description;
        }
        else
        {
            titleText.text = "Î´½âËø";
            descriptionText.text = "£¿£¿£¿";
        }
    }
}
