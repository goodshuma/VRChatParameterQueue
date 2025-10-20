using UnityEngine;

public class ParameterQueue : MonoBehaviour
{
    [SerializeField]
    int maxQueueSize = 10;
    [SerializeField]
    string parameterName = "Queue";
    [SerializeField]
    QueueType queueType = QueueType.Int;


    enum QueueType
    {
        Int,
        Float
    }
}
