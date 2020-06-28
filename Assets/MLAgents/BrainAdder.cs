using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.Barracuda;

public class BrainAdder : MonoBehaviour
{
    public Transform root;
    public NNModel nnModel;
    public RenderTexture camTexture;
    public int MaxStep;
    public void Start()
    {
        List<Transform> transforms = FindRecursive(root);

        for (int i = 0; i < transforms.Count; i++)
        {
            Transform transform = transforms[i];
            if (transform.gameObject.name.Contains("Tongue") || transform.gameObject.name.Contains("Teeth"))
            {
                continue;
            }

            BehaviorParameters bp = transform.GetComponent<BehaviorParameters>();
            if (bp == null)
            {
                bp = transform.gameObject.AddComponent<BehaviorParameters>();
            }

            bp.BehaviorName = "SnapperController";
            bp.BrainParameters.VectorObservationSize = 55;
            // bp.BrainParameters.NumStackedVectorObservations = 4;
            bp.BrainParameters.VectorActionSpaceType = SpaceType.Continuous;
            bp.BrainParameters.VectorActionSize = new int[] { 3, 3, 3 };
            bp.Model = nnModel;

            RigTransformAgent rta = transform.GetComponent<RigTransformAgent>();
            if (rta == null)
            {
                rta = transform.gameObject.AddComponent<RigTransformAgent>();
            }

            rta.Index = i;
            rta.IndexRange = transforms.Count;
            rta.MaxStep = MaxStep;

            DecisionRequester dr = transform.GetComponent<DecisionRequester>();
            if (dr == null)
            {
                dr = transform.gameObject.AddComponent<DecisionRequester>();
            }

            dr.DecisionPeriod = 2;
            dr.TakeActionsBetweenDecisions = true;

            RenderTextureSensorComponent rts = transform.GetComponent<RenderTextureSensorComponent>();
            if (rts == null)
            {
                rts = transform.gameObject.AddComponent<RenderTextureSensorComponent>();
            }

            rts.RenderTexture = camTexture;
            rts.Grayscale = true;
        }
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

}