using System.Collections.Generic;
using UnityEngine;

public class ReplayController : MonoBehaviour
{
    public StoryController story;

    private List<Step> steps = new List<Step>();
    private int cursor = 0;

    public void Record(Step step)
    {
        if (!ModeController.IsPlay()) return;

        steps.Add(step);
        Debug.Log($"[Replay] Record Step {step.step_id}");
    }

    public void ReplayAll()
    {
        if (steps.Count == 0)
        {
            Debug.Log("No steps to replay.");
            return;
        }

        Debug.Log("=== Start Replay ===");
        ModeController.SwitchToReplay();
        cursor = 0;

        Debug.Log("=== Replay End ===");
    }

    public void ReplayStep(int index)
    {
        if (index < 0 || index >= steps.Count) return;

        ModeController.SwitchToReplay();
        cursor = index;

        Step step = steps[cursor];
        Debug.Log($"[Replay] Apply Step {step.step_id}");

        //  注意：ReplayController 不直接操作 Story
        FindObjectOfType<InteractionController>().PlayerInteract(
            step.npc_id,
            step.interaction.ask_content,
            step.interaction.submit_evidence_id
        );
    }
}
