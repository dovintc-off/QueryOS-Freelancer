# Virtual In Game Machine (QueryOS)

A modular command-line interface (CLI) subsystem built on top of MonoGame. This architecture decouples the core operating system logic from the game engine's rendering layer, allowing for expanding game mechanics via external script files.

## Core Features

### 1. Isolated File System Sandbox
All virtual OS directories and assets reside inside the `FSR/` (File System Root) folder local to the main executable. The kernel serves as a secure wrapper over `System.IO` with explicit protections against Directory Traversal (`..`) vulnerabilities, isolating all runtime execution strictly within the sandbox boundaries.

### 2. File Metadata Architecture
Every file and directory within the sandbox uses hidden sidecar metadata files (`.inf` and `.fold-inf`). Written in a compressed tokenized layout (`S:1,O:root,P:755`), these structures track:
* **S (Stage):** Progression-gated visibility tied to story mechanics.
* **O (Owner):** Multi-user permission simulation.
* **P (Permissions):** Standard POSIX-like access controls.

### 3. Non-Static Multiprocess Design
The kernel utilizes an abstract execution context model where every running utility inherits from a unified base class. 
* **Process Lifecycle:** Tasks dynamically register their presence through a central manager, automatically acquiring unique process identification numbers.
* **Input/Output Contexts:** Processes utilize standalone buffer instances, preventing console output contamination during simultaneous or background execution.

---

## Architectural Breakdown

### Initialization Sequence
1. **Bootstrap Phase:** The engine ensures directory structures match preset conditions and parses primary configuration layouts.
2. **System Root (PID 1):** Spawns the foundational operating system monitor tasked with microkernel coordination.
3. **Storage Subsystem (PID 2):** Initializes sandbox barriers, scans standard file maps, and enforces metadata filtering rules.
4. **Shell Interpreter (PID 3):** Attaches transparent keyboard input capture frames and handles the processing loop for user commands.

---

## Technical Specifications & About
* **Framework:** MonoGame Cross-Platform Desktop (Maybe)
* **UI Integration:** Myra UI Layer
* **Target Version:** .NET 8.0 / C# 12
* **License:** [GNU General Public License v3.0](LICENSE)
* **Author:** Dov1ntc