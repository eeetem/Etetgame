using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;

namespace MultiplayerXeno.Prefabs
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

				WorldObject.Cover eastCover = WorldObject.Cover.None;
				WorldObject.Cover westCover = WorldObject.Cover.None;
				WorldObject.Cover southCover = WorldObject.Cover.None;
				WorldObject.Cover northCover = WorldObject.Cover.None;
				WorldObject.Cover northeastCover = WorldObject.Cover.None;
				WorldObject.Cover northwestCover =WorldObject.Cover.None;
				WorldObject.Cover southeastCover = WorldObject.Cover.None;
				WorldObject.Cover southwestCover = WorldObject.Cover.None;
				
				
				if (cover != null && cover.Attributes != null)
				{
					
					 eastCover = (WorldObject.Cover) int.Parse(cover.Attributes["E"]?.InnerText ?? "0");
					 westCover = (WorldObject.Cover)int.Parse(cover.Attributes["W"]?.InnerText ??  "0");
					 southCover = (WorldObject.Cover)int.Parse(cover.Attributes["S"]?.InnerText ??  "0");
					 northCover = (WorldObject.Cover)int.Parse(cover.Attributes["N"]?.InnerText ??  "0");
					 northeastCover = (WorldObject.Cover)int.Parse(cover.Attributes["NE"]?.InnerText ?? "0");
					 northwestCover = (WorldObject.Cover)int.Parse(cover.Attributes["NW"]?.InnerText ??  "0");
					 southeastCover = (WorldObject.Cover)int.Parse(cover.Attributes["SE"]?.InnerText ??  "0");
					 southwestCover = (WorldObject.Cover)int.Parse(cover.Attributes["SW"]?.InnerText ??  "0");

					
					
				}

				
				
				WorldObjectType type = new WorldObjectType();


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

				Dictionary<WorldObject.Direction, WorldObject.Cover> covers = new Dictionary<WorldObject.Direction, WorldObject.Cover>();


				covers.Add(WorldObject.Direction.North, northCover);
				covers.Add(WorldObject.Direction.South, southCover);
				covers.Add(WorldObject.Direction.East, eastCover);
				covers.Add(WorldObject.Direction.West, westCover);
				covers.Add(WorldObject.Direction.SouthEast, southeastCover);
				covers.Add(WorldObject.Direction.SouthWest, southwestCover);
				covers.Add(WorldObject.Direction.NorthEast, northeastCover);
				covers.Add(WorldObject.Direction.NorthWest, northwestCover);
				
				
				
				
				
				
				
				
				type.Covers = covers;
				
				Prefabs.Add(name,type);
				
#if CLIENT
			
				type.GenerateSpriteSheet(Game1.Textures[name]);
#endif


			}
		}
	}
}