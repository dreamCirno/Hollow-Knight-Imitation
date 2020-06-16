using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpenCondition : MonoBehaviour
{
    [SerializeField] private BattleGate leftBattleGate;
    [SerializeField] private BattleGate rightBattleGate;
    [SerializeField] private Fly[] flys;

    private bool isOpen;

    private void Start()
    {
        flys = GetComponentsInChildren<Fly>();
    }

    private void Update()
    {
        if (!isOpen)
        {
            for (int i = 0; i < flys.Length; i++)
            {
                if (!flys[i].GetDeadStatment())
                {
                    return;
                }
            }
            isOpen = true;
            UnlockGates();
        }
    }

    public void UnlockGates()
    {
        leftBattleGate.Unlock();
        rightBattleGate.Unlock();
    }
}
