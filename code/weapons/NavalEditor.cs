using Sandbox;
using Sandbox.Tools;

[Spawnable]
[Library("weapon_navaleditor", Title = "Contraption Editor")]
partial class NavalEditor : Carriable
{
	private TimeSince timeSinceUse;

	private int UseDistance = 1500;

	[Net, Local, Predicted]
	private bool RenderPreviewArea { get; set; } = false;

	protected virtual float UseCooldown => 0.5f;
	public override string ViewModelPath => "models/sernikb/serns_blueprint_tool_v.vmdl_c";

	public override void Spawn()
	{
		base.Spawn();

		SetModel("models/sernikb/serns_blueprint_tool.vmdl_c"); 
	}

	public override void Simulate( Client client )
	{
		if ( Owner is not Player owner ) return;

		if ( !IsServer )
			return;

		if ( timeSinceUse < UseCooldown )
			return;

		using ( Prediction.Off() )
		{
			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				timeSinceUse = 0;

				var MaxTraceDistance = UseDistance;
				var startPos = Owner.EyePosition;
				var dir = Owner.EyeRotation.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.UseHitboxes()
					.Size( 2 )
					.Ignore( Owner )
					.Run();

				//if ( !tr.Hit || !tr.Entity.IsValid() )
					//return;

				var ent = TypeLibrary.Create<Entity>( "nvl_contraption_base" );

				ent.Position = tr.EndPosition + new Vector3(0,0,40);
				ent.Rotation = Rotation.From( new Angles( 0, Owner.EyeRotation.Angles().yaw, 0 ) );

			}
		}
	}

	public override void ActiveStart(Entity ent)
	{
		base.ActiveStart(ent);

		if ( IsServer )
		{
			RenderPreviewArea = true;
		}

	}

	public override void ActiveEnd(Entity ent, bool dropped)
	{
		base.ActiveEnd(ent, dropped);

		if ( IsServer )
		{
			RenderPreviewArea = false;
		}

	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

	}

	public override void OnCarryDrop(Entity dropper)
	{
	}

	[Event.Frame]
	public void OnFrame()
	{
		if ( RenderPreviewArea )
		{
			//if (!IsActiveChild()) return; this function got removed ?

			var tr = Trace.Ray( Owner.EyePosition, Owner.EyePosition + Owner.EyeRotation.Forward * UseDistance )
				.UseHitboxes()
				.Size( 2 )
				.Ignore( Owner )
				.Run();

			//if ( !tr.Hit )
			//return;

			DebugOverlay.Box( tr.EndPosition + new Vector3( 50, -50, 10 ), tr.EndPosition + new Vector3( -50, 50, 10 ) );
			DebugOverlay.Box( tr.EndPosition + new Vector3( 50, -50, 40 ), tr.EndPosition + new Vector3( -50, 50, 40 ) );
		}
	}


	public override void SimulateAnimator(PawnAnimator anim)
	{
		anim.SetAnimParameter( "holdtype", 4);
		anim.SetAnimParameter( "aimat_weight", 1.0f);
		anim.SetAnimParameter( "holdtype_handedness", 0);
	}
}
