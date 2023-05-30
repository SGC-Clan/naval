using Sandbox;
using Sandbox.Utility;

[Spawnable]
[Library( "noise_test", Title = "Noise Test" )]
public partial class NoiseTest : Prop
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/balloonregular01.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	[Event.Client.Frame]
	public void OnFrame()
	{
		var pos = Position;
		var right = Rotation.Right * 4;
		var forward = Rotation.Forward * 4;
		var up = Rotation.Up * 50;
		var offset = Time.Now * 20.0f;
		var offsetz = Time.Now;

		var mode = (int)((Time.Now * 0.3f) % 3);

		switch ( mode )
		{
			case 0:
				{
					DebugOverlay.Text( "Perlin", pos );
					break;
				}

			case 1:
				{
					DebugOverlay.Text( "Fbm", pos );
					break;
				}

			case 2:
				{
					DebugOverlay.Text( "Simplex", pos );
					break;
				}
		}


		var size = 100;

		pos -= right * size * 0.5f;
		pos -= forward * size * 0.5f;

		for ( float x = 0; x < size; x++ )
			for ( float y = 0; y < size; y++ )
			{
				float val = 0;

				switch ( mode )
				{
					case 0:
						{
							val = Noise.Perlin( x + offset, y, offsetz );
							break;
						}
					case 1:
						{
							val = Noise.Fbm( 2, x + offset, y, offsetz );
							break;
						}
					case 2:
						{
							val = Noise.Simplex( x + offset, y, offsetz );
							break;
						}
				}

				var start = pos + x * right + y * forward;
				DebugOverlay.Line( start, start + up * val, Color.Lerp( Color.Red, Color.Green, val ) );
			}
	}
}
