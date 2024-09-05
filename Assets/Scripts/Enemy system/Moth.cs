using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moth : Enemy
{
    //Start at a random spot on the grid, when it his turn, will random move at one direction with random distance
    //destroying all cell along the way
    [SerializeField] private int _currentCol;
    [SerializeField] private int _currentRow;
    [SerializeField] private int moveStep;
    [SerializeField] private Material _telegraphMat;
    [SerializeField] private Material _mothMat;
    private GameObject _MothMarker;
    private GameObject _MothPattern;
    private MaterialPropertyBlock _matBlock;

    [System.Serializable]
    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    [SerializeField] private Direction direction;


    protected override void Start()
    {
        base.Start();
    }

    protected override void Setup()
    {
        CreateWarningMesh();
        ChoseDirection();
        _currentCol = Random.Range(0, GridSystem.DataGrid.GetLength(0));
        _currentRow = Random.Range(0, GridSystem.DataGrid.GetLength(1));
        moveStep = Random.Range(1, 6);
        _MothMarker.transform.position = new Vector3(_currentCol, _currentRow);
        Telegraph();
    }
    public override IEnumerator EffectEvent()
    {
        switch (direction)
        {
            case Direction.Up:
                _currentRow = Mathf.Max(0, _currentRow - moveStep);
                break;
            case Direction.Down:
                _currentRow = Mathf.Min(GridSystem.Row - 1, _currentRow + moveStep);
                break;
            case Direction.Left:
                _currentCol = Mathf.Max(0, _currentCol - moveStep);
                break;
            case Direction.Right:
                _currentCol = Mathf.Min(GridSystem.Col - 1, _currentCol + moveStep);
                break;
        }
        _MothMarker.transform.position = new Vector3(_currentCol, _currentRow);
        Tween.ShakeLocalPosition(enemyVisual.transform, Vector3.up * 100f, GameplayManager._gameplaySpeed, 1f,
    true, Ease.OutElastic);

        Sequence shithead = Sequence.Create(cycles: 1, CycleMode.Yoyo);
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.EnemyTurn);
        yield return shithead.ToYieldInstruction();
        Telegraph();
    }

    protected override void Telegraph()
    {
        moveStep = Random.Range(0, 6);
        _MothMarker.transform.position = new Vector3(_currentCol, _currentRow);

        ChoseDirection();
    }
    private void ChoseDirection()
    {
        _MothPattern.transform.localPosition = new Vector3(_currentCol,_currentRow);
        direction = (Direction)Random.Range(0, 3);
        switch (direction)
        {
            case Direction.Up:
                _MothPattern.transform.localScale = new Vector3(1,moveStep);
                break;
            case Direction.Down:
                _MothPattern.transform.localScale = new Vector3(1,-moveStep);
                break;
            case Direction.Left:
                _MothPattern.transform.localScale = new Vector3(moveStep,1);
                break;
            case Direction.Right:
                _MothPattern.transform.localScale = new Vector3(-moveStep,1);
                break;
        }
    }
    private void CreateWarningMesh()
    {
        Destroy(_MothMarker);
        Destroy(_MothPattern);
        Vector3[] verts = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangle = new int[6];

        //index of the vertices
        verts[0] = new Vector3(0 - 0.5f, 1 - 0.5f);
        verts[1] = new Vector3(1 - 0.5f, 1 - 0.5f);
        verts[2] = new Vector3(0 - 0.5f, 0 - 0.5f);
        verts[3] = new Vector3(1 - 0.5f, 0 - 0.5f);

        //create a new uv
        uv[0] = new Vector2(0 - 0.5f, 1 - 0.5f);
        uv[1] = new Vector2(1 - 0.5f, 1 - 0.5f);
        uv[2] = new Vector2(0 - 0.5f, 0 - 0.5f);
        uv[3] = new Vector2(1 - 0.5f, 0 - 0.5f);

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
        _MothMarker = new GameObject("Moth", typeof(MeshFilter), typeof(MeshRenderer));
        _MothMarker.GetComponent<MeshFilter>().mesh = mesh;
        _MothMarker.GetComponent<MeshRenderer>().material = _mothMat;

        _MothPattern = new GameObject("Moth pattern", typeof(MeshFilter), typeof(MeshRenderer));
        _MothPattern.transform.localScale = new Vector3(1, 1);
        _MothPattern.GetComponent<MeshFilter>().mesh = mesh;
        _MothPattern.GetComponent<MeshRenderer>().material = _telegraphMat;

        _matBlock = new MaterialPropertyBlock();
        _matBlock.SetInt("_Highlight", 0);
        _MothMarker.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);
        _MothPattern.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);
    }

    public override void HeadupTelegraph()
    {
        _matBlock.SetInt("_Highlight", 1);
        _MothPattern.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);
        _MothMarker.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);

    }
    public override void ReleaseHeadupTelegraph()
    {
        _matBlock.SetInt("_Highlight", 0);
        _MothPattern.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);
        _MothMarker.GetComponent<MeshRenderer>().SetPropertyBlock(_matBlock);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(_MothMarker);
        Destroy(_MothPattern);
    }
}
