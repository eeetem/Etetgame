
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace DefconNull.Rendering.CustomUIElements;

public sealed class TextLabel : Grid
{
	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
			GenerateText();
		}
	}
	

	private string _text = "";

	public TextLabel() : base()
	{
		Background = new SolidBrush(Color.Transparent);
	}

	public void GenerateText()
	{
		Width = (int?) (_text.Length * UILayout.UiLayout.GlobalScale.X * 24*Height);
		InvalidateMeasure();
		ColumnsProportions.Clear();
		
		Task.Run(() =>
		{
			Thread.Sleep(100);
			int row = 0;
			int coulumn = 0;
			foreach (char c in _text)
			{
				Thread.Sleep(10);
				var charImage = new Image();
				charImage.Background = new SolidBrush(Color.Transparent);
				charImage.Renderable = new TextureRegion(TextureManager.GetTextTexture(c));
				if(c == ' ')
					charImage.Renderable = new ColoredRegion((TextureRegion) charImage.Renderable,Color.Transparent);

				charImage.VerticalAlignment = VerticalAlignment.Center;
				charImage.Left = 1;
				charImage.Height = Height-5;
				charImage.Width = Height-5;
				ColumnSpacing = 0;
				RowSpacing = 0;
				ColumnsProportions.Add(new Proportion(ProportionType.Auto));
				Widgets.Add(charImage);
				SetRow(charImage, row);
				SetColumn(charImage, coulumn);
				coulumn++;
			}
			InvalidateMeasure();
		});

		

	}
	
}