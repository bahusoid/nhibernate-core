using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH2116
{
	class ClientTradingLimitDetail
	{
		public virtual int TradingLimitDetailID { get; set; }
		public virtual int OrderID { get; set; }
		public virtual int SubOrderID { get; set; }
		public virtual int SubOrderReqID { get; set; }
		public virtual decimal OrderedValue { get; set; }
		public virtual decimal TradingLimitBalance { get; set; }
		public virtual string Name { get; set; }
		public virtual ClientTradingLimit ClientTradingLimit { get; set; }
	}
	
	class ClientTradingLimit
	{
		public virtual int TradingLimitID { get; set; }
		public virtual int ClientID { get; set; }
		public virtual int CurrencyID { get; set; }
		public virtual decimal OpenBalance { get; set; }
		public virtual decimal WarningThreshold { get; set; }
		public virtual IList<ClientTradingLimitDetail> TradingLimitDetails { get; set; } = new List<ClientTradingLimitDetail>();
	}
}
