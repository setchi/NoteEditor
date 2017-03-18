using NoteEditor.DTO;
using NoteEditor.Model;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;

namespace NoteEditor.Presenter
{
    public class SettingsWindowPresenter : MonoBehaviour
    {
        [SerializeField]
        GameObject itemPrefab;
        [SerializeField]
        Transform itemContentTransform;

        static string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Settings");
        static string fileName = "settings.json";
        static string filePath = Path.Combine(directoryPath, fileName);

        string LoadSettingsJson()
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, JsonUtility.ToJson(SettingsDTO.GetDefaultSettings()), System.Text.Encoding.UTF8);
            }

            return File.ReadAllText(filePath, System.Text.Encoding.UTF8);
        }

        void SaveSettings()
        {
            File.WriteAllText(filePath, SettingsSerializer.Serialize(), System.Text.Encoding.UTF8);
        }

        void Awake()
        {
            SettingsSerializer.Deserialize(LoadSettingsJson());

            EditData.MaxBlock.Do(_ => Enumerable.Range(0, itemContentTransform.childCount)
                    .Select(i => itemContentTransform.GetChild(i))
                    .ToList()
                    .ForEach(child => DestroyObject(child.gameObject)))
                .Do(maxNum =>
                {
                    if (Settings.NoteInputKeyCodes.Value.Count < maxNum)
                    {
                        Settings.NoteInputKeyCodes.Value.AddRange(
                            Enumerable.Range(0, maxNum - Settings.NoteInputKeyCodes.Value.Count)
                                .Select(_ => KeyCode.None));
                    }
                })
                .SelectMany(maxNum => Enumerable.Range(0, maxNum))
                .Subscribe(num =>
                {
                    var obj = Instantiate(itemPrefab) as GameObject;
                    obj.transform.SetParent(itemContentTransform);

                    var item = obj.GetComponent<InputNoteKeyCodeSettingsItem>();
                    item.SetData(num, num < Settings.NoteInputKeyCodes.Value.Count ? Settings.NoteInputKeyCodes.Value[num] : KeyCode.None);
                });


            Observable.Merge(
                     Settings.RequestForChangeInputNoteKeyCode.AsUnitObservable(),
                     EditData.MaxBlock.AsUnitObservable(),
                     Settings.WorkSpacePath.AsUnitObservable())
                 .Where(_ => Settings.IsOpen.Value)
                 .DelayFrame(1)
                 .Subscribe(_ => SaveSettings());
        }
    }
}
