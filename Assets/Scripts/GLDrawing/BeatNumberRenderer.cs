using NoteEditor.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.GLDrawing
{
    public class BeatNumberRenderer : SingletonMonoBehaviour<BeatNumberRenderer>
    {
        [SerializeField]
        GameObject beatNumberPrefab;

        List<RectTransform> rectTransformPool = new List<RectTransform>();
        List<Text> textPool = new List<Text>();

        static int size;
        static int currentIndex = 0;

        static public void Draw(Vector3 pos, int number, Transform parent)
        {
            if (currentIndex < size)
            {
                Instance.textPool[currentIndex].gameObject.SetActive(true);
                Instance.rectTransformPool[currentIndex].position = pos;
                Instance.textPool[currentIndex].text = number.ToString();
            }
            else
            {
                var obj = Instantiate(Instance.beatNumberPrefab, pos, Quaternion.identity) as GameObject;
                obj.transform.SetParent(parent);
                Instance.rectTransformPool.Add(obj.GetComponent<RectTransform>());
                Instance.textPool.Add(obj.GetComponent<Text>());
                size++;
            }

            currentIndex++;
        }

        static public void Begin()
        {
            foreach (var go in Instance.textPool.Take(currentIndex).Select(text => text.gameObject))
            {
                go.SetActive(false);
            }

            currentIndex = 0;
        }

        static public void End()
        {
            if (currentIndex * 2 < size)
            {
                foreach (var text in Instance.textPool.Skip(currentIndex + 1))
                {
                    DestroyObject(text.gameObject);
                }

                Instance.rectTransformPool.RemoveRange(currentIndex, size - currentIndex);
                Instance.textPool.RemoveRange(currentIndex, size - currentIndex);
                size = currentIndex;
            }
        }
    }
}
