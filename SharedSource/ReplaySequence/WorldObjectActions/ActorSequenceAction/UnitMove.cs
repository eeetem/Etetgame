using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Riptide;
#if CLIENT
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
#endif

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class UnitMove : UnitSequenceAction
{
    private List<Vector2Int> Path = new List<Vector2Int>();

    protected bool Equals(UnitMove other)
    {
        return base.Equals(other) && Path.SequenceEqual(other.Path);
    }

    public override Message? MakeTestingMessage()
    {
        Requirements = new TargetingRequirements();
        Requirements.Position = new Vector2Int(12, 5);
        Requirements.TypesToIgnore = new List<string>();
        Requirements.TypesToIgnore.Add("Unit");
        Requirements.ActorID = 123;
        Path.Add(new Vector2Int(12, 5));
        Path.Add(new Vector2Int(12, 6));
        Path.Add(new Vector2Int(12, 7));
        var m = Message.Create();
        Serialize(m);
        return m;

    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((UnitMove) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (base.GetHashCode() * 397) ^ Path.GetHashCode();
        }
    }

    public static UnitMove Make(int actorID,List<Vector2Int> path)
    {
        UnitMove t = (GetAction(SequenceType.Move) as UnitMove)!;
        t.Path = path;
        t.Requirements = new TargetingRequirements(actorID);
        return t;
    }

    public override SequenceType GetSequenceType()
    {
        return SequenceType.Move;
    }
#if SERVER
    public override bool ShouldSendToPlayerServerCheck(bool player1)
    {
        if (base.ShouldSendToPlayerServerCheck(player1)) return true;
        foreach (var pos in Path)
        {
            var wtile = (WorldTile) WorldManager.Instance.GetTileAtGrid(pos);
            if( wtile.IsVisible(team1: player1))return true;
        }

        return false;
    }


    protected override SequenceAction FilterForPlayerInternal(bool player1)
    { 
        Console.WriteLine("filtering for player: "+player1+" "+Actor.WorldObject.ID);
        if (Actor.IsPlayer1Team != player1)
        {
            Console.WriteLine("performing filter");
            var ret = Make(Requirements.ActorID, Path);
            List<Vector2Int> newPath = new List<Vector2Int>();
            bool justVisible = false;
            foreach (var p in ret.Path)
            {
                if (WorldManager.Instance.GetTileAtGrid(p).IsVisible(team1: player1))
                {
                    newPath.Add(p);
                    justVisible = true;
                }else if (justVisible)
                {
                    newPath.Add(p);
                    justVisible = false;
                }
            }
            ret.Path = newPath;
            return ret;
        }
        Console.WriteLine("no filter needed");
        return base.FilterForPlayerInternal(player1);
       
    }
#endif
	

    public override bool ShouldDo()
    {
        return Actor != null && !Actor.Paniced;
    }

    protected override void RunSequenceAction()
    {

        while (Path.Count >0)
        {

            if(Path[0] != Actor.WorldObject.TileLocation.Position)
                Actor.WorldObject.Face(Utility.Vec2ToDir(Path[0] - Actor.WorldObject.TileLocation.Position));
            WorldManager.Instance.MakeFovDirty();

#if CLIENT
				Thread.Sleep((int) (WorldManager.Instance.GetTileAtGrid(Path[0]).TraverseCostFrom(Actor.WorldObject.TileLocation.Position)*200));
#else
            while (WorldManager.Instance.FovDirty)//make sure we get all little turns and moves updated serverside
                Thread.Sleep(10);

#endif
			
		
            Console.WriteLine("moving to: "+Path[0]+" path size left: "+Path.Count);
					
            Actor.WorldObject.TileLocation.UnitAtLocation = null;
            var newTile = WorldManager.Instance.GetTileAtGrid(Path[0]);
            Actor.WorldObject.TileLocation = newTile;
            newTile.UnitAtLocation = Actor;

            Path.RemoveAt(0);


		

#if CLIENT
					if (Actor.WorldObject.IsVisible())
					{
						Audio.PlaySound("footstep", Utility.GridToWorldPos(Actor.WorldObject.TileLocation.Position));
					}
#endif
					
            if(Path.Count > 0)
                Console.WriteLine("queued movement task to: "+Path[0]+" path size left: "+Path.Count);
	
        }
        Console.WriteLine("movement task is done for: "+Actor.WorldObject.ID+" "+Actor.WorldObject.TileLocation.Position);
			
        Actor.canTurn = true;


    }
	

    protected override void SerializeArgs(Message message)
    {
        base.SerializeArgs(message);
        message.AddSerializables<Vector2Int>(Path.ToArray());
    }

    protected override void DeserializeArgs(Message message)
    {
        base.DeserializeArgs(message);
        Path = message.GetSerializables<Vector2Int>().ToList();
    }

#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}