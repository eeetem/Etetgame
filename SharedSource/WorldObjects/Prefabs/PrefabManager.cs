using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using DefconNull.WorldActions;
using DefconNull.WorldActions.DeliveryMethods;
using DefconNull.WorldActions.UnitAbility;
#if CLIENT
using DefconNull.Rendering;
#endif
namespace DefconNull.WorldObjects;

public static class PrefabManager
{
	public static Dictionary<string, WorldObjectType> WorldObjectPrefabs = new Dictionary<string, WorldObjectType>();
	public static Dictionary<string, UnitType> UnitPrefabs = new Dictionary<string, UnitType>();
	public static Dictionary<string, StatusEffectType> StatusEffects = new Dictionary<string, StatusEffectType>();


	public static void MakePrefabs()
	{
		XmlDocument xmlDoc= new XmlDocument();
		xmlDoc.Load(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)+"/ObjectData.xml"); 

		foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("statuseffect"))
		{

			string name = xmlObj.GetElementsByTagName("name")[0]?.InnerText ?? string.Empty;
			var effectelement = xmlObj.GetElementsByTagName("consequences")[0];
			if (effectelement != null)
			{
				var itm = ParseConsequences(effectelement);
				var st = new StatusEffectType(name,itm);
				StatusEffects.Add(name, st);
			}

		}

		
		
		foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("object"))
		{
			if(xmlObj == null) continue;
			string name = xmlObj.GetElementsByTagName("name")[0]?.InnerText ?? throw new Exception("null name for a prefab");
			
			WorldObjectType type = new WorldObjectType(name);

			bool faceable = true;
			bool edge = false;
			bool surface = false;
			bool impassible = false;
			Cover vcover = Cover.None;
			Cover scover = Cover.None;
			if (xmlObj.HasAttributes && xmlObj.Attributes["Faceable"] != null)
			{
				faceable = bool.Parse(xmlObj?.Attributes?["Faceable"]?.InnerText ?? "false");
			}
			if (xmlObj!.HasAttributes && xmlObj.Attributes?["Edge"] != null)
			{
				edge = bool.Parse(xmlObj?.Attributes?["Edge"]?.InnerText!);
			}
			if (xmlObj!.HasAttributes && xmlObj.Attributes!["Surface"] != null)
			{
				surface = bool.Parse(xmlObj?.Attributes?["Surface"]!.InnerText!);
				if (xmlObj!.HasAttributes && xmlObj.Attributes!["Impassible"] != null)
				{
					impassible = bool.Parse(xmlObj?.Attributes?["Impassible"]!.InnerText!);
				}
			}
			

			if (xmlObj!.HasAttributes && xmlObj.Attributes?["SCover"] != null)
			{
				scover = (Cover)int.Parse(xmlObj?.Attributes?["SCover"]!.InnerText!);
			}
			if (xmlObj!.HasAttributes && xmlObj.Attributes?["VCover"] != null)
			{
				string innerHtml = xmlObj?.Attributes?["VCover"]!.InnerText!;
				if (innerHtml.Contains("+"))
				{
					vcover = Cover.None;
					type.VisibilityObstructFactor = int.Parse(innerHtml.Replace("+",""));
				}
				else
				{
					vcover = (Cover)int.Parse(xmlObj?.Attributes?["VCover"]!.InnerText!);	
				}

					
			}
			else if (scover != Cover.None)
			{
				vcover = scover;
			}

			if (xmlObj!.HasAttributes && xmlObj.Attributes?["lifetime"] != null)
			{
				type.lifetime = int.Parse(xmlObj?.Attributes?["lifetime"]!.InnerText!);
			}

			type.Faceable = faceable;
			type.VisibilityCover = vcover;
			type.SolidCover = scover;
			type.Edge = edge;
			type.Surface = surface;
			type.Impassible = impassible;
			type.MaxHealth = int.Parse(xmlObj?.Attributes?["health"]?.InnerText ?? "8");
			
			if(xmlObj!.GetElementsByTagName("destroyConsequences").Count > 0){
				type.DestructionConseqences = ParseConsequences(xmlObj.GetElementsByTagName("destroyConsequences")[0]!);	
			} 
			


#if CLIENT

			if (xmlObj.HasAttributes && xmlObj.Attributes?["z"] != null)
			{
				type.Zoffset = float.Parse(xmlObj?.Attributes?["z"]!.InnerText!);
			}


			var defaultSpritename = xmlObj.GetElementsByTagName("sprite")[0]?.Attributes["source"]?.InnerText;
			var defaultFrames = int.Parse(xmlObj.GetElementsByTagName("sprite")[0]?.Attributes?["frames"]?.InnerText ?? "1");
			var defaultanimFps = int.Parse(xmlObj.GetElementsByTagName("sprite")[0]?.Attributes?["animFPS"]?.InnerText ?? "1");


			var xmlNodeList = xmlObj.GetElementsByTagName("sprite")[0]?.ChildNodes;

			List<SpriteVariation> spriteVariations = new List<SpriteVariation>();
			if (xmlNodeList != null)
			{
				foreach (var node in xmlNodeList)
				{
					
					XmlElement obj = (XmlElement) node;
					if (obj.Name == "variation")
					{
						var variation = new SpriteVariation(obj.GetAttribute("id"), int.Parse(obj.GetAttribute("weight")));
						spriteVariations.Add(variation);
					}
			
				}
			}


#endif
			
	
				
			WorldObjectPrefabs.Add(name,type);
				
#if CLIENT
			type.GenerateSpriteSheet(defaultSpritename,spriteVariations);//this is a bit inconsistent but eeeh
#endif
		}

		foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("unit"))
		{
			if(xmlObj == null) continue;
			string name = xmlObj.GetElementsByTagName("name")[0]?.InnerText ?? throw new Exception("null name for a prefab");
			
			List<UnitAbility> actionsList = new List<UnitAbility>();
			
			var actions = ((XmlElement) xmlObj).GetElementsByTagName("action");
			foreach (var act in actions)
			{ 
				actionsList.Add(ParseUnitAbility((XmlElement)act,(ushort)actionsList.Count));
			}
		
			
			UnitType unitType = new UnitType(name,  actionsList);
			
			unitType.Faceable = true;
			unitType.SolidCover = Cover.Full;
			unitType.VisibilityCover = Cover.None;

			unitType.MaxHealth = int.Parse(xmlObj?.Attributes?["health"]!.InnerText ?? "10");
			unitType.MoveRange = int.Parse(xmlObj.Attributes?["moveRange"]?.InnerText ?? "4");
			unitType.Maxdetermination = int.Parse(xmlObj.Attributes?["determination"]?.InnerText ?? "2");
			
			unitType.MaxMovePoints = int.Parse(xmlObj.Attributes?["moves"]?.InnerText ?? "2");
			unitType.MaxActionPoints = int.Parse(xmlObj.Attributes?["actions"]?.InnerText ?? "1");
			unitType.SightRange = int.Parse(xmlObj.Attributes?["sightrange"]?.InnerText ?? "16");
				
			

			var speff = ((XmlElement) xmlObj).GetElementsByTagName("spawneffect")[0];
			if (speff != null)
			{
				unitType.SpawnEffect = ParseConsequences((XmlElement) speff);
			}
			

#if CLIENT
			unitType.GenerateSpriteSheet("Units/"+name, new List<SpriteVariation>());//this is a bit inconsistent but eeeh
#endif
			WorldObjectPrefabs.Add(name,unitType);
			UnitPrefabs.Add(name,unitType);
		}
        
	}

	private static UnitAbility ParseUnitAbility(XmlElement actobj, ushort index)
	{
		ushort detCost = ushort.Parse(actobj.Attributes?["detCost"]?.InnerText ?? "0");
		ushort moveCost =     ushort.Parse(actobj.Attributes?["moveCost"]?.InnerText ?? "0");
		ushort actCost =   ushort.Parse(actobj.Attributes?["actCost"]?.InnerText ?? "0"); 
		ushort overWatchRange =   ushort.Parse(actobj.Attributes?["overwatch"]?.InnerText ?? "0"); 
		string name = actobj.GetElementsByTagName("name")[0]?.InnerText ?? "";
		string tip = actobj.GetElementsByTagName("tip")[0]?.InnerText ?? string.Empty;
		string aids = actobj.GetElementsByTagName("targetAid")[0]?.InnerText ?? "";
		var aidList =aids.Split(",").ToList();
		aidList.RemoveAll(x => x == "");
		
		bool aiExempt =  bool.Parse(actobj.Attributes?["aiExempt"]?.InnerText ?? "false"); 

		List<Effect> effects = ParseWorldEffects(actobj);
		var immideateActivation = false;
		//total fucking shitcode
		if (effects[0].GetType() == typeof(WorldEffect))
		{
			WorldEffect e = (WorldEffect) effects[0];
			if (e.DeliveryMethod.GetType() == typeof(ImmideateDelivery))
			{
				immideateActivation = true;
			}
		}

		var immideaateActivationOverride = bool.Parse(actobj.Attributes?["immideate"]?.InnerText ?? immideateActivation.ToString());
		UnitAbility a = new UnitAbility(name, tip, detCost, moveCost, actCost, overWatchRange,effects,immideaateActivationOverride,index,aiExempt,aidList);
		return a;
	}

	private static Shootable ParseShoot(XmlElement xmlElement)
	{
		
		int dmg = int.Parse(xmlElement.Attributes?["dmg"]?.InnerText ?? "0");
		int detRes = int.Parse(xmlElement.Attributes?["detRes"]?.InnerText ?? "0");
		int supression = int.Parse(xmlElement.Attributes?["supression"]?.InnerText ?? "0");
		int supressionRange = int.Parse(xmlElement.Attributes?["supressionRange"]?.InnerText ?? "0");
		int dropoff = int.Parse(xmlElement.Attributes?["dropOffRange"]?.InnerText ?? "10");



		var shoot = new Shootable(dmg, detRes, supression, supressionRange, dropoff);
		Vector2Int offset = Vector2Int.Parse(xmlElement.Attributes?["offset"]?.InnerText ?? "0,0");
		shoot.Offset = offset;
		return shoot;
	}

	private static WorldEffect ParseWorldEffect(XmlElement xmlObj)
	{
		DeliveryMethod? dvm = null;
		WorldConseqences eff = new WorldConseqences();

		//loop through all child nodes of the element
		if (xmlObj.GetElementsByTagName("delivery")[0] == null)
		{
			dvm = new ImmideateDelivery();
		}
		else
		{
			XmlNode node = xmlObj.GetElementsByTagName("delivery")[0]!.ChildNodes[0]!;
			if (node.Name == "throwable")
			{	
				
				int throwRange = int.Parse(node.Attributes?["throwRange"]?.InnerText ?? "5");
				dvm = new Throwable(throwRange);
			}else if (node.Name == "vissionCast")
			{	
				
				int throwRange = int.Parse(node.Attributes?["range"]?.InnerText ?? "10");
				dvm = new VissionCast(throwRange);
			}
		

			
      
        
			
			if(dvm==null)
				throw new Exception("no delivery method");
			

		}
		

		//make function
		var effectelement = xmlObj.GetElementsByTagName("consequences")[0];
		if (effectelement != null)
		{
			eff = ParseConsequences(effectelement);
		}
		
			
		WorldEffect itm = new WorldEffect(dvm,eff);
		
		Vector2Int offset = Vector2Int.Parse(xmlObj.Attributes?["offset"]?.InnerText ?? "0,0");
		itm.Offset = offset;

		return itm;
	}

	private static List<Effect> ParseWorldEffects(XmlElement xmlObj)
	{
		List<Effect> effects = new List<Effect>();
		foreach (var elem in xmlObj)
		{
			XmlElement xmlElement = (XmlElement) elem;
			if (xmlElement.Name == "shoot")
			{
				effects.Add(ParseShoot(xmlElement));
			}else if (xmlElement.Name == "effect")
			{
				effects.Add(ParseWorldEffect(xmlElement));
			}
			
		}

		return effects;
	}

	private static WorldConseqences ParseConsequences(XmlNode effect)
	{
		WorldConseqences eff = new WorldConseqences();
				
		eff.Range = int.Parse(effect.Attributes?["range"]?.InnerText ?? "0");
		eff.ExRange = int.Parse(effect.Attributes?["exRange"]?.InnerText ?? "0");

		eff.Los = bool.Parse(effect.Attributes?["los"]?.InnerText ?? "false");
		string ignores = effect.Attributes?["ignore"]?.InnerText ?? "";
		eff.Ignores = ignores.Split(',').ToList();
		string target = effect.Attributes?["target"]?.InnerText ?? "any";
		string[] targets = target.Split(',');
		foreach (var tar in targets)
		{
			switch (tar)
			{
				case "friend":
					eff.TargetFriend = true;
					break;
				case "foe":
					eff.TargetFoe = true;
					break;		
				case "self":
					eff.TargetSelf = true;
					break;	
				case "any":
					eff.TargetFriend = true;
					eff.TargetSelf = true;
					eff.TargetFoe = true;
					break;
				
			}
		}
		XmlNode? dmgitm = ((XmlElement) effect).GetElementsByTagName("damage")[0];
		if (dmgitm != null)
		{
			eff.Dmg =  int.Parse(dmgitm.Attributes?["dmg"]?.InnerText ?? "0");
			eff.DetRes =  int.Parse(dmgitm.Attributes?["detRes"]?.InnerText ?? "0");
			eff.EnvRes =  int.Parse(dmgitm.Attributes?["envRes"]?.InnerText ?? "0");
			eff.Det = int.Parse(dmgitm.Attributes?["det"]?.InnerText ?? "0");
		}
		XmlNode? fowSpot = ((XmlElement) effect).GetElementsByTagName("fowSpot")[0];
		if (fowSpot != null)
		{
			eff.FogOfWarSpot = true;
			eff.FogOfWarSpotScatter = int.Parse(fowSpot.Attributes?["scatter"]?.InnerText ?? "0");
		}
        
		XmlNode? valitm = ((XmlElement) effect).GetElementsByTagName("values")[0];
		if (valitm != null)
		{
			eff.Act =  new ValueChange(valitm.Attributes?["act"]?.InnerText ?? "0");
			eff.Move = new ValueChange(valitm.Attributes?["move"]?.InnerText ?? "0");
			eff.MoveRange = new ValueChange(valitm.Attributes?["moveRange"]?.InnerText ?? "0");
		}
		XmlNode? placeItem = ((XmlElement) effect).GetElementsByTagName("place")[0];
		if(placeItem !=null)
		{
			string? innerText = placeItem.Attributes?["name"]?.InnerText;
			if (innerText != null)
			{
				eff.PLaceItemConsequence = innerText;
			}
		}


		var statusapp = ((XmlElement) effect).GetElementsByTagName("applystatus");
		foreach (var status in statusapp)
		{
			var s = (XmlElement) status;
			string? statname = s.Attributes?["status"]?.InnerText;
			int duration = int.Parse(s.Attributes?["duration"]?.InnerText ?? "100000");
			if (statname != null)
			{
				eff.AddStatus.Add(new Tuple<string?, int>(statname,duration));
			}
		}
		var statusrem = ((XmlElement) effect).GetElementsByTagName("removestatus");
		foreach (var status in statusrem)
		{
			var s = (XmlElement) status;
			string? statname = s.Attributes?["status"]?.InnerText;
			if (statname != null)
			{
				eff.RemoveStatus.Add(statname);
			}
		}

		var extraction = ((XmlElement)effect).GetElementsByTagName("vfx");
		foreach (var elem in extraction)
		{
			var node = (XmlNode) elem;
					
			eff.Effects.Add(new Tuple<string, string, string>(node.Attributes?["name"]?.InnerText,node.Attributes?["target"]?.InnerText,node.Attributes?["speed"]?.InnerText));
		}

		var sfx = ((XmlElement)effect).GetElementsByTagName("sfx")[0]?.Attributes?["name"]?.InnerText;
		eff.Sfx = sfx;


		return eff;
	}

}