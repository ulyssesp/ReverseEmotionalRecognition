using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class FacialAction
{
    [SerializeField]
    public NamedVector3[] Transforms;

    public static FacialAction fromTransforms(List<Transform> transforms)
    {
        FacialAction fa = new FacialAction();
        fa.Transforms = transforms.Select(t => new NamedVector3(t.name, t.localPosition)).ToArray();
        return fa;
    }
}