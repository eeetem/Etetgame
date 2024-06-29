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
			return Dmg == other.Dmg && DetResistance == other.DetResistance && EnvResistance == other.EnvResistance && ObjID == other.ObjID;
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
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
				return hashCode;
			}
		}

		public override SequenceType GetSequenceType()
		{
			return SequenceType.TakeDamage;
		}

		public override BatchingMode Batching => BatchingMode.AsyncBatchSameType;
		public int Dmg;
		public int DetResistance;
		public int EnvResistance;
		public int ObjID = -1;


		public static TakeDamage Make(int dmg, int detResistance, int objID, int envRes = -999)
		{
			var t = GetAction(SequenceType.TakeDamage) as TakeDamage;
			t.Dmg = dmg;
			t.DetResistance = detResistance;
			t.EnvResistance = detResistance;
			if (envRes != -999)
				t.EnvResistance = envRes;
			t.ObjID = objID;
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

			return GetObject(ObjID);
				
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
				unit.ClearOverWatch();
			}
			else
			{
				int dmgNoResist = Dmg - EnvResistance;

				//	

#if CLIENT
				new PopUpText("Damage: " + dmgNoResist, obj.TileLocation.Position, Color.Gray, 0.4f);
#endif
				obj.Health -= dmgNoResist;
				Log.Message("WORLD OBJECT MANAGER","object "+ObjID+" hit for: " + dmgNoResist+"\n "+"outcome: health=" + obj.Health);

			
			}

			if (obj.Health <= 0)
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
		}

		protected override void DeserializeArgs(Message args)
		{
			Dmg = args.GetInt();
			DetResistance = args.GetInt();
			EnvResistance = args.GetInt();
			ObjID = args.GetInt();
		}

#if CLIENT
		public override void DrawConsequence(Vector2 pos, SpriteBatch batch)
		{
			int resistance = DetResistance;
			Texture2D damageSprite = TextureManager.GetTexture("HoverHud/Consequences/genericDamage");
			Texture2D blockSprite = TextureManager.GetTexture("HoverHud/Consequences/detShield");
			Color resistColor = Color.White;
			int sumDamage = Dmg - resistance;
			if (GetTargetObject().UnitComponent == null)
			{
				resistance = EnvResistance;
				//damageSprite = TextureManager.GetTexture("HoverHud/Consequences/cover");
				blockSprite = TextureManager.GetTexture("HoverHud/Consequences/envShield");
			//	dmgColor = Color.Orange;
			}else if (GetTargetObject().UnitComponent.Determination <= 0)
			{
				bool supressed = true;
				resistColor = Color.Red;
				sumDamage = Dmg;
				resistance = 0;
			}
			
			pos += new Vector2(0, 0);
			Vector2 offset = new Vector2(damageSprite.Width-2,0);
			batch.DrawNumberedIcon(Dmg.ToString(), damageSprite, pos+offset, Color.White);
			batch.DrawNumberedIcon(resistance.ToString(), blockSprite, pos+offset*2f, Color.Red,resistColor);
			batch.DrawText("=", pos+offset*2+new Vector2(26,7),2f, Color.White);
			batch.DrawNumberedIcon(sumDamage.ToString(), TextureManager.GetTexture("HoverHud/Consequences/recivedDamage"), pos+offset*3+new Vector2(8,0), Color.White);

		}

		public override void DrawTooltip(Vector2 pos, float scale, SpriteBatch batch)
		{
			

			Texture2D blockSprite = TextureManager.GetTexture("HoverHud/Consequences/detShield");
			string blockName = "  Damage [Red]dodged[-] due to determination\n";
			bool supressed = false;
			if (GetTargetObject().UnitComponent == null)
			{
				blockSprite = TextureManager.GetTexture("HoverHud/Consequences/envShield");
				blockName = "  Damage [Red]resisted[-] by environment resistance\n";
			}else if (GetTargetObject().UnitComponent.Determination <= 0)
			{
				supressed = true;
			}

			string tip = "" +
			             "           Damage:\n" +
			             "  [Green]Base damage[-] received\n" + blockName;
			if (supressed)
			{
				tip += "  Resistance [Red]bypassed[-] x- lack of determination\n";
			}

			tip += "  [Green]Final Damage[-]";
			batch.DrawText( tip, pos, scale, Color.White);
			batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/genericDamage"), pos + new Vector2(0, 6)*scale,scale/2f,Color.White);
			batch.Draw(blockSprite ,pos + new Vector2(0, 16*scale),scale/2f,Color.White);
			if (supressed)
			{
				batch.Draw(blockSprite ,pos + new Vector2(0, 29)*scale,scale/2f,Color.Red);
				batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/recivedDamage"), pos + new Vector2(0, 42)*scale,scale/2f,Color.White);
			}
			else
			{
				batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/recivedDamage"), pos + new Vector2(0, 30)*scale,scale/2f,Color.White);
			}

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

				if (obj.PreviewData.finalDmg >= obj.Health && obj.Type.DestructionConseqences != null)
				{
					var cons = obj.Type.DestructionConseqences.GetApplyConsequnces(obj,obj);
					foreach (var con in cons)
					{
						con.Preview(spriteBatch);
					}
				}
	
			}

		}
#endif

		public override string ToString()
		{
			return "TakeDamage: " + Dmg + " to " + ObjID;
		}
	}
}