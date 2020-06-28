using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "FACSStore", menuName = "BaislessFabric/FACSStore", order = 1)]
[System.Serializable]
public class FACSStore : ScriptableObject
{
    [SerializeField]
    public NamedFacialAction[] FacialActions;

    public FACSStore()
    {
        FacialActions = new NamedFacialAction[0];
    }

    public void AddFacialAction(string name, FacialAction facialAction)
    {
        FacialActions = FacialActions.Where(fa => fa.ActionName != name).ToArray();
        FacialActions = FacialActions.Append(new NamedFacialAction(name, facialAction)).ToArray();
    }
}