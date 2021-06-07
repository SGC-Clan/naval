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
				var input = Owner.Input;
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

				if ( input.Pressed( InputButton.Attack1 ) )
				{

					var CurMass = prop.PhysicsGroup.Mass;

					prop.PhysicsGroup.Mass = CurMass / 2;

					CreateHitEffects( tr.EndPos );

				}
				else if ( input.Pressed( InputButton.Attack2 ) )
				{

					var CurMass = prop.PhysicsGroup.Mass;

					prop.PhysicsGroup.Mass = CurMass * 2;

					CreateHitEffects( tr.EndPos );

				}
				else if ( input.Down( InputButton.Reload ) )
				{

					DebugOverlay.Text( prop.Position, "Mass: " + prop.PhysicsGroup.Mass );

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
