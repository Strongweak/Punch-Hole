using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

public class HorizontalEnemy : Enemy
{
    [SerializeField] private int _firstrow;
    [SerializeField] private int _secRow;
    private GameObject _firstWarning;
    private GameObject _secondWarning;
    [SerializeField] private Material warningMaterial;
    private MaterialPropertyBlock _matBlock;
    protected override void Start()
    {
        CreateWarningMesh();
        base.Start();
    }
    

    public override IEnumerator EffectEvent()
    {
         Tween.ShakeLocalPosition(enemyVisual.transform, Vector3.up * 100f, GameplayManager._gameplaySpeed, 1f,
             true, Ease.OutElastic);
        Sequence shithead = Sequence.Create(cycles: 1, CycleMode.Yoyo);
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.EnemyTurn);
        for (int i = 0; i < GridSystem.Col; i++)
        {
            Block tempObject;
            int tempValue;
            tempObject = GridSystem.DataGrid[i, _firstrow].Item1;
            tempValue = GridSystem.DataGrid[i, _firstrow].Item2;
            Sequence firstColTween = new Sequence();
            Sequence secondTween = new Sequence();

            // Tween for the first row
            if (GridSystem.DataGrid[i, _firstrow].Item1 != null)
            {
                firstColTween = Tween.Scale(GridSystem.DataGrid[i, _firstrow].Item1.transform, Vector3.one * 1.2f,
                        GameplayManager._gameplaySpeed)
                    .Chain(Tween.Position(GridSystem.DataGrid[i, _firstrow].Item1.transform, new Vector3(i, _secRow),
                        GameplayManager._gameplaySpeed, Ease.OutExpo))
                    .ChainDelay(GameplayManager._gameplaySpeed)
                    .Chain(Tween.Scale(GridSystem.DataGrid[i, _firstrow].Item1.transform, Vector3.one,
                        GameplayManager._gameplaySpeed))
                    .ChainDelay(GameplayManager._gameplaySpeed);

            }

            // Tween for the second row
            if (GridSystem.DataGrid[i, _secRow].Item1 != null)
            {
                secondTween = Tween.Scale(GridSystem.DataGrid[i, _secRow].Item1.transform, Vector3.one * 1.2f,
                        GameplayManager._gameplaySpeed)
                    .Chain(Tween.Position(GridSystem.DataGrid[i, _secRow].Item1.transform, new Vector3(i, _firstrow),
                        GameplayManager._gameplaySpeed, Ease.OutExpo))
                    .ChainDelay(GameplayManager._gameplaySpeed)
                    .Chain(Tween.Scale(GridSystem.DataGrid[i, _secRow].Item1.transform, Vector3.one,
                        GameplayManager._gameplaySpeed))
                    .ChainDelay(GameplayManager._gameplaySpeed);
            }

            // currentSequence = Sequence.Create(cycles: 1, CycleMode.Yoyo).Group(firstColTween).Group(secondTween)
            //     .ChainCallback(Telegraph).ChainDelay(GameplayManager.Instance.gameplaySpeed);
            shithead.Group(firstColTween).Group(secondTween);
            //yield return currentSequence.ToYieldInstruction();
            // then the data
            GridSystem.DataGrid[i, _firstrow].Item1 = GridSystem.DataGrid[i, _secRow].Item1;
            GridSystem.DataGrid[i, _firstrow].Item2 = GridSystem.DataGrid[i, _secRow].Item2;
            GridSystem.DataGrid[i, _secRow].Item1 = tempObject;
            GridSystem.DataGrid[i, _secRow].Item2 = tempValue;
        }
        yield return shithead.ToYieldInstruction();
        Telegraph();
    }

    protected override void Setup()
    {
        Telegraph();
    }
    protected override void Telegraph()
    {
        List<int> numbers = new List<int>();
        for (int i = 0; i < GridSystem.Row; i++) // Adjust the range as needed
        {
            numbers.Add(i);
        }
        _firstrow = numbers[Random.Range(0, numbers.Count)];
        numbers.Remove(_firstrow);
        _secRow = numbers[Random.Range(0, numbers.Count)];
        _firstWarning.transform.position = new Vector3(GridSystem.Col/2,_firstrow);
        _secondWarning.transform.position = new Vector3(GridSystem.Col/2,_secRow);

    }
    
    private void CreateWarningMesh()
    {
        Destroy(_firstWarning);
        Destroy(_secondWarning);
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

        _firstWarning = new GameObject("Warning1",typeof(MeshFilter), typeof(MeshRenderer));
        _firstWarning.transform.localScale = new Vector3(GridSystem.Col, 1);
        _firstWarning.GetComponent<MeshFilter>().mesh = mesh;
        _firstWarning.GetComponent<MeshRenderer>().material = warningMaterial;
        
        _secondWarning = new GameObject("Warning2",typeof(MeshFilter), typeof(MeshRenderer));
        _secondWarning.transform.localScale = new Vector3(GridSystem.Col, 1);
        _secondWarning.GetComponent<MeshFilter>().mesh = mesh;
        _secondWarning.GetComponent<MeshRenderer>().material = warningMaterial;
        
        _matBlock = new MaterialPropertyBlock();
        _matBlock.SetInt("_Highlight", 0);
        _firstWarning.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);
        _secondWarning.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);
    }

    public override void HeadupTelegraph()
    {
        _matBlock.SetInt("_Highlight",1);
        
        _secondWarning.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);
        _firstWarning.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);
    }    
    public override void ReleaseHeadupTelegraph()
    {
        _matBlock.SetInt("_Highlight",0);
        _secondWarning.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);
        _firstWarning.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(_firstWarning);
        Destroy(_secondWarning);
    }
}
