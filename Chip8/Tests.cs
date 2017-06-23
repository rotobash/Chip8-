using System;

using NUnit.Framework;
using Microsoft.Xna.Framework.Input;

namespace Chip8
{
	[TestFixture]
	public class OpCodeTest
	{
		Chip8 emu;
		Random rnd;

		[SetUp]
		public void Setup()
		{
			emu = new Chip8();
			rnd = new Random();
		}

		[Test]
		/// <summary>
		/// Tests clearing the screen
		/// Opcodes: 0x00E0
		/// </summary>
		public void TestScreenClear()
		{
			for (int x = 0; x < (64 * 32); x++)
			{
				emu.gfxBuf[x] = (byte)rnd.Next(0, 2);
			}
			emu.progCounter = 0;
			emu.ExecuteInstruction(0x00E0);


			for (int x = 0; x < (64 * 32); x++)
			{
				Assert.AreEqual(emu.gfxBuf[x], 0);
			}
		}

		[Test]
		/// <summary>
		/// Tests the call and return instructions.
		/// Opcodes: 0x2NNN, 0x00EE
		/// </summary>
		public void TestCallAndReturn()
		{
			emu.progCounter = 0xF;
			//Call
			emu.ExecuteInstruction(0x2200);
			Assert.AreEqual(emu.progCounter, 0x200);
			Assert.AreEqual(emu.stack[0], 0xF);
			Assert.AreEqual(emu.stackPointer, 0x1);
			//Return
			emu.ExecuteInstruction(0x00EE);
			Assert.AreEqual(emu.progCounter, 0xF);
			Assert.AreEqual(emu.stackPointer, 0x0);

		}

		[Test]
		/// <summary>
		/// Tests multiple call and return instructions.
		/// Opcodes: 0x2NNN, 0x00EE
		/// </summary>
		public void TestMultipleCallsAndReturns()
		{
			emu.progCounter = 0xF;
			//Call
			emu.ExecuteInstruction(0x2200);
			Assert.AreEqual(emu.progCounter, 0x200);
			Assert.AreEqual(emu.stack[emu.stackPointer - 1], 0xF);
			Assert.AreEqual(emu.stackPointer, 0x1);
			//Call
			emu.ExecuteInstruction(0x2420);
			Assert.AreEqual(emu.progCounter, 0x420);
			Assert.AreEqual(emu.stack[emu.stackPointer - 1], 0x200);
			Assert.AreEqual(emu.stackPointer, 0x2);
			//Return
			emu.ExecuteInstruction(0x00EE);
			Assert.AreEqual(emu.progCounter, 0x200);
			Assert.AreEqual(emu.stackPointer, 0x1);
			//Call
			emu.ExecuteInstruction(0x2534);
			Assert.AreEqual(emu.progCounter, 0x534);
			Assert.AreEqual(emu.stack[emu.stackPointer - 1], 0x200);
			Assert.AreEqual(emu.stackPointer, 0x2);;
			//Call
			emu.ExecuteInstruction(0x2587);
			Assert.AreEqual(emu.progCounter, 0x587);
			Assert.AreEqual(emu.stack[emu.stackPointer - 1], 0x534);
			Assert.AreEqual(emu.stackPointer, 0x3);
			//Return
			emu.ExecuteInstruction(0x00EE);
			Assert.AreEqual(emu.progCounter, 0x534);
			Assert.AreEqual(emu.stackPointer, 0x2);
			//Return
			emu.ExecuteInstruction(0x00EE);
			Assert.AreEqual(emu.progCounter, 0x200);
			Assert.AreEqual(emu.stackPointer, 0x1);
			//Return
			emu.ExecuteInstruction(0x00EE);
			Assert.AreEqual(emu.progCounter, 0xF);
			Assert.AreEqual(emu.stackPointer, 0x0);

		}

		[Test]
		/// <summary>
		/// Tests the jump instruction.
		/// Opcodes: 0x1NNN
		/// </summary>
		public void TestJump() 
		{
			emu.progCounter = 0xF;
			//Jump
			emu.ExecuteInstruction(0x1200);
			Assert.AreEqual(emu.progCounter, 0x200);
			Assert.AreEqual(emu.stack[0], 0x0);
			Assert.AreEqual(emu.stackPointer, 0x0);

		}

		[Test]
		/// <summary>
		/// Tests multiple jump instructions.
		/// Opcodes: 0x1NNN
		/// </summary>
		public void TestJumps() 
		{
			emu.progCounter = 0xF;
			//Jump
			emu.ExecuteInstruction(0x1200);
			Assert.AreEqual(emu.progCounter, 0x200);
			Assert.AreEqual(emu.stack[0], 0x0);
			Assert.AreEqual(emu.stackPointer, 0x0);
			//Jump
			emu.ExecuteInstruction(0x1647);
			Assert.AreEqual(emu.progCounter, 0x647);
			Assert.AreEqual(emu.stack[0], 0x0);
			Assert.AreEqual(emu.stackPointer, 0x0);
			//Jump
			emu.ExecuteInstruction(0x1432);
			Assert.AreEqual(emu.progCounter, 0x432);
			Assert.AreEqual(emu.stack[0], 0x0);
			Assert.AreEqual(emu.stackPointer, 0x0);
			//Jump
			emu.ExecuteInstruction(0x1A34);
			Assert.AreEqual(emu.progCounter, 0xA34);
			Assert.AreEqual(emu.stack[0], 0x0);
			Assert.AreEqual(emu.stackPointer, 0x0);

		}

		[Test]
		/// <summary>
		/// Tests skip to next instruction if V[x] is equal to NN
		/// Opcodes: 0x3XNN
		/// </summary>
		public void TestVXEqualNN()
		{
			emu.V[0x2] = 0x33;
			emu.progCounter = 0;
			emu.ExecuteInstruction(0x3234);
			emu.ExecuteInstruction(0x3233);
			Assert.AreEqual(emu.progCounter, 2);

		}


		[Test]
		/// <summary>
		/// Tests skip to next instruction if V[x] is not equal to NN.
		/// Opcodes: 0x4XNN
		/// </summary>
		public void TestVXNotEqualNN()
		{
			emu.V[0x2] = 0x33;
			emu.progCounter = 0;
			emu.ExecuteInstruction(0x4233);
			emu.ExecuteInstruction(0x4234);
			Assert.AreEqual(emu.progCounter, 2);
		}

		[Test]
		/// <summary>
		/// Tests skipping if v registers are equal
		/// Opcodes: 0x5XY0
		/// </summary>
		public void TestVXEqualVY()
		{
			emu.V[0x2] = 0xFF;
			emu.V[0x5] = 0xF0;
			emu.progCounter = 0;

			emu.ExecuteInstruction(0x5250);

			emu.progCounter = 0;
			emu.V[0x5] = 0xFF;

			emu.ExecuteInstruction(0x5250);
			Assert.AreEqual(emu.progCounter, 2);
		}

		[Test]
		/// <summary>
		/// Tests setting VX to NN
		/// Opcodes: 0x6XNN
		/// </summary>
		public void TestSetVXToNN() 
		{
			emu.progCounter = 0;

			emu.ExecuteInstruction(0x65B0);
			Assert.AreEqual(emu.V[0x5], 0xB0);
		}

		[Test]
		/// <summary>
		/// Tests adding NN to VX
		/// Opcodes: 0x7XNN
		/// </summary>
		public void TestAddNNToVX() 
		{
			emu.progCounter = 0;
			emu.V[0x5] = 0x0;

			emu.ExecuteInstruction(0x75B0);
			Assert.AreEqual(emu.V[0x5], 0xB0);

			emu.ExecuteInstruction(0x7550);
			Assert.AreEqual(emu.V[0x5], 0x0);
		}

		[Test]
		/// <summary>
		/// Tests setting VX to VY
		/// Opcodes: 0x8XY0
		/// </summary>
		public void TestSetVXToVY() 
		{
			emu.progCounter = 0;
			emu.V[0x5] = 0;
			emu.V[0x7] = 0xB0;

			emu.ExecuteInstruction(0x8570);
			Assert.AreEqual(emu.V[0x5], 0xB0);

			emu.progCounter = 0;
			emu.V[0x5] = 0xFF;
			emu.V[0x7] = 0xB0;

			emu.ExecuteInstruction(0x8570);
			Assert.AreEqual(emu.V[0x5], 0xB0);
		}

		[Test]
		/// <summary>
		/// Tests setting VX to VX or'd with VY
		/// Opcodes: 0x8XY1
		/// </summary>
		public void TestSetVXToVXOrVY() 
		{
			emu.progCounter = 0;
			emu.V[0x5] = 0x45;
			emu.V[0x7] = 0xB0;
			byte expected = (0x45 | 0xB0);

			emu.ExecuteInstruction(0x8571);
			Assert.AreEqual(emu.V[0x5], expected);

			emu.progCounter = 0;
			emu.V[0x5] = 0xFF;
			emu.V[0x7] = 0xB0;
			expected = (0xFF | 0xB0);

			emu.ExecuteInstruction(0x8571);
			Assert.AreEqual(emu.V[0x5], expected);
		}

		[Test]
		/// <summary>
		/// Tests and'ing VX and VY.
		/// Opcodes: 0x8XY2
		/// </summary>
		public void TestSetVXToVXAndVY() 
		{
			emu.progCounter = 0;
			emu.V[0x5] = 0x45;
			emu.V[0x7] = 0xB0;
			byte expected = (0x45 & 0xB0);

			emu.ExecuteInstruction(0x8572);
			Assert.AreEqual(emu.V[0x5], expected);

			emu.progCounter = 0;
			emu.V[0x5] = 0xFF;
			emu.V[0x7] = 0xB0;
			expected = (0xFF & 0xB0);

			emu.ExecuteInstruction(0x8572);
			Assert.AreEqual(emu.V[0x5], expected);
		}

		[Test]
		/// <summary>
		/// Tests xor'ing VX and VY.
		/// Opcodes: 0x8XY3
		/// </summary>
		public void TestSetVXToVXXorVY() 
		{
			emu.progCounter = 0;
			emu.V[0x5] = 0x45;
			emu.V[0x7] = 0xB0;
			byte expected = (0x45 ^ 0xB0);

			emu.ExecuteInstruction(0x8573);
			Assert.AreEqual(emu.V[0x5], expected);

			emu.progCounter = 0;
			emu.V[0x5] = 0xFF;
			emu.V[0x7] = 0xB0;
			expected = (0xFF ^ 0xB0);

			emu.ExecuteInstruction(0x8573);
			Assert.AreEqual(emu.V[0x5], expected);
		}

		[Test]
		/// <summary>
		/// Tests adding VY to VX. Set VF to 1 if theres a carry.
		/// Opcodes: 0x8XY4
		/// </summary>
		public void TestAddVYToVX() 
		{
			emu.progCounter = 0;
			emu.V[0xF] = 0xF;
			emu.V[0x5] = 0;
			emu.V[0x7] = 0xB0;
			byte expected = 0xB0;

			emu.ExecuteInstruction(0x8574);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 0);

			emu.V[0xF] = 0xF;
			emu.progCounter = 0;
			emu.V[0x5] = 0xFA;
			emu.V[0x7] = 0xB0;
			expected = (0xFA + 0xB0) & 0x00FF;

			emu.ExecuteInstruction(0x8574);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 1);
		}

		[Test]
		/// <summary>
		/// Tests subtracting VY from VX. Set VF to 0 if theres a borrow.
		/// Opcodes: 0x8XY5
		/// </summary>
		public void TestSubtractVYFromVX() 
		{
			emu.progCounter = 0;
			emu.V[0xF] = 0xF;
			emu.V[0x5] = 0;
			emu.V[0x7] = 0xB0;
			byte expected = (byte)(0x100 - 0xB0);

			emu.ExecuteInstruction(0x8575);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 0);

			emu.progCounter = 0;
			emu.V[0xF] = 0xF;
			emu.V[0x5] = 0xFA;
			emu.V[0x7] = 0xB0;
			expected = (0xFA - 0xB0);

			emu.ExecuteInstruction(0x8575);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 1);
		}

		[Test]
		/// <summary>
		/// Tests bit shifting VX right by one. Should store LSB in VF.
		/// Opcodes: 0x8XY8
		/// </summary>
		public void TestShiftVXRight() 
		{
			emu.progCounter = 0;
			emu.V[0x5] = 0xB0;
			byte expected = 0xB0 >> 1;

			emu.ExecuteInstruction(0x8576);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 0x0);

			emu.progCounter = 0;
			emu.V[0x5] = 0x0F;
			expected = 0x0F >> 1;

			emu.ExecuteInstruction(0x8576);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 0x1);

			emu.progCounter = 0;
			emu.V[0x5] = 0x56;
			expected = 0x56 >> 1;

			emu.ExecuteInstruction(0x8576);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 0x0);
		}

		[Test]
		/// <summary>
		/// Tests subtracting VX from VY. Set VF to 0 if theres a borrow.
		/// Opcodes: 0x8XY7
		/// </summary>
		public void TestSetVXToVYSubtractVX() 
		{
			emu.progCounter = 0;
			emu.V[0xF] = 0xF;
			emu.V[0x7] = 0;
			emu.V[0x5] = 0xB0;
			byte expected = (byte)(0x100 - 0xB0);

			emu.ExecuteInstruction(0x8577);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 0);

			emu.progCounter = 0;
			emu.V[0xF] = 0xF;
			emu.V[0x7] = 0xFA;
			emu.V[0x5] = 0xB0;
			expected = (0xFA - 0xB0);

			emu.ExecuteInstruction(0x8577);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 1);
		}

		[Test]
		/// <summary>
		/// Tests bit shifting VX left by one. Should store MSB in VF.
		/// Opcodes: 0x8XY8
		/// </summary>
		public void TestShiftVXLeft() 
		{
			emu.progCounter = 0;
			emu.V[0x5] = 0xB0;
			byte expected = (0xB0 << 1) & 0xFF;

			emu.ExecuteInstruction(0x857E);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 1);

			emu.progCounter = 0;
			emu.V[0x5] = 0x0F;
			expected = (0x0F << 1) & 0xFF;

			emu.ExecuteInstruction(0x857E);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 0);

			emu.progCounter = 0;
			emu.V[0x5] = 0x56;
			expected = (0x56 << 1) & 0xFF;;

			emu.ExecuteInstruction(0x857E);
			Assert.AreEqual(emu.V[0x5], expected);
			Assert.AreEqual(emu.V[0xF], 0);
		}

		[Test]
		/// <summary>
		/// Tests skipping if v registers are not equal
		/// Opcodes: 0x9XY0
		/// </summary>
		public void TestVXNotEqualVY()
		{
			emu.V[0x2] = 0xFF;
			emu.V[0x5] = 0xF0;
			emu.progCounter = 0;

			emu.ExecuteInstruction(0x9250);
			Assert.AreEqual(emu.progCounter, 2);

			emu.progCounter = 0;
			emu.V[0x5] = 0xFF;

			emu.ExecuteInstruction(0x9250);
		}

		[Test]
		/// <summary>
		/// Tests setting the index register to NNN.
		/// Opcodes: 0xANNN
		/// </summary>
		public void TestSetIToNNN() 
		{
			emu.progCounter = 0;
			emu.indexReg = 0;

			emu.ExecuteInstruction(0xA5AD);
			Assert.AreEqual(emu.indexReg, 0x5AD);
		}

		[Test]
		/// <summary>
		/// Test jumping to memory address (V0 + NNN).
		/// Opcodes: 0xBNNN
		/// </summary>
		public void TestJumpToV0PlusNNN() 
		{
			emu.progCounter = 0;
			emu.V[0x0] = 0;
			emu.ExecuteInstruction(0xB5AD);
			Assert.AreEqual(emu.progCounter, 0x5AD);


			emu.progCounter = 0;
			emu.V[0x0] = 0x4D;

			emu.ExecuteInstruction(0xB5AD);
			Assert.AreEqual(emu.progCounter, (0x5AD + 0x4D));
		}

		[Test]
		/// <summary>
		/// Test drawing a small image stored in memory at the index pointed to by I
		/// Opcodes: 0xDXYN
		/// </summary>
		public void TestDraw() 
		{
			emu.progCounter = 0;
			emu.indexReg = 0x0800;
			emu.memory[emu.indexReg] = 0x55;
			emu.memory[emu.indexReg + 1] = 0xAA;
			emu.memory[emu.indexReg + 2] = 0x55;
			emu.V[0x2] = 0x4;
			emu.V[0x5] = 0x2;

			/*Should draw at (4, 2):
			 * 01010101
			 * 10101010
			 * 01010101
			 */
			emu.ExecuteInstruction(0xD253);

			for (int y = 0; y < 3; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					Assert.AreEqual((x + y) % 2, emu.gfxBuf[(x + 4 + ((y + 2) * 64))]);
					Console.Write("{0}", emu.gfxBuf[(x + 4 + ((y + 2) * 64))]);
				}
				Console.Write("\n");
			}
			Console.Write("\n");
			//Do it again to see if VF is set properly
			emu.ExecuteInstruction(0xD253);
			Assert.AreEqual(1, emu.V[0xF]);
			for (int y = 0; y < 3; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					Assert.AreEqual(0, emu.gfxBuf[(x + 4 + ((y + 2) * 64))]);
					Console.Write("{0}", emu.gfxBuf[(x + 4 + ((y + 2) * 64))]);
				}
				Console.Write("\n");
			}

		}

		[Test]
		/// <summary>
		/// Tests skipping if key in VX is pressed
		/// Opcodes: 0xEX9E
		/// </summary>
		public void TestSkipIfPressed() 
		{
			emu.progCounter = 0;
            emu.V[0x4] = 0xB;
            emu.PressKey(0xC);
			emu.ExecuteInstruction(0xE49E);

			emu.progCounter = 0;
            emu.V[0x4] = 0xC;
            emu.PressKey(0xC);
			emu.ExecuteInstruction(0xE49E);
			Assert.AreEqual(2, emu.progCounter);
		}

		[Test]
		/// <summary>
		/// Tests skipping if key in VX is not pressed.
		/// Opcodes: 0xEXA1
		/// </summary>
		public void TestSkipIfNotPressed() 
		{
			emu.progCounter = 0;
            emu.V[0x4] = 0xC;
            emu.PressKey(0xC);
			emu.ExecuteInstruction(0xE4A1);

			emu.progCounter = 0;
            emu.V[0x4] = 0xB;
            emu.PressKey(0xC);
			emu.ExecuteInstruction(0xE4A1);
			Assert.AreEqual(2, emu.progCounter);
		}

		[Test]
		/// <summary>
		/// Tests setting VX to value of delay timer
		/// Opcodes: 0xFX07
		/// </summary>
		public void TestSetVXToDelay() 
		{
			emu.progCounter = 0;
			emu.delayTimer = 0xFF;
			emu.V[0x4] = 0xA;

			emu.ExecuteInstruction(0xF407);
			Assert.AreEqual(0xFF, emu.V[0x4]);
		}

		[Test]
		/// <summary>
		/// Tests blocking for a keypress and storing it in VX.
		/// Opcodes: 0xFX0A
		/// </summary>
		public void TestWaitForKeyPress() 
		{
			emu.progCounter = 0;

			emu.ExecuteInstruction(0xF40A);
			emu.Step();
			emu.Step();
			emu.Step();
			Assert.AreEqual(0, emu.progCounter);

			emu.PressKey(0xC);
			Assert.AreEqual(0xC, emu.V[0x4]);

		}

		[Test]
		/// <summary>
		/// Tests setting delay timer to VX
		/// Opcodes: 0xFX15
		/// </summary>
		public void TestSetDelayToVX() 
		{
			emu.progCounter = 0;
			emu.delayTimer = 0xF;
			emu.V[0x4] = 0xA1;

			emu.ExecuteInstruction(0xF415);
			Assert.AreEqual(0xA1, emu.delayTimer);
		}

		[Test]
		/// <summary>
		/// Tests setting sound timer to VX
		/// Opcodes: 0xFX18
		/// </summary>
		public void TestSetSoundToVX() 
		{
			emu.progCounter = 0;
			emu.soundTimer = 0xF;
			emu.V[0x4] = 0xA1;

			emu.ExecuteInstruction(0xF418);
			Assert.AreEqual(0xA1, emu.soundTimer);
		}

		[Test]
		/// <summary>
		/// Tests adding VX to I
		/// Opcodes: 0xFX1E
		/// </summary>
		public void TestAddVXToI() 
		{
			emu.progCounter = 0;
			emu.indexReg = 0x300;
			emu.V[0x7] = 0xA1;

			emu.ExecuteInstruction(0xF71E);
			Assert.AreEqual((0x300 + 0xA1), emu.indexReg);
		}

		[Test]
		/// <summary>
		/// Tests setting I to the memory location containing the font character represented in VX
		/// Opcodes: 0xFX29
		/// </summary>
		public void TestSetIToCharLocation() 
		{
			emu.progCounter = 0;
			emu.indexReg = 0x300;
			emu.V[0x6] = 0xA;

			emu.ExecuteInstruction(0xF629);

			Assert.AreEqual(0x32, emu.indexReg);

			emu.indexReg = 0x300;
			emu.V[0x6] = 0x6;

			emu.ExecuteInstruction(0xF629);

			Assert.AreEqual(0x1E, emu.indexReg);
		}

		[Test]
		/// <summary>
		/// Tests storing a binary coded decimal representation of VX at memory location I
		/// Opcodes: 0xFX33
		/// </summary>
		public void TestBCDStore() 
		{
			emu.progCounter = 0;
			emu.indexReg = 0x300;
			emu.V[0x7] = 0xAB;

			emu.ExecuteInstruction(0xF733);
			Assert.AreEqual(1, emu.memory[emu.indexReg]);
			Assert.AreEqual(7, emu.memory[emu.indexReg + 1]);
			Assert.AreEqual(1, emu.memory[emu.indexReg + 2]);
		}

		[Test]
		/// <summary>
		/// Tests storing V0 to VX starting at memory location I
		/// Opcodes: 0xFX55
		/// </summary>
		public void TestStoreV0ToVX() 
		{
			emu.progCounter = 0;
			emu.indexReg = 0x800;
			for (byte x = 0; x < 7; x++)
			{
				emu.V[x] = (byte)(x + 0x80);
			}

			emu.ExecuteInstruction(0xF755);

			for (byte x = 0; x < 7; x++)
			{
				Assert.AreEqual((x + 0x80), emu.memory[emu.indexReg + x]);
			}
		}

		[Test]
		/// <summary>
		/// Tests retrieving V0 to VX starting at memory location I
		/// Opcodes: 0xFX65
		/// </summary>
		public void TestRetrieveV0ToVX() 
		{
			emu.progCounter = 0;
			emu.indexReg = 0x800;
			for (byte x = 0; x < 7; x++)
			{
				emu.memory[emu.indexReg + x] = (byte)(x + 0x80);
				Console.Write("{0} ", emu.memory[emu.indexReg + x]);
			}
			Console.Write("\n");

			emu.ExecuteInstruction(0xF765);

			for (byte x = 0; x < 7; x++)
			{
				Console.Write("{0} ", emu.V[x]);
				Assert.AreEqual((x + 0x80), emu.V[x]);
			}
		}
	}
}

