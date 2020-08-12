# Pathfinding
Many pathfinding algorithm



## Contents

* 【Basic】
  * A*
  * BFS
  * Dijkstra
* 【Bidirectional Search】
  * Bidirection A*
* 【Any-Angle Search】
  * Theta*
    * Lazy Theta*
* 【Incremental Search】
  * D*
  * LPA*
  * D* Lite
  * Path Adaptive A*
  * Tree Adaptive A*
  * 【Moving Target】
    * Generalized Adaptive A* (GAA*)
  	* Generalized Fringe-Retrieving A* (G-FRA*)
  	* Moving Target D* Lite (MT-D* Lite)
* 【Hierarchical Search】
  * HPA*
* 【Other Search】
  * Flow Field
  * Jump Point Search
    - JPS Plus



## How to play

### Choose scene

* [HPA] scene for HPA* algorithm, press space key to run the algorithm.
* [Pathfinding] scene for other search algorithm, select GameObject [Search], choose algorithm in Inspector, press space key to run the algorithm.
* [FlowField] scene for flowfield algorithm, you can see the result immediately without any operation.

### Handle grid

- You can drag the start and goal node
- Left click can add obstacle
- Right click can remove obstacle



## Reference

[Github PathFinding.js](https://github.com/qiao/PathFinding.js)

[Github HierarchicalPathfinder](https://github.com/Rydra/HierarchicalPathfinder)

Many papers