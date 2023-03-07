using CommonData;
using MultiplayerXeno;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class GameSetupLayout : UiLayout
{
	
	private static int soldierCount = 0;
	private static int scoutCount = 0;
	private static int heavyCount = 0;

	public override Widget Generate(Desktop desktop)
	{
		
			var grid = new Grid
			{

				ColumnSpacing = 0,
				RowSpacing = 0,
				
			};

			
			//soldier counter
			var soldierButton = new TextButton
			{
			
				GridColumn = 2,
				GridRow = 1,
				Text = "Soldiers: "+soldierCount
			};
			grid.Widgets.Add(soldierButton);
			var freeslots = new Label()
			{
			
				GridColumn = 2,
				GridRow = 0,
				Text = "Free Units "+(WorldManager.Instance.CurrentMap.unitCount-soldierCount-scoutCount-heavyCount),
			};
			grid.Widgets.Add(freeslots);
			
			var soldierLeft = new TextButton
			{
				GridColumn = 1,
				GridRow = 1,
				Text = "<"
			};
			soldierLeft.Click += (s, a) =>
			{
				soldierCount--;
				if (soldierCount < 0)
				{
					soldierCount = 0;
				}

				soldierButton.Text = "Soldiers: " + soldierCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(soldierLeft);
			var soldierRight = new TextButton
			{
				GridColumn = 3,
				GridRow = 1,
				Text = ">"
			};
			soldierRight.Click += (s, a) =>
			{
				if (soldierCount + scoutCount + heavyCount+1 >  WorldManager.Instance.CurrentMap.unitCount)
				{
					return;
				}

				soldierCount++;
				soldierButton.Text = "Soldiers: " + soldierCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(soldierRight);
			
			//scout counter
			var scoutButton = new TextButton
			{
				GridColumn = 2,
				GridRow = 3,
				Text = "Scouts: "+scoutCount
			};
			grid.Widgets.Add(scoutButton);
			var scountLeft = new TextButton
			{
				GridColumn = 1,
				GridRow = 3,
				Text = "<"
			};
			scountLeft.Click += (s, a) =>
			{
				scoutCount--;
				if (scoutCount < 0)
				{
					scoutCount = 0;
				}

				scoutButton.Text = "Scouts: " + scoutCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(scountLeft);
			var scoutRight = new TextButton
			{
				GridColumn = 3,
				GridRow = 3,
				Text = ">"
			};
			scoutRight.Click += (s, a) =>
			{
				if (soldierCount + scoutCount + heavyCount+ 1 > WorldManager.Instance.CurrentMap.unitCount)
				{
					return;
				}

				scoutCount++;
				scoutButton.Text = "Scouts: " + scoutCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(scoutRight);
			
			
			//heavy counter
			var heavyButton = new TextButton
			{
				GridColumn = 2,
				GridRow = 2,
				Text = "Heavies: "+heavyCount
			};
			grid.Widgets.Add(heavyButton);
			var heavyLeft = new TextButton
			{
				GridColumn = 1,
				GridRow = 2,
				Text = "<"
			};
			heavyLeft.Click += (s, a) =>
			{
				heavyCount--;
				if (heavyCount < 0)
				{
					heavyCount = 0;
				}

				heavyButton.Text = "Heavies: " + heavyCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(heavyLeft);
			var heavyRight = new TextButton
			{
				GridColumn = 3,
				GridRow = 2,
				Text = ">"
			};
			heavyRight.Click += (s, a) =>
			{
				if (soldierCount + scoutCount + heavyCount+ 1 >  WorldManager.Instance.CurrentMap.unitCount)
				{
					return;
				}

				heavyCount++;
				heavyButton.Text = "Heavies: " + heavyCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(heavyRight);


			var confirm = new TextButton
			{
				GridColumn = 2,
				GridRow = 4,
				Text = "Confirm"
			};
			confirm.Click += (s, a) =>
			{
				UnitStartDataPacket packet = new UnitStartDataPacket();
				packet.Scouts = scoutCount;
				packet.Soldiers = soldierCount;
				packet.Heavies = heavyCount;
				Networking.serverConnection.Send(packet);
				UI.SetUI(new GameLayout());
			};
			grid.Widgets.Add(confirm);


			return grid;
	}
}