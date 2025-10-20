using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

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

        AnimationClip emptyClip;

        public void GenerateParameterQueue()
        {
            emptyClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Animation/ProxyAnim/proxy_empty.anim");

            AssetDatabase.StartAssetEditing();
            AnimatorControllerParameterType paramType =
                queueType == QueueType.Int
                ? AnimatorControllerParameterType.Int
                : AnimatorControllerParameterType.Float;
            for (int i = 0; i < maxQueueSize; i++)
            {
                string paramName = parameterName + "_" + i.ToString("D3");
                AddParameter(animatorController, paramName, paramType);
            }
            AddParameter(animatorController, parameterName + "_AddValue", paramType);
            AddParameter(animatorController, parameterName + "_Add", AnimatorControllerParameterType.Bool);
            AddParameter(animatorController, parameterName + "_Next", AnimatorControllerParameterType.Bool);
            AddParameter(animatorController, parameterName + "_Count", AnimatorControllerParameterType.Int);
            AddParameter(animatorController, "false", AnimatorControllerParameterType.Bool);

            string layerName = "PQ_" + parameterName;
            for (int i = 0; i < animatorController.layers.Length; i++)
            {
                if (animatorController.layers[i].name == layerName)
                {
                    animatorController.RemoveLayer(i);
                    i--;
                }
            }

            AnimatorControllerLayer layer = new AnimatorControllerLayer();
            layer.name = layerName;
            layer.defaultWeight = 1f;
            layer.stateMachine = new AnimatorStateMachine();
            layer.stateMachine.name = layerName;
            var idleState = AddState(layer.stateMachine, "Idle", new Vector3(300, 0, 0));
            var addQueueStateMachine = layer.stateMachine.AddStateMachine("AddQueue_SM", new Vector3(300, 80, 0));
            {
                var state = AddState(addQueueStateMachine, "Entry", new Vector3(300, -80, 0));
                var transition = TransitionInit(state.AddExitTransition());
                transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "false");
            }
            for (int i = 0; i < maxQueueSize; i++)
            {
                var addState = AddState(addQueueStateMachine, "InsertTo" + i.ToString("D3"), new Vector3(300, i * 80, 0));
                var driver = addState.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                {
                    var parameter = new VRCAvatarParameterDriver.Parameter()
                    {
                        name = parameterName + "_" + i.ToString("D3"),
                        type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Copy,
                        source = parameterName + "_AddValue"
                    };
                    driver.parameters.Add(parameter);
                }
                {
                    var parameter = new VRCAvatarParameterDriver.Parameter()
                    {
                        name = parameterName + "_Count",
                        type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                        value = i + 1
                    };
                }
                {
                    var parameter = new VRCAvatarParameterDriver.Parameter()
                    {
                        name = parameterName + "_Add",
                        type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                        value = 0f
                    };
                }
                {
                    var transition = addQueueStateMachine.AddEntryTransition(addState);
                    transition.AddCondition(AnimatorConditionMode.Equals, i, parameterName + "_Count");
                }
                {
                    var transition = TransitionInit(addState.AddTransition(idleState));
                    transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "false");
                }
            }

            {
                var nextState = AddState(layer.stateMachine, "ShiftQueue", new Vector3(550, 80, 0));
                var driver = nextState.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                for (int i = 0; i < maxQueueSize - 1; i++)
                {
                    var parameter = new VRCAvatarParameterDriver.Parameter()
                    {
                        name = parameterName + "_" + i.ToString("D3"),
                        type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Copy,
                        source = parameterName + "_" + (i + 1).ToString("D3")
                    };
                    driver.parameters.Add(parameter);
                }
                {
                    var parameter = new VRCAvatarParameterDriver.Parameter()
                    {
                        name = parameterName + "_Count",
                        type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Add,
                        value = -1
                    };
                    driver.parameters.Add(parameter);
                }
                {
                    var parameter = new VRCAvatarParameterDriver.Parameter()
                    {
                        name = parameterName + "_Next",
                        type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                        value = 0f
                    };
                    driver.parameters.Add(parameter);
                }
                {
                    var transition = TransitionInit(idleState.AddTransition(nextState));
                    transition.AddCondition(AnimatorConditionMode.If, 0f, parameterName + "_Next");
                    transition.AddCondition(AnimatorConditionMode.Greater, -1f, parameterName + "_Count");
                }
                {
                    var transition = TransitionInit(nextState.AddTransition(idleState));
                    transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "false");
                }
            }

            {
                var transition = TransitionInit(idleState.AddTransition(addQueueStateMachine));
                transition.AddCondition(AnimatorConditionMode.If, 0f, parameterName + "_Add");
                transition.AddCondition(AnimatorConditionMode.Less, maxQueueSize, parameterName + "_Count");
            }

            animatorController.AddLayer(layer);

            AssetDatabase.StopAssetEditing();
        }

        public enum QueueType
        {
            Int,
            Float
        }

        public AnimatorState AddState(AnimatorStateMachine stateMachine, string stateName, Vector3 position)
        {
            AnimatorState state = stateMachine.AddState(stateName, position);
            state.writeDefaultValues = false;
            state.motion = emptyClip;
            return state;
        }

        public AnimatorStateTransition TransitionInit(AnimatorStateTransition transition)
        {
            transition.exitTime = 0f;
            transition.hasExitTime = false;
            transition.duration = 0f;
            return transition;
        }

        public static bool ParameterListContaints(AnimatorControllerParameter[] parameters, string name)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        /*
        public static void AddParameter(AnimatorController animatorController, string name, bool defaultValue)
        {
            AddParameter(animatorController, name, AnimatorControllerParameterType.Bool);
            foreach (var parameter in animatorController.parameters)
            {
                if (parameter.name == name)
                {
                    parameter.defaultBool = defaultValue;
                }
            }
        }
        public static void AddParameter(AnimatorController animatorController, string name, int defaultValue)
        {
            AddParameter(animatorController, name, AnimatorControllerParameterType.Int);
            foreach (var parameter in animatorController.parameters)
            {
                if (parameter.name == name)
                {
                    parameter.defaultInt = defaultValue;
                }
            }
        }
        public static void AddParameter(AnimatorController animatorController, string name, float defaultValue)
        {
            AddParameter(animatorController, name, AnimatorControllerParameterType.Float);
            foreach (var parameter in animatorController.parameters)
            {
                if (parameter.name == name)
                {
                    parameter.defaultFloat = defaultValue;
                }
            }
        }
        */
        public static void AddParameter(AnimatorController animatorController, string name, AnimatorControllerParameterType type)
        {
            if (ParameterListContaints(animatorController.parameters, name))
            {
                return;
            }
            animatorController.AddParameter(name, type);
        }
    }
}
