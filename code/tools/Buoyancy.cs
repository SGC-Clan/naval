namespace Sandbox.Tools
{
	[Library( "tool_buoyancy", Title = "Buoyancy", Description = "Mouse 1 - Make prop buoyant", Group = "construction" )]
	public partial class BuoyancyTool : BaseTool
	{
		private Prop target;

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit || !tr.Body.IsValid() || !tr.Entity.IsValid() || tr.Entity.IsWorld )
					return;

				if ( tr.Entity.PhysicsGroup == null || tr.Entity.PhysicsGroup.BodyCount > 1 )
					return;

				if ( tr.Entity is not Prop prop )
					return;

				if ( Input.Pressed( InputButton.Attack1 ) )
				{

					//var CurMass = prop.PhysicsGroup.Mass;

					//prop.PhysicsGroup.Mass = CurMass / 2;
					prop.PhysicsGroup.SetSurface( "wood" );

					CreateHitEffects( tr.EndPos );
					//ViewModelEntity?.SetAnimBool( "reload", true );

				}
				else if ( Input.Down( InputButton.Attack2 ) )
				{

					//DebugOverlay.Text( prop.Position, "Mass: " + prop.PhysicsGroup.Mass );

				}
				else
				{
					return;
				}


			}
		}

		private void Reset()
		{
			target = null;
		}

		public override void Activate()
		{
			base.Activate();

			Reset();
		}

		public override void Deactivate()
		{
			base.Deactivate();

			Reset();
		}
	}
}
