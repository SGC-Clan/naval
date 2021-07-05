using Sandbox.Rcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	[Library]
	public class SitController : WalkController
	{
		private PhysicsBody SeatBody;
		private Vector3 GroundLocalPos;

		public override void Simulate()
		{
			EyePosLocal = Vector3.Up * (EyeHeight * Pawn.Scale);
			UpdateBBox();

			EyePosLocal += TraceOffset;
			EyeRot = Input.Rotation;

			SeatBody = (Pawn as NavalPlayer).SitEntity;

			if ( SeatBody == null || Input.Down( InputButton.Use ) ) //when seat is missing or player pressed E button
			{
				ExitSeat();
			}
			if ( SeatBody != null )
			{
				//disarm player that seats
				Pawn.Inventory.SetActiveSlot( -1, true );

				Position = SeatBody.Transform.PointToWorld( GroundLocalPos ) - SeatBody.Velocity * Time.Delta;
				Velocity = SeatBody.Velocity;
			}
		}

		public void ExitSeat() 
		{
			ConsoleSystem.Run( "naval_sit" );
		}

	}
}
