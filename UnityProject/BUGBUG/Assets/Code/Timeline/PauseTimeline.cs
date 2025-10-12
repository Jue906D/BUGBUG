using UnityEngine;
using UnityEngine.Playables;

public class PauseClip : PlayableBehaviour
{
    private bool triggered = false;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!triggered && info.weight > 0f)   // 1st enter
        {
            var director = playable.GetGraph().GetResolver() as PlayableDirector;
            director.Pause();
            triggered = true;
        }
    }
}

[CreateAssetMenu(menuName = "Timeline Custom/Pause Clip")]
public class PauseAsset : PlayableAsset
{
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        
        return ScriptPlayable<PauseClip>.Create(graph);
    }
}

public class PauseTimeline : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector m_Director;
    public void Pause()
    {
        m_Director.Pause();
    }
} 
