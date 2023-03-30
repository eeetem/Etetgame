using CommonData;

namespace MultiplayerXeno;

public class ProxyAttack : Attack
{
	public ProxyAttack() : base(null)
	{
	}

	protected override int GetDamage(Controllable actor)
	{
		return 3;
	}

	protected override int GetSupressionRange(Controllable actor)
	{
		return 2;
	}

	protected override int GetdeterminationResistanceEffect(Controllable actor)
	{
		return 1;
	}
}