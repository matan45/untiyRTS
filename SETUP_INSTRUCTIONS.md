# Hex Terrain System - Quick Setup Guide

## Current Status
✅ All terrain scripts have been created and compiled successfully
✅ turnBase scene has the HexGridSystem GameObject with all components
❌ Terrain ScriptableObject assets need to be created

## Why Create Menu Isn't Showing
Unity sometimes needs a domain reload or the Editor to be restarted for new menu items to appear. This is normal.

## Quick Solution: Use the Inspector Context Menu

1. **Open Unity Editor**

2. **Open the turnBase scene** (if not already open)

3. **Select the `HexGridSystem` GameObject** in the Hierarchy

4. **In the Inspector, find the `TerrainAssetSetup` component**
   - If it's not there, add it: Click "Add Component" → search for "TerrainAssetSetup"

5. **Right-click on the component name "Terrain Asset Setup"**

6. **Select "Create All Terrain Assets" from the context menu**

7. **Check the Console** - you should see "✅ All terrain assets created successfully"

8. **Remove the TerrainAssetSetup component** (you won't need it anymore)

## Configure the HexGridManager

Now that assets are created, configure the components:

### 1. HexGridManager
- **Generation Settings**: Drag `Assets/ScriptableObjects/Terrain/DefaultMapSettings` here
- **Generate On Start**: ✓ Check this box

### 2. HexGridRenderer
- **Terrain Tilemap**: Drag the `TerrainTilemap` child GameObject here
- **Fog Of War Tilemap**: Drag the `FogOfWarTilemap` child GameObject here

### 3. PerlinNoiseTerrainGenerator
Assign all the terrain assets from `Assets/ScriptableObjects/Terrain/`:
- **Deep Water Terrain**: DeepWater.asset
- **Water Terrain**: Water.asset
- **Grassland Terrain**: Grassland.asset
- **Plains Terrain**: Plains.asset
- **Hills Terrain**: Hills.asset
- **Mountains Terrain**: Mountains.asset
- **Forest Terrain**: Forest.asset

## Test It!

1. **Press Play** in Unity

2. **You should see**:
   - A hex terrain grid generated in the Scene view
   - Console message: "Terrain generation complete!"
   - Colorful tiles representing different terrain types

3. **Use Scene view** to navigate and see the generated terrain

## Troubleshooting

### "TerrainAssetSetup component not found"
The script needs to compile first. Wait a moment and try adding the component again.

### "Create All Terrain Assets" doesn't appear in context menu
Make sure you're right-clicking on the **component name**, not just anywhere in the Inspector.

### No terrain visible after pressing Play
- Check that HexGridManager has the map settings assigned
- Check that HexGridRenderer has both tilemaps assigned
- Check Console for errors

### Compilation errors
If you see errors about missing namespaces, let me know - we may need to rebuild some files.

## Alternative: Create Assets Via Code

If the context menu doesn't work, you can:

1. Create a new empty GameObject
2. Add the `TerrainAssetSetup` component to it
3. Select the GameObject
4. In Inspector, find `TerrainAssetSetup`
5. Right-click the component → "Create All Terrain Assets"

## What's Next?

Once terrain is generating successfully:
- Integrate with your existing BuildingPlacer for hex-based building placement
- Add unit movement using the HexPathfinder
- Test the Fog of War system with units

See `Assets/Scripts/Terrain/README.md` for complete documentation!
