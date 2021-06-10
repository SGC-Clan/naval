using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class VersionString : Panel
{
	public Label Label;

	public VersionString()
	{
		Label = Add.Label( "Naval Alpha 0.x.x ", "value" );
	}

}
