using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardEntity", menuName = "CreateCardEntity")]
public class CardEntity : ScriptableObject {
    public int id;
    public new string color;
     public new string mark;
    public int number;
    public Sprite icon;
}