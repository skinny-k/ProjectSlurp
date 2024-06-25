using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HapticsManager : MonoBehaviour
{
    public static HapticsManager Instance;
    static Gamepad _gamepad => Gamepad.current;

    float _currentRumbleStrength = 0f;

    // called a stack because it is generally used as such, but we may need to remove a specific event if it times out, so we need greater access provided by ArrayList
    List<HapticEventInfo> _hapticStack = new List<HapticEventInfo>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    
    public static HapticEventInfo TimedRumble(float strength, float duration)
    {
        HapticEventInfo ev = new HapticEventInfo(strength, duration);
        Instance.AddHapticEvent(ev);

        Instance.StartCoroutine(Instance.EngageRumbleWithDuration(ev));

        return ev;
    }

    public static HapticEventInfo StartRumble(float strength)
    {
        HapticEventInfo ev = new HapticEventInfo(strength, -1f);
        Instance.AddHapticEvent(ev);

        Instance.EngageRumble(ev);

        return ev;
    }

    public static void StopRumble(HapticEventInfo rumble)
    {
        if (rumble.duration == -1)
        {
            Instance.RemoveHapticEvent(rumble);
        }
    }

    private void AddHapticEvent(HapticEventInfo ev)
    {
        _hapticStack.Add(ev);
    }

    private void RemoveHapticEvent(HapticEventInfo ev)
    {
        _hapticStack.Remove(ev);
        ev.Dispose();

        if (_hapticStack.Count != 0)
        {
            // We don't care about the duration -- whatever is handling the other event should end it at the necessary time,
            // so DO NOT put a new event back in the stack!
            EngageRumble(_hapticStack[_hapticStack.Count - 1]);
        }
        else
        {
            _gamepad.ResetHaptics();
        }
    }

    private void EngageRumble(HapticEventInfo ev)
    {
        _gamepad.SetMotorSpeeds(ev.strength, ev.strength);
    }

    private IEnumerator EngageRumbleWithDuration(HapticEventInfo ev)
    {
        if (ev.strength >= _currentRumbleStrength && ev.strength <= 1)
        {
            _gamepad.SetMotorSpeeds(ev.strength, ev.strength);

            yield return new WaitForSeconds(ev.duration);

            _gamepad.ResetHaptics();
            RemoveHapticEvent(ev);
        }
        else yield return new WaitForSeconds(0);
    }

    public class HapticEventInfo : IEquatable<HapticEventInfo>, IDisposable
    {
        public float strength { get; private set; } = 0f;
        public float duration { get; private set; } = 0f;

        internal HapticEventInfo(float strength, float duration)
        {
            this.strength = strength;
            this.duration = duration;
        }

        public bool Equals(HapticEventInfo other)
        {
            return (this.strength == other.strength && this.duration == other.duration);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    void OnDestroy()
    {
       _gamepad.ResetHaptics();
    }
}
