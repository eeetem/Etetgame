using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using IDrawable = DefconNull.Rendering.IDrawable;

namespace DefconNull.WorldObjects;

public partial class Unit// :IDrawable
{
	
    public bool IsMyTeam()
    {
        return GameManager.IsPlayer1 == IsPlayer1Team;
    }


}