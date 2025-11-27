using UnityEngine;
using UnityEditor;
using System;

namespace RTS.Terrain.Data.Editor
{
    /// <summary>
    /// Custom editor for TerrainMovementConfigSO.
    /// Provides a button to auto-populate all terrain types with default values.
    /// </summary>
    [CustomEditor(typeof(TerrainMovementConfigSO))]
    public class TerrainMovementConfigSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Populate All Terrain Types with Defaults", GUILayout.Height(30)))
            {
                PopulateDefaults();
            }

            EditorGUILayout.HelpBox(
                "Click the button above to auto-populate all 11 terrain types with recommended default values. " +
                "This will overwrite any existing configuration.",
                MessageType.Info);
        }

        private void PopulateDefaults()
        {
            var config = (TerrainMovementConfigSO)target;
            var terrainDataField = typeof(TerrainMovementConfigSO).GetField("terrainData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (terrainDataField == null)
            {
                Debug.LogError("Could not find terrainData field");
                return;
            }

            // Create default data for all terrain types
            var defaults = new TerrainMovementConfigSO.TerrainMovementData[]
            {
                new() { terrainType = TerrainType.Grassland, movementCost = 1.0f, isPassable = true },
                new() { terrainType = TerrainType.Plains, movementCost = 1.0f, isPassable = true },
                new() { terrainType = TerrainType.Desert, movementCost = 1.5f, isPassable = true },
                new() { terrainType = TerrainType.Tundra, movementCost = 1.5f, isPassable = true },
                new() { terrainType = TerrainType.Snow, movementCost = 2.0f, isPassable = true },
                new() { terrainType = TerrainType.Hills, movementCost = 2.0f, isPassable = true },
                new() { terrainType = TerrainType.Mountains, movementCost = 10.0f, isPassable = false },
                new() { terrainType = TerrainType.Water, movementCost = 10.0f, isPassable = false },
                new() { terrainType = TerrainType.DeepWater, movementCost = 10.0f, isPassable = false },
                new() { terrainType = TerrainType.Forest, movementCost = 1.5f, isPassable = true },
                new() { terrainType = TerrainType.Swamp, movementCost = 2.5f, isPassable = true },
            };

            Undo.RecordObject(config, "Populate Terrain Movement Defaults");
            terrainDataField.SetValue(config, defaults);
            EditorUtility.SetDirty(config);

            Debug.Log("Populated TerrainMovementConfig with default values for all 11 terrain types.");
        }
    }
}
