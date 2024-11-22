using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace DGP.UnityTimers.PlayerLoop
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

        public static void InjectSystemAsFirstChildOf(Type parentSystemType, Type newSystemType, PlayerLoopSystem.UpdateFunction updateDelegate)
        {
            if (parentSystemType == null) throw new ArgumentNullException(nameof(parentSystemType));
            if (newSystemType == null) throw new ArgumentNullException(nameof(newSystemType));
            if (updateDelegate == null) throw new ArgumentNullException(nameof(updateDelegate));
            
            var newSystem = new PlayerLoopSystem { type = newSystemType, updateDelegate = updateDelegate };
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            
            if (!TryInjectSubSystem(ref currentPlayerLoop, newSystem, parentSystemType, InjectionPosition.Before))
                throw new ArgumentException($"{parentSystemType.Name} not found in player loop");

            injectedSystems.Add(newSystem);
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }
        
        public static void InjectSystemAsLastChildOf(Type parentSystemType, Type newSystemType, PlayerLoopSystem.UpdateFunction updateDelegate)
        {
            if (parentSystemType == null) throw new ArgumentNullException(nameof(parentSystemType));
            if (newSystemType == null) throw new ArgumentNullException(nameof(newSystemType));
            if (updateDelegate == null) throw new ArgumentNullException(nameof(updateDelegate));
            
            var newSystem = new PlayerLoopSystem { type = newSystemType, updateDelegate = updateDelegate };
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            
            if (!TryInjectSubSystem(ref currentPlayerLoop, newSystem, parentSystemType, InjectionPosition.After))
                throw new ArgumentException($"{parentSystemType.Name} not found in player loop");

            injectedSystems.Add(newSystem);
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }
        
        private static bool TryInjectSubSystem(ref PlayerLoopSystem currentLoop, PlayerLoopSystem newSubsystem, Type insertTarget, InjectionPosition injectionPosition)
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
                var childSystems = currentSubSystems[targetIndex].subSystemList;

                if (childSystems == null) {
                    var singleSubSystem = new PlayerLoopSystem[1] { newSubsystem };
                    currentSubSystems[targetIndex].subSystemList = singleSubSystem;
                    return true;
                }
                
                var newSubSystems = new PlayerLoopSystem[childSystems.Length + 1];
                
                var insertIndex = injectionPosition == InjectionPosition.Before ? 0 : newSubSystems.Length - 1;

                for (int i = 0; i < newSubSystems.Length; i++) {
                    if (i < insertIndex)
                        newSubSystems[i] = childSystems[i];
                    else if (i == insertIndex) {
                        newSubSystems[i] = newSubsystem;
                    }
                    else {
                        newSubSystems[i] = childSystems[i - 1];
                    }
                }

                currentSubSystems[targetIndex].subSystemList = newSubSystems;
                
                return true;
            } else {
                for (var i = 0; i < currentSubSystems.Length; i++) {
                    var subSystem = currentSubSystems[i];
                    
                    if (TryInjectSubSystem(ref subSystem, newSubsystem, insertTarget, injectionPosition)) {
                        currentSubSystems[i] = subSystem;
                        return true;
                    }
                }

                return false;
            }
        }
        
        
        public static void InjectSystemBefore(Type injectBefore, Type newSystemType, PlayerLoopSystem.UpdateFunction updateDelegate)
        {
            if (newSystemType == null) throw new ArgumentNullException(nameof(newSystemType));
            if (updateDelegate == null) throw new ArgumentNullException(nameof(updateDelegate));
            if (injectBefore == null) throw new ArgumentNullException(nameof(injectBefore));
            
            var newSystem = new PlayerLoopSystem { type = newSystemType, updateDelegate = updateDelegate };
            var currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();

            if (!TryInjectSystem(ref currentPlayerLoop, newSystem, injectBefore, InjectionPosition.Before))
                throw new ArgumentException($"{injectBefore.Name} not found in player loop");

            injectedSystems.Add(newSystem);
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }

        private static bool TryInjectSystem(ref PlayerLoopSystem currentLoop, PlayerLoopSystem newSystem, Type insertTarget, InjectionPosition injectionPosition)
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
                        newSubSystems[i] = newSystem;
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
                    
                    if (TryInjectSystem(ref subSystem, newSystem, insertTarget, injectionPosition)) {
                        currentSubSystems[i] = subSystem;
                        return true;
                    }
                }

                return false;
            }
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