using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epiphany.SearchBoost.Configuration
{
	/// <summary>
	/// Represents a rule for matching search boosts
	/// </summary>
	public class Rule
	{
		private bool _isWildcard;

		private List<string> _docTypeAliases;

		private List<string> _nodeIds;

		private double _boostAmount;

		private int _boostIfNewerThan;

		private string _dateField;

		public bool IsWildcard
		{
			get { return _isWildcard; }
		}

		public List<string> DocTypeAliases
		{
			get { return _docTypeAliases; }
		}

		public List<string> NodeIds
		{
			get { return _nodeIds; }
		}

		public double BoostAmount
		{
			get { return _boostAmount; }
		}

		public int BoostIfNewerThan
		{
			get { return _boostIfNewerThan; }
		}

		public string DateField
		{
			get { return _dateField; }
		}

		public Rule(bool isWildCard, double boostAmount, int boostIfNewerThan, string dateField)
		{
			_isWildcard = isWildCard;

			_boostAmount = boostAmount;

			_boostIfNewerThan = boostIfNewerThan;

			_dateField = dateField;

			_docTypeAliases = new List<string>();

			_nodeIds = new List<string>();
		}
	}
}