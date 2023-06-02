using Sandbox.Component;

namespace Sandbox;

/// <summary>
/// Adds functions to Entity
/// </summary>
public static class NavalWaterExtensions
{
	/// <summary>
	/// Get the water level for this entity
	/// </summary>
	public static float GetNavalWaterLevel( this IEntity self )
	{
		var c = self.Components.Get<NavalWaterEffectComponent>();
		return c?.WaterLevel ?? 0;
	}

	/// <summary>
	/// Clear any current water level. Pretend we're not in water anymore.
	/// Usually called on Respawn.
	/// </summary>
	public static void ClearNavalWaterLevel( this IEntity self )
	{
		self.Components.RemoveAny<NavalWaterEffectComponent>();
	}
}
