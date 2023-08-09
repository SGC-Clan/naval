using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.Utility;
using static Sandbox.Game;

namespace Sandbox
{

	public class EditorCamera : Sandbox.EntityComponent
	{
		public EditorEntity EditorEnt;

		Angles LookAngles;
		Vector3 MoveInput;

		Vector3 Position;
		Rotation Rotation;

		Vector3 TargetPos;
		Rotation TargetRot;

		bool PivotEnabled;
		float PivotDist;
		Vector3 PivotOffset;

		float MoveSpeed;
		float BaseMoveSpeed = 300.0f;
		float FovOverride = 0;

		float LerpMode = 0;

		private static RootPanel RootPanel = null;
		private static NavalEditorOverlay overlay = null;

		protected override void OnActivate()
		{

			TargetPos = Camera.Position;
			TargetRot = Camera.Rotation;

			Position = TargetPos;
			Rotation = TargetRot;
			PivotDist = 100;
			LookAngles = Rotation.Angles();
			FovOverride = 80;

			PivotEnabled = true;

			//
			// Set the devcamera class on the HUD. It's up to the HUD what it does with it.
			//
			//Game.RootPanel?.SetClass( "NavalEditorOverlay", true );

			// Only create if we're using the dev cam
			if ( RootPanel == null )
			{
				RootPanel = new();
				overlay = RootPanel.AddChild<NavalEditorOverlay>();
				overlay.Show();
			}

			//overlay?.Activated();
		}



		[GameEvent.Client.BuildInput]
		internal void BuildInput()
		{

			MoveInput = Input.AnalogMove;
			
			MoveSpeed = 1;

			if ( Input.Down( "attack2" ) )
			{
				LookAngles += Input.AnalogLook;
			}

			Input.ClearActions();
			Input.AnalogMove = default;
			Input.StopProcessing = true;
		}

		void PivotMove()
		{
			PivotDist -= MoveInput.x * RealTime.Delta * 100 * (PivotDist / 50);
			PivotDist = PivotDist.Clamp( 1, 1000 );

			TargetRot = Rotation.From( LookAngles );
			Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );

			TargetPos = EditorEnt.Position + Rotation.Forward * -PivotDist;
			Position = TargetPos;
		}

		void FreeMove()
		{
			var mv = MoveInput.Normal * BaseMoveSpeed * RealTime.Delta * Rotation * MoveSpeed;

			TargetRot = Rotation.From( LookAngles );
			TargetPos += mv;

			Position = Vector3.Lerp( Position, TargetPos, 10 * RealTime.Delta * (1 - LerpMode) );
			Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );
		}

		[GameEvent.Client.PostCamera]
		public void Update()
		{

			if ( this.Enabled == false ) return;
			if ( EditorEnt == null ) return;
			if ( !EditorEnt.IsValid ) return;

			if ( PivotEnabled )
			{
				PivotMove();
			}
			else
			{
				FreeMove();
			}

			Camera.Rotation = Rotation;
			Camera.Position = Position;
			Camera.FirstPersonViewer = null;

		}

	}
}
