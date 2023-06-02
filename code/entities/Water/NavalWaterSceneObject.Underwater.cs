using NativeEngine;
using System;
using Sandbox;

namespace Sandbox
{
	public partial class NavalWaterSceneObject : SceneCustomObject
    {
        protected VertexBuffer vbUnderwaterStencil;

        /// <summary>
        /// Makes a vertex buffer cube for fog with the given bounds
		/// </summary>
        public VertexBuffer MakeCuboid( Vector3 mins, Vector3 maxs )
		{
            var vb = new VertexBuffer();
			vb.Init( true );
            Vector3 center = ( (mins + maxs) / 2 ) - Transform.Position;
            Vector3 size = maxs - mins;
            
            Rotation rot = new Rotation();

			var f = rot.Forward * -size.x * 0.5f;
			var l = rot.Left * -size.y * 0.5f;
			var u = rot.Up * -size.z * 0.5f;

			vb.AddQuad( new Ray( center + f, f.Normal ), l, u );
			vb.AddQuad( new Ray( center - f, -f.Normal ), l, -u );

			vb.AddQuad( new Ray( center + l, l.Normal ), -f, u );
			vb.AddQuad( new Ray( center - l, -l.Normal ), f, u );

			vb.AddQuad( new Ray( center + u, u.Normal ), f, l );
			//VertexBufferExtenison.AddQuad( vb, new Ray( center - u, -u.Normal ), f, -l );

            return vb;
        }
    }
}
