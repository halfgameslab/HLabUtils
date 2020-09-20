
namespace Mup.ShopSystem.Data
{
    /// <summary>
    /// Class to store the payment ID and the item value.
    /// </summary>
    [System.Serializable]
    public class M_PaymentMethod
    {
        /// <summary>
        /// Currency name.
        /// </summary>
        public string CurrencyName;
        /// <summary>
        /// Item value.
        /// </summary>
        public int Value;
    }

    /// <summary>
    /// Shop argument.
    /// </summary>
    public class ShopArgumentHandler
    {
        private string _description;
        private ShopArgument _type;
        private int _code;

        /// <summary>
        /// Method to get the Shop Arg description.
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            return _description;
        }
        /// <summary>
        /// Method to get the shop arg type.
        /// </summary>
        /// <returns></returns>
        public ShopArgument GetArgType()
        {
            return _type;
        }
        /// <summary>
        /// Method to get the shop arg code.
        /// </summary>
        /// <returns></returns>
        public int GetCode()
        {
            return _code;
        }

        public ShopArgumentHandler()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stype">Argument type</param>
        /// <param name="code">Code</param>
        /// <param name="description">Description</param>
        public ShopArgumentHandler(ShopArgument stype, int code, string description)
        {
            _type = stype;
            _code = code;
            _description = description;
        }
        /// <summary>
        /// Get the argument string.
        /// </summary>
        /// <returns>Type Code: code | description.</returns>
        public override string ToString()
        {
            switch (_type)
            {
                case ShopArgument.Error:
                    return string.Concat("<color=red>[" + _type + "]</color> Code: " + _code + " | " + _description);
                case ShopArgument.Fail:
                    return string.Concat("<color=yellow>[" + _type + "]</color> Code: " + _code + " | " + _description);
                case ShopArgument.Success:
                    return string.Concat("<color=green>[" + _type + "]</color> Code: " + _code + " | " + _description);
                default:
                    return string.Concat(_type + " Code: " + _code + " | " + _description);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public enum ShopArgument
    {
        Error,
        Fail,
        Success,
    }
}