using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(UnityEngine.UI.Button), typeof(Image))]
public class Button : MonoBehaviour
{
    [SerializeField] private Buttons _id;
    [SerializeField] private bool _isHidden = false;
    [SerializeField] private List<ButtonVariant> _buttonVariants;
    private int _currentButtonVariant;
    private Animator _animator;
    private TMPro.TextMeshProUGUI _text;
    private Image _imageIcon;
    private Image _imageBackground;
    private Image _imageOutline;
    private RectTransform _childLayoutGroup;
    private bool _isActive = true;
    private UnityEngine.UI.Button _button;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        _imageOutline = GetComponent<Image>();
        _imageBackground = GetComponentsInChildren<Image>()[1];
        _imageIcon = GetComponentsInChildren<Image>()[3];
        _childLayoutGroup = GetComponentsInChildren<RectTransform>()[3];
        _button = GetComponent<UnityEngine.UI.Button>();

        if (GetVariantCount() == 0)
        {
            ButtonVariant variant = new ButtonVariant(_text.text, _imageIcon.sprite);
            _buttonVariants.Add(variant);
        }
        SetVariant(0);

        if (_imageIcon.sprite == null) _imageIcon.gameObject.SetActive(false);
        if (_text.text == "") _text.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        StartCoroutine(Layout());
    }

    IEnumerator Layout()
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_childLayoutGroup);
    }

    public int GetID()
    {
        return (int) _id;
    }

    public bool IsHidden()
    {
        return _isHidden;
    }

    public void SetTrigger(string trigger)
    {
        _animator.SetTrigger(trigger);
    }

    private void SetText(string text)
    {
        if (!_text) return;
        _text.text = text;
    }

    private void SetTextColor(Color color)
    {
        if (!_text) return;
        _text.color = color;
    }

    private void SetTextAlpha(float alpha)
    {
        if (!_text) return;
        Color color = _text.color;
        Mathf.Clamp(alpha, 0f, 1f);
        color.a = alpha;
        SetTextColor(color);
    }

    private void SetIcon(Sprite sprite)
    {
        if (!_imageIcon) return;
        if (!sprite)
        {
            if (_imageIcon.gameObject.activeSelf) _imageIcon.gameObject.SetActive(false);
            return;
        }

        if (!_imageIcon.gameObject.activeSelf) _imageIcon.gameObject.SetActive(true);
        _imageIcon.sprite = sprite;
    }

    private void SetIconColor(Color color)
    {
        SetImageColor(color, _imageIcon);
    }

    private void SetIconAlpha(float alpha)
    {
        SetImageAlpha(alpha, _imageIcon);
    }

    private void SetBackgroundColor(Color color)
    {
        SetImageColor(color, _imageBackground);
    }

    private void SetBackgroundAlpha(float alpha)
    {
        SetImageAlpha(alpha, _imageBackground);
    }

    private void SetOutlineColor(Color color)
    {
        SetImageColor(color, _imageOutline);
    }

    private void SetOutlineAlpha(float alpha)
    {
        SetImageAlpha(alpha, _imageOutline);
    }

    private void SetImageColor(Color color, Image image)
    {
        if (!image) return;
        image.color = color;
    }

    private void SetImageAlpha(float alpha, Image image)
    {
        if (!image) return;
        Color color = image.color;
        Mathf.Clamp(alpha, 0f, 1f);
        color.a = alpha;
        SetImageColor(color, image);
    }

    public int GetVariantCount()
    {
        return _buttonVariants.Count;
    }

    public int GetCurrentVariant()
    {
        return _currentButtonVariant;
    }

    public void SetVariant(int id)
    {
        if (id >= GetVariantCount()) return;

        _currentButtonVariant = id;
        ButtonVariant variant = _buttonVariants[_currentButtonVariant];

        SetText(variant.text);
        SetIcon(variant.icon);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_childLayoutGroup); // the layout groups tend to not update that greatly :(
    }

    public void SetActive(bool active)
    {
        if (_isActive == active) return;

        _button.interactable = active;

        float alpha = active ? 1f : 0.5f;
        SetTextAlpha(alpha);
        SetIconAlpha(alpha);
        SetBackgroundAlpha(alpha);
        
        _isActive = active;
    }

    public bool IsActive()
    {
        return _isActive;
    }
}