using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    public StoryController story;
    public ReplayController replay;

    private int stepCounter = 0;

    void Start()
    {
        ModeController.SwitchToPlay();
    }

    // ===== 玩家一次完整交互 =====
    public void PlayerInteract(
        string npcId,
        string askContent,
        List<string> submitEvidenceIds
    )
    {
        if (!ModeController.IsPlay())
        {
            Debug.Log("Not in Play Mode.");
            return;
        }

        Interaction interaction = new Interaction
        {
            ask_content = askContent,
            submit_evidence_id = submitEvidenceIds
        };

        ResultState result = story.ExecuteInteraction(npcId, interaction);

        Step step = new Step
        {
            step_id = stepCounter++,
            npc_id = npcId,
            interaction = interaction,
            result_state = result
        };

        replay.Record(step);
    }

    // ===== 测试用按钮 =====
    public void TestInteract()
    {
        Debug.Log("BUTTON CLICKED !!!");
        PlayerInteract(
            "DengDaLing",
            "你觉得 Rose 是个怎样的人？",
            new List<string> { "111", "112", "113" }
        );
    }

    public void TestReplay()
    {
        replay.ReplayAll();
    }
}
