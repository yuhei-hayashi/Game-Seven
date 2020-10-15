using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class FieldController : MonoBehaviourPunCallbacks
{
   public FieldModel model;
   public void InitField(int fieldID,string fieldColor)
   {
       model = new FieldModel(fieldID,fieldColor);
   }
}
