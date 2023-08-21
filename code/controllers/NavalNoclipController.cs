
namespace Sandbox
{
	[Library]
	public class NavalNoclipController : BasePlayerController
	{
		public override void Simulate()
		{
			var pl = Pawn as NavalPlayer;

			var fwd = pl.InputDirection.x.Clamp( -1f, 1f );
			var left = pl.InputDirection.y.Clamp( -1f, 1f );
			var rotation = pl.ViewAngles.ToRotation();

			var vel = (rotation.Forward * fwd) + (rotation.Left * left);

			vel = vel.Normal * 3000;

			if ( Input.Down( "jump" ) )
				vel += Vector3.Up * 10000;

			if ( Input.Down( "run" ) )
				vel *= 35.0f;

			if ( Input.Down( "duck" ) )
				vel *= 0.2f;

			Velocity += vel * Time.Delta;

			if ( Velocity.LengthSquared > 0.01f )
			{
				Position += Velocity * Time.Delta;
			}

			Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );

			EyeRotation = rotation;
			WishVelocity = Velocity;
			GroundEntity = null;
			BaseVelocity = Vector3.Zero;

			SetTag( "noclip" );
		}

	}
}
