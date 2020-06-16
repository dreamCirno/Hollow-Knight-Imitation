using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEffect : MonoBehaviour
{
    [Header("粒子系统")]
    [SerializeField] ParticleSystem doubleJump;
    [SerializeField] ParticleSystem fallTrail;
    [SerializeField] ParticleSystem wallSlideDust;
    [SerializeField] ParticleSystem burstRocks;
    [SerializeField] ParticleSystem dustL;
    [SerializeField] ParticleSystem dustR;
    [SerializeField] ParticleSystem lowHealthLeak;
    [SerializeField] ParticleSystem hitLeft;
    [SerializeField] ParticleSystem hitRight;
    [SerializeField] ParticleSystem ashLeft;
    [SerializeField] ParticleSystem ashRight;
    [SerializeField] ParticleSystem shade;
    [SerializeField] ParticleSystem roarDust;
    [SerializeField] ParticleSystem roarDustLil;
    [SerializeField] ParticleSystem dustJump;

    public enum EffectType
    {
        DoubleJump, FallTrail, WallSlideDust, BurstRocks, DustLeft, DustRight, LowHealthLeak, HitLeft, HitRight, AshLeft, AshRight, Shade,
        RoarDust, RoarDustLil, DustJump,
    }

    public void DoEffect(EffectType effectType, bool enabled)
    {
        switch (effectType)
        {
            case EffectType.DoubleJump:
                if (enabled)
                    doubleJump.Play();
                else
                    doubleJump.Stop();
                break;
            case EffectType.FallTrail:
                if (enabled)
                    fallTrail.Play();
                else
                    fallTrail.Stop();
                break;
            case EffectType.WallSlideDust:
                if (enabled)
                    wallSlideDust.Play();
                else
                    wallSlideDust.Stop();
                break;
            case EffectType.BurstRocks:
                if (enabled)
                    burstRocks.Play();
                else
                    burstRocks.Stop();
                break;
            case EffectType.DustLeft:

                break;
            case EffectType.DustRight:

                break;
            case EffectType.LowHealthLeak:
                if (enabled)
                    lowHealthLeak.Play();
                else
                    lowHealthLeak.Stop();
                break;
            case EffectType.HitLeft:
                if (enabled)
                    hitLeft.Play();
                else
                    hitLeft.Stop();
                break;
            case EffectType.HitRight:
                if (enabled)
                    hitRight.Play();
                else
                    hitRight.Stop();
                break;
            case EffectType.AshLeft:
                if (enabled)
                    ashLeft.Play();
                else
                    ashLeft.Stop();
                break;
            case EffectType.AshRight:
                if (enabled)
                    ashRight.Play();
                else
                    ashRight.Stop();
                break;
            case EffectType.Shade:
                if (enabled)
                    shade.Play();
                else
                    shade.Stop();
                break;
            case EffectType.RoarDust:
                if (enabled)
                    roarDust.Play();
                else
                    roarDust.Stop();
                break;
            case EffectType.RoarDustLil:
                if (enabled)
                    roarDustLil.Play();
                else
                    roarDustLil.Stop();
                break;
            case EffectType.DustJump:
                if (enabled)
                    dustJump.Play();
                else
                    dustJump.Stop();
                break;
            default:
                break;
        }
    }
}
