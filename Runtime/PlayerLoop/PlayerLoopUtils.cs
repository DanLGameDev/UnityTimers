using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace DGP.PlayerLoopTimers.PlayerLoop
{
    public static class PlayerLoopUtils
    {
        private static List<PlayerLoopSystem> injectedSystems = new List<PlayerLoopSystem>();
        
        private enum InjectionPosition
        {
            Before,
            After
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            Application.quitting += ClearInjectedSystems;
        }

        private static void ClearInjectedSystems()
        {
            Application.quitting -= ClearInjectedSystems;
            
            foreach (var loopSystem in injectedSystems)
                TryRemoveSystem(loopSystem.type);

            injectedSystems.Clear();
        }
        
        public static void InjectSystemAfter(Type injectAfter, Type newSystemType, PlayerLoopSystem.UpdateFunction updateDelegate)
        {
            if (newSystemType == null) throw new ArgumentNullException(nameof(newSystemType));
            if (updateDelegate == null) throw new ArgumentNullException(nameof(updateDelegate));
            if (injectAfter == null) throw new ArgumentNullException(nameof(injectAfter));
            
            var newSystem = new PlayerLoopSystem { type = newSystemType, updateDelegate = updateDelegate };
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();

            if (!TryInjectSystem(ref currentPlayerLoop, newSystem, injectAfter, InjectionPosition.After))
                throw new ArgumentException($"{injectAfter.Name} not found in player loop");

            injectedSystems.Add(newSystem);
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }
        
        public static void InsertSystemBefore(Type injectBefore, Type newSystemType, PlayerLoopSystem.UpdateFunction updateDelegate)
        {
            if (newSystemType == null) throw new ArgumentNullException(nameof(newSystemType));
            if (updateDelegate == null) throw new ArgumentNullException(nameof(updateDelegate));
            if (injectBefore == null) throw new ArgumentNullException(nameof(injectBefore));
            
            var newSystem = new PlayerLoopSystem { type = newSystemType, updateDelegate = updateDelegate };
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();

            if (!TryInjectSystem(ref currentPlayerLoop, newSystem, injectBefore, InjectionPosition.After))
                throw new ArgumentException($"{injectBefore.Name} not found in player loop");

            injectedSystems.Add(newSystem);
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }
        
        public static bool TryRemoveSystem(Type systemType)
        {
            if (systemType == null) throw new ArgumentNullException(nameof(systemType));

            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();

            var removedAtleastOne = false;
            var wasRemoved = false;

            do {
                wasRemoved = TryRemoveType(ref currentPlayerLoop, systemType);
                removedAtleastOne = wasRemoved || removedAtleastOne;
            } while (wasRemoved);
            
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            
            return removedAtleastOne;
        }

        private static bool TryRemoveType(ref PlayerLoopSystem currentSystem, Type type, bool removeAll = false)
        {
            var subSystems = currentSystem.subSystemList;
            if (subSystems == null) return false;

            for (int i = subSystems.Length-1; i >= 0; i--) {
                if (subSystems[i].type == type) {
                    var childSystems = new PlayerLoopSystem[subSystems.Length - 1];

                    Array.Copy(subSystems, childSystems, i);
                    Array.Copy(subSystems, i + 1, childSystems, i, subSystems.Length - i - 1);

                    currentSystem.subSystemList = childSystems;

                    return true;
                }

                if (TryRemoveType(ref subSystems[i], type))
                    return true;
            }

            return false;
        }

        private static bool TryInjectSystem(ref PlayerLoopSystem currentLoop, PlayerLoopSystem toInsert, Type insertTarget, InjectionPosition injectionPosition)
        {
            var currentSubSystems = currentLoop.subSystemList;
            if (currentSubSystems == null) return false;

            int targetIndex = -1;
            for (int i = 0; i < currentSubSystems.Length; i++) {
                if (currentSubSystems[i].type == insertTarget) {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex != -1) {
                var newSubSystems = new PlayerLoopSystem[currentSubSystems.Length + 1];

                var insertIndex = injectionPosition == InjectionPosition.Before ? targetIndex : targetIndex + 1;

                for (int i = 0; i < newSubSystems.Length; i++) {
                    if (i < insertIndex)
                        newSubSystems[i] = currentSubSystems[i];
                    else if (i == insertIndex) {
                        newSubSystems[i] = toInsert;
                    }
                    else {
                        newSubSystems[i] = currentSubSystems[i - 1];
                    }
                }

                currentLoop.subSystemList = newSubSystems;
                return true;
            } else {
                for (var i = 0; i < currentSubSystems.Length; i++) {
                    var subSystem = currentSubSystems[i];
                    
                    if (TryInjectSystem(ref subSystem, toInsert, insertTarget, injectionPosition)) {
                        currentSubSystems[i] = subSystem;
                        return true;
                    }
                }

                return false;
            }
        }
       
        
        #region Debug
        public static void PrintPlayerLoop(PlayerLoopSystem playerLoop)
        {
            StringBuilder sb = new();
            sb.AppendLine("Unity PlayerLoopSystem");
            foreach (PlayerLoopSystem subsystem in playerLoop.subSystemList) {
                PrintSubsystems(subsystem, sb, 0);
            }

            Debug.Log(sb.ToString());
        }

        static void PrintSubsystems(PlayerLoopSystem system, StringBuilder sb, int level)
        {
            sb.Append(' ', level * 5).AppendLine(system.type.ToString());
            if (system.subSystemList == null || system.subSystemList.Length == 0) return;

            foreach (PlayerLoopSystem subsystem in system.subSystemList) {
                PrintSubsystems(subsystem, sb, level + 1);
            }
        }
        #endregion
    }
}