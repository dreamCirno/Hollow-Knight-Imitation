#if UNITY_2017_1_OR_NEWER
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Com.LuisPedroFonseca.ProCamera2D;

[TrackColor(1f, 0f, 0.3403339f)]
[TrackClipType(typeof(PC2DAddOrRemoveCameraTargetClip))]
public class PC2DAddOrRemoveCameraTargetTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		foreach (var clip in GetClips())
		{
			var customClip = clip.asset as PC2DAddOrRemoveCameraTargetClip;
			if (customClip != null)
				customClip.ClipReference = clip;
		}

		return ScriptPlayable<PC2DAddOrRemoveCameraTargetMixerBehaviour>.Create(graph, inputCount);
	}
}
#endif