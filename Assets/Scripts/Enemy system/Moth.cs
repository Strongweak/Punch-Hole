using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moth : Enemy
{
    //Start at a random spot on the grid, when it his turn, will random move at one direction with random distance
    //destroying all cell along the way
    [SerializeField] private int startCol;
    [SerializeField] private int startRow;
    [SerializeField] private int moveStep;
    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    private Direction direction;

    protected override void Start()
    {
        base.Start();
    }


    public override IEnumerator EffectEvent()
    {
        Tween.ShakeLocalPosition(enemyVisual.transform, Vector3.up * 100f, GameplayManager._gameplaySpeed, 1f,
            true, Ease.OutElastic);
        for (int i = 0; i < moveStep; i++)
        {
            if(startCol > GridSystem.DataGrid.GetLength(0) || startCol < GridSystem.DataGrid.GetLength(0))
            {
                yield break;
            }

        }
        Sequence shithead = Sequence.Create(cycles: 1, CycleMode.Yoyo);
        Observer.Instance.TriggerEvent(ObserverConstant.OnStateChange, GameState.EnemyTurn);
        yield return shithead.ToYieldInstruction();
        Telegraph();
    }

    protected override void Telegraph()
    {
       // startCol = Random.Range(0, GridSystem.dataGrid.GetLength(0));
        //startRow = Random.Range(0, GridSystem.dataGrid.GetLength(1));
        moveStep = Random.Range(0, 6);
    }
    private void ChoseDirection()
    {
        
    }
}
