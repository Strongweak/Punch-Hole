using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Shape pack")]
public class ShapeListSO : ScriptableObject
{
    public List<ShapeData> spawnableData;
}
