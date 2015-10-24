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
        static int prevActiveCount = 0;
        static int currentActiveCount = 0;

        static public void Render(Vector3 pos, int number)
        {
            if (currentActiveCount < size)
            {
                if (currentActiveCount >= prevActiveCount)
                {
                    Instance.textPool[currentActiveCount].gameObject.SetActive(true);
                }

                Instance.rectTransformPool[currentActiveCount].position = pos;
                Instance.textPool[currentActiveCount].text = number.ToString();
            }
            else
            {
                var obj = Instantiate(Instance.beatNumberPrefab, pos, Quaternion.identity) as GameObject;
                obj.transform.SetParent(Instance.transform);
                Instance.rectTransformPool.Add(obj.GetComponent<RectTransform>());
                Instance.textPool.Add(obj.GetComponent<Text>());
                size++;
            }

            currentActiveCount++;
        }

        static public void Begin()
        {
            prevActiveCount = currentActiveCount;
            currentActiveCount = 0;
        }

        static public void End()
        {
            if (currentActiveCount < prevActiveCount)
            {
                for (int i = currentActiveCount; i < prevActiveCount; i++)
                {
                    Instance.textPool[i].gameObject.SetActive(false);
                }
            }

            if (currentActiveCount * 2 < size)
            {
                foreach (var text in Instance.textPool.Skip(currentActiveCount + 1))
                {
                    DestroyObject(text.gameObject);
                }

                Instance.rectTransformPool.RemoveRange(currentActiveCount, size - currentActiveCount);
                Instance.textPool.RemoveRange(currentActiveCount, size - currentActiveCount);
                size = currentActiveCount;
            }
        }
    }
}
