using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Cuyahoga.Core.Service;

namespace Cuyahoga.Core
{
	public class CuyahogaCoreContainer : WindsorContainer
	{
		/// <summary>
		/// Constructor. The configuration is read from the web.config.
		/// </summary>
		public CuyahogaCoreContainer() : base(new XmlInterpreter())
		{
			Initialize();
		}

		private void Initialize()
		{
			RegisterFacilities();
			RegisterServices();
			ConfigureLegacySessionFactory();
		}

		protected virtual void RegisterFacilities()
		{
		}

		protected virtual void RegisterServices()
		{
			// The core services are registrated via services.config

			// Utility services
			AddComponent("core.sessionfactoryhelper", typeof (SessionFactoryHelper));

			// Legacy
			AddComponent("corerepositoryadapter", typeof (CoreRepositoryAdapter));
		}

		protected virtual void ConfigureLegacySessionFactory()
		{
			var sessionFactoryHelper = Resolve<SessionFactoryHelper>();
			sessionFactoryHelper.ConfigureLegacySessionFactory();
		}
	}
}