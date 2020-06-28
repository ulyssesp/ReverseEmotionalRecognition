using UnityEngine;

[System.Serializable]
public class NamedFacialAction
{
    [SerializeField]
    public string ActionName;
    [SerializeField]
    public FacialAction value;
    public NamedFacialAction(string name, FacialAction value)
    {
        this.ActionName = name;
        this.value = value;
    }
}