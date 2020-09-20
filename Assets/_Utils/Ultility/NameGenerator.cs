// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameGenerator.cs" company="Dauler Palhares">
//  © Copyright Dauler Palhares da Costa Viana 2017.
//          http://github.com/DaulerPalhares
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultility
{
    public static class NameGenerator
    {
        /// <summary>
        /// Generate new Living Creature
        /// </summary>
        /// <returns>Return a name</returns>
        public static string LivingCreatureName()
        {
            var pattern = Random.Range(0, 100);
            if (pattern >= 0 && pattern < 40)
            {
                return StartName() + NameEnd();
            }
            else if (pattern >= 40 && pattern < 50)
            {
                return NameVowel() + NameLink() + NameEnd();
            }
            else if (pattern >= 50 && pattern < 60)
            {
                return StartName() + NameVowel() + NameLink() + NameEnd();
            }
            else if (pattern >= 60 && pattern < 70)
            {
                return NameVowel() + NameLink() + NameVowel() + NameLink() + NameEnd();
            }
            else if (pattern >= 70 && pattern < 80)
            {
                return StartName() + NameVowel() + NameLink() + NameVowel() + NameLink() + NameEnd();
            }
            else if (pattern >= 80 && pattern < 90)
            {
                return NameVowel() + NameLink() + NameVowel() + NameLink() + NameVowel() + NameLink() + NameEnd();
            }
            else if (pattern >= 90 && pattern < 100)
            {
                return StartName() + NameVowel() + NameLink() + NameVowel() + NameLink() + NameVowel() + NameLink() +
                       NameEnd();
            }
            return null;
        }
        /// <summary>
        /// Generate new City name
        /// </summary>
        /// <returns></returns>
        public static string CityName()
        {
            var pattern = Random.Range(0, 100);
            if (pattern >= 0 && pattern < 20)
            {
                return StartName() + CityNameEnd();
            }
            else if (pattern >= 20 && pattern < 40)
            {
                return NameVowel() + NameLink() + CityNameEnd();
            }
            else if (pattern >= 40 && pattern < 60)
            {
                return StartName() + NameVowel() + NameLink() + CityNameEnd();
            }
            else if (pattern >= 60 && pattern < 80)
            {
                return NameVowel() + NameLink() + NameVowel() + NameLink() + CityNameEnd();
            }
            else if (pattern >= 80 && pattern < 100)
            {
                return StartName() + NameVowel() + NameLink() + NameVowel() + NameLink() + CityNameEnd();
            }
            return null;
        }
        #region Generator library Creatures
        private static string StartName()
        {
            var startName = new[,]
            {
            {"B", "C", "D", "F", "G", "H", "J", "K", "L", "M", "N", "P", "R", "S", "T", "V", "W", "X", "Z"},
            {"B", "C", "Ch", "D", "F", "G", "K", "P", "Ph", "S", "T", "V", "Z", "R", "L", "", "", "", ""},
            {"Ch", "St", "Th", "Ct", "Ph", "Qu", "Squ", "Sh", "", "", "", "", "", "", "", "", "", "", ""}
        };
            var a = Random.Range(0, startName.GetLength(0));
            for (var i = 0; i < 1; i++)
            {
                var b = Random.Range(0, startName.GetLength(1));
                if (startName[a, b] == "")
                {
                    i--;
                }
                else
                {
                    return startName[a, b];
                }
            }
            return null;
        }
        private static string NameVowel()
        {
            var nameVowel = new[,]
            {
            {"a", "e", "i", "o", "u", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""},
            {
                "ao", "ae", "ai", "au", "ay", "eo", "ea", "ei", "ey", "io", "ia", "iu", "oa", "oe", "oi", "ou", "oy",
                "ui",
                "uo", "uy", "ee", "oo"
            }
        };
            var a = Random.Range(0, nameVowel.GetLength(0));
            for (var i = 0; i < 1; i++)
            {
                var b = Random.Range(0, nameVowel.GetLength(1));
                if (nameVowel[a, b] == "")
                {
                    i--;
                }
                else
                {
                    return nameVowel[a, b];
                }
            }
            return null;
        }
        private static string NameLink()
        {
            var nameLink = new[,]
            {
            {
                "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "r", "s", "t", "v", "w", "x", "z", "", "",
                "",
                "", "", "", "", "", "", "", "", "", "", "", ""
            },
            {
                "b", "c", "ch", "d", "f", "g", "k", "p", "ph", "r", "s", "t", "v", "z", "r", "l", "n", "", "", "", "",
                "",
                "", "", "", "", "", "", "", "", "", "", "", ""
            },
            {
                "ch", "rt", "rl", "rs", "rp", "rb", "rm", "st", "th", "ct", "ph", "qu", "tt", "bb", "nn", "mm", "gg",
                "cc",
                "dd", "ff", "pp", "rr", "ll", "vv", "ww", "ck", "squ", "lm", "sh", "wm", "wb", "wt", "lb", "rg"
            }
        };
            var a = Random.Range(0, nameLink.GetLength(0));
            for (var i = 0; i < 1; i++)
            {
                var b = Random.Range(0, nameLink.GetLength(1));
                if (nameLink[a, b] == "")
                {
                    i--;
                }
                else
                {
                    return nameLink[a, b];
                }
            }
            return null;
        }
        private static string NameEnd()
        {
            var nameEnd = new[]
            {
            "id", "ant", "on", "ion", "an", "in", "at", "ate", "us", "oid", "aid", "al", "ark", "ork", "irk", "as",
            "os", "e", "o", "a", "y", "or", "ore", "es", "ot", "at", "ape", "ope", "el", "er", "ex", "ox", "ax", "ie",
            "eep", "ap", "op", "oop", "aut", "ond", "ont", "oth"
        };
            var a = Random.Range(0, nameEnd.Length);
            return nameEnd[a];
        }
        private static string CityNameEnd()
        {
            var nameEnd = new[]
                {"ville", "polis", " City", " Village", "town", "port", "boro", "burg", "burgh", "garden", "field", "ness"};
            var a = Random.Range(0, nameEnd.Length);
            return nameEnd[a];
        }
        #endregion
    }
}