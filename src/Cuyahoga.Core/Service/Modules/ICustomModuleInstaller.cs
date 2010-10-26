namespace Cuyahoga.Core.Service.Modules
{
	public interface ICustomModuleInstaller
	{
		/// <summary>
		/// 	Indicates if a module or assembly can be installed from the given location.
		/// </summary>
		bool CanInstall { get; }

		/// <summary>
		/// 	Indicates if a module or assembly can be upgraded from the given location.
		/// </summary>
		bool CanUpgrade { get; }

		/// <summary>
		/// 	Indicates if a module or assembly can be uninstalled from the given location.
		/// </summary>
		bool CanUninstall { get; }

		/// <summary>
		/// 	Install the database part of a Cuyaghoga component.
		/// </summary>
		void Install();

		/// <summary>
		/// 	Upgrade the database part of a Cuyahoga component to higher version.
		/// </summary>
		void Upgrade();

		/// <summary>
		/// 	Uninstall the database part of a Cuyaghoga component.
		/// </summary>
		void Uninstall();
	}
}