using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour
{
    public GameObject HintBubble;        // NPC 头上的问号气泡 Canvas
    public GameObject HintInteraction;   // “按键交互”提示

    // 监测玩家是否在交互中
    private isInteraction i;

    private bool playerInRange = false;

    void Start()
    {
        GameObject player = GameObject.Find("PF Player");
        i = player.GetComponent<isInteraction>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (!i.getIsTalk())
                HintInteraction.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HintInteraction.SetActive(false);
        }
    }

    public bool getPlayerInRange()
    {
        return playerInRange;
    }

    // =================================================
    // 由 NpcSumBubble 调用
    // =================================================
    public void RefreshBubbleVisibility(bool isVisible)
    {
        if (HintBubble != null)
            HintBubble.SetActive(isVisible);
    }
}
