using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NHibernate.Test.NHSpecificTest.NH1460
{

    #region Item Class

    public class Item : Entity<Guid>
    {
        public string Name
        {
            get;
            set;
        }

        private IList<ItemDetail> _itemDetails = new List<ItemDetail> ();

        public ReadOnlyCollection<ItemDetail> ItemDetails
        {
            get
            {
                return new List<ItemDetail> (_itemDetails).AsReadOnly ();
            }
        }

        public void AddSubItem( ItemDetail id )
        {
            if( id != null && _itemDetails.Contains (id) == false )
            {
                id.Owner = this;
                _itemDetails.Add (id);
            }
        }
    }

    #endregion

    #region ItemDetail class

    public class ItemDetail : Entity<int>
    {
        public Item Owner
        {
            get;
            internal set;
        }

        public string Code
        {
            get;
            set;
        }
    }

    #endregion

    #region Entity Base

    [Serializable]
    public abstract class Entity<TId>
    {
        private TId         _id = default (TId);

        public TId Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public int Version
        {
            get;
            private set;
        }

        #region Equals and Hashcode

        public override bool Equals( object obj )
        {
            Entity<TId> other = obj as Entity<TId>;

            if( other == null || this.GetType () != other.GetType () )
            {
                return false;
            }

            bool otherIsTransient = Equals (other.Id, default (TId));
            bool thisIsTransient = Equals (this.Id, default (TId));

            if( otherIsTransient && thisIsTransient )
            {
                return ReferenceEquals (this, other);
            }

            return Id.Equals (other.Id);

        }

        private int? _oldHashCode;

        public override int GetHashCode()
        {
            // Once we have a hash code we'll never change it
            if( _oldHashCode.HasValue )
            {
                return _oldHashCode.Value;
            }

            bool thisIsTransient = Equals (Id, default (TId));


            // When this instance is transient, we use the base GetHashCode()
            // and remember it, so an instance can NEVER change its hash code.

            if( thisIsTransient )
            {
                _oldHashCode = base.GetHashCode ();

                return _oldHashCode.Value;
            }

            return Id.GetHashCode ();
        }

        #endregion

        #region Operator overloads

        public static bool operator ==( Entity<TId> left, Entity<TId> right )
        {
            return Equals (left, right);
        }

        public static bool operator !=( Entity<TId> left, Entity<TId> right )
        {
            return Equals (left, right) == false;
        }

        #endregion

    }

    #endregion

}
