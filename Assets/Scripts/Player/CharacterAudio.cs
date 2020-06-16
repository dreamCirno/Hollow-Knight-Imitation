using UnityEngine;

public class CharacterAudio : MonoBehaviour
{
    [SerializeField] AudioSource mainEffectAudioSouce = null;
    [Header("Audio Sources")]
    [SerializeField] AudioSource hardLandingAudioSource = null;
    [SerializeField] AudioSource jumpAudioSource = null;
    [SerializeField] AudioSource takeHitAudioSource = null;
    [SerializeField] AudioSource landingAudioSource = null;
    [SerializeField] AudioSource backDashAudioSource = null;
    [SerializeField] AudioSource dashAudioSource = null;
    [SerializeField] AudioSource fallingAudioSource = null;
    [SerializeField] AudioSource footstepsRunAudioSource = null;
    [SerializeField] AudioSource footstepsWalkAudioSource = null;
    [SerializeField] AudioSource wallSlideAudioSource = null;
    [SerializeField] AudioSource nailArtChargeAudioSource = null;
    [SerializeField] AudioSource nailArtReadyAudioSource = null;
    [SerializeField] AudioSource slugWalkAudioSource = null;
    [SerializeField] AudioSource superDashingAudioSource = null;
    [SerializeField] AudioSource superDashChargeAudioSource = null;
    [SerializeField] AudioSource dreamNailAudioSource = null;
    [SerializeField] AudioSource swimAudioSource = null;
    [SerializeField] AudioSource wallJumpAudioSource = null;
    [SerializeField] AudioSource heroDamageAudioSource = null;
    [SerializeField] AudioSource heroWingsAudioSource = null;
    [Header("Audio Clips")]
    [SerializeField] AudioClip swordHitReject = null;

    public enum AudioType
    {
        HardLanding, Jump, TakeHit, Landing, BackDash, Dash, Falling, FootstepsRun, FoorstepsWalk,
        WallSlide, NailArtCharge, NailArtReady, SlugWalk, SuperDashing, SuperDashCharge, DreamNail,
        Swim, WallJump, HeroDamage, HeroWings,
    }

    public enum AudioClipType
    {
        SwordHitReject,
    }

    public void Play(AudioType audioType, bool playState)
    {
        AudioSource audioSource = null;
        switch (audioType)
        {
            case AudioType.HardLanding:
                audioSource = hardLandingAudioSource;
                break;
            case AudioType.Jump:
                audioSource = jumpAudioSource;
                break;
            case AudioType.TakeHit:
                audioSource = takeHitAudioSource;
                break;
            case AudioType.Landing:
                audioSource = landingAudioSource;
                break;
            case AudioType.BackDash:
                audioSource = backDashAudioSource;
                break;
            case AudioType.Dash:
                audioSource = dashAudioSource;
                break;
            case AudioType.Falling:
                audioSource = fallingAudioSource;
                break;
            case AudioType.FootstepsRun:
                audioSource = footstepsRunAudioSource;
                break;
            case AudioType.FoorstepsWalk:
                audioSource = footstepsWalkAudioSource;
                break;
            case AudioType.WallSlide:
                audioSource = wallSlideAudioSource;
                break;
            case AudioType.NailArtCharge:
                audioSource = nailArtChargeAudioSource;
                break;
            case AudioType.NailArtReady:
                audioSource = nailArtReadyAudioSource;
                break;
            case AudioType.SlugWalk:
                audioSource = slugWalkAudioSource;
                break;
            case AudioType.SuperDashing:
                audioSource = superDashingAudioSource;
                break;
            case AudioType.SuperDashCharge:
                audioSource = superDashChargeAudioSource;
                break;
            case AudioType.DreamNail:
                audioSource = dreamNailAudioSource;
                break;
            case AudioType.Swim:
                audioSource = swimAudioSource;
                break;
            case AudioType.WallJump:
                audioSource = wallJumpAudioSource;
                break;
            case AudioType.HeroDamage:
                audioSource = heroDamageAudioSource;
                break;
            case AudioType.HeroWings:
                audioSource = heroWingsAudioSource;
                break;
        }
        if (audioSource != null)
        {
            if (playState)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }

    public void PlayJump()
    {
        jumpAudioSource.Play();
    }

    public void PlayOneShot(AudioClipType audioType)
    {
        switch (audioType)
        {
            case AudioClipType.SwordHitReject:
                mainEffectAudioSouce.PlayOneShot(swordHitReject);
                break;
        }
    }
}
