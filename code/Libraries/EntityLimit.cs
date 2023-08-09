using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Sandbox
{
	/// <summary>
	/// A class that limits the amount of entities.
	/// </summary>
	public class EntityLimit
	{
		/// <summary>
		/// Maximum entities in this list before we start deleting
		/// </summary>
		public virtual int MaxTotal { get; set; } = 2;

		/// <summary>
		/// List of entities currently in this shit
		/// </summary>
		public List<Entity> List { get; protected set; } = new List<Entity>();

		/// <summary>
		/// Watch an entity, contribute to the count and delete when its their turn
		/// </summary>
		public void Watch( ModelEntity ent )
		{
			List.Add( ent );

			Maintain();
		}

		/// <summary>
		/// Maintain the list, delete entities if they need deleting
		/// </summary>
		void Maintain()
		{
			// Remove any that have been deleted already
			List.RemoveAll( x => !x.IsValid() );

			while ( List.Count > MaxTotal )
			{
				Retire( List.First() );
			}
		}

		/// <summary>
		/// Delete this entity and remove it from the list
		/// </summary>
		public void Retire( Entity ent )
		{
			List.RemoveAll( x => x == ent );

			ent.Delete();
		}
	}
}