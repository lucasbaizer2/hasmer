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

Format:
```
.hasm <version: int> <mode: "auto" | "exact">
```
Example:
```hasm
.hasm 84 auto
```

The `version` is an integer representing the HBC version that the file is written for.

The `mode` tells the (dis)assembler how to interpret instructions with [variants](./concepts#variant-instructions).

# Data Declaration

Hermes instructions often reference constant data, such as the [Array Buffer](./concepts.md#array-buffer). Data declarations are used to define the data referenced by those instructions.

Format:
```
.data <type: "A" | "K" | "V"><label: int> <type: "Integer" | "String" | "Null" | "True" | "False" | "Number">[v1, v2, ...]
```
Example:
```hasm
.data A351 String["username", "password"]
```

The data defined within the square brackets must match the data type of the `type` specification. 

# Function Declaration

Functions represent compiled JavaScript functions containing Hermes bytecode.

Format:
```
.start <type: "Function" | "Constructor" | "NCFunction"> <<name: String>>(this, par1, par2, ...)
    declaration1
    declaration2
    ...

    instruction1
    instruction2
    ...
.end
```
Example:
```hasm
.start Function <myFuncName>(this)
    .id 263
    .params 1
    .registers 1
    .symbols 0
    .strict

    LoadConstUndefined r0
    Ret r0
.end
```

## Function ID Declaration

Every function, including closures, need a unique ID. This is ID set with the ID declaration.

Format:
```
.id <id: int>
```
Example:
```hasm
.id 263
```

## Function Parameters Declaration

The parameters declaration defines how many parameters a function takes (including the `this` parameter, which every function takes).

Format:
```
.params <params: int>
```
Example:
```hasm
.params 1
```

## Function Registers Declaration

The registers declaration defines how many registers the function uses.

Format:
```
.registers <registers: int>
```
Example:
```hasm
.registers 1
```

## Function Symbols Declaration

The symbols declaration defines how many slots should be allocated for the function's [Environment](./concepts.md#environment).

Format:
```
.symbols <symbols: int>
```
Example:
```hasm
.symbols 1
```

## Function Strict Declaration

The `strict` declaration declares whether the function should be interpreted in [strict mode](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Strict_mode) or not. 

Format:
```
.strict
```
