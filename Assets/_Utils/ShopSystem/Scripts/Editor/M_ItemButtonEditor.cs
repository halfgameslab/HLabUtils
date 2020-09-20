using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Mup.ShopSystem.Data;
using System.Linq;
using System;

namespace Mup.ShopSystem.UI
{
    [CustomEditor(typeof(M_ItemButton))]
    public class M_ItemButtonEditor : Editor
    {
        private GUIContent _nameContent = new GUIContent("Name:", "Item Name, If used with the MultiLanguage system put the string ID. ");
        private List<M_GenericItem> _avaliableItems;
        M_ItemButton _target;

        SerializedProperty _itemName;
        SerializedProperty _payment;
        SerializedProperty _btnTxt;
        SerializedProperty _onButtonValue;
        SerializedProperty _dontHaveText;
        SerializedProperty _alreadEquiped;
        SerializedProperty _alreadHave;



        public override void OnInspectorGUI()
        {
            _avaliableItems = M_ItemDataManager.GetAllAvaliableProducts();
            _target = (M_ItemButton)target;
            serializedObject.Update();
            //Find all Variables;
            _itemName = serializedObject.FindProperty("_itemName");
            _payment = serializedObject.FindProperty("_paymentMethod");
            _btnTxt = serializedObject.FindProperty("_buttonText");
            _onButtonValue = serializedObject.FindProperty("_onButtonValue");
            _dontHaveText = serializedObject.FindProperty("_donthaveText");
            _alreadEquiped = serializedObject.FindProperty("_alreadEquip");
            _alreadHave = serializedObject.FindProperty("_alreadHaveText");
            //
            ConstrucInspector();
            serializedObject.ApplyModifiedProperties();

        }

        private void ConstrucInspector()
        {
            int itemChoose = _avaliableItems.FindIndex(T => T.ItemName == _itemName.stringValue);
            if (itemChoose == -1)
            {
                EditorGUI.BeginChangeCheck();
                itemChoose = EditorGUILayout.Popup(0, ItemNameListToPopup());
                if (EditorGUI.EndChangeCheck())
                {
                    _itemName.stringValue = _avaliableItems[itemChoose].ItemName;
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                itemChoose = EditorGUILayout.Popup("Item Name",itemChoose, ItemNameListToPopup());
                if (EditorGUI.EndChangeCheck())
                {
                    _itemName.stringValue = _avaliableItems[itemChoose].ItemName;
                }
                ConstructPaymentSelection(itemChoose);
            }
        }

        private void ConstructPaymentSelection(int selectedItem)
        {
            if (_avaliableItems[selectedItem].PayInfo.Count > 0)
            {
                string[] paymentsAvaliable = new string[_avaliableItems[selectedItem].PayInfo.Count];
                for (int i = 0; i < _avaliableItems[selectedItem].PayInfo.Count; i++)
                {
                    paymentsAvaliable[i] = _avaliableItems[selectedItem].PayInfo[i].CurrencyName;
                }
                int itemChoose = paymentsAvaliable.ToList().FindIndex(T => T == _payment.stringValue);
                EditorGUI.BeginChangeCheck();
                itemChoose = EditorGUILayout.Popup("Payment Method",itemChoose, paymentsAvaliable);
                if (EditorGUI.EndChangeCheck())
                {
                    _payment.stringValue = paymentsAvaliable[itemChoose];
                }
                _onButtonValue.boolValue = EditorGUILayout.Toggle("Display Item Value On Button", _onButtonValue.boolValue);
                _dontHaveText.stringValue = EditorGUILayout.TextField("Dont Have Text", _dontHaveText.stringValue);
                _alreadEquiped.stringValue = EditorGUILayout.TextField("Already equiped this item Text", _alreadEquiped.stringValue);
                _alreadHave.stringValue = EditorGUILayout.TextField("Already Have this item Text", _alreadHave.stringValue);
            }
            else
            {
                EditorGUILayout.LabelField("This item is not avaliable to buy with any currency");
            }
        }

        private string[] ItemNameListToPopup()
        {
            string[] itemNameAvaliables = new string[_avaliableItems.Count];
            for (int i = 0; i < _avaliableItems.Count; i++)
            {
                itemNameAvaliables[i] = _avaliableItems[i].ItemName;
            }
            return itemNameAvaliables;
        }


        Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}