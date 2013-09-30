using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Epiphany.SearchBoost.Configuration;
using Epiphany.SearchBoost.Helpers;
using umbraco.businesslogic;
using umbraco.interfaces;
using umbraco.BusinessLogic;

namespace Epiphany.SearchBoost
{
	public class StartUpHander : ApplicationBase
	{
		public StartUpHander()
		{
			//if not already set, create file watcher for config folder
			if (HttpContext.Current.Application["searchBoost"] == null)
			{
			    //code to expire config if the config file is changed
			    string path = HttpContext.Current.Server.MapPath("~/config/");
			    HttpContext.Current.Application.Add("searchBoost", new FileSystemWatcher(path));
			    FileSystemWatcher watcher = (FileSystemWatcher)HttpContext.Current.Application["searchBoost"];
			    watcher.EnableRaisingEvents = true;
			    watcher.IncludeSubdirectories = true;
			    watcher.Changed += new FileSystemEventHandler(this.ExpireConfig);
			    watcher.Created += new FileSystemEventHandler(this.ExpireConfig);
			    watcher.Deleted += new FileSystemEventHandler(this.ExpireConfig);
			}

			//set up the event handers for the indexers
			RulesHelper.SetUpEventHandlers();

			RulesHelper.LogUmbracoDebugMessage("SearchBoost startup events have run");
		}

		/// <summary>
		/// Expires the config for the package
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ExpireConfig(object sender, FileSystemEventArgs e)
		{
			PackageConfig.RefreshInstance();
		}
	}
}