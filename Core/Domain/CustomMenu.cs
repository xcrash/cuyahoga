using System;
using System.Collections;

namespace Cuyahoga.Core.Domain
{
	/// <summary>
	/// The Menu class serves as a container for links to Nodes that need to be displayed outside 
	/// the regular Node hierarchy.
	/// </summary>
	[Serializable]
	public class CustomMenu
	{
		private int _id;
		private string _name;
		private string _placeholder;
		private Node _rootNode;
		private IList _nodes;
		private DateTime _updateTimestamp;

		/// <summary>
		/// Property Id (int)
		/// </summary>
		public int Id
		{
			get { return this._id; }
			set { this._id = value; }
		}

		/// <summary>
		/// Property Name (string)
		/// </summary>
		public string Name
		{
			get { return this._name; }
			set { this._name = value; }
		}

		/// <summary>
		/// Property Placeholder (string)
		/// </summary>
		public string Placeholder
		{
			get { return this._placeholder; }
			set { this._placeholder = value; }
		}

		/// <summary>
		/// Property RootNode (Node)
		/// </summary>
		public Node RootNode
		{
			get { return this._rootNode; }
			set { this._rootNode = value; }
		}

		/// <summary>
		/// Property Nodes (IList)
		/// </summary>
		public IList Nodes
		{
			get { return this._nodes; }
			set { this._nodes = value; }
		}

		/// <summary>
		/// Property UpdateTimestamp (DateTime)
		/// </summary>
		public DateTime UpdateTimestamp
		{
			get { return this._updateTimestamp; }
			set { this._updateTimestamp = value; }
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public CustomMenu()
		{
			this._id = -1;
			this._nodes = new ArrayList();
		}
	}
}