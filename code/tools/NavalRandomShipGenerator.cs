using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandbox.Tools
{
	[Library( "tool_naval_randomshipgenerator", Title = "Random Ship Generator (Naval)", Description = "Create randomly generated ships", Group = "construction2" )]
	public partial class NavalRandomShipGenerator : BaseTool
	{
		public float Seed = 124182748;
		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{

				if ( Input.Pressed( InputButton.PrimaryAttack ) )
				{
					CreateShip();
				}

				if ( Input.Pressed( InputButton.SecondaryAttack ) )
				{
					
				}

			}
		}


		public void CreateShip()
		{

			Entity SpawnEnt( Vector3 entPos, Angles entRot, string entModel, string entClass )
			{
				var ent = TypeLibrary.Create<Entity>( entClass );
				ent.Position = entPos;
				ent.Rotation = Rotation.From( entRot );
				(ent as ModelEntity)?.SetModel( entModel );

				return ent;
			}

			var startPos = Owner.EyePosition;
			var dir = Owner.EyeRotation.Forward;

			var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
				.Ignore( Owner )
				.HitLayer( CollisionLayer.Debris )
				.Run();
			var SpawnPos = tr.EndPosition + new Vector3( 0, 0, 50 );

			CreateHitEffects( SpawnPos );

			SpawnEnt( SpawnPos, Owner.Rotation.Angles(), "models/rust_vehicles/rowboat/rowboat.vmdl", "Sandbox.Prop" );

		}
	}	
}
