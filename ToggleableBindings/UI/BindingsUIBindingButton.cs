﻿#nullable enable

using System;
using TMPro;
using ToggleableBindings.Debugging;
using ToggleableBindings.Extensions;
using ToggleableBindings.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToggleableBindings.UI
{
    public class BindingsUIBindingButton : MonoBehaviour, ISubmitHandler, ICancelHandler, IPointerClickHandler
    {
        internal static FakePrefab Prefab { get; }

        public event Action? Selected;
        public event Action? Canceled;

        public Binding? Binding { get; private set; }

        public bool IsSelected { get; private set; }

        public AudioSource AudioSource { get => _audioSource; set => _audioSource = value; }

        [SerializeField]
        private Text _title = null!;

        [SerializeField]
        private Image _bindingImage = null!;

        [SerializeField]
        private Sprite? _defaultSprite, _selectedSprite;

        [SerializeField]
        private Animator _iconAnimator, _chainAnimator = null!;

        [SerializeField]
        private AudioSource _audioSource = null!;

        [SerializeField]
        private AudioEvent _selectedSound, _deselectedSound;

        /// <summary>
        /// Creates the prefab for this object.
        /// </summary>
        static BindingsUIBindingButton()
        {
            //! Instantiate template prefab
            var baseGO = ObjectFactory.Instantiate(BaseGamePrefabs.NailButton);

            //! Find children and component vars
            var textGO = baseGO.FindChild("Text");
            var imageGO = baseGO.FindChild("Image");
            var chainAnimGO = baseGO.FindChild("Chain_Anim");

            //! Assert the rest
            Assert.IsNotNull(textGO);
            Assert.IsNotNull(imageGO);
            Assert.IsNotNull(chainAnimGO);

            //! Add components
            var button = baseGO.AddComponent<BindingsUIBindingButton>();

            //! Set variables
            var vanillaButton = baseGO.GetComponent<BossDoorChallengeUIBindingButton>();
            button._selectedSound = vanillaButton.selectedSound;
            button._deselectedSound = vanillaButton.deselectedSound;

            button._title = textGO.GetComponent<Text>();
            button._title.text = "???";

            button._bindingImage = imageGO.GetComponent<Image>();
            button._iconAnimator = imageGO.GetComponent<Animator>();
            button._chainAnimator = chainAnimGO.GetComponent<Animator>();

            //! Remove components
            textGO.RemoveComponent<AutoLocalizeTextUI>();
            baseGO.RemoveComponent<BossDoorChallengeUIBindingButton>();

            //! Create prefab and destroy temporary object
            Prefab = new FakePrefab(baseGO, nameof(BindingsUIBindingButton));
            DestroyImmediate(baseGO, true);
        }

        private void Awake()
        {
            GetComponent<MenuButton>().DontPlaySelectSound = true;
        }

        private void Start()
        {
            Assert.IsNotNull(_audioSource);

            UpdateState(false);
        }

        public void Setup(Binding binding)
        {
            Binding = binding ?? throw new ArgumentNullException(nameof(binding));
            _defaultSprite = binding.DefaultSprite != null ? binding.DefaultSprite : Binding.UnknownDefault;
            _selectedSprite = binding.SelectedSprite != null ? binding.SelectedSprite : Binding.UnknownSelected;
            _title.text = binding.Name.ToUpper();

            IsSelected = binding.IsApplied;
        }

        private void UpdateState(bool playEffects)
        {
            float startTime = playEffects ? 0f : 1f;
            if (_bindingImage)
            {
                _bindingImage.sprite = IsSelected ? _selectedSprite : _defaultSprite;
                _bindingImage.SetNativeSize();
            }

            if (_chainAnimator)
                _chainAnimator.Play(IsSelected ? "Bind" : "Unbind", -1, startTime);

            if (_iconAnimator)
                _iconAnimator.Play("Select", -1, startTime);

            if (playEffects)
            {
                GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");

                if (_audioSource)
                {
                    AudioEvent selectSound = IsSelected ? _selectedSound : _deselectedSound;
                    _audioSource.PlayOneShot(selectSound.Clip);
                }
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            var canBeApplied = ToggleableBindings.EnforceBindingRestrictions
                ? (Binding?.CanBeApplied() ?? false)
                : true;

            if (canBeApplied.Value)
            {
                IsSelected = !IsSelected;

                UpdateState(true);
                Selected?.Invoke();
            }
            else
            {
                var applyMsgGO = ObjectFactory.Instantiate(CustomPrefabs.BindingApplyMsg);
                var tmpText = applyMsgGO.FindChild("Text").GetComponent<TextMeshPro>();
                tmpText.text = canBeApplied.Information ?? tmpText.text;
            }
        }

        public void OnCancel(BaseEventData eventData)
        {
            Canceled?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSubmit(eventData);
        }
    }
}