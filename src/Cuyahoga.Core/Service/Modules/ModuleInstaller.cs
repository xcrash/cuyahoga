using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cuyahoga.Core.Util;
using log4net;

namespace Cuyahoga.Core.Service.Modules
{
	/// <summary>
	/// 	The ModuleInstaller class is responsible for installing, upgrading and uninstalling
	/// 	database tables and records for module.
	/// </summary>
	public class ModuleInstaller
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly string _installRootDirectory;
		private readonly Assembly _assembly;
		private Version _currentVersionInDatabase;
		private ICustomModuleInstaller customInstaller;

		/// <summary>
		/// 	The current version of the module/assembly in the database.
		/// </summary>
		public Version CurrentVersionInDatabase
		{
			get { return this._currentVersionInDatabase; }
		}

		/// <summary>
		/// 	The version of the assembly that is to be installed or upgraded.
		/// </summary>
		public Version NewAssemblyVersion
		{
			get { return this._assembly.GetName().Version; }
		}

		/// <summary>
		/// 	Indicates if a module or assembly can be installed from the given location.
		/// </summary>
		public bool CanInstall
		{
			get { return customInstaller.CanInstall; }
		}

		/// <summary>
		/// 	Indicates if a module or assembly can be upgraded from the given location.
		/// </summary>
		public bool CanUpgrade
		{
			get { return customInstaller.CanUpgrade; }
		}

		/// <summary>
		/// 	Indicates if a module or assembly can be uninstalled from the given location.
		/// </summary>
		public bool CanUninstall
		{
			get { return customInstaller.CanUninstall; }
		}

		/// <summary>
		/// 	Default constructor.
		/// </summary>
		/// <param name = "installRootDirectory">The physical path of the directory where the install
		/// 	scripts are located. This is the root install directory without 'Database/DatabaseType'.</param>
		/// <param name = "assembly">The (optional) assembly that has to be upgraded or uninstalled.</param>
		public ModuleInstaller(string installRootDirectory, Assembly assembly)
		{
			this._installRootDirectory = installRootDirectory;
			this._assembly = assembly;

			if (this._assembly != null)
			{
				CheckCurrentVersionInDatabase();
			}
			LoadCustomInstallers();
		}

		/// <summary>
		/// 	Install the database part of a Cuyaghoga component.
		/// </summary>
		public void Install()
		{
			if (CanInstall)
			{
				customInstaller.Install();
			}
			else
			{
				throw new InvalidOperationException("Can't install assembly from: " + this._installRootDirectory);
			}
		}

		/// <summary>
		/// 	Upgrade the database part of a Cuyahoga component to higher version.
		/// </summary>
		public void Upgrade()
		{
			if (CanUpgrade)
			{
				customInstaller.Upgrade();
			}
			else
			{
				throw new InvalidOperationException("Can't upgrade assembly from: " + this._installRootDirectory);
			}
		}

		/// <summary>
		/// 	Uninstall the database part of a Cuyaghoga component.
		/// </summary>
		public void Uninstall()
		{
			if (CanUninstall)
			{
				customInstaller.Uninstall();
			}
			else
			{
				throw new InvalidOperationException("Can't uninstall assembly from: " + this._installRootDirectory);
			}
		}

		private void CheckCurrentVersionInDatabase()
		{
			if (this._assembly != null)
			{
				this._currentVersionInDatabase = DatabaseUtil.GetAssemblyVersion(this._assembly.GetName().Name);
			}
		}

		private void LoadCustomInstallers()
		{
			var mainInstaller = new SqlFileModuleInstaller(_installRootDirectory, _assembly);
			if (this._assembly != null)
			{
				var customInstallers =
					this._assembly.GetTypes()
						.Where(t => typeof (ICustomModuleInstaller).IsAssignableFrom(t) &&
						            t.IsPublic &&
						            !t.IsInterface &&
						            !t.IsAbstract &&
						            t.GetConstructor(new Type[0]) != null &&
						            !t.Equals(GetType()))
						.Select(t => Activator.CreateInstance(t))
						.Cast<ICustomModuleInstaller>();

				this.customInstaller = new CombineCustomModuleInstaller(mainInstaller, customInstallers);
			}
			else
			{
				this.customInstaller = mainInstaller;
			}
		}

		private class CombineCustomModuleInstaller : ICustomModuleInstaller
		{
			private readonly IEnumerable<ICustomModuleInstaller> childs;

			public CombineCustomModuleInstaller(ICustomModuleInstaller customModuleInstaller,
			                                    IEnumerable<ICustomModuleInstaller> customModuleInstallers)
			{
				this.childs = new[] {customModuleInstaller}.Union(customModuleInstallers);
			}

			public bool CanInstall
			{
				get { return childs.Any(c => c.CanInstall); }
			}

			public bool CanUpgrade
			{
				get { return childs.Any(c => c.CanUpgrade); }
			}

			public bool CanUninstall
			{
				get { return childs.Any(c => c.CanUninstall); }
			}

			public void Install()
			{
				foreach (var customModuleInstaller in childs)
				{
					customModuleInstaller.Install();
				}
			}

			public void Upgrade()
			{
				foreach (var customModuleInstaller in childs)
				{
					customModuleInstaller.Upgrade();
				}
			}

			public void Uninstall()
			{
				foreach (var customModuleInstaller in childs.Reverse())
				{
					customModuleInstaller.Uninstall();
				}
			}
		}
	}
}