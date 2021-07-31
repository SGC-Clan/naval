using Sandbox;

[Library]
public class VehicleController : PawnController
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

		var vehicle = player.Vehicle as Prop;
		if ( !vehicle.IsValid() ) return;

		vehicle.Simulate( Client );

		if ( player.Vehicle == null )
		{
			Position = vehicle.Position + vehicle.Rotation.Up * 100;
			Velocity += vehicle.Rotation.Right * 200;
			return;
		}

		float heightOffset = (20 * vehicle.Scale);

		Position = vehicle.Position + vehicle.Rotation.Up * heightOffset;
		Rotation = vehicle.Rotation;
		EyeRot = Input.Rotation;
		EyePosLocal = Vector3.Up * (64 - heightOffset);
		Velocity = vehicle.Velocity;

		SetTag( "noclip" );
		SetTag( "sitting" );
	}
}
