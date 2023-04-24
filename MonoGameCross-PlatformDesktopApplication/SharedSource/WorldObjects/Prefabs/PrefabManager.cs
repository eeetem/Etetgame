using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MultiplayerXeno;
using MultiplayerXeno.Items;

namespace MultiplayerXeno;

public static partial class PrefabManager
{
	public static Dictionary<string, WorldObjectType> WorldObjectPrefabs = new Dictionary<string, WorldObjectType>();
	public static Dictionary<string, UsableItem> UseItems = new Dictionary<string, UsableItem>();


	public static void MakePrefabs()
	{
		XmlDocument xmlDoc= new XmlDocument();
		xmlDoc.Load("ObjectData.xml"); 


		foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("object"))
		{

			string name = xmlObj.GetElementsByTagName("name")[0]?.InnerText;
			

			ControllableType? controllableType = null;
			XmlNode contollableObj = xmlObj.GetElementsByTagName("controllable")[0];
			if (contollableObj != null)
			{
				controllableType = new ControllableType(name);
				controllableType.MoveRange = int.Parse(contollableObj.Attributes?["moveRange"]?.InnerText ?? "4");
				controllableType.WeaponRange = int.Parse(contollableObj.Attributes?["weaponRange"]?.InnerText ?? "4");
				controllableType.MaxHealth = int.Parse(contollableObj.Attributes?["health"]?.InnerText ?? "10");
				controllableType.Maxdetermination = int.Parse(contollableObj.Attributes?["determination"]?.InnerText ?? "2");
				controllableType.InventorySize = int.Parse(contollableObj.Attributes?["inventory"]?.InnerText ?? "1");
				controllableType.MaxMovePoints = int.Parse(contollableObj.Attributes?["moves"]?.InnerText ?? "2");
				controllableType.MaxFirePoints = int.Parse(contollableObj.Attributes?["actions"]?.InnerText ?? "1");
				controllableType.WeaponDmg = int.Parse(contollableObj.Attributes?["attack"]?.InnerText ?? "4");
				controllableType.SupressionRange = int.Parse(contollableObj.Attributes?["supression"]?.InnerText ?? "1");
				controllableType.OverWatchSize = int.Parse(contollableObj.Attributes?["overwatch"]?.InnerText ?? "2");
				controllableType.SightRange = int.Parse(contollableObj.Attributes?["sightrange"]?.InnerText ?? "16");
				XmlNode extraction = ((XmlElement)contollableObj).GetElementsByTagName("action")[0];
				if (extraction != null)
				{
					var action = new Tuple<string, ActionType>(extraction.Attributes?["name"]?.InnerText,(ActionType)int.Parse(extraction.Attributes?["type"]?.InnerText));
					controllableType.extraActions.Add(action);
				}

			}

			WorldObjectType type = new WorldObjectType(name,controllableType);
				


			bool faceable = true;
			bool edge = false;
			bool surface = false;
			bool impassible = false;
			int MaxHealth = 10;
			Cover vcover = Cover.None;
			Cover scover = Cover.None;
			if (xmlObj.HasAttributes && xmlObj.Attributes["Faceable"] != null)
			{
				faceable = bool.Parse(xmlObj?.Attributes?["Faceable"].InnerText);
			}
			if (xmlObj.HasAttributes && xmlObj.Attributes["Edge"] != null)
			{
				edge = bool.Parse(xmlObj?.Attributes?["Edge"].InnerText);
			}
			if (xmlObj.HasAttributes && xmlObj.Attributes["Surface"] != null)
			{
				surface = bool.Parse(xmlObj?.Attributes?["Surface"].InnerText);
				if (xmlObj.HasAttributes && xmlObj.Attributes["Impassible"] != null)
				{
					impassible = bool.Parse(xmlObj?.Attributes?["Impassible"].InnerText);
				}
			}
			if (xmlObj.HasAttributes && xmlObj.Attributes["SCover"] != null)
			{
				scover = (Cover)int.Parse(xmlObj?.Attributes?["SCover"].InnerText);
			}
			if (xmlObj.HasAttributes && xmlObj.Attributes["VCover"] != null)
			{
				string innerHtml = xmlObj?.Attributes?["VCover"].InnerText;
				if (innerHtml.Contains("+"))
				{
					vcover = Cover.None;
					type.VisibilityObstructFactor = int.Parse(innerHtml.Replace("+",""));
				}
				else
				{
					vcover = (Cover)int.Parse(xmlObj?.Attributes?["VCover"].InnerText);	
				}

					
			}
			else if (scover != Cover.None)
			{
				vcover = scover;
			}
			if (xmlObj.HasAttributes && xmlObj.Attributes["Health"] != null)
			{
				MaxHealth = int.Parse(xmlObj?.Attributes?["Health"].InnerText);
			}
			if (xmlObj.HasAttributes && xmlObj.Attributes["lifetime"] != null)
			{
				type.lifetime = int.Parse(xmlObj?.Attributes?["lifetime"].InnerText);
			}

			type.Faceable = faceable;
			type.VisibilityCover = vcover;
			type.SolidCover = scover;
			type.Edge = edge;
			type.Surface = surface;
			type.Impassible = impassible;
			type.MaxHealth = MaxHealth;
			if(xmlObj.GetElementsByTagName("DestroyEffect").Count > 0){
				type.desturctionEffect = ParseEffect(xmlObj.GetElementsByTagName("DestroyEffect")[0]);	
			} 
			


#if CLIENT
			Vector2 Offset = new Vector2(-1.5f, -0.5f);
			var spritename = xmlObj.GetElementsByTagName("sprite")[0]?.Attributes["name"]?.InnerText;
			int spriteVariations = int.Parse(xmlObj.GetElementsByTagName("sprite")[0]?.Attributes["variations"]?.InnerText ?? "1");
#endif
			
				
				
				

#if CLIENT

			type.Transform = new Transform2();
			type.Transform.Position = Utility.GridToWorldPos(Offset);

				
#endif

			
				
			WorldObjectPrefabs.Add(name,type);
				
#if CLIENT
			if (spritename != null)
			{
				type.GenerateSpriteSheet(spritename,spriteVariations);//this is a bit inconsistent but eeeh
				if (type.Controllable != null)
				{
					type.Controllable.CrouchSpriteSheet = Utility.MakeSpriteSheet(TextureManager.GetTexture(spritename + "Crouch"),3,3);
				}

					
			}
			else
			{
				type.GenerateSpriteSheet(name,spriteVariations);
				if (type.Controllable != null)
				{
					type.Controllable.CrouchSpriteSheet = Utility.MakeSpriteSheet(TextureManager.GetTexture(name + "Crouch"),3,3);
				}
			}


#endif


		}
			

		foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("item"))
		{

				
			string? name = xmlObj.GetElementsByTagName("name")[0]?.InnerText;
				
			UsageMethod? usg = null;
			WorldEffect? eff = new WorldEffect();

			XmlNode? grenade = xmlObj.GetElementsByTagName("throwable")[0];
			if (grenade != null)
			{
					
				int throwRange = int.Parse(grenade.Attributes?["throwRange"]?.InnerText ?? "5");
				usg = new Throwable(throwRange);
					


			}

			if (usg == null)
			{
				throw new Exception("coulnt parse item: no usage method");
			}
			//make function
			eff = ParseEffect(xmlObj.GetElementsByTagName("effect")[0]);
			
			UsableItem itm = new UsableItem(name,usg,eff);
			UseItems.Add(name,itm);
				
		}
	}

	private static WorldEffect ParseEffect(XmlNode effect)
	{
		WorldEffect eff = new WorldEffect();
				
		eff.range = int.Parse(effect.Attributes?["range"]?.InnerText ?? "3");
		XmlNode? dmgitm = ((XmlElement) effect).GetElementsByTagName("damage")[0];
		if (dmgitm != null)
		{
			eff.dmg = int.Parse(dmgitm.Attributes?["dmg"]?.InnerText ?? "0");
			eff.detDmg = int.Parse(dmgitm.Attributes?["detDmg"]?.InnerText ?? "0");
		}
		XmlNode? placeItem = ((XmlElement) effect).GetElementsByTagName("place")[0];
		if(placeItem !=null)
		{
			string innerText = placeItem.Attributes?["name"]?.InnerText;
			if (innerText != null)
			{
				eff.placeItemPrefab = innerText;
						
			}
		}
#if CLIENT
		var extraction = ((XmlElement)effect).GetElementsByTagName("vfx");
		foreach (var elem in extraction)
		{
			var node = (XmlNode) elem;
					
			eff.effects.Add(new Tuple<string, string, string>(node.Attributes["name"].InnerText,node.Attributes["target"].InnerText,node.Attributes["speed"].InnerText));
		}

		var sfx = (((XmlElement)effect).GetElementsByTagName("sfx")[0])?.Attributes?["name"]?.InnerText;
		eff.sfx = sfx;
#endif

		return eff;
	}

}