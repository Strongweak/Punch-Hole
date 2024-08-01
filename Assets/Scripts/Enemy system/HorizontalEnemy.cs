using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

public class HorizontalEnemy : Enemy
{
    [SerializeField] private int firstrow;
    [SerializeField] private int secRow;
    protected override void Start()
    {
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
    }
}
