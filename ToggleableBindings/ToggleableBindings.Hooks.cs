﻿#nullable enable

using System;
using ToggleableBindings.VanillaBindings;
using UnityEngine;

namespace ToggleableBindings
{
    public sealed partial class ToggleableBindings
    {
        internal static event Action? MainMenuOrQuit;

        private void AddHooks()
        {
            On.GameManager.Update += GameManager_Update;
            On.GameManager.OnApplicationQuit += GameManager_OnApplicationQuit;
            On.GameManager.ReturnToMainMenu += GameManager_ReturnToMainMenu;
        }

        private void RemoveHooks()
        {
            On.GameManager.Update -= GameManager_Update;
            On.GameManager.OnApplicationQuit -= GameManager_OnApplicationQuit;
            On.GameManager.ReturnToMainMenu -= GameManager_ReturnToMainMenu;
        }

        private System.Collections.IEnumerator GameManager_ReturnToMainMenu(On.GameManager.orig_ReturnToMainMenu orig, GameManager self, GameManager.ReturnToMainMenuSaveModes saveMode, Action<bool> callback)
        {
            MainMenuOrQuit?.Invoke();
            return orig(self, saveMode, callback);
        }

        private void GameManager_OnApplicationQuit(On.GameManager.orig_OnApplicationQuit orig, GameManager self)
        {
            MainMenuOrQuit?.Invoke();
            orig(self);
        }

        private void GameManager_Update(On.GameManager.orig_Update orig, GameManager self)
        {
            orig(self);
            if (Input.GetKeyDown(KeyCode.G))
            {
                Log(self.profileID);
                /*foreach (var (_, binding) in BindingManager.RegisteredBindings)
                    Log(binding.Name + " - Applied: " + binding.IsApplied);
                foreach (var evnt in EventRegister.eventRegister)
                    Log(evnt.Key + ": " + string.Join(", ", evnt.Value.Select(e => e.gameObject.ListHierarchy()).ToArray()));*/
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                var binding = BindingManager.GetRegisteredBinding<NailBinding>();
                if (binding is null)
                {
                    LogError("NailBinding is not registered!");
                    return;
                }

                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                var binding = BindingManager.GetRegisteredBinding<ShellBinding>();
                if (binding is null)
                    return;

                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                var binding = BindingManager.GetRegisteredBinding<SoulBinding>();
                if (binding is null)
                    return;

                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                var binding = BindingManager.GetRegisteredBinding<CharmsBinding>();
                if (binding is null)
                    return;

                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }
            
            if (Input.GetKeyDown(KeyCode.Semicolon))
            {
                var binding = BindingManager.GetRegisteredBinding("SpeedBinding");
                if (binding is null)
                    return;

                if (binding.IsApplied)
                    binding.Restore();
                else
                    binding.Apply();
            }
        }
    }
}