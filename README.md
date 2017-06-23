# Chip8Sharp
A Chip8 Emulator written in C#

## Usage

### Windows
Ensure .NET4 redistributables are installed
Open Chip8.exe

### Linux/Mac
Open terminal
Navigate to Release folder
`chmod +x Chip8`
`./Chip8 gamename` - where the argument is the game you want to play. No argument defaults to "BRIX".

## About
Chip8 is a virtual machine designed by Joseph Weisbecker in the mid 1970's that ran on COSMAC VIP general computers. It has a minimalistic instruction set and
was intended to facilitate game programming on said computer. 

The original COSMAC computers had 4k of RAM, a ~20hz processor with 16 general purpose registers that understood 35 instructions and a 64x32 monochrome screen. The first 512 bytes of memory were intended for the interpreter.

In this version the screen has scaled for 2x the original size, and the bytes reserved for the interpreter are loaded with a fontset 
needed by the games.

## Games
I've included games originally made for the Chip8 VM in this emulator; as I understand it, these games are public domain.

Thanks to all the original authors.

## Testing
All opcodes have an associated test with them located in "Tests.cs" folder. The tests check the state of the emulator after executing an instruction.

Two opcodes do not have tests: 0x0NNN (an unused opcode) and 0xCXNN (store the logical and of NN & a random number into VX)

## Assembler
I've included an Chip8 assembler written by Craig Thomas if you're interested in writing your own Chip8 games.
