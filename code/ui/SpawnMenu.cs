using Sandbox;
using Sandbox.Tools;
using Sandbox.UI;
using Sandbox.UI.Construct;


[Library]
public partial class SpawnMenu : Panel
{
	public static SpawnMenu Instance;
	readonly Panel toollist;

	public SpawnMenu()
	{
		Instance = this;

		StyleSheet.Load( "/ui/SpawnMenu.scss" );

		var left = Add.Panel( "left" );
		{
			var tabs = left.AddChild<ButtonGroup>();
			tabs.AddClass( "tabs" );

			var body = left.Add.Panel( "body" );

			{
				var props = body.AddChild<SpawnList>();
				tabs.SelectedButton = tabs.AddButtonActive( "Props", ( b ) => props.SetClass( "active", b ) );

				var ents = body.AddChild<EntityList>();
				tabs.AddButtonActive( "Entities", ( b ) => ents.SetClass( "active", b ) );

				var models = body.AddChild<CloudModelList>();
				tabs.AddButtonActive( "s&works", ( b ) => models.SetClass( "active", b ) );
			}
		}

		var right = Add.Panel( "right" );
		{
			var tabs = right.Add.Panel( "tabs" );
			{
				tabs.Add.Button( "Tools" ).AddClass( "active" );
				tabs.Add.Button( "Utility" );
			}
			var body = right.Add.Panel( "body" );
			{
				toollist = body.Add.Panel( "toollist" );
				{
					RebuildToolList();
				}
				body.Add.Panel( "inspector" );
			}
		}

	}

	void RebuildToolList()
	{
		toollist.DeleteChildren( true );

		foreach ( var entry in TypeLibrary.GetDescriptions<BaseTool>() )
		{
			if ( entry.Title == "BaseTool" )
				continue;

			var button = toollist.Add.Button( entry.Title );
			button.SetClass( "active", entry.ClassName == ConsoleSystem.GetValue( "tool_current" ) );

			button.AddEventListener( "onclick", () =>
			{
				ConsoleSystem.Run( "tool_current", entry.ClassName );
				ConsoleSystem.Run( "inventory_current", "weapon_tool" );

				foreach ( var child in toollist.Children )
					child.SetClass( "active", child == button );
			} );
		}
	}

	public override void Tick()
	{
		base.Tick();

		Parent.SetClass( "spawnmenuopen", Input.Down( InputButton.Menu ) );

		UpdateActiveTool();
	}

	void UpdateActiveTool()
	{
		var toolCurrent = ConsoleSystem.GetValue( "tool_current" );
		var tool = string.IsNullOrWhiteSpace( toolCurrent ) ? null : TypeLibrary.GetDescription<BaseTool>( toolCurrent );

		foreach ( var child in toollist.Children )
		{
			if ( child is Button button )
			{
				child.SetClass( "active", tool != null && button.Text == tool.Title );
			}
		}
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		RebuildToolList();
	}
}
