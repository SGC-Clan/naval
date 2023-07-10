using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	internal partial class NavalEditorOverlay
	{
		public NavalEditorOverlay()
		{
			Hide();
		}

		public void Show()
		{
			SetClass( "hidden", false );
		}

		public void Hide()
		{
			SetClass( "hidden", true );
		}

		internal static Action<MousePanelEvent> OnMouseClicked;

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			if ( e.Target != this )
				return;

			OnMouseClicked?.Invoke( e );
		}

		internal static Action<MousePanelEvent> OnMouseMoved;

		protected override void OnMouseMove( MousePanelEvent e )
		{
			base.OnMouseMove( e );

			if ( e.Target != this )
				return;

			OnMouseMoved?.Invoke( e );
		}
	}
}
