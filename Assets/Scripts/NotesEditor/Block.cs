using UniRx;
using UnityEngine;

public class Block : MonoBehaviour
{
    public ReactiveProperty<int> sample = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> state = new ReactiveProperty<int>(0);
    public int BlockNum = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
