using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Volkswagen SA2 implementation from https://github.com/bri3d/sa2_seed_key
    /// </summary>
    class VolkswagenSA2 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] tape = GetParameterBytearray(parameters, "InstructionTape");

            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            SA2SeedKey sk = new SA2SeedKey(tape, BytesToInt(inSeed, Endian.Big, 0));
            IntToBytes(sk.Execute(), outKey, Endian.Big);
            return true;
        }

        public override string GetProviderName()
        {
            return "VolkswagenSA2";
        }
    }


    public class SA2SeedKey
    {
        uint Register = 0;
        uint CarryFlag = 0;
        byte[] InstructionTape = Array.Empty<byte>();
        uint InstructionPointer = 0;
        List<uint> ForPointers = new List<uint>();
        List<int> ForIterations = new List<int>();

        public SA2SeedKey(byte[] tape, uint seed)
        {
            InstructionTape = tape;
            Register = seed;
        }

        private void RegisterShiftLeft()
        {
            CarryFlag = Register & 0x80000000;
            Register <<= 1;
            if (CarryFlag > 0)
            {
                Register |= 1;
            }
            InstructionPointer++;
        }

        private void RegisterShiftRight()
        {
            CarryFlag = Register & 1;
            Register >>= 1;
            if (CarryFlag > 0)
            {
                Register |= 0x80000000;
            }
            InstructionPointer++;
        }

        private void Add()
        {
            CarryFlag = 0;
            long outputRegister = Register;
            outputRegister += FetchOperand();
            if (outputRegister > 0xFFFFFFFF)
            {
                CarryFlag = 1;
                outputRegister &= 0xFFFFFFFF;
            }
            Register = (uint)outputRegister;
            InstructionPointer += 5;
        }

        private void Subtract()
        {
            CarryFlag = 0;
            long outputRegister = Register;
            outputRegister -= FetchOperand();
            if (outputRegister > 0xFFFFFFFF)
            {
                CarryFlag = 1;
                outputRegister &= 0xFFFFFFFF;
            }
            Register = (uint)outputRegister;
            InstructionPointer += 5;
        }

        private void ExclusiveOr()
        {
            uint op = FetchOperand();
            Register ^= op;
            InstructionPointer += 5;
        }

        private void LoopInit()
        {
            ForIterations.Insert(0, FetchOperandByte() - 1);
            InstructionPointer += 2;
            ForPointers.Insert(0, InstructionPointer);
        }

        private void LoopNext()
        {
            if (ForIterations[0] > 0)
            {
                ForIterations[0]--;
                InstructionPointer = ForPointers[0];
            }
            else
            {
                ForIterations.RemoveAt(0);
                ForPointers.RemoveAt(0);
                InstructionPointer++;
            }
        }

        private void BranchConditional()
        {
            uint skipCount = (uint)FetchOperandByte() + 2;
            if (CarryFlag == 0)
            {
                InstructionPointer += skipCount;
            }
            else
            {
                InstructionPointer += 2;
            }
        }

        private void BranchUnconditional()
        {
            InstructionPointer += (uint)FetchOperandByte() + 2;
        }

        private void EndExecution()
        {
            InstructionPointer++;
        }

        private uint FetchOperand()
        {
            byte[] operands = InstructionTape.Skip((int)InstructionPointer + 1).Take(4).ToArray();
            uint interpretedInt = (uint)operands[0] << 24 | (uint)operands[1] << 16 | (uint)operands[2] << 8 | (uint)operands[3];
            return interpretedInt;
        }
        private byte FetchOperandByte()
        {
            return InstructionTape[InstructionPointer + 1];
        }

        public uint Execute()
        {
            Dictionary<int, Action> instructionSet = new Dictionary<int, Action>();
            instructionSet.Add(0x81, RegisterShiftLeft);
            instructionSet.Add(0x82, RegisterShiftRight);
            instructionSet.Add(0x93, Add);
            instructionSet.Add(0x84, Subtract);
            instructionSet.Add(0x87, ExclusiveOr);
            instructionSet.Add(0x68, LoopInit);
            instructionSet.Add(0x49, LoopNext);
            instructionSet.Add(0x4A, BranchConditional);
            instructionSet.Add(0x6B, BranchUnconditional);
            instructionSet.Add(0x4C, EndExecution);

            while (InstructionPointer < InstructionTape.Length)
            {
                // Uncomment to trace execution
                // Console.WriteLine($"IP: 0x{InstructionPointer:X} Reg: 0x{Register:X8} CF: 0x{CarryFlag:X8} Op: {instructionSet[InstructionTape[InstructionPointer]].Method.Name} ");
                instructionSet[InstructionTape[InstructionPointer]]();
            }
            Console.WriteLine($"Exec done, reg = 0x{Register:X8}");
            return Register;
        }
    }
}
