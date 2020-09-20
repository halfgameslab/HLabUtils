using Akapagion_Library;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mup.ShopSystem.Data
{
    /// <summary>
    /// Base class of all items.
    ///     -the base class only: 
    ///         (int)       ID
    ///         (string)    Item Name
    ///         (string)    Item Description
    ///         (int)       Value               
    ///         (string)    Event name to dispatch). 
    /// If you need something else u need to inherit from this.
    /// </summary>
    [System.Serializable]
    public class M_GenericItem
    {
        [assembly: InternalsVisibleTo("M_StoreManager")]
        [assembly: InternalsVisibleTo("M_ItemDataEditor")]

        /// <summary>
        /// Item unique identifier.
        /// </summary>
        [SerializeField]
        internal int _id;
        /// <summary>
        /// Public Acessor for the unique Identifer;
        /// </summary>
        public int Id { get { return _id; } }
        /// <summary>
        /// Item name. Use '_' to separeta the strings. DO NOT USE SPACE.
        /// If used with the MultiLanguage system put the string ID. 
        /// </summary>
        [SerializeField]
        internal string _itemName;
        /// <summary>
        /// Public Acessor for the Item Name.
        /// </summary>
        public string ItemName { get { return _itemName; } }
        /// <summary>
        /// Item description
        /// If used with the MultiLanguage system put the string ID.
        /// </summary>
        [SerializeField]
        internal string _itemDescription;
        /// <summary>
        /// Public Acessor for the Item Description.
        /// </summary>
        public string ItemDescription { get { return _itemDescription; } }
        /// <summary>
        /// Item payment method and value.
        /// </summary>
        [SerializeField]
        internal List<M_PaymentMethod> _payInfo = new List<M_PaymentMethod>();
        /// <summary>
        /// Public Acessor for the Item Value.
        /// </summary>
        public List<M_PaymentMethod> PayInfo { get { return _payInfo; } }
        /// <summary>
        /// Event to dispatch afther you buy the item.
        /// </summary>
        [SerializeField]
        internal string _buyEvent;

        /// <summary>
        /// Method to get the item value.
        /// </summary>
        /// <param name="payment">Payment to get the value.</param>
        /// <returns>Value of this item based on the Payment.</returns>
        public int ItemValue(string payment)
        {
            M_PaymentMethod temp = _payInfo.Find(T => T.CurrencyName == payment);
            if (temp != null)
            {
                return temp.Value;
            }
            else return -1;
        }

        /// <summary>
        /// Format the item name (if you forgot to captalize the first letter this will captalize for you).
        /// </summary>
        /// <returns>The captialized item name (without the _)</returns>
        public string NameToString()
        {
            string[] names = _itemName.Split('_');
            string name = "";
            foreach (var s in names)
            {
                name += StringExtension.CaptalizeFirstLetter(s) + " ";
            }
            return name;
        }
    }
}