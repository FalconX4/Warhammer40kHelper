using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ListStringData", menuName = "Data/ListStringData")]
public class ListStringScriptableObject : ScriptableObject
{
    public List<string> List = new List<string>();
}
