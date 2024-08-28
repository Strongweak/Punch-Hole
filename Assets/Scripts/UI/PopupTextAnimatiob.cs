using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PopupTextAnimatiob : MonoBehaviour
{
    public TMP_Text textComponent;
    private Sequence sequence;
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
        if (Input.GetKeyDown(KeyCode.F))
        {
            PopupAnimation();
        }
        // var textInfo = textComponent.textInfo;
        // for (int i = 0; i < textInfo.characterCount; i++)
        // {
        //     var charInfo = textInfo.characterInfo[i];

        //     if (!charInfo.isVisible)
        //     {
        //         continue;
        //     }
        //     var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;


        //     for (int j = 0; j < 4; ++j)
        //     {
        //         var orig = verts[charInfo.vertexIndex + j];
        //         verts[charInfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin(Time.time * 2f + orig.x * 0.01f) * 10f, 0);

        //     }
        // }
        // for (int i = 0; i < textInfo.meshInfo.Length; ++i)
        // {

        //     var meshInfo = textInfo.meshInfo[i];
        //     meshInfo.mesh.vertices = meshInfo.vertices;
        //     textComponent.UpdateGeometry(meshInfo.mesh, i);

        // }
    }

    void PopupAnimation()
    {
        Sequence sequence = DOTween.Sequence();
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
            }

            textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

            // Create a local variable to keep track of which character's vertices are being animated
            int index = i;

            // Add the scaling animation to the sequence
            sequence.Insert(i * delayBetweenText,DOTween.To(() => 0f, x =>
            {
                float scale = x;

                for (int j = 0; j < 4; j++)
                {
                    verts[vertexIndex + j] = charInfo.bottomLeft + (originalVerts[j] - charInfo.bottomLeft) * scale;
                }

                textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

            }, 1f, characterAnimSpeed).SetEase(Ease.OutBack));

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
        }
        Debug.Log("Animation is end");
    }
}
