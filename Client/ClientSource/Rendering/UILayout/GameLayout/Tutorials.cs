using System.Threading;
using System.Threading.Tasks;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using DefconNull.WorldObjects.Units.Actions;

namespace DefconNull.Rendering.UILayout.GameLayout;

public partial class GameLayout
{
    public static void BasicTutorialSequence()
	{
		Task.Run(() =>
		{
			bigTutorialNote = false;
			tutorial = true;
			canEndTurnTutorial = false;
			tutorialActionLock = ActiveActionType.Move;
			highlightTile = new Vector2Int(19, 25);
			tutorialNote = "Welcome to [Green]Etetgame![-]\n" +
			               "Use WASD to move the camera and mouse wheel to zoom in and out!\n";
			Thread.Sleep(10000);
			tutorialNote = "Each unit has a set number of [Green]movement[-] points and [Orange]action[-] points.\n\nThese points can be spent to move your units and use their actions/abilities.\n\n" +
			               "At the end of each turn your units will recover all [Orange]action[-] and \n[Green]movement[-] points.\n\n" +
			               "Select the [Green]Scout[-] and double click the highlighted tile to move to it, using two of your [Green]movement points[-].";
			tutorialActionLock = ActiveActionType.Move;
			highlightTile = new Vector2Int(19, 31);
			var scout = GameManager.GetMyTeamUnits()[0];
			while (scout.WorldObject.TileLocation.Position != new Vector2Int(19, 31))
			{
				Thread.Sleep(350);
			}
			highlightTile = new Vector2Int(-1, -1);
			
			tutorialNote = "[Green]Turning[-]\n" +
			               "You can turn to face any direction after each move by double right clicking.\n" +
			               "Try it on the highlighted tile";
			tutorialActionLock = ActiveActionType.Face;
			highlightTile = new Vector2Int(23, 31);
			while (scout.WorldObject.Facing != Direction.East)
			{
				Thread.Sleep(350);
			}
			highlightTile = new Vector2Int(-1, -1);
			
			tutorialNote = "[Green]Clearing the Map[-]\n" +
			               "We've cleared this area, move to the highlighted tile to clear the cross angle before proceeding forward\n";
			tutorialActionLock = ActiveActionType.Move;
			highlightTile = new Vector2Int(20, 34);
			while (scout.WorldObject.TileLocation.Position != new Vector2Int(20, 34))
			{
				Thread.Sleep(350);
			}
			highlightTile = new Vector2Int(-1, -1);

			tutorialNote = "[Green]Clearing the Map[-]\n" +
			               "Double right click the highlighted tile to check the cross-fire";
			tutorialActionLock = ActiveActionType.Face;
			highlightTile = new Vector2Int(23, 30);
			while (scout.WorldObject.Facing != Direction.NorthEast)
			{
				Thread.Sleep(350);
			}
			highlightTile = new Vector2Int(-1, -1);

			tutorialNote = "[Green]Ending Turn[-]\n" +
			               "We have no more movement points remaining so lets end our turn.\n" +
			               "Click the end turn button located in the top right corner";
			TutorialEndTurn();
			
			int gruntId = 0;
			foreach (var u in GameManager.lastRecievedUnitPositionsP2)
			{
				if (u.Value.Item2.Prefab == "Grunt")
				{
					gruntId = u.Key;
					break;
				}
			}

			var mv = Action.Actions[Action.ActionType.Move];
			var fc = Action.Actions[Action.ActionType.Face];
			mv.SendToServer(gruntId, new Action.ActionExecutionParamters(new Vector2Int(22, 31)));
			Thread.Sleep(2000);
			fc.SendToServer(gruntId, new Action.ActionExecutionParamters(new Vector2Int(20,34)));
			MoveCamera.Make(new Vector2Int(22,31),true,0).GenerateTask().RunTaskSynchronously();
			Thread.Sleep(300);

			NetworkingManager.EndTurn();
			
			tutorialNote = "[Green]Cover[-]\n" +
			               "There's 3 types of cover. [Green]Low walls[-], [Yellow]Half walls[-] and [Red]Full walls[-].\n" +
			               "[Green]Low[-] - [Red]-2[-] damage, [Red]-4[-] if crouched\n" +
			               "[Yellow]High[-] - [Red]-4[-] damage, Cannot be hit if crouched\n" +
			               "[Red]Full[-] - Full walls, cannot be hit when crouching or standing.\n\n" +
			               "There's colored indicators on your cursor indicating the cover of nearby tiles.\n" +
			               "Press [Yellow]X[-] to select your shoot ability and try to shoot the [Red]enemy[-].";
			highlightTile = new Vector2Int(22, 31);
			while (activeAction != ActiveActionType.Action || !Equals(ActionTarget, WorldManager.Instance.GetTileAtGrid(new Vector2Int(22, 31)).UnitAtLocation!.WorldObject))
			{
				Thread.Sleep(350);
			}
			highlightTile = new Vector2Int(-1, -1);
			
			tutorialNote = "[Green]Cover[-]\n" +
			               "The [Red]grunt[-] is behind a[Yellow] Half[-] wall, the cover would absorb all your damage.\n\n" +
			               "[Yellow]Right click[-] to de-select the ability and move to highlighted tile to flank the [Red]enemy[-]";
			highlightTile = new Vector2Int(22, 33);
			tutorialActionLock = ActiveActionType.Move;
			while (scout.WorldObject.TileLocation.Position != new Vector2Int(22, 33))
			{
				Thread.Sleep(350);
			}

			tutorialNote = "[Green]Combat[-]\n" +
			               "Face the [Red]grunt[-] on the highlighted tile";
			highlightTile = new Vector2Int(22, 31);
			tutorialActionLock = ActiveActionType.Face;
			while (scout.WorldObject.Facing != Direction.North)
			{
				Thread.Sleep(350);
			}
			
			tutorialNote = "[Green]Combat[-]\n" +
			               "Finally shoot the [Red]enemy[-] By pressing [Yellow]X[-] and then [Yellow]Spacebar[-],\n\n" +
			               "Using 1 [Green]movement[-] and 1 [Orange]action[-] point. \n" +
			               "When aiming at an [Red]enemy[-], their health-bar will highlight the amount of damage they will receive.";

			TutorialFire(scout,new Vector2Int(22, 31));

			tutorialNote = "[Green]Great Work![-]\n" +
			               "Move to the highlighted tile";
			highlightTile = new Vector2Int(24, 32);
			tutorialActionLock = ActiveActionType.Move;
			while (scout.WorldObject.TileLocation.Position != new Vector2Int(24, 32))
			{
				Thread.Sleep(350);
			}

			tutorialNote = "We are out of movement and action points, lets end our turn for now";
			TutorialEndTurn();
			
			int heavyId = 1;
			foreach (var u in GameManager.lastRecievedUnitPositionsP2)
			{
				if (u.Value.Item2.Prefab == "Heavy")
				{
					heavyId = u.Key;
					break;
				}
			}
			
			mv.SendToServer(heavyId, new Action.ActionExecutionParamters(new Vector2Int(27, 26)));
			Thread.Sleep(2000);
			fc.SendToServer(heavyId, new Action.ActionExecutionParamters(new Vector2Int(26,31)));
			NetworkingManager.EndTurn();

			tutorialNote = "[Green]Crouching[-]\n" +
			               "Move to the highlighted tile";
			highlightTile = new Vector2Int(27,32);
			tutorialActionLock = ActiveActionType.Move;
			while (scout.WorldObject.TileLocation.Position != new Vector2Int(27, 32))
			{
				Thread.Sleep(350);
			}
			
			tutorialNote = "[Green]Crouching[-]\n" +
			               "Turn to the highlighted tile";
			highlightTile = new Vector2Int(27, 29);
			tutorialActionLock = ActiveActionType.Face;
			while (scout.WorldObject.Facing != Direction.North)
			{
				Thread.Sleep(350);
			}
			
			MoveCamera.Make(new Vector2Int(27,26),true,0).GenerateTask().RunTaskSynchronously();
			Thread.Sleep(300);

			tutorialNote = "[Green]Crouching[-]\n" +
			               "We have spotted another enemy!\n" +
			               "Crouching behind a half wall will conceal you from enemy view\n\n" +
			               "Crouch by pressing Z then spacebar";
			TutorialCrouch(scout);

			tutorialNote = "[Green]Crouching[-]\n" +
			               "Crouching consumes a movement point.\n" +
			               "We have one movement point remaining but lets end turn for now.";
			TutorialEndTurn();
			
			mv.SendToServer(heavyId, new Action.ActionExecutionParamters(new Vector2Int(27,28)));
			NetworkingManager.EndTurn();
			
			
			
			
			
			/*
			int heavyId = 1;
			foreach (var u in GameManager.lastRecievedUnitPositionsP2)
			{
				if (u.Value.Item2.Prefab == "Heavy")
				{
					heavyId = u.Key;
					break;
				}
			}

			var mv = Action.Actions[Action.ActionType.Move];
			mv.SendToServer(heavyId, new Action.ActionExecutionParamters(new Vector2Int(31, 36)));
			MoveCamera.Make(new Vector2Int(32,37),true,0).GenerateTask().RunTaskSynchronously();
			Thread.Sleep(300);
			var act = new Action.ActionExecutionParamters(new Vector2Int(29, 43));
			act.AbilityIndex = 0;

			MoveCamera.Make(new Vector2Int(29,43),true,0).GenerateTask().RunTaskSynchronously();
			NetworkingManager.EndTurn();

			 tutorialNote = "[Green]Crouching[-]\n" +
			               "The [Red]enemy Heavy[-] is awaiting your approach.\n" +
			               "Approach him carefully by moving to the highlighted tile.";
			TutorialMove(scout,new Vector2Int(26,42));

			tutorialNote = "[Green]Crouching[-]\n" +
			               "You can hide under [Yellow]half cover[-] to avoid being seen.\n" +
			               "Crouch by pressing [Yellow]Z[-] and then [Yellow]Spacebar[-], this will consume one movement point.";
			TutorialCrouch(scout);

			tutorialNote = "[Green]Hiding[-]\n" +
			               "Now that you're hidden, you can walk towards the [Red]Heavy[-].\n";
			TutorialMove(scout,new Vector2Int(29,42));

			tutorialNote = "[Green]Hiding[-]\n" +
			               "Uh Oh! You're out of [Green]movement points[-]!\n" +
			               "Lets end our turn and see what the [Red]Heavy[-] does.";
			tutorialActionLock = ActiveActionType.Action;
			TutorialEndTurn();

			mv.SendToServer(heavyId, new Action.ActionExecutionParamters(new Vector2Int(33, 39)));
			NetworkingManager.EndTurn();

			tutorialNote = "[Green]Hiding[-]\n" +
			               "Approach the corner to peak the [Red]Heavy[-]";
			tutorialAbilityIndex = -1;
			TutorialMove(scout, new Vector2Int(33, 42));

			tutorialNote = "[Green]Determination & Suppression[-]\n" +
			               "Face in the [Red]Heavy's[-] direction";

			highlightTile = new Vector2Int(33, 40);
			tutorialActionLock = ActiveActionType.Face;
			tutorialAbilityIndex = -1;

			tutorialNote = "The [Red]Heavy[-] has exposed himself.\n By hiding behind cover the [Red]Heavy[-] did not expect our unit to be so close \n\n" +
			               "Lets take advantage of this situation and attack our enemy while they are out in the open.";

			while (scout.WorldObject.Facing != Direction.North)
			{
				Thread.Sleep(350);
			}

			tutorialNote = "[Green]Determination & Suppression[-]\n" +
			               "Fire at the [Red]Heavy[-]!\n" +
			               "Shooting at or near an [Red]enemy[-] will suppress one of their determination points";
			TutorialFire(scout,new Vector2Int(33, 39));

			tutorialNote = "[Green]Panic[-]\n" +
			               "Once a units determination reaches 0 they will be [Red]Panicked[-].\n" +
			               "Panicked units will immediately crouch and regenerate one less movement points next turn.\n\n" +
			               "Panicked units will not regenerate a determination point next turn\n" +
			               "End your turn as you have no more movement or action points";
			TutorialEndTurn();

			mv.SendToServer(heavyId, new Action.ActionExecutionParamters(new Vector2Int(31, 36)));
			NetworkingManager.EndTurn();

			tutorialNote = "[Green]Victory![-]\n" +
			               "Because of the [Red]Heavy's[-] previous [Red]panicked[-] state,\n he is unable to retreat safely.\n\n" +
			               "Lets first retreat our [Green]Scout[-] to cover before we finish him off and claim [Green]Victory![-]";

			tutorialActionLock = ActiveActionType.Move;
			highlightTile = new Vector2Int(30, 42);
			TutorialMove(scout,new Vector2Int(30, 42));

			tutorialNote = "Stand up by using the crouch ability again, so we dont shoot the wall";
			TutorialCrouch(scout);

			tutorialNote = "Face the heavy";
			highlightTile = new Vector2Int(33, 39);

			tutorialNote = "shoot him";
			tutorialActionLock = ActiveActionType.Action;
			highlightTile = new Vector2Int(33, 39);
			TutorialFire(scout,new Vector2Int(33, 39));

			bigTutorialNote = true;
			tutorialNote = "[Green]End of basic tutorial![-]\n" +
			               "You now understand the basics of the game,\n we recommend you complete the [Orange]advanced tutorial[-] to learn more about the complex mechanics in this game.\n\n" +
			               "Press [Orange]ESC[-] to quit to main menu.";
			while (UI.currentUi is GameLayout)
			{
				Thread.Sleep(1000);
			}
			*/
			tutorial = false;
		}); 
	}
    
    public static void AdvancedTutorialSequence()
	{
		Task.Run(() =>
		{
			bigTutorialNote = false;
			tutorial = true;
			canEndTurnTutorial = false;
			tutorialActionLock = ActiveActionType.Move;
			highlightTile = new Vector2Int(1, 1);
			tutorialNote = "Welcome to Etetgame!\n" +
			               "Use WASD to move the camera and mouse wheel to zoom in and out!\n";
			Thread.Sleep(15000);
			tutorialNote = "[Green]The Scout[-]\n" +
			               "You currently control the [Green]Scout[-], one of the five unique unit classes in the game.\n\n" +
			               "The [Green]Scout[-] is [Green]quick[-], [Green]aggressive[-] and has [Green]long range of sight[-], however it's [Red]fragile[-] and has [Red]sharp damage dropoff[-] over range.\n\n" +
			               "Select the [Green]Scout[-] now and double click the highlighted tile to move to it, using one of your [Green]movement points[-].";
			tutorialActionLock = ActiveActionType.Move;
			highlightTile = new Vector2Int(19, 38);
			var scout = GameManager.GetMyTeamUnits()[0];
			while (scout.WorldObject.TileLocation.Position != new Vector2Int(19, 38))
			{
				Thread.Sleep(350);
			}
			highlightTile = new Vector2Int(-1, -1);
			tutorialNote = "[Green]Cover[-]\n" +
			               "There's 3 types of cover. [Green]Low[-], [Yellow]High[-] and [Red]Full[-].\n" +
			               "[Green]Low[-] - [Red]-2[-] damage, [Red]-4[-] if crouched\n" +
			               "[Yellow]High[-] - [Red]-4[-] damage, Cannot be hit if crouched\n" +
			               "[Red]Full[-] - Full walls, cannot bit hit crouching or standing.\n\n" +
			               "There's colored indicators on your cursor indicating cover of nearby tiles.\n" +
			               "Press [Yellow]X[-] to select your shoot ability and try to shoot the [Red]enemy[-].";
			while (activeAction != ActiveActionType.Action || !Equals(ActionTarget, WorldManager.Instance.GetTileAtGrid(new Vector2Int(23, 42)).UnitAtLocation!.WorldObject))
			{
				Thread.Sleep(350);
			}
			
			tutorialNote = "[Green]Cover[-]\n" +
			               "The [Red]enemy[-] unit is behind [Yellow]High[-] cover, the cover would absorb all your damage.\n\n" +
			               "[Yellow]Right click[-] to de-select the ability and move to highlighted tile to flank the [Red]enemy[-]";
			TutorialMove(scout,new Vector2Int(23,39));

			tutorialNote = "[Green]Cover[-]\n" +
			               "Finally shoot the [Red]enemy[-] By pressing [Yellow]X[-] and then [Yellow]Spacebar[-],\n\n" +
			               "Using 1 [Green]move[-] and 1 [Orange]action[-] point.";
			TutorialFire(scout,new Vector2Int(23, 42));
			
			tutorialNote = "[Green]Turning[-]\n" +
			               "Now that you've killed the [Red]enemy[-] double [Yellow]right click[-] the highlighted tile to face into its direction.\n\n" +
			               "You can turn units once per movement.";
			tutorialActionLock = ActiveActionType.Face;
			highlightTile = new Vector2Int(25, 39);

			while (scout.WorldObject.Facing != Direction.East)
			{
				Thread.Sleep(350);
			}

			tutorialNote = "[Green]End turn[-]\n" +
			               "You're out of [Green]movement[-] and [Orange]action[-] points." +
			               "You can now end your turn by pressing [Orange]end turn[-] button in top right corner";
			highlightTile = new Vector2Int(-1, -1);
			TutorialEndTurn();
			
			int heavyId = 0;
			foreach (var u in GameManager.lastRecievedUnitPositionsP2)
			{
				if (u.Value.Item2.Prefab == "Heavy")
				{
					heavyId = u.Key;
					break;
				}
			}
			
			var mv = Action.Actions[Action.ActionType.Move];
			var ow = Action.Actions[Action.ActionType.OverWatch];
			mv.SendToServer(heavyId, new Action.ActionExecutionParamters(new Vector2Int(32, 37)));
			MoveCamera.Make(new Vector2Int(32,37),true,0).GenerateTask().RunTaskSynchronously();
			Thread.Sleep(300);
			var act = new Action.ActionExecutionParamters(new Vector2Int(29, 43));
			act.AbilityIndex = 0;
			ow.SendToServer(heavyId,act);
			
			MoveCamera.Make(new Vector2Int(29,43),true,0).GenerateTask().RunTaskSynchronously();
			NetworkingManager.EndTurn();

			tutorialNote = "[Green]Overwatch and Hiding[-]\n" +
			               "The [Red]enemy Heavy[-] is awaiting your approach and has [Purple]overwatched[-] it. If you enter the area you will be automatically attacked.\n" +
			               "Approach it carefully by moving to a highlighted tile.";
			TutorialMove(scout,new Vector2Int(26,42));
			
			tutorialNote = "[Green]Overwatch and Hiding[-]\n" +
			               "You can hide under [Yellow]high cover[-] to avoid being seen and shot by [Purple]overwatch[-].\n" +
			               "Crouch by pressing [Yellow]Z[-] and then [Yellow]Spacebar[-].";
			TutorialCrouch(scout);

			tutorialNote = "[Green]Overwatch and Hiding[-]\n" +
			               "Now that you've hidden, you can walk towards the [Red]Heavy[-].\n";
			TutorialMove(scout,new Vector2Int(29,42));

			tutorialNote = "[Green]Scouts Ability[-]\n" +
			               "Uh Oh! You're out of [Green]movement points[-]!\n" +
			               "You can use the scout's special ability by pressing [Yellow]C[-] and [Yellow]Spacebar[-] to gain more [Green]movement points[-].\n" +
			               "The ability is very strong, however it will use up 3 [Blue]determination[-] leaving you open to be [Red]suppressed[-] if you get shot at.\n" +
			               "Use it wisely in your games!";
			tutorialActionLock = ActiveActionType.Action;
			tutorialAbilityIndex = 2;
			while (scout.MovePoints<=0)
			{
				Thread.Sleep(350);
			}
			tutorialNote = "[Green]Scouts Ability[-]\n" +
			               "Move to flank the [Red]Heavy[-]";
			tutorialAbilityIndex = -1;
			TutorialMove(scout, new Vector2Int(33, 42));

			tutorialNote = "[Green]Scouts Ability[-]\n" +
			               "Face the [Red]Heavy[-]";
			highlightTile = new Vector2Int(33, 40);
			tutorialActionLock = ActiveActionType.Face;
			tutorialAbilityIndex = -1;
			while (scout.WorldObject.Facing != Direction.North)
			{
				Thread.Sleep(350);
			}
			
			tutorialNote = "[Green]Scouts Ability[-]\n" +
			               "Fire at the [Red]Heavy[-]!";
			TutorialFire(scout,new Vector2Int(32, 37));
			
			tutorialNote = "[Green]Scout Tutorial[-]\n" +
			               "Shooting the [Red]Heavy[-] has interrupted the [Purple]overwatch[-], if you had other friendly units they would've been able to pass unobstructed.\n" +
			               "This is the end of [Green]Scout[-] tutorial, [Orange]end your turn[-] to move onto the Grunt tutorial.";
			TutorialEndTurn();
			
			tutorialNote = "[Green]The Grunt[-]\n" +
			               "The [Green]Grunt[-] has the [Green]longest weapon range[-], [Green]high damage[-] and has [Green]high health[-], however it [Red]lacks utility[-] and [Red]needs support[-] for scouting and suppression.\n\n" +
			               "move the [Green]Grunt[-] to highlighted tile.";
			do
			{
				Thread.Sleep(2000);
			}while (GameManager.GetMyTeamUnits().Count == 0 || SequenceManager.SequenceRunning);
			var grunt = GameManager.GetMyTeamUnits()[0];
			SelectUnit(null);
			TutorialMove(grunt, new Vector2Int(22, 44));
			tutorialNote = "[Green]Cover[-]\n" +
			               "You are standing in front of [Green]Light[-] cover. Crouch for better protection by pressing [Yellow]Z[-] and [Yellow]Spacebar[-].";
			TutorialCrouch(grunt);
			tutorialNote = "[Green]Cover[-]\n" +
			               "You're out of [Green]movement points[-]. [Orange]End your turn[-]";
			TutorialEndTurn();
			tutorialNote = "";

			int enemyScout1 = 0;
			int enemyScout2 = 0;
			foreach (var u in GameManager.lastRecievedUnitPositionsP2)
			{
				if (u.Value.Item2.Prefab == "Scout" && !u.Value.Item2.UnitData.Value.Team1)
				{
					if(enemyScout1 == 0)
					{
						enemyScout1 = u.Key;
					}
					else
					{
						enemyScout2 = u.Key;
						break;
					}
				}
			}
			var abl = Action.Actions[Action.ActionType.UseAbility];
			
			mv.SendToServer(enemyScout1, new Action.ActionExecutionParamters(new Vector2Int(29, 44)));
			MoveCamera.Make(new Vector2Int(29,44),true,0).GenerateTask().RunTaskSynchronously();
			
			var param = new Action.ActionExecutionParamters(WorldObjectManager.GetObject(enemyScout1));
			param.AbilityIndex = 1;
			abl.SendToServer(enemyScout1,param);
			do
			{
				Thread.Sleep(3000);
			}while (SequenceManager.SequenceRunning);

			param = new Action.ActionExecutionParamters(WorldObjectManager.GetObject(grunt.WorldObject.ID));
			param.AbilityIndex = 0;
			abl.SendToServer(enemyScout1,param);
			do
			{
				Thread.Sleep(3000);
			}while (SequenceManager.SequenceRunning);
			
			
			mv.SendToServer(enemyScout2, new Action.ActionExecutionParamters(new Vector2Int(29, 43)));
			MoveCamera.Make(new Vector2Int(29,43),true,0).GenerateTask().RunTaskSynchronously();
			
			param = new Action.ActionExecutionParamters(WorldObjectManager.GetObject(enemyScout2));
			param.AbilityIndex = 1;
			abl.SendToServer(enemyScout2,param);
			do
			{
				Thread.Sleep(3000);
			}while (SequenceManager.SequenceRunning);
			param =	new Action.ActionExecutionParamters(WorldObjectManager.GetObject(grunt.WorldObject.ID));
			param.AbilityIndex = 0;
			abl.SendToServer(enemyScout2,param);
			do
			{
				Thread.Sleep(3000);
			}while (SequenceManager.SequenceRunning);
			
			NetworkingManager.EndTurn();
			Thread.Sleep(1500);
			tutorialNote = "[Green]Suppression[-]\n" +
			               "The [Red]Enemy Scouts[-] are being reckless. They used their ability using up determination and grouped up.\n" +
			               "[Red]Fire[-] at the [Red]Scout[-] by pressing [Green]X[-], the area of effect [Blue]suppression[-] will suppress both of them";
			TutorialFire(grunt, new Vector2Int(29, 44));
			tutorialNote = "[Green]Suppression[-]\n" +
			               "The [Red]Scouts[-] have been shot while on 0 [Blue]determination[-] and are now [Red]Panicked[-], were [Red]forcefully crouched[-], next turn [Red]Will loose a move point[-], and [Red]Will not regenerate determination[-].\n\n" +
			               "If you look at your [Blue]determination[-] bar above your [Green]Grunt[-] you will see that you have 2 full [Blue]determination[-] and 1 [Orange]regenerating[-] meaning next turn you'll have 3 [Blue]determination[-], which is enough to use [Green]Grunt's[-] special ability.\n" +
			               "End your turn by pressing [Orange]end turn[-]";
			TutorialEndTurn();
			tutorialNote = "";
			mv.SendToServer(enemyScout1, new Action.ActionExecutionParamters(new Vector2Int(34, 44)));
			MoveCamera.Make(new Vector2Int(34,44),true,0).GenerateTask().RunTaskSynchronously();
			do
			{
				Thread.Sleep(1500);
			}while (SequenceManager.SequenceRunning);
			mv.SendToServer(enemyScout2, new Action.ActionExecutionParamters(new Vector2Int(34, 43)));
			MoveCamera.Make(new Vector2Int(34,43),true,0).GenerateTask().RunTaskSynchronously();
			NetworkingManager.EndTurn();
			
			tutorialNote = "[Green]The Grunt[-]\n" +
			               "The [Green]Grunt's[-] ability has [Green]extremely high damage[-] however units with [Blue]determination[-] can [Red]dodge[-] the damage.\n" +
			               "Every damage source will be [Red]dodged[-] by units that have [Blue]determination[-]. For most attacks the [Red]dodge[-] will only reduce damage by 1, but in case of [Green]Grunt's[-] ability it completely nullifies the damage.\n\n" +
			               "Use [Green]Grunt's[-] special ability to instantly kill the healthy [Red]Scout[-].";
			do
			{
				Thread.Sleep(1000);
			} while (SequenceManager.SequenceRunning);
			tutorialAbilityIndex = 2;
			tutorialActionLock = ActiveActionType.Action;
			highlightTile = new Vector2Int(34,43);
			var acts = grunt.ActionPoints.Current;
			while (acts == grunt.ActionPoints.Current)
			{
				Thread.Sleep(350);
			}
			
			tutorialNote = "[Green]The Grunt[-]\n" +
			               "Keep in mind if the enemy has a [Red]Grunt[-] nearby they can [Yellow]\"Counter-Snipe\"[-] your [Green]Grunt[-] as using the ability will leave grunt on 0 [Blue]determination[-].\n\n" +
			               "The [Red]Scout[-] has been [Green]eliminated[-].\n" +
			               "End your turn by pressing [Orange]end turn[-]";
			highlightTile = new Vector2Int(-1, -1);
			do
			{
				Thread.Sleep(1500);
			} while (SequenceManager.SequenceRunning);
			TutorialEndTurn();
			tutorialNote = "";
			MoveCamera.Make(new Vector2Int(29, 44),true,0).GenerateTask().RunTaskSynchronously();

			do
			{
				Thread.Sleep(1500);
			} while (SequenceManager.SequenceRunning || !GameManager.IsMyTurn());
			var heavy = GameManager.GetMyTeamUnits()[1];
			tutorialNote = "[Green]The Heavy[-]\n" +
			               "The [Red]enemy[-] is swarming you with units trying to overwhelm you.\n\n" +
			               "Thankfully you kept a [Green]Heavy[-] nearby, the heavy has [Green]very high health and determination[-] and is great at [Green]Suppressing units[-]." +
			               "Heavy can [Green]easily[-] hold off groups of enemies on his own but he's [Red]very slow[-] and can be avoided by more agile units aswell having [Red]mediocre damage output[-] so he needs assistance with finishing off the units he suppresses.\n" +
			               "Move the heavy into position";
			TutorialMove(heavy, new Vector2Int(22, 45));
			tutorialNote = "[Green]The Heavy's ability[-]\n" +
			               "The [Green]Heavy's[-] special ability [Blue]suppresses[-] units in a small area. It's excellent for punishing overly aggressive [Red]enemy[-] plays.\n" +
			               "[Blue]Suppress[-] the Scouts by pressing [Yellow]X[-] and [Yellow]Spacebar[-]\n";
			tutorialAbilityIndex = 2;
			tutorialActionLock = ActiveActionType.Action;
			tutorialUnitLock = heavy.WorldObject.ID;
			highlightTile = new Vector2Int(29, 44);
			acts = heavy.ActionPoints.Current;
			while (acts == heavy.ActionPoints.Current)
			{
				Thread.Sleep(350);
			}
			tutorialUnitLock = -1;
			tutorialActionLock = ActiveActionType.None;
			highlightTile = new Vector2Int(-1, -1);

			tutorialNote = "[Green]The Heavy[-]\n" +
			               "All the [Red]enemy scouts[-] are [Red]panicked[-]. They're are now easy pickings for your other units.\n\n" +
			               "[Orange]End your turn[-] to finish this part of the tutorial.";
			TutorialEndTurn();
			MoveCamera.Make(new Vector2Int(35,44),true,0).GenerateTask().RunTaskSynchronously();
			bigTutorialNote = true;
			tutorialNote = "[Green]End of tutorial![-]\n" +
			               "The game currently has 2 other units, the [Green]Officer[-] and the [Green]Specialist[-].\n\n" +
			               "[Green]Officer[-] provides [Green]extremely strong support abilities[-] but [Red]lacks any meaningful firepower[-] and [Red]Death of an officer suppresses nearby friendlies![-]\n\n" +
			               "[Green]Specialist[-] is a support unit for [Green]destroying cover[-] who can also provide [Green]some suppression[-] but [Red]isn't great for a frontal confrontation[-].\n\n" +
			               "You now understand the basics of the game, you can explore all abilities and strategies by practicing against AI or yourself in the singleplayer modes!\n\n" +
			               "Press [Orange]ESC[-] to quit to main menu.";
			while (UI.currentUi is GameLayout)
			{
				Thread.Sleep(1000);
			}
			
			tutorial = false;
			



		});
	}

	private static void TutorialEndTurn()
	{
		canEndTurnTutorial = true;
		while (GameManager.IsMyTurn())
		{
			Thread.Sleep(10);
		}
		canEndTurnTutorial = false;
	}

	private static void TutorialCrouch(Unit u)
	{
		tutorialAbilityIndex = 0;
		tutorialActionLock = ActiveActionType.Action;
		highlightTile = u.WorldObject.TileLocation.Position;
		bool courch = u.Crouching;
		while (u.Crouching == courch)
		{
			Thread.Sleep(350);	
		}
		tutorialActionLock = ActiveActionType.None;
		highlightTile = new Vector2Int(-1, -1);
	}
	private static void TutorialMove(Unit u, Vector2Int pos)
	{
		tutorialAbilityIndex = -1;
		tutorialUnitLock = u.WorldObject.ID;
		tutorialActionLock = ActiveActionType.Move;
		highlightTile = pos;
		while (u.WorldObject.TileLocation.Position != pos)
		{
			Thread.Sleep(350);
		}

		tutorialUnitLock = -1;
		tutorialActionLock = ActiveActionType.None;
		highlightTile = new Vector2Int(-1, -1);
	}
	private static void TutorialFire(Unit u, Vector2Int pos)
	{
		tutorialAbilityIndex = 1;
		tutorialActionLock = ActiveActionType.Action;
		highlightTile = pos;
		var acts = u.ActionPoints.Current;
		while (acts == u.ActionPoints.Current)
		{
			Thread.Sleep(350);
		}
		tutorialActionLock = ActiveActionType.None;
		highlightTile = new Vector2Int(-1, -1);
		tutorialAbilityIndex = -1;
	}
}