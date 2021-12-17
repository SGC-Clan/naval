using Sandbox;
using Sandbox.Tools;

[Library("weapon_navaleditor", Title = "Contraption Editor")]
partial class NavalEditor : Carriable
{
	private TimeSince timeSinceUse;

	private int UseDistance = 1500;

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
			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				timeSinceUse = 0;

				var MaxTraceDistance = UseDistance;
				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.UseHitboxes()
					.Size( 2 )
					.Ignore( Owner )
					.Run();

				//if ( !tr.Hit || !tr.Entity.IsValid() )
					//return;

				var ent = Library.Create<Entity>( "nvl_contraption_base" );

				ent.Position = tr.EndPos + new Vector3(0,0,40);
				ent.Rotation = Rotation.From( new Angles( 0, Owner.EyeRot.Angles().yaw, 0 ) );

			}
		}
	}

	public override void ActiveStart(Entity ent)
	{
		base.ActiveStart(ent);

	}

	public override void ActiveEnd(Entity ent, bool dropped)
	{
		base.ActiveEnd(ent, dropped);

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
		if (!IsActiveChild()) return;

		var tr = Trace.Ray( Owner.EyePos, Owner.EyePos + Owner.EyeRot.Forward * UseDistance )
			.UseHitboxes()
			.Size( 2 )
			.Ignore( Owner )
			.Run();

		//if ( !tr.Hit )
			//return;

		DebugOverlay.Box( tr.EndPos + new Vector3( 50, -50, 10 ) , tr.EndPos + new Vector3( -50, 50, 10 ) );
		DebugOverlay.Box( tr.EndPos + new Vector3( 50, -50, 40 ), tr.EndPos + new Vector3( -50, 50, 40 ) );
	}

	public override void SimulateAnimator(PawnAnimator anim)
	{
		anim.SetParam("holdtype", 4);
		anim.SetParam("aimat_weight", 1.0f);
		anim.SetParam("holdtype_handedness", 0);
	}
}
