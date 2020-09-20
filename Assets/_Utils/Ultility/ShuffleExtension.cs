// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shuffle.cs" company="Dauler Palhares">
//  © Copyright Dauler Palhares da Costa Viana 2017.
//          http://github.com/DaulerPalhares
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Ultility
{
    public static class ShuffleExtension
    {
        /// <summary>
        /// Shuffle a Type T Array
        /// </summary>
        /// <param name="array">Array to shuffle</param>
        /// <returns>Suffled array</returns>
        public static T[] Array<T>(T[] array)
        {
            var rnd = new System.Random();

            for (var i = 0; i < array.Length - 1; i++)
            {
                var randomIndex = rnd.Next(i, array.Length);
                var tempItem = array[randomIndex];
                array[randomIndex] = array[i];
                array[i] = tempItem;
            }
            return array;
        }
        /// <summary>
        /// Shuffle a Type T Array
        /// </summary>
        /// <param name="array">Array to shuffle</param>
        /// <param name="seed">Seed to use</param>
        /// <returns>Suffled array</returns>
        public static T[] Array<T>(T[] array, string seed)
        {
            var rnd = new System.Random(seed.GetHashCode());

            for (var i = 0; i < array.Length - 1; i++)
            {
                var randomIndex = rnd.Next(i, array.Length);
                var tempItem = array[randomIndex];
                array[randomIndex] = array[i];
                array[i] = tempItem;
            }
            return array;
        }
        /// <summary>
        /// Shuffle a Type T[,] Array
        /// </summary>
        /// <param name="array">Array to shuffle</param>
        /// <returns>Suffled array</returns>
        public static T[,] Simple<T>(T[,] array)
        {
            var rnd = new System.Random();
            var lengthRow = array.GetLength(1);

            for (var i = array.Length - 1; i > 0; i--)
            {
                var i0 = i / lengthRow;
                var i1 = i % lengthRow;

                var j = rnd.Next(i + 1);
                var j0 = j / lengthRow;
                var j1 = j % lengthRow;

                var temp = array[i0, i1];
                array[i0, i1] = array[j0, j1];
                array[j0, j1] = temp;
            }
            return array;
        }
        /// <summary>
        /// Shuffle a Type T[,] Array
        /// </summary>
        /// <param name="array">Array to shuffle</param>
        /// <param name="seed">Seed to use</param>
        /// <returns>Suffled array</returns>
        public static T[,] Simple<T>(T[,] array,string seed)
        {
            var rnd = new System.Random(seed.GetHashCode());
            var lengthRow = array.GetLength(1);

            for (var i = array.Length - 1; i > 0; i--)
            {
                var i0 = i / lengthRow;
                var i1 = i % lengthRow;

                var j = rnd.Next(i + 1);
                var j0 = j / lengthRow;
                var j1 = j % lengthRow;

                var temp = array[i0, i1];
                array[i0, i1] = array[j0, j1];
                array[j0, j1] = temp;
            }
            return array;
        }
    }
}
