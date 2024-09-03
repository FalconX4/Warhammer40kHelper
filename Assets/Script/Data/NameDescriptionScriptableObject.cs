using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NameDescriptionData", menuName = "Data/NameDescriptionData")]
public class NameDescriptionScriptableObject : ScriptableObject
{
    [Serializable]
    public struct NameDescriptionData
    {
        public string Name;
        public string Description;
    }
    public List<NameDescriptionData> NameDescription = new List<NameDescriptionData>();
}
