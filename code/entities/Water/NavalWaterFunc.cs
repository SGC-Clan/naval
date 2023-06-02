using Editor;

namespace Sandbox
{
	/// <summary>
	/// Generic water volume. Make sure to have light probe volume envelop the volume of the water for the water to gain proper lighting.
	/// </summary>
	[Library( "func_water" )]
	[HammerEntity, Solid]
	[HideProperty( "enable_shadows" )]
	[HideProperty( "SetColor" )]
	[Title( "Water Volume" ), Category( "Gameplay" ), Icon( "water" )]
	public partial class NavalWaterFunc : NavalWater
	{
		public override void Spawn()
		{
			base.Spawn();

			Transmit = TransmitType.Always;

			CreatePhysics();
			EnableDrawing = false;
		}

		public override void ClientSpawn()
		{
			Game.AssertClient();
			base.ClientSpawn();

			CreatePhysics();
		}

		void CreatePhysics()
		{
			var physicsGroup = SetupPhysicsFromModel( PhysicsMotionType.Keyframed, true );
			physicsGroup.SetSurface( "water" );
		}
	}
}
