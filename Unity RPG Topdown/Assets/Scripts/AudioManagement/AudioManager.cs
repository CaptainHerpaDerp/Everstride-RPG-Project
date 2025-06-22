using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;
using System.Collections;
using Core;
using Core.Enums;

namespace AudioManagement
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Volume")]
        [Range(0, 1)]
        [SerializeField] private float masterVolume = 1, uiVolume = 1;

        private Bus masterBus, uiBus;

        private List<EventInstance> events;

        protected override void Awake()
        {
            base.Awake();

            events = new List<EventInstance>();

            masterBus = RuntimeManager.GetBus("bus:/");
            uiBus = RuntimeManager.GetBus("bus:/ui");
        }

        private void FixedUpdate()
        {
            SetBusVolume();
        }

        public void PlayOneShot(EventReference sound, Vector3 position)
        {
            RuntimeManager.PlayOneShot(sound, position);
        }

        public EventInstance CreateInstance(EventReference eventReference, Vector3 position)
        {
            EventInstance newEventInstance = RuntimeManager.CreateInstance(eventReference);
            newEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            events.Add(newEventInstance);
            return newEventInstance;
        }

        #region Volume Control

        private void SetBusVolume()
        {
            masterBus.setVolume(masterVolume);
            uiBus.setVolume(uiVolume);
        }

        #endregion

        #region Destroy Handling

        private void CleanUpEventInstances()
        {
            if (events == null)
                return;

            foreach (EventInstance eventInstance in events)
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }
        }

        private void OnDestroy()
        {
            CleanUpEventInstances();
        }

        #endregion
    }
}