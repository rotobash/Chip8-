using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chip8
{
	public class Chip8
	{
		public byte[] gfxBuf;

		internal ushort opCode;
		internal ushort indexReg;
		internal ushort progCounter;
		internal ushort stackPointer;

		internal byte delayTimer;
		internal byte soundTimer;

		internal byte[] memory;
		internal byte[] V;
		internal ushort[] stack;
		int KeyVXLocation;
        byte keyPressed;

		byte[] keyboard = { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };
		static Operation[] opFunctions = { Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine, A, B, C, D, E, F };
		static Operation[] eightOpFunctions = { EightZero, EightOne, EightTwo, EightThree, EightFour, EightFive, EightSix, EightSeven, NOP, NOP, NOP, NOP, NOP, NOP, EightE };

		static Operation[] fOpFunctions = { NOP, NOP, NOP, FThree, NOP, FFive, NOP, FSeven, FEight, FNine, FA, NOP, NOP, NOP, FE, NOP};

		//Font in bytes
		byte[] fontset = {
		  0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
		  0x20, 0x60, 0x20, 0x20, 0x70, // 1
		  0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
		  0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
		  0x90, 0x90, 0xF0, 0x10, 0x10, // 4
		  0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
		  0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
		  0xF0, 0x10, 0x20, 0x40, 0x40, // 7
		  0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
		  0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
		  0xF0, 0x90, 0xF0, 0x90, 0x90, // A
		  0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
		  0xF0, 0x80, 0x80, 0x80, 0xF0, // C
		  0xE0, 0x90, 0x90, 0x90, 0xE0, // D
		  0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
		  0xF0, 0x80, 0xF0, 0x80, 0x80  // F
		};

		internal KeyboardState keyState;
		Random rand;
		public Dictionary<Keys, byte> KeyboardTranslation;

		delegate void Operation(ushort op, Chip8 emu);

		public Chip8()
		{
			rand = new Random();
			memory = new byte[4096];
			V = new byte[16];
			keyboard = new byte[16];
			stack = new ushort[16];

			//dimensions
			gfxBuf = new byte[64 * 32];

			delayTimer = 0;
			soundTimer = 0;

			//start where game is loaded
			progCounter = 0x200;
			opCode = 0;
			indexReg = 0;
			stackPointer = 0;

			//load font
			for (int i = 0; i < 80; ++i)
				memory[i] = fontset[i];

			//translate byte representation of keys to a qwerty keyboard
			KeyboardTranslation = new Dictionary<Keys, byte>() {
				{Keys.X, 0x0},
				{Keys.D1, 0x1},
				{Keys.D2, 0x2},
				{Keys.D3, 0x3},
				{Keys.Q, 0x4},
				{Keys.W, 0x5},
				{Keys.E, 0x6},
				{Keys.A, 0x7},
				{Keys.S, 0x8},
				{Keys.D, 0x9},
				{Keys.Z, 0xA},
				{Keys.C, 0xB},
				{Keys.D4, 0xC},
				{Keys.R, 0xD},
				{Keys.F, 0xE},
				{Keys.V, 0xF},
                {Keys.None, 0xFF}
			};
			WaitForKey = false;
		}


		public void LoadGame(string file)
		{
			byte[] gameBytes = File.ReadAllBytes(file);

			//load game into memory
			for (int i = 0; i < gameBytes.Length; ++i)
				//chip 8 reserves the first 512 bytes for itself
				memory[i + 0x200] = gameBytes[i];
		}

		public void Step()
		{
			if (!WaitForKey) {
				//fetch
				byte firstByte = memory [progCounter];
				byte secondByte = memory [progCounter + 1];
                opCode = (ushort)(firstByte << 8 | secondByte);
                progCounter += 2;

				//decode & execute
                ExecuteInstruction(opCode);
			}

			if (delayTimer > 0)
				--delayTimer;

			if (soundTimer > 0)
			{
				--soundTimer;
				if (soundTimer != 0)
					Console.Beep();
			}

			keyState = Keyboard.GetState ();
		}

		public void ExecuteInstruction(ushort op) {
			opFunctions [(op & 0xF000) >> 12] (op, this);
		}

		public void Draw()
		{
			DrawFlag = false;
		}

		public bool DrawFlag
		{
			get;
			private set;
		}



		public bool WaitForKey
		{
			get;
			private set;
		}

		public void PressKey(byte key)
		{
            keyPressed = key;
            if (WaitForKey) 
            {
                V[KeyVXLocation] = key;
                WaitForKey = false;
				progCounter += 2;
			}
		}

		//Operations

		//0x0
		static void Zero(ushort op, Chip8 emu)
		{
			switch ((op & 0xFF00) >> 8)
			{
				case 0x00:
					{
						if ((op & 0x000F) == 0x000E)
						{
							--emu.stackPointer;
							emu.progCounter = emu.stack[emu.stackPointer];
						} else {
							//clear screen
							emu.gfxBuf = new byte[64 * 32];
						}
						break;
					}

				default:
					break;
			}
		}

		/// <summary>
		/// 0x1NNN
		/// Jump to the address at NNN
		/// </summary>
		/// <param name="op">Op containing address</param>
		/// <param name="emu">Emu.</param>
		static void One(ushort op, Chip8 emu)
		{
			emu.progCounter = (ushort)(op & 0x0FFF);
		}

		/// <summary>
		/// 0x2NNN
		/// Calls the address at NNN
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void Two(ushort op, Chip8 emu)
		{
			emu.stack[emu.stackPointer] = emu.progCounter;
			++emu.stackPointer;
			emu.progCounter = (ushort)(op & 0x0FFF);
		}

		/// <summary>
		/// 0x3XNN
		/// Skip instruction if V[X] == NN
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void Three(ushort op, Chip8 emu)
		{
			byte register = (byte)((op & 0x0F00) >> 8);
			if (emu.V[register] == (op & 0x00FF))
				emu.progCounter += 2;
		}

		/// <summary>
		/// 0x4XNN
		/// Skip instruction if V[X] != NN
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void Four(ushort op, Chip8 emu)
		{
			byte register = (byte)((op & 0x0F00) >> 8);
			if (emu.V[register] != (op & 0x00FF))
				emu.progCounter += 2;
		}

		//0x5
		/// <summary>
		/// 0x5XY0
		/// Skip instruction if V[X] == V[Y]
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void Five(ushort op, Chip8 emu)
		{
			byte X = (byte)((op & 0x0F00) >> 8);
			byte Y = (byte)((op & 0x00F0) >> 4);
			if (emu.V[X] == emu.V[Y])
				emu.progCounter += 2;
		}

		//0x6
		/// <summary>
		/// 0x6XNN
		/// Set V[X]to NN
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void Six(ushort op, Chip8 emu)
		{
			byte X = (byte)((op & 0x0F00) >> 8);
			emu.V[X] = (byte)(op & 0x00FF);
		}

		//0x7
		/// <summary>
		/// 0x7XNN
		/// Adds NN to V[X]
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void Seven(ushort op, Chip8 emu)
		{
			byte X = (byte)((op & 0x0F00) >> 8);
            ushort temp = (ushort)(emu.V[X] + (op & 0x00FF));
            emu.V[X] = (temp < 256) ? (byte)temp : (byte)(temp - 256);
		}

		//0x8
		static void Eight(ushort op, Chip8 emu)
		{
			eightOpFunctions[op & 0x000F](op, emu);
		}

		//0x9
		/// <summary>
		/// 0x9XY0
		/// Skip instruction if V[X] == V[Y]
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void Nine(ushort op, Chip8 emu)
		{
			byte X = (byte)((op & 0x0F00) >> 8);
			byte Y = (byte)((op & 0x00F0) >> 4);
			if (emu.V[X] != emu.V[Y])
				emu.progCounter += 2;
		}

		//0xA
		static void A(ushort op, Chip8 emu)
		{
			emu.indexReg = (ushort)(op & 0x0FFF);
		}

		//0xB
		static void B(ushort op, Chip8 emu)
		{
			emu.progCounter = (ushort)(emu.V[0] + (op & 0x0FFF));
		}

		//0xC
		static void C(ushort op, Chip8 emu)
		{
			byte X = (byte)((op & 0x0F00) >> 8);
			byte number = (byte)(op & 0x00FF);
			emu.V[X] = (byte)(emu.rand.Next(0, 256) & number);
		}

		//0xD
		/// <summary>
		/// 0xDXYR
		/// 
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void D(ushort op, Chip8 emu)
		{
			int rows = op & 0x000F;
			int xReg = (op & 0x0F00) >> 8;
			int yReg = (op & 0x00F0) >> 4;
			ushort x = emu.V[xReg];
			ushort y = emu.V[yReg];
			ushort pixel;

			emu.V[0xF] = 0;

			for (int yline = 0; yline < rows; yline++)
			{
				pixel = emu.memory[emu.indexReg + yline];
                int ycoord = (yline + y) % 32;
				for(int xline = 0; xline < 8; xline++)
                {
                    int xcoord = (xline + x) % 64;
					//take each bit of the byte stored at memory at I + yline that is 1 and draw it
					if((pixel & (0x80 >> xline)) != 0)
					{
                        int position = xcoord + (ycoord * 64);

						if (position >= 2048)
							break;
						
						if(emu.gfxBuf[position] == 1)
							emu.V[0xF] = 1;                                 
						emu.gfxBuf[position] ^= 1;
					}
				}
			}

			emu.DrawFlag = true;
		}

		//0xE
		static void E(ushort op, Chip8 emu)
		{
			byte X = (byte)((op & 0x0F00) >> 8);
            emu.KeyVXLocation = emu.V[X];

			switch (op & 0x00FF)
			{
				case 0x9E:
					{
                        if (emu.V[X] == emu.keyPressed)
                            emu.progCounter += 2;
                        else
                            emu.keyPressed = 0xFF;
					}
					break;

				case 0xA1:
                    {
                        if (emu.V[X] != emu.keyPressed)
                            emu.progCounter += 2;
                        else
                            emu.keyPressed = 0xFF;
					}
					break;
			}
		}

		//0xF
		static void F(ushort op, Chip8 emu)
		{
			fOpFunctions[op & 0x000F](op, emu);
		}

		/// <summary>
		/// 0x8XY0
		/// Sets VX to VY
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void EightZero(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			int Y = (op & 0x00F0) >> 4;

			emu.V[X] = emu.V[Y];
		}

		/// <summary>
		/// 0x8XY1
		/// Sets VX to (VX or VY)
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void EightOne(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			int Y = (op & 0x00F0) >> 4;

			emu.V[X] |= emu.V[Y];
		}
			
		/// <summary>
		/// 0x8XY2
		/// Sets VX to (VX and VY)
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void EightTwo(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			int Y = (op & 0x00F0) >> 4;

			emu.V[X] &= emu.V[Y];
		}

		/// <summary>
		/// 0x8XY3
		/// Sets VX to (VX xor VY)
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void EightThree(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			int Y = (op & 0x00F0) >> 4;

			emu.V[X] ^= emu.V[Y];
		}

		/// <summary>
		/// 0x8XY4
		/// Adds VY to VX and set VF to 1 if there's a carry
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void EightFour(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			int Y = (op & 0x00F0) >> 4;

			ushort val = (ushort)(emu.V[X] + emu.V[Y]);
			emu.V[0xF] = val > 0xFF ? (byte)0x1 : (byte)0x0;
			emu.V[X] = (byte)val;
		}

		/// <summary>
		/// 0x8XY5
		/// Subtracts VY from VX and set VF to 0 if there's a borrow
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void EightFive(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			int Y = (op & 0x00F0) >> 4;

            if(emu.V[X] < emu.V[Y]) 
            {
                emu.V[0xF] = (byte)0x0;
                emu.V[X] = (byte)(256 + (emu.V[X] - emu.V[Y]));
            } else {
                emu.V[0xF] = (byte)0x1;
                emu.V[X] = (byte)(emu.V[X] - emu.V[Y]);
            }
		}

		/// <summary>
		/// 0x8XY6
		/// Shifts VY right by one and stores in VX then set VF to the LSB
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void EightSix(ushort op, Chip8 emu)
		{
            int X = (op & 0x0F00) >> 8;
            int Y = (op & 0x00F0) >> 4;

            emu.V[0xF] = (byte)(emu.V[Y] & 0x1);
			emu.V[X] = (byte)(emu.V[Y] >> 1);
		}

		/// <summary>
		/// 0x8XY7
		/// Sets VX to (VY - VX) and sets VF to 0 if there's a borrow
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void EightSeven(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			int Y = (op & 0x00F0) >> 4;

            if(emu.V[Y] < emu.V[X]) 
            {
                emu.V[0xF] = (byte)0x0;
                emu.V[X] = (byte)(emu.V[Y] - emu.V[X]);
            } else {
                emu.V[X] = (byte)(256 + (emu.V[Y] - emu.V[X]));
                emu.V[0xF] = (byte)0x1;
            }
		}


		/// <summary>
		/// 0x8XYE
		/// Shifts VY left by one and stored in VX then set VF to the MSB
		/// </summary>
		/// <param name="op">Op.</param>
		/// <param name="emu">Emu.</param>
		static void EightE(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			int Y = (op & 0x00F0) >> 4;

			byte MSB = (byte)((emu.V[Y] & 0x80) >> 7);
			emu.V[X] = (byte)((emu.V[Y] << 1) & 0xFF);
			emu.V[0xF] = MSB;
		}

		static void FSeven(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			emu.V[X] = emu.delayTimer;
		}

		static void FA(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			emu.KeyVXLocation = X;
			emu.WaitForKey = true;

		}

		static void FFive(ushort op, Chip8 emu)
        {
            int X = (op & 0x0F00) >> 8;

			switch ((op & 0x00F0) >> 4)
			{
				case 0x1:
					{
						emu.delayTimer = emu.V[X];
					}
					break;
				
				case 0x5:
					{
						for (int i = 0; i <= X; ++i)
						{
							emu.memory[emu.indexReg + i] = emu.V[i];
						}
					}
					break;

				case 0x6:
					{
						for (int i = 0; i <= X; ++i)
						{
							emu.V[i] = emu.memory[emu.indexReg + i];
						}
					}
					break;
			}
		}

		static void FEight(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			emu.soundTimer = emu.V[X];
		}

		static void FE(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;
			emu.indexReg += emu.V[X];
		}

		static void FNine(ushort op, Chip8 emu)
		{
			int X = (op & 0x0F00) >> 8;

			emu.indexReg = (ushort)(emu.V[X] * 5);
		}

		static void FThree(ushort op, Chip8 emu)
		{
			emu.memory[emu.indexReg] = (byte)(emu.V[(op & 0x0F00) >> 8] / 100);
			emu.memory[emu.indexReg + 1] = (byte)((emu.V[(op & 0x0F00) >> 8] / 10) % 10);
			emu.memory[emu.indexReg + 2] = (byte)((emu.V[(op & 0x0F00) >> 8] % 100) % 10);
		}

		static void NOP(ushort op, Chip8 emu)
		{
			
		}

	}

}
