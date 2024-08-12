using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

public class HorizontalEnemy : Enemy
{
    [SerializeField] private int firstrow;
    [SerializeField] private int secRow;
    private GameObject firstWarning;
    private GameObject secondWarning;
    [SerializeField] private Material warningMaterial;
    private MaterialPropertyBlock matBlock;
    protected override void Start()
    {
        CreateWarningMesh();
        base.Start();
    }
    

    public override IEnumerator EffectEvent()
    {
         Tween.ShakeLocalPosition(enemyVisual.transform, Vector3.up * 100f, GameplayManager.Instance.gameplaySpeed, 1f,
             true, Ease.OutElastic);
        Sequence shithead = Sequence.Create(cycles: 1, CycleMode.Yoyo);
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.EnemyTurn);
        for (int i = 0; i < GridSystem.col; i++)
        {
            GameObject tempObject;
            int tempValue;
            tempObject = GridSystem.dataGrid[i, firstrow].Item1;
            tempValue = GridSystem.dataGrid[i, firstrow].Item2;
            Sequence firstColTween = new Sequence();
            Sequence secondTween = new Sequence();

            // Tween for the first row
            if (GridSystem.dataGrid[i, firstrow].Item1 != null)
            {
                firstColTween = Tween.Scale(GridSystem.dataGrid[i, firstrow].Item1.transform, Vector3.one * 1.2f,
                        GameplayManager.Instance.gameplaySpeed)
                    .Chain(Tween.Position(GridSystem.dataGrid[i, firstrow].Item1.transform, new Vector3(i, secRow),
                        GameplayManager.Instance.gameplaySpeed, Ease.OutExpo))
                    .ChainDelay(GameplayManager.Instance.gameplaySpeed)
                    .Chain(Tween.Scale(GridSystem.dataGrid[i, firstrow].Item1.transform, Vector3.one,
                        GameplayManager.Instance.gameplaySpeed))
                    .ChainDelay(GameplayManager.Instance.gameplaySpeed);

            }

            // Tween for the second row
            if (GridSystem.dataGrid[i, secRow].Item1 != null)
            {
                secondTween = Tween.Scale(GridSystem.dataGrid[i, secRow].Item1.transform, Vector3.one * 1.2f,
                        GameplayManager.Instance.gameplaySpeed)
                    .Chain(Tween.Position(GridSystem.dataGrid[i, secRow].Item1.transform, new Vector3(i, firstrow),
                        GameplayManager.Instance.gameplaySpeed, Ease.OutExpo))
                    .ChainDelay(GameplayManager.Instance.gameplaySpeed)
                    .Chain(Tween.Scale(GridSystem.dataGrid[i, secRow].Item1.transform, Vector3.one,
                        GameplayManager.Instance.gameplaySpeed))
                    .ChainDelay(GameplayManager.Instance.gameplaySpeed);
            }

            // currentSequence = Sequence.Create(cycles: 1, CycleMode.Yoyo).Group(firstColTween).Group(secondTween)
            //     .ChainCallback(Telegraph).ChainDelay(GameplayManager.Instance.gameplaySpeed);
            shithead.Group(firstColTween).Group(secondTween);
            //yield return currentSequence.ToYieldInstruction();
            // then the data
            GridSystem.dataGrid[i, firstrow].Item1 = GridSystem.dataGrid[i, secRow].Item1;
            GridSystem.dataGrid[i, firstrow].Item2 = GridSystem.dataGrid[i, secRow].Item2;
            GridSystem.dataGrid[i, secRow].Item1 = tempObject;
            GridSystem.dataGrid[i, secRow].Item2 = tempValue;
        }
        yield return shithead.ToYieldInstruction();
        Telegraph();
    }

    protected override void Telegraph()
    {
        List<int> numbers = new List<int>();
        for (int i = 0; i < GridSystem.row; i++) // Adjust the range as needed
        {
            numbers.Add(i);
        }
        firstrow = numbers[Random.Range(0, numbers.Count)];
        numbers.Remove(firstrow);
        secRow = numbers[Random.Range(0, numbers.Count)];
        firstWarning.transform.position = new Vector3(GridSystem.col/2,firstrow);
        secondWarning.transform.position = new Vector3(GridSystem.col/2,secRow);

    }
    
    private void CreateWarningMesh()
    {
        Destroy(firstWarning);
        Destroy(secondWarning);
        Vector3[] verts = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangle = new int[6];

        //index of the vertices
        verts[0] = new Vector3(0-0.5f,1-0.5f);
        verts[1] = new Vector3(1-0.5f,1-0.5f);
        verts[2] = new Vector3(0-0.5f,0-0.5f);
        verts[3] = new Vector3(1-0.5f,0-0.5f);
        
        //create a new uv
        uv[0] = new Vector2(0-0.5f,1-0.5f);
        uv[1] = new Vector2(1-0.5f,1-0.5f);
        uv[2] = new Vector2(0-0.5f,0-0.5f);
        uv[3] = new Vector2(1-0.5f,0-0.5f);

        //mandatory:
        //corner MUST be clockwise order
        //display in clockwise to face to camera
        triangle[0] = 0;
        triangle[1] = 1;
        triangle[2] = 2;
        triangle[3] = 2;
        triangle[4] = 1;
        triangle[5] = 3;

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.uv = uv;
        mesh.triangles = triangle;

        firstWarning = new GameObject("Warning1",typeof(MeshFilter), typeof(MeshRenderer));
        firstWarning.transform.localScale = new Vector3(GridSystem.col, 1);
        firstWarning.GetComponent<MeshFilter>().mesh = mesh;
        firstWarning.GetComponent<MeshRenderer>().material = warningMaterial;
        
        secondWarning = new GameObject("Warning2",typeof(MeshFilter), typeof(MeshRenderer));
        secondWarning.transform.localScale = new Vector3(GridSystem.col, 1);
        secondWarning.GetComponent<MeshFilter>().mesh = mesh;
        secondWarning.GetComponent<MeshRenderer>().material = warningMaterial;
        
        matBlock = new MaterialPropertyBlock();
        matBlock.SetInt("_Highlight", 0);
        firstWarning.GetComponent<MeshRenderer>().SetPropertyBlock(matBlock);
        secondWarning.GetComponent<MeshRenderer>().SetPropertyBlock(matBlock);
    }

    public override void HeadupTelegraph()
    {
        matBlock.SetInt("_Highlight",1);
        
        secondWarning.GetComponent<MeshRenderer>().SetPropertyBlock(matBlock);
        firstWarning.GetComponent<MeshRenderer>().SetPropertyBlock(matBlock);
    }    
    public override void ReleaseHeadupTelegraph()
    {
        matBlock.SetInt("_Highlight",0);
        secondWarning.GetComponent<MeshRenderer>().SetPropertyBlock(matBlock);
        firstWarning.GetComponent<MeshRenderer>().SetPropertyBlock(matBlock);
    }
}
