using Sandbox;
using Sandbox.UI;
using naval.Teams;

[Library]
public partial class NavalHud : HudEntity<RootPanel>
{
	public NavalHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/ui/NavalHud.scss" );

		RootPanel.AddChild<NameTags>();
		RootPanel.AddChild<CrosshairCanvas>();
		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
		RootPanel.AddChild<InventoryBar>();
		RootPanel.AddChild<CurrentTool>();
		RootPanel.AddChild<SpawnMenu>();
		RootPanel.AddChild<VersionString>();
		RootPanel.AddChild<StatusBar>();
		RootPanel.AddChild<Compass>();
	}
}
