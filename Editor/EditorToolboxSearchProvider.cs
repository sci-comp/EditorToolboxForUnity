using System.Collections.Generic;
using UnityEditor.Search;

namespace EditorToolbox
{
    static class CommandsSearchProvider
    {
        public static string id = "EditorCommands";

        private static readonly Dictionary<string, System.Action> commands = new()
        {
            // Commands
            { "Hide Scene View Gizmos", EditorCommands.HideSceneViewGizmos },
            { "Lock Inspector", EditorCommands.LockInspector },
            { "Screenshot", EditorCommands.Screenshot },
            // Scene
            { "Alphabetize Children", EditorCommands.AlphabetizeChildren },
            { "Center To Geometry", EditorCommands.CenterToGeometry },
            { "Destroy Colliders", EditorCommands.DestroyColliders },
            { "Move All Colliders To Parent", EditorCommands.MoveAllCollidersToParent },
            { "Move Colliders On Top Child To Parent", EditorCommands.MoveCollidersOnTopChildToParent },
            { "Move Components On Top Child To Parent", EditorCommands.MoveComponentsOnTopChildToParent },
            { "Paste Component As New", EditorCommands.PasteComponentAsNew },
            { "Push Children To Grid", EditorCommands.PushChildrenToGrid },
            { "Rename To Prefab Name", EditorCommands.RenameToPrefabName },
            { "ResetCategory Transform Position", EditorCommands.ResetCategoryTransformPosition },
            { "ResetCategory Transform Rotation", EditorCommands.ResetCategoryTransformRotation },
            { "ResetLocal Transform Position", EditorCommands.ResetLocalTransformPosition },
            { "Reverse Sibling Order", EditorCommands.ReverseSiblingOrder },
            { "Select Children", EditorCommands.SelectChildren },
            { "Select Parents", EditorCommands.SelectParents },
            { "Toggle Active Status", EditorCommands.ToggleActiveStatus },
            // Asset
            { "Remove Missing Scripts", EditorCommands.RemoveMissingScripts },
            // Window
            { "Add Prefix Or Suffix", AddPrefixOrSuffix.ShowWindow },
            { "Assign Mesh Collection To Mesh Filter", AssignMeshCollectionToMeshFilters.ShowWindow },
            { "Assign Physic Material To All Colliders", AssignPhysicMaterialToAllColliders.ShowWindow },
            { "Assign Terrain Data And Sync Heightmap", AssignTerrainDataAndSyncHeightmap.ShowWindow },
            { "Capitalize First Letter After Prefix", CapitalizeFirstLetterAfterPrefix.ShowWindow },
            { "Replace Term", ReplaceTerm.ShowWindow },
        };

        [SearchItemProvider]
        public static SearchProvider CreateProvider()
        {
            return new SearchProvider(id, "Commands")
            {
                filterId = "cmd:",
                priority = 9999,
                fetchItems = (context, items, provider) => FetchItems(context, provider),
            };
        }

        private static IEnumerable<SearchItem> FetchItems(SearchContext context, SearchProvider provider)
        {
            foreach (var command in commands)
            {
                if (context.searchQuery.Contains(command.Key))
                {
                    yield return provider.CreateItem(context, command.Key.Replace(" ", ""));
                }
            }
        }

        [SearchActionsProvider]
        public static IEnumerable<SearchAction> ActionHandlers()
        {
            foreach (var command in commands)
            {
                yield return new SearchAction(id, command.Key, null, command.Key + " in selected objects",
                    (SearchItem item) => command.Value.Invoke());
            }
        }
    }
}

