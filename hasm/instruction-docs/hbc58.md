---
title: Hermes Bytecode 58 Documentation
nav_order: 19
parent: Hermes Bytecode Documentation
---

# Hermes Bytecode 58 Documentation

# NewObjectWithBuffer

__Opcode:__ 0x00

__Available Since:__ HBC Version 40

__Encoded Size:__ 10 bytes

```
NewObjectWithBuffer <Reg8> <UInt16> <UInt16> <UInt16> <UInt16>
```

Creates an object from a static map of keys and values, e.g. `var x = { 'a': 3, 'b': 4 }`. Any non-constant elements can be set afterwards with [PutOwnByIndex](#putownbyindex).

> Operand 1 `(Reg8)`: The destination.

> Operand 2 `(UInt16)`: The preallocation size hint.

> Operand 3 `(UInt16)`: The number of static elements.

> Operand 4 `(UInt16)`: The offset in the [Object Key Buffer](../concepts.md#object-key-buffer) to get keys from.

> Operand 5 `(UInt16)`: The offset in the [Object Value Buffer](../concepts.md#object-value-buffer) to get values from.

## Variants

### NewObjectWithBufferLong

__Opcode:__ 0x01

__Available Since:__ HBC Version 40

__Encoded Size:__ 14 bytes

```
NewObjectWithBufferLong <Reg8> <UInt16> <UInt16> <UInt32> <UInt32>
```

---

# NewObject

__Opcode:__ 0x02

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
NewObject <Reg8>
```

Creates a new, empty Object using the built-in constructor (regardless of whether it was overridden), e.g. `var x = {}`.

> Operand 1 `(Reg8)`: The destination.

---

# NewObjectWithParent

__Opcode:__ 0x03

__Available Since:__ HBC Version 46

__Encoded Size:__ 3 bytes

```
NewObjectWithParent <Reg8> <Reg8>
```

Creates a new empty Object with the specified parent. If the parent is null, no parent is used. If the parent is not an object, the builtin Object.prototype is used. Otherwise the parent itself is used.

> Operand 1 `(Reg8)`: The destination.

> Operand 2 `(Reg8)`: The parent.

---

# NewArrayWithBuffer

__Opcode:__ 0x04

__Available Since:__ HBC Version 40

__Encoded Size:__ 8 bytes

```
NewArrayWithBuffer <Reg8> <UInt16> <UInt16> <UInt16>
```

Creates an array from a static list of values, e.g. `var x = [1, 2, 3].`. Any non-constant elements can be set afterwards with [PutOwnByIndex](#putownbyindex).

> Operand 1 `(Reg8)`: The destination.

> Operand 2 `(UInt16)`: The preallocation size hint.

> Operand 3 `(UInt16)`: The number of static elements.

> Operand 4 `(UInt16)`: The offset in the [Array Buffer](../concepts.md#array-buffer) to get values from.

## Variants

### NewArrayWithBufferLong

__Opcode:__ 0x05

__Available Since:__ HBC Version 40

__Encoded Size:__ 10 bytes

```
NewArrayWithBufferLong <Reg8> <UInt16> <UInt16> <UInt32>
```

---

# NewArray

__Opcode:__ 0x06

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
NewArray <Reg8> <UInt16>
```

Creates a new array of a given size, e.g. `var x = new Array(size)`.

> Operand 1 `(Reg8)`: The destination.

> Operand 2 `(UInt16)`: The size of the array.

---

# Mov

__Opcode:__ 0x07

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
Mov <Reg8> <Reg8>
```

Copies the value of one register into another, e.g. `x = y`.

> Operand 1 `(Reg8)`: The destination.

> Operand 2 `(Reg8)`: The source.

## Variants

### MovLong

__Opcode:__ 0x08

__Available Since:__ HBC Version 40

__Encoded Size:__ 9 bytes

```
MovLong <Reg32> <Reg32>
```

---

# Negate

__Opcode:__ 0x09

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
Negate <Reg8> <Reg8>
```

Performs a unary minus operation, e.g. `x = -y`.

> Operand 1 `(Reg8)`: The destination.

> Operand 2 `(Reg8)`: The source.

---

# Not

__Opcode:__ 0x0a

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
Not <Reg8> <Reg8>
```

No description available.

---

# BitNot

__Opcode:__ 0x0b

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
BitNot <Reg8> <Reg8>
```

No description available.

---

# TypeOf

__Opcode:__ 0x0c

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
TypeOf <Reg8> <Reg8>
```

No description available.

---

# Eq

__Opcode:__ 0x0d

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
Eq <Reg8> <Reg8> <Reg8>
```

No description available.

---

# StrictEq

__Opcode:__ 0x0e

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
StrictEq <Reg8> <Reg8> <Reg8>
```

No description available.

---

# Neq

__Opcode:__ 0x0f

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
Neq <Reg8> <Reg8> <Reg8>
```

No description available.

---

# StrictNeq

__Opcode:__ 0x10

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
StrictNeq <Reg8> <Reg8> <Reg8>
```

No description available.

---

# Less

__Opcode:__ 0x11

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
Less <Reg8> <Reg8> <Reg8>
```

No description available.

---

# LessEq

__Opcode:__ 0x12

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
LessEq <Reg8> <Reg8> <Reg8>
```

No description available.

---

# Greater

__Opcode:__ 0x13

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
Greater <Reg8> <Reg8> <Reg8>
```

No description available.

---

# GreaterEq

__Opcode:__ 0x14

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
GreaterEq <Reg8> <Reg8> <Reg8>
```

No description available.

---

# Add

__Opcode:__ 0x15

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
Add <Reg8> <Reg8> <Reg8>
```

No description available.

---

# AddN

__Opcode:__ 0x16

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
AddN <Reg8> <Reg8> <Reg8>
```

No description available.

---

# Mul

__Opcode:__ 0x17

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
Mul <Reg8> <Reg8> <Reg8>
```

No description available.

---

# MulN

__Opcode:__ 0x18

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
MulN <Reg8> <Reg8> <Reg8>
```

No description available.

---

# Div

__Opcode:__ 0x19

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
Div <Reg8> <Reg8> <Reg8>
```

No description available.

---

# DivN

__Opcode:__ 0x1a

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
DivN <Reg8> <Reg8> <Reg8>
```

No description available.

---

# Mod

__Opcode:__ 0x1b

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
Mod <Reg8> <Reg8> <Reg8>
```

No description available.

---

# Sub

__Opcode:__ 0x1c

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
Sub <Reg8> <Reg8> <Reg8>
```

No description available.

---

# SubN

__Opcode:__ 0x1d

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
SubN <Reg8> <Reg8> <Reg8>
```

No description available.

---

# LShift

__Opcode:__ 0x1e

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
LShift <Reg8> <Reg8> <Reg8>
```

No description available.

---

# RShift

__Opcode:__ 0x1f

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
RShift <Reg8> <Reg8> <Reg8>
```

No description available.

---

# URshift

__Opcode:__ 0x20

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
URshift <Reg8> <Reg8> <Reg8>
```

No description available.

---

# BitAnd

__Opcode:__ 0x21

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
BitAnd <Reg8> <Reg8> <Reg8>
```

No description available.

---

# BitXor

__Opcode:__ 0x22

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
BitXor <Reg8> <Reg8> <Reg8>
```

No description available.

---

# BitOr

__Opcode:__ 0x23

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
BitOr <Reg8> <Reg8> <Reg8>
```

No description available.

---

# InstanceOf

__Opcode:__ 0x24

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
InstanceOf <Reg8> <Reg8> <Reg8>
```

No description available.

---

# IsIn

__Opcode:__ 0x25

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
IsIn <Reg8> <Reg8> <Reg8>
```

No description available.

---

# GetEnvironment

__Opcode:__ 0x26

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
GetEnvironment <Reg8> <UInt8>
```

Gets an [Environment](../concepts.md#environment) from N levels up the stack. 0 is the current environment, 1 is the caller's environment, etc.

> Operand 1 `(Reg8)`: The destination.

> Operand 2 `(UInt8)`: The amount of levels up the stack to retrieve the environment from.

---

# StoreToEnvironment

__Opcode:__ 0x27

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
StoreToEnvironment <Reg8> <UInt8> <Reg8>
```

Stores a value in an [Environment](../concepts.md#environment).

> Operand 1 `(Reg8)`: The environment (as fetched by [GetEnvironment](#getenvironment)).

> Operand 2 `(UInt8)`: The environment slot number to store the value into.

> Operand 3 `(Reg8)`: The value to store.

## Variants

### StoreToEnvironmentL

__Opcode:__ 0x28

__Available Since:__ HBC Version 40

__Encoded Size:__ 5 bytes

```
StoreToEnvironmentL <Reg8> <UInt16> <Reg8>
```

---

# StoreNPToEnvironment

__Opcode:__ 0x29

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
StoreNPToEnvironment <Reg8> <UInt8> <Reg8>
```

Stores a non-pointer value in an [Environment](../concepts.md#environment).

> Operand 1 `(Reg8)`: The environment (as fetched by [GetEnvironment](#getenvironment)).

> Operand 2 `(UInt8)`: The environment slot number to store the value into.

> Operand 3 `(Reg8)`: The value to store.

## Variants

### StoreNPToEnvironmentL

__Opcode:__ 0x2a

__Available Since:__ HBC Version 40

__Encoded Size:__ 5 bytes

```
StoreNPToEnvironmentL <Reg8> <UInt16> <Reg8>
```

---

# LoadFromEnvironment

__Opcode:__ 0x2b

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
LoadFromEnvironment <Reg8> <Reg8> <UInt8>
```

Reads a value from an [Environment](../concepts.md#environment).

> Operand 1 `(Reg8)`: The destination.

> Operand 2 `(Reg8)`: The environment (as fetched by [GetEnvironment](#getenvironment)).

> Operand 3 `(UInt8)`: The environment slot number to read the value from.

## Variants

### LoadFromEnvironmentL

__Opcode:__ 0x2c

__Available Since:__ HBC Version 40

__Encoded Size:__ 5 bytes

```
LoadFromEnvironmentL <Reg8> <Reg8> <UInt16>
```

---

# GetGlobalObject

__Opcode:__ 0x2d

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
GetGlobalObject <Reg8>
```

Gets a reference to the [Global Object](../concepts.md#global-object).

> Operand 1 `(Reg8)`: The destination.

---

# GetNewTarget

__Opcode:__ 0x2e

__Available Since:__ HBC Version 44

__Encoded Size:__ 2 bytes

```
GetNewTarget <Reg8>
```

Gets the value of NewTarget from the frame. // TODO: elaborate

> Operand 1 `(Reg8)`: The destination.

---

# CreateEnvironment

__Opcode:__ 0x2f

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
CreateEnvironment <Reg8>
```

Creates a new [Environment](../concepts.md#environment) to store values captured by closures.

> Operand 1 `(Reg8)`: The destination.

---

# DeclareGlobalVar

__Opcode:__ 0x30

__Available Since:__ HBC Version 40

__Encoded Size:__ 5 bytes

```
DeclareGlobalVar <UInt32S>
```

Declare a global variable by [String Table](../concepts.md#string-table) index. The variable will be set to undefined.

> Operand 1 `(UInt32S)`: The name of the variable to declare.

---

# GetById

__Opcode:__ 0x32

__Available Since:__ HBC Version 40

__Encoded Size:__ 6 bytes

```
GetById <Reg8> <Reg8> <UInt8> <UInt16S>
```

Get an object property by [String Table](../concepts.md#string-table) index.

> Operand 1 `(Reg8)`: The destination.

> Operand 2 `(Reg8)`: The object to retrieve the property from.

> Operand 3 `(UInt8)`: The cache index of the property. This is omitted from Hasm when in [Auto Mode](../concepts.md#auto-mode).

> Operand 4 `(UInt16S)`: The name of the property to retrieve from the object.

## Variants

### GetByIdShort

__Opcode:__ 0x31

__Available Since:__ HBC Version 40

__Encoded Size:__ 5 bytes

```
GetByIdShort <Reg8> <Reg8> <UInt8> <UInt8S>
```

### GetByIdLong

__Opcode:__ 0x33

__Available Since:__ HBC Version 40

__Encoded Size:__ 8 bytes

```
GetByIdLong <Reg8> <Reg8> <UInt8> <UInt32S>
```

---

# TryGetById

__Opcode:__ 0x34

__Available Since:__ HBC Version 40

__Encoded Size:__ 6 bytes

```
TryGetById <Reg8> <Reg8> <UInt8> <UInt16S>
```

No description available.

## Variants

### TryGetByIdLong

__Opcode:__ 0x35

__Available Since:__ HBC Version 40

__Encoded Size:__ 8 bytes

```
TryGetByIdLong <Reg8> <Reg8> <UInt8> <UInt32S>
```

---

# PutById

__Opcode:__ 0x36

__Available Since:__ HBC Version 40

__Encoded Size:__ 6 bytes

```
PutById <Reg8> <Reg8> <UInt8> <UInt16S>
```

No description available.

## Variants

### PutByIdLong

__Opcode:__ 0x37

__Available Since:__ HBC Version 40

__Encoded Size:__ 8 bytes

```
PutByIdLong <Reg8> <Reg8> <UInt8> <UInt32S>
```

---

# TryPutById

__Opcode:__ 0x38

__Available Since:__ HBC Version 40

__Encoded Size:__ 6 bytes

```
TryPutById <Reg8> <Reg8> <UInt8> <UInt16S>
```

No description available.

## Variants

### TryPutByIdLong

__Opcode:__ 0x39

__Available Since:__ HBC Version 40

__Encoded Size:__ 8 bytes

```
TryPutByIdLong <Reg8> <Reg8> <UInt8> <UInt32S>
```

---

# PutNewOwnById

__Opcode:__ 0x3b

__Available Since:__ HBC Version 45

__Encoded Size:__ 5 bytes

```
PutNewOwnById <Reg8> <Reg8> <UInt16S>
```

No description available.

## Variants

### PutNewOwnByIdShort

__Opcode:__ 0x3a

__Available Since:__ HBC Version 45

__Encoded Size:__ 4 bytes

```
PutNewOwnByIdShort <Reg8> <Reg8> <UInt8S>
```

### PutNewOwnByIdLong

__Opcode:__ 0x3c

__Available Since:__ HBC Version 45

__Encoded Size:__ 7 bytes

```
PutNewOwnByIdLong <Reg8> <Reg8> <UInt32S>
```

---

# PutNewOwnNEById

__Opcode:__ 0x3d

__Available Since:__ HBC Version 45

__Encoded Size:__ 5 bytes

```
PutNewOwnNEById <Reg8> <Reg8> <UInt16S>
```

No description available.

## Variants

### PutNewOwnNEByIdLong

__Opcode:__ 0x3e

__Available Since:__ HBC Version 45

__Encoded Size:__ 7 bytes

```
PutNewOwnNEByIdLong <Reg8> <Reg8> <UInt32S>
```

---

# PutOwnByIndex

__Opcode:__ 0x3f

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
PutOwnByIndex <Reg8> <Reg8> <UInt8>
```

No description available.

## Variants

### PutOwnByIndexL

__Opcode:__ 0x40

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
PutOwnByIndexL <Reg8> <Reg8> <UInt32>
```

---

# PutOwnByVal

__Opcode:__ 0x41

__Available Since:__ HBC Version 45

__Encoded Size:__ 5 bytes

```
PutOwnByVal <Reg8> <Reg8> <Reg8> <UInt8>
```

No description available.

---

# DelById

__Opcode:__ 0x42

__Available Since:__ HBC Version 40

__Encoded Size:__ 5 bytes

```
DelById <Reg8> <Reg8> <UInt16S>
```

No description available.

## Variants

### DelByIdLong

__Opcode:__ 0x43

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
DelByIdLong <Reg8> <Reg8> <UInt32S>
```

---

# GetByVal

__Opcode:__ 0x44

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
GetByVal <Reg8> <Reg8> <Reg8>
```

No description available.

---

# PutByVal

__Opcode:__ 0x45

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
PutByVal <Reg8> <Reg8> <Reg8>
```

No description available.

---

# DelByVal

__Opcode:__ 0x46

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
DelByVal <Reg8> <Reg8> <Reg8>
```

No description available.

---

# PutOwnGetterSetterByVal

__Opcode:__ 0x47

__Available Since:__ HBC Version 45

__Encoded Size:__ 6 bytes

```
PutOwnGetterSetterByVal <Reg8> <Reg8> <Reg8> <Reg8> <UInt8>
```

No description available.

---

# GetPNameList

__Opcode:__ 0x48

__Available Since:__ HBC Version 40

__Encoded Size:__ 5 bytes

```
GetPNameList <Reg8> <Reg8> <Reg8> <Reg8>
```

No description available.

---

# GetNextPName

__Opcode:__ 0x49

__Available Since:__ HBC Version 40

__Encoded Size:__ 6 bytes

```
GetNextPName <Reg8> <Reg8> <Reg8> <Reg8> <Reg8>
```

No description available.

---

# Call

__Opcode:__ 0x4a

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
Call <Reg8> <Reg8> <UInt8>
```

No description available.

## Variants

### CallLong

__Opcode:__ 0x51

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
CallLong <Reg8> <Reg8> <UInt32>
```

---

# Construct

__Opcode:__ 0x4b

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
Construct <Reg8> <Reg8> <UInt8>
```

No description available.

## Variants

### ConstructLong

__Opcode:__ 0x52

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
ConstructLong <Reg8> <Reg8> <UInt32>
```

---

# Call1

__Opcode:__ 0x4c

__Available Since:__ HBC Version 41

__Encoded Size:__ 4 bytes

```
Call1 <Reg8> <Reg8> <Reg8>
```

No description available.

---

# CallDirect

__Opcode:__ 0x4d

__Available Since:__ HBC Version 40

__Encoded Size:__ 5 bytes

```
CallDirect <Reg8> <UInt8> <UInt16>
```

No description available.

## Variants

### CallDirectLongIndex

__Opcode:__ 0x53

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
CallDirectLongIndex <Reg8> <UInt8> <UInt32>
```

---

# Call2

__Opcode:__ 0x4e

__Available Since:__ HBC Version 41

__Encoded Size:__ 5 bytes

```
Call2 <Reg8> <Reg8> <Reg8> <Reg8>
```

No description available.

---

# Call3

__Opcode:__ 0x4f

__Available Since:__ HBC Version 41

__Encoded Size:__ 6 bytes

```
Call3 <Reg8> <Reg8> <Reg8> <Reg8> <Reg8>
```

No description available.

---

# Call4

__Opcode:__ 0x50

__Available Since:__ HBC Version 41

__Encoded Size:__ 7 bytes

```
Call4 <Reg8> <Reg8> <Reg8> <Reg8> <Reg8> <Reg8>
```

No description available.

---

# CallBuiltin

__Opcode:__ 0x54

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
CallBuiltin <Reg8> <UInt8> <UInt8>
```

No description available.

---

# Ret

__Opcode:__ 0x55

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
Ret <Reg8>
```

No description available.

---

# Catch

__Opcode:__ 0x56

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
Catch <Reg8>
```

No description available.

---

# DirectEval

__Opcode:__ 0x57

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
DirectEval <Reg8> <Reg8>
```

No description available.

---

# Throw

__Opcode:__ 0x58

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
Throw <Reg8>
```

No description available.

---

# Debugger

__Opcode:__ 0x59

__Available Since:__ HBC Version 40

__Encoded Size:__ 1 byte

```
Debugger 
```

No description available.

---

# DebuggerCheckBreak

__Opcode:__ 0x5a

__Available Since:__ HBC Version 40

__Encoded Size:__ 1 byte

```
DebuggerCheckBreak 
```

No description available.

---

# ProfilePoint

__Opcode:__ 0x5b

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
ProfilePoint <UInt16>
```

No description available.

---

# Unreachable

__Opcode:__ 0x5c

__Available Since:__ HBC Version 40

__Encoded Size:__ 1 byte

```
Unreachable 
```

Unreachable opcode for stubs and similar.

---

# CreateClosure

__Opcode:__ 0x5d

__Available Since:__ HBC Version 40

__Encoded Size:__ 5 bytes

```
CreateClosure <Reg8> <Reg8> <UInt16>
```

No description available.

## Variants

### CreateClosureLongIndex

__Opcode:__ 0x5e

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
CreateClosureLongIndex <Reg8> <Reg8> <UInt32>
```

---

# CreateGeneratorClosure

__Opcode:__ 0x5f

__Available Since:__ HBC Version 52

__Encoded Size:__ 5 bytes

```
CreateGeneratorClosure <Reg8> <Reg8> <UInt16>
```

No description available.

## Variants

### CreateGeneratorClosureLongIndex

__Opcode:__ 0x60

__Available Since:__ HBC Version 52

__Encoded Size:__ 7 bytes

```
CreateGeneratorClosureLongIndex <Reg8> <Reg8> <UInt32>
```

---

# CreateThis

__Opcode:__ 0x61

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
CreateThis <Reg8> <Reg8> <Reg8>
```

No description available.

---

# SelectObject

__Opcode:__ 0x62

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
SelectObject <Reg8> <Reg8> <Reg8>
```

No description available.

---

# LoadParam

__Opcode:__ 0x63

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
LoadParam <Reg8> <UInt8>
```

No description available.

## Variants

### LoadParamLong

__Opcode:__ 0x64

__Available Since:__ HBC Version 40

__Encoded Size:__ 6 bytes

```
LoadParamLong <Reg8> <UInt32>
```

---

# LoadConstUInt8

__Opcode:__ 0x65

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
LoadConstUInt8 <Reg8> <UInt8>
```

No description available.

---

# LoadConstInt

__Opcode:__ 0x66

__Available Since:__ HBC Version 40

__Encoded Size:__ 6 bytes

```
LoadConstInt <Reg8> <Imm32>
```

No description available.

---

# LoadConstDouble

__Opcode:__ 0x67

__Available Since:__ HBC Version 40

__Encoded Size:__ 10 bytes

```
LoadConstDouble <Reg8> <Double>
```

No description available.

---

# LoadConstString

__Opcode:__ 0x68

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
LoadConstString <Reg8> <UInt16S>
```

No description available.

## Variants

### LoadConstStringLongIndex

__Opcode:__ 0x69

__Available Since:__ HBC Version 40

__Encoded Size:__ 6 bytes

```
LoadConstStringLongIndex <Reg8> <UInt32S>
```

---

# LoadConstUndefined

__Opcode:__ 0x6a

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
LoadConstUndefined <Reg8>
```

No description available.

---

# LoadConstNull

__Opcode:__ 0x6b

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
LoadConstNull <Reg8>
```

No description available.

---

# LoadConstTrue

__Opcode:__ 0x6c

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
LoadConstTrue <Reg8>
```

No description available.

---

# LoadConstFalse

__Opcode:__ 0x6d

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
LoadConstFalse <Reg8>
```

No description available.

---

# LoadConstZero

__Opcode:__ 0x6e

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
LoadConstZero <Reg8>
```

No description available.

---

# CoerceThisNS

__Opcode:__ 0x6f

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
CoerceThisNS <Reg8> <Reg8>
```

No description available.

---

# LoadThisNS

__Opcode:__ 0x70

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
LoadThisNS <Reg8>
```

No description available.

---

# ToNumber

__Opcode:__ 0x71

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
ToNumber <Reg8> <Reg8>
```

No description available.

---

# ToInt32

__Opcode:__ 0x72

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
ToInt32 <Reg8> <Reg8>
```

No description available.

---

# AddEmptyString

__Opcode:__ 0x73

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
AddEmptyString <Reg8> <Reg8>
```

No description available.

---

# GetArgumentsPropByVal

__Opcode:__ 0x74

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
GetArgumentsPropByVal <Reg8> <Reg8> <Reg8>
```

No description available.

---

# GetArgumentsLength

__Opcode:__ 0x75

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
GetArgumentsLength <Reg8> <Reg8>
```

No description available.

---

# ReifyArguments

__Opcode:__ 0x76

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
ReifyArguments <Reg8>
```

No description available.

---

# CreateRegExp

__Opcode:__ 0x77

__Available Since:__ HBC Version 40

__Encoded Size:__ 14 bytes

```
CreateRegExp <Reg8> <UInt32S> <UInt32S> <UInt32>
```

No description available.

---

# SwitchImm

__Opcode:__ 0x78

__Available Since:__ HBC Version 40

__Encoded Size:__ 18 bytes

```
SwitchImm <Reg8> <UInt32> <Addr32> <UInt32> <UInt32>
```

No description available.

---

# StartGenerator

__Opcode:__ 0x79

__Available Since:__ HBC Version 52

__Encoded Size:__ 1 byte

```
StartGenerator 
```

No description available.

---

# ResumeGenerator

__Opcode:__ 0x7a

__Available Since:__ HBC Version 52

__Encoded Size:__ 3 bytes

```
ResumeGenerator <Reg8> <Reg8>
```

No description available.

---

# CompleteGenerator

__Opcode:__ 0x7b

__Available Since:__ HBC Version 52

__Encoded Size:__ 1 byte

```
CompleteGenerator 
```

No description available.

---

# CreateGenerator

__Opcode:__ 0x7c

__Available Since:__ HBC Version 52

__Encoded Size:__ 5 bytes

```
CreateGenerator <Reg8> <Reg8> <UInt16>
```

No description available.

## Variants

### CreateGeneratorLongIndex

__Opcode:__ 0x7d

__Available Since:__ HBC Version 52

__Encoded Size:__ 7 bytes

```
CreateGeneratorLongIndex <Reg8> <Reg8> <UInt32>
```

---

# Jmp

__Opcode:__ 0x7e

__Available Since:__ HBC Version 40

__Encoded Size:__ 2 bytes

```
Jmp <Addr8>
```

No description available.

## Variants

### JmpLong

__Opcode:__ 0x7f

__Available Since:__ HBC Version 40

__Encoded Size:__ 5 bytes

```
JmpLong <Addr32>
```

---

# JmpTrue

__Opcode:__ 0x80

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
JmpTrue <Addr8> <Reg8>
```

No description available.

## Variants

### JmpTrueLong

__Opcode:__ 0x81

__Available Since:__ HBC Version 40

__Encoded Size:__ 6 bytes

```
JmpTrueLong <Addr32> <Reg8>
```

---

# JmpFalse

__Opcode:__ 0x82

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
JmpFalse <Addr8> <Reg8>
```

No description available.

## Variants

### JmpFalseLong

__Opcode:__ 0x83

__Available Since:__ HBC Version 40

__Encoded Size:__ 6 bytes

```
JmpFalseLong <Addr32> <Reg8>
```

---

# JmpUndefined

__Opcode:__ 0x84

__Available Since:__ HBC Version 40

__Encoded Size:__ 3 bytes

```
JmpUndefined <Addr8> <Reg8>
```

No description available.

## Variants

### JmpUndefinedLong

__Opcode:__ 0x85

__Available Since:__ HBC Version 40

__Encoded Size:__ 6 bytes

```
JmpUndefinedLong <Addr32> <Reg8>
```

---

# SaveGenerator

__Opcode:__ 0x86

__Available Since:__ HBC Version 52

__Encoded Size:__ 2 bytes

```
SaveGenerator <Addr8>
```

No description available.

## Variants

### SaveGeneratorLong

__Opcode:__ 0x87

__Available Since:__ HBC Version 52

__Encoded Size:__ 5 bytes

```
SaveGeneratorLong <Addr32>
```

---

# JLess

__Opcode:__ 0x88

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JLess <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JLessLong

__Opcode:__ 0x89

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JLessLong <Addr32> <Reg8> <Reg8>
```

---

# JNotLess

__Opcode:__ 0x8a

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JNotLess <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JNotLessLong

__Opcode:__ 0x8b

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JNotLessLong <Addr32> <Reg8> <Reg8>
```

---

# JLessN

__Opcode:__ 0x8c

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JLessN <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JLessNLong

__Opcode:__ 0x8d

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JLessNLong <Addr32> <Reg8> <Reg8>
```

---

# JNotLessN

__Opcode:__ 0x8e

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JNotLessN <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JNotLessNLong

__Opcode:__ 0x8f

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JNotLessNLong <Addr32> <Reg8> <Reg8>
```

---

# JLessEqual

__Opcode:__ 0x90

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JLessEqual <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JLessEqualLong

__Opcode:__ 0x91

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JLessEqualLong <Addr32> <Reg8> <Reg8>
```

---

# JNotLessEqual

__Opcode:__ 0x92

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JNotLessEqual <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JNotLessEqualLong

__Opcode:__ 0x93

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JNotLessEqualLong <Addr32> <Reg8> <Reg8>
```

---

# JLessEqualN

__Opcode:__ 0x94

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JLessEqualN <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JLessEqualNLong

__Opcode:__ 0x95

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JLessEqualNLong <Addr32> <Reg8> <Reg8>
```

---

# JNotLessEqualN

__Opcode:__ 0x96

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JNotLessEqualN <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JNotLessEqualNLong

__Opcode:__ 0x97

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JNotLessEqualNLong <Addr32> <Reg8> <Reg8>
```

---

# JGreater

__Opcode:__ 0x98

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JGreater <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JGreaterLong

__Opcode:__ 0x99

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JGreaterLong <Addr32> <Reg8> <Reg8>
```

---

# JNotGreater

__Opcode:__ 0x9a

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JNotGreater <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JNotGreaterLong

__Opcode:__ 0x9b

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JNotGreaterLong <Addr32> <Reg8> <Reg8>
```

---

# JGreaterN

__Opcode:__ 0x9c

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JGreaterN <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JGreaterNLong

__Opcode:__ 0x9d

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JGreaterNLong <Addr32> <Reg8> <Reg8>
```

---

# JNotGreaterN

__Opcode:__ 0x9e

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JNotGreaterN <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JNotGreaterNLong

__Opcode:__ 0x9f

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JNotGreaterNLong <Addr32> <Reg8> <Reg8>
```

---

# JGreaterEqual

__Opcode:__ 0xa0

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JGreaterEqual <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JGreaterEqualLong

__Opcode:__ 0xa1

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JGreaterEqualLong <Addr32> <Reg8> <Reg8>
```

---

# JNotGreaterEqual

__Opcode:__ 0xa2

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JNotGreaterEqual <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JNotGreaterEqualLong

__Opcode:__ 0xa3

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JNotGreaterEqualLong <Addr32> <Reg8> <Reg8>
```

---

# JGreaterEqualN

__Opcode:__ 0xa4

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JGreaterEqualN <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JGreaterEqualNLong

__Opcode:__ 0xa5

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JGreaterEqualNLong <Addr32> <Reg8> <Reg8>
```

---

# JNotGreaterEqualN

__Opcode:__ 0xa6

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JNotGreaterEqualN <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JNotGreaterEqualNLong

__Opcode:__ 0xa7

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JNotGreaterEqualNLong <Addr32> <Reg8> <Reg8>
```

---

# JEqual

__Opcode:__ 0xa8

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JEqual <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JEqualLong

__Opcode:__ 0xa9

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JEqualLong <Addr32> <Reg8> <Reg8>
```

---

# JNotEqual

__Opcode:__ 0xaa

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JNotEqual <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JNotEqualLong

__Opcode:__ 0xab

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JNotEqualLong <Addr32> <Reg8> <Reg8>
```

---

# JStrictEqual

__Opcode:__ 0xac

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JStrictEqual <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JStrictEqualLong

__Opcode:__ 0xad

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JStrictEqualLong <Addr32> <Reg8> <Reg8>
```

---

# JStrictNotEqual

__Opcode:__ 0xae

__Available Since:__ HBC Version 40

__Encoded Size:__ 4 bytes

```
JStrictNotEqual <Addr8> <Reg8> <Reg8>
```

No description available.

## Variants

### JStrictNotEqualLong

__Opcode:__ 0xaf

__Available Since:__ HBC Version 40

__Encoded Size:__ 7 bytes

```
JStrictNotEqualLong <Addr32> <Reg8> <Reg8>
```

---

