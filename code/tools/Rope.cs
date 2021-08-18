namespace Sandbox.Tools
{
	[Library( "tool_rop", Title = "Rope", Description = "Join two things together with a rope", Group = "construction" )]
	public partial class RopeTool : BaseTool
	{
		private PhysicsBody targetBody;
		private int targetBone;
		private Vector3 targetPosition;

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if ( !Input.Pressed( InputButton.Attack1 ) )
					return;

				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit )
					return;

				if ( !tr.Body.IsValid() )
					return;

				if ( !tr.Entity.IsValid() )
					return;

				if ( tr.Entity is not ModelEntity )
					return;

				if ( !targetBody.IsValid() )
				{
					targetBody = tr.Body;
					targetBone = tr.Bone;
					targetPosition = tr.Body.Transform.PointToLocal( tr.EndPos );

					CreateHitEffects( tr.EndPos );

					return;
				}

				if ( targetBody == tr.Body )
					return;

				var rope = Particles.Create( "particles/rope.vpcf" );

				if ( targetBody.Entity.IsWorld )
				{
					rope.SetPosition( 0, targetPosition );
				}
				else
				{
					rope.SetEntityBone( 0, targetBody.Entity, targetBone, new Transform( targetPosition ) );
				}

				if ( tr.Entity.IsWorld )
				{
					rope.SetPosition( 1, tr.Body.Transform.PointToLocal( tr.EndPos ) );
				}
				else
				{
					rope.SetEntityBone( 1, tr.Entity, tr.Bone, new Transform( tr.Body.Transform.PointToLocal( tr.EndPos ) ) );
				}

				var spring = PhysicsJoint.Spring
					.From( targetBody, targetPosition )
					.To( tr.Body, tr.Body.Transform.PointToLocal( tr.EndPos ) )
					.WithFrequency( 5.0f )
					.WithDampingRatio( 0.7f )
					.WithReferenceMass( targetBody.Mass )
					.WithMinRestLength( 0 )
					.WithMaxRestLength( 200 )
					.WithCollisionsEnabled()
					.Create();

				spring.EnableAngularConstraint = false;
				spring.OnBreak( () =>
				{
					rope?.Destroy( true );
					spring.Remove();
				} );

				CreateHitEffects( tr.EndPos );

				Reset();
			}
		}

		private void Reset()
		{
			targetBody = null;
			targetBone = -1;
			targetPosition = default;
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
