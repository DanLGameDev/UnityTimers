using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace DGP.UnityTimers.PlayerLoop
{
    internal enum LoopPosition
    {
        Before,
        After,
        FirstChildOf,
        LastChildOf
    }

    public static class PlayerLoopUtils
    {
        internal static bool TryInsert(PlayerLoopSystem loop, Type anchor, LoopPosition position, PlayerLoopSystem system, out PlayerLoopSystem result)
        {
            result = loop;

            var children = loop.subSystemList;
            if (children == null)
                return false;

            var anchorIndex = Array.FindIndex(children, child => child.type == anchor);

            if (anchorIndex >= 0) {
                result.subSystemList = InsertAround(children, anchorIndex, position, system);
                return true;
            }

            for (var i = 0; i < children.Length; i++) {
                if (!TryInsert(children[i], anchor, position, system, out var newChild))
                    continue;

                result.subSystemList = ReplaceAt(children, i, newChild);
                return true;
            }

            return false;
        }

        internal static PlayerLoopSystem RemoveAll(PlayerLoopSystem loop, Type systemType)
        {
            if (loop.subSystemList == null)
                return loop;

            loop.subSystemList = loop.subSystemList
                .Where(child => child.type != systemType)
                .Select(child => RemoveAll(child, systemType))
                .ToArray();

            return loop;
        }

        private static PlayerLoopSystem[] InsertAround(PlayerLoopSystem[] siblings, int anchorIndex, LoopPosition position, PlayerLoopSystem system)
        {
            if (position == LoopPosition.Before)
                return InsertAt(siblings, anchorIndex, system);

            if (position == LoopPosition.After)
                return InsertAt(siblings, anchorIndex + 1, system);

            var anchorSystem = siblings[anchorIndex];
            var children = anchorSystem.subSystemList ?? Array.Empty<PlayerLoopSystem>();
            var index = position == LoopPosition.FirstChildOf ? 0 : children.Length;

            anchorSystem.subSystemList = InsertAt(children, index, system);
            return ReplaceAt(siblings, anchorIndex, anchorSystem);
        }

        private static PlayerLoopSystem[] InsertAt(PlayerLoopSystem[] systems, int index, PlayerLoopSystem system)
        {
            var list = new List<PlayerLoopSystem>(systems);
            list.Insert(index, system);
            return list.ToArray();
        }

        private static PlayerLoopSystem[] ReplaceAt(PlayerLoopSystem[] systems, int index, PlayerLoopSystem system)
        {
            var copy = (PlayerLoopSystem[])systems.Clone();
            copy[index] = system;
            return copy;
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