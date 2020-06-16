using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakablePole : TutePole
{
    [SerializeField] List<GameObject> readyDisableList;

    protected override void Dead()
    {
        base.Dead();
    }

    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage, attackPosition);
    }
}
