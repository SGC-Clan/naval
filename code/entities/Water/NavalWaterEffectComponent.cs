namespace Sandbox.Component;

/// <summary>
/// Added to an entity when it enters water. Removed when it leaves water.
/// </summary>
public partial class NavalWaterEffectComponent : EntityComponent, ISingletonComponent
{
	/// <summary>
	/// The water entity we're inside of
	/// </summary>
	[Net, Predicted]
	public Entity WaterEntity { get; set; }

	/// <summary>
	/// How submerged we are in the water. 0 is none, 1 is over our head.
	/// </summary>
	[Net, Predicted]
	public float WaterLevel { get; set; }
}
