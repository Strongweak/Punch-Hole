using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Shape data")]
public class ShapeData : ScriptableObject
{
    public int col;
    public int row;
    public Row[] board;
    
    public void Clear()
    {
        for (int i = 0; i < row; i++)
        {
            board[i].ClearRow();
        }
    }
    
    public void CreateNewBoard()
    {
        board = new Row[row];
        for (int i = 0; i < row; i++)
        {
            board[i] = new Row(col);
        }
    }
    [System.Serializable]
    public class Row
    {
        public bool[] col;
        private int size;
    
        public Row()
        {
            
        }
    
    
        public Row(int size)
        {
            NewRow(size);
        }
    
        public void NewRow(int size)
        {
            this.size = size;
            col = new bool[this.size];
            ClearRow();
        }
    
        public void ClearRow()
        {
            for(int i= 0; i< this.size; i++)
            {
                col[i] = false;
            }
        }

    }
}
