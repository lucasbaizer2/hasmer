---
title: Hasm Assembly
nav_order: 1
parent: Hasm Assembly Docs
---

# Hasm Assembly

This page describes the format for Hasm assembly files.
Hasm files can be disassembled from [HBC](./concepts#hbc) files and assembled back into HBC files as well using the hasmer command line.

# Version Declaration

All Hasm files begin with a version declaration.
```
.hasm <version: int> <mode: "auto" | "exact">
```
The `version` is an integer representing the HBC version that the file is written for.

The `mode` tells the (dis)assembler how to interpret instructions with [variants](./concepts#variant-instructions).

// TODO: more docs!
