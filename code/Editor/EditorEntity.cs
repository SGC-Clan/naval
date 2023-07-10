using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class EditorEntity : ModelEntity
	{

		[Net] public Player User { get; set; }
		public bool AmIUser => User != null && User.Client != null && User.Client == Game.LocalClient;

		private EditorCamera EditorCam;

		public EditorEntity()
		{
			
		}

		public override void Spawn()
		{
			Model = Model.Load( "models/editor/axis_helper.vmdl" );
		}

		public override void ClientSpawn() 
		{
			base.ClientSpawn();

			SetupEditorCamera( User );

		}

		public void SetupEditorCamera( Player user ) 
		{
			if ( !User.IsValid ) return;

			var camera = user.Components.Get<EditorCamera>( true );

			if ( camera == null )
			{
				camera = new EditorCamera();
				camera.EditorEnt = this;
				user.Components.Add( camera );
				return;
			}

			camera.Enabled = true;
		}

		public void DisableEditorCamera( Player user )
		{ 
			if ( !User.IsValid ) return;
			var camera = user.Components.Get<EditorCamera>( true );
			if ( camera != null )
			{
				camera.EditorEnt = null;
				camera.Enabled = false;
			}

		}

		[Event.Client.Frame]
		public void OnFrame()
		{

			if ( AmIUser ) 
			{
				//EditorCam.Update();
			}

		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( !User.IsValid ) return;
			DisableEditorCamera( User );

		}


	}
}
