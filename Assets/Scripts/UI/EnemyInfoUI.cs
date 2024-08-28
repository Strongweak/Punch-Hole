using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyInfoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _headerText;
    [SerializeField] private TMP_Text _informationText;
    [SerializeField] private TMP_Text _healthText;
    public float delayBetweenCharacter = 0.02f;
    public float characterAnimSpeed = 0.2f;
    public AnimationCurve curve;
    private Tween currentpopupTween;
    private Sequence[] _sequenceSlot = new Sequence[3];
    
    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void FeedData(EnemySO data)
    {
        currentpopupTween.Complete();
        currentpopupTween = Tween.Scale(transform, Vector3.one, 0.2f, Ease.OutBack);
        _headerText.SetText(data.name);
        _informationText.SetText(data.description);
        _healthText.SetText($"Hp: {data.health.ToString()}");
        PopupAnimation(_informationText,_sequenceSlot[0]);
        PopupAnimation(_headerText, _sequenceSlot[1]);
        PopupAnimation(_healthText, _sequenceSlot[2]);
    }

    public void Hide()
    {
        currentpopupTween.Complete();
        currentpopupTween = Tween.Scale(transform, Vector3.zero, 0.2f, Ease.OutBack).OnComplete(() =>
        {
            foreach (var sequence in _sequenceSlot)
            {
                sequence.Stop();
            }
            _headerText.ForceMeshUpdate();
            _informationText.ForceMeshUpdate();
            _healthText.ForceMeshUpdate();
            gameObject.SetActive(false);
        });
    }


    void PopupAnimation(TMP_Text stringText, Sequence slot)
    {
        slot.Stop();
        stringText.ForceMeshUpdate();
        // Array to store original positions for each character

        slot = Sequence.Create(cycles: 1, CycleMode.Yoyo);
        var textInfo = stringText.textInfo;
        Vector3[][] originalVertices = new Vector3[textInfo.characterCount][];
        // Loop through each character in the text
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            var verts = textInfo.meshInfo[materialIndex].vertices;

            // Save original positions
            originalVertices[i] = new Vector3[4];
            for (int j = 0; j < 4; j++)
            {
                originalVertices[i][j] = verts[vertexIndex + j];
            }

            // Scale to zero
            for (int j = 0; j < 4; j++)
            {
                verts[vertexIndex + j] = charInfo.bottomLeft; // Set all vertices to the bottom left position
            }
        }
        stringText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            var verts = textInfo.meshInfo[materialIndex].vertices;

            Vector3[] originalVerts = originalVertices[i];
            Tween floatTween = Tween.Custom(0f, 1f, characterAnimSpeed, x =>
            {
                float scale = x;

                for (int j = 0; j < 4; j++)
                {
                    verts[vertexIndex + j] = charInfo.bottomLeft + (originalVerts[j] - charInfo.bottomLeft) * scale;
                }

                stringText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            }, curve);
            // Add the scaling animation to the sequence
            slot.Insert(atTime: i * delayBetweenCharacter, floatTween);
        }
    }
}