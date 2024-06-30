using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.AI;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using DefconNull.WorldActions;
using info.lundin.math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Action = DefconNull.WorldObjects.Units.Actions.Action;
using Move = DefconNull.AI.Move;
using Thickness = Myra.Graphics2D.Thickness;
using Unit = DefconNull.WorldObjects.Unit;
using WorldObject = DefconNull.WorldObjects.WorldObject;

namespace DefconNull.Rendering.UILayout.GameLayout;

public partial class GameLayout : MenuLayout
{

	public static List<Vector2Int>[] PreviewMoves = Array.Empty<List<Vector2Int>>();
	
	public static void SetScore(int score)
	{
		if (ScoreIndicator != null)
		{
			ScoreIndicator.Text = "" + score;
		}
	}
	public static void SetPercentComplete(float score)
	{
		if (CompleteIndicator != null)
		{
			if (score < 0)
			{
				CompleteIndicator.Visible = false;
			}
			else
			{	
				CompleteIndicator.Visible = true;
				CompleteIndicator.Text = "Opponents Turn Progress: " + (score*100).ToString("n2") + "%";
			}
			
		}
	}

	public static Unit? SelectedUnit { get; private set;} = null!;
	public static Unit? SelectedEnemyUnit { get; private set;} = null!;
	public static bool movePreviewDirty = false;
	public static void SelectUnit(Unit? unit)
	{
		if(!generated) return;
		if (unit == null)
		{
			unit = GameManager.GetTeamUnits(GameManager.IsPlayer1).FirstOrDefault();
		}
		if(unit is null)return;

		if (!unit.IsMyTeam())
		{
			SelectedEnemyUnit = unit;
			return;
		}
		SelectHudAction(null);

		SelectedUnit = unit;
		movePreviewDirty = true;
		UI.SetUI( new GameLayout());
		
		Camera.SetPos(unit.WorldObject.TileLocation.Position);
	}

	public static readonly int[,,] AIMoveCache = new int[100,100,2];
	public static void ReMakeMovePreview()
	{
		if(UI.currentUi is not GameLayout) return;
		
		if(SelectedUnit == null) return;
		var ret =  SelectedUnit.GetPossibleMoveLocations();
		
	
		Array.Resize(ref PreviewMoves, ret.Length);
		
		foreach (var p in PreviewMoves)
		{
			if(p != null)p.Clear();
		}

		for (int j = 0; j < ret.Length; j++)
		{
			if(PreviewMoves[j] == null)
			{
				PreviewMoves[j] = new List<Vector2Int>();
			}
			else
			{
				PreviewMoves[j].Clear();
			}
		}
		
		int i = 0;
		foreach (var g in ret)
		{
			foreach (var item in g)
			{
				PreviewMoves[i].Add(item.Item1);
			}

			i++;
		}
		UpdateHudButtons();
	}

	private static RenderTarget2D hoverHudRenderTarget;
	private static RenderTarget2D consequenceListRenderTarget;

	private static RenderTarget2D?timerRenderTarget;
	//private static RenderTarget2D? chatRenderTarget;
	//private static RenderTarget2D? chatScreenRenderTarget;
	private static Dictionary<int, RenderTarget2D> unitBarRenderTargets;
	private static Dictionary<int, RenderTarget2D>?targetBarRenderTargets;
		
	
	public static string tutorialNote = "";
	private static bool bigTutorialNote = false;
	private static Vector2Int highlightTile = new Vector2Int(-1,-1);
	public static bool tutorial = false;
	private static int tutorialUnitLock = -1;
	public static void TutorialSequence()
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
			               "[Red]Full[-] - Full walls, cannot be hit crouching or standing.\n\n" +
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
			MoveCamera.Make(new Vector2Int(32,37),true,0).RunSynchronously();;
			Thread.Sleep(300);
			var act = new Action.ActionExecutionParamters(new Vector2Int(29, 43));
			act.AbilityIndex = 0;
			ow.SendToServer(heavyId,act);
			
			MoveCamera.Make(new Vector2Int(29,43),true,0).RunSynchronously();;
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
			MoveCamera.Make(new Vector2Int(29,44),true,0).RunSynchronously();;
			
			var param = new Action.ActionExecutionParamters( WorldObjectManager.GetObject(enemyScout1)!);
			param.AbilityIndex = 1;
			abl.SendToServer(enemyScout1,param);
			do
			{
				Thread.Sleep(3000);
			}while (SequenceManager.SequenceRunning);
			param = new Action.ActionExecutionParamters(WorldObjectManager.GetObject(grunt.WorldObject.ID)!);
			param.AbilityIndex = 0;
			abl.SendToServer(enemyScout1,param);
			do
			{
				Thread.Sleep(3000);
			}while (SequenceManager.SequenceRunning);
			
			
			mv.SendToServer(enemyScout2, new Action.ActionExecutionParamters(new Vector2Int(29, 43)));
			MoveCamera.Make(new Vector2Int(29,43),true,0).RunSynchronously();;
			
			param = new Action.ActionExecutionParamters(WorldObjectManager.GetObject(enemyScout2)!);
			param.AbilityIndex = 1;
			abl.SendToServer(enemyScout2,param);
			do
			{
				Thread.Sleep(3000);
			}while (SequenceManager.SequenceRunning);
			param = new Action.ActionExecutionParamters(WorldObjectManager.GetObject(grunt.WorldObject.ID)!);
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
			MoveCamera.Make(new Vector2Int(34,44),true,0).RunSynchronously();;
			do
			{
				Thread.Sleep(1500);
			}while (SequenceManager.SequenceRunning);
			mv.SendToServer(enemyScout2, new Action.ActionExecutionParamters(new Vector2Int(34, 43)));
			MoveCamera.Make(new Vector2Int(34,43),true,0).RunSynchronously();;
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
			MoveCamera.Make(new Vector2Int(29, 44),true,0).RunSynchronously();;

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
			               "[Blue]Suppress[-] the Scouts by pressing [Yellow]C[-] and [Yellow]Spacebar[-]\n";
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
			MoveCamera.Make(new Vector2Int(35,44),true,0).RunSynchronously();;
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
	}
	public static void Init()
	{
		hoverHudRenderTarget = new RenderTarget2D(graphicsDevice,250,150);
		consequenceListRenderTarget = new RenderTarget2D(graphicsDevice,135,500);
	
		timerRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("GameHud/UnitBar/unitframe").Width,TextureManager.GetTexture("GameHud/UnitBar/unitframe").Height);
		unitBarRenderTargets = new Dictionary<int, RenderTarget2D>();
		targetBarRenderTargets = new Dictionary<int, RenderTarget2D>();
	}


	public static void MakeUnitBarRenders(SpriteBatch batch)
	{
		if(_unitBar == null ||  _unitBar.Widgets.Count == 0) return;
		if( GameManager.GetMyTeamUnits().Count == 0) return;
		int columCounter = 0;
		//sort by id
		GameManager.GetMyTeamUnits().Sort((a, b) => a.WorldObject.ID.CompareTo(b.WorldObject.ID));
		foreach (var unit in new List<Unit>(GameManager.GetMyTeamUnits()))
		{
			if (!unitBarRenderTargets.ContainsKey(unit.WorldObject.ID))
			{
				unitBarRenderTargets.Add(unit.WorldObject.ID, new RenderTarget2D(graphicsDevice, TextureManager.GetTexture("GameHud/UnitBar/Background").Width, TextureManager.GetTexture("GameHud/UnitBar/Background").Height));

			}

			graphicsDevice.SetRenderTarget(unitBarRenderTargets[unit.WorldObject.ID]);
			graphicsDevice.Clear(Color.Transparent);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
			batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/Background"), Vector2.Zero, Color.White);
			int i;
			if (unit.ActionPoints > 0 || unit.MovePoints > 0)
			{
				batch.End();
				PostProcessing.PostProcessing.ApplyUIEffect( new Vector2(TextureManager.GetTexture("GameHud/UnitBar/screen").Width, TextureManager.GetTexture("GameHud/UnitBar/screen").Height));
				batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.AnisotropicClamp, effect: PostProcessing.PostProcessing.UIGlowEffect);
				batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/screen"), Vector2.Zero, Color.White);
				batch.Draw(TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon"), Vector2.Zero, Color.White);
				batch.DrawText(columCounter + 1 + "", new Vector2(10, 5), Color.White);
				batch.End();

				PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(5,5));
				batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.AnisotropicClamp, effect: PostProcessing.PostProcessing.UIGlowEffect);

				int notchpos = 0;
				for (i = 0; i < unit.MovePoints; i++)
				{
					
					batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/moveNotch"), new Vector2(6 * notchpos, 0), Color.White);
					notchpos++;
				}

				for (i = 0; i < unit.ActionPoints; i++)
				{
					batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/fireNotch"), new Vector2(6 * notchpos, 0), Color.White);
					
					notchpos++;
				}
				batch.End();
			}
			else
			{
				batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/screen"), Vector2.Zero, Color.White);
				batch.Draw(TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon"), Vector2.Zero, Color.White);
				batch.DrawText(columCounter + 1 + "", new Vector2(10, 5), Color.White);
				batch.End();
			}




			var healthTexture = TextureManager.GetTexture("GameHud/UnitBar/red");
			var healthTextureoff = TextureManager.GetTexture("GameHud/UnitBar/redoff");
			float healthWidth = healthTexture.Width;
			float healthHeight = healthTexture.Height;
			int baseWidth = TextureManager.GetTexture("GameHud/UnitBar/Background").Width;
			float healthBarWidth = (healthWidth + 1) * unit.WorldObject.Type.MaxHealth;
			float emtpySpace = baseWidth - healthBarWidth;
			Vector2 healthBarPos = new Vector2(emtpySpace / 2f, 22);


			for (int y = 0; y < unit.WorldObject.Type.MaxHealth; y++)
			{
				var indicator = healthTexture;
				bool health = !(y >= unit.WorldObject.Health);
				if (health)
				{
					PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(healthWidth, healthHeight));
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
					indicator = healthTexture;
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					indicator = healthTextureoff;
				}

				batch.Draw(indicator, healthBarPos + new Vector2((healthWidth + 1) * y, 0), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
				batch.End();
			}

			healthTexture = TextureManager.GetTexture("GameHud/UnitBar/green");
			healthTextureoff = TextureManager.GetTexture("GameHud/UnitBar/greenoff");
			healthWidth = healthTexture.Width;
			healthHeight = healthTexture.Height;
			healthBarWidth = (healthWidth + 1) * unit.Type.Maxdetermination;
			emtpySpace = baseWidth - healthBarWidth;
			healthBarPos = new Vector2(emtpySpace / 2f, 25);


			for (int y = 0; y < unit.Type.Maxdetermination; y++)
			{
				var indicator = healthTexture;
				bool health = !(y >= unit.Determination);
				if (health)
				{
					PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(healthWidth, healthHeight));
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
					indicator = healthTexture;
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					indicator = healthTextureoff;
				}

				batch.Draw(indicator, healthBarPos + new Vector2((healthWidth + 1) * y, 0), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
				batch.End();
			}

			columCounter++;
		}

		Debug.Assert(unitBarRenderTargets != null, nameof(unitBarRenderTargets) + " != null");
		int realWidth = unitBarRenderTargets[GameManager.GetMyTeamUnits()[0].WorldObject.ID].Width;
		int realHeight = unitBarRenderTargets[GameManager.GetMyTeamUnits()[0].WorldObject.ID].Height;
		//float scale = (float) (_unitBar.MaxWidth! / _unitBar.Widgets.Count/realWidth);

		bool twoLayer = false;

		float scale = (float) (_unitBar.MaxHeight! / 2 /realHeight);
		if (realWidth * scale * _unitBar.Widgets.Count > _unitBar.MaxWidth!)//only go for two layer if just downscaling to two layer size is not enough
		{
			twoLayer = true;
		}
		else
		{
			scale = Math.Min((float) (_unitBar.MaxWidth! / _unitBar.Widgets.Count/realWidth),(float)(_unitBar.MaxHeight! /realHeight));
		}
		
		int w = (int) (realWidth * scale);
		if (w % realWidth > 0) { w+= realWidth - w % realWidth; }
		float actualScale =  (float) w/realWidth;
		actualScale *= 0.95f;
		int h = (int) (realHeight * actualScale);
		columCounter = 0;
		if (!twoLayer)
		{
			_unitBar.Top = (int) ( (_unitBar.MaxHeight! - h)/2f);
		}

		foreach (var unit in GameManager.GetMyTeamUnits())
		{
			
			if(_unitBar.Widgets.Count > columCounter ){
				ImageButton elem = (ImageButton)_unitBar.Widgets[columCounter];
				int colum = columCounter;
				if(twoLayer&& columCounter >= _unitBar.Widgets.Count/2){
					colum = columCounter - _unitBar.Widgets.Count/2;
					elem.GridRow = 1;
				}
				else
				{
					elem.GridRow = 0;
				}
				elem.GridColumn = colum;
				elem.Width = w;
				elem.ImageWidth = w;
				elem.Height = h;
				elem.ImageHeight = h;
				elem.Background = new SolidBrush(Color.Transparent);
				elem.FocusedBackground = new SolidBrush(Color.Transparent);
				elem.OverBackground = new SolidBrush(Color.Transparent);
				elem.PressedBackground = new SolidBrush(Color.Transparent);
				elem.PressedImage = new TextureRegion(unitBarRenderTargets[unit.WorldObject.ID]);
				elem.Image = new TextureRegion(unitBarRenderTargets[unit.WorldObject.ID]);
				if (unit.Equals(SelectedUnit)){
					elem.Top = 10;
				}
				else
				{
					elem.Top = 0;
				}
			}
			columCounter++;
		}

		if(targetBar == null ||  targetBar.Widgets.Count < suggestedTargets.Count) return;
		if( suggestedTargets.Count == 0) return;
		foreach (var wo in suggestedTargets)
		{
			var unit = wo.UnitComponent;
			if (!targetBarRenderTargets.ContainsKey(wo.ID))
			{
				targetBarRenderTargets.Add(wo.ID, new RenderTarget2D(graphicsDevice, TextureManager.GetTextureFromPNG("Units/" + wo.Type.Name+"/Icon").Width,TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon").Height));

			}
			graphicsDevice.SetRenderTarget(targetBarRenderTargets[wo.ID]);
			graphicsDevice.Clear(Color.Transparent);
			if (unit != null && unit.IsMyTeam())
			{
				PostProcessing.PostProcessing.SetOutlineEffectColor(Color.Green);	
			}
			else
			{
				PostProcessing.PostProcessing.SetOutlineEffectColor(Color.Red);
			}

			batch.Begin(samplerState: SamplerState.PointClamp, effect: PostProcessing.PostProcessing.OutLineEffect);
			batch.Draw(TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon"), Vector2.Zero, Color.White);
			batch.End();
		}
		
		
		columCounter = 0;
		foreach (var unit in suggestedTargets)
		{
			ImageButton elem = (ImageButton)targetBar.Widgets[columCounter];
			elem.GridRow = 0;
			elem.GridColumn = columCounter;
			elem.Width = w;
			elem.ImageWidth = w;
			elem.Height = h;
			elem.ImageHeight = h;
			elem.Background = new SolidBrush(Color.Transparent);
			elem.FocusedBackground = new SolidBrush(Color.Transparent);
			elem.OverBackground = new SolidBrush(Color.Transparent);
			elem.PressedBackground = new SolidBrush(Color.Transparent);
			elem.PressedImage = new TextureRegion(targetBarRenderTargets[unit.ID]);
			elem.Image = new TextureRegion(targetBarRenderTargets[unit.ID]);
			if (unit.Equals(ActionTarget)){
				elem.Background =  new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/reticle"));
			}
			else
			{
				elem.Top = 0;
			}
			
			columCounter++;
			
			
		}
		

	}

	private static Grid? _unitBar;
	private static Grid? targetBar;
	private static HorizontalStackPanel? targetBarStack;
	private static ImageButton? ConfirmButton;
	private static ImageButton? OverWatchToggle;
	public static Label? ScoreIndicator;
	public static Label? CompleteIndicator;
	private static ImageButton? endBtn;

	private static bool generated = false;
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		if (!tutorial)
		{
			tutorialNote = "";
			highlightTile = new Vector2Int(-1, -1);
			tutorialActionLock = ActiveActionType.None;
			tutorialUnitLock = -1;
			tutorialAbilityIndex = -1;
			canEndTurnTutorial = true;
		}
		WorldManager.Instance.MakeFovDirty();
		var panel = new Panel ();

		if (GameManager.spectating && !tutorial)
		{
			var spec = new Label()
			{
				Top = 50,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left
			};
			spec.Text = "Spectating";


			panel.Widgets.Add(spec);
			var swapTeam = new TextButton
			{
				Top = (int) (100f * GlobalScale.Y),
				Left = (int) (-10f * GlobalScale.X),
				Width = (int) (80 * GlobalScale.X),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Text = "Change POV",
				//Scale = globalScale
			};
			swapTeam.Click += (o, a) =>
			{
				
				GameManager.SwapSpecPov();
				WorldManager.Instance.MakeFovDirty();
				UI.SetUI(null);
			};
			panel.Widgets.Add(swapTeam);
		}
#if DEBUG
		/*var doAI = new TextButton
		{
			Top = (int) (200f * GlobalScale.Y),
			Left = (int) (-10f * GlobalScale.X),
			Width = (int) (80 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
			Text = "FinishTurnWithAI",
			//Scale = globalScale
		};
		
		doAI.Click += (o, a) =>
		{
			NetworkingManager.SendAITurn();
		};
		panel.Widgets.Add(doAI);
		*/
		var ruler = new TextButton
		{
			Top = (int)(160f * GlobalScale.Y),
			Left = (int)(-10f * GlobalScale.X),
			Width = (int)(60 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
			Text = "Ruler",
		};
		ruler.Click += (o, a) =>
		{
			//Log.Message("Test","1. ruler button clicked");
			if (CheckCurrentTool() == null)
			{
				//Log.Message("Test","2. selected ruler tool");
				SelectGameTool(new RulerTool());
			}
		};
		panel.Widgets.Add(ruler);
		
		var FOVtool = new TextButton
		{
			Top = (int)(195f * GlobalScale.Y),
			Left = (int)(-10f * GlobalScale.X),
			Width = (int)(80 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
			Text = "FOVTool",
		};
		FOVtool.Click += (o, a) =>
		{
			if (CheckCurrentTool() == null)
			{
				SelectGameTool(new FOVTool());
			}
		};
		panel.Widgets.Add(FOVtool);
#endif		

		endBtn = new ImageButton()
		{
			Top = (int) (26f * GlobalScale.X),
			Left = (int) (0f * GlobalScale.X),
			Width = (int) (TextureManager.GetTexture("GameHud/UnitBar/end button").Width * GlobalScale.X * 0.9f),
			Height = (int) (TextureManager.GetTexture("GameHud/UnitBar/end button").Height * GlobalScale.X * 0.9f),
			ImageWidth = (int) (TextureManager.GetTexture("GameHud/UnitBar/end button").Width * GlobalScale.X * 0.9f),
			ImageHeight = (int) (TextureManager.GetTexture("GameHud/UnitBar/end button").Height * GlobalScale.X * 0.9f),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
			Background = new SolidBrush(Color.Transparent),
			OverBackground = new SolidBrush(Color.Transparent),
			PressedBackground = new SolidBrush(Color.Transparent),
			Image = new TextureRegion(TextureManager.GetTexture("GameHud/UnitBar/end button")),
		};
		endBtn.Click += (o, a) =>
		{
			if(canEndTurnTutorial)
				GameManager.TryEndTurn();
		};


		panel.Widgets.Add(endBtn);
		

		if (inputBox == null)
		{
			inputBox = new TextBox();
			inputBox.Visible = false;
		}

	
		
		inputBox.HorizontalAlignment = HorizontalAlignment.Left;
		inputBox.Background = new SolidBrush(Color.Transparent);
		inputBox.Border = new SolidBrush(Color.Black);
		inputBox.BorderThickness = new Thickness(3);
		inputBox.Background = new SolidBrush(Color.Black*0.5f);
		inputBox.Left = (int) (0f * GlobalScale.X);
		inputBox.Width = (int) (200 * GlobalScale.Y);
		inputBox.Top = (int) (80 * GlobalScale.Y);
		inputBox.Font = DefaultFont.GetFont(FontSize / 3f);
		inputBox.TextColor = Color.White;
		inputBox.VerticalAlignment = VerticalAlignment.Center;
		inputBox.KeyDown += (sender, args) =>
		{
			if (args.Data == Keys.Enter)
			{
				if (inputBox.Text!= null && inputBox.Text.Length > 0)
				{
					Chat.SendMessage(inputBox.Text);
					inputBox.Text = "";
					inputBox.Visible = false;
					UI.Desktop.FocusedKeyboardWidget = null;
				}
			}

		};
		panel.Widgets.Add(inputBox);
		

		
		if (ScoreIndicator == null)
		{
			ScoreIndicator = new Label()
			{
				Top = (int) (28.5f * GlobalScale.X),
				Left = (int) (735f * GlobalScale.X),
			};
			SetScore(0);
		}
		

		panel.Widgets.Add(ScoreIndicator);
		
		if (CompleteIndicator == null)
		{
			CompleteIndicator = new Label()
			{
				Top=150,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			SetPercentComplete(-1);
		}
		panel.Widgets.Add(CompleteIndicator);


		_unitBar = new Grid()
		{
			GridColumnSpan = 4,
			GridRowSpan = 1,
			RowSpacing = 2,
			ColumnSpacing = 2,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Top,
			MaxWidth = (int)(365f*GlobalScale.X),
			//Width = (int)(365f*globalScale.X),
			MaxHeight = (int)(26f*GlobalScale.X),
			//Height = (int)(38f*globalScale.X),
			Top = (int)(0f*GlobalScale.Y),
			Left = (int)(-5f*GlobalScale.X),
			//ShowGridLines = true,
		};
		panel.Widgets.Add(_unitBar);
		targetBarStack = new HorizontalStackPanel();
		targetBarStack.HorizontalAlignment = HorizontalAlignment.Center;
		targetBarStack.VerticalAlignment = VerticalAlignment.Bottom;
		targetBarStack.Top = (int) (-230 * GlobalScale.Y);
		targetBarStack.MaxWidth = (int) (365f * GlobalScale.X);
		//	targetBarStack.Width = (int) (365f * globalScale.X);
	
		
		targetBar = new Grid()
		{
			GridColumnSpan = 4,
			GridRowSpan = 1,
			RowSpacing = 2,
			ColumnSpacing = 2,
			Padding = new Thickness(0),
			Margin = new Thickness(0),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			//MaxWidth = (int)(365f*globalScale.X),
			//Width = (int)(50f*globalScale.X),
			//MaxHeight = (int)(26f*globalScale.X),
			//Height = (int)(38f*globalScale.X),
			Top = (int)(25f*GlobalScale.Y),
			Left = (int)(-5f*GlobalScale.X),
			ShowGridLines = false,
		};
	
		var left = new Image();
		left.Height = (int) (34f * GlobalScale.X);
		left.Width = (int) (16f * GlobalScale.X);
		left.HorizontalAlignment = HorizontalAlignment.Right;
		left.Renderable = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/leftq"));
		//left.
		var right = new Image();
		right.Height = (int) (34f * GlobalScale.X);
		right.Width = (int) (16f * GlobalScale.X);
		right.HorizontalAlignment = HorizontalAlignment.Left;
		right.Renderable = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/righte"));
		
		targetBarStack.Widgets.Add(left);
		targetBarStack.Widgets.Add(targetBar);
		targetBarStack.Widgets.Add(right);
		
		panel.Widgets.Add(targetBarStack);
		
		targetBarStack.Visible = false;
		

		ConfirmButton = new ImageButton();
		ConfirmButton.HorizontalAlignment = HorizontalAlignment.Center;
		ConfirmButton.VerticalAlignment = VerticalAlignment.Bottom;
		ConfirmButton.Top = (int) (-90 * GlobalScale.X);
		ConfirmButton.Image = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/confirmation4"));
		ConfirmButton.ImageWidth = (int) (80 * GlobalScale.X);
		ConfirmButton.Width = (int) (80 * GlobalScale.X);
		ConfirmButton.ImageHeight = (int) (20 * GlobalScale.X);
		ConfirmButton.Height = (int) (20 * GlobalScale.X);
		ConfirmButton.Click += (sender, args) =>
		{
			DoActiveAction();
		};
		ConfirmButton.Visible = activeAction == ActiveActionType.Action || activeAction == ActiveActionType.Overwatch;

		panel.Widgets.Add(ConfirmButton);
		
		OverWatchToggle = new ImageButton();
		OverWatchToggle.HorizontalAlignment = HorizontalAlignment.Center;
		OverWatchToggle.VerticalAlignment = VerticalAlignment.Bottom;
		OverWatchToggle.Top = (int) (-90 * GlobalScale.X);
		OverWatchToggle.Left = (int) (50 * GlobalScale.X);
	

		OverWatchToggle.ImageWidth = (int) (25 * GlobalScale.X);
		OverWatchToggle.Width = (int) (25 * GlobalScale.X);
		OverWatchToggle.ImageHeight = (int) (25 * GlobalScale.X);
		OverWatchToggle.Height = (int) (25 * GlobalScale.X);
		OverWatchToggle.Click += (sender, args) =>
		{
			ToggleOverWatch();
		};
		OverWatchToggle.Visible = false;
		

		panel.Widgets.Add(OverWatchToggle);
		
		/*foreach (var unit in new List<Unit>(GameManager.GetMyTeamUnits()))
		{
			var unitPanel = new ImageButton();

			Unit u = unit;
			unitPanel.Click += (sender, args) =>
			{
				SelectUnit(u);
			};
			_unitBar.Widgets.Add(unitPanel);
		}
		*/

		generated = true;
		if (SelectedUnit == null) return panel;


		ActionButtons.Clear();
		var crouchbtn = new ImageButton();
		HudActionButton crouchHudBtn = new HudActionButton(crouchbtn, TextureManager.GetTexture("GameHud/BottomBar/crouch"),SelectedUnit,delegate(Unit unit, WorldObject target)
			{
				unit.DoAction(Action.ActionType.Crouch,new Action.ActionExecutionParamters());
			}, 
			delegate(Unit unit, WorldObject vector2Int)
			{
				if(unit.MovePoints<0) return new Tuple<bool, string>(false,"Not Enough Move Points");
				return new Tuple<bool, string>(true,"");
			}
			
			,new AbilityCost(0,0,1),"Crouching increases the benefit of cover and allows hiding behind tall cover.",true);

		panel.Widgets.Add(crouchbtn);

		ActionButtons.Add(crouchHudBtn);

		
		int i = 0;
		foreach (var action in SelectedUnit.Abilities)
		{
			var btn = new ImageButton();
			var hudBtn = new HudActionButton(btn,action,SelectedUnit);
			ActionButtons.Add(hudBtn);
			
			panel.Widgets.Add(btn);
			i++;
		}


		int top = (int) (-9*GlobalScale.X) ;
		float scale = GlobalScale.X * 0.9f;
		int totalBtns = ActionButtons.Count;
		int btnWidth = (int) (34 * scale);
		int totalWidth = totalBtns * btnWidth;
		int startOffest = Game1.resolution.X / 2 - totalWidth / 2;

		i = 0;
		int index = 0;
		foreach (var actionBtn in new List<HudActionButton>(ActionButtons))
		{
			actionBtn.UpdateIcon();
			var UIButton = actionBtn.UIButton;
			UIButton.HorizontalAlignment = HorizontalAlignment.Left;
			UIButton.VerticalAlignment = VerticalAlignment.Bottom;
			UIButton.Width = (int) (32 * scale);
			UIButton.Height = (int) (32 * scale);
			UIButton.ImageHeight = (int) (32 * scale);
			UIButton.ImageWidth = (int) (32 * scale);
			UIButton.Top = top;
			UIButton.Left = startOffest + btnWidth*index;
			index++;
		}


	
		generated = true;
		return panel;
	}

	private static GameTool currentTool = null;
	public static void SelectGameTool(GameTool input)
	{
		currentTool = input;
	}
	public static GameTool CheckCurrentTool()
	{
		return currentTool;
	}
	
	
	
	private static readonly List<HudActionButton> ActionButtons = new();
	private static bool ActionForce = false;
	
	private static ActiveActionType tutorialActionLock = ActiveActionType.None;
	private static int tutorialAbilityIndex = -1;
	private static bool canEndTurnTutorial = true;
	private static void DoActiveAction()
	{
		if(ActionTarget == null) return;
		if (activeAction != ActiveActionType.Action && activeAction != ActiveActionType.Overwatch) return;
		
		if(highlightTile != new Vector2Int(-1,-1) && ActionTarget.TileLocation.Position != highlightTile) return;
		if(tutorialActionLock != ActiveActionType.None && activeAction != tutorialActionLock) return;
		if(tutorialUnitLock != -1 && tutorialUnitLock != SelectedUnit.WorldObject.ID) return;
		if(tutorialAbilityIndex != -1 && ActionButtons.IndexOf(HudActionButton.SelectedButton) != tutorialAbilityIndex) return;

		if (ActionForce||activeAction == ActiveActionType.Overwatch)
		{
			if (!HudActionButton.SelectedButton.IsAbleToPerform(ActionTarget).Item1) return;//only check for point is we're forcing
		}
		else
		{

			var res = HudActionButton.SelectedButton.ShouldBeAbleToPerform(ActionTarget);
			if (!res.Item1)
			{
				ActionForce = true;
				UpdateHudButtons();
				return;
			}

		}

	

		if (activeAction == ActiveActionType.Action)
		{
			HudActionButton.SelectedButton!.PerformAction(ActionTarget);
		}
		else
		{
			HudActionButton.SelectedButton!.OverwatchAction(ActionTarget.TileLocation.Position);
		}


		SelectHudAction(null);
	}
	

	private static WorldObject? actionTarget;

	public static WorldObject? ActionTarget
	{
		get => actionTarget;
		set
		{
			actionTarget = value;
			if(actionTarget!= null && activeAction == ActiveActionType.Action)
				Camera.SetPos(actionTarget.TileLocation.Position);
		}
		
		
	}
	
	private static List<WorldObject> suggestedTargets = new();
	private int GetSelectedTargetIndex() {
		for (int i = 0; i < suggestedTargets.Count; i++)
		{
			if (suggestedTargets[i].Equals(ActionTarget))
			{
				return i;
			}
		}

		return -1;
	}

	static List<(Rectangle,Action<Vector2,float,SpriteBatch>)> _tooltipRects = new();
	
	private static readonly Dictionary<WorldObject,List<SequenceAction>> SortedConsequences = new();
	public override void RenderBehindHud(SpriteBatch batch, float deltatime)
	{
		if(SelectedUnit==null) return;
		
		base.RenderBehindHud(batch, deltatime);
		MakeUnitBarRenders(batch);
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
		
		
		var TileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		TileCoordinate = Vector2.Clamp(TileCoordinate, Vector2.Zero, new Vector2(99, 99));
	
		var mousepos = Utility.GridToWorldPos(TileCoordinate+new Vector2(-1.5f,-0.5f));
		for (int i = 0; i < 8; i++)
		{
			if(SequenceManager.SequenceRunningRightNow) break;//get cover gets blocked by sequence manager and freezes the game
			var indicator = TextureManager.GetSpriteSheet("coverIndicator",3,3)[i];
			Color c = Color.White;
			switch (WorldManager.Instance.GetCover(TileCoordinate,(Direction) i,ignoreControllables:true))
			{
				case Cover.Full:
					c = Color.Red;
					break;
				case Cover.High:
					c = Color.Yellow;
					break;
				case Cover.Low:
					c = Color.Green;
					break;
			}


			batch.Draw(indicator, mousepos, c);
		}

		var args = new Action.ActionExecutionParamters(TileCoordinate);

		switch (activeAction)
		{
			case ActiveActionType.Move:
				Action.Actions[Action.ActionType.Move].Preview(SelectedUnit, args, batch);
				break;
			case ActiveActionType.Face:
				Action.Actions[Action.ActionType.Face].Preview(SelectedUnit, args, batch);
				break;
			case ActiveActionType.Action:
				if (HudActionButton.SelectedButton == null)
				{
					throw new Exception("Action as active action without selected action button");
				}
				if(ActionTarget!=null)HudActionButton.SelectedButton.Preview(ActionTarget, batch);
				break;
			case ActiveActionType.Overwatch:
				if (ActionTarget != null) HudActionButton.SelectedButton.PreviewOverwatch(ActionTarget.TileLocation.Position, batch);
				break;
		}
		
		

		batch.End();

		if (activeAction == ActiveActionType.Action)
		{
			foreach (var target in suggestedTargets)
			{

				if (!target.IsVisible()) continue;

	
				PostProcessing.PostProcessing.SetOutlineEffectColor(Color.White);
				

				batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, effect: PostProcessing.PostProcessing.OutLineEffect);
				Texture2D sprite = target.GetTexture();
				batch.Draw(sprite, target.GetDrawTransform().Position + Utility.GridToWorldPos(new Vector2(1.5f, 0.5f)), null, Color.White, 0, sprite.Bounds.Center.ToVector2(), 1, SpriteEffects.None, 0);
				batch.End();
			}
		}

		if (ActionTarget != null && !ActionTarget.Type.Surface)
		{

			if (ActionTarget.UnitComponent != null && ActionTarget.UnitComponent.IsMyTeam())
			{
				PostProcessing.PostProcessing.SetOutlineEffectColor(Color.Green);
			}
			else
			{
				PostProcessing.PostProcessing.SetOutlineEffectColor(Color.Red);
			}
			batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, effect: PostProcessing.PostProcessing.OutLineEffect);
			Texture2D sprite = ActionTarget.GetTexture();
			batch.Draw(sprite, ActionTarget.GetDrawTransform().Position + Utility.GridToWorldPos(new Vector2(1.5f, 0.5f)), null, Color.White, 0, sprite.Bounds.Center.ToVector2(), 1, SpriteEffects.None, 0);
			batch.End();
		}
		
		
		
		
		panicAnimCounter += deltatime/100f;
		if (panicAnimCounter >= 4)
		{
			panicAnimCounter = 0;
		}

		counter += deltatime/1000f;
		if (counter > 2)
		{
			counter= 0;
		}
		animopacity = counter;
		if (counter > 1)
		{
			animopacity = 2- counter;
		}

		var tile = WorldManager.Instance.GetTileAtGrid(TileCoordinate);
		if (drawExtra)
		{
			foreach (var obj in tile.ObjectsAtLocation)
			{
				DrawHoverHud(batch, obj);

			}

			foreach (var edge in tile.GetAllEdges())
			{
				DrawHoverHud(batch, edge);
			}


		}
		var tiles = WorldManager.Instance.GetTilesAround(TileCoordinate, 10);
		foreach (var t in tiles)
		{
			foreach (var edge in t.GetAllEdges())
			{ 
				if (!Equals(edge.PreviewData, new PreviewData()))
				{ 
					DrawHoverHud(batch, edge);
				}
			}
		}
		foreach (var controllable in GameManager.GetAllUnits())
		{
			if (controllable.WorldObject.TileLocation != null && controllable.WorldObject.IsVisible())
			{
				DrawHoverHud(batch, controllable.WorldObject);
			}
		}
		
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);

		if (SelectedEnemyUnit != null)
		{

			foreach (var t in SelectedEnemyUnit?.VisibleTiles)
			{
				Color c = Color.White;
				if (t.Value == Visibility.Partial)
				{
					c = Color.Red;
				}

				batch.Draw(TextureManager.GetTexture("eye"), Utility.GridToWorldPos(t.Key+new Vector2(0.5f,0.5f)), null, c, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

			}

		}

		foreach (var u in GameManager.GetEnemyTeamUnits())
		{
			if (u.Overwatch.Item1)
			{
				foreach (var owTile in new List<Vector2Int>(u.OverWatchedTiles)){
					batch.Draw(TextureManager.GetTexture("HoverHud/overwatch"), Utility.GridToWorldPos(owTile), null, Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
				}
			}
		}
		batch.End();
		
		
		
		graphicsDevice.SetRenderTarget(timerRenderTarget);
	
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		
		var frame = TextureManager.GetTexture("GameHud/UnitBar/unitframe");
		batch.Draw(frame, new Vector2(0,0), null, Color.White, 0, Vector2.Zero, 1,SpriteEffects.None, 0);
		
		var totalLenght = 259 + 30;
		var fraction = GameManager.TimeTillNextTurn / (GameManager.PreGameData.TurnTime * 1000);
		var displayLenght = totalLenght - totalLenght * fraction;
		
		batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/Timer"), new Vector2(x:-46,y:-29), null, Color.Gray, 0, Vector2.Zero,1 ,SpriteEffects.None, 0);
		batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/Timer"), new Vector2(x:-46,y:-29), new Rectangle(0,0,190+(int)displayLenght,80), Color.White, 0, Vector2.Zero,1 ,SpriteEffects.None, 0);
		batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/scoreboard"), new Vector2(x:396,y:29), sourceRectangle:null, Color.Gray, 0, Vector2.Zero,scale:1 ,SpriteEffects.None, 0);
		batch.End();
		PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(TextureManager.GetTexture("GameHud/UnitBar/enemyTurn").Width,TextureManager.GetTexture("GameHud/UnitBar/enemyTurn").Height),false);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp,effect:PostProcessing.PostProcessing.UIGlowEffect);
		var turn = TextureManager.GetTexture("GameHud/UnitBar/enemyTurn");
		if (GameManager.IsMyTurn())
		{		
			turn = TextureManager.GetTexture("GameHud/UnitBar/yourTurn");
		}

		batch.Draw(turn, new Vector2(0, 0), null,Color.White, 0, Vector2.Zero, new Vector2(1f,1f),SpriteEffects.None, 0);
		batch.End();

		//final Draw
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		
		//batch.Draw(rightCornerRenderTarget, new Vector2(Game1.resolution.X - (rightCornerRenderTarget.Width-104)*globalScale.Y*1.3f, Game1.resolution.Y - rightCornerRenderTarget.Height*globalScale.Y*1.3f), null, Color.White, 0, Vector2.Zero, globalScale.Y*1.3f ,SpriteEffects.None, 0);

	
		batch.Draw(timerRenderTarget, new Vector2(Game1.resolution.X-timerRenderTarget.Width*GlobalScale.X*0.9f, 0), null, Color.White, 0, Vector2.Zero, GlobalScale.X*0.9f ,SpriteEffects.None, 0);
		
		if (activeAction == ActiveActionType.Action || activeAction == ActiveActionType.Overwatch)
		{
			var box = TextureManager.GetTexture("GameHud/BottomBar/mainBox");
			var line = TextureManager.GetTexture("GameHud/BottomBar/executeLine2");
			if (!HudActionButton.SelectedButton.HasPoints())
			{
				line = TextureManager.GetTexture("GameHud/BottomBar/executeLine1");
			}
			infoboxscale = GlobalScale.X * 0.8f;
			infoboxtip = new Vector2((Game1.resolution.X - box.Width * infoboxscale) / 2f, Game1.resolution.Y - (box.Height+2) * infoboxscale);
			batch.Draw(box, infoboxtip, null, Color.White, 0, Vector2.Zero, infoboxscale,SpriteEffects.None, 0);
			batch.Draw(line, infoboxtip, null, Color.White, 0, Vector2.Zero, infoboxscale,SpriteEffects.None, 0);
		}

		batch.End();
		
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		string chatmsg = "";
		int extraLines = 0;
		int width = 40;
		foreach (var msg in Chat.Messages)
		{
			chatmsg +=msg+"\n";
			if (msg.Length > width)
			{
				extraLines++;
			}

			extraLines++;
		}
		batch.DrawText(chatmsg,new Vector2(15,-7*extraLines+240*GlobalScale.Y),1.5f,width,Color.White);
		batch.End();
		
		if(SelectedUnit!=null)
		{
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
			var portrait = TextureManager.GetTextureFromPNG("Units/" + SelectedUnit.Type.Name+"/portrait");
			ushort hpPercent = (ushort) Math.Max(0,(float)SelectedUnit.Health / SelectedUnit.Type.MaxHealth*10f);
			ushort detPercent = (ushort) Math.Max(0,(float)SelectedUnit.Determination.Current / SelectedUnit.Type.Maxdetermination*10f);
			hpPercent = (ushort) Math.Min((ushort) 10,hpPercent);
			detPercent = (ushort) Math.Min((ushort) 10,detPercent);
			var detBar = TextureManager.GetTexture("GameHud/BottomBar/detBar"+(10 - detPercent));
			var hpBar = TextureManager.GetTexture("GameHud/BottomBar/hpBar"+(10 - hpPercent));
			var sight = TextureManager.GetTexture("GameHud/BottomBar/sightrange");
			var move = TextureManager.GetTexture("GameHud/BottomBar/moverange");
			var portraitScale = 1.25f*GlobalScale.Y;
			//infoboxtip = ;
			var padding = portrait.Width * 0.85f;
			batch.Draw(portrait, new Vector2(0, Game1.resolution.Y - portrait.Height * portraitScale),portraitScale,Color.White);
			batch.Draw(detBar, new Vector2(padding*portraitScale, Game1.resolution.Y - hpBar.Height* portraitScale ),portraitScale,Color.White);
			batch.Draw(hpBar, new Vector2(padding*portraitScale, Game1.resolution.Y - hpBar.Height* portraitScale),portraitScale,Color.White);
			batch.DrawText(SelectedUnit.Health.ToString(), new Vector2((padding+5)*portraitScale, Game1.resolution.Y - 20* portraitScale),portraitScale,24,Color.White);
			batch.DrawText(SelectedUnit.Determination.Current.ToString(), new Vector2((padding+30)*portraitScale, Game1.resolution.Y - 20* portraitScale),portraitScale,24,Color.White);
			batch.DrawText(SelectedUnit.Type.Name.ToString(),new Vector2(15,Game1.resolution.Y-50),portraitScale,Color.White);
			var pos = new Vector2((padding-20) * portraitScale, Game1.resolution.Y - 125 * portraitScale);
			Color c = Color.White;
			if (SelectedUnit.MoveRangeEffect.Current > 0)
			{
				c = Color.Green;
			}else if (SelectedUnit.MoveRangeEffect.Current < 0)
			{
				c = Color.Red;
			}
			batch.DrawText(""+SelectedUnit.GetMoveRange(), pos + new Vector2(-15,0), portraitScale, 24, c);
			batch.Draw(move, pos,portraitScale,Color.White);
			
			pos = new Vector2((padding-20) * portraitScale, Game1.resolution.Y - 150 * portraitScale);
			batch.DrawText(""+SelectedUnit.GetSightRange(), pos + new Vector2(-15,0), portraitScale, 24, Color.White);
			batch.Draw(sight, pos,portraitScale,Color.White);
			
			
			pos = new Vector2(145 * portraitScale, Game1.resolution.Y - 115 * portraitScale);
			batch.DrawNumberedIcon(SelectedUnit.ActionPoints.Current.ToString(), TextureManager.GetTexture("HoverHud/Consequences/circlePoint"), pos, portraitScale);
			pos = new Vector2(145 * portraitScale, Game1.resolution.Y - 80 * portraitScale);
			batch.DrawNumberedIcon(SelectedUnit.MovePoints.Current.ToString(), TextureManager.GetTexture("HoverHud/Consequences/movePoint"), pos, portraitScale);
			pos = new Vector2(145 * portraitScale, Game1.resolution.Y - 45 * portraitScale);
			batch.DrawNumberedIcon(SelectedUnit.Determination.Current.ToString(), TextureManager.GetTexture("HoverHud/Consequences/determinationFlame"), pos, portraitScale);
			if (SelectedUnit.CanTurn)
			{
				pos = new Vector2(145 * portraitScale, Game1.resolution.Y - 145 * portraitScale);
				batch.Draw(TextureManager.GetTexture("GameHud/BottomBar/turn"), pos, portraitScale,Color.White);
			}

			batch.End();
		}
		
		batch.Begin(transformMatrix:Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Immediate);
		//tooltip
		var mouse = Camera.GetMouseWorldPos();
		foreach (var rect in _tooltipRects)
		{
			if(rect.Item1.Contains(mouse))
			{
				var scale = 1 / Camera.GetZoom()*GlobalScale.X;
				var blank = TextureManager.GetTexture("");//)
				batch.Draw(blank,new Rectangle(mouse.ToPoint(),new Point((int) (350*scale),(int) (75*scale))),Color.Black*0.7f);
				rect.Item2.Invoke(mouse,scale,batch);
			}
		}
		
		if (highlightTile != new Vector2Int(-1, -1))
		{
			var surface = WorldManager.Instance.GetTileAtGrid(highlightTile).Surface;
			if (surface != null)
			{
				batch.Draw(surface!.GetTexture(), surface.GetDrawTransform().Position, null, Color.Red * animopacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
			}
		}
		batch.End();
		batch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Immediate);
		if (tutorialNote != "")
		{
			//draw middle of the screen
			var blank = TextureManager.GetTexture("");
			//batch.Draw(blank,new Rectangle(new Point((int) (Game1.resolution.X/2f),(int) (100*globalScale.X)),new Point((int) (350*globalScale.X),(int) (75*globalScale.X))),Color.Black*0.7f);
			//batch.DrawText(tutorialNote, new Vector2(Game1.resolution.X/2f,100*globalScale.X),globalScale.X,50,Color.White);
			Vector2 screenCenter = new Vector2(Game1.resolution.X / 2f, Game1.resolution.Y / 2f);
			
			
			Vector2 squareSize = new Vector2(460 * GlobalScale.X, 100 * GlobalScale.X);
			if (bigTutorialNote)
			{
				squareSize = new Vector2(460 * GlobalScale.X, 250 * GlobalScale.X);
			}
			Rectangle squareRect = new Rectangle((screenCenter - squareSize / 2f).ToPoint(), squareSize.ToPoint());
			squareRect.Y -= (int)(130 * GlobalScale.X);
			if (bigTutorialNote)
			{
				squareRect.Y += (int) (100 * GlobalScale.X);
			}
			batch.Draw(blank, squareRect, Color.Black * 0.7f);
			batch.DrawText(tutorialNote, new Vector2(squareRect.X,squareRect.Y), GlobalScale.X*0.9f, 63, Color.White);
		}
		batch.End();

		batch.Begin(transformMatrix: Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
		if (CheckCurrentTool() != null)
		{
			currentTool.render(batch);
		}
		batch.End();

		
	}
	private static Vector2 infoboxtip = new(0,0);
	private static float infoboxscale = 1f;

	public override void RenderFrontHud(SpriteBatch batch, float deltatime)
	{
		if(SelectedUnit==null) return;
		base.RenderFrontHud(batch, deltatime);
		
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);

		char[] characters = { 'Z','X','C','V', 'B', 'N' };
		for (int i = 0; i < ActionButtons.Count && i < characters.Length; i++)
		{
			batch.DrawText(characters[i].ToString(), new Vector2(ActionButtons[i].UIButton.Left + 16 * GlobalScale.Y + 3 * GlobalScale.Y, ActionButtons[i].UIButton.Top + Game1.resolution.Y + 1 * GlobalScale.Y), GlobalScale.Y * 1.6f, 1, Color.White);
			if (activeAction != ActiveActionType.Action && activeAction != ActiveActionType.Overwatch)
			{
				Vector2 start = new Vector2(ActionButtons[i].UIButton.Left + -4 * GlobalScale.Y + 3 * GlobalScale.Y, ActionButtons[i].UIButton.Top + Game1.resolution.Y + -70 * GlobalScale.Y);
				Vector2 offset = new Vector2(16,0)*GlobalScale.Y;
				int j = 0;
				Color c = Color.White;
				if (ActionButtons[i].Cost.MovePoints > 0)
				{
					if (SelectedUnit.MovePoints < ActionButtons[i].Cost.MovePoints) c = Color.Red;
					batch.DrawNumberedIcon(ActionButtons[i].Cost.MovePoints.ToString(), TextureManager.GetTexture("HoverHud/Consequences/movePoint"), start + offset*j, GlobalScale.Y * 0.7f, c, Color.White);
					j++;
				}

				if (ActionButtons[i].Cost.ActionPoints > 0)
				{
					c = Color.White;
					if (SelectedUnit.ActionPoints < ActionButtons[i].Cost.ActionPoints) c = Color.Red;
					batch.DrawNumberedIcon(ActionButtons[i].Cost.ActionPoints.ToString(), TextureManager.GetTexture("HoverHud/Consequences/circlePoint"), start + offset * j, GlobalScale.Y * 0.7f, c, Color.White);
					j++;
				}


				if (ActionButtons[i].Cost.Determination > 0)
				{
					c = Color.White;
					if (SelectedUnit.Determination < ActionButtons[i].Cost.Determination) c = Color.Red;
					batch.DrawNumberedIcon(ActionButtons[i].Cost.Determination.ToString(), TextureManager.GetTexture("HoverHud/Consequences/determinationFlame"), start + offset * j, GlobalScale.Y * 0.7f, c, Color.White);
				}
				
			}
		}

		
		if (activeAction == ActiveActionType.Action || activeAction == ActiveActionType.Overwatch)
		{
			batch.DrawText("spacebar",infoboxtip+new Vector2(165,5)*infoboxscale,infoboxscale*0.8f,50,Color.White);
			if (HudActionButton.SelectedButton.CanOverwatch)
			{
				batch.DrawText("ctrl", infoboxtip + new Vector2(240, 5) * infoboxscale, infoboxscale * 0.8f, 50, Color.White);
			}

			string toolTipText = HudActionButton.SelectedButton!.Tooltip;
	
			batch.DrawText(toolTipText,infoboxtip+new Vector2(45,38)*infoboxscale, infoboxscale*0.8f, 51, Color.White);
			Vector2 startpos = infoboxtip + new Vector2(19, 32) * infoboxscale;
			Vector2 offset = new Vector2(0, 40)*infoboxscale;

			AbilityCost cost = HudActionButton.SelectedButton.Cost;
			
			Color c = Color.White;
			if (cost.ActionPoints > SelectedUnit.ActionPoints) c = Color.Red;
			batch.DrawText(cost.ActionPoints.ToString(), startpos, GlobalScale.X * 2f, 24, c);

			c = Color.White;
			if (cost.MovePoints > SelectedUnit.MovePoints) c = Color.Red;
			batch.DrawText(cost.MovePoints.ToString(), startpos + offset, GlobalScale.X * 2f, 24, c);
			
			c = Color.White;
			if (cost.Determination > SelectedUnit.Determination) c = Color.Red;
			batch.DrawText(cost.Determination.ToString(), startpos + offset*2f, GlobalScale.X * 2f, 24, c);



			if (ActionTarget != null)
			{
				if (activeAction == ActiveActionType.Action)
				{
					var res = HudActionButton.SelectedButton.IsAbleToPerform(ActionTarget);
					if (res.Item1)
					{
						res = HudActionButton.SelectedButton.ShouldBeAbleToPerform(ActionTarget);
						Color g = Color.Yellow;
						if (ActionForce) g = Color.Red;
						if (!res.Item1)
						{
							batch.DrawText(res.Item2, infoboxtip + new Vector2(100-res.Item2.Length, -10) * infoboxscale, GlobalScale.X, 25, g);
						}
					}
					else
					{
						batch.DrawText(res.Item2, infoboxtip + new Vector2(100-res.Item2.Length, -10) * infoboxscale, GlobalScale.X, 25, Color.DarkRed);
					}
				}

			}
			
		}
		batch.End();

#if  DEBUG
		

		if (drawExtra)
		{
			
			var TileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			TileCoordinate = Vector2.Clamp(TileCoordinate, Vector2.Zero, new Vector2(99, 99));
			AIAction.MoveCalcualtion details;
			AIAction.MoveCalcualtion details2;
			var path = PathFinding.GetPath(SelectedUnit.WorldObject.TileLocation.Position, TileCoordinate);
			int moveUse = 1;
			while (path.Cost > SelectedUnit.GetMoveRange()*moveUse)
			{
				moveUse++;
			}
			int res = AIAction.GetTileMovementScore(TileCoordinate,moveUse,false,SelectedUnit, out details);
			int res2 = AIAction.GetTileMovementScore(TileCoordinate,moveUse,true, SelectedUnit, out details2);

			string text = $" Total: {res}\n Closest Distance: {details.ClosestDistance}\n Distance Reward: {details.DistanceReward}\n ProtectionPenalty: {details.ProtectionPentalty}\n";
			

			text += $" Clumping Penalty: {details.ClumpingPenalty}\n  Damage Potential: {details.DamagePotential}\n Cover Bonus: {details.CoverBonus}\n";
			batch.Begin(samplerState: SamplerState.AnisotropicClamp);
			batch.DrawText(text,Vector2.One,  3,100, Color.Green);
			batch.End();
			
			
			
			string text2 = $" Total: {res2}\n Closest Distance: {details2.ClosestDistance}\n Distance Reward: {details2.DistanceReward}\n ProtectionPenalty: {details2.ProtectionPentalty}\n";
			

			text2 += $" Clumping Penalty: {details2.ClumpingPenalty}\n Damage Potential: {details2.DamagePotential}\n Cover Bonus: {details2.CoverBonus}\n ";
			batch.Begin(samplerState: SamplerState.AnisotropicClamp);
			batch.DrawText(text2,new Vector2(700,0),  3,100, Color.Red);
			batch.End();
			AIMoveCache[(int) TileCoordinate.X, (int) TileCoordinate.Y, 1] = res2;
			AIMoveCache[(int) TileCoordinate.X, (int) TileCoordinate.Y, 0] = res;
		}

#endif

	}

	private Vector2Int _mouseTileCoordinate = new(0, 0);
	private Vector2Int _lastMouseTileCoordinate = new(0, 0);

	public override void Update(float deltatime)
	{
		base.Update(deltatime);
		_tooltipRects.Clear();
		var count = 0;

	

		//moves selected contorlable to the top
		_mouseTileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		_mouseTileCoordinate = Vector2.Clamp(_mouseTileCoordinate, Vector2.Zero, new Vector2(99, 99));

		if (_mouseTileCoordinate != _lastMouseTileCoordinate)
		{
			switch (activeAction)
			{
				case ActiveActionType.Move:
				case ActiveActionType.Face:
					activeAction = ActiveActionType.None;
					break;
			}
		}


		

		var units = GameManager.GetAllUnits();
		int targetIndex =units.IndexOf(SelectedUnit);
		if (targetIndex != -1)
		{
			
			for (int i = targetIndex; i <units.Count - 1; i++)
			{
				units[i] = units[i + 1];
				units[i + 1] = SelectedUnit;
			}
		}

		var tile = WorldManager.Instance.GetTileAtGrid(_mouseTileCoordinate);
		
		if (tile.UnitAtLocation != null)
		{
			targetIndex = units.IndexOf(tile.UnitAtLocation);
			if (targetIndex != -1)
			{
				Unit target = units[targetIndex];
				for (int i = targetIndex; i < units.Count - 1; i++)
				{
						
					units[i] = units[i + 1];
					units[i + 1] = target;
				}
			}
			
		}

		ProcessKeyboard();
		if(movePreviewDirty)
		{
			movePreviewDirty = false;
			ReMakeMovePreview();
		}
		//bad
		if (!GameManager.IsMyTurn())
		{
			if (endBtn != null) endBtn.Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("GameHud/UnitBar/end button")), Color.Gray);
		}
		else
		{
			if (endBtn != null) endBtn.Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("GameHud/UnitBar/end button")), Color.White);
		}
		_lastMouseTileCoordinate = _mouseTileCoordinate;
		
		
	}

	private bool drawExtra;
	
	public void ProcessKeyboard()
	{
		//Console.WriteLine(UI.Desktop.FocusedKeyboardWidget);
		if (lastKeyboardState.IsKeyDown(Keys.Tab) && currentKeyboardState.IsKeyUp(Keys.Tab))
		{
			UI.Desktop.FocusedKeyboardWidget = null;//override myra focus switch functionality
			//loop through all units and find one with unused movepoints
			var units = GameManager.GetMyTeamUnits();
			int currentIndex = units.IndexOf(SelectedUnit);

			// Start from the next unit
			currentIndex = (currentIndex + 1) % units.Count;

			// Loop through all units starting from the current
			for (int i = 0; i < units.Count; i++)
			{
				var unit = units[currentIndex];

				// If the unit has points left, select it and break the loop
				if (unit.MovePoints.Current > 0)
				{
					SelectUnit(unit);
					break;
				}

				// Move to the next unit, wrap around if necessary
				currentIndex = (currentIndex + 1) % units.Count;
			}
			
		}
		if(inputBox == null && inputBox.IsKeyboardFocused) return;
		
		//if(UI.Desktop.FocusedKeyboardWidget != null) return;


		drawExtra = currentKeyboardState.IsKeyDown(Keys.LeftAlt);

		if (JustPressed(Keys.Enter))
		{
			inputBox.Visible = true;
			inputBox?.SetKeyboardFocus();
		}

		if (currentKeyboardState.IsKeyDown(Keys.CapsLock) && !lastKeyboardState.IsKeyDown(Keys.CapsLock))
		{
			endBtn!.DoClick();
		}
	

		if (suggestedTargets.Count > 0)
		{
			if (JustPressed(Keys.Q))
			{
				int idx = GetSelectedTargetIndex();
				if (idx <= 0)
				{
					ActionTarget = suggestedTargets[suggestedTargets.Count - 1];
				}
				else
				{
					ActionTarget = suggestedTargets[idx - 1];
				}

				Camera.SetPos(ActionTarget.TileLocation.Position);
				UpdateHudButtons();
			}
			else if (JustPressed(Keys.E))
			{
				int idx = GetSelectedTargetIndex();
				if (idx >= suggestedTargets.Count - 1)
				{
					ActionTarget = suggestedTargets[0];
				}
				else
				{
					ActionTarget = suggestedTargets[idx + 1];
				}

				Camera.SetPos(ActionTarget.TileLocation.Position);
				UpdateHudButtons();
			}
		}

		if (JustPressed(Keys.Z))
		{
			if (ActionButtons.Count > 0)
			{
				ActionButtons[0].UIButton.DoClick();
			}
		}else if (JustPressed(Keys.X))
		{
			if (ActionButtons.Count >1)
			{
				ActionButtons[1].UIButton.DoClick();
			}
		}else if (JustPressed(Keys.C))
		{
			if (ActionButtons.Count > 2)
			{
				ActionButtons[2].UIButton.DoClick();
			}
		}else if (JustPressed(Keys.V))
		{
			if (ActionButtons.Count > 3)
			{
				ActionButtons[3].UIButton.DoClick();
			}
		}else if (JustPressed(Keys.B))
		{
			if (ActionButtons.Count > 4)
			{
				ActionButtons[4].UIButton.DoClick();
			}
		}else if (JustPressed(Keys.N))
		{
			if (ActionButtons.Count >5)
			{
				ActionButtons[5].UIButton.DoClick();
			}
		}

		if (JustPressed(Keys.D1) && GameManager.GetMyTeamUnits().Count > 0)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[0]);
		}else if (JustPressed(Keys.D2) && GameManager.GetMyTeamUnits().Count > 1)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[1]);
		}else if (JustPressed(Keys.D3) && GameManager.GetMyTeamUnits().Count > 2)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[2]);
		}else if (JustPressed(Keys.D4) && GameManager.GetMyTeamUnits().Count > 3)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[3]);
		}else if (JustPressed(Keys.D5) && GameManager.GetMyTeamUnits().Count > 4)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[4]);
		}else if (JustPressed(Keys.D6) && GameManager.GetMyTeamUnits().Count > 5)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[5]);
		}else if (JustPressed(Keys.D7) && GameManager.GetMyTeamUnits().Count > 6)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[6]);
		}else if (JustPressed(Keys.D8) && GameManager.GetMyTeamUnits().Count > 7)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[7]);
		}else if (JustPressed(Keys.D9) && GameManager.GetMyTeamUnits().Count > 8)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[8]);
		}else if (JustPressed(Keys.D0) && GameManager.GetMyTeamUnits().Count > 9)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[9]);
		}

		if (JustPressed(Keys.Space))
		{ 
			DoActiveAction();
		}

		if (JustPressed(Keys.LeftControl))
		{
			ToggleOverWatch();
		}

	}

	private enum ActiveActionType
	{
		Move,
		Face,
		Action,
		None,
		Overwatch,
	}

	private static ActiveActionType activeAction = ActiveActionType.None;

	public override void MouseDown(Vector2Int position, bool rightclick)
	{
		base.MouseDown(position, rightclick);

		position = Vector2.Clamp(position, Vector2.Zero, new Vector2(99, 99));
		
		var tile =WorldManager.Instance.GetTileAtGrid( position);

		if (CheckCurrentTool() != null)
		{
			Log.Message("Test"," 3. click called");
			currentTool.click(position, rightclick);
			return;
		}

		if (tile.UnitAtLocation != null&& tile.UnitAtLocation.WorldObject.GetMinimumVisibility() <= tile.GetVisibility() && (activeAction == ActiveActionType.None || activeAction == ActiveActionType.Move)) { 
			SelectUnit(tile.UnitAtLocation);
			return;
		}
		
		if(SelectedUnit == null) return;
		SelectedEnemyUnit = null;
		if (rightclick)
		{
			switch (activeAction)
			{
				case ActiveActionType.None:
					activeAction = ActiveActionType.Face;
					ActionTarget = tile.Surface;
					break;
				case ActiveActionType.Face:
					if(tutorialActionLock == ActiveActionType.None || (tutorialActionLock == ActiveActionType.Face && highlightTile == position))
						SelectedUnit.DoAction(Action.ActionType.Face, new Action.ActionExecutionParamters(position));
					break;
				case ActiveActionType.Move:
					activeAction = ActiveActionType.None;
					break;
				case ActiveActionType.Action:
				case ActiveActionType.Overwatch:
					SelectHudAction(null);
					break;
					

			}
		}
		else
		{
			switch (activeAction)
			{

				case ActiveActionType.None:
					activeAction = ActiveActionType.Move;
					ActionTarget = tile.Surface;
					break;
				case ActiveActionType.Face:
					activeAction = ActiveActionType.None;
					break;
				case ActiveActionType.Move:
					if(tutorialActionLock == ActiveActionType.None || (tutorialActionLock == ActiveActionType.Move && highlightTile == position))
					{
						if(tutorialUnitLock == -1 || tutorialUnitLock == SelectedUnit.WorldObject.ID)
							SelectedUnit.DoAction(Action.ActionType.Move, new Action.ActionExecutionParamters(position));
					}
					break;
				case ActiveActionType.Action:
					if (HudActionButton.SelectedButton.SelfOnly)
					{
						ActionTarget = SelectedUnit.WorldObject;
						break;
					}
					var edgeList = tile.GetAllEdges();
					if (drawExtra && edgeList.Count>0)
					{
						int currentlySelected = -1;
						if(ActionTarget != null)
						{
							currentlySelected = edgeList.IndexOf(ActionTarget);
						}
						if (currentlySelected == -1)
						{
							ActionTarget = edgeList[0];
						}
						else
						{
							if (currentlySelected == edgeList.Count - 1)
							{
								ActionTarget = edgeList[0];
							}
							else
							{
								ActionTarget = edgeList[currentlySelected + 1];
							}
						}
					}
					else
					{
						if(tile.UnitAtLocation!= null)
						{
							ActionTarget = tile.UnitAtLocation.WorldObject;
						}
						else
						{
							ActionTarget = tile.Surface;
						}
					}


					
				
					break;
				case ActiveActionType.Overwatch:
					ActionTarget = WorldManager.Instance.GetTileAtGrid(position).Surface;
					break;

			}

		}
		UpdateHudButtons();
	}


	private static float counter;
	private static float animopacity;
	private static TextBox? inputBox;
	private static float panicAnimCounter;


	private static int consPreviewTarget = -1;


	public static void DrawHoverHud(SpriteBatch batch, WorldObject obj)
	{

		Unit? unit = obj.UnitComponent;
		var MousePos = Camera.GetMouseWorldPos();
		var MousePosGrid = Utility.WorldPostoGrid(MousePos);
		
		graphicsDevice.SetRenderTarget(hoverHudRenderTarget);
		graphicsDevice.Clear(Color.White*0);
		float opacity = 0.95f;
		

		if (Equals(SelectedUnit, unit) || MousePosGrid == obj.TileLocation.Position || (obj.Type.Edge && Utility.IsClose(obj,MousePosGrid)))
		{
			opacity = 1;
		}

		var healthTexture = TextureManager.GetTexture("HoverHud/health");
		var nohealthTexture = TextureManager.GetTexture("HoverHud/nohealth");

		if (unit == null)
		{
			healthTexture = TextureManager.GetTexture("HoverHud/healthenv");
			nohealthTexture = TextureManager.GetTexture("HoverHud/nohealthenv");
		}
		float healthWidth = healthTexture.Width;
		float baseWidth = hoverHudRenderTarget.Width;
		float healthBarWidth = healthWidth*obj.Type.MaxHealth;
		float emtpySpace = baseWidth - healthBarWidth;
		//Vector2 healthBarPos = new Vector2(emtpySpace/2f,36);
		Vector2 healthBarPos = new Vector2(5f, 70);


		Vector2 offset = new Vector2(0,0);
		if (unit == null)
		{
			switch (obj.Facing)
			{
				case Direction.North:
					offset = new Vector2(1, 1);
					break;
				case Direction.West:
					offset = new Vector2(0.5f, 1);
					break;
			}
		}
	
		var hudDrawPoint = Utility.GridToWorldPos((Vector2) obj.TileLocation.Position + offset) + new Vector2(-10, -120);
		float hudScale = 1f;
		Vector2 finalHealthBarPos = default;
		for (int y = 0; y < obj.Type.MaxHealth; y++)
		{
			if (y >= obj.Health)
			{

				batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				batch.Draw(nohealthTexture, healthBarPos + new Vector2(healthWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
				batch.End();
			}
			else
			{
				
				batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				batch.Draw(healthTexture, healthBarPos + new Vector2(healthWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
				batch.End();
				if (obj.PreviewData.finalDmg >= obj.Health - y)
				{
					PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(TextureManager.GetTexture("HoverHud/detGreen").Width,TextureManager.GetTexture("HoverHud/detGreen").Height), true);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
					batch.Draw(healthTexture, healthBarPos + new Vector2(healthWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				
				}

			}
			finalHealthBarPos = healthBarPos + new Vector2(healthWidth * y, 0);
		}

		if (unit != null)
		{
			batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);

			if (unit.Overwatch.Item1)
			{
				var abl = unit.Abilities[unit.Overwatch.Item2];
				batch.Draw(abl.Icon,new Vector2(0,0),null,Color.White,0,Vector2.Zero,1f,SpriteEffects.None,0);
				batch.Draw(TextureManager.GetTexture("HoverHud/overwatch"),new Vector2(32,0),null,Color.White,0,Vector2.Zero,1f,SpriteEffects.None,0);
				_tooltipRects.Add(new ValueTuple<Rectangle, Action<Vector2,float,SpriteBatch>>(new Rectangle((int) hudDrawPoint.X, (int) hudDrawPoint.Y, (int) (32*hudScale), (int) (32*hudScale)),(pos, scale, innerBatch) =>
				{
					
					innerBatch.DrawText("      Overwatch Ability: [Green]"+abl.Name + "[-]\n" + abl.Tooltip, pos, scale, 48, Color.White);

				}));
				_tooltipRects.Add(new ValueTuple<Rectangle, Action<Vector2,float,SpriteBatch>>(new Rectangle((int) hudDrawPoint.X+32, (int) hudDrawPoint.Y, (int) (32*hudScale), (int) (32*hudScale)),(pos, scale, innerBatch) =>
				{
					innerBatch.DrawText("      Overwatch:\n" +
					                    "Can be interrupted by [Green]panicking[-] or [Green]dealing damage[-] to the unit\n"+
					                    "Unit will attack with [Red]"+abl.Name+"[-] any enemies entering the targeted area.\n"
						, pos, scale, 48, Color.White);

				}));
			}

			int i = 0;
			
			foreach (var effect in unit.StatusEffects)
			{
				var pos = new Vector2(23 * i, 30);
				var texture = TextureManager.GetTextureFromPNG("Icons/"+effect.Type.Name);
				batch.Draw(texture,pos,null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				batch.DrawText(effect.Duration+"", new Vector2(23*i+10,30), 1f, 100, Color.White);
				_tooltipRects.Add(new ValueTuple<Rectangle, Action<Vector2,float,SpriteBatch>>(new Rectangle((int) ((int)pos.X+hudDrawPoint.X), (int) ((int)pos.Y+hudDrawPoint.Y), (int) (texture.Width*hudScale), (int) (texture.Height*hudScale)),effect.DrawTooltip));

				i++;
			}
			batch.End();

			float detWidth = 0;
			float detHeight = 0;
			float detbarWidht;
			float detEmtpySpace;
			Vector2 DetPos = default;
		

			detWidth = TextureManager.GetTexture("HoverHud/detGreen").Width;
			detHeight = TextureManager.GetTexture("HoverHud/detGreen").Height;
			detbarWidht = detWidth * unit.Type.Maxdetermination;
			detEmtpySpace = baseWidth - detbarWidht;
			//DetPos = new Vector2(detEmtpySpace / 2f, 28);
			DetPos = new Vector2(5f, 60);

			
			var animSheet = TextureManager.GetSpriteSheet("HoverHud/panicSheet", 1, 4);
			for (int y = 0; y < unit.Type.Maxdetermination; y++)
			{
				if (unit.Panicked)
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(animSheet[(int) panicAnimCounter], DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
					continue;
				}

				if (y == unit.Determination)
				{
					batch.Begin(sortMode: SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(TextureManager.GetTexture("HoverHud/detBlank"), DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.Draw(TextureManager.GetTexture("HoverHud/detYellow"), DetPos + new Vector2(detWidth * y, 0), null, Color.White * (animopacity + 0.1f), 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}
				else if (y >= unit.Determination)
				{

					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(TextureManager.GetTexture("HoverHud/detBlank"), DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(TextureManager.GetTexture("HoverHud/detGreen"), DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
					if (unit.WorldObject.PreviewData.detDmg >= unit.Determination - y)
					{
						PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(TextureManager.GetTexture("HoverHud/detGreen").Width, TextureManager.GetTexture("HoverHud/detGreen").Height), true);
						batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
						batch.Draw(TextureManager.GetTexture("HoverHud/detGreen"), DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
						batch.End();

					}

				}



			}
		
			Vector2Int pointPos = new Vector2Int((int)healthBarPos.X, 53+30);
			int o = 10;
			i = 0;
			var nextT = unit.GetPointsNextTurn();
			for (int j = 0; j < Math.Max(nextT.Item1,unit.MovePoints.Current); j++)
			{
				Texture2D t;
				if (j < unit.MovePoints)
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					t = TextureManager.GetTexture("HoverHud/movepoint");
				
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					t = TextureManager.GetTexture("HoverHud/nomovepoint");
				}

				batch.Draw(t, pointPos + new Vector2(o * i, 0), Color.White);
				batch.End();
				i++;
			}

			i++;
			
			for (int j = 0; j < Math.Max(nextT.Item2,unit.ActionPoints.Current); j++)
			{	
				Texture2D t;
				if (j < unit.ActionPoints)
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					t = TextureManager.GetTexture("HoverHud/actionpoint");
				
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					t = TextureManager.GetTexture("HoverHud/noactionpoint");
				}
				batch.Draw(t, pointPos + new Vector2(o * i, 0), Color.White);
				batch.End();
				i++;
			}
		}
		if (unit != null)
		{
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		
		
			Texture2D indicator = TextureManager.GetTexture("HoverHud/friendly");
			if (!unit.IsMyTeam()) indicator = TextureManager.GetTexture("HoverHud/enemy");
			
			batch.Draw(indicator,finalHealthBarPos+new Vector2(10,0),Color.White);
			
			indicator = TextureManager.GetTexture("HoverHud/uncrouch");
			if(unit.Crouching) indicator = TextureManager.GetTexture("HoverHud/crouch");
			
			batch.Draw(indicator,finalHealthBarPos+new Vector2(26,0),Color.White);
		
			batch.End();
		}

		graphicsDevice.SetRenderTarget(consequenceListRenderTarget);
		graphicsDevice.Clear(Color.White*0);
		
		float consScale = 0.8f;
		var consDrawPoint = Utility.GridToWorldPos((Vector2) obj.TileLocation.Position + offset) + new Vector2(-consequenceListRenderTarget.Width, -80) * consScale;
		if (SortedConsequences.Count > 0 && SortedConsequences.ContainsKey(obj))
		{
			var rect = new Rectangle((int) consDrawPoint.X + consequenceListRenderTarget.Width - 64, (int) consDrawPoint.Y, (int) (64 * consScale), (int) (32 * consScale));
			if (rect.Contains(MousePos))
			{
				consPreviewTarget = obj.ID;
			}
			if(consPreviewTarget == obj.ID)
			{
				if (SortedConsequences.TryGetValue(obj, out var list))
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
					Vector2 pos = new Vector2(0, 0);
					foreach (var cons in list)
					{	
						if(!cons.ShouldDo())continue;
				
						_tooltipRects.Add(new ValueTuple<Rectangle, Action<Vector2,float,SpriteBatch>>(new Rectangle((int) ((int)pos.X+consDrawPoint.X), (int) ((int)pos.Y+consDrawPoint.Y), (int) (200*consScale), (int) (30*consScale)),cons.DrawTooltip));

						//batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/InfoBox"),pos,null,Color.White,0,Vector2.Zero,1f,SpriteEffects.None,0);
						cons.DrawConsequence(pos,batch);
						pos+= new Vector2(0, 30);
					}
					batch.End();
				}
			}
			else
			{
				batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
				batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/info"),new Vector2(consequenceListRenderTarget.Width-32,0), Color.White);
				batch.End();
			}


		


			
		}
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);

		batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(hoverHudRenderTarget,hudDrawPoint,null,Color.White*opacity,0,Vector2.Zero,hudScale,SpriteEffects.None,0);
		batch.Draw(consequenceListRenderTarget,consDrawPoint,null,Color.White*opacity,0,Vector2.Zero,consScale,SpriteEffects.None,0);
		batch.End();
		
	}

	private static void ToggleOverWatch()
	{
		if (activeAction == ActiveActionType.Action)
		{
			if (HudActionButton.SelectedButton!.CanOverwatch)
			{
				activeAction = ActiveActionType.Overwatch;
			}
			else
			{
				activeAction = ActiveActionType.Action;
			}


		}
		else if(activeAction == ActiveActionType.Overwatch)
		{
			activeAction = ActiveActionType.Action;
		}

		UpdateHudButtons();
	}

	private static void UpdateHudButtons()
	{
		if(ConfirmButton is null) return;
		if(targetBarStack is null) return;
		if(OverWatchToggle is null) return;
		foreach (var act in ActionButtons)
		{
			act.UpdateIcon();
		}

		switch (activeAction)
		{
			case ActiveActionType.None:
				ConfirmButton.Visible = false;
				targetBarStack!.Visible = false;
				OverWatchToggle.Visible = false;

				break;
			case ActiveActionType.Overwatch:
				OverWatchToggle.Image = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/overwatchON"));
				ConfirmButton!.Visible = true;
				targetBarStack!.Visible = false;
				OverWatchToggle.Visible = true;
				suggestedTargets.Clear();
				break;
			case ActiveActionType.Action:
				ConfirmButton!.Visible = true;
				targetBarStack!.Visible = true;
				OverWatchToggle.Visible = false;
				if (HudActionButton.SelectedButton.CanOverwatch)
				{
					OverWatchToggle.Image = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/overwatchOFF"));
					OverWatchToggle.Visible = true;
				}

				List<Unit> potentialTargets = new();
				GameManager.GetMyTeamUnits().ForEach(x => potentialTargets.Add(x));
				GameManager.GetEnemyTeamUnits().ForEach(x => potentialTargets.Add(x));
				suggestedTargets = HudActionButton.SelectedButton.GetSuggestedTargets(potentialTargets);
				if (ActionTarget != null && !HudActionButton.SelectedButton.IsAbleToPerform(ActionTarget).Item1)
				{
					ConfirmButton.Image = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/confirmation2"));
				}else if (ActionTarget != null && !HudActionButton.SelectedButton.ShouldBeAbleToPerform((WorldObject)ActionTarget).Item1)
				{
					if (ActionForce)
					{
						ConfirmButton.Image = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/confirmation4"));
					}
					else
					{
						ConfirmButton.Image = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/confirmation3"));
					}

				}
				else
				{
					ConfirmButton.Image = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/confirmation1"));
				}

				if (suggestedTargets.Count > 0 && ActionTarget == null)
				{
					ActionTarget = suggestedTargets[0];
				}
				if (ActionTarget != null)
				{
					consPreviewTarget = ActionTarget.ID;
				}
				else
				{
					consPreviewTarget = SelectedUnit.WorldObject.ID;
				}

				break;
		}
	

		/*if (suggestedTargets.Count < 2) targetBarStack.Visible = false;

		foreach (var unit in suggestedTargets)
		{
			var unitPanel = new ImageButton();

			Unit u = unit.UnitComponent;
			unitPanel.Click += (sender, args) =>
			{
				ActionTarget = unit;
				Console.WriteLine("Target set to " + ActionTarget);
			};
			targetBar!.Widgets.Add(unitPanel);
		}
		*/

		targetBarStack!.Proportions!.Clear();
		targetBarStack!.Proportions!.Add(new Proportion(ProportionType.Pixels, 40));
		targetBarStack!.Proportions!.Add(new Proportion(ProportionType.Pixels, 200 * suggestedTargets.Count));
		targetBarStack!.Proportions!.Add(new Proportion(ProportionType.Pixels, 40));
		
		
		SortedConsequences.Clear();
		if(ActionTarget != null && activeAction == ActiveActionType.Action)
		{
			foreach (var act in HudActionButton.SelectedButton.GetConsequences(ActionTarget))
			{
				if(act.GetType() == typeof(FaceUnit)) continue;
				if (act.GetType() == typeof(WorldObjectManager.TakeDamage))
				{
					var tkDmg = (WorldObjectManager.TakeDamage) act;
					WorldObject? obj = tkDmg.GetTargetObject();
					if(obj == null) continue;
					if (!SortedConsequences.ContainsKey(obj))
					{
						SortedConsequences.Add(obj,new List<SequenceAction>());
					}
					SortedConsequences[obj].Add(act);
			
				}if (act.GetType() == typeof(Shoot))
				{
					var shoot = (Shoot) act;
					var obj = WorldObjectManager.GetObject(shoot.Projectile.Result.HitObjId);
					if(obj == null) continue;
					if (!SortedConsequences.ContainsKey(obj))
					{
						SortedConsequences.Add(obj,new List<SequenceAction>());
					}
					SortedConsequences[obj].Add(act);
			
				}else if (act.IsUnitAction)
				{
					var uact = (UnitSequenceAction) act;
					var actor = uact.GetAffectedActor(-1);
					if (actor != null)
					{
						var obj = actor.WorldObject;
						if (!SortedConsequences.ContainsKey(obj))
						{
							SortedConsequences.Add(obj,new List<SequenceAction>());
						}
						SortedConsequences[obj].Add(act);
					}
				
				}
			}
		}
	
	}

	public static void SelectHudAction(HudActionButton? hudActionButton)
	{
		
		if (!GameManager.IsMyTurn())
		{
			hudActionButton = null;
		}
		HudActionButton.SelectedButton = hudActionButton;
		activeAction = ActiveActionType.Action;
		if (hudActionButton == null)
		{
			activeAction = ActiveActionType.None;
			
		}
		ActionTarget = null;
		ActionForce = false;
		
		UpdateHudButtons();
	}

	public static void CleanUp()
	{
		ActionButtons.Clear();
	}
}