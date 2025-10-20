using UnityEditor.Animations;
using UnityEngine;

public class ParameterQueue : MonoBehaviour
{
    [SerializeField]
    int maxQueueSize = 10;
    [SerializeField]
    string parameterName = "Queue";
    [SerializeField]
    QueueType queueType = QueueType.Int;
    [SerializeField]
    AnimatorController animatorController;

    public void GenerateParameterQueue()
    {
        for (int i = 0; i < maxQueueSize; i++)
        {
            string paramName = parameterName + "_" + i.ToString("D3");
            AnimatorControllerParameterType paramType =
                queueType == QueueType.Int
                ? AnimatorControllerParameterType.Int
                : AnimatorControllerParameterType.Float;
            animatorController.AddParameter(paramName, paramType);
        }
    }




    enum QueueType
    {
        Int,
        Float
    }
}
