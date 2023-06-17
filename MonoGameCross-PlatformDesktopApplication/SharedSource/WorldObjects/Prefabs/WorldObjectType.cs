using MultiplayerXeno;


namespace MultiplayerXeno
{
	public partial class WorldObjectType
	{

		public readonly string? TypeName;
		public int MaxHealth;
		public int lifetime = -100;
		public WorldObjectType(string? name,UnitType? unitType)
		{
			TypeName = name;
			if (unitType != null)
			{
				Unit = unitType;
			}
			
		}

		public void SpecialBehaviour(WorldObject objOfType)
		{
			switch (TypeName)
			{
				case "capturePoint":
					GameManager.CapturePoints.Add(objOfType);
					break;
				case "spawnPointT1":

					GameManager.T1SpawnPoints.Add(objOfType.TileLocation.Position);
		
					break;
				case "spawnPointT2":

					GameManager.T2SpawnPoints.Add(objOfType.TileLocation.Position);

					break;
			}
		}


		public Cover SolidCover = Cover.None;
		public Cover VisibilityCover = Cover.None;

		public readonly UnitType? Unit;
		public WorldEffect? DesturctionEffect;

		//should probably be an enum
		public bool Faceable { get; set; }
		public bool Edge { get; set; }
		public bool Surface { get; set; }
		public bool Impassible { get; set; }
		public int VisibilityObstructFactor { get; set; }
	}
}