using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _headerText;
    [SerializeField] private TextMeshProUGUI _informationText;
    [SerializeField] private TextMeshProUGUI _healthText;

    public void FeedData(EnemySO data)
    {
        _headerText.SetText(data.name);
        _informationText.SetText(data.description);
        _healthText.SetText($"Hp: {data.health.ToString()}");
    }
}
