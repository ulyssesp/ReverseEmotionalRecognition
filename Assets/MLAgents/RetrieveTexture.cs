using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;

public class RetrieveTexture : SideChannel
{
    private Agent mAgent;

    public RetrieveTexture()
    {
        mAgent = GameObject.FindGameObjectWithTag("agent").GetComponent<Agent>();
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        int res = msg.ReadInt32();
        if (res == 3)
        {
            mAgent.AddReward(1);
            mAgent.EndEpisode();
        }
    }
}
