using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

public class PopupTextAnimation : MonoBehaviour
{
    public TMP_Text textComponent;
    private Sequence currentSequence;
    public float delayBetweenText;
    public float characterAnimSpeed;

    public AnimationCurve curve;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.F))
        // {
        //     PopupAnimation();
        // }
    }

    void PopupAnimation()
    {
        currentSequence.Complete();
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

        currentSequence = Sequence.Create(cycles: 1, CycleMode.Yoyo);
        Debug.Log("Animation is running");
        var textInfo = textComponent.textInfo;
        // Loop through each character in the text
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            var verts = textInfo.meshInfo[materialIndex].vertices;

            // Save original positions
            Vector3[] originalVerts = new Vector3[4];
            for (int j = 0; j < 4; j++)
            {
                originalVerts[j] = verts[vertexIndex + j];
            }

            // Scale to zero
            for (int j = 0; j < 4; j++)
            {
                verts[vertexIndex + j] = charInfo.bottomLeft; // Set all vertices to the bottom left position
                textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            }


            Tween floatTween = Tween.Custom(0f, 1f, characterAnimSpeed, x =>
            {
                float scale = x;

                for (int j = 0; j < 4; j++)
                {
                    verts[vertexIndex + j] = charInfo.bottomLeft + (originalVerts[j] - charInfo.bottomLeft) * scale;
                }

                textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            }, curve);
            // Add the scaling animation to the sequence
            currentSequence.Insert(atTime: i * delayBetweenText, floatTween);

            #region DOTween

            // Animate back to normal size
            // float elapsedTime = 0f;

            // while (elapsedTime < characterAnimSpeed)
            // {
            //     elapsedTime += Time.deltaTime;
            //     float scale = curve.Evaluate(elapsedTime / characterAnimSpeed);

            //     for (int j = 0; j < 4; j++)
            //     {
            //         verts[vertexIndex + j] = charInfo.bottomLeft + (originalVerts[j] - charInfo.bottomLeft) * scale;
            //     }

            //     textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            //     yield return null;
            // }

            // // Ensure final position is set
            // for (int j = 0; j < 4; j++)
            // {
            //     verts[vertexIndex + j] = originalVerts[j];
            // }

            // textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

            //yield return new WaitForSeconds(delayBetweenText); // Delay between each character animation

            #endregion
        }

        Debug.Log("Animation is end");
    }

    private void OnDisable()
    {
        currentSequence.Complete();
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}