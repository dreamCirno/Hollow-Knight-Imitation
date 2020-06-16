using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstLandingBehaviour : StateMachineBehaviour
{
    CharacterAudio sound;
    ParticleSystem flockParticle;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (sound == null)
            FindObjectOfType<CharacterAudio>().Play(CharacterAudio.AudioType.HardLanding, true);
        else
            sound.Play(CharacterAudio.AudioType.HardLanding, true);
        // 相机震动
        var shakePreset = ProCamera2DShake.Instance.ShakePresets[0];
        ProCamera2DShake.Instance.Shake(shakePreset);
        GameObject flock = GameObject.Find("Flock");
        if (flock != null)
        {
            flockParticle = GameObject.Find("Flock").GetComponent<ParticleSystem>();
            flockParticle.Play();
        }
        FindObjectOfType<SoulOrb>().DelayShowOrb(2);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("FirstLanding", true);
        FindObjectOfType<GameManager>().SetEnableInput(true);
    }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
