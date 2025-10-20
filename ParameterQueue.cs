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
        AnimatorControllerParameterType paramType =
            queueType == QueueType.Int
            ? AnimatorControllerParameterType.Int
            : AnimatorControllerParameterType.Float;
        for (int i = 0; i < maxQueueSize; i++)
        {
            string paramName = parameterName + "_" + i.ToString("D3");
            animatorController.AddParameter(paramName, paramType);
        }
        animatorController.AddParameter(parameterName + "_AddValue", paramType);
        animatorController.AddParameter(parameterName + "_Add", AnimatorControllerParameterType.Bool);
        animatorController.AddParameter(parameterName + "_Next", AnimatorControllerParameterType.Bool);
        animatorController.AddParameter(parameterName + "_Count", AnimatorControllerParameterType.Int);


    }




    enum QueueType
    {
        Int,
        Float
    }
}
