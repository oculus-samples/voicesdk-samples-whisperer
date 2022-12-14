using Facebook.WitAi.Interfaces;
using UnityEngine;

namespace Facebook.WitAi.Events.UnityEventListeners
{
    [RequireComponent(typeof(ITranscriptionEventProvider))]
    public class TranscriptionEventListener : MonoBehaviour, ITranscriptionEvent
    {
        [SerializeField] private WitTranscriptionEvent onPartialTranscription = new();

        [SerializeField] private WitTranscriptionEvent onFullTranscription = new();

        private ITranscriptionEvent _events;

        private ITranscriptionEvent TranscriptionEvents
        {
            get
            {
                if (null == _events)
                {
                    var eventProvider = GetComponent<ITranscriptionEventProvider>();
                    if (null != eventProvider) _events = eventProvider.TranscriptionEvents;
                }

                return _events;
            }
        }

        private void OnEnable()
        {
            var events = TranscriptionEvents;
            if (null != events)
            {
                events.OnPartialTranscription.AddListener(onPartialTranscription.Invoke);
                events.OnFullTranscription.AddListener(onFullTranscription.Invoke);
            }
        }

        private void OnDisable()
        {
            var events = TranscriptionEvents;
            if (null != events)
            {
                events.OnPartialTranscription.RemoveListener(onPartialTranscription.Invoke);
                events.OnFullTranscription.RemoveListener(onFullTranscription.Invoke);
            }
        }

        public WitTranscriptionEvent OnPartialTranscription => onPartialTranscription;
        public WitTranscriptionEvent OnFullTranscription => onFullTranscription;
    }
}
