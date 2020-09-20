using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mup.M_Sprites
{
    public class M_SpriteLoader : MonoBehaviour
    {
        static private M_SpriteLoader _instance;
        static public M_SpriteLoader Instance {
            get
            {
                if (!_instance)
                    _instance = GameObject.FindObjectOfType<M_SpriteLoader>();

                return _instance;
            }
        }

        [SerializeField] Sprite[] _spriteList;

        public int ArrayLength
        {
            get { return _spriteList.Length; }
        }

        private void Awake()
        {
            _instance = this;
        }

        public Sprite GetSpriteByIndex(int index)
        {
            return _spriteList[index];
        }

        public Sprite GetSpriteByName(string spriteName)
        {
            foreach(Sprite sprite in _spriteList)
            {
                if (sprite.name == spriteName)
                    return sprite;
            }

            return default(Sprite);
        }
    }
}