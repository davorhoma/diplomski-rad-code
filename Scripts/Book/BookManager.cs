using Mirror.BouncyCastle.Crypto.Prng;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

[System.Serializable]
public enum BookLanguage { ENGLISH = 0, FRENCH = 1, GERMAN = 2, ITALIAN = 3, SPANISH = 4, PORTUGUESE = 5, DUTCH = 6, RUSSIAN = 7 };

public class BookManager : MonoBehaviour
{
    [SerializeField] private Animator _rightPageAnimator;
    private BookLanguage _currentLanguage;

    [SerializeField] private Image[] _currentImages;
    [SerializeField] private Sprite[] _englishSprites;
    [SerializeField] private Sprite[] _frenchSprites;
    [SerializeField] private Sprite[] _germanSprites;
    [SerializeField] private Sprite[] _italianSprites;
    [SerializeField] private Sprite[] _spanishSprites;
    [SerializeField] private Sprite[] _portugueseSprites;
    [SerializeField] private Sprite[] _dutchSprites;
    [SerializeField] private Sprite[] _russianSprites;

    [SerializeField] private Image _secondImage;
    private int _nextSpriteIndex = 2;

    private void Start()
    {
        AssignLanguage();
    }

    private void OnEnable()
    {
        _rightPageAnimator.Play("Empty", 0, 0f);
        _nextSpriteIndex = 2;
        AssignLanguage();
    }


    public void PreviousPage()
    {
        if (_rightPageAnimator.GetCurrentAnimatorStateInfo(0).IsName("NextPage"))
        {
            _rightPageAnimator.ResetTrigger("TrNextPage");
            _rightPageAnimator.SetTrigger("TrPrevPage");
        }
    }

    public void NextPage()
    {
        var stateInfo = _rightPageAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Empty") || stateInfo.IsName("PrevPage"))
        {
            _rightPageAnimator.ResetTrigger("TrPrevPage");
            _rightPageAnimator.SetTrigger("TrNextPage");
        }
    }

    public void ChangeLanguage(string languageStr)
    {
        if (!Enum.TryParse(languageStr, out _currentLanguage)) return;

        AssignLanguage();
    }

    private void AssignLanguage()
    {
        int firstImageIndex = 0;
        int secondImageIndex = _rightPageAnimator.GetCurrentAnimatorStateInfo(0).IsName("NextPage") ? 2 : 1;
        int lastImageIndex = 3;

        switch (_currentLanguage)
        {
            case BookLanguage.ENGLISH:
                _currentImages[0].sprite = _englishSprites[firstImageIndex];
                _currentImages[1].sprite = _englishSprites[secondImageIndex];
                _currentImages[2].sprite = _englishSprites[lastImageIndex];
                break;
            case BookLanguage.FRENCH:
                _currentImages[0].sprite = _frenchSprites[firstImageIndex];
                _currentImages[1].sprite = _frenchSprites[secondImageIndex];
                _currentImages[2].sprite = _frenchSprites[lastImageIndex];
                break;
            case BookLanguage.GERMAN:
                _currentImages[0].sprite = _germanSprites[firstImageIndex];
                _currentImages[1].sprite = _germanSprites[secondImageIndex];
                _currentImages[2].sprite = _germanSprites[lastImageIndex];
                break;
            case BookLanguage.ITALIAN:
                _currentImages[0].sprite = _italianSprites[firstImageIndex];
                _currentImages[1].sprite = _italianSprites[secondImageIndex];
                _currentImages[2].sprite = _italianSprites[lastImageIndex];
                break;
            case BookLanguage.SPANISH:
                _currentImages[0].sprite = _spanishSprites[firstImageIndex];
                _currentImages[1].sprite = _spanishSprites[secondImageIndex];
                _currentImages[2].sprite = _spanishSprites[lastImageIndex];
                break;
            case BookLanguage.PORTUGUESE:
                _currentImages[0].sprite = _portugueseSprites[firstImageIndex];
                _currentImages[1].sprite = _portugueseSprites[secondImageIndex];
                _currentImages[2].sprite = _portugueseSprites[lastImageIndex];
                break;
            case BookLanguage.DUTCH:
                _currentImages[0].sprite = _dutchSprites[firstImageIndex];
                _currentImages[1].sprite = _dutchSprites[secondImageIndex];
                _currentImages[2].sprite = _dutchSprites[lastImageIndex];
                break;
            case BookLanguage.RUSSIAN:
                _currentImages[0].sprite = _russianSprites[firstImageIndex];
                _currentImages[1].sprite = _russianSprites[secondImageIndex];
                _currentImages[2].sprite = _russianSprites[lastImageIndex];
                break;
        }
    }

    public void OnHalfRotation()
    {
        switch (_currentLanguage)
        {
            case BookLanguage.ENGLISH:
                _secondImage.sprite = _englishSprites[_nextSpriteIndex];
                break;
            case BookLanguage.FRENCH:
                _secondImage.sprite = _frenchSprites[_nextSpriteIndex];
                break;
            case BookLanguage.GERMAN:
                _secondImage.sprite = _germanSprites[_nextSpriteIndex];
                break;
            case BookLanguage.ITALIAN:
                _secondImage.sprite = _italianSprites[_nextSpriteIndex];
                break;
            case BookLanguage.SPANISH:
                _secondImage.sprite = _spanishSprites[_nextSpriteIndex];
                break;
            case BookLanguage.PORTUGUESE:
                _secondImage.sprite = _portugueseSprites[_nextSpriteIndex];
                break;
            case BookLanguage.DUTCH:
                _secondImage.sprite = _dutchSprites[_nextSpriteIndex];
                break;
            case BookLanguage.RUSSIAN:
                _secondImage.sprite = _russianSprites[_nextSpriteIndex];
                break;
        }

        _nextSpriteIndex = _nextSpriteIndex == 2 ? 1 : 2;
    }

    public void BackToStart()
    {
        gameObject.SetActive(false);
    }
}
