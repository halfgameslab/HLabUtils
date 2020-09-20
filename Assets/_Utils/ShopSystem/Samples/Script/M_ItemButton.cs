using Mup.EventSystem.Events;
using Mup.Misc.Generic;
using Mup.Multilanguage.Plugins;
using Mup.ShopSystem;
using Mup.ShopSystem.Data;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mup.ShopSystem.UI
{
    /// <summary>
    /// Exemple Button
    /// </summary>
    public class M_ItemButton : MonoBehaviour
    {
        private const string SHOP_NAME = "#_SHOP_MANAGER_#";

        private Button _myButton;
        private M_GenericItem _itemToBuy;
        /// <summary>
        /// Payment method that button will use.
        /// </summary>
        [SerializeField] protected string _paymentMethod;
        /// <summary>
        /// Item name to find in the database. (this can be a int field).
        /// </summary>
        [SerializeField] protected string _itemName;
        /// <summary>
        /// Button text. (this button is using the Multilanguage system.
        /// </summary>
        [SerializeField] protected ML_TextMeshProExtension _buttonText;
        /// <summary>
        /// The button will show the item value on the text?
        /// </summary>
        [SerializeField] protected bool _onButtonValue;
        [SerializeField] private string _donthaveText;
        [SerializeField] private string _alreadEquip;
        [SerializeField] private string _alreadHaveText;

        private void OnEnable()
        {
            this.SetInstanceName(SHOP_NAME);
            _buttonText = GetComponentInChildren<ML_TextMeshProExtension>();
            this.AddEventListener(M_ShopEvents.ON_SUCCESS, OnSuccesBuy);
            _itemToBuy = M_ItemDataManager.GetProductInfo(_itemName);
            _myButton = GetComponent<Button>();
            ReloadButton();
        }

        private void OnSuccesBuy(ES_Event ev)
        {
            print("CHECK");
            _myButton.onClick.RemoveAllListeners();
            CheckButtonStatus();
        }

        /// <summary>
        /// Initialize the button.
        /// </summary>
        /// <param name="item">Item to assign to the button</param>
        /// <param name="show">will show the item value in the button text?</param>
        /// <param name="payment">Payment to use with this button</param>
        public void Init(M_GenericItem item, bool show, string payment)
        {
            _itemToBuy = item;
            _itemName = _itemToBuy.ItemName;
            _onButtonValue = show;
            _paymentMethod = payment;
            SetButtonString("@generic_label_buy");
        }

        private void CheckButtonStatus()
        {
            if (Inventory.MyItems.Contains(_itemToBuy))
            {
                _myButton.onClick.AddListener(() => Inventory.Equip(_itemToBuy));
                if (_buttonText != null)
                {
                    CheckButtonString();
                }
            }
            else
            {
                _myButton.onClick.AddListener(() => M_ShopManager.BuyItem(_itemToBuy, _paymentMethod));
                if (_buttonText != null)
                {
                    SetButtonString(_donthaveText);
                }
            }
            _myButton.onClick.AddListener(() => ReloadButton());
        }

        private void ReloadButton()
        {
            _myButton.onClick.RemoveAllListeners();
            _myButton.onClick = new Button.ButtonClickedEvent();
            CheckButtonStatus();
        }

        private void CheckButtonString()
        {
            if (Inventory.Instance.SelectedSkin == _itemToBuy.Id)
            {
                SetButtonString(_alreadEquip);
            }
            else
            {
                SetButtonString(_alreadHaveText);
            }
        }

        /// <summary>
        /// Set the desired string to the button.
        /// </summary>
        /// <param name="buttonString">String to set in the button.</param>
        public void SetButtonString(string buttonString)
        {
            if (_onButtonValue)
            {
                int productValue = M_ItemDataManager.GetProductValue(_itemToBuy, _paymentMethod);

                if (productValue != -1)
                {
                    _buttonText.Text = buttonString + "+ </br> " + M_ShopManager.Currency + productValue;
                }
                else
                {
                    _buttonText.Text = "Not Avaliable using " + _paymentMethod;
                    _myButton.enabled = false;
                }
            }
            else
            {
                _buttonText.Text = buttonString;
            }
        }
    }
}