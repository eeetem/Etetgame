using System.Collections.Generic;
using CommonData;
using MultiplayerXeno;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class GameSetupLayout : UiLayout
{
	private List<SquadMember> _composition = new List<SquadMember>();
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{

		var panel = new Panel();

		var freeslots = new Label()
		{
				
			Text = "Free Units "+(WorldManager.Instance.CurrentMap.unitCount-_composition.Count),
		};
		panel.Widgets.Add(freeslots);

		var unitStack = new HorizontalStackPanel();
		unitStack.HorizontalAlignment = HorizontalAlignment.Center;
		unitStack.VerticalAlignment = VerticalAlignment.Bottom;
		panel.Widgets.Add(unitStack);
			
			
		//one button for each unit type
		string[] units = new []{"Gunner", "Heavy", "Scout"};

		foreach (var unit in units)
		{
			var unitButton = new TextButton()
			{
				Text = unit,
				GridColumn = 1,
				GridRow = 1,
			};
			unitButton.Click += (s, a) =>
			{
				//add unit to composition
				_composition.Add(new SquadMember()
				{
					ClassName = unit,
					Position = new Vector2Int(0,0)
				});
				freeslots.Text = "Free Units "+(WorldManager.Instance.CurrentMap.unitCount-_composition.Count);
				unitStack.Widgets.Add(new TextButton()
				{
					Text = unit,
					GridColumn = 1,
					GridRow = 1,
				});
			};
			panel.Widgets.Add(unitButton);
		}


		var confirm = new TextButton
		{
			GridColumn = 2,
			GridRow = 4,
			Text = "Confirm"
		};
		confirm.Click += (s, a) =>
		{
			
			Networking.serverConnection.Send(packet);
			//UI.SetUI(new GameLayout());
			//awaiting player
		};
		panel.Widgets.Add(confirm);


		return panel;
	}
}