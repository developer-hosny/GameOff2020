﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleTypeSwitch : MonoBehaviour
{
    [Flags]
    public enum VehicleType
    {
        DISABLED = 0,
        JET = 1,
        HOVER = 2,
        CAR = 4,
    }

    public VehicleType vehicleType = VehicleType.JET;

    public RVP.GasMotor             gasMotor;
    public RVP.HoverTankMotor       hoverMotor;

    public RVP.SteeringControl      regularSteer;
    public RVP.HoverTankSteer       hoverSteer;
    public GameObject               hoverWheels;
    public GameObject               regularWheels;

    public RVP.Transmission         transmission;
    public RVP.TireScreech          tireScreech;

    public RVP.VehicleParent        vehicleParent;

    public Transform[] regularWheelTransforms;
    public Transform[] hoverWheelTransforms;
    public Transform[] visualWheelTransforms;

    public float hoverCenterOfGravity = -0.3f;
    public float regularCenterOfGravity = -0.1f;

    private float hoverLerpTarget = 1.0f;
    private float hoverLerpValue = 1.0f;

    private float holdToSwitchTimer = 0.0f;

    public float holdToSwitchDurationSeconds = 1.0f;

    private void Start()
    {
        UnityInputModule.instance.controls.Player.Switch.canceled += context => OnSwitch(); // Switch on release
        UnityInputModule.instance.controls.Player.Switch.canceled += context => { holdToSwitchTimer = 0.0f; };

        // If both Hover and Car have their bits matching, then toggle one. We can only initialize with one enabled. They are mutually exclusive
        if (HasVehicleBit(VehicleType.HOVER) == HasVehicleBit(VehicleType.CAR)) 
        {
            ToggleVehicleBit(VehicleType.HOVER);
        }

        // Don't switch, just initialize
        OnSwitch(false);
    }

    private void Update()
    {
        OnHoldToSwitch();
    }

    private void LateUpdate()
    {
        // Move towards the target value (0-1) and then, generate an ease-in-ease-out curve for the interpolation
        hoverLerpValue = Mathf.MoveTowards(hoverLerpValue, hoverLerpTarget, Time.deltaTime * 3.0f);
        float realHoverLerp = Mathf.SmoothStep(0.0f, 1.0f, hoverLerpValue);

        for (int i = 0; i < visualWheelTransforms.Length; i++)
        {
            visualWheelTransforms[i].position = Vector3.Lerp(regularWheelTransforms[i].position, hoverWheelTransforms[i].position, realHoverLerp);
            visualWheelTransforms[i].rotation = Quaternion.Lerp(regularWheelTransforms[i].rotation, hoverWheelTransforms[i].rotation, realHoverLerp);
        }
    }

    bool HasVehicleBit(VehicleType typeToCheck)
    {
        return (vehicleType & typeToCheck) == typeToCheck;
    }

    void ToggleVehicleBit(VehicleType typeToToggle)
    {
        vehicleType ^= typeToToggle;
    }

    // TODO: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Interactions.html#hold
    void OnHoldToSwitch()
    {
        if (UnityInputModule.instance.controls.Player.Switch.ReadValue<float>() > 0.5f)
        {
            if (holdToSwitchTimer > 1.0f) return; // Force the button to be released before switching again

            holdToSwitchTimer += Time.deltaTime / holdToSwitchDurationSeconds;

            if (holdToSwitchTimer > 1.0f)
            {
                ToggleVehicleBit(VehicleType.JET);

                holdToSwitchTimer = 0.0f;
            }
        }
    }

    void OnSwitch(bool doToggle = true)
    {
        // Exit early if you are a jet. Need to hold to switch to a ground-type first
        if (HasVehicleBit(VehicleType.JET)) return;

        if (doToggle)
        {
            // Swap the bits for the hovercar and the car. Works only if they start staggered, which is enforced
            ToggleVehicleBit(VehicleType.HOVER);    // Toggle the Hover bitmask
            ToggleVehicleBit(VehicleType.CAR);      // Toggle the Car bitmask
        }

        vehicleParent.hover = HasVehicleBit(VehicleType.HOVER);

        hoverLerpTarget = vehicleParent.hover ? 1.0f : 0.0f;

        if (vehicleParent.hover)
        {
            vehicleParent.engine = hoverMotor;

            hoverWheels.SetActive(true);
            hoverMotor.gameObject.SetActive(true);
            hoverSteer.gameObject.SetActive(true);

            regularWheels.SetActive(false);
            gasMotor.gameObject.SetActive(false);
            regularSteer.gameObject.SetActive(false);
            transmission.gameObject.SetActive(false);
            tireScreech.gameObject.SetActive(false);

            vehicleParent.burnoutThreshold = -1;
            vehicleParent.burnoutSpin = 0;
            vehicleParent.holdEbrakePark = false;

            vehicleParent.centerOfMassOffset = Vector3.up * hoverCenterOfGravity;
            vehicleParent.SetCenterOfMass();
        }
        else
        {
            vehicleParent.engine = gasMotor;

            hoverWheels.SetActive(false);
            hoverMotor.gameObject.SetActive(false);
            hoverSteer.gameObject.SetActive(false);

            regularWheels.SetActive(true);
            gasMotor.gameObject.SetActive(true);
            regularSteer.gameObject.SetActive(true);
            transmission.gameObject.SetActive(true);
            tireScreech.gameObject.SetActive(true);

            vehicleParent.burnoutThreshold = 0.9f;
            vehicleParent.burnoutSpin = 5;
            vehicleParent.holdEbrakePark = true;

            vehicleParent.centerOfMassOffset = Vector3.up * -regularCenterOfGravity;
            vehicleParent.SetCenterOfMass();
        }
    }
}