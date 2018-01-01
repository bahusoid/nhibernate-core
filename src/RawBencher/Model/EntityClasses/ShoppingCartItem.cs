﻿//------------------------------------------------------------------------------
// <auto-generated>This code was generated by LLBLGen Pro v4.2.</auto-generated>
//------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace NH.Bencher.EntityClasses
{
	/// <summary>Class which represents the entity 'ShoppingCartItem'</summary>
	public partial class ShoppingCartItem
	{
		#region Class Member Declarations
		private Product _product;
		private System.DateTime _dateCreated;
		private System.DateTime _modifiedDate;
		private System.Int32 _quantity;
		private System.String _shoppingCartId;
		private System.Int32 _shoppingCartItemId;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="ShoppingCartItem"/> class.</summary>
		public ShoppingCartItem() : base()
		{
			_shoppingCartItemId = default(System.Int32);
			OnCreated();
		}

		/// <summary>Method called from the constructor</summary>
		partial void OnCreated();

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. </returns>
		public override int GetHashCode()
		{
			int toReturn = base.GetHashCode();
			toReturn ^= this.ShoppingCartItemId.GetHashCode();
			return toReturn;
		}
	
		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj)
		{
			if(obj == null) 
			{
				return false;
			}
			ShoppingCartItem toCompareWith = obj as ShoppingCartItem;
			return toCompareWith == null ? false : ((this.ShoppingCartItemId == toCompareWith.ShoppingCartItemId));
		}
		

		#region Class Property Declarations
		/// <summary>Gets or sets the DateCreated field. </summary>	
		public virtual System.DateTime DateCreated
		{ 
			get { return _dateCreated; }
			set { _dateCreated = value; }
		}

		/// <summary>Gets or sets the ModifiedDate field. </summary>	
		public virtual System.DateTime ModifiedDate
		{ 
			get { return _modifiedDate; }
			set { _modifiedDate = value; }
		}

		/// <summary>Gets or sets the Quantity field. </summary>	
		public virtual System.Int32 Quantity
		{ 
			get { return _quantity; }
			set { _quantity = value; }
		}

		/// <summary>Gets or sets the ShoppingCartId field. </summary>	
		public virtual System.String ShoppingCartId
		{ 
			get { return _shoppingCartId; }
			set { _shoppingCartId = value; }
		}

		/// <summary>Gets the ShoppingCartItemId field. </summary>	
		public virtual System.Int32 ShoppingCartItemId
		{ 
			get { return _shoppingCartItemId; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'ShoppingCartItem.Product - Product.ShoppingCartItems (m:1)'</summary>
		public virtual Product Product
		{
			get { return _product; }
			set { _product = value; }
		}
		
		#endregion
	}
}
