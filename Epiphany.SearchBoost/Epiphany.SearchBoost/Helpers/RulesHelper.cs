using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epiphany.SearchBoost.Configuration;
using Examine;
using Examine.LuceneEngine.Providers;

namespace Epiphany.SearchBoost.Helpers
{
	public static class RulesHelper
	{
		/// <summary>
		/// Hooks the IndexSets that have rules up to the events we need to use
		/// </summary>
		public static void SetUpEventHandlers()
		{
			//loop through indexers and check for matching rule sets
			foreach (var thingy in ExamineManager.Instance.IndexProviderCollection)
			{
				string name = ((System.Configuration.Provider.ProviderBase)thingy).Name;

				if (PackageConfig.Instance.RuleSets.Any(a => a.IndexName == name))
				{
					((LuceneIndexer)thingy).DocumentWriting += ProcessRulesForIndex;

					LogUmbracoDebugMessage(string.Format("Attaching SearchBoost Event Handler to: {0}", name));
				}
			}
		}

		/// <summary>
		/// Event Handler to check if the Document being indexed needs to be boosted
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void ProcessRulesForIndex(object sender, Examine.LuceneEngine.DocumentWritingEventArgs e)
		{
			//check that the sender is what we think it is
			if (sender is System.Configuration.Provider.ProviderBase)
			{
				string name = ((System.Configuration.Provider.ProviderBase)sender).Name;

				//ensure there is a rule set to process for the indexer (in case config has changed, but application hasn't restarted
				if (PackageConfig.Instance.RuleSets.Any(a => a.IndexName == name))
				{
					RuleSet rules = PackageConfig.Instance.RuleSets.First(a => a.IndexName == name);

					//get all of the rules that might apply to our current document
					int nodeId = e.NodeId;
					string docTypeAlias = string.Empty;
					
					if (e.Fields.ContainsKey("nodeTypeAlias"))
					{
						docTypeAlias = e.Fields["nodeTypeAlias"];
					}

					//only proceed if we were able to get the info we need
					if (nodeId > 0 && string.IsNullOrEmpty(docTypeAlias) == false)
					{
						double boost = 0;
						int daysTemp = -1;

						var rulesToApply = rules.Rules.Where(a => a.IsWildcard == true || a.NodeIds.Contains(nodeId.ToString()) || a.DocTypeAliases.Contains(docTypeAlias)).ToList();

						if (rulesToApply.Count > 0)
						{
							foreach (var rule in rulesToApply)
							{
								bool setBoost = false;
								int daysOld = 0;

								//if there is a date field to check make sure that it matches the settings, otherwise it was a general match and we can go ahead and boost it
								if (rule.BoostIfNewerThan > 0 && string.IsNullOrEmpty(rule.DateField) == false)
								{
									if (e.Fields.ContainsKey(rule.DateField))
									{
										DateTime documentDate = DateTime.MinValue;
										DateTime todaysDate = DateTime.Now;

										DateTime.TryParse(e.Fields[rule.DateField], out documentDate);

										daysOld = (int)(documentDate - todaysDate).TotalDays;

										if (daysOld <= rule.BoostIfNewerThan)
										{
											daysTemp = daysOld;
											setBoost = true;
										}
									}
								}
								else
								{
									setBoost = true;
								}

								//if we should boost the document, use the rule set's settings to determine if the boost should stack or use the highest value
								if (setBoost == true)
								{
									if (rules.StackBoosts == true)
									{
										boost += rule.BoostAmount;
									}
									else
									{
										if (rule.BoostAmount > boost)
										{
											boost = rule.BoostAmount;
										}
									}
								}
							}

							//if there is a boost score, boost the document
							if (boost > 0)
							{
								//e.Document.Add(new Lucene.Net.Documents.Field("boostDebug", string.Format("Boosted document by: {0}, days old: {1}, matchingRules: {2}", boost, daysTemp, rulesToApply.Count), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

								e.Document.SetBoost((float)boost);
							}
						}
						else
						{
							//e.Document.Add(new Lucene.Net.Documents.Field("boostDebug", string.Format("SearchBoost does not have a rule for: {0}, {1}", e.NodeId, docTypeAlias), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
						}
					}
					else
					{
						//e.Document.Add(new Lucene.Net.Documents.Field("boostDebug", string.Format("SearchBoost could not find required fields for node id: {0}", e.NodeId), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
					}
				}
			}
		}

		/// <summary>
		/// Logs a debug message to the Umbraco log table
		/// </summary>
		/// <param name="message"></param>
		public static void LogUmbracoDebugMessage(string message)
		{
			try
			{
				umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, umbraco.BusinessLogic.User.GetUser(0), -1, message);
			}
			catch
			{
			
			}
		}
	}
}