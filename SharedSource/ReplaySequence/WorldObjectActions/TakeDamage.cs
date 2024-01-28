using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

#if CLIENT
using DefconNull.Rendering;
using Microsoft.Xna.Framework;
#endif

namespace DefconNull.ReplaySequence.WorldObjectActions;
public static partial class WorldObjectManager
{

    public class TakeDamage : SequenceAction
    {
        protected bool Equals(TakeDamage other)
        {
            return Dmg == other.Dmg && DetResistance == other.DetResistance && EnvResistance == other.EnvResistance && ObjID == other.ObjID && Position.Equals(other.Position) && Ignores.SequenceEqual(other.Ignores);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TakeDamage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Dmg;
                hashCode = (hashCode * 397) ^ DetResistance;
                hashCode = (hashCode * 397) ^ EnvResistance;
                hashCode = (hashCode * 397) ^ ObjID;
                hashCode = (hashCode * 397) ^ Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Ignores.GetHashCode();
                return hashCode;
            }
        }

        public override SequenceType GetSequenceType()
        {
            return SequenceType.TakeDamage;
        }

        public override BatchingMode Batching => BatchingMode.Sequential;
        public int Dmg;
        public int DetResistance;
        public int EnvResistance;
        public int ObjID = -1;
        public Vector2Int Position = new Vector2Int(-1, -1);
        private List<string> Ignores = new List<string>();

        public static TakeDamage Make(int dmg, int detResistance, int envRes, Vector2Int position, List<string> ignores)
        {
            var t = GetAction(SequenceType.TakeDamage) as TakeDamage;
            t.ObjID = -1;
            t.Dmg = dmg;
            t.EnvResistance = envRes;
            t.DetResistance = detResistance;
            t.Position = position;
            t.Ignores = ignores;
            return t;
        }



        public static TakeDamage Make(int dmg, int detResistance, int objID, int envRes = -999)
        {
            var t = GetAction(SequenceType.TakeDamage) as TakeDamage;
            t.Dmg = dmg;
            t.DetResistance = detResistance;
            t.EnvResistance = detResistance;
            if (envRes != -999)
                t.EnvResistance = envRes;
            t.ObjID = objID;
            t.Position = new Vector2Int(-1, -1);
            return t;
        }

        public override bool ShouldDo()
        {

            if (ObjID != -1)
            {
                if (GetObject(ObjID) == null)
                {
                    Log.Message("UNITS","object with id: " + ObjID + " not found for purposes of takedamage");
                    return false;
                }
                return true;
            }
			

		
			
            if (Position == new Vector2Int(-1, -1)) return false;
            var tile = WorldManager.Instance.GetTileAtGrid(Position);
            if (tile.UnitAtLocation == null) return false;
            var obj = tile.UnitAtLocation;
            if (Ignores.Contains(obj!.Type.Name)) return false;

            return true;
        }
#if SERVER
		public override bool ShouldSendToPlayerServerCheck(bool player1)
		{
			if (GetTargetObject() == null) return false;
			return GetTargetObject()!.ShouldBeVisibilityUpdated(team1: player1);
		}
#endif


        public WorldObject? GetTargetObject()
        {
            if (ObjID != -1)
            {
                return GetObject(ObjID);
            }
            else
            {
                var u = WorldManager.Instance.GetTileAtGrid(Position).UnitAtLocation;
                if (u is null) return null;
                if (Ignores.Contains(u.Type.Name)) return null;
                return u.WorldObject;
            }

            return null;
        }

        protected override void RunSequenceAction()
        {

            var dmg = Dmg;
				
            var obj = GetTargetObject();
            if (obj == null) throw new Exception("object not found for damage");
	
            Log.Message("UNITS",obj + " got hit " + obj.TileLocation.Position);
				
            if (obj.UnitComponent != null)
            {
                var unit = obj.UnitComponent;
                Log.Message("UNITS","Taking damage: " + Dmg + " with det resis: " + DetResistance);
                if (unit.Determination > 0)
                {
                    Log.Message("UNITS","blocked by determination");
                    dmg = Dmg - DetResistance;

                }

                if (dmg <= 0)
                {
                    Log.Message("UNITS","0 damage");
                    return;
                }


                obj.Health -= dmg;
			
#if CLIENT
                new PopUpText("Damage: " + dmg, obj.TileLocation.Position, Color.Red, 0.8f);
#endif
                Log.Message("UNITS","unit hit for: " + dmg);
                Log.Message("UNITS","outcome: health=" + obj.Health);
                if (obj.Health <= 0)
                {
                    Log.Message("UNITS","dead");
                    unit.ClearOverWatch();
#if CLIENT
                    Audio.PlaySound("death",obj.TileLocation.Position);
#endif
                }
                else
                {
#if CLIENT
                    Audio.PlaySound("grunt",obj.TileLocation.Position);
#endif
                }
            }
            else
            {
                int dmgNoResist = Dmg - EnvResistance;

                //	

#if CLIENT
                new PopUpText("Damage: " + dmgNoResist, obj.TileLocation.Position, Color.Gray, 0.4f);
#endif
                obj.Health -= dmgNoResist;
                Log.Message("UNITS","object hit for: " + dmgNoResist);
                Log.Message("UNITS","outcome: health=" + obj.Health);
			
            }

				
            if (obj.LifeTime != -100)
            {
                obj.LifeTime -= dmg;
            }
            if (obj.Health <= 0 || (obj.LifeTime <= 0 && obj.LifeTime != -100))
            {
                Destroy(obj);
            }

        }
	
	

        protected override void SerializeArgs(Message message)
        {
            message.Add(Dmg);
            message.Add(DetResistance);
            message.Add(EnvResistance);
            message.Add(ObjID);
            message.Add(Position);
        }

        protected override void DeserializeArgs(Message args)
        {
            Dmg = args.GetInt();
            DetResistance = args.GetInt();
            EnvResistance = args.GetInt();
            ObjID = args.GetInt();
            Position = args.GetSerializable<Vector2Int>();
        }

#if CLIENT
        public override void DrawDesc(Vector2 pos, SpriteBatch batch)
        {
            if (GetTargetObject().UnitComponent == null)
            {
                if (EnvResistance != 0)
                {
                    batch.DrawText("DMG :" + Dmg + "(" + (Dmg - EnvResistance) + ")", pos, Color.White);
                    return;
                }

                batch.DrawText("DMG : " + Dmg, pos, Color.White);
                return;
            }else if ( GetTargetObject().UnitComponent.Determination > 0)
            {
                batch.DrawText("DMG :" + Dmg+"("+(Dmg-DetResistance)+")", pos, Color.White);
                return;
            }

            batch.DrawText("DMG : " + Dmg, pos, Color.White);
        }

        public override void Preview(SpriteBatch spriteBatch)
        {

		
            var obj = GetTargetObject();

            if (obj !=null && obj!.IsVisible())
            {
                obj!.PreviewData.totalDmg += Dmg;


                Texture2D sprite = obj.GetTexture();
                spriteBatch.Draw(sprite, obj.GetDrawTransform().Position, Color.Red * 0.8f);


                //this is scuffed
                if (obj.UnitComponent == null )
                {
                    obj!.PreviewData.finalDmg += Dmg - EnvResistance;
                    obj!.PreviewData.determinationBlock += EnvResistance;
                }else if (obj.UnitComponent.Determination > 0)
                {
                    obj!.PreviewData.finalDmg += Dmg - DetResistance;
                    obj!.PreviewData.determinationBlock += DetResistance;
                }
                else
                {
                    obj!.PreviewData.finalDmg += Dmg;
                }
            }

        }
#endif

        public override string ToString()
        {
            return "TakeDamage: " + Dmg + " to " + ObjID +" at "+Position;
        }
    }
}