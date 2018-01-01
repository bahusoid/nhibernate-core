﻿//------------------------------------------------------------------------------
// <auto-generated>This code was generated by LLBLGen Pro v4.2.</auto-generated>
//------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace NH.Bencher.EntityClasses
{
	/// <summary>Class which represents the entity 'Employee'</summary>
	public partial class Employee
	{
		#region Class Member Declarations
		private ISet<Document> _documents;
		private ISet<EmployeeDepartmentHistory> _employeeDepartmentHistories;
		private ISet<EmployeePayHistory> _employeePayHistories;
		private ISet<JobCandidate> _jobCandidates;
		private Person _person;
		private ISet<PurchaseOrderHeader> _purchaseOrderHeaders;
		private SalesPerson _salesPerson;
		private System.DateTime _birthDate;
		private System.Boolean _currentFlag;
		private System.Int32 _employeeId;
		private System.String _gender;
		private System.DateTime _hireDate;
		private System.String _loginId;
		private System.String _maritalStatus;
		private System.DateTime _modifiedDate;
		private System.String _nationalIdnumber;
		private Nullable<System.Int16> _organizationLevel;
		private System.String _organizationNode;
		private System.Guid _rowguid;
		private System.Boolean _salariedFlag;
		private System.Int16 _sickLeaveHours;
		private System.String _title;
		private System.Int16 _vacationHours;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="Employee"/> class.</summary>
		public Employee() : base()
		{
			_documents = new HashSet<Document>();
			_employeeDepartmentHistories = new HashSet<EmployeeDepartmentHistory>();
			_employeePayHistories = new HashSet<EmployeePayHistory>();
			_jobCandidates = new HashSet<JobCandidate>();
			_purchaseOrderHeaders = new HashSet<PurchaseOrderHeader>();
			_employeeId = default(System.Int32);
			_organizationLevel = default(Nullable<System.Int16>);
			OnCreated();
		}

		/// <summary>Method called from the constructor</summary>
		partial void OnCreated();

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. </returns>
		public override int GetHashCode()
		{
			int toReturn = base.GetHashCode();
			toReturn ^= this.EmployeeId.GetHashCode();
			return toReturn;
		}
	
		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}
		

		#region Class Property Declarations
		/// <summary>Gets or sets the BirthDate field. </summary>	
		public virtual System.DateTime BirthDate
		{ 
			get { return _birthDate; }
			set { _birthDate = value; }
		}

		/// <summary>Gets or sets the CurrentFlag field. </summary>	
		public virtual System.Boolean CurrentFlag
		{ 
			get { return _currentFlag; }
			set { _currentFlag = value; }
		}

		/// <summary>Gets the EmployeeId field. </summary>	
		public virtual System.Int32 EmployeeId
		{ 
			get { return _employeeId; }
		}

		/// <summary>Gets or sets the Gender field. </summary>	
		public virtual System.String Gender
		{ 
			get { return _gender; }
			set { _gender = value; }
		}

		/// <summary>Gets or sets the HireDate field. </summary>	
		public virtual System.DateTime HireDate
		{ 
			get { return _hireDate; }
			set { _hireDate = value; }
		}

		/// <summary>Gets or sets the LoginId field. </summary>	
		public virtual System.String LoginId
		{ 
			get { return _loginId; }
			set { _loginId = value; }
		}

		/// <summary>Gets or sets the MaritalStatus field. </summary>	
		public virtual System.String MaritalStatus
		{ 
			get { return _maritalStatus; }
			set { _maritalStatus = value; }
		}

		/// <summary>Gets or sets the ModifiedDate field. </summary>	
		public virtual System.DateTime ModifiedDate
		{ 
			get { return _modifiedDate; }
			set { _modifiedDate = value; }
		}

		/// <summary>Gets or sets the NationalIdnumber field. </summary>	
		public virtual System.String NationalIdnumber
		{ 
			get { return _nationalIdnumber; }
			set { _nationalIdnumber = value; }
		}

		/// <summary>Gets the OrganizationLevel field. </summary>	
		public virtual Nullable<System.Int16> OrganizationLevel
		{ 
			get { return _organizationLevel; }
		}

		/// <summary>Gets or sets the OrganizationNode field. </summary>	
		public virtual System.String OrganizationNode
		{ 
			get { return _organizationNode; }
			set { _organizationNode = value; }
		}

		/// <summary>Gets or sets the Rowguid field. </summary>	
		public virtual System.Guid Rowguid
		{ 
			get { return _rowguid; }
			set { _rowguid = value; }
		}

		/// <summary>Gets or sets the SalariedFlag field. </summary>	
		public virtual System.Boolean SalariedFlag
		{ 
			get { return _salariedFlag; }
			set { _salariedFlag = value; }
		}

		/// <summary>Gets or sets the SickLeaveHours field. </summary>	
		public virtual System.Int16 SickLeaveHours
		{ 
			get { return _sickLeaveHours; }
			set { _sickLeaveHours = value; }
		}

		/// <summary>Gets or sets the Title field. </summary>	
		public virtual System.String Title
		{ 
			get { return _title; }
			set { _title = value; }
		}

		/// <summary>Gets or sets the VacationHours field. </summary>	
		public virtual System.Int16 VacationHours
		{ 
			get { return _vacationHours; }
			set { _vacationHours = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'Document.Employee - Employee.Documents (m:1)'</summary>
		public virtual ISet<Document> Documents
		{
			get { return _documents; }
			set { _documents = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'EmployeeDepartmentHistory.Employee - Employee.EmployeeDepartmentHistories (m:1)'</summary>
		public virtual ISet<EmployeeDepartmentHistory> EmployeeDepartmentHistories
		{
			get { return _employeeDepartmentHistories; }
			set { _employeeDepartmentHistories = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'EmployeePayHistory.Employee - Employee.EmployeePayHistories (m:1)'</summary>
		public virtual ISet<EmployeePayHistory> EmployeePayHistories
		{
			get { return _employeePayHistories; }
			set { _employeePayHistories = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'JobCandidate.Employee - Employee.JobCandidates (m:1)'</summary>
		public virtual ISet<JobCandidate> JobCandidates
		{
			get { return _jobCandidates; }
			set { _jobCandidates = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'Employee.Person - Person.Employee (1:1)'</summary>
		public virtual Person Person
		{
			get { return _person; }
			set { _person = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'PurchaseOrderHeader.Employee - Employee.PurchaseOrderHeaders (m:1)'</summary>
		public virtual ISet<PurchaseOrderHeader> PurchaseOrderHeaders
		{
			get { return _purchaseOrderHeaders; }
			set { _purchaseOrderHeaders = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'SalesPerson.Employee - Employee.SalesPerson (1:1)'</summary>
		public virtual SalesPerson SalesPerson
		{
			get { return _salesPerson; }
			set { _salesPerson = value; }
		}
		
		#endregion
	}
}
