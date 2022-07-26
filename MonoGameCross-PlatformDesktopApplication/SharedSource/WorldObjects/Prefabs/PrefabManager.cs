using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MultiplayerXeno.Structs;

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
				XmlNode cover = xmlObj.GetElementsByTagName("cover")[0];

				Cover eastCover = Cover.None;
				Cover westCover = Cover.None;
				Cover southCover = Cover.None;
				Cover northCover = Cover.None;
				Cover northeastCover = Cover.None;
				Cover northwestCover =Cover.None;
				Cover southeastCover = Cover.None;
				Cover southwestCover = Cover.None;
				
				
				if (cover != null && cover.Attributes != null)
				{
					
					 eastCover = (Cover) int.Parse(cover.Attributes["E"]?.InnerText ?? "0");
					 westCover = (Cover)int.Parse(cover.Attributes["W"]?.InnerText ??  "0");
					 southCover = (Cover)int.Parse(cover.Attributes["S"]?.InnerText ??  "0");
					 northCover = (Cover)int.Parse(cover.Attributes["N"]?.InnerText ??  "0");
					 northeastCover = (Cover)int.Parse(cover.Attributes["NE"]?.InnerText ?? "0");
					 northwestCover = (Cover)int.Parse(cover.Attributes["NW"]?.InnerText ??  "0");
					 southeastCover = (Cover)int.Parse(cover.Attributes["SE"]?.InnerText ??  "0");
					 southwestCover = (Cover)int.Parse(cover.Attributes["SW"]?.InnerText ??  "0");

					
					
				}

				ControllableType? controllableType = null;
				XmlNode contollableObj = xmlObj.GetElementsByTagName("controllable")[0];
				if (contollableObj != null)
				{
					controllableType = new ControllableType();
					controllableType.moveRange = int.Parse(contollableObj.Attributes["moveRange"].InnerText);
				}

				WorldObjectType type = new WorldObjectType(name,controllableType);


				bool faceable = true;
				if (xmlObj.HasAttributes && xmlObj.Attributes["Faceable"] != null)
				{
					 faceable = bool.Parse(xmlObj?.Attributes?["Faceable"].InnerText);
				}

				type.Faceable = faceable;


#if CLIENT
				string stringoffset = xmlObj.GetElementsByTagName("sprite")[0].Attributes["offset"].InnerText;
				float x = float.Parse(stringoffset.Substring(0, stringoffset.IndexOf(",")));
				float y = float.Parse(stringoffset.Substring(stringoffset.IndexOf(",")+1, stringoffset.Length - stringoffset.IndexOf(",")-1));
				Vector2 Offset = new Vector2(x, y);
				int drawlayer = int.Parse(xmlObj.GetElementsByTagName("sprite")[0].Attributes["layer"].InnerText);
#endif
			
				
				
				

#if CLIENT

				type.Offset = WorldObjectManager.GridToWorldPos(Offset+ new Vector2(-0.5f,-0.5f));
				type.DrawLayer = drawlayer;
				
				
#endif

				Dictionary<Direction, Cover> covers = new Dictionary<Direction, Cover>();


				covers.Add(Direction.North, northCover);
				covers.Add(Direction.South, southCover);
				covers.Add(Direction.East, eastCover);
				covers.Add(Direction.West, westCover);
				covers.Add(Direction.SouthEast, southeastCover);
				covers.Add(Direction.SouthWest, southwestCover);
				covers.Add(Direction.NorthEast, northeastCover);
				covers.Add(Direction.NorthWest, northwestCover);
				
				
				
				
				
				
				
				
				type.Covers = covers;
				
				Prefabs.Add(name,type);
				
#if CLIENT
			
				type.GenerateSpriteSheet(Game1.Textures[name]);
#endif


			}
			
		}
	
	}
}