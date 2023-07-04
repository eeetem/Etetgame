using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MultiplayerXeno.Items;

namespace MultiplayerXeno;

public static class PrefabManager
{
	public static Dictionary<string, WorldObjectType> WorldObjectPrefabs = new Dictionary<string, WorldObjectType>();
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
				var itm = ParseEffect(effectelement);
				var st = new StatusEffectType(name,itm);
				StatusEffects.Add(name, st);
			}

		}

		foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("object"))
		{
			if(xmlObj == null) continue;
			string name = xmlObj.GetElementsByTagName("name")[0]?.InnerText ?? throw new Exception("null name for a prefab");
			

			UnitType? controllableType = null;
			XmlNode? contollableObj = xmlObj.GetElementsByTagName("controllable")[0];
			if (contollableObj != null)
			{
				
				controllableType = new UnitType(name);
				controllableType.MoveRange = int.Parse(contollableObj.Attributes?["moveRange"]?.InnerText ?? "4");
				controllableType.Maxdetermination = int.Parse(contollableObj.Attributes?["determination"]?.InnerText ?? "2");
				controllableType.InventorySize = int.Parse(contollableObj.Attributes?["inventory"]?.InnerText ?? "1");
				controllableType.MaxMovePoints = int.Parse(contollableObj.Attributes?["moves"]?.InnerText ?? "2");
				controllableType.MaxActionPoints = int.Parse(contollableObj.Attributes?["actions"]?.InnerText ?? "1");
				controllableType.OverWatchSize = int.Parse(contollableObj.Attributes?["overwatch"]?.InnerText ?? "2");
				controllableType.SightRange = int.Parse(contollableObj.Attributes?["sightrange"]?.InnerText ?? "16");
				
				
				XmlNode? defaultact = ((XmlElement) contollableObj).GetElementsByTagName("defaultAttack")[0];
				int determinationChange = int.Parse(defaultact?.Attributes?["det"]?.InnerText ?? "0");
				ValueChange movePointChange =     new ValueChange(defaultact?.Attributes?["mpoint"]?.InnerText ?? "-1");
				ValueChange actionPointChange =   new ValueChange(defaultact?.Attributes?["apoint"]?.InnerText ?? "-1"); 
				WorldAction action =	PraseWorldAction((XmlElement) defaultact! ?? throw new InvalidOperationException());

				controllableType.DefaultAttack =  new ExtraAction(action.Name,action.Description,determinationChange,movePointChange,actionPointChange,action,false);
				
				var speff = ((XmlElement) contollableObj).GetElementsByTagName("spawneffect")[0];
				if (speff != null)
				{
					controllableType.SpawnEffect = ParseEffect((XmlElement) speff);
				}



				var actions = ((XmlElement) contollableObj).GetElementsByTagName("action");
				foreach (var act in actions)
				{ 
					controllableType.Actions.Add(ParseControllableAction((XmlElement)act));
				}
				var toggleActions = ((XmlElement) contollableObj).GetElementsByTagName("toggleaction");
				foreach (var act in toggleActions)
				{
					//	ExtraToggleAction toggle = new ExtraToggleAction();
					XmlElement actobj = (XmlElement) act;
					ExtraAction on = ParseControllableAction((XmlElement)actobj.GetElementsByTagName("toggleon")[0]! ?? throw new InvalidOperationException());
					ExtraAction off = ParseControllableAction((XmlElement)actobj.GetElementsByTagName("toggleoff")[0]! ?? throw new InvalidOperationException());
					ExtraToggleAction toggle = new ExtraToggleAction(on,off);
					controllableType.Actions.Add(toggle);
				}

			}

			WorldObjectType type = new WorldObjectType(name,controllableType);
				


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
			if (xmlObj!.HasAttributes && xmlObj.Attributes?["Health"] != null)
			{
				maxHealth = int.Parse(xmlObj?.Attributes?["Health"]!.InnerText!);
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
				type.DesturctionEffect = ParseEffect(xmlObj.GetElementsByTagName("destroyEffect")[0]!);	
			} 
			


#if CLIENT
			Vector2 offset = new Vector2(-1.5f, -0.5f);
			//	if(faceable){
			//			Offset = 
			//	}
			var spritename = xmlObj.GetElementsByTagName("sprite")[0]?.Attributes["name"]?.InnerText;
			int spriteVariations = int.Parse(xmlObj.GetElementsByTagName("sprite")[0]?.Attributes["variations"]?.InnerText ?? "1");
			if (spritename == null)
			{
				spritename = name;
			}
			if (type.Unit != null)
			{
				spritename = "Units/" + spritename;
			}
#endif
			
				

#if CLIENT
			type.Transform = new Transform2();
			type.Transform.Position = Utility.GridToWorldPos(offset);
#endif

			
				
			WorldObjectPrefabs.Add(name,type);
				
#if CLIENT

	

			if (type.Unit != null)
			{
				type.GenerateSpriteSheet(spritename+"/Stand",spriteVariations,true);//this is a bit inconsistent but eeeh
				type.Unit.CrouchSpriteSheet = Utility.MakeSpriteSheet(TextureManager.GetTextureFromPNG(spritename + "/Crouch"),3,3);
			}
			else
			{
				type.GenerateSpriteSheet(spritename,spriteVariations);//this is a bit inconsistent but eeeh
			}
			

#endif
		}

		foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("item"))
		{
			var effect = PraseWorldAction(xmlObj);
			string innerText = xmlObj.GetElementsByTagName("availability")[0]?.InnerText ?? "";
			var itm = new UsableItem(effect, innerText != "" ? innerText.Split(",").ToList() : new List<string>());
			UseItems.Add(effect.Name,itm);
		}
	}

	private static ExtraAction ParseControllableAction(XmlElement actobj)
	{
		string actname;
		string tooltip;
		int DeterminationChange = int.Parse(actobj.Attributes?["det"]?.InnerText ?? "0");
		ValueChange MovePointChange =     new ValueChange(actobj.Attributes?["mpoint"]?.InnerText ?? "0");
		ValueChange ActionPointChange =   new ValueChange(actobj.Attributes?["apoint"]?.InnerText ?? "0"); 
		WorldAction action;
		action = PraseWorldAction(actobj);
		
		var immideaateActivation = bool.Parse(actobj.Attributes?["immideate"]?.InnerText ?? "false");
		ExtraAction a = new ExtraAction(action.Name, action.Description, DeterminationChange, MovePointChange, ActionPointChange, action,immideaateActivation);
		return a;
	}

	private static WorldAction PraseWorldAction(XmlElement xmlObj)
	{

		List<DeliveryMethod> usgs = new List<DeliveryMethod>();
		WorldEffect? eff = new WorldEffect();
		string name = xmlObj.GetElementsByTagName("name")[0]?.InnerText ?? "";
		string tip = xmlObj.GetElementsByTagName("tip")[0]?.InnerText ?? string.Empty;
		string aid = xmlObj.GetElementsByTagName("targetAid")[0]?.InnerText ?? "none";
		WorldAction.TargetAid tAid = WorldAction.TargetAid.None;
		
		switch (aid)
		{
			case "none":
				tAid = WorldAction.TargetAid.None;
				break;
			case "unit":
				tAid = WorldAction.TargetAid.Unit;
				break;
			case "enemy":
				tAid = WorldAction.TargetAid.Enemy;
				break;
		}
		//loop through all child nodes of the element
		foreach (var n in  xmlObj.ChildNodes)
		{
			XmlNode node = (XmlNode) n;
			DeliveryMethod dvm = null;
			if (node.Name == "throwable")
			{	
				
				int throwRange = int.Parse(node.Attributes?["throwRange"]?.InnerText ?? "5");
				dvm = new Throwable(throwRange);
			}else if (node.Name == "vissionCast")
			{	
				
				int throwRange = int.Parse(node.Attributes?["range"]?.InnerText ?? "10");
				dvm = new VissionCast(throwRange);
			}
			else if (node.Name == "shootable")
			{
				int dmg = int.Parse(node.Attributes?["dmg"]?.InnerText ?? "0");
				int detRes = int.Parse(node.Attributes?["detRes"]?.InnerText ?? "0");
				int supression = int.Parse(node.Attributes?["supression"]?.InnerText ?? "0");
				int supressionRange = int.Parse(node.Attributes?["supressionRange"]?.InnerText ?? "0");
				int dropoff = int.Parse(node.Attributes?["dropOffRange"]?.InnerText ?? "10");
				dvm = new Shootable(dmg,detRes,supression,supressionRange,dropoff);
			}
	
			if (dvm != null)
			{
				Vector2Int offset = Vector2Int.Parse(node.Attributes?["offset"]?.InnerText ?? "0,0");
				dvm.offset = offset;
				usgs.Add(dvm);
			}
		}
		
		if (usgs.Count == 0)
		{
			usgs.Add(new ImmideateDelivery());
		}
		//make function
		var effectelement = xmlObj.GetElementsByTagName("effect")[0];
		if (effectelement != null)
		{
			eff = ParseEffect(effectelement);
		}
		
			
		WorldAction itm = new WorldAction(name,tip,usgs,eff);
#if CLIENT
		itm.targetAid = tAid;
#endif
	
		return itm;
	}

	private static WorldEffect ParseEffect(XmlNode effect)
	{
		WorldEffect eff = new WorldEffect();
				
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
#if CLIENT
		var extraction = ((XmlElement)effect).GetElementsByTagName("vfx");
		foreach (var elem in extraction)
		{
			var node = (XmlNode) elem;
					
			eff.Effects.Add(new Tuple<string, string, string>(node.Attributes?["name"]?.InnerText,node.Attributes?["target"]?.InnerText,node.Attributes?["speed"]?.InnerText));
		}

		var sfx = ((XmlElement)effect).GetElementsByTagName("sfx")[0]?.Attributes?["name"]?.InnerText;
		eff.Sfx = sfx;
#endif

		return eff;
	}

}