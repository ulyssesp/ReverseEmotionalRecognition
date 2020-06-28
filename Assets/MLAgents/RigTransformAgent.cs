using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System.Linq;


public class RigTransformAgent : Agent
{
    public int Index;
    public int IndexRange;
    private Vector3 m_InitialPositionUnaltered;
    private Vector3 m_InitialPosition;
    private EmotionDetect m_EmotionReceiver;

    public float m_EmotionMult;

    private Vector3 MostEmotion;
    private float MostEmotionValue;
    private float LastEmotionValue;

    public override void Initialize()
    {
        transform.localPosition = Vector3.zero;
        m_InitialPositionUnaltered = transform.position;
        m_InitialPosition = m_InitialPositionUnaltered;
        m_InitialPosition.x = Mathf.Abs(m_InitialPosition.x * 50f);
        m_InitialPosition.y -= 1.7f;
        m_InitialPosition.z = (m_InitialPosition.z - 0.09f) * 50;

        MostEmotion = Vector3.zero;
        m_EmotionReceiver = GameObject.FindGameObjectWithTag("EmotionReceiver").GetComponent<EmotionDetect>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(m_EmotionTargets);
        sensor.AddOneHotObservation(Index, IndexRange);
        sensor.AddObservation(Mathf.Abs(m_InitialPosition.x));
        sensor.AddObservation(m_InitialPosition.y);
        sensor.AddObservation(m_InitialPosition.z);

        sensor.AddObservation(transform.localPosition.x * 40f);
        sensor.AddObservation(transform.localPosition.y * 40f);
        sensor.AddObservation(transform.localPosition.z * 40f);

        // TODO: Add more emotion detection stuff
        sensor.AddObservation(m_EmotionReceiver.Happy);
        sensor.AddObservation(m_EmotionReceiver.Sad);
        sensor.AddObservation(m_EmotionReceiver.Angry);
        sensor.AddObservation(m_EmotionReceiver.Disgusted);
        sensor.AddObservation(m_EmotionReceiver.Fearful);
        sensor.AddObservation(m_EmotionReceiver.Neutral);
        sensor.AddObservation(m_EmotionReceiver.Surprised);
        sensor.AddObservation(m_EmotionReceiver.Contempt);
    }

    public override void OnActionReceived(float[] act)
    {
        Vector3 position = transform.localPosition;

        position.x = Mathf.Clamp(position.x + 0.001f * Mathf.Clamp(act[0], -1, 1), -0.025f, 0.025f);
        position.y = Mathf.Clamp(position.y + 0.002f * Mathf.Clamp(act[1], -1, 1), -0.025f, 0.025f);
        position.z = Mathf.Clamp(position.z + 0.001f * Mathf.Clamp(act[2], -1, 1), -0.025f, 0.025f);

        // switch ((int)act[0])
        // {
        //     case 1:
        //         position.x = Mathf.Clamp(position.x + 0.0005f, -0.025f, 0.025f);
        //         break;
        //     case 2:
        //         position.x = Mathf.Clamp(position.x - 0.0005f, -0.025f, 0.025f);
        //         break;
        // }

        // switch ((int)act[1])
        // {
        //     case 1:
        //         position.y = Mathf.Clamp(position.y + 0.002f, -0.025f, 0.025f);
        //         break;
        //     case 2:
        //         position.y = Mathf.Clamp(position.y - 0.002f, -0.025f, 0.025f);
        //         break;
        // }

        // switch ((int)act[2])
        // {
        //     case 1:
        //         position.y = Mathf.Clamp(position.z + 0.001f, -0.025f, 0.025f);
        //         break;
        //     case 2:
        //         position.y = Mathf.Clamp(position.z - 0.001f, -0.025f, 0.025f);
        //         break;
        // }

        // nsform.localPosition = position * (act[0] + act[1] + act[2] == 0 ? 0.98f : 1f);
        transform.localPosition = position;


        // Rewards
        if (MaxStep > 0)
        {
            List<float> arr = new List<float>{
            m_EmotionReceiver.Happy
            , m_EmotionReceiver.Sad
            , m_EmotionReceiver.Angry
            , m_EmotionReceiver.Disgusted
            , m_EmotionReceiver.Fearful
            , m_EmotionReceiver.Neutral
            , m_EmotionReceiver.Surprised
            , m_EmotionReceiver.Contempt
        };

            int emotionTarget = Mathf.FloorToInt(Academy.Instance.EnvironmentParameters.GetWithDefault("emotion", -1));
            float emotionTargetValue = arr[emotionTarget];

            // for (int i = 0; i < 8; i++)
            // {
            //     AddReward(arr[i] * (i == emotionTarget ? Academy.Instance.EnvironmentParameters.GetWithDefault("emotion-reward", 1) : -0.25f) / (float)MaxStep);
            // }

            AddReward((emotionTargetValue - LastEmotionValue > 0.002f ? 1f : -0.25f) / (float)MaxStep);
            LastEmotionValue = emotionTargetValue;

            if (emotionTargetValue > MostEmotionValue)
            {
                MostEmotionValue = emotionTargetValue;
                MostEmotion = transform.localPosition;
                AddReward(0.5f);
            }

            AddReward(emotionTargetValue * Academy.Instance.EnvironmentParameters.GetWithDefault("emotion-reward", 0) / (float)MaxStep);

            // Vector3 distanceToNeutral = transform.localPosition - MostEmotion;
            // AddReward(Mathf.Abs(distanceToNeutral.x) * 40f * -0.125f / (float)MaxStep);
            // AddReward(Mathf.Abs(distanceToNeutral.y) * 40f * -0.0675f / (float)MaxStep);
            // AddReward(Mathf.Abs(distanceToNeutral.z) * 40f * -0.0675f / (float)MaxStep);
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(
            0,
            Random.Range(-0.005f, 0.005f) + MostEmotion.y,
            Random.Range(-0.005f, 0.005f) + MostEmotion.z
        );
        // int emotionTarget = Mathf.FloorToInt(Academy.Instance.EnvironmentParameters.GetWithDefault("emotion", -1));
        // int emotionTargetTwo = Mathf.FloorToInt(Academy.Instance.EnvironmentParameters.GetWithDefault("emotion-two", -1));
        // for (int i = 0; i < m_EmotionTargets.Count; i++)
        // {
        //     if (i == emotionTarget || i == emotionTargetTwo)
        //     {
        //         m_EmotionTargets[i] = 1;
        //     }
        //     else
        //     {
        //         m_EmotionTargets[i] = 0;
        //     }
        // }
    }
}