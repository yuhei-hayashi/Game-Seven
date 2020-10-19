using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffect : MonoBehaviour
{
    public Dictionary<int,System.Action> cardEffect = new Dictionary<int,System.Action>();

    public void EffectInstall()
    {
        cardEffect.Add(1,Draw);
    }

     void Draw()
    {
    }


}
