using TMPro;
using UnityEngine;

public class TestimonyItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;

    public void SetData(string title, string content)
    {
        titleText.text = title;
        bodyText.text = content;
    }
}
