using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;

public class ColSwapper : Enemy
{
    [SerializeField] private int firstCol;
    [SerializeField] private int secCol;
    protected override void Start()
    {
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
    }
    
}
