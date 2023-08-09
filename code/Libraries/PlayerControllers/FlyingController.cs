
namespace Sandbox
{
	[Library]
	[System.Obsolete( "This will be deleted soon. It's advised to stop using it if you can." )]
	public class FlyingController : BasePlayerController
	{
		public float Bounce { get; set; } = 0.25f;
		public float Size { get; set; } = 20.0f;

		public override void Simulate()
		{
			var pl = Pawn as Player;

			Rotation = pl.ViewAngles.ToRotation();

			var fwd = pl.InputDirection.x.Clamp( -1f, 1f );
			var left = pl.InputDirection.y.Clamp( -1f, 1f );

			var vel = (Rotation.Forward * fwd) + (Rotation.Left * left);

			vel = vel.Normal * 2000;

			if ( Input.Down( "run" ) )
				vel *= 5.0f;

			if ( Input.Down( "duck" ) )
				vel *= 0.2f;

			Velocity += vel * Time.Delta;

			if ( Velocity.LengthSquared > 0.01f )
			{
				Move( Velocity * Time.Delta );
			}

			Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );

			if ( Input.Down( "jump" ) )
				Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );
		}

		public void Move( Vector3 delta, int a = 0 )
		{
			if ( a > 1 )
				return;

			var len = delta.Length;
			var targetPos = Position + delta;
			var tr = Trace.Ray( Position, targetPos ).StaticOnly().Size( Size ).Run();

			if ( tr.StartedSolid )
			{
				Position = targetPos;
			}
			else if ( tr.Hit )
			{
				Position = tr.EndPosition + tr.Normal;

				// subtract the normal from our velocity
				Velocity = Velocity.SubtractDirection( tr.Normal * (1.0f + Bounce) );

				delta = Velocity.Normal * delta.Length * (1.0f - tr.Fraction);

				Move( delta, ++a );
			}
			else
			{
				Position = tr.EndPosition;
			}
		}
	}
}
