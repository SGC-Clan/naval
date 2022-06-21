using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandbox.Tools
{
	[Library( "tool_naval_duplicator", Title = "Duplicator (Naval)", Description = "Copy or Paste contraptions", Group = "construction2" )]
	public partial class NavalDuplicator : BaseTool
	{
		public string TempSavedEntClass = null;
		public string TempSavedEntModel = null;
		public Angles TempSavedEntRotation = new Angles(0,0,0);
		public float TempSavedEntScale = 1f;
		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{

				if ( Input.Pressed( InputButton.PrimaryAttack ) )
				{
					Paste();
				}

				if ( Input.Pressed( InputButton.SecondaryAttack ) )
				{
					Copy();
				}

			}
		}

		public void Copy()
		{
			var startPos = Owner.EyePosition;
			var dir = Owner.EyeRotation.Forward;

			var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
				.Ignore( Owner )
				.HitLayer( CollisionLayer.Debris )
				.Run();

			if ( !tr.Hit || !tr.Entity.IsValid() )
				return;

			if ( tr.Entity is Player )
				return;

			CreateHitEffects( tr.EndPosition );

			if ( tr.Entity.IsWorld )
				return;

			TempSavedEntClass = tr.Entity.ClassName;
			Log.Info( TempSavedEntClass );

			TempSavedEntModel = (tr.Entity as ModelEntity)?.GetModelName();
			Log.Info( TempSavedEntModel );

			TempSavedEntRotation = tr.Entity.Rotation.Angles();

			TempSavedEntScale = tr.Entity.Scale;

			var particle = Particles.Create( "particles/physgun_freeze.vpcf" );
			particle.SetPosition( 0, tr.Entity.Position );
		}

		public void Paste() 
		{

			var startPos = Owner.EyePosition;
			var dir = Owner.EyeRotation.Forward;

			var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
				.Ignore( Owner )
				.HitLayer( CollisionLayer.Debris )
				.Run();

			if ( TempSavedEntClass == null )
				return;
			if ( TempSavedEntModel == null )
				return;

			CreateHitEffects( tr.EndPosition );

			var ent = TypeLibrary.Create<Entity>( TempSavedEntClass );
			ent.Position = tr.EndPosition;
			ent.Rotation = Rotation.From( TempSavedEntRotation );
			ent.Scale = TempSavedEntScale;
			( ent as ModelEntity )?.SetModel( TempSavedEntModel );
		}
	}	
}
