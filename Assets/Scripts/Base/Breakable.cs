using UnityEngine;

public class Breakable : MonoBehaviour
{
    [SerializeField] protected int health;
    [SerializeField] protected bool isDead;

    protected void CheckIsDead()
    {
        if (health <= 0 && !isDead)
        {
            Dead();
        }
    }

    public virtual void Hurt(int damage)
    {
        if (!isDead)
        {
            health -= damage;
        }
    }

    public virtual void Hurt(int damage, Transform attackPosition)
    {
        if (!isDead)
        {
            health -= damage;
        }
    }

    protected virtual void Dead()
    {
        isDead = true;
    }

}
