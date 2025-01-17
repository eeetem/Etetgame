﻿using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;
#if CLIENT
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
using Microsoft.Xna.Framework.Input;
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

    public override BatchingMode Batching => BatchingMode.NonBlockingAlone;

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
        if (Path.Count == 0) return false;
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

    public override List<SequenceAction> SendPrerequesteInfoToPlayer(bool player1)
    {
        var b =  base.SendPrerequesteInfoToPlayer(player1);
        if (Actor.IsPlayer1Team != player1)//spot the unit that will walk in
        {
            if (Path.Count > 0)
            {
                GameManager.SpotUnit(GameManager.GetPlayer(player1)!, Actor.WorldObject.ID, (Path[0], Actor.WorldObject.GetData()));
            }
        }
        else//spot everyone else the unit will see
        {
            Vector2Int lastPos = Actor.WorldObject.TileLocation.Position;
            HashSet<Vector2Int> seenTiles = new HashSet<Vector2Int>();
            HashSet<int> seenUnits = new HashSet<int>();
            HashSet<int> alreadySeenUnitsToUpdate = new HashSet<int>();
            var p = GameManager.GetPlayer(player1);
            if (p == null) return b;
            
            Direction lastFace = Actor.WorldObject.Facing;
            foreach (var spot in Path)
            {
                if (spot == lastPos)
                    continue;
                Direction face = Utility.Vec2ToDir(spot-lastPos);
                var tiles = WorldManager.Instance.GetVisibleTiles(spot, face, Actor.GetSightRange(), Actor.Crouching);
                var moreTiles = WorldManager.Instance.GetVisibleTiles(spot, lastFace, Actor.GetSightRange(), Actor.Crouching);
                foreach (var keyValuePair in moreTiles)
                {
                    if (!tiles.ContainsKey(keyValuePair.Key))
                    {
                        tiles.TryAdd(keyValuePair.Key,keyValuePair.Value);
                    }else if (keyValuePair.Value > tiles[keyValuePair.Key])
                    {
                        tiles[keyValuePair.Key] = keyValuePair.Value;
                    }
                }
                foreach (var loc in tiles)
                {
                    var tile = WorldManager.Instance.GetTileAtGrid(loc.Key);
                    if (tile.UnitAtLocation != null)
                    {
                        if (tile.UnitAtLocation.WorldObject.GetMinimumVisibility() <= loc.Value && tile.UnitAtLocation.IsPlayer1Team != player1)
                            seenUnits.Add(tile.UnitAtLocation.WorldObject.ID);
                        
                    }
                    foreach (var knownUnit in p.KnownUnitPositions.ToList())
                    {
                        if (knownUnit.Value.Item1 == loc.Key && knownUnit.Value.Item2.UnitData!.Value.Team1 != player1)
                        {
                            var realUnit = WorldObjectManager.GetObject(knownUnit.Key);
                            if (realUnit != null && realUnit.TileLocation.Position != loc.Key)
                            {
                                if (knownUnit.Value.Item2.UnitData!.Value.Crouching && loc.Value > Visibility.Partial)
                                {
                                    p.KnownUnitPositions.Remove(knownUnit.Key);
                                }
                                else if (!knownUnit.Value.Item2.UnitData.Value.Crouching && loc.Value > Visibility.None)
                                {
                                    p.KnownUnitPositions.Remove(knownUnit.Key);
                                }
                            }
                                    
                        }
                    }
                    
                    if(loc.Value>Visibility.None)
                        seenTiles.Add(loc.Key);
                    
                }
                lastPos = spot;
                lastFace = face;
            }

            foreach (var u in seenUnits)
            {
                var unit = WorldObjectManager.GetObject(u); ;
                GameManager.SpotUnit(GameManager.GetPlayer(player1)!,u,(unit!.TileLocation.Position, unit.GetData()));
            }
            
          
            foreach (var t in seenTiles)
            {
                b.Add(TileUpdate.Make(t,false,true));
            }
           
           
        }
        b.Add(UnitUpdate.Make(player1));
        return b;
    }
#endif
	

    public override bool ShouldDo()
    {
        if (Path.Count == 0) return false;
        return Actor != null && !Actor.Panicked;
        
    }

    protected override void RunSequenceAction()
    {
        bool hasClicked = false;
        Actor.CanTurn = true;
        Log.Message("UNITS", "starting movement task for: " + Actor.WorldObject.ID + " " + Actor.WorldObject.TileLocation.Position+ " path size: "+Path.Count);
        int walk = 0;
        while (Path.Count >0)
        {
            walk++;
            if (Actor.WorldObject.TileLocation != null)
            {

                int sleepTime = 0;
#if CLIENT
                //(int) *
                var anim = Actor.Type.GetAnimation(Actor.WorldObject.spriteVariation, "Walk", Actor.WorldObject.GetExtraState());
                sleepTime = (int) ((1000f / anim.Item2) *  anim.Item1);
                sleepTime += 25;
                if (anim.Item1 == 0)
                {
                    sleepTime = (int) (WorldManager.Instance.GetTileAtGrid(Path[0]).TraverseCostFrom(Actor.WorldObject.TileLocation.Position)*300f);
                }
                if(Mouse.GetState().LeftButton == ButtonState.Pressed)
                    sleepTime = 25;
#endif

#if CLIENT
                Thread.Sleep(sleepTime/2); 
     
#endif
                
                if (Path[0] != Actor.WorldObject.TileLocation.Position)
                    Actor.WorldObject.Face(Utility.Vec2ToDir(Path[0] - Actor.WorldObject.TileLocation.Position));
#if CLIENT
                Thread.Sleep(sleepTime/2); 
#endif
            }

            Log.Message("UNITS","moving to: "+Path[0]+" path size left: "+Path.Count);
					
            Actor.MoveTo(Path[0]);
            

            Path.RemoveAt(0);


		

#if CLIENT
      
            if(Path.Count>0)
                Actor.WorldObject.StartAnimation("Walk");


            if (Actor.WorldObject.IsVisible())
            {
                Audio.PlaySound("footstep", Utility.GridToWorldPos(Actor.WorldObject.TileLocation.Position));
            }
#endif
					
            if(Path.Count > 0)
                Log.Message("UNITS","queued     movement task to: "+Path[0]+" path size left: "+Path.Count);
	
        }
        Log.Message("UNITS","movement task is done for: "+Actor.WorldObject.ID+" "+Actor.WorldObject.TileLocation.Position);
			
        Actor.CanTurn = true;
#if SERVER
        GameManager.ShouldRecalculateUnitPositions = true;
        GameManager.ShouldUpdateUnitPositions = true;
#endif

    }
	

    protected override void SerializeArgs(Message message)
    {
        base.SerializeArgs(message);
        message.AddSerializables(Path.ToArray());
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