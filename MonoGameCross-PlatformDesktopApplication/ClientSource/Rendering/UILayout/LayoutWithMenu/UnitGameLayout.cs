using CommonData;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.UILayouts.LayoutWithMenu;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class UnitGameLayout : GameLayout
{

	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		
		var root = base.Generate(desktop, lastLayout);
		if (UI.SelectedControllable != null && !UI.SelectedControllable.IsMyTeam()) return root;
		if (UI.SelectedControllable == null) return root;
		UI.ReMakeMovePreview();


		var localroot = (Panel) root;
		descBox = new Label()
		{
			Top = (int)(-70 * globalScale.Y),
			MaxWidth = (int) (600 * globalScale.X),
			MinHeight = 40,
			MaxHeight = 100,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Bottom,
			Background = new SolidBrush(Color.Black),
			Text = PreviewDesc,
			TextAlign = TextHorizontalAlignment.Center,
			Wrap = true,
			Font = DefaultFont.GetFont(FontSize/2)
		};
		localroot.Widgets.Add(descBox);


		var buttonContainer = new Grid()
		{
			GridColumn = 1,
			GridRow = 3,
			GridColumnSpan = 4,
			GridRowSpan = 1,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Bottom,
			//ShowGridLines = true,
		};
		localroot.Widgets.Add(buttonContainer);

		buttonContainer.RowsProportions.Add(new Proportion(ProportionType.Pixels, 20));
		var fire = new ImageButton()
		{
			GridColumn = 0,
			GridRow = 1,
			Width = (int)(60*globalScale.X),
			Height = (int)(70*globalScale.Y),
			Image = new TextureRegion(TextureManager.GetTexture("UI/Fire")),
			//	Scale = new Vector2(1.5f)
		};
		fire.Click += (o, a) => Action.SetActiveAction(ActionType.Attack);
		fire.MouseEntered += (o, a) => SetPreviewDesc("Shoot at a selected target. Anything in the blue area will get suppressed and lose determination. Cost: 1 action, 1 move");
		buttonContainer.Widgets.Add(fire);
		var watch = new ImageButton
		{
			GridColumn = 1,
			GridRow = 1,
			Width = (int)(60*globalScale.X),
			Height = (int)(70*globalScale.Y),
			Image = new TextureRegion(TextureManager.GetTexture("UI/Overwatch"))
		};
		watch.Click += (o, a) => Action.SetActiveAction(ActionType.OverWatch);
		watch.MouseEntered += (o, a) => SetPreviewDesc("Watch Selected Area. First enemy to enter the area will be shot at automatically. Cost: 1 action, 1 move. Unit Cannot act anymore in this turn");
		buttonContainer.Widgets.Add(watch);
		var crouch = new ImageButton
		{
			GridColumn = 2,
			GridRow = 1,
			Width = (int)(60*globalScale.X),
			Height = (int)(70*globalScale.Y),
			Image = new TextureRegion(TextureManager.GetTexture("UI/Crouch"))
		};
		crouch.MouseEntered += (o, a) => SetPreviewDesc("Crouching improves benefits of cover and allows hiding under tall cover however you can move less tiles. Cost: 1 move");
		crouch.Click += (o, a) =>
		{
			if (UI.SelectedControllable != null)
			{
				UI.SelectedControllable.DoAction(Action.Actions[ActionType.Crouch], null);
			}
		};
		buttonContainer.Widgets.Add(crouch);
		int column = 3;
		
		foreach (var act in UI.SelectedControllable.Type.extraActions)
		{
			var actBtn = new ImageButton
			{
				GridColumn = column,
				GridRow = 1,
				Width = (int)(60*globalScale.X),
				Height = (int)(70*globalScale.Y),
				Image = new TextureRegion(TextureManager.GetTexture("UI/" + act.Item1))
			};
			actBtn.Click += (o, a) => Action.SetActiveAction(act.Item2);
			actBtn.MouseEntered += (o, a) => SetPreviewDesc(Action.Actions[act.Item2].Description);
			buttonContainer.Widgets.Add(actBtn);
			column++;
		}

		if (UI.SelectedControllable != null)
		{
			var unitDetails = new VerticalStackPanel();
			unitDetails.VerticalAlignment = VerticalAlignment.Bottom;
			unitDetails.Background = new SolidBrush(Color.Black);
			unitDetails.HorizontalAlignment = HorizontalAlignment.Right;
			localroot.Widgets.Add(unitDetails);
			unitDetails.Widgets.Add(new Label()
			{
				Text = "Move Range:"+UI.SelectedControllable.Type.MoveRange,
				Font = DefaultFont.GetFont(FontSize/2),
				TextAlign = TextHorizontalAlignment.Center
			});
			unitDetails.Widgets.Add(new Label()
			{
				Text = "Sight Range:"+UI.SelectedControllable.Type.SightRange,
				Font = DefaultFont.GetFont(FontSize/2),
				TextAlign = TextHorizontalAlignment.Center
			});
			unitDetails.Widgets.Add(new Label()
			{
				Text = "Weapon Range:"+UI.SelectedControllable.Type.WeaponRange,
				Font = DefaultFont.GetFont(FontSize/2),
				TextAlign = TextHorizontalAlignment.Center
			});
			unitDetails.Widgets.Add(new Label()
			{
				Text = "Damage:"+UI.SelectedControllable.Type.WeaponDmg,
				Font = DefaultFont.GetFont(FontSize/2),
				TextAlign = TextHorizontalAlignment.Center
			});
		}

		return localroot;
	}

}