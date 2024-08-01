using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Scoring scaling")]
public class ScoringSO : ScriptableObject
{
    public List<ScoreScale> score;
}

[System.Serializable]
public class ScoreScale
{
    public string quote;
    public int mult;
}