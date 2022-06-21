namespace Sandbox.Tools
{
	[Library( "tool_Mass", Title = "Mass", Description = "Mouse 1 - Set Mass to half of current mass; Mouse 2 - Set Mass to double of current mass; - Reload - Outputs Current Mass", Group = "construction" )]
	public partial class MassTool : BaseTool
	{
		private Prop target;

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				var startPos = Owner.EyePosition;
				var dir = Owner.EyeRotation.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit || !tr.Body.IsValid() || !tr.Entity.IsValid() || tr.Entity.IsWorld )
					return;

				if ( tr.Entity.PhysicsGroup == null || tr.Entity.PhysicsGroup.BodyCount > 1 )
					return;

				if ( tr.Entity is not Prop prop )
					return;

				if ( Input.Pressed( InputButton.PrimaryAttack ) )
				{

					var CurMass = prop.PhysicsGroup.Mass;

					prop.PhysicsGroup.Mass = CurMass / 2;

					CreateHitEffects( tr.EndPosition );

				}
				else if ( Input.Pressed( InputButton.SecondaryAttack ) )
				{

					var CurMass = prop.PhysicsGroup.Mass;

					prop.PhysicsGroup.Mass = CurMass * 2;

					CreateHitEffects( tr.EndPosition );

				}
				else if ( Input.Down( InputButton.Reload ) )
				{

					DebugOverlay.Text( "Mass: " + prop.PhysicsGroup.Mass, prop.Position  );

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
