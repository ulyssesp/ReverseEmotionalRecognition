using UnityEngine;

[System.Serializable]
public class NamedVector3
{
    [SerializeField]
    public string VectorName;
    [SerializeField]
    public Vector3 value;
    public NamedVector3(string name, Vector3 value)
    {
        this.VectorName = name;
        this.value = value;
    }
}