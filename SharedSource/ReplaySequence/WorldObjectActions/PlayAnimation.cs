using System.Threading;
using Riptide;

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class PlayAnimation : SequenceAction
{
    public override SequenceType GetSequenceType()
    {
        return SequenceType.PlayAnimation;
    }

    protected override void RunSequenceAction()
    {
#if CLIENT
        var obj = WorldObjectManager.GetObject(_objId);
        obj.StartAnimation(_animation);
        while(obj.IsAnimating) 
        {
            Thread.Sleep(100);
        }
#endif
        
    }

    protected override void SerializeArgs(Message message)
    {
        message.AddInt(_objId);
        message.AddString(_animation);
    }

    protected override void DeserializeArgs(Message message)
    {
        _objId = message.GetInt();
        _animation = message.GetString();
    }
#if SERVER
    public override bool ShouldSendToPlayerServerCheck(bool player1)
    {
        return  WorldObjectManager.GetObject(_objId).ShouldBeVisibilityUpdated(player1);
    }
#endif
    

    int _objId = -1;
    string _animation = "";
    public static PlayAnimation Make(int objId, string animation)
    {
   
        PlayAnimation t = (GetAction(SequenceType.PlayAnimation) as PlayAnimation)!;
        t._objId = objId;
        t._animation = animation;
        return t;
    }
}