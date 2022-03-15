---
title: Hermes Concepts
parent: Hasm Assembly Docs
---

# Hermes Concepts

This document contains documentation on the concepts of Hermes assembly and the Hasm format.

## Names, Acronyms, and Fundamental Concepts

It can be difficult to understand what all the different parts of the hasmer ecosystem are.
Below are the definitions of some basic concepts about hasmer to help explain what the pieces are.

### hasmer
[hasmer](https://lucasbaizer2.github.io/hasmer) is an open source command line tool for (dis)assembling and decompiling Hermes bytecode. 

### Hermes
[Hermes](https://github.com/facebook/hermes) is a JavaScript engine optimized for executing JavaScript in [React Native](https://reactnative.dev). Hermes has its own JavaScript compiler which outputs Hermes bytecode files. These bytecode files are optimized for execution in the Hermes engine at runtime, and many React Native apps use this precompiled format.

### HBC
HBC is an acronym for "**H**ermes **b**yte**c**ode". HBC is a binary bytecode format that is executed by the Hermes engine at runtime. It represents a precompiled JavaScript file containing HBC instructions.

## Array Buffer

The Array Buffer is a set of constant data encoded in an HBC file.
Constant array data from the original JavaScript file is encoded into the Array Buffer.
At runtime, when constructing arrays with constant data, the data is retrieved from the Array Buffer.

As an example, if the original JavaScript had this code:
```js
var myArray = [1, 2, 3, "hello world"];
```
The values `1`, `2`, `3`, and `"hello world"` would be encoded consecutively into the Array Buffer. HBC instructions which load constant array data retrieve the values from the Array Buffer by index, which refers to the offset in the buffer that the values are encoded at.

## Object Key Buffer

The Object Key Buffer is very similar to the [Array Buffer](#array-buffer),
except it contains the keys (i.e. properties) of objects.

As an example, if the original JavaScript had this code:
```js
var myObject = {
    key1: "hello",
    key2: "World"
};
```
The keys `"key1"` and `"key2"` would be encoded into the Object Key Buffer.

## Object Value Buffer

The Object Value Buffer is very similar to the [Array Buffer](#array-buffer),
except it contains the values of objects.

As an example, if the original JavaScript had this code:
```js
var myObject = {
    key1: "hello",
    key2: "World"
};
```
The values `"hello"` and `"World"` would be encoded into the Object Value Buffer.

## String Table

The String Table is an array-like structure containing all of the strings in the HBC file.
The String Table includes property names, function names, constant strings, etc.

As an example, if the original JavaScript had this code:
```js
function main() {
    print(global.someProperty);
}
```
The String Table would contain entries for the strings `main`, `print`, `global,` and `someProperty`.

## Environment

Environments are how variables are exchanged between nested functions and closures.
A parent function stores variables references by closures into its environment,
and the closure can access the environment variables from its parents.

As an example, if the original JavaScript had this code:
```js
function main() {
    var val = "hello ";
    var myClosure = () => {
        val += "world";
    };
    myClosure();
    print(val); // prints "hello world"
}
```
The `main` function stores the `val` variable into its environment.
When `myClosure` is invoked, it obtains a reference to the parent environment
and retrieves the `val` variable. It then modifies `val` (i.e. appends `"world"`)
and stores that value back into the environment of its parent.

## Global Function

The Global Function is where all code is put into at compile-time.
The Hermes execution engine runs the `global` function at runtime.

As an example, if the original JavaScript had this code:
```js
print("hello, world");
```
The decompiled HBC would look something like this:
```js
function global() {
    print("hello, world");
}
```

## Global Object

The Global Object (i.e. the identifier `global`) is where all global functions and properties are stored.
At compile time, all global functions and properties are assigned to be values within the
`global` object. 

As an example, if the original JavaScript had this code:
```js
function main() {
    print(Object.keys(global));
}
```
The output would include the `main` function, as well as other Hermes built-ins
(such as the [Global Function](#global-function)).
