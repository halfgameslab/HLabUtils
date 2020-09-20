using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mup.M_Sprites
{
    [RequireComponent(typeof(Image))]
    [ExecuteInEditMode]
    public class M_SpriteImageExtension : MonoBehaviour
    {
        [SerializeField] private int _spriteIndex = 0;

        private Image _image;

        public Sprite ImageSprite
        {
            set
            {
                if (!_image)
                    _image = GetComponent<Image>();
                _image.sprite = value;
            }
            get { return _image.sprite; }
        }

        public int SpriteIndex
        {
            set
            {
                ImageSprite = M_SpriteLoader.Instance.GetSpriteByIndex(value);
                _spriteIndex = value;
            }
            get { return _spriteIndex; }
        }

        public string SpriteName
        {
            set
            {
                ImageSprite = M_SpriteLoader.Instance.GetSpriteByName(value);
            }
        }

        public void OnEnable()
        {
            _image = this.GetComponent<Image>();
        }
    }
}