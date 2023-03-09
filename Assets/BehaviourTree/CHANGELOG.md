# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

Note this package is still in very early development and in high flux. 
Version numbers don't follow semantic versioning yet, and data format may change in future which will be highlighted in this document.

# [0.0.18] - 08-03-2023

## Update 18

- Blackboard Keys can now be renamed by double clicking on the blackboard key label
- Nodes can be copy, pasted, and duplicated in the tree view

# [0.0.17] - 04-03-2023

## NodeProperty<T>s, Blackboard Overrides, Switch Node

The NodeProperty class replaces the BlackboardProperty class, allowing node variables to be specified on the node, or optionally bind them directly to a blackboard key value.

### Changed features
- Renamed BlackboardProperty to NodeProperty
- Added a default value to the NodeProperty class
- Exposing a NodeProperty variable on a node allows the value to be specified on the node instance or bind it to a blackboard key.
- Added blackboard key overrides to the BehaviourTreeInstance component allowing blackboard key values to be override at the game object level.
- Added a new Switch composite node which allows executing a specific child based on an index value.

# [0.0.16] - 02-03-2023

## Runtime Blackboard Key support

Added runtime support for reading and writing blackboard values from components.

See BehaviourTreeInstance:
- SetBlackboardValue<T>
- GetBlackboardValue<T>
- FindBlackboardKey<T>

### Changed features
- Renamed BehaviourTreeRunner to BehaviourTreeInstance
- Changed behaviour tree initialisation from Start to Awake
- Added missing string blackboard key type
- Added SetBlackboardValue<T> method to BehaviourTreeInstance
- Added GetBlackboardValue<T> method to BehaviourTreeInstance
- Added FindBlackboardKey<T> method to BehaviourTreeInstance
- Added custom icon to BehaviourTreeInstance MonoBehaviour

# [0.0.15] - 01-03-2023

## Blackboard Refactor

The blackboard has been refactored significantly to support many more types, including any custom types defined in a project.

Note this will break any previously defined blackboard keys in your tree so you'll need to recreate them and assign them to a node.

Additionally, there are two new nodes:
- SetProperty
- CompareProperty

These nodes can be placed in a tree and used to set an arbitary blackboard key to a specific value, or compare a key to a specific value respectively.

### Changed features

- Added BlackboardProperty<T> type for reading and writing blackboard keys from nodes.
- Added generic BlackboardKey<T> type. Subclass this type to add support for custom types to the blackboard.
- Fixed editor assembly definition to only build for the editor.

# [0.0.14] - 01-03-2023

### Major changes

None

### Changed features

- Added CHANGELOG.md
- Amended Samples folder name to adhere to unity package standards

# [0.0.13] - (Previous Changes - prior to changelog.md)

## Major changes

- Added initial subtree implementation.
- Add a SubTreeNode to the graph and assign a behaviour tree to run it as a subtree in the graph.
- Auto Select nodes after they've been created so they appear in the inspector immediately
- New node scripts are automatically added to the tree on creation when dragging
- Block the UI while new node scripts are compiling
- Various other smaller changes and bug fixes