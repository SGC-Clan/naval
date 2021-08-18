using Sandbox;

[Library]
public class CarController : PawnController
{
	public override void FrameSimulate()
	{
		base.FrameSimulate();

		Simulate();
	}

	public override void Simulate()
	{
		var player = Pawn as NavalPlayer;
		if ( !player.IsValid() ) return;

		var car = player.Vehicle as CarEntity;
		if ( !car.IsValid() ) return;

		car.Simulate( Client );

		if ( player.Vehicle == null )
		{
			Position = car.Position + car.Rotation.Up * 100;
			Velocity += car.Rotation.Right * 200;
			return;
		}

		float heightOffset = (20 * car.Scale);

		Position = car.Position + car.Rotation.Up * heightOffset;
		Rotation = car.Rotation;
		EyeRot = Input.Rotation;
		EyePosLocal = Vector3.Up * (64 - heightOffset);
		Velocity = car.Velocity;

		SetTag( "noclip" );
		SetTag( "sitting" );
	}
}
