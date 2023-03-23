#nullable enable
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MultiplayerXeno;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class GameSetupLayout : UiLayout
{
	private readonly List<SquadMember> _composition = new List<SquadMember>();
	private Label freeslots;
	private List<Vector2Int> mySpawnPoints = new List<Vector2Int>();
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{

		var panel = new Panel();
		
		mySpawnPoints= GameManager.IsPlayer1 ? GameManager.T1SpawnPoints : GameManager.T2SpawnPoints;
		Camera.SetPos(mySpawnPoints[0]);
		
		freeslots = new Label()
		{
			Text = "Free Units "+(WorldManager.Instance.CurrentMap.unitCount-_composition.Count),
		};
		panel.Widgets.Add(freeslots);

		var unitStack = new HorizontalStackPanel();
		unitStack.HorizontalAlignment = HorizontalAlignment.Center;
		unitStack.VerticalAlignment = VerticalAlignment.Bottom;
		unitStack.Spacing = 25;
		panel.Widgets.Add(unitStack);
			
			
		//one button for each unit type
		string[] units = {"Gunner", "Heavy", "Scout"};

		foreach (var unit in units)
		{
			var unitButton = new TextButton()
			{
				Text = unit,
				GridColumn = 1,
				GridRow = 1,
				Top = -50,	
			};
			unitButton.Click += (s, a) =>
			{
				if (_composition.Count >= WorldManager.Instance.CurrentMap.unitCount)
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
			SquadCompPacket packet = new SquadCompPacket();
			packet.Composition = _composition;
			Networking.serverConnection.Send(packet);
			var lbl = new Label();
			lbl.Text = "Waiting for other players";
			lbl.HorizontalAlignment = HorizontalAlignment.Center;
			lbl.VerticalAlignment = VerticalAlignment.Center;
			panel.Widgets.Add(lbl);
		};
		panel.Widgets.Add(confirm);


		return panel;
	}

	
	SquadMember? _currentlyPlacing;
	private void StartPlacing(string unit)
	{
		_currentlyPlacing = new SquadMember();
		_currentlyPlacing.Prefab = unit;
	}

	public override void MouseDown(Vector2Int position, bool righclick)
	{
		base.MouseDown(position, righclick);
		SquadMember memberAtLocation = null;
		foreach (var member in _composition)
		{
			if (member.Position == position)
			{
				memberAtLocation = member;
			}
		}

		if(_currentlyPlacing!=null&& memberAtLocation == null && mySpawnPoints.Contains(position))
		{
			_currentlyPlacing.Position = position;
			_composition.Add(_currentlyPlacing);
			_currentlyPlacing = null;
		}
		else if(memberAtLocation!=null)
		{
			_currentlyPlacing = memberAtLocation;
			_composition.Remove(memberAtLocation);
		}

		freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - _composition.Count);
	}

	public override void Update(float deltatime)
	{
		base.Update(deltatime);
		if (_currentlyPlacing != null)
		{
			var TileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			TileCoordinate = Vector2.Clamp(TileCoordinate, Vector2.Zero, new Vector2(99, 99));
			var Mousepos = TileCoordinate;
			_currentlyPlacing.Position = Mousepos;
		}

	}

	public override void Render(SpriteBatch batch, float deltatime)
	{
		base.Render(batch, deltatime);
	
		if (_currentlyPlacing != null)
		{
			var previewSprite = PrefabManager.Prefabs[_currentlyPlacing.Prefab].spriteSheet[0][0];
			batch.Draw(previewSprite, Utility.GridToWorldPos(_currentlyPlacing.Position+ new Vector2(-1.5f, -0.5f)), Color.White*0.5f);
		}

		foreach (var member in _composition)
		{
			var previewSprite = PrefabManager.Prefabs[member.Prefab].spriteSheet[0][0];
			batch.Draw(previewSprite, Utility.GridToWorldPos(member.Position+ new Vector2(-1.5f, -0.5f)), Color.White);
		}
		foreach (var point in mySpawnPoints)
		{
			batch.DrawCircle(Utility.GridToWorldPos((Vector2)point-new Vector2(-0.5f,-0.5f)),10,10,Color.Red,20f);
		}
	}
}