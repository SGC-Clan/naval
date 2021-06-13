using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

public class Compass : Panel
{
	public Label North;
	public Label East;
	public Label West;
	public Label South;
	//public Label DBV1;
	//public Label DBV2;

	public Compass()
	{

		North = Add.Label( "N", "north" );
		East = Add.Label( "E", "east" );
		West = Add.Label( "W", "west" );
		South = Add.Label( "S", "south" );

		//DBV1 = Add.Label( "DEBUG", "dbv1" );
		//DBV2 = Add.Label( "DEBUG", "dbv2" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;


		var bearing = -player.EyeRot.Angles().yaw - 180;
		var pov = Math.Clamp( -(player.EyeRot.Angles().pitch + 28) / 90, -1, 1 );

		if ( Math.Round( bearing ) < 0)
		{
			bearing = bearing + 360;
		};



		var RNPosX = (Math.PI / 180) * -bearing;
		var RNPosY = (Math.PI / 180) * -bearing;

		var REPosX = (Math.PI / 180) * -(bearing + 28) + 90;
		var REPosY = (Math.PI / 180) * -(bearing + 28) + 90;

		var RWPosX = (Math.PI / 180) * -(bearing - 28) - 90;
		var RWPosY = (Math.PI / 180) * -(bearing - 28) - 90;

		var RSPosX = (Math.PI / 180) * -(bearing - 53) - 180;
		var RSPosY = (Math.PI / 180) * -(bearing - 53) - 180;



		var NPosX = Screen.Width / 2 + Math.Round( Math.Sin( RNPosX ) * 50 );
		var NPosY = Screen.Height - 70 + Math.Round( Math.Cos( RNPosY ) * 50 * pov );

		var EPosX = Screen.Width / 2 + Math.Round( Math.Sin( REPosX ) * 50 );
		var EPosY = Screen.Height - 70 + Math.Round( Math.Cos( REPosY ) * 50 * pov );

		var WPosX = Screen.Width / 2 + Math.Round( Math.Sin( RWPosX ) * 50 );
		var WPosY = Screen.Height - 70 + Math.Round( Math.Cos( RWPosY ) * 50 * pov );

		var SPosX = Screen.Width / 2 + Math.Round( Math.Sin( RSPosX ) * 50 );
		var SPosY = Screen.Height - 70 + Math.Round( Math.Cos( RSPosY ) * 50 * pov );

		North.Text = "N";
		East.Text = "E";
		West.Text = "W";
		South.Text = "S";

		//DBV1.Text = "Bearing: " + bearing + " - POV: " + pov + " - Math.PI: " + Math.PI;
		//DBV2.Text = "Deg: 360, Rad: " + (Math.PI / 180) * 360 + " - Deg: 180, Rad: " + ( Math.PI / 180 ) * 180 + " - Deg: 90, Rad: " + (Math.PI / 180) * 90 + " - Deg: 1, Rad: " +( Math.PI / 180 ) * 1;

		North.Style.Set( $"color: red; top: " + NPosY + "px; left: " + NPosX + "px;" );
		East.Style.Set( $"color: white; top: " + EPosY + "px; left: " + EPosX + "px;");
		West.Style.Set( $"color: white; top: " + WPosY + "px; left: " + WPosX + "px;" );
		South.Style.Set( $"color: white; top: " + SPosY + "px; left: " + SPosX + "px;" );


	}
}
