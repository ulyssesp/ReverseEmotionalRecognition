using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;


public class RigTransformAgent : Agent
{
    public int Index;
    private Vector3 m_InitialPositionUnaltered;
    private Vector3 m_InitialPosition;
    private EmotionReceiver m_EmotionReceiver;

    public override void Initialize()
    {
        m_InitialPositionUnaltered = transform.position;
        m_InitialPosition = m_InitialPositionUnaltered;
        m_InitialPosition.x *= 50f;
        m_InitialPosition.y -= 1.7f;
        m_InitialPosition.z = (m_InitialPosition.z - 0.09f) * 50;

        m_EmotionReceiver = GameObject.FindGameObjectWithTag("EmotionReceiver").GetComponent<EmotionReceiver>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Index);
        sensor.AddObservation(m_InitialPosition.x);
        sensor.AddObservation(m_InitialPosition.y);
        sensor.AddObservation(m_InitialPosition.z);

        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);
        sensor.AddObservation(transform.localPosition.z);

        // TODO: Add more emotion detection stuff
        sensor.AddObservation(m_EmotionReceiver.Angry);
        sensor.AddObservation(m_EmotionReceiver.Disgusted);
        sensor.AddObservation(m_EmotionReceiver.Fearful);
        sensor.AddObservation(m_EmotionReceiver.Happy);
        sensor.AddObservation(m_EmotionReceiver.Neutral);
        sensor.AddObservation(m_EmotionReceiver.Sad);
        sensor.AddObservation(m_EmotionReceiver.Surprised);
    }

    public override void OnActionReceived(float[] act)
    {
        Vector3 position = transform.localPosition;

        position.x += 0.001f * Mathf.Clamp(act[0], -1, 1);
        position.y += 0.002f * Mathf.Clamp(act[1], -1, 1);
        position.z += 0.001f * Mathf.Clamp(act[2], -1, 1);

        // switch ((int)act[0])
        // {
        //     case 1:
        //         position.x += Mathf.Sign(m_InitialPositionUnaltered.x) * 0.0005f;
        //         break;
        //     case 2:
        //         position.x -= Mathf.Sign(m_InitialPositionUnaltered.x) * 0.0005f;
        //         break;
        // }

        // switch ((int)act[1])
        // {
        //     case 1:
        //         position.y += 0.001f;
        //         break;
        //     case 2:
        //         position.y -= 0.001f;
        //         break;
        // }

        // switch ((int)act[2])
        // {
        //     case 1:
        //         position.z += 0.0002f;
        //         break;
        //     case 2:
        //         position.z -= 0.0002f;
        //         break;
        // }

        // Slowly transform back to original position
        Vector3 distanceToNeutral = m_InitialPositionUnaltered - transform.position;
        position += transform.TransformVector(distanceToNeutral * 0.01f);
        transform.localPosition = position;

        // Rewards
        AddReward(Mathf.Min(Mathf.Log10(m_EmotionReceiver.Happy + 1), 1f));
        SetReward(-0.001f * Mathf.Abs(distanceToNeutral.x));
        SetReward(-0.001f * Mathf.Abs(distanceToNeutral.y));
        SetReward(-0.001f * Mathf.Abs(distanceToNeutral.z));
    }

    public override void OnEpisodeBegin()
    {
        transform.position = m_InitialPositionUnaltered;
    }
}