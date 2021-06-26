using Sandbox.Rcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	[Library]
	public class SitController : BasePlayerController
	{
		public override void Simulate()
		{
			if ( GroundEntity == null )
				return;

			if ( (GroundEntity as NavalShipController).SeatUser == null )
				return;

			var Seat = (GroundEntity as NavalShipController).SeatUser;

			Position = Seat.Position;
			Rotation = Seat.Rotation;

			SetTag( "Sitting" );
		}

	}
}
