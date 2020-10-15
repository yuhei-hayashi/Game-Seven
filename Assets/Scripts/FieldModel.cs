using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldModel
{
    public int id;
    public string color;
    public int num;
    public int cards;
    public FieldModel(int fieldID,string fieldColor)
        {
            id = fieldID - 1;
            color = fieldColor;
            num = fieldID;
            cards = 0;
        }
}
