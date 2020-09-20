using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mup.ShopSystem.Data
{
    /// <summary>
    /// Class that will handle the ItemDatabase and payment methods..
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Mup/ShopSystem/Create GenericItem Database"), System.Serializable]
    public class M_ItemDatabase : ScriptableObject
    {

        [assembly: InternalsVisibleTo("M_SotrePopulate")]
        [assembly: InternalsVisibleTo("M_ItemDataEditor")]
        /// <summary>
        /// The Item database.
        /// </summary>
        [SerializeField]
        internal List<M_GenericItem> _itemData = new List<M_GenericItem>();
        /// <summary>
        /// List with all payment methods.
        /// </summary>
        [SerializeField]
        public string[] PaymentMethods = new string[] { "Cash", "In-Game" };
        /// <summary>
        /// Find an item in the database.
        /// </summary>
        /// <param name="name">Item name to find</param>
        /// <returns>The item with the specific Name</returns>
        internal M_GenericItem FetchItemByName(string name)
        {
            return _itemData.Find(T => T.ItemName == name);
        }
        /// <summary>
        /// Find an item in the database.
        /// </summary>
        /// <param name="id">Item Id to find</param>
        /// <returns>The item with the specific Id</returns>
        internal M_GenericItem FetchItemById(int id)
        {
            return _itemData.Find(T => T.Id == id);
        }
    }
}