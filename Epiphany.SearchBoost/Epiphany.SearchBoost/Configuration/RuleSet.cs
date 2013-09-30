using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epiphany.SearchBoost.Configuration
{
	/// <summary>
	/// Represents a set of rules for a specific search indexer
	/// </summary>
	public class RuleSet
	{
		private string _indexName;

		private bool _stackBoosts;

		private List<Rule> _rules;

		public string IndexName
		{
			get { return _indexName; }
		}

		public bool StackBoosts
		{
			get { return _stackBoosts; }
		}

		public List<Rule> Rules
		{
			get { return _rules; }
		}

		public RuleSet(string indexName, bool stackBoosts)
		{
			_indexName = indexName;

			_stackBoosts = stackBoosts;

			_rules = new List<Rule>();
		}
	}
}