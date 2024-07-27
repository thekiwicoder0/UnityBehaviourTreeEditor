# Changelog

All notable changes to this package will be documented in this file.

# [0.0.31] - WIP
## Added
- SubTree node styling + icon.
- Added `Create SubTree` feature to node context menu.
- Added `TabView` to the main panel. Tree views now open in a new tab. 
- Close runtime tabs when exiting playmode.
- Editor window state is now serialized. Previous tabs will be restored after closing and reopening the window.
- New node script assets will be opened automatically in visual studio after compilation.
- Added subtree section to node canvas context menu
- Added New Subtree option to node canvas context menu
- Snap new nodes created via context menu to the grid
- Added Expand Subtree context menu option to subtree nodes

## Removed
- Removed breadcrumbs from main panel toolbar. Replaced with tabs

## Fixed
- Blackboard key value property label not appearing on some nodes
- Fixed node states in debugger when using subtrees
- Double clicking a subtree node when in playmode will open tree instance instead of asset
- [Opening BehaviourTree Editor causes error #15](https://github.com/thekiwicoder0/UnityBehaviourTreeEditor/issues/15)
- [Chance of failure comparison](https://github.com/thekiwicoder0/UnityBehaviourTreeEditor/issues/9)

# [0.0.30] - 01-06-2024
## Changed
- Runtime: Selector / Sequencer / Parallel nodes now actively tick all children rather than ticking from the previous child. 
- Runtime: Added TickDelta to context object to support multiple tick modes
- Nodes: RandomPosition min/max values expanded to vector3
- Nodes: Parallel node now has a threshold property to determine how many children need to succeed before this node returns success.
- Editor: Behaviour Tree menu item is now accessed under `Window -> AI -> Behaviour Tree` to conform with standard Unity tools
- Editor: Grid snap size is now 225 by default. 
- Editor: Populate GameObject behaviour tree instances to asset selector while in play mode
- Editor: Improved debugger performance when inspecting an active tree.

## Added
- Runtime: Conditional Nodes. Specialisation of an ActionNode which returns true/false. Has built in node option for negating the condition
- Runtime: BehaviourTreeInstance.ManualTick() function to control behaviour tree tick manually
- Runtime: BehaviourTreeInstance.StartBehaviour() function to manually start a behaviour
- Runtime: TickMode to control when the tree is ticked (update/fixedupdate/lateupdate/manual)
- Runtime: StartMode to control when the tree is started (awake/onenable/start/manual)
- Runtime: Unity profile markers for runtime and editor
- ProjectSettings: grid snap size to project settings
- ProjectSettings: autoSelectNodeHierarchy option to enable subtree selection by default
- Editor: Added icons to nodes on canvas 

## Removed

- Runtime: Removed Switch Node
- Runtime: Removed Interrupt Selector Node
- Runtime: Removed Random Selector Node

# [0.0.21] - 02-05-2024

### Changed
- Updated behaviour tree view classes to use [UXmlElement] attribute instead of deprecated uxml factory / traits

# [0.0.20] - 29-11-2023

### Added
- N/A

### Changed
- Improved Open Behaviour Tree dialog menu

### Fixed
- BlackboardKey 'Delete' context menu option now appears when clicking anywhere within the row

# [0.0.19] - 27-03-2023

- Restructured repository to contain just the package rather than the project. Installation URL is now just https://github.com/thekiwicoder0/UnityBehaviourTreeEditor.git
- Increased node text size slightly and center aligned
- Increased node description size slightly
- Use GraphElement.Capabilities to disable node snapping rather than editor prefs

# [0.0.18] - 09-03-2023

- Blackboard Keys can now be renamed by double clicking on the blackboard key label
- Nodes can be copy, pasted, and duplicated in the tree view
- Nodes snap to 15 pixels to align with the grid background. (Disabled adjacent node snapping)

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