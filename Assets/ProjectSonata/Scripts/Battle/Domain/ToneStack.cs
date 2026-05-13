using System;
using System.Collections.Generic;
using ProjectSonata.Core.Utilities;
using UnityEngine;

namespace ProjectSonata.Battle.Domain
{
    public class ToneStack
    {
        // This is the base tone that will never be removed from the stack.
        // Just like the Slime in Genshin-Impact that will always have elements on it.
        private OptionalField<Tone> _baseTone;

        // Event handler
        // TODO: Is it necessary to implement an EventBus or Signal-Slot system for better event handling?
        public event Action<ToneStack> OnChange;

        // This class is called a 'stack' but actually a queue-like data structure.
        // It contains at most 3 tones, including the base tone if exists.
        private readonly List<Tone> _tones;

        public ToneStack()
        {
            _tones = new List<Tone>(3);
        }

        public IReadOnlyList<Tone> Tones => _tones;

        public void AddTone(Tone tone)
        {
            if (_tones.Contains(tone))
            {
                Debug.LogWarning($"ToneStack already contains tone {tone}. Skipping adding duplicate tone.");
                return;
            }

            if (_tones.Count >= 3)
            {
                if (_baseTone.enabled)
                {
                    _tones.RemoveAt(1); // Remove the second tone, keep the base tone if exists.
                    Debug.Log("ToneStack is full. Removing the second tone to make room for the new tone.");
                }
                else
                {
                    _tones.RemoveAt(0); // Remove the first tone, since there's no base tone.
                    Debug.Log("ToneStack is full. Removing the first tone to make room for the new tone.");
                }
            }

            _tones.Add(tone);

            OnChange?.Invoke(this);
        }
    }
}
