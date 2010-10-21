using Cuyahoga.Core;

namespace Cuyahoga.Web.Components
{
	/// <summary>
	/// The CuyahogaContainer serves as the IoC container for Cuyahoga.
	/// </summary>
	public class CuyahogaContainer : CuyahogaCoreContainer
	{
		/// <summary>
		/// Constructor. The configuration is read from the web.config.
		/// </summary>
		public CuyahogaContainer() : base()
		{
		}

		protected override void RegisterServices()
		{
			AddComponent("web.moduleloader", typeof (ModuleLoader));
			base.RegisterServices();
		}
	}
}