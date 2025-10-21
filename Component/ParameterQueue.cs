using System.Collections.Generic;
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
            AssetDatabase.AddObjectToAsset(layer.stateMachine, animatorController);
            layer.stateMachine.name = layerName;
            layer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
            animatorController.AddLayer(layer);
            var idleState = AddState(layer.stateMachine, "Idle", new Vector3(300, 0, 0));
            var addQueueStateMachine = layer.stateMachine.AddStateMachine("AddQueue_SM", new Vector3(300, 80, 0));
            {
                var state = AddState(addQueueStateMachine, "Entry", new Vector3(300, -80, 0));
                var transition = TransitionInit(state.AddExitTransition());
                transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "false");
            }
            List<AnimatorStateMachine> addedStateMachines = new List<AnimatorStateMachine>();
            AnimatorStateMachine remStateMachine = null;
            int len = maxQueueSize.ToString().Length;
            {
                int power = (int)Mathf.Pow(10, len - 1);
                int div = maxQueueSize / power;
                int rem = maxQueueSize % power;
                for (int i = 0; i < div; i++)
                {
                    addedStateMachines.Add(
                        addQueueStateMachine.AddStateMachine(
                            (i * power).ToString() + "~" + ((i + 1) * power - 1).ToString()
                            , new Vector3(300, i * 80, 0)));
                    var transition = addQueueStateMachine.AddEntryTransition(addedStateMachines[i]);
                    transition.AddCondition(AnimatorConditionMode.Greater, i * power - 1, parameterName + "_Count");
                    transition.AddCondition(AnimatorConditionMode.Less, (i + 1) * power, parameterName + "_Count");
                }
                if (rem != 0)
                {
                    remStateMachine = addQueueStateMachine.AddStateMachine(
                        (div * power).ToString() + "~" + (maxQueueSize - 1).ToString()
                        , new Vector3(300, div * 80, 0));
                    var transition = addQueueStateMachine.AddEntryTransition(remStateMachine);
                    transition.AddCondition(AnimatorConditionMode.Greater, div * power - 1, parameterName + "_Count");
                    transition.AddCondition(AnimatorConditionMode.Less, maxQueueSize, parameterName + "_Count");
                }
            }
            for (int i = len; i > 2; i--)
            {
                int power = (int)Mathf.Pow(10, i - 1);
                int div = maxQueueSize / power;
                int rem = maxQueueSize % power;
                List<AnimatorStateMachine> tempStateMachines = new List<AnimatorStateMachine>(addedStateMachines);
                addedStateMachines.Clear();
                for (int e = 0; e < div; e++)
                {
                    {
                        var state = AddState(tempStateMachines[e], "Entry", new Vector3(300, -80, 0));
                        var transition = TransitionInit(state.AddExitTransition());
                        transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "false");
                    }
                    for (int u = 0; u < 10; u++)
                    {
                        addedStateMachines.Add(
                            tempStateMachines[e].AddStateMachine(
                                (power * e + u * power / 10).ToString() + "~" + (power * e + (u + 1) * power / 10 - 1).ToString()
                                , new Vector3(300, u * 80, 0)));
                        var transition = tempStateMachines[e].AddEntryTransition(addedStateMachines[e * 10 + u]);
                        transition.AddCondition(AnimatorConditionMode.Greater, power * e + u * power / 10 - 1, parameterName + "_Count");
                        transition.AddCondition(AnimatorConditionMode.Less, power * e + (u + 1) * power / 10, parameterName + "_Count");
                    }
                }
                if (rem != 0)
                {
                    int div2 = rem / (power / 10);
                    rem = rem % (power / 10);
                    {
                        var state = AddState(remStateMachine, "Entry", new Vector3(300, -80, 0));
                        var transition = TransitionInit(state.AddExitTransition());
                        transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "false");
                    }
                    for (int e = 0; e < div2; e++)
                    {
                        addedStateMachines.Add(remStateMachine.AddStateMachine(
                            (div * power + e * power / 10).ToString() + "~" + (div * power + (e + 1) * power / 10 - 1).ToString()
                            , new Vector3(300, e * 80, 0)));
                        var transition = remStateMachine.AddEntryTransition(addedStateMachines[div * 10 + e]);
                        transition.AddCondition(AnimatorConditionMode.Greater, div * power + e * power / 10 - 1, parameterName + "_Count");
                        transition.AddCondition(AnimatorConditionMode.Less, div * power + (e + 1) * power / 10, parameterName + "_Count");
                    }
                    var tempRemStateMachine = remStateMachine.AddStateMachine(
                         (div * power + div2 * power / 10).ToString() + "~" + (maxQueueSize - 1).ToString()
                         , new Vector3(300, div2 * 80, 0));
                    var transition2 = remStateMachine.AddEntryTransition(tempRemStateMachine);
                    remStateMachine = tempRemStateMachine;
                    transition2.AddCondition(AnimatorConditionMode.Greater, div * power + div2 * power / 10 - 1, parameterName + "_Count");
                    transition2.AddCondition(AnimatorConditionMode.Less, maxQueueSize, parameterName + "_Count");
                }
            }
            addedStateMachines.Add(remStateMachine);
            foreach (var addedStateMachine in addedStateMachines)
            {
                var state = AddState(addedStateMachine, "Entry", new Vector3(300, -80, 0));
                var transition = TransitionInit(state.AddExitTransition());
                transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "false");
            }
            for (int i = 0; i < maxQueueSize; i++)
            {
                var addState = AddState(
                    addedStateMachines[i / 10],
                    "InsertTo" + i.ToString("D3"),
                    new Vector3(300, i % 10 * 80, 0));
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
                    driver.parameters.Add(parameter);
                }
                {
                    var parameter = new VRCAvatarParameterDriver.Parameter()
                    {
                        name = parameterName + "_Add",
                        type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                        value = 0f
                    };
                    driver.parameters.Add(parameter);
                }
                {
                    var transition = addedStateMachines[i / 10].AddEntryTransition(addState);
                    transition.AddCondition(AnimatorConditionMode.Equals, i, parameterName + "_Count");
                }
                {
                    var transition = TransitionInit(addState.AddTransition(idleState));
                    transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "false");
                }
            }
            /*
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
                    driver.parameters.Add(parameter);
                }
                {
                    var parameter = new VRCAvatarParameterDriver.Parameter()
                    {
                        name = parameterName + "_Add",
                        type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                        value = 0f
                    };
                    driver.parameters.Add(parameter);
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
            */

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

            AssetDatabase.SaveAssets();

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
