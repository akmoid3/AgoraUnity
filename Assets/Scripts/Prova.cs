using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Prova : MonoBehaviour
{
   [SerializeField] InputActionReference spawnActionMap;


   private void Start()
   {
      spawnActionMap.action.performed += ProvaMethod;
   }

   private void ProvaMethod(InputAction.CallbackContext obj)
   {
         Debug.Log("FUNZIONA ");
   }
}
