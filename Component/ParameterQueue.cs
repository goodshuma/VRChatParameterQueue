using UnityEngine;
using VRC.SDKBase;

namespace dev.ReiraLab.Runtime
{
    [AddComponentMenu("ReiraLab/Parameter Queue")]
    public class ParameterQueue : MonoBehaviour, IEditorOnly
    {
        public int maxQueueSize = 10;
        public string parameterName = "Queue";
        public QueueType queueType = QueueType.Int;
        public RuntimeAnimatorController animatorController;

        public enum QueueType
        {
            Int,
            Float
        }
    }
}
