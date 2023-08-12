﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using DefconNull.World.WorldActions;
using DefconNull.World.WorldActions.DeliveryMethods;
#if CLIENT
using DefconNull.Rendering;
#endif
namespace DefconNull.World.WorldObjects;

public static class PrefabManager
{
	public static Dictionary<string, WorldObjectType> WorldObjectPrefabs = new Dictionary<string, WorldObjectType>();
	public static Dictionary<string, UnitType> UnitPrefabs = new Dictionary<string, UnitType>();
	public static Dictionary<string, UsableItem> UseItems = new Dictionary<string, UsableItem>();
	public static Dictionary<string, StatusEffectType> StatusEffects = new Dictionary<string, StatusEffectType>();


	public static void MakePrefabs()
	{
		XmlDocument xmlDoc= new XmlDocument();
		xmlDoc.Load(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)+"/ObjectData.xml"); 

		foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("statuseffect"))
		{

			string name = xmlObj.GetElementsByTagName("name")[0]?.InnerText ?? string.Empty;
			var effectelement = xmlObj.GetElementsByTagName("effect")[0];
			if (effectelement != null)
			{
				var itm = ParseConsiquences(effectelement);
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
			int maxHealth = 10;
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
			type.MaxHealth = maxHealth;
			
			if(xmlObj!.GetElementsByTagName("destroyEffect").Count > 0){
				type.DesturctionEffect = ParseConsiquences(xmlObj.GetElementsByTagName("destroyEffect")[0]!);	
			} 
			


#if CLIENT

			if (xmlObj.HasAttributes && xmlObj.Attributes?["z"] != null)
			{
				type.Zoffset = float.Parse(xmlObj?.Attributes?["z"]!.InnerText!);
			}


			var spritename = xmlObj.GetElementsByTagName("sprite")[0]?.Attributes["name"]?.InnerText;
			var xmlNodeList = xmlObj.GetElementsByTagName("sprite")[0]?.ChildNodes;

			List<Tuple<string, int>> spriteVariations = new List<Tuple<string, int>>();
			if (xmlNodeList != null)
			{
				foreach (var node in xmlNodeList)
				{
					XmlElement obj = (XmlElement) node;
					spriteVariations.Add(new Tuple<string, int>(obj.GetAttribute("id"), int.Parse(obj.GetAttribute("weight"))));
				}
			}
			if(spriteVariations.Count == 0){
				spriteVariations.Add(new Tuple<string, int>("", 1));
			}

			if (spritename == null)
			{
				spritename = name;
			}

#endif
			
				

			
				
			WorldObjectPrefabs.Add(name,type);
				
#if CLIENT

	


			type.GenerateSpriteSheet(spritename,spriteVariations);//this is a bit inconsistent but eeeh

#endif
		}

		foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("unit"))
		{
			if(xmlObj == null) continue;
			string name = xmlObj.GetElementsByTagName("name")[0]?.InnerText ?? throw new Exception("null name for a prefab");


			UnitType unitType  = new UnitType(name);
			unitType.Faceable = true;
			unitType.SolidCover = Cover.Full;
			unitType.VisibilityCover = Cover.None;

			unitType.MaxHealth = int.Parse(xmlObj?.Attributes?["Health"]!.InnerText ?? "10");
			unitType.MoveRange = int.Parse(xmlObj.Attributes?["moveRange"]?.InnerText ?? "4");
			unitType.Maxdetermination = int.Parse(xmlObj.Attributes?["determination"]?.InnerText ?? "2");
			unitType.InventorySize = int.Parse(xmlObj.Attributes?["inventory"]?.InnerText ?? "1");
			unitType.MaxMovePoints = int.Parse(xmlObj.Attributes?["moves"]?.InnerText ?? "2");
			unitType.MaxActionPoints = int.Parse(xmlObj.Attributes?["actions"]?.InnerText ?? "1");
			unitType.OverWatchSize = int.Parse(xmlObj.Attributes?["overwatch"]?.InnerText ?? "2");
			unitType.SightRange = int.Parse(xmlObj.Attributes?["sightrange"]?.InnerText ?? "16");
				
				
			XmlElement defaultact = (XmlElement)xmlObj.GetElementsByTagName("defaultAction")[0]! ?? throw new InvalidOperationException();
			unitType.DefaultAttack = ParseUnitAbility(defaultact);

			var speff = ((XmlElement) xmlObj).GetElementsByTagName("spawneffect")[0];
			if (speff != null)
			{
				unitType.SpawnEffect = ParseConsiquences((XmlElement) speff);
			}

			var actions = ((XmlElement) xmlObj).GetElementsByTagName("action");
			foreach (var act in actions)
			{ 
				unitType.Actions.Add(ParseUnitAbility((XmlElement)act));
			}
			var toggleActions = ((XmlElement) xmlObj).GetElementsByTagName("toggleaction");
			foreach (var act in toggleActions)
			{
				//	ExtraToggleAction toggle = new ExtraToggleAction();
				XmlElement actobj = (XmlElement) act;
				UnitAbility on = ParseUnitAbility((XmlElement)actobj.GetElementsByTagName("toggleon")[0]! ?? throw new InvalidOperationException());
				UnitAbility off = ParseUnitAbility((XmlElement)actobj.GetElementsByTagName("toggleoff")[0]! ?? throw new InvalidOperationException());
				ExtraToggleAction toggle = new ExtraToggleAction(on,off);
				unitType.Actions.Add(toggle);
			}


#if CLIENT
			unitType.GenerateSpriteSheet("Units/"+name+"/Stand");//this is a bit inconsistent but eeeh
			unitType.CrouchSpriteSheet = Utility.MakeSpriteSheet(TextureManager.GetTextureFromPNG("Units/"+name + "/Crouch"),3,3);
#endif
			WorldObjectPrefabs.Add(name,unitType);
			UnitPrefabs.Add(name,unitType);
		}

		foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("item"))
		{
			var effects = ParseWorldEffects(xmlObj);
			string avail = xmlObj.GetElementsByTagName("availability")[0]?.InnerText ?? "";
			string name = xmlObj.GetElementsByTagName("name")[0]?.InnerText ?? "";
			string tip = xmlObj.GetElementsByTagName("tip")[0]?.InnerText ?? "";
			List<string> availableToUnits = new List<string>();
			if (avail != "")
			{
				availableToUnits = avail.Split(',').ToList();
			}
		
			
			var itm = new UsableItem(name, tip, effects, availableToUnits.ToList());
			UseItems.Add(itm.Name,itm);
		}
	}

	private static UnitAbility ParseUnitAbility(XmlElement actobj)
	{
		string actname;
		string tooltip;
		int DetCost = int.Parse(actobj.Attributes?["detCost"]?.InnerText ?? "0");
		int MoveCost =     int.Parse(actobj.Attributes?["moveCost"]?.InnerText ?? "0");
		int ActCost =   int.Parse(actobj.Attributes?["actCost"]?.InnerText ?? "0"); 
		string name = actobj.GetElementsByTagName("name")[0]?.InnerText ?? "";
		string tip = actobj.GetElementsByTagName("tip")[0]?.InnerText ?? string.Empty;
		if(DetCost<0||MoveCost<0||ActCost<0){
			throw new Exception("negative cost for action");
		}
		List<IWorldEffect> effects = ParseWorldEffects(actobj);

		var immideaateActivation = bool.Parse(actobj.Attributes?["immideate"]?.InnerText ?? "false");
		UnitAbility a = new UnitAbility(name, tip, DetCost, MoveCost, ActCost, effects,immideaateActivation);
		return a;
	}

	private static Shootable ParseShoot(XmlElement xmlElement)
	{
		
		int dmg = int.Parse(xmlElement.Attributes?["dmg"]?.InnerText ?? "0");
		int detRes = int.Parse(xmlElement.Attributes?["detRes"]?.InnerText ?? "0");
		int supression = int.Parse(xmlElement.Attributes?["supression"]?.InnerText ?? "0");
		int supressionRange = int.Parse(xmlElement.Attributes?["supressionRange"]?.InnerText ?? "0");
		int dropoff = int.Parse(xmlElement.Attributes?["dropOffRange"]?.InnerText ?? "10");

		return new Shootable(dmg, detRes, supression, supressionRange, dropoff);
	}

	private static WorldEffect ParseWorldEffect(XmlElement xmlObj)
	{
		DeliveryMethod dvm = null;
		WorldConsiqences? eff = new WorldConsiqences();
/*
		string aid = xmlObj.GetElementsByTagName("targetAid")[0]?.InnerText ?? "none";
		WorldEffect.TargetAid tAid = WorldEffect.TargetAid.None;
		
		switch (aid)
		{
			case "none":
				tAid = WorldEffect.TargetAid.None;
				break;
			case "unit":
				tAid = WorldEffect.TargetAid.Unit;
				break;
			case "enemy":
				tAid = WorldEffect.TargetAid.Enemy;
				break;
		}*/
		//loop through all child nodes of the element
		if (xmlObj.GetElementsByTagName("delivery")[0] == null)
		{
			dvm = new ImmideateDelivery();
		}
		else
		{
			XmlNode node = (XmlNode) xmlObj.GetElementsByTagName("delivery")[0]!.ChildNodes[0]!;
			if (node.Name == "throwable")
			{	
				
				int throwRange = int.Parse(node.Attributes?["throwRange"]?.InnerText ?? "5");
				dvm = new Throwable(throwRange);
			}else if (node.Name == "vissionCast")
			{	
				
				int throwRange = int.Parse(node.Attributes?["range"]?.InnerText ?? "10");
				dvm = new VissionCast(throwRange);
			}
		

			//	if (dvm != null)
			//	{
			//		Vector2Int offset = Vector2Int.Parse(node.Attributes?["offset"]?.InnerText ?? "0,0");
			//		dvm.offset = offset;
			//		usgs.Add(dvm);
			//	}
			if(dvm==null)
				throw new Exception("no delivery method");
		}
		

		//make function
		var effectelement = xmlObj.GetElementsByTagName("consiquences")[0];
		if (effectelement != null)
		{
			eff = ParseConsiquences(effectelement);
		}
		
			
		WorldEffect itm = new WorldEffect(dvm,eff);

		return itm;
	}

	private static List<IWorldEffect> ParseWorldEffects(XmlElement xmlObj)
	{
		List<IWorldEffect> effects = new List<IWorldEffect>();
		foreach (XmlElement shoot in xmlObj.GetElementsByTagName("shoot"))
		{
			effects.Add(ParseShoot(shoot));
		}
		foreach (XmlElement effect in xmlObj.GetElementsByTagName("effect"))
		{
			effects.Add(ParseWorldEffect(effect));
		}
		return effects;
	}

	private static WorldConsiqences ParseConsiquences(XmlNode effect)
	{
		WorldConsiqences eff = new WorldConsiqences();
				
		eff.Range = int.Parse(effect.Attributes?["range"]?.InnerText ?? "1");
		eff.ExRange = int.Parse(effect.Attributes?["exRange"]?.InnerText ?? "0");
		eff.Visible = bool.Parse(effect.Attributes?["visible"]?.InnerText ?? "true");
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
			eff.Det = int.Parse(dmgitm.Attributes?["det"]?.InnerText ?? "0");
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
				eff.PlaceItemPrefab = innerText;
			}
		}
		XmlNode? giveItem = ((XmlElement) effect).GetElementsByTagName("giveItem")[0];
		if(giveItem !=null)
		{
			string? innerText = giveItem.Attributes?["item"]?.InnerText;
			if (innerText != null)
			{
				eff.GiveItem = new VariableValue(innerText);
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