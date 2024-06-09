using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;


public class Board : MonoBehaviour
{
    [SerializeField] private GameObject dice;
    [SerializeField] private DiceManager diceManager;
    [SerializeField] private AudioManager audioManager;

    private Rigidbody diceRb;

    public delegate void RollAction(int num);
    public RollAction rollEvent;

    private bool read = true;

    void Start()
    {
        diceRb = dice.GetComponent<Rigidbody>();
        diceManager.rollEvent.AddListener(InitiazlizedDiceRollingProcedure);
    }

    private void OnDestroy()
    {
        diceManager.rollEvent.RemoveListener(InitiazlizedDiceRollingProcedure);
    }
    //
    private void InitiazlizedDiceRollingProcedure()
    {
        read = true;
    }



    private void OnTriggerStay(Collider other)
    {
        if((diceRb.velocity.x == 0f && diceRb.velocity.y == 0f && diceRb.velocity.z == 0f) && read)
        {
            read = false;
            int turnedTo  = TurnedTo(other.gameObject.name);
            int audioIndx = audioManager.audioList.FindIndex((aud) => aud.name == turnedTo.ToString());
            rollEvent.Invoke(turnedTo);
            audioManager.PlaySimpleAudio(audioIndx);
        }
    }

    private int TurnedTo(String numChar) 
    {
        int max = 6;
        int min = 1;
        int subject = StringToIntConverter(numChar);
        return (max - subject) + min;
    }

    public int StringToIntConverter(String str)
    {
        int i;
        bool success = int.TryParse(str, out i);
        return success ? i : -1;
    }

}