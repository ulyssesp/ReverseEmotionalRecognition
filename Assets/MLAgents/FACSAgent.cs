
using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.Barracuda;
using System.Linq;


[RequireComponent(typeof(FACSStoreComponent), typeof(DecisionRequester))]
public class FACSAgent : Agent
{
    public EmotionDetect EmotionDetector;
    private FACSStoreComponent facsStore;
    public List<float> facialActionValues;
    private List<float> EmotionValues;
    private Dictionary<string, Transform> transforms;

    protected override void OnEnable()
    {
        // Has to be in OnEnable to happen before initialize
        facsStore = GetComponent<FACSStoreComponent>();
        GetComponent<BehaviorParameters>().BrainParameters.VectorObservationSize = 8 + facsStore.facsstore.FacialActions.Length;
        GetComponent<BehaviorParameters>().BrainParameters.VectorActionSize = new int[] { facsStore.facsstore.FacialActions.Length };

        base.OnEnable();
    }

    public override void Initialize()
    {
        facialActionValues = new List<float>(new float[facsStore.facsstore.FacialActions.Length]);
        transforms = FindRecursive(facsStore.Root).ToDictionary(t => t.name, t => t);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        EmotionValues = new List<float>{
              EmotionDetector.Happy
            , EmotionDetector.Sad
            , EmotionDetector.Angry
            , EmotionDetector.Disgusted
            , EmotionDetector.Fearful
            , EmotionDetector.Neutral
            , EmotionDetector.Surprised
            , EmotionDetector.Contempt
        };

        sensor.AddObservation(EmotionValues);
        sensor.AddObservation(facialActionValues);
    }


    public override void OnActionReceived(float[] act)
    {
        for (int i = 0; i < act.Length; i++)
        {
            facialActionValues[i] = Mathf.Clamp(facialActionValues[i] + 0.06f * act[i], 0f, 1f);
        }

        ApplyFacialActionValues();

        int emotionTarget = Mathf.FloorToInt(Academy.Instance.EnvironmentParameters.GetWithDefault("emotion", 0));
        float emotionTargetValue = EmotionValues[emotionTarget];
        AddReward(emotionTargetValue * Academy.Instance.EnvironmentParameters.GetWithDefault("emotion-reward", 1) / (float)MaxStep);
    }

    static List<Transform> FindRecursive(Transform transform)
    {
        if (transform.childCount == 0)
        {
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

    public override void OnEpisodeBegin()
    {
        for (int i = 0; i < facialActionValues.Count; i++)
        {
            facialActionValues[i] = Random.value;
        }

        ApplyFacialActionValues();
    }

    private void ApplyFacialActionValues()
    {
        Dictionary<string, (Vector3, int)> facResults = new Dictionary<string, (Vector3, int)>();

        for (int i = 0; i < facsStore.facsstore.FacialActions.Length; i++)
        {
            FacialAction fac = facsStore.facsstore.FacialActions[i].value;
            float value = facialActionValues[i];

            foreach (NamedVector3 vec in fac.Transforms)
            {
                if (facResults.ContainsKey(vec.VectorName))
                {
                    (Vector3, int) currentValue = facResults[vec.VectorName];
                    facResults[vec.VectorName] = ((currentValue.Item1 * currentValue.Item2 + value * vec.value) / (float)currentValue.Item2, currentValue.Item2 + 1);
                }
                else
                {
                    facResults.Add(vec.VectorName, (value * vec.value, 1));
                }
            }
        }

        foreach (KeyValuePair<string, (Vector3, int)> kvp in facResults)
        {
            transforms[kvp.Key].localPosition = kvp.Value.Item1;
        }
    }

}