using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Cuyahoga.Core.Domain;
using Cuyahoga.Core.Util;
using log4net;

namespace Cuyahoga.Core.Service.Modules
{
	public class SqlFileModuleInstaller : ICustomModuleInstaller
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly string _databaseScriptsDirectory;
		private readonly Assembly _assembly;
		private readonly DatabaseType _databaseType;
		private string _installScriptFile;
		private string _uninstallScriptFile;
		private readonly ArrayList _upgradeScriptVersions;
		private Version _currentVersionInDatabase;

		/// <summary>
		/// 	Indicates if a module or assembly can be installed from the given location.
		/// </summary>
		public bool CanInstall
		{
			get { return CheckCanInstall(); }
		}

		/// <summary>
		/// 	Indicates if a module or assembly can be upgraded from the given location.
		/// </summary>
		public bool CanUpgrade
		{
			get { return CheckCanUpgrade(); }
		}

		/// <summary>
		/// 	Indicates if a module or assembly can be uninstalled from the given location.
		/// </summary>
		public bool CanUninstall
		{
			get { return CheckCanUninstall(); }
		}

		/// <summary>
		/// 	Default constructor.
		/// </summary>
		/// <param name = "installRootDirectory">The physical path of the directory where the install
		/// 	scripts are located. This is the root install directory without 'Database/DatabaseType'.</param>
		/// <param name = "assembly">The (optional) assembly that has to be upgraded or uninstalled.</param>
		public SqlFileModuleInstaller(string installRootDirectory, Assembly assembly)
		{
			this._assembly = assembly;
			this._databaseType = DatabaseUtil.GetCurrentDatabaseType();
			string databaseSubDirectory = Path.Combine("Database", this._databaseType.ToString().ToLower());
			this._databaseScriptsDirectory = Path.Combine(installRootDirectory, databaseSubDirectory);

			this._upgradeScriptVersions = new ArrayList();
			CheckDatabaseScripts();
			// Sort the versions in ascending order. This way it's easy to iterate through the scripts
			// when upgrading.
			this._upgradeScriptVersions.Sort();

			if (this._assembly != null)
			{
				CheckCurrentVersionInDatabase();
			}
		}

		/// <summary>
		/// 	Install the database part of a Cuyaghoga component.
		/// </summary>
		public void Install()
		{
			log.Info("Installing module with " + this._installScriptFile);
			DatabaseUtil.ExecuteSqlScript(this._installScriptFile);
		}

		/// <summary>
		/// 	Upgrade the database part of a Cuyahoga component to higher version.
		/// </summary>
		public void Upgrade()
		{
			log.Info("Upgrading " + this._assembly.GetName().Name);
			// Iterate through the sorted versions that are extracted from the upgrade script names.
			foreach (Version version in this._upgradeScriptVersions)
			{
				// Only run the script if the version is higher than the current database version
				if (version > this._currentVersionInDatabase)
				{
					string upgradeScriptPath = Path.Combine(this._databaseScriptsDirectory, version.ToString(3) + ".sql");
					log.Info("Running upgrade script " + upgradeScriptPath);
					DatabaseUtil.ExecuteSqlScript(upgradeScriptPath);
					this._currentVersionInDatabase = version;
				}
			}
		}

		/// <summary>
		/// 	Uninstall the database part of a Cuyaghoga component.
		/// </summary>
		public void Uninstall()
		{
			log.Info("Uninstalling module with " + this._installScriptFile);
			DatabaseUtil.ExecuteSqlScript(this._uninstallScriptFile);
		}

		private void CheckDatabaseScripts()
		{
			var directory = new DirectoryInfo(this._databaseScriptsDirectory);
			if (directory.Exists)
			{
				foreach (FileInfo file in directory.GetFiles("*.sql"))
				{
					if (file.Name.ToLower() == "install.sql")
					{
						this._installScriptFile = file.FullName;
					}
					else if (file.Name.ToLower() == "uninstall.sql")
					{
						this._uninstallScriptFile = file.FullName;
					}
					else
					{
						// Extract the version from the script filename.
						// NOTE: these filenames have to be in the major.minor.patch.sql format
						string[] extractedVersion = file.Name.Split('.');
						if (extractedVersion.Length == 4)
						{
							var version = new Version(
								Int32.Parse(extractedVersion[0]),
								Int32.Parse(extractedVersion[1]),
								Int32.Parse(extractedVersion[2]));
							this._upgradeScriptVersions.Add(version);
						}
						else
						{
							log.Warn(String.Format("Invalid SQL script file found in {0}: {1}", this._databaseScriptsDirectory, file.Name));
						}
					}
				}
			}
		}

		private void CheckCurrentVersionInDatabase()
		{
			if (this._assembly != null)
			{
				this._currentVersionInDatabase = DatabaseUtil.GetAssemblyVersion(this._assembly.GetName().Name);
			}
		}

		private bool CheckCanInstall()
		{
			return this._currentVersionInDatabase == null && this._installScriptFile != null;
		}

		private bool CheckCanUpgrade()
		{
			if (this._assembly != null)
			{
				if (this._currentVersionInDatabase != null && this._upgradeScriptVersions.Count > 0)
				{
					// Upgrade is possible if the script with the highest version number
					// has a number higher than the current database version AND when the
					// assembly version number is equal or higher than the script with
					// the highest version number.
					var highestScriptVersion = (Version) this._upgradeScriptVersions[this._upgradeScriptVersions.Count - 1];

					if (this._currentVersionInDatabase < highestScriptVersion
					    && this._assembly.GetName().Version >= highestScriptVersion)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool CheckCanUninstall()
		{
			return (this._assembly != null && this._uninstallScriptFile != null);
		}
	}
}