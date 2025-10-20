using UnityEditor.Animations;
using UnityEngine;

namespace dev.ReiraLab.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("ReiraLab/Parameter Queue")]
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

            string layerName = "PQ_" + parameterName;
            for (int i = 0; i < animatorController.layers.Length; i++)
            {
                if (animatorController.layers[i].name == layerName)
                {
                    animatorController.RemoveLayer(i);
                    i--;
                }
            }
            animatorController.AddLayer(layerName);
            AnimatorControllerLayer layer = null;
            for (int i = 0; i < animatorController.layers.Length; i++)
            {
                if (animatorController.layers[i].name == layerName)
                {
                    layer = animatorController.layers[i];
                    break;
                }
            }

            layer.stateMachine = new AnimatorStateMachine();
            layer.stateMachine.name = layerName;
            layer.stateMachine.AddState("Idle", new Vector3(300, 0, 0));

        }

        public enum QueueType
        {
            Int,
            Float
        }
    }
}
