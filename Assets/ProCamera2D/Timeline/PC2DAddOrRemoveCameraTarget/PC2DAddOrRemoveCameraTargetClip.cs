#if UNITY_2017_1_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Com.LuisPedroFonseca.ProCamera2D;

[Serializable]
public class PC2DAddOrRemoveCameraTargetClip : PlayableAsset, ITimelineClipAsset
{
	public PC2DAddOrRemoveCameraTargetBehaviour template = new PC2DAddOrRemoveCameraTargetBehaviour();
	public ExposedReference<Transform> cameraTarget;

	public TimelineClip ClipReference { get; set; }

	public ClipCaps clipCaps
	{
		get { return ClipCaps.None; }
	}

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		var playable = ScriptPlayable<PC2DAddOrRemoveCameraTargetBehaviour>.Create(graph, template);
		PC2DAddOrRemoveCameraTargetBehaviour clone = playable.GetBehaviour();
		clone.ClipReference = ClipReference;
		clone.cameraTarget = cameraTarget.Resolve(graph.GetResolver());
		return playable;
	}
}
#endif