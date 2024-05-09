# Painting with a wave function collapse brush
> in other words boundless tileset-agnostic graph-based WFC.

TLDR: 
- This is a project about creating a WFC brush.
- An interactive demo is available [here](https://julzerinos.itch.io/wave-function-collapse-brush).


## Motivation

The [wave function collapse (WFC) algorithm](https://github.com/mxgmn/WaveFunctionCollapse) [1] (based on quantum behaviours) is a powerful tool in the field of procedural asset generation, often prized by game developers looking to create game maps with believable structures. The goal of the algorithm is to "pre-generate" assets which are used as a complete product in the creative workflow, although a more flexible, mutable version of the algorithm may be desired to naturally extend the working canvas in any given direction during runtime. 

This could be to support a "wave function collapse" brush in map generation software, allowing artists to quickly fill and modify an unbounded area using WFC as part of a larger handmade image (or map) or as a trigger following a player's movements on an infinitely procedurally generated game map. Furthermore, runtime modification is desired to extend the interactivity with the algorithm, by erasing or overwriting local patches of cells. This could be used further extend the brush behaviour by allowing artists to locally change an undesirable slice of the WFC algorithm output, or as a mechanic in a game which revolves around a dynamically changing map, where patches of the map recently visited by, but now not visible to a player will be rehydrated.

## Graph-based

To enable the unbounded expansion of a WFC canvas a graph-based implementation was chosen. Whereas the most common method of implementation being a two dimensional matrix could potentially support infinite maps with an appropriate amount of neighbouring matrix and wrapping management, a graph allows for uninterrupted growth. The additional side benefit is supporting arbitrary tile shapes and even irregular grids (such Voronoi grids [3]).

The graph implementation is generic with nodes managing their content as a property (WFC cells are not directly nodes of the graph). The graph contains knowledge about the nodes' cardinality (number of neighbours). Neighbours are stored in a node with specific directions (e.g. the northern neighbour is always at neighbour index `0` - if there is no neighbour, then return `null`). This is done to maintain a consistent behaviour with retrieving tile connection data. The graph also supports a reverse look up for retrieving nodes by their content (i.e. cells) - to achieve this functionality, cells support unique hashing based only on their physical position (no two cells can be at a given position). To get around issues with floating point number comparisons, spatial data is compared (in hashing) at a fixed precision.

Expansion of a wave graph is done by graph traversal, described in the pseudo code in Appendix A. Initially a wave graph should be initialized with at least one node to allow for expansion. For every iteration of the node traversal there are three cases which have to be handled, these are
1. A neighbouring node does not exist - it is created and registered as a neighbour.
2. A neighbouring node exists, but is not known to the node - this occurs when two "wavefronts" of the graph collide, the two nodes must be registered as neighbours.
3. A neighbouring node exists and is known to the node - graph traversal continues.

In cases (2) and (3) it is also vital to validate constraints between the nodes. A new node will start with its cell in superposition, but when connecting two unintroduced nodes either of them may not be in total superposition. This is similar to the propagate phase of normal WFC, except constraints are validated in both directions.
## Algorithm input

The input of the wave function algorithm is generated on the basis of a `configuration.json` file appended together with the tile set. An example of such a file is in Appendix B. A basic transformation script is applied to generate all possible rotations for every input tile. The required fields are
- `tiles` - a list of tiles by names, these are indexed in order of loading (usually alphabetically) and represent the order of entries in `types`.
- `probabilities` - an array of probability values (not normalized) for the occurrence of a tile at the `tiles` index being picked during the collapse phase of WFC. This is used to minimize the appearance of a tile (with a probability of `0` being the value for the last possible choice).
- `offsets` - an array of vectors indicating where a given cell's neighbour is in physical space (centre). The order is important as this defines the neighbour direction index used for WFC constraints. The staple is to follow clockwise starting at north (NESW).
- `types` - an array the same length as `tiles`, where each element is an array of the tile's edge "type" in a given direction index (based on `offsets`). Two tiles sharing the same type on their common edge are considered as matching neighbours in WFC.
## Interaction

Unity (`LTS 2022.3.22f1`) and its scripting framework in C# is used for the implementation. This allows for adding basic interaction without much trouble. A map generation script was created for expanding a tile set by clicking on existing tiles (collapsed cells). The camera can be scrolled infinitely in any direction to allow for the infinite expansion of the generated map. Since the map is being manipulated in runtime, a wrapper class is created over the static methods implementing (`WaveFunctionCollapse`) the Wave function collapse algorithm which is referred to as the `WaveFunctionCollapseComputer`. It is the interface between the map generator and the functionalities of WFC which stores the current wave graph. 

Finally, to maintain a steady framerate, updated tiles are directly yielded via iterators and applied to the map, thus eliminating the need for costly middleman copies and iterating over all of the graph nodes. With this, tiles are always updated locally in their patch.

An interactive demo is available [here](https://julzerinos.itch.io/wave-function-collapse-brush).

## Rehydrating patches

Rehydrating refers to "re-visiting" previously generated patches of the canvas. A patch can be erased and subsequently be subjected to WFC again, thus overwriting a patch with new tiles. The effect of this is determined by the size of the patch. For very small patches, the untouched neighboring tiles apply rigid constraints. The more space is provided the stronger the effect of the "amnesia".

Erasing cells is executed with graph traversal (as described in Appendix C) with the goal of resetting cells encountered to their initial total superposition. An important aspect of this script is that cells can only be constrained to neighbouring cells which are not going to reset, otherwise the reset would simply not happen with the first cell being constrained to its originally constraining neighbours. Only existing nodes are taken into consideration. During a reset, no new nodes are added to the wave graph.
## Results

Tile sets can be separated into complete and incomplete sets. Complete tile sets are such which satisfy the condition that there exists a tile (or its transformation) for every possible cell pocket - it is possible to find or transform a tile to match every possible combination of neighbouring tiles. Complete tile sets are comfortable for the WFC algorithm as it is impossible to reach a fail state. M. Gumin introduces a similar notion in their work [1], referring to such tile sets as "easy". Three tile sets were used to test the implementation, these are
- [The Knots tile set](https://github.com/mxgmn/WaveFunctionCollapse/tree/master/tilesets/Knots) used in the original Wave function collapse implementation [1]. The tile set is appended with a "cap" tile (see Appendix D - leftmost tile) to make it a complete tile set.
- [KayKit's Medieval Builder Pack](https://kaylousberg.itch.io/kaykit-medieval-builder-pack) - a hexagonal incomplete tile set (Appendix E).
- [Danae](https://www.linkedin.com/in/anastasia-danai-panagiotopoulou-38a7bb244/)'s procedurally pre-generated hex tiles (generated by `tileset/generator.blend`) using a Blender geometry nodes tile generator - potentially complete, depending on the number of unique tiles generated.

Using the Wave function collapse brush is satisfying, much like unravelling an underlying pattern. Without any rendering optimizations (such as GPU instancing) a low-tier device is able to maintain up to 10000 tiles without suffering framerate issues (Knots tile set). 

The completeness of the Knots tile set is evident with the brush's ability to close any pocket (see Appendix F). The simplicity of the tile set stems from its binary connection property, an edge is either empty or a pipe connector. 

Incomplete tile sets are more difficult and will often result in patches of invalid cells (Appendix G). This is where the reset function comes into play. Since the cells are already defined (exist), resetting them to an initial superposition and subsequently executing WFC is equivalent to original WFC with constraints. Resetting also covers the functionality of redefining an area of the canvas. For instance, due to rigid constraints in the tile set connections, there are two "biomes" in the KayKit tile set - grass roads or sandy beaches. Resetting a patch following one pattern (biome) allows for another neighboring pattern to take over.

A tile set generator can be created with Blender geometry nodes which (via script) exports a large amount of unique (edge-wise) tiles. Experiments with such a tile set can be seen in Appendix H. The script used to export a given set of tiles can be found in Appendix I.
## Further work

While this implementation achieves its goal of creating a boundless wave-based runtime-mutable variant of Wave function collapse there is many possible extensions which could enhance the experience.

Starting with the quality of output. This approach suffers with incomplete tile sets as a wavefront is only locally constrained and may result in a large number of invalid tiles when the collapsing wave is "cornered". This could potentially be alleviated with an implementation of [nested WFC](https://arxiv.org/pdf/2308.07307) [2] to back propagate and fix local context conflicts.

In terms of optimization, graph nodes or cells could be entirely eliminated if they are not going to be used. This will free resources for maintaining relevant nodes. Conversely, a large number of repetitive assets (such as cell tiles) should make use of (GPU) instancing or object pooling.

Support for more interesting grids is a potential extension. This would require each tile to declare its own offset and for the defined connections to encode fit between two irregular tile edges. Another interesting method which could used for this encoding or as a standalone, is bitmask encoding, allowing a single edge type to support more than one other connection type (eg. for pattern transitions).

## References

1. "Wave function collapse algorithm" by M. Gumin, accessed at https://github.com/mxgmn/WaveFunctionCollapse
2. "Extend Wave Function Collapse Algorithm to Large-Scale Content Generation" by Y. Nie, S. Zheng, Z. Zhuang and X. Song (Southern University of Science and Technology), accessed at https://arxiv.org/pdf/2308.07307
3. "Automatic Generation of Game Content using a Graph-based Wave Function Collapse Algorithm" by H. Kim, S. Lee, H. Lee, et al. (NCsoft & Hongik University), accessed at https://www.researchgate.net/publication/336086804_Automatic_Generation_of_Game_Content_using_a_Graph-based_Wave_Function_Collapse_Algorithm
## Appendix

The source code is hosted [at this repository](https://github.com/julzerinos/wave-function-collapse-brush).
### A. Adding cells to graph (pseudo code)

```python
add_cells (cell_count: int, seed_cell: Cell, offsets: vector[]):
	if cell not in wave_graph:
		seed_node = Node(cell)
		wave_graph.add(seed_node)
	seed_node = wave_graph.get_node(seed_cell)

	cell_frontier = Set([seed_cell])
	node_queue = Queue([seed_node])

	while node_queue is not empty:
		node = node_queue.dequeue()

		for (offset, direction_index) in offsets:
			neighbor_position = node.physical_position + offset
			neighbor_cell = Cell(neighbor_position)
			neighbor_node = wave_graph.get_node(neighbor_cell)

			if neighbor_node is null:
				if cell_frontier.count >= cell_count:
					continue

				neighbor_node = Node(neighbor_cell)
				wave_graph.add(neighbor_node)

			if neighbor_node was created or node.neighbors does not contain neighbor_node:
				node.register_neighbor(neighbor_node, direction_index)
				neighbor_node.register_neighbor(neighbor_node, wave_graph.opposite_direction(direction_index))

			if neighbor_cell is not in cell_frontier and (cell_frontier.count >= cell_count or neighbor_node was created):
				cell_frontier.add(neighbor_cell)
				node_queue.enqueue(neighbor_node)

			if node.cell is not in total superposition and neighbor_node.cell is in total superposition:
				constrain_cell_to_neighbor(neighbor_node, node)

			if neighbor_node.cell is not in total superposition and neighbor_node.cell is not in failed state:
				constrain_cell_to_neighbor(node, neighbor_node)


constrain_cell_to_neighbor (node: Node, neighbor_node: Node):
	constraints = Set()
	for tile in neighbor_node.cell:
		constraints.union(
			tile.connections[wave_graph.opposite_direction(direction_index_to_neighbor)]
		)
	node.cell.intersect(constraints)
		
```

### B. Example `configuration.json` (Knots tileset)

```json
{
  "tiles": [
    "cap",
    "corner",
    "cross",
    "empty",
    "line",
    "t"
  ],
  "probabilities": [
    0,
    1,
    1,
    1,
    1,
    1
  ],
  "offsets": [
    {
      "x": 0,
      "y": 1
    },
    {
      "x": 1,
      "y": 0
    },
    {
      "x": 0,
      "y": -1
    },
    {
      "x": -1,
      "y": 0
    }
  ],
  "types": { 
  // Nested arrays must be objects to allow for parsing with Unity's JsonUtility
    "array": [
      {
        "array": [
          1,
          0,
          0,
          0
        ]
      },
	...
```

### C. Resetting cells in a wave graph (pseudo code)

```python
reset_cells (cell_count: int, seed_cell: cell):
	if cell not in wave_graph:
		return

	seed_node = wave_graph.get_node(seed_cell)

	cell_frontier = Set([seed_cell])
	node_queue = Queue([seed_node])

	while node_queue is not empty:
		node = node_queue.dequeue()
		node.cell = Cell(node.cell.physical_position)

		for (neighbor_node, direction_index) in node.neighbors:
			if cell_frontier.count >= cell_count:
				if neighbor_node.cell is not in total superposition and neighbor_node.cell is not failed:
					constrain_cell_to_neighbor(node, neighbor_node)

				continue

			node_queue.enqueue(neighbor_node)
			cell_frontier.add(neighbor_node.cell)
```

### D. Knots tile set

![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240508140423.png?raw=true)

### E. KayKit's Medieval Builder Pack tile set

![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240508182628.png?raw=true)

### F. Complete tile sets - Knots

![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240508185457.png?raw=true)

![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240508190554.png?raw=true)

### G. Incomplete tile sets - KayKit

![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240508192010.png?raw=true)
![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240508192312.png?raw=true)
![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240508192328.png?raw=true)
![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240508192835.png?raw=true)
![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240508193019.png?raw=true)
### H. Blender geometry node tileset
![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240509203932.png?raw=true)
![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240509203951.png?raw=true)
![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240509204031.png?raw=true)
![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240509204045.png?raw=true)
![](https://github.com/julzerinos/wave-function-collapse-brush/blob/assets/Pasted%20image%2020240509204054.png?raw=true)
### I. Blender tile generator export script

Simplified, full script is embedded in the generator file `tileset/generator.blend`.

```python
import bpy
import numpy as np
import itertools
import random
import os
import time
from mathutils import Vector

def create_tile_file(combination):
    obj.modifiers["GeometryNodes"][idt_types] = combination   

    time.sleep(2)
    obj.location = Vector((0, 0, 0))

    filepath = f'export/tile_{combination}.fbx'
    bpy.ops.export_scene.fbx(
        filepath=filepath,
        use_visible=True,
        object_types={'MESH'}
    )
 
obj = bpy.data.objects['Plane']

idt_types = obj.modifiers["GeometryNodes"].node_group.interface.items_tree["Types"].identifier
 
number_of_tiles = 4
for i in np.random.choice(len(permutations), number_of_tiles, replace=False):
    combination = 1000000
    for j, r in enumerate(permutations[i]):
        combination += int(r) * 10**j

    create_tile_file(combination)
```

