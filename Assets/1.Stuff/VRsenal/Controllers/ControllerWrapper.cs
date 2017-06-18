using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControllerWrapper : MonoBehaviour
{
    public static List<ControllerWrapper> allControllers = new List<ControllerWrapper>();

    [Header("If null, uses GetComponent")]
    public SteamVR_TrackedObject trackedObject;
    public SteamVR_TrackedObject TrackedObject
    {
        get
        {
            return trackedObject;
        }
    }

    public SteamVR_Controller.Device Device
    {
        get
        {
            return SteamVR_Controller.Input((int)trackedObject.index);
        }
    }

    /// <summary>
    /// used to update the deltaAxis0 value, for GetDeltaAxis() method.
    /// </summary>
    Vector2 oldAxis0, deltaAxis0;
    
    void Awake()
    {
        if (trackedObject == null)
        {
            trackedObject = GetComponent<SteamVR_TrackedObject>();
        }

        if (!allControllers.Contains(this))
        {
            allControllers.Add(this);
        }

    }

    void Update()
    {
        deltaAxis0 = (GetTouch(ButtonMask.Touchpad) && !GetTouchDown(ButtonMask.Touchpad)) ? GetAxis() - oldAxis0 : Vector2.zero;
        oldAxis0 = GetAxis();
    }

    public enum ButtonMask
    {
        System, // = (1ul << (int)EVRButtonId.k_EButton_System); // reserved
        ApplicationMenu, // = (1ul << (int)EVRButtonId.k_EButton_ApplicationMenu);
        Grip, // = (1ul << (int)EVRButtonId.k_EButton_Grip);
        Axis0, // = (1ul << (int)EVRButtonId.k_EButton_Axis0);
        Axis1, // = (1ul << (int)EVRButtonId.k_EButton_Axis1);
        Axis2, // = (1ul << (int)EVRButtonId.k_EButton_Axis2);
        Axis3, // = (1ul << (int)EVRButtonId.k_EButton_Axis3);
        Axis4, // = (1ul << (int)EVRButtonId.k_EButton_Axis4);
        Touchpad, // = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Touchpad);
        Trigger, // = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Trigger);

    }

    /// <summary>
    /// converts wrapped enum ButtonMask to the SteamVR_Controller.ButtonMask equivalent (for ease of reference)
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    public ulong GetSteamVrControllerButtonMask(ButtonMask mask)
    {
        switch (mask)
        {
        case ButtonMask.System:
            return SteamVR_Controller.ButtonMask.System;

        case ButtonMask.ApplicationMenu:
            return SteamVR_Controller.ButtonMask.ApplicationMenu;

        case ButtonMask.Grip:
            return SteamVR_Controller.ButtonMask.Grip;

        case ButtonMask.Axis0:
            return SteamVR_Controller.ButtonMask.Axis0;

        case ButtonMask.Axis1:
            return SteamVR_Controller.ButtonMask.Axis1;

        case ButtonMask.Axis2:
            return SteamVR_Controller.ButtonMask.Axis2;

        case ButtonMask.Axis3:
            return SteamVR_Controller.ButtonMask.Axis3;

        case ButtonMask.Axis4:
            return SteamVR_Controller.ButtonMask.Axis4;

        case ButtonMask.Touchpad:
            return SteamVR_Controller.ButtonMask.Touchpad;

        case ButtonMask.Trigger:
            return SteamVR_Controller.ButtonMask.Trigger;

        }
        return 0;
    }

    #region Button masks GetTouch and GetPress overrides, wrapped.
    public bool GetTouchDown(ButtonMask button)
    {
        return GetTouchDown(GetSteamVrControllerButtonMask(button));
    }
    public bool GetTouchUp(ButtonMask button)
    {
        return GetTouchUp(GetSteamVrControllerButtonMask(button));
    }
    public bool GetTouch(ButtonMask button)
    {
        return GetTouch(GetSteamVrControllerButtonMask(button));
    }
    public bool GetPressDown(ButtonMask button)
    {
        return GetPressDown(GetSteamVrControllerButtonMask(button));
    }
    public bool GetPressUp(ButtonMask button)
    {
        return GetPressUp(GetSteamVrControllerButtonMask(button));
    }
    public bool GetPress(ButtonMask button)
    {
        return GetPress(GetSteamVrControllerButtonMask(button));
    }
    #endregion

    #region Button masks GetTouch and GetPress, using original SteamVR_Controller.ButtonMask

    /// <summary>
    /// Touch is triggered HALFWAY for Touchpad and Trigger
    /// use SteamVR_Controller.ButtonMask.X which returns ulong.
    /// </summary>
    private bool GetTouchDown(ulong button)
    {
        if (trackedObject == null) return false;
        if (!trackedObject.isValid) return false;
        var device = SteamVR_Controller.Input((int)trackedObject.index);
        return device.GetTouchDown(button);
    }

    /// <summary>
    /// Touch is triggered HALFWAY for Touchpad and Trigger
    /// use SteamVR_Controller.ButtonMask.X which returns ulong.
    /// </summary>
    private bool GetTouchUp(ulong button)
    {
        if (trackedObject == null) return false;
        if (!trackedObject.isValid) return false;
        var device = SteamVR_Controller.Input((int)trackedObject.index);
        return device.GetTouchUp(button);
    }

    /// <summary>
    /// Touch is triggered HALFWAY for Touchpad and Trigger
    /// use SteamVR_Controller.ButtonMask.X which returns ulong.
    /// </summary>
    private bool GetTouch(ulong button)
    {
        if (trackedObject == null) return false;
        if (!trackedObject.isValid) return false;
        var device = SteamVR_Controller.Input((int)trackedObject.index);
        return device.GetTouch(button);
    }

    /// <summary>
    /// Press is triggered when any button is fully pressed
    /// use SteamVR_Controller.ButtonMask.X which returns ulong.
    /// </summary>
    private bool GetPressDown(ulong button)
    {
        if (trackedObject == null) return false;
        if (!trackedObject.isValid) return false;
        var device = SteamVR_Controller.Input((int)trackedObject.index);
        return device.GetPressDown(button);
    }

    /// <summary>
    /// Press is triggered when any button is fully pressed
    /// use SteamVR_Controller.ButtonMask.X which returns ulong.
    /// </summary>
    private bool GetPressUp(ulong button)
    {
        if (trackedObject == null) return false;
        if (!trackedObject.isValid) return false;
        var device = SteamVR_Controller.Input((int)trackedObject.index);
        return device.GetPressUp(button);
    }

    /// <summary>
    /// Press is triggered when any button is fully pressed
    /// use SteamVR_Controller.ButtonMask.X which returns ulong.
    /// </summary>
    private bool GetPress(ulong button)
    {
        if (trackedObject == null) return false;
        if (!trackedObject.isValid) return false;
        var device = SteamVR_Controller.Input((int)trackedObject.index);
        return device.GetPress(button);
    }

    #endregion

    public Vector3 GetControllerVelocity()
    {
        if (Device != null)
        {
            return Device.velocity;
        }
        return Vector3.zero;
    }

    public Vector3 GetControllerAngularVelocity()
    {
        if (Device != null)
        {
            return Device.angularVelocity;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Returns the default axis data (Axis0)
    /// </summary>
    /// <returns></returns>
    public Vector2 GetAxis()
    {
        if (trackedObject == null) return Vector2.zero;
        if (!trackedObject.isValid) return Vector2.zero;
        var device = SteamVR_Controller.Input((int)trackedObject.index);
        return device.GetAxis();

    }

    public Vector2 GetDeltaAxis()
    {
        return deltaAxis0;
    }

    /// <summary>
    /// Axis0 is the touchpad. Axis1.x is the trigger
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    public Vector2 GetAxis(Valve.VR.EVRButtonId axis)
    {
        if (trackedObject == null) return Vector2.zero;
        if (!trackedObject.isValid) return Vector2.zero;
        var device = SteamVR_Controller.Input((int)trackedObject.index);
        return device.GetAxis(axis);

    }

    /// <summary>
    /// Vibrates controller with #amount# intensity (0..1) - just for this frame. 
    /// Use higher level Vibrate_ functions for more complex vibrations
    /// </summary>
    /// <param name="intensity">vibration intensity, between 0 and 1</param>
    /// <param name="axis">which axis to vibrate on. NOT IMPLEMENTED</param>
    public void VibrateRaw(float intensity, Valve.VR.EVRButtonId axis = Valve.VR.EVRButtonId.k_EButton_Axis0)
    {
        if (trackedObject == null) return;
        if (!trackedObject.isValid) return;
        var device = SteamVR_Controller.Input((int)trackedObject.index);
        device.TriggerHapticPulse((ushort)(intensity * 3999), axis);
    }
}
