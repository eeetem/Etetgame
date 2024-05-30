#nullable enable

using System.Collections.Generic;
using DefconNull.Networking;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Thickness = Myra.Graphics2D.Thickness;

namespace DefconNull.Rendering.UILayout;

public class SquadCompBuilderLayout : MenuLayout
{
	private static readonly List<SquadMember> MyComposition = new List<SquadMember>();
	private static readonly List<SquadMember> OtherComposition = new List<SquadMember>();
	private Label freeslots;
	private Label otherfreeslots;
	private List<Vector2Int> _mySpawnPoints = new List<Vector2Int>();
	private List<Vector2Int> _otherSpawnPoints = new List<Vector2Int>();//for practice mode

	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		WorldManager.Instance.MakeFovDirty();
		var panel = new Panel();
		_mySpawnPoints= GameManager.IsPlayer1 ?GameManager.T1SpawnPoints :GameManager.T2SpawnPoints;
		if(_mySpawnPoints.Count != 0)
			Camera.SetPos(_mySpawnPoints[0]);
		if (GameManager.spectating)
		{
			_mySpawnPoints = GameManager.T1SpawnPoints;

			_otherSpawnPoints = GameManager.T2SpawnPoints;
		}

		//Camera.SetPos(_mySpawnPoints[0]);
		freeslots = new Label()
		{
			Text = "Free Units: "+(WorldManager.Instance.CurrentMap.unitCount-MyComposition.Count),
		};
		otherfreeslots = new Label()
		{
			Top = 50,
			Text = "Free Units(team 2): "+(WorldManager.Instance.CurrentMap.unitCount-OtherComposition.Count),
			Visible = GameManager.spectating
		};
		panel.Widgets.Add(freeslots);
		panel.Widgets.Add(otherfreeslots);

		var unitStack = new HorizontalStackPanel();
		unitStack.HorizontalAlignment = HorizontalAlignment.Center;
		unitStack.VerticalAlignment = VerticalAlignment.Bottom;
		unitStack.Spacing = 25;
		panel.Widgets.Add(unitStack);


		List<string> units = new List<string>();
		//one button for each unit type
		foreach (var obj in PrefabManager.UnitPrefabs)
		{
			units.Add(obj.Value.Name);
			
		}

		foreach (var unit in units)
		{
			var unitButton = new ImageTextButton()
			{
				Text = unit,
				Background = null,
				
				GridColumn = 1,
				GridRow = 1,
				Top = -50,	
				Image = new TextureRegion(TextureManager.GetTexture("Squadcomp/"+ unit + "/Icon"))
				
				
			};
			unitButton.Click += (s, a) =>
			{
				if (MyComposition.Count >= WorldManager.Instance.CurrentMap.unitCount)
				{
					return;
				}
				StartPlacing(unit);
			};
			unitStack.Widgets.Add(unitButton);
		}


		var confirm = new TextButton
		{
			VerticalAlignment = VerticalAlignment.Bottom,
			HorizontalAlignment = HorizontalAlignment.Center,
			Text = "Confirm"
		};
		confirm.Click += (s, a) =>
		{
			if (GameManager.spectating)
			{
				NetworkingManager.SendDualSquadComp(MyComposition,OtherComposition);
			}
			else
			{
				NetworkingManager.SendSquadComp(MyComposition);
				var lbl = new Label();
				lbl.Text = "Waiting for other players";
				lbl.HorizontalAlignment = HorizontalAlignment.Center;
				lbl.VerticalAlignment = VerticalAlignment.Center;
				panel.Widgets.Add(lbl);
			}


		};
		panel.Widgets.Add(confirm);


		return panel;
	}

	
	static SquadMember? _currentlyPlacing;
	private static void StartPlacing(string unit)
	{
		_currentlyPlacing = new SquadMember(unit);
	}

	private Panel? itemMenu;
	public override void MouseDown(Vector2Int position, bool rightclick)
	{
		base.MouseDown(position, rightclick);
		SquadMember? memberAtLocation = null;
		foreach (var member in MyComposition)
		{
			if (member.Position == position)
			{
				memberAtLocation = member;
			}
		}
		foreach (var member in OtherComposition)
		{
			if (member.Position == position)
			{
				memberAtLocation = member;
			}
		}

		if(_currentlyPlacing!=null&& memberAtLocation == null && (_mySpawnPoints.Contains(position)|| _otherSpawnPoints.Contains(position)))
		{
			var currentlyPlacing = _currentlyPlacing.Value;
			currentlyPlacing.Position = position;
			if(_mySpawnPoints.Contains(position))
			{
				MyComposition.Add(currentlyPlacing);
			}
			else
			{
				OtherComposition.Add(currentlyPlacing);
			}
			
			var placed = currentlyPlacing;
			_currentlyPlacing = null;
			UI.Desktop.Widgets.Remove(itemMenu);
			itemMenu = new Panel();
			itemMenu.HorizontalAlignment = HorizontalAlignment.Center;
			itemMenu.VerticalAlignment = VerticalAlignment.Center;
			itemMenu.Background = new SolidBrush(Color.Black * 0.5f);
			itemMenu.BorderThickness = new Thickness(1);


			UI.Desktop.Widgets.Add(itemMenu);
			
		}
		else if(memberAtLocation!=null)
		{
			_currentlyPlacing = memberAtLocation;
			MyComposition.Remove(memberAtLocation.Value);
			OtherComposition.Remove(memberAtLocation.Value);
		}

		freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - MyComposition.Count);
		otherfreeslots.Text = "Free Units(team 2): " + (WorldManager.Instance.CurrentMap.unitCount - OtherComposition.Count);
	}

	public override void Update(float deltatime)
	{
		base.Update(deltatime);
		if (_currentlyPlacing != null)
		{
			var TileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			TileCoordinate = Vector2.Clamp(TileCoordinate, Vector2.Zero, new Vector2(99, 99));
			var Mousepos = TileCoordinate;
			_currentlyPlacing = _currentlyPlacing.Value with { Position = Mousepos };
		}

	}

	public override void RenderBehindHud(SpriteBatch batch, float deltatime)
	{
		base.RenderBehindHud(batch, deltatime);
		batch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		if (_currentlyPlacing.HasValue)
		{
			var previewSprite = PrefabManager.UnitPrefabs[_currentlyPlacing.Value.Prefab].GetSprite(0,0,"/Stand");
			batch.Draw(previewSprite, Utility.GridToWorldPos(_currentlyPlacing.Value.Position+ new Vector2(-1.5f, -0.5f)), Color.White*0.5f);
		}

		foreach (var member in MyComposition)
		{
			var previewSprite = PrefabManager.UnitPrefabs[member.Prefab].GetSprite(0,0,"/Stand");
			batch.Draw(previewSprite, Utility.GridToWorldPos(member.Position+ new Vector2(-1.5f, -0.5f)), Color.White);
			
		}
		foreach (var member in OtherComposition)
		{
			var previewSprite = PrefabManager.UnitPrefabs[member.Prefab].GetSprite(0,0,"/Stand");
			batch.Draw(previewSprite, Utility.GridToWorldPos(member.Position+ new Vector2(-1.5f, -0.5f)), Color.White);
			
		}
		foreach (var point in _mySpawnPoints)
		{
			batch.DrawCircle(Utility.GridToWorldPos((Vector2)point-new Vector2(-0.5f,-0.5f)),10,10,Color.Red,20f);
		}
		foreach (var point in _otherSpawnPoints)
		{
			batch.DrawCircle(Utility.GridToWorldPos((Vector2)point-new Vector2(-0.5f,-0.5f)),10,10,Color.Green,20f);
		}
		batch.End();
	}
}