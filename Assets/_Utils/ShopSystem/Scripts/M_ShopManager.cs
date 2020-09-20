using UnityEngine;
using Mup.EventSystem.Events;
using Mup.ShopSystem.Data;
using Mup.QuestSystem;

/// <summary>
/// ShopSystem Developed by:MUP Studios.
/// </summary>
namespace Mup.ShopSystem
{
    public class M_ShopEvents
    {
        public const string ON_SUCCESS = "ON_SUCCESS";
        public const string ON_FAIL = "ON_FAIL";
    }
    
    /// <summary>
    /// Class that will handle all shop transactions and deliver the item to the player.
    /// </summary>
    public class M_ShopManager : MonoBehaviour
    {
        /// <summary>
        /// Static reference to this stance.
        /// </summary>
        public static M_ShopManager Instance;
        /// <summary>
        /// Currency Symbol to display.
        /// </summary>
        [SerializeField] private string _currencySymbol;
        /// <summary>
        /// Acessor to the Currency Symbol to display.
        /// </summary>
        public static string Currency{ get { return Instance._currencySymbol; } }

        /// </summary>
        private M_ShopManager()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            //IS_MonoInstance.GetInstanceByID("QuestManager").AddEventListener(ES_Event.ON_COLLECT, OnQuestCollectHandler);
        }

        public void OnQuestCollectHandler(ES_Event ev)
        {
            QS_Quest collectedQuest = (QS_Quest)ev.Data;
            foreach (QS_Reward reward in collectedQuest.Rewards)
            {
                RewardItem(reward.ID);
            }            
        }

        /// <summary>
        /// This method will try to make a purchase using the desired payment method 
        /// and after that will dispatch the event. 
        /// 
        /// ||Remember to set the Instance Name to listem to this events)||
        /// 
        /// </summary>
        /// <param name="product">Product to purchase</param>
        /// <param name="paymentMethod">Choosen payment</param>
        public static void BuyItem(M_GenericItem product, string paymentMethod)
        {
            ShopArgumentHandler status = new ShopArgumentHandler();
            if (product != null)
            {
                int productValue = M_ItemDataManager.GetProductValue(product, paymentMethod);
                if (productValue == -1)
                {
                    status = new ShopArgumentHandler(ShopArgument.Error, 1, string.Concat(paymentMethod, " doesn't exist for ", product.NameToString()));
                    IS_InstanceManager.GetInstanceByID("ShopManager").DispatchEvent(M_ShopEvents.ON_FAIL);
                }
                else
                {
                    if (PlayerPrefs.GetInt("MUP_"+paymentMethod) >= productValue)
                    {
                        PlayerPrefs.SetInt("MUP_" + paymentMethod, PlayerPrefs.GetInt("MUP_" + paymentMethod) - productValue);
                        print(PlayerPrefs.GetInt("MUP_" + paymentMethod));
                        status = new ShopArgumentHandler(ShopArgument.Success, 0, string.Concat("Bought (NAME:", product.NameToString(), "|ID: " + product.Id));
                        IS_InstanceManager.GetInstanceByID("ShopManager").DispatchEvent(M_ShopEvents.ON_SUCCESS, product);
                    }
                    else
                    {
                        status = new ShopArgumentHandler(ShopArgument.Fail, 0, string.Concat("Not Enought ", paymentMethod, " to buy (NAME:", product.NameToString(), "|ID: " + product.Id, ") Need: ", productValue, "| Have: ", PlayerPrefs.GetInt("MUP_"+paymentMethod)));
                        IS_InstanceManager.GetInstanceByID("ShopManager").DispatchEvent(M_ShopEvents.ON_FAIL);
                    }
                }
            }
            else
            {
                status = new ShopArgumentHandler(ShopArgument.Error, 0, "Item doesn't exist in database.");
                IS_InstanceManager.GetInstanceByID("ShopManager").DispatchEvent(M_ShopEvents.ON_FAIL);
            }
            Debug.Log(status.ToString());
        }

        /// <summary>
        /// Reward the item to the player.
        /// </summary>
        private void RewardItem(string ItemName)
        {
            M_GenericItem itemToReward = M_ItemDataManager.GetProductInfo(ItemName);
            ShopArgumentHandler status = new ShopArgumentHandler();
            if (itemToReward == null)
            {
                status = new ShopArgumentHandler(ShopArgument.Error, 0, string.Concat(itemToReward + " doesn't exist in database."));
                IS_InstanceManager.GetInstanceByID("ShopManager").DispatchEvent(M_ShopEvents.ON_FAIL);
            }
            else
            {
                status = new ShopArgumentHandler(ShopArgument.Success, 1, string.Concat("Rewarded (NAME:", itemToReward.NameToString(), "|ID: " + itemToReward.Id));
                IS_InstanceManager.GetInstanceByID("ShopManager").DispatchEvent(M_ShopEvents.ON_SUCCESS, itemToReward);
            }
            Debug.Log(status.ToString());
        }

        /// <summary>
        /// Not yet implemented.
        /// </summary>
        /// <param name="product">Product to sell</param>
        public static void SellItem(M_GenericItem product)
        {

        }
    }
}