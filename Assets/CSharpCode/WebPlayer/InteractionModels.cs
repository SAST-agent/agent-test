// InteractionModels.cs
using System;
using System.Collections.Generic;

[Serializable]
public class Step
{
    public int step_id;
    public string npc_id;
    public Interaction interaction;
    public ResultState result_state;
}

[Serializable]
public class Interaction
{
    public string ask_content;
    public List<string> submit_evidence_id;
    public string npc_reply; // legacy
}

[Serializable]
public class ResultState
{
    public int stage;
    public List<string> achievements;
    public List<string> testimony;
    public List<string> visible_npcs;
    public List<string> visible_evidences;
}
