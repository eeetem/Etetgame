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
        if (obj.GetType() != GetType()) return false;
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
            if( wtile.IsVisible(Actor.WorldObject.GetMinimumVisibility(),team1: player1))return true;
        }

        return false;
    }

    public override void FilterForPlayer(bool player1)
    { 
        if (Actor.IsPlayer1Team != player1)
        {
            List<Vector2Int> newPath = new List<Vector2Int>();
            bool justVisible = ((WorldTile)Actor.WorldObject.TileLocation).IsVisible(team1: player1);

            foreach (var p in Path)
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
            Path = newPath;
        }
    }

    public override List<SequenceAction> GenerateInfoActions(bool player1)
    {
        var b =  base.GenerateInfoActions(player1);
        if (Actor.IsPlayer1Team != player1)//spot the unit that will walk in
        {
            if (Path.Count > 0)
            {
                b.Add(SpotUnit.Make(Actor.WorldObject.ID, Path[0],player1));
            }
        }
        else//spot everyone else the unit will see
        {
            Vector2Int lastPos = Actor.WorldObject.TileLocation.Position;
            HashSet<Vector2Int> seenTiles = new HashSet<Vector2Int>();
            HashSet<int> seenUnits = new HashSet<int>();
            foreach (var spot in Path)
            {
                var tiles = WorldManager.Instance.GetVisibleTiles(spot, Utility.Vec2ToDir(spot-lastPos), Actor.GetSightRange(), Actor.Crouching);
                foreach (var loc in tiles)
                {
                    var tile = WorldManager.Instance.GetTileAtGrid(loc.Key);
                    if (tile.UnitAtLocation != null && tile.UnitAtLocation.WorldObject.GetMinimumVisibility() <= loc.Value && tile.UnitAtLocation.IsPlayer1Team != player1)
                        seenUnits.Add(tile.UnitAtLocation.WorldObject.ID);
                    if(loc.Value>Visibility.None)
                        seenTiles.Add(loc.Key);
                }
                lastPos = spot;
            }

            foreach (var u in seenUnits)
            {
                b.Add(SpotUnit.Make(u, player1));
            }
            foreach (var t in seenTiles)
            {
                b.Add(TileUpdate.Make(t,false));
            }
           
        }

        return b;
    }
#endif
	

    public override bool ShouldDo()
    {
        return Actor != null && !Actor.Panicked;
    }

    protected override void RunSequenceAction()
    {
 
        Actor.canTurn = true;
        Log.Message("UNITS", "starting movement task for: " + Actor.WorldObject.ID + " " + Actor.WorldObject.TileLocation.Position+ " path size: "+Path.Count);
        WorldManager.Instance.MakeFovDirty();
        while (Path.Count >0)
        {
            

            if (Actor.WorldObject.TileLocation != null)
            {
                if (Path[0] != Actor.WorldObject.TileLocation.Position)
                    Actor.WorldObject.Face(Utility.Vec2ToDir(Path[0] - Actor.WorldObject.TileLocation.Position));
                
#if CLIENT
                Thread.Sleep((int) (WorldManager.Instance.GetTileAtGrid(Path[0]).TraverseCostFrom(Actor.WorldObject.TileLocation.Position)*200));
#else
                while (WorldManager.Instance.FovDirty) //make sure we get all little turns and moves updated serverside
                    Thread.Sleep(10);
#endif
            }

            Log.Message("UNITS","moving to: "+Path[0]+" path size left: "+Path.Count);
					
            Actor.MoveTo(Path[0]);

            Path.RemoveAt(0);


		

#if CLIENT
            if (Actor.WorldObject.IsVisible())
            {
                Audio.PlaySound("footstep", Utility.GridToWorldPos(Actor.WorldObject.TileLocation.Position));
            }
#endif
					
            if(Path.Count > 0)
                Log.Message("UNITS","queued     movement task to: "+Path[0]+" path size left: "+Path.Count);
	
        }
        Log.Message("UNITS","movement task is done for: "+Actor.WorldObject.ID+" "+Actor.WorldObject.TileLocation.Position);
			
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