using Sandbox;
using Sandbox.Tools;

[Library("weapon_navaleditor", Title = "Contraption Editor")]
partial class NavalEditor : Carriable
{

	public override string ViewModelPath => "models/sernikb/serns_blueprint_tool.vmdl_c";

	public override void Spawn()
	{
		base.Spawn();

		SetModel("models/sernikb/serns_blueprint_tool.vmdl_c"); 
	}

	public override void Simulate(Client owner)
	{
	
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
	}

	public override void SimulateAnimator(PawnAnimator anim)
	{
		anim.SetParam("holdtype", 1);
		anim.SetParam("aimat_weight", 1.0f);
		anim.SetParam("holdtype_handedness", 1);
	}
}
