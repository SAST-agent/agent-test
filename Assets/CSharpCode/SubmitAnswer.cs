using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using TMPro;

public class SubmitAnswer : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;
    private isInteraction interaction;

    [Header("Answer Panel")]
    public GameObject answerPanel;

    [Header("Input Fields")]
    public TMP_InputField murdererInput;    // 凶手
    public TMP_InputField motivationInput;  // 动机
    public TMP_InputField methodInput;      // 手段

    [Header("Submit Logic")]
    public AnswerSubmitController answerSubmitController;

    private void Start()
    {
        if (player != null)
            interaction = player.GetComponent<isInteraction>();
    }

    /// <summary>
    /// 绑定在 SubmitButton 上
    /// </summary>
    public void Submit()
    {
        Debug.Log("SubmitAnswer() CALLED");
        // 1. 读取三个输入框
        string murderer = murdererInput.text;
        string motivation = motivationInput.text;
        string method = methodInput.text;

        Debug.Log($"[SubmitAnswer] murderer={murderer}, motivation={motivation}, method={method}");

        // 2. 传给真正的提交器
        answerSubmitController.selectedMurderer = murderer;
        answerSubmitController.motivationText = motivation;
        answerSubmitController.methodText = method;

        answerSubmitController.SubmitAnswer();

        // 3. UI & 状态收尾
        answerPanel.SetActive(false);

        if (interaction != null)
            interaction.changeIsSubmit(true);

        EventSystem.current.SetSelectedGameObject(null);
    }
}