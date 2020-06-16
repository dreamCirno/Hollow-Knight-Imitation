using UnityEngine;

public class SlideBehaviour : StateMachineBehaviour
{
    CharacterController2D character;
    CharacterEffect effecter;
    CharacterAudio audioPlayer;

    private void Awake()
    {
        audioPlayer = FindObjectOfType<CharacterAudio>();
        effecter = FindObjectOfType<CharacterEffect>();
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (character == null)
            character = animator.GetComponent<CharacterController2D>();
        audioPlayer.Play(CharacterAudio.AudioType.WallSlide, true);
        effecter.DoEffect(CharacterEffect.EffectType.WallSlideDust, true);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (character == null)
            character = animator.GetComponent<CharacterController2D>();
        audioPlayer.Play(CharacterAudio.AudioType.WallSlide, false);
        effecter.DoEffect(CharacterEffect.EffectType.WallSlideDust, false);
        character.SlideWall_ResetJumpCount();
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
