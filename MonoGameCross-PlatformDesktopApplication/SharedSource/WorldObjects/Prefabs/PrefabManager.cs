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
					controllableType.MaxHealth = int.Parse(contollableObj.Attributes?["health"]?.InnerText ?? "10");
					controllableType.RunAndGun = bool.Parse(contollableObj.Attributes?["rNg"]?.InnerText ?? "false");
				}

				WorldObjectType type = new WorldObjectType(name,controllableType);


				bool faceable = true;
				bool edge = false;
				bool surface = false;
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
				}
				if (xmlObj.HasAttributes && xmlObj.Attributes["Cover"] != null)
				{
					cover = (Cover)int.Parse(xmlObj?.Attributes?["Cover"].InnerText);
				}

				type.Faceable = faceable;
				type.Cover = cover;
				type.Edge = edge;
				type.Surface = surface;


#if CLIENT
				string stringoffset = xmlObj.GetElementsByTagName("sprite")[0].Attributes["offset"].InnerText;
				float x = float.Parse(stringoffset.Substring(0, stringoffset.IndexOf(",")));
				float y = float.Parse(stringoffset.Substring(stringoffset.IndexOf(",")+1, stringoffset.Length - stringoffset.IndexOf(",")-1));
				Vector2 Offset = new Vector2(x, y);
				int drawlayer = int.Parse(xmlObj.GetElementsByTagName("sprite")[0].Attributes["layer"].InnerText);
				var spritename = xmlObj.GetElementsByTagName("sprite")[0]?.Attributes["name"]?.InnerText;
#endif
			
				
				
				

#if CLIENT

				type.Transform = new Transform2();
				type.Transform.Position = Utility.GridToWorldPos(Offset);
				type.DrawLayer = drawlayer;
		
				
				
#endif

			
				
				Prefabs.Add(name,type);
				
#if CLIENT
				if (spritename != null)
				{
					type.GenerateSpriteSheet(Game1.Textures[spritename]);
				}
				else
				{
					type.GenerateSpriteSheet(Game1.Textures[name]);
				}


#endif


			}
			
		}
	
	}
}