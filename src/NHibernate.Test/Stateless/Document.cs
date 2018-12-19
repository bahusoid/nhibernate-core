using System;
using System.Collections.Generic;

namespace NHibernate.Test.Stateless
{
	public class Document
	{
		private string text;
		private string name;
		private DateTime? lastModified;

		public Document() { }

		public Document(string text, string name)
		{
			this.text = text;
			this.name = name;
		}

		public string Text
		{
			get { return text; }
			set { text = value; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public DateTime? LastModified
		{
			get { return lastModified; }
			set { lastModified = value; }
		}

		public int Rating { get; set; }

		public virtual IList<Document> References { get; set; } = new List<Document>();
	}
}
