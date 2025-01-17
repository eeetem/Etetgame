﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using DefconNull.ReplaySequence;
using DefconNull.WorldActions;
using DefconNull.WorldActions.DeliveryMethods;
using DefconNull.WorldActions.UnitAbility;
using Microsoft.VisualBasic.CompilerServices;
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
		xmlDoc.Load( Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)+"/ObjectData.xml"); 

		foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("statuseffect"))
		{

			string name = xmlObj.GetElementsByTagName("name")[0]?.InnerText ?? string.Empty;
			string tip = xmlObj.GetElementsByTagName("tip")[0]?.InnerText ?? string.Empty;
			var effectelement = xmlObj.GetElementsByTagName("consequences")[0];
			if (effectelement != null)
			{
				var itm = ParseConsequences(effectelement);
				var st = new StatusEffectType(name,tip,itm);
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
				type.lifetimeTick = bool.Parse(xmlObj?.Attributes?["lifetime"]!.InnerText!);
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
			var speff = ((XmlElement) xmlObj).GetElementsByTagName("spawnConsequences")[0];
			if (speff != null)
			{
				type.SpawnConseqences = ParseConsequences((XmlElement) speff);
			}


#if CLIENT

			if (xmlObj.HasAttributes && xmlObj.Attributes?["z"] != null)
			{
				type.Zoffset = float.Parse(xmlObj?.Attributes?["z"]!.InnerText!,CultureInfo.InvariantCulture);
			}


			var defaultSpritename = xmlObj.GetElementsByTagName("sprite")[0]?.Attributes["source"]?.InnerText;
			var anims = new Dictionary<string, int>();
			if (xmlObj.GetElementsByTagName("animations")[0] != null)
			{
				foreach (var child in  (xmlObj.GetElementsByTagName("animations")[0]).ChildNodes!)
				{
					var obj = (XmlElement) child;
					if(obj.Name != "anim") continue;
					anims.Add(obj.GetAttribute("name"),int.Parse(obj.GetAttribute("fps")));
				}
			}


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
			type.GenerateSpriteSheet(defaultSpritename,spriteVariations,anims);//this is a bit inconsistent but eeeh
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
				
			

			var speff = ((XmlElement) xmlObj).GetElementsByTagName("spawnConsequences")[0];
			if (speff != null)
			{
				unitType.SpawnConseqences = ParseConsequences((XmlElement) speff);
			}
			if(xmlObj!.GetElementsByTagName("destroyConsequences").Count > 0){
				unitType.DestructionConseqences = ParseConsequences(xmlObj.GetElementsByTagName("destroyConsequences")[0]!);	
			} 
			var anims = new Dictionary<string, int>();
			if (xmlObj.GetElementsByTagName("animations")[0] != null)
			{
				foreach (var child in  (xmlObj.GetElementsByTagName("animations")[0]).ChildNodes!)
				{
					var obj = (XmlElement) child;
					if(obj.Name != "anim") continue;
					anims.Add(obj.GetAttribute("name"),int.Parse(obj.GetAttribute("fps")));
				}
			}
		
#if CLIENT
			unitType.GenerateSpriteSheet("Units/"+name, new List<SpriteVariation>(),anims);//this is a bit inconsistent but eeeh
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
		ushort supression = ushort.Parse(xmlElement.Attributes?["supression"]?.InnerText ?? "0");
		int supressionRange = int.Parse(xmlElement.Attributes?["supressionRange"]?.InnerText ?? "0");
		int dropoff = int.Parse(xmlElement.Attributes?["dropOffRange"]?.InnerText ?? "10");
		int shotCount = int.Parse(xmlElement.Attributes?["shotCount"]?.InnerText ?? "3");
		int shotDelay = int.Parse(xmlElement.Attributes?["shotDelay"]?.InnerText ?? "350");
		float shotSpread = float.Parse(xmlElement.Attributes?["shotSpread"]?.InnerText ?? "0.5");
		string sound = xmlElement.Attributes?["shotSound"]?.InnerText ?? "";



		var shoot = new Shootable(dmg, detRes, supression, supressionRange, dropoff,shotCount,shotDelay,shotSpread,sound);
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
			if (node.Name == "vissionCast")
			{	
				
				int throwRange = int.Parse(node.Attributes?["range"]?.InnerText ?? "10");
				dvm = new VissionCast(throwRange);
			}else if (node.Name == "projectile")
			{
				int range = int.Parse(node.Attributes?["range"]?.InnerText ?? "10");
				int spot = int.Parse(node.Attributes?["fowSpot"]?.InnerText ?? "3");
				bool ignoreUnits = bool.Parse(node.Attributes?["ignoreUnits"]?.InnerText ?? "false");
				string particleName = node.Attributes?["particleName"]?.InnerText ?? "";
				float particleSpeed = float.Parse(node.Attributes?["particleSpeed"]?.InnerText ?? "1");
				List<SpawnParticle.RandomisedParticleParams> list = new List<SpawnParticle.RandomisedParticleParams>();
				ParseParticles(node, list);
				
				dvm = new Projectile(particleName, particleSpeed,range,spot,ignoreUnits,list);
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
		eff.LosCheckCover = (Cover)int.Parse(effect.Attributes?["los"]?.InnerText ?? "0");

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
				case "env":
					eff.TargetEnv = true;
					break;
				case "foe":
					eff.TargetEnemy = true;
					break;		
				case "self":
					eff.TargetSelf = true;
					break;	
				case "any":
					eff.TargetFriend = true;
					eff.TargetEnemy = true;
					eff.TargetSelf = true;
					eff.TargetEnv = true;
					break;
				
			}
		}
		XmlNode? dmgitm = ((XmlElement) effect).GetElementsByTagName("damage")[0];
		if (dmgitm != null)
		{
			eff.Dmg =  int.Parse(dmgitm.Attributes?["dmg"]?.InnerText ?? "0");
			eff.DetRes =  int.Parse(dmgitm.Attributes?["detRes"]?.InnerText ?? "0");
			eff.EnvRes =  int.Parse(dmgitm.Attributes?["envRes"]?.InnerText ?? "0");
			eff.Det = ushort.Parse(dmgitm.Attributes?["det"]?.InnerText ?? "0");

		}
		XmlNode? fowSpot = ((XmlElement) effect).GetElementsByTagName("fowSpot")[0];
		if (fowSpot != null)
		{
			eff.FogOfWarSpot = true;
			eff.FogOfWarSpotScatter = int.Parse(fowSpot.Attributes?["scatter"]?.InnerText ?? "0");
		}
		XmlNode? particles = ((XmlElement) effect).GetElementsByTagName("particles")[0];
		if (particles!= null)
		{
			ParseParticles(particles, eff.ParticleParamsList);
		}

		XmlNode? valitm = ((XmlElement) effect).GetElementsByTagName("values")[0];
		if (valitm != null)
		{
			eff.ChangeValues = true;
			eff.Act =  new ValueChange(valitm.Attributes?["act"]?.InnerText ?? "0");
			eff.Move = new ValueChange(valitm.Attributes?["move"]?.InnerText ?? "0");
			eff.MoveRange = new ValueChange(valitm.Attributes?["moveRange"]?.InnerText ?? "0");
			eff.Determination = new ValueChange(valitm.Attributes?["determination"]?.InnerText ?? "0");
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

	private static void ParseParticles(XmlNode particles, List<SpawnParticle.RandomisedParticleParams> list)
	{

			var particlesList = ((XmlElement) particles).GetElementsByTagName("particle");
			foreach (var p in particlesList)
			{
				var element = (XmlElement) p;
				var parm = new SpawnParticle.RandomisedParticleParams();
				parm.TextureName = element.Attributes?["name"]?.InnerText ?? "";
				parm.Count = int.Parse(element.Attributes?["count"]?.InnerText ?? "1");
				parm.SpawnList = new List<SpawnParticle.RandomisedParticleParams>();

				ParseParticles(element,parm.SpawnList);
				
	
				var velxRange = element.Attributes?["velocityXRange"]?.InnerText ?? "0,0";
				var velyRange = element.Attributes?["velocityYRange"]?.InnerText ?? "0,0";
				var accxRange = element.Attributes?["accelerationXRange"]?.InnerText ?? "0,0";
				var accyRange = element.Attributes?["accelerationYRange"]?.InnerText ?? "0,0";
				var lifeRange = element.Attributes?["lifetimeRange"]?.InnerText ?? "1,1";
				var rotationRange = element.Attributes?["rotationRange"]?.InnerText ?? "0,0";
				var scaleRange = element.Attributes?["scaleRange"]?.InnerText ?? "1,1";

				var splitX = velxRange.Split(',');
				parm.VelocityXMin = float.Parse(splitX[0],CultureInfo.InvariantCulture);
				parm.VelocityXMax = float.Parse(splitX[1],CultureInfo.InvariantCulture);

				var splitY = velyRange.Split(',');
				parm.VelocityYMin = float.Parse(splitY[0],CultureInfo.InvariantCulture);
				parm.VelocityYMax = float.Parse(splitY[1],CultureInfo.InvariantCulture);

				var splitAccX = accxRange.Split(',');
				parm.AccelerationXMin = float.Parse(splitAccX[0],CultureInfo.InvariantCulture);
				parm.AccelerationXMax = float.Parse(splitAccX[1],CultureInfo.InvariantCulture);

				var splitAccY = accyRange.Split(',');
				parm.AccelerationYMin = float.Parse(splitAccY[0],CultureInfo.InvariantCulture);
				parm.AccelerationYMax = float.Parse(splitAccY[1],CultureInfo.InvariantCulture);

				var splitLife = lifeRange.Split(',');
				parm.LifetimeMin = int.Parse(splitLife[0],CultureInfo.InvariantCulture);
				parm.LifetimeMax = int.Parse(splitLife[1],CultureInfo.InvariantCulture);
				
				var splitRot = rotationRange.Split(',');
				parm.RotationMin = float.Parse(splitRot[0],CultureInfo.InvariantCulture);
				parm.RotationMax = float.Parse(splitRot[1],CultureInfo.InvariantCulture);
				
				var splitScale = scaleRange.Split(',');
				parm.ScaleMin = float.Parse(splitScale[0],CultureInfo.InvariantCulture);
				parm.ScaleMax = float.Parse(splitScale[1],CultureInfo.InvariantCulture);
	
				parm.Damping = float.Parse(element.Attributes?["damping"]?.InnerText ?? "0.999",CultureInfo.InvariantCulture);
				parm.SpawnDelay = int.Parse(element.Attributes?["spawnDelay"]?.InnerText ?? "50",CultureInfo.InvariantCulture);
				
				
				list.Add(parm);
			}
			
		
	}
}