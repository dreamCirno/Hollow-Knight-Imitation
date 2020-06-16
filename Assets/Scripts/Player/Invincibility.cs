using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invincibility : MonoBehaviour
{
    public SpriteRenderer render;
    public Color normalColor;
    public Color flashColor;
    public bool isInvincible;
    public int duration;

    public IEnumerator SetInvincibility()
    {
        isInvincible = true;
        Physics2D.IgnoreLayerCollision(13, 15, true);
        for (int i = 0; i < duration; i++)
        {
            yield return new WaitForSeconds(0.1f);
            render.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            render.color = normalColor;
        }
        Physics2D.IgnoreLayerCollision(13, 15, false);
        isInvincible = false;
    }
}
