using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
public class EmotionReceiver : MonoBehaviour
{
    private OSC m_osc;

    public float Angry = 0;
    public float Disgusted = 0;
    public float Fearful = 0;
    public float Happy = 0;
    public float Neutral = 0;
    public float Sad = 0;
    public float Surprised = 0;
    public float Contempt = 0;

    public void Start()
    {
        m_osc = GetComponent<OSC>();
        m_osc.SetAddressHandler("/angry", OnReceiveAngry);
        m_osc.SetAddressHandler("/disgusted", OnReceiveDisgusted);
        m_osc.SetAddressHandler("/fearful", OnReceiveFearful);
        m_osc.SetAddressHandler("/happy", OnReceiveHappy);
        m_osc.SetAddressHandler("/neutral", OnReceiveNeutral);
        m_osc.SetAddressHandler("/sad", OnReceiveSad);
        m_osc.SetAddressHandler("/surprised", OnReceiveSurprised);
        m_osc.SetAddressHandler("/contempt", OnReceiveSurprised);
    }
    private void OnReceiveFearful(OscMessage message)
    {
        Fearful = message.GetFloat(0);
    }

    private void OnReceiveDisgusted(OscMessage message)
    {
        Disgusted = message.GetFloat(0);
    }
    private void OnReceiveNeutral(OscMessage message)
    {
        Neutral = message.GetFloat(0);
    }
    private void OnReceiveSurprised(OscMessage message)
    {
        Surprised = message.GetFloat(0);
    }

    private void OnReceiveAngry(OscMessage message)
    {
        Angry = message.GetFloat(0);
    }

    private void OnReceiveHappy(OscMessage message)
    {
        Happy = message.GetFloat(0);
    }


    private void OnReceiveSad(OscMessage message)
    {
        Sad = message.GetFloat(0);
    }

}