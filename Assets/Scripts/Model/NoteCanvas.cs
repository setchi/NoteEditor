using NoteEditor.Notes;
using NoteEditor.Utility;
using UniRx;
using UnityEngine;

namespace NoteEditor.Model
{
    public class NoteCanvas : SingletonMonoBehaviour<NoteCanvas>
    {
        ReactiveProperty<float> width_ = new ReactiveProperty<float>();
        ReactiveProperty<float> offsetX_ = new ReactiveProperty<float>();
        ReactiveProperty<float> scaleFactor_ = new ReactiveProperty<float>();
        ReactiveProperty<bool> isMouseOverNotesRegion_ = new ReactiveProperty<bool>();
        ReactiveProperty<bool> isMouseOverWaveformRegion_ = new ReactiveProperty<bool>();
        ReactiveProperty<NotePosition> closestNotePosition_ = new ReactiveProperty<NotePosition>();

        public static ReactiveProperty<float> Width { get { return Instance.width_; } }
        public static ReactiveProperty<float> OffsetX { get { return Instance.offsetX_; } }
        public static ReactiveProperty<float> ScaleFactor { get { return Instance.scaleFactor_; } }
        public static ReactiveProperty<bool> IsMouseOverNotesRegion { get { return Instance.isMouseOverNotesRegion_; } }
        public static ReactiveProperty<bool> IsMouseOverWaveformRegion { get { return Instance.isMouseOverWaveformRegion_; } }
        public static ReactiveProperty<NotePosition> ClosestNotePosition { get { return Instance.closestNotePosition_; } }

        void Awake()
        {
            this.ObserveEveryValueChanged(_ => Screen.width)
                .DistinctUntilChanged()
                .Subscribe(w => ScaleFactor.Value = 1280f / w);
            // .Subscribe(w => NoteCanvas.ScaleFactor.Value = canvasScaler.referenceResolution.x / w);
        }
    }
}
