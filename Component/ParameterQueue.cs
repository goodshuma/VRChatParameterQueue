using UnityEditor.Animations;
using UnityEngine;

namespace dev.ReiraLab.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("ReiraLab/Parameter Queue")]
    public class ParameterQueue : MonoBehaviour
    {
        public int maxQueueSize = 10;
        public string parameterName = "Queue";
        public QueueType queueType = QueueType.Int;
        public AnimatorController animatorController;

        public enum QueueType
        {
            Int,
            Float
        }
    }
}
