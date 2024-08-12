using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;

public class ColSwapper : Enemy
{
    [SerializeField] private int firstCol;
    [SerializeField] private int secCol;
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
        Tween.ShakeLocalPosition(enemyVisual.transform,Vector3.up* 100f,GameplayManager.Instance.gameplaySpeed,1f, true, Ease.OutElastic);
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.EnemyTurn);
        Sequence shithead = Sequence.Create(cycles: 1, CycleMode.Yoyo);
        for (int i = 0; i < GridSystem.row; i++)
        {
            GameObject tempObject;
            int tempValue;
            tempObject = GridSystem.dataGrid[firstCol, i].Item1;
            tempValue = GridSystem.dataGrid[firstCol, i].Item2;
            Sequence firstColTween = new Sequence();
            Sequence secondTween = new Sequence();

            //Tween.Position(GridSystem.dataGrid[firstCol, i].Item1.transform,new Vector3(secCol, i), 0.2f).Group(Tween.Position(GridSystem.dataGrid[secCol, i].Item1.transform, new Vector3(firstCol,i), 0.2f));
            // //swap the visual first
            if (GridSystem.dataGrid[firstCol, i].Item1 != null)
            {
                //Tween.Scale(GridSystem.dataGrid[firstCol, i].Item1.transform, Vector3.one * 0.2f, GameplayManager.Instance.gameplaySpeed);
                firstColTween = Tween.Scale(GridSystem.dataGrid[firstCol, i].Item1.transform, Vector3.one * 1.2f, GameplayManager.Instance.gameplaySpeed)
                    .Chain(Tween.Position(GridSystem.dataGrid[firstCol, i].Item1.transform, new Vector3(secCol, i), GameplayManager.Instance.gameplaySpeed, Ease.OutExpo))
                    .ChainDelay(GameplayManager.Instance.gameplaySpeed)
                    .Chain(Tween.Scale(GridSystem.dataGrid[firstCol, i].Item1.transform, Vector3.one, GameplayManager.Instance.gameplaySpeed))
                    .ChainDelay(GameplayManager.Instance.gameplaySpeed);
                //GridSystem.dataGrid[firstCol, i].Item1.transform.position = new Vector3(secCol, i);
            }
            
            if (GridSystem.dataGrid[secCol, i].Item1 != null)
            {
                secondTween = Tween.Scale(GridSystem.dataGrid[secCol, i].Item1.transform, Vector3.one * 1.2f, GameplayManager.Instance.gameplaySpeed)
                    .Chain(Tween.Position(GridSystem.dataGrid[secCol, i].Item1.transform, new Vector3(firstCol, i), GameplayManager.Instance.gameplaySpeed, Ease.OutExpo))
                    .ChainDelay(GameplayManager.Instance.gameplaySpeed)
                    .Chain(Tween.Scale(GridSystem.dataGrid[secCol, i].Item1.transform, Vector3.one, GameplayManager.Instance.gameplaySpeed))
                    .ChainDelay(GameplayManager.Instance.gameplaySpeed);
                //GridSystem.dataGrid[secCol, i].Item1.transform.position = new Vector3(firstCol,i);
            }

            shithead.Group(firstColTween).Group(secondTween);
            // currentSequence = Sequence.Create(cycles: 1, CycleMode.Yoyo).Group(firstColTween).Group(secondTween)
            //     .ChainCallback(Telegraph).ChainDelay(GameplayManager.Instance.gameplaySpeed);
            // then the data
            GridSystem.dataGrid[firstCol, i].Item1 = GridSystem.dataGrid[secCol, i].Item1;
            GridSystem.dataGrid[firstCol, i].Item2 = GridSystem.dataGrid[secCol, i].Item2;
            GridSystem.dataGrid[secCol, i].Item1 = tempObject;
            GridSystem.dataGrid[secCol, i].Item2 = tempValue;

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
        firstCol = numbers[Random.Range(0, numbers.Count)];
        numbers.Remove(firstCol);
        secCol = numbers[Random.Range(0, numbers.Count)];
        firstWarning.transform.position = new Vector3(firstCol,GridSystem.row/2);
        secondWarning.transform.position = new Vector3(secCol,GridSystem.row/2);
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
        firstWarning.transform.localScale = new Vector3(1, GridSystem.row);
        firstWarning.GetComponent<MeshFilter>().mesh = mesh;
        firstWarning.GetComponent<MeshRenderer>().material = warningMaterial;
        
        secondWarning = new GameObject("Warning2",typeof(MeshFilter), typeof(MeshRenderer));
        secondWarning.transform.localScale = new Vector3(1, GridSystem.row);
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
