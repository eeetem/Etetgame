using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using CommonData;

namespace MultiplayerXeno
{
	public static partial class PrefabManager
	{
			public static Dictionary<string, WorldObjectType> Prefabs = new Dictionary<string, WorldObjectType>();


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
					controllableType = new ControllableType();
					controllableType.MoveRange = int.Parse(contollableObj.Attributes?["moveRange"]?.InnerText ?? "4");
					controllableType.WeaponRange = int.Parse(contollableObj.Attributes?["weaponRange"]?.InnerText ?? "4");
					controllableType.MaxHealth = int.Parse(contollableObj.Attributes?["health"]?.InnerText ?? "10");
					controllableType.MaxMovePoints = int.Parse(contollableObj.Attributes?["moves"]?.InnerText ?? "2");
					controllableType.MaxTurnPoints = int.Parse(contollableObj.Attributes?["turns"]?.InnerText ?? "2");
					controllableType.WeaponDmg = int.Parse(contollableObj.Attributes?["attack"]?.InnerText ?? "4");
					controllableType.RunAndGun = bool.Parse(contollableObj.Attributes?["rNg"]?.InnerText ?? "false");
				
				}

				WorldObjectType type = new WorldObjectType(name,controllableType);


				bool faceable = true;
				bool edge = false;
				bool surface = false;
				bool impassible = false;
				Cover cover = Cover.None;
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
				if (xmlObj.HasAttributes && xmlObj.Attributes["Cover"] != null)
				{
					cover = (Cover)int.Parse(xmlObj?.Attributes?["Cover"].InnerText);
				}

				type.Faceable = faceable;
				type.Cover = cover;
				type.Edge = edge;
				type.Surface = surface;
				type.Impassible = true;


#if CLIENT
				string stringoffset = xmlObj.GetElementsByTagName("sprite")[0].Attributes["offset"].InnerText;
				float x = float.Parse(stringoffset.Substring(0, stringoffset.IndexOf(",")));
				float y = float.Parse(stringoffset.Substring(stringoffset.IndexOf(",")+1, stringoffset.Length - stringoffset.IndexOf(",")-1));
				Vector2 Offset = new Vector2(x, y);
				var spritename = xmlObj.GetElementsByTagName("sprite")[0]?.Attributes["name"]?.InnerText;
				int spriteVariations = int.Parse(xmlObj.GetElementsByTagName("sprite")[0]?.Attributes["variations"]?.InnerText ?? "1");
#endif
			
				
				
				

#if CLIENT

				type.Transform = new Transform2();
				type.Transform.Position = Utility.GridToWorldPos(Offset);

				
#endif

			
				
				Prefabs.Add(name,type);
				
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
			
		}
	
	}
}