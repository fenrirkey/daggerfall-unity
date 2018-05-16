﻿using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaggerfallWorkshop.Game
{
    [RequireComponent(typeof(PlayerMotor))]
    public class PlayerSpeedChanger : MonoBehaviour
    {
        private PlayerMotor playerMotor;

        // If checked, the run key toggles between running and walking. Otherwise player runs if the key is held down and walks otherwise
        // There must be a button set up in the Input Manager called "Run"
        public bool toggleRun = false;

        // Daggerfall base speed constants. (courtesy Allofich)
        public const float classicToUnitySpeedUnitRatio = 39.5f; // was estimated from comparing a walk over the same distance in classic and DF Unity
        public const float dfWalkBase = 150f;
        private const float dfCrouchBase = 50f;
        private const float dfRideBase = dfWalkBase + 225f;
        private const float dfCartBase = dfWalkBase + 100f;

        public float walkSpeedOverride = 6.0f;
        public bool useWalkSpeedOverride = false;

        public float runSpeedOverride = 11.0f;
        public bool useRunSpeedOverride = false;

        private void Start()
        {
            playerMotor = GameManager.Instance.PlayerMotor;
        }

        private void Update()
        {

        }

        public void HandleInputSpeedAdjustment(ref float speed)
        {
            if (!playerMotor.IsRiding && playerMotor.IsGrounded)
            {
                if (InputManager.Instance.HasAction(InputManager.Actions.Run))
                {
                    try
                    {
                        // If running isn't on a toggle, then use the appropriate speed depending on whether the run button is down
                        if (!toggleRun)
                            speed = GetRunSpeed(speed);
                        else
                            speed = (speed == GetBaseSpeed() ? GetRunSpeed(speed) : GetBaseSpeed());
                    }
                    catch
                    {
                        speed = GetRunSpeed(speed);
                    }
                }
                // Handle sneak key. Reduces movement speed to half, then subtracts 1 in classic speed units
                else if(InputManager.Instance.HasAction(InputManager.Actions.Sneak))
                {
                    speed /= 2;
                    speed -= (1 / classicToUnitySpeedUnitRatio);
                }
            }
        }
        /// <summary>
        /// Get LiveSpeed adjusted for swimming, walking, crouching or riding
        /// </summary>
        /// <returns>Speed based on player.Stats.LiveSpeed</returns>
        public float GetBaseSpeed()
        {
            Entity.PlayerEntity player = GameManager.Instance.PlayerEntity;
            float baseSpeed = 0;
            float playerSpeed = player.Stats.LiveSpeed;
            if (playerMotor == null) // fixes null reference bug.
                playerMotor = GameManager.Instance.PlayerMotor;
            if (playerMotor.IsCrouching)
                baseSpeed = (playerSpeed + dfCrouchBase) / classicToUnitySpeedUnitRatio;
            else if (playerMotor.IsRiding)
                baseSpeed = (playerSpeed + dfRideBase) / classicToUnitySpeedUnitRatio;
            else
                baseSpeed = GetWalkSpeed(player);
            return baseSpeed;
        }
        /// <summary>
        /// Get LiveSpeed adjusted for walking
        /// </summary>
        /// <param name="player">the PlayerEntity to use</param>
        /// <returns></returns>
        public float GetWalkSpeed(Entity.PlayerEntity player)
        {
            if (useWalkSpeedOverride == true)
                return walkSpeedOverride;
            else
                return (player.Stats.LiveSpeed + dfWalkBase) / classicToUnitySpeedUnitRatio;
        }
        /// <summary>
        /// Get LiveSpeed adjusted for running
        /// </summary>
        /// <param name="baseSpeed"></param>
        /// <returns></returns>
        public float GetRunSpeed(float baseSpeed)
        {
            if (useRunSpeedOverride)
                return runSpeedOverride;
            Entity.PlayerEntity player = GameManager.Instance.PlayerEntity;
            float runSpeed = baseSpeed * (1.25f + (player.Skills.GetLiveSkillValue(DFCareer.Skills.Running) / 200f));
            return runSpeed;
        }

        /// <summary>
        /// Get LiveSpeed adjusted for swimming
        /// </summary>
        /// <param name="baseSpeed"></param>
        /// <returns></returns>
        public float GetSwimSpeed(float baseSpeed)
        {
            Entity.PlayerEntity player = GameManager.Instance.PlayerEntity;
            float swimSpeed = (baseSpeed * (player.Skills.GetLiveSkillValue(DFCareer.Skills.Swimming) / 200f)) + (baseSpeed / 4);
            return swimSpeed;
        }
    }
}