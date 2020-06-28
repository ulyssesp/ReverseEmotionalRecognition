using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.Barracuda;


public class AllJointAgent : Agent
{
    public Transform Root;
    public EmotionDetect m_EmotionReceiver;
    private List<Transform> Joints;



    public override void Initialize()
    {
        Joints = FindRecursive(Root);

        // BehaviorParameters bp = transform.GetComponent<BehaviorParameters>();
        // bp.BrainParameters.VectorActionSpaceType = SpaceType.Continuous;
        // bp.BrainParameters.VectorActionSize = new int[] { Joints.Count * 3 };
        // bp.BrainParameters.VectorObservationSize = Joints.Count * 3 + 8;

        m_EmotionReceiver = GameObject.FindGameObjectWithTag("EmotionReceiver").GetComponent<EmotionDetect>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(m_EmotionReceiver.Happy);
        sensor.AddObservation(m_EmotionReceiver.Sad);
        sensor.AddObservation(m_EmotionReceiver.Angry);
        sensor.AddObservation(m_EmotionReceiver.Disgusted);
        sensor.AddObservation(m_EmotionReceiver.Fearful);
        sensor.AddObservation(m_EmotionReceiver.Neutral);
        sensor.AddObservation(m_EmotionReceiver.Surprised);
        sensor.AddObservation(m_EmotionReceiver.Contempt);

        foreach (Transform joint in Joints)
        {
            sensor.AddObservation(joint.localPosition.x * 40f);
            sensor.AddObservation(joint.localPosition.y * 40f);
            sensor.AddObservation(joint.localPosition.z * 40f);
        }
    }
    public override void OnActionReceived(float[] act)
    {
        Debug.Log(act.Length);
        for (int i = 0; i < act.Length / 3; i++)
        {
            Vector3 position = Joints[i].transform.localPosition;
            position.x = Mathf.Clamp(position.x + 0.001f * Mathf.Clamp(act[i * 3 + 0], -1, 1), -0.025f, 0.025f);
            position.y = Mathf.Clamp(position.y + 0.002f * Mathf.Clamp(act[i * 3 + 1], -1, 1), -0.025f, 0.025f);
            position.z = Mathf.Clamp(position.z + 0.001f * Mathf.Clamp(act[i * 3 + 2], -1, 1), -0.025f, 0.025f);
            Joints[i].transform.localPosition = position;
        }

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
        AddReward(emotionTargetValue * Academy.Instance.EnvironmentParameters.GetWithDefault("emotion-reward", 0) / (float)MaxStep);
    }

    static List<Transform> FindRecursive(Transform transform)
    {
        if (transform.childCount == 0)
        {
            if (transform.gameObject.name.Contains("Tongue") || transform.gameObject.name.Contains("Teeth"))
            {
                return new List<Transform>();
            }
            return new List<Transform>() { transform };
        }
        else
        {
            List<Transform> arr = new List<Transform>(transform.childCount);
            for (int i = 0; i < transform.childCount; i++)
            {
                arr.AddRange(FindRecursive(transform.GetChild(i)));
            }

            return arr;
        }
    }

}