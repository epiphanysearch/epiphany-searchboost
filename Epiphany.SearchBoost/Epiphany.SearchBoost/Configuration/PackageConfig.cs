using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using Examine;
using System.Web.Hosting;

namespace Epiphany.SearchBoost.Configuration
{
	/// <summary>
	/// The global config object for the package
	/// </summary>
	public class PackageConfig
	{
		//private backing fields
		private List<RuleSet> _ruleSets;
		private static PackageConfig _instance;

		//public properties
		public List<RuleSet> RuleSets
		{
			get
			{
				return _ruleSets;
			}
		}

		public static PackageConfig Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new PackageConfig();
				}
				return _instance;
			}
		}

		/// <summary>
		/// Creates the Config option, initialises collections and loads config information from the file
		/// </summary>
		private PackageConfig()
		{
			_ruleSets = new List<RuleSet>();

			LoadXmlConfig();
		}

		/// <summary>
		/// Loads the content from the config file
		/// </summary>
		private void LoadXmlConfig()
		{
			//load document
			XmlDocument document = new XmlDocument();
			document.Load(HostingEnvironment.MapPath("~/config/searchBoost.config"));

			//loop through each config item and set it up
			foreach (XmlNode node in document.SelectNodes("/searchBoost/ruleSet"))
			{
				if (node.NodeType != XmlNodeType.Element)
				{
					continue;
				}

				string indexSet = node.Attributes["indexName"].Value;

				string stackCheck = node.Attributes["stackBoosts"].Value.ToLower();

				bool stackBoosts = false;

				if (stackCheck == "true")
				{
					stackBoosts = true;
				}

				//only process rule sets if it doesn't exist already
				if (_ruleSets.Any(a => a.IndexName == indexSet) == false)
				{
					//create the rule set
					RuleSet item = new RuleSet(indexSet, stackBoosts);

					//loop through the child rules an create those too
					foreach (XmlNode ruleItem in node.SelectNodes("./rule"))
					{
						bool isWildCard = false;

						string docTypesTemp = ruleItem.Attributes["docTypeAliases"].Value;

						string nodeIdsTemp = ruleItem.Attributes["nodeIds"].Value;

						if (docTypesTemp == "*")
						{
							isWildCard = true;
						}

						double boostAmount = 0;
						double.TryParse(ruleItem.Attributes["boostAmount"].Value, out boostAmount);

						int boostIfNewerThan = 0;
						int.TryParse(ruleItem.Attributes["boostIfNewerThan"].Value, out boostIfNewerThan);

						string dateField = ruleItem.Attributes["dateField"].Value;

						//create basic rule
						Rule rule = new Rule(isWildCard, boostAmount, boostIfNewerThan, dateField);

						//if not wildcard, add the conditions
						if (isWildCard == false)
						{
							foreach (string docType in docTypesTemp.Split(','))
							{
								rule.DocTypeAliases.Add(docType.Trim());
							}

							foreach (string nodeId in nodeIdsTemp.Split(','))
							{
								rule.NodeIds.Add(nodeId.Trim());
							}
						}

						item.Rules.Add(rule);
					}

					_ruleSets.Add(item);
				}
			}
		}

		/// <summary>
		/// Clears the singleton, so that it's refreshed next time someone asks for it
		/// </summary>
		public static void RefreshInstance()
		{
			_instance = null;
		}

		/// <summary>
		/// Method to list out useful information for debugging the rules in case of issues
		/// </summary>
		/// <returns></returns>
		public string ListDebugInfo()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("<h2>Epiphany Search Boost Debug Info:</h2>\n");

			builder.Append("<h3>Indexers Defined on the Site in ExamineSettings.Config</h3>\n");

			foreach (var thingy in ExamineManager.Instance.IndexProviderCollection)
			{
				builder.AppendFormat("Indexer name: {0}<br/>\n", ((System.Configuration.Provider.ProviderBase)thingy).Name);
			}

			builder.Append("<h3>Rule Sets:</h3>\n");

			if (_instance.RuleSets.Count == 0)
			{
				builder.Append("<p><strong>No rule sets defined! Note: only rule sets that match an indexer are stored in the config object.</strong></p>\n");
			}
			else
			{
				foreach (RuleSet ruleSet in PackageConfig.Instance.RuleSets)
				{
					builder.AppendFormat("<h4>Rule Set applies to index: {0}, Stack Boosts: {1}</h4>\n", ruleSet.IndexName, ruleSet.StackBoosts);

					int count = 0;

					foreach (Rule rule in ruleSet.Rules)
					{
						count++;

						builder.AppendFormat("<p><strong>Rule {5}:</strong><br/>Is Wildcard: {6}<br/>DocTypes: {0}<br/>NodeIds: {1}<br/>Boost Amount: {2}<br/>Boost If Newer Than: {3}<br/>Date Field: {4}</p>\n",
							string.Join(",", rule.DocTypeAliases),
							string.Join(",", rule.NodeIds),
							rule.BoostAmount,
							rule.BoostIfNewerThan,
							rule.DateField,
							count,
							rule.IsWildcard);
					}

					builder.Append("<hr/>\n");
				}
			}

			return builder.ToString();
		}
	}
}