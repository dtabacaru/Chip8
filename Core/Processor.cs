using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace Chip8.Core
{
    public class DisplayEventArgs:EventArgs
    {
        public bool[,] Display;

        public DisplayEventArgs(bool[,] display)
        {
            Display = display;
        }
    }

    public class Processor
    {
        private const int STACK_SIZE = 1024;
        private const double CYCLE_TIME = (1.0 / CPU_CONSTANTS.TIMER_FREQUENCY) * 0.1;

        private Stack<int> m_Stack = new Stack<int>(STACK_SIZE);
        private byte[] m_DataRegisters = new byte[CPU_CONSTANTS.NUM_DATA_REGISTERS];
        private byte[] m_InstructionBytes = new byte[CPU_CONSTANTS.INSTRUCTION_BYTE_LEN];
        private int m_AdrressRegister = 0;
        private int m_Instruction = 0;
        private int m_ExecutionPointer = CPU_CONSTANTS.RESET_VECTOR;
        private byte[] m_Memory = new byte[CPU_CONSTANTS.MEMORY_BYTE_LEN];
        private Timer m_DelayTimer = new Timer();
        private volatile byte m_DelayTimerRegister = 0;
        private Timer m_KeyPressResetTimer = new Timer();
        private volatile Chip8Keys m_KeyPressed = Chip8Keys.UNKNOWN;
        public  bool[,] m_Display = new bool[CPU_CONSTANTS.DISPLAY_WIDTH,CPU_CONSTANTS.DISPLAY_HEIGHT];

        public delegate void DisplayEventDelegate(object sender, DisplayEventArgs e);
        public event DisplayEventDelegate DisplayEvent;

        private FileInfo m_RomFile;

        public bool DebugLog = false;
        public bool WrapY = true;

        public volatile bool Stop = false;
        public volatile bool Pause = false;

        private bool m_SkipNext = false;
        private bool m_BackTrack = false;

        private Random m_Random = new Random();
        private Stopwatch m_ProcessorCycleStopWatch = new Stopwatch();
        private Stopwatch m_BlockStopWatch = new Stopwatch();
        private Stopwatch m_DebugStopWatch = new Stopwatch();
        private StreamWriter m_DebugStream = null;

        public Processor Copy()
        {
            Processor savedProcessor = new Processor(m_RomFile.FullName);
            savedProcessor.m_Stack = new Stack<int>(m_Stack.ToArray());
            Array.Copy(m_DataRegisters, savedProcessor.m_DataRegisters, m_DataRegisters.Length);
            Array.Copy(m_InstructionBytes, savedProcessor.m_InstructionBytes, m_InstructionBytes.Length);
            savedProcessor.m_AdrressRegister = m_AdrressRegister;
            savedProcessor.m_Instruction = m_Instruction;
            savedProcessor.m_ExecutionPointer = m_ExecutionPointer;
            Array.Copy(m_Memory, savedProcessor.m_Memory, m_Memory.Length);
            savedProcessor.m_DelayTimerRegister = m_DelayTimerRegister;
            savedProcessor.m_KeyPressed = m_KeyPressed;
            for (int i = 0; i < m_Display.GetLength(0); i++)
                for (int j = 0; j < m_Display.GetLength(1); j++)
                    savedProcessor.m_Display[i, j] = m_Display[i, j];

            savedProcessor.DebugLog = DebugLog;
            savedProcessor.WrapY = WrapY;

            return savedProcessor;
        }

        public void KeyPress(char keyCode)
        {
            m_KeyPressResetTimer.Stop();

            switch ((int)keyCode)
            {
                case 120: // X
                    m_KeyPressed = Chip8Keys.KEY_0;
                    break;
                case 49:  // 1
                    m_KeyPressed = Chip8Keys.KEY_1;
                    break;
                case 50:  // 2
                    m_KeyPressed = Chip8Keys.KEY_2;
                    break;
                case 51:  // 3
                    m_KeyPressed = Chip8Keys.KEY_3;
                    break;
                case 113: // Q
                    m_KeyPressed = Chip8Keys.KEY_4;
                    break;
                case 119: // W
                    m_KeyPressed = Chip8Keys.KEY_5;
                    break;
                case 101: // E
                    m_KeyPressed = Chip8Keys.KEY_6;
                    break;
                case 97:  // A
                    m_KeyPressed = Chip8Keys.KEY_7;
                    break;
                case 115: // S
                    m_KeyPressed = Chip8Keys.KEY_8;
                    break;
                case 100: // D
                    m_KeyPressed = Chip8Keys.KEY_9;
                    break;
                case 122: // Z
                    m_KeyPressed = Chip8Keys.KEY_A;
                    break;
                case 99:  // C
                    m_KeyPressed = Chip8Keys.KEY_B;
                    break;
                case 52:  // 4
                    m_KeyPressed = Chip8Keys.KEY_C;
                    break;
                case 114: // R
                    m_KeyPressed = Chip8Keys.KEY_D;
                    break;
                case 102: // F
                    m_KeyPressed = Chip8Keys.KEY_E;
                    break;
                case 118: // V
                    m_KeyPressed = Chip8Keys.KEY_F;
                    break;
                default:
                    m_KeyPressed = Chip8Keys.UNKNOWN;
                    break;
            }

            m_KeyPressResetTimer.Start();
        }

        public void Run()
        {
            if(DebugLog)
            {
                if(m_DebugStream == null)
                    m_DebugStream = new StreamWriter(m_RomFile.Name + DateTime.Now.ToString("-yyyy-MM-dd-hh-mm-ss") + ".ch8log");

                while (!Stop)
                {
                    while (Pause)
                        Block(0.001);

                    m_ProcessorCycleStopWatch.Restart();
                    ProcessInstructionDebug();
                    WaitCycle(CYCLE_TIME);
                }
            }
            else
            {
                while (!Stop)
                {
                    while (Pause)
                        Block(0.001);

                    m_ProcessorCycleStopWatch.Restart();
                    ProcessInstruction();
                    WaitCycle(CYCLE_TIME);
                }
            }

            Stop = false;
        }

        public void UpdateDisplay(int startX, int startY, int rows)
        {
            m_DataRegisters[0x0F] = 0x00;

            int wrappedX = 0;
            int wrappedY = 0;
            bool pixelSet = false;
            for (int x = startX; x < (startX + CPU_CONSTANTS.SPRITE_LEN); x++)
            {
                wrappedX = x%CPU_CONSTANTS.DISPLAY_WIDTH;

                for (int y = startY; y < (startY + rows); y++)
                {
                    if(!WrapY && y >= CPU_CONSTANTS.DISPLAY_HEIGHT)
                        continue;

                    wrappedY = y%CPU_CONSTANTS.DISPLAY_HEIGHT;

                    pixelSet = (m_Memory[m_AdrressRegister + (y - startY)] >> (7 - (x - startX)) & 0x01) == 1;

                    if (m_Display[wrappedX, wrappedY] && pixelSet)
                        m_DataRegisters[0x0F] = 0x01;

                    m_Display[wrappedX, wrappedY] = m_Display[wrappedX, wrappedY] ^ pixelSet;
                }
            }

            DisplayEvent?.Invoke(this, new DisplayEventArgs(m_Display));
        }

        private void ProcessInstruction()
        {
            m_InstructionBytes[1] = m_Memory[m_ExecutionPointer++];
            m_InstructionBytes[0] = m_Memory[m_ExecutionPointer++];
            m_Instruction = BitConverter.ToUInt16(m_InstructionBytes, 0);

            switch ((CHIP_8_INSTRUCTIONS)(m_Instruction >> 12))
            {
                case CHIP_8_INSTRUCTIONS.FUNC1:

                    switch ((MISC_SUB_INSTRUCTIONS)(m_Instruction & 0xFFF))
                    {
                        case MISC_SUB_INSTRUCTIONS.CLS:
                            ClearDisplay();
                            break;
                        case MISC_SUB_INSTRUCTIONS.RET:
                            m_ExecutionPointer = m_Stack.Pop();
                            break;
                        default:
                            break;
                    }
                    break;
                case CHIP_8_INSTRUCTIONS.GOTO:
                    m_ExecutionPointer = m_Instruction & 0xFFF;
                    break;
                case CHIP_8_INSTRUCTIONS.CALL:
                    m_Stack.Push(m_ExecutionPointer);
                    m_ExecutionPointer = m_Instruction & 0xFFF;
                    break;
                case CHIP_8_INSTRUCTIONS.EQUALS_CTE:
                    m_SkipNext = m_DataRegisters[(m_Instruction & 0xF00) >> 8] == (m_Instruction & 0xFF);
                    m_ExecutionPointer = m_SkipNext ? (m_ExecutionPointer + 2) : m_ExecutionPointer;
                    break;
                case CHIP_8_INSTRUCTIONS.NOT_EQUALS_CTE:
                    m_SkipNext = m_DataRegisters[(m_Instruction & 0xF00) >> 8] != (m_Instruction & 0xFF);
                    m_ExecutionPointer = m_SkipNext ? (m_ExecutionPointer + 2) : m_ExecutionPointer;
                    break;
                case CHIP_8_INSTRUCTIONS.EQUALS_VAR:
                    switch (m_Instruction & 0xF)
                    {
                        case 0x0:
                            m_SkipNext = m_DataRegisters[(m_Instruction & 0xF00) >> 8] == m_DataRegisters[(m_Instruction & 0x0F0) >> 4];
                            m_ExecutionPointer = m_SkipNext ? (m_ExecutionPointer + 2) : m_ExecutionPointer;
                            break;
                        default:
                            break;
                    }
                    break;
                case CHIP_8_INSTRUCTIONS.SET_CTE:
                    m_DataRegisters[(m_Instruction & 0xF00) >> 8] = (byte)(m_Instruction & 0xFF);
                    break;
                case CHIP_8_INSTRUCTIONS.ADD_CTE:
                    m_DataRegisters[(m_Instruction & 0xF00) >> 8] += (byte)(m_Instruction & 0xFF);
                    break;
                case CHIP_8_INSTRUCTIONS.MANIP_VAR:
                    switch ((MANIP_VAR_SUB_INSTRUCTIONS)(m_Instruction & 0x000F))
                    {
                        case MANIP_VAR_SUB_INSTRUCTIONS.SET_VAR:
                            m_DataRegisters[(m_Instruction & 0xF00) >> 8] = m_DataRegisters[(m_Instruction & 0xF0) >> 4];
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.OR_VAR:
                            m_DataRegisters[(m_Instruction & 0xF00) >> 8] |= m_DataRegisters[(m_Instruction & 0xF0) >> 4];
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.AND_VAR:
                            m_DataRegisters[(m_Instruction & 0xF00) >> 8] &= m_DataRegisters[(m_Instruction & 0xF0) >> 4];
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.XOR_VAR:
                            m_DataRegisters[(m_Instruction & 0xF00) >> 8] ^= m_DataRegisters[(m_Instruction & 0xF0) >> 4];
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.ADD_VAR:
                            if ((m_DataRegisters[(m_Instruction & 0xF00) >> 8] + m_DataRegisters[(m_Instruction & 0xF0) >> 4]) > 255)
                                m_DataRegisters[0xF] = 1;
                            else
                                m_DataRegisters[0xF] = 0;
                            m_DataRegisters[(m_Instruction & 0xF00) >> 8] += m_DataRegisters[(m_Instruction & 0xF0) >> 4];
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.SUBTRACT_VAR:
                            if (m_DataRegisters[(m_Instruction & 0xF00) >> 8] > m_DataRegisters[(m_Instruction & 0xF0) >> 4])
                                m_DataRegisters[0xF] = 1;
                            else
                                m_DataRegisters[0xF] = 0;
                            m_DataRegisters[(m_Instruction & 0xF00) >> 8] -= m_DataRegisters[(m_Instruction & 0xF0) >> 4];
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.RIGHT_SHIFT:
                            m_DataRegisters[0xF] = (byte)(m_DataRegisters[(m_Instruction & 0xF00) >> 8] & 0x01);
                            m_DataRegisters[(m_Instruction & 0xF00) >> 8] = (byte)(m_DataRegisters[(m_Instruction & 0xF00) >> 8] >> 1);
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.VAR_SUBTRACT:
                            if (m_DataRegisters[(m_Instruction & 0xF0) >> 4] > m_DataRegisters[(m_Instruction & 0xF00) >> 8])
                                m_DataRegisters[0xF] = 1;
                            else
                                m_DataRegisters[0xF] = 0;
                            m_DataRegisters[(m_Instruction & 0xF00) >> 8] = (byte)(m_DataRegisters[(m_Instruction & 0xF0) >> 4] - m_DataRegisters[(m_Instruction & 0xF00) >> 8]);
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.LEFT_SHIFT:
                            m_DataRegisters[0xF] = (byte)((m_DataRegisters[(m_Instruction & 0xF00) >> 8] & 0x80) >> 7);
                            m_DataRegisters[(m_Instruction & 0xF00) >> 8] = (byte)(m_DataRegisters[(m_Instruction & 0xF00) >> 8] << 1);
                           break;
                        default:
                            break;
                    }
                    break;
                case CHIP_8_INSTRUCTIONS.NOT_EQUALS_VAR:
                    switch (m_Instruction & 0xF)
                    {
                        case 0x0:
                            m_SkipNext = m_DataRegisters[(m_Instruction & 0xF00) >> 8] != m_DataRegisters[(m_Instruction & 0x0F0) >> 4];
                            m_ExecutionPointer = m_SkipNext ? (m_ExecutionPointer + 2) : m_ExecutionPointer;
                            break;
                        default:
                            break;
                    }
                    break;
                case CHIP_8_INSTRUCTIONS.SET_ADDR:
                    m_AdrressRegister = (m_Instruction & 0xFFF);
                    break;
                case CHIP_8_INSTRUCTIONS.GOTO_PLUS:
                    m_ExecutionPointer = ((m_Instruction & 0xFFF) + m_DataRegisters[0]);
                    break;
                case CHIP_8_INSTRUCTIONS.RAND:
                    m_DataRegisters[(m_Instruction & 0xF00) >> 8] = (byte)(((int)Math.Round((m_Random.NextDouble() * 255))) & (m_Instruction & 0xFF));
                    break;
                case CHIP_8_INSTRUCTIONS.DISP:
                    UpdateDisplay(m_DataRegisters[(m_Instruction & 0xF00) >> 8], m_DataRegisters[(m_Instruction & 0xF0) >> 4], m_Instruction & 0xF);
                    break;
                case CHIP_8_INSTRUCTIONS.KEY_SKIP:
                    switch ((KEY_SKIP_SUB_INSTRUCTIONS)(m_Instruction & 0x0FF))
                    {
                        case KEY_SKIP_SUB_INSTRUCTIONS.PRESSED:
                            m_SkipNext = (byte)m_KeyPressed == m_DataRegisters[(m_Instruction & 0xF00) >> 8];
                            m_ExecutionPointer = m_SkipNext ? (m_ExecutionPointer + 2) : m_ExecutionPointer;
                            m_KeyPressed = m_SkipNext ? Chip8Keys.UNKNOWN : m_KeyPressed;
                            break;
                        case KEY_SKIP_SUB_INSTRUCTIONS.NOT_PRESSED:
                            m_SkipNext = (byte)m_KeyPressed != m_DataRegisters[(m_Instruction & 0xF00) >> 8];
                            m_ExecutionPointer = m_SkipNext ? (m_ExecutionPointer + 2) : m_ExecutionPointer;
                            m_KeyPressed = m_SkipNext ? m_KeyPressed : Chip8Keys.UNKNOWN;
                            break;
                        default:
                            break;
                    }
                    break;
                case CHIP_8_INSTRUCTIONS.FUNC2:
                    switch ((FUNCTION_SUB_INSTRUCTIONS)(m_Instruction & 0xFF))
                    {
                        case FUNCTION_SUB_INSTRUCTIONS.SET_VAR_DELAY_TIMER:
                            m_DataRegisters[(m_Instruction & 0xF00) >> 8] = m_DelayTimerRegister;
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.AWAIT_KEYPRESS:
                            m_BackTrack = m_KeyPressed == Chip8Keys.UNKNOWN;
                            m_DataRegisters[(m_Instruction & 0xF00) >> 8] = m_BackTrack ? m_DataRegisters[(m_Instruction & 0xF00) >> 8] : (byte)m_KeyPressed;
                            m_ExecutionPointer = m_BackTrack ? (m_ExecutionPointer - 2) : m_ExecutionPointer;
                            m_KeyPressed = Chip8Keys.UNKNOWN;
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.SET_DELAY_TIMER_VAR:
                            m_DelayTimerRegister = m_DataRegisters[(m_Instruction & 0xF00) >> 8];
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.SET_SOUND_TIMER_VAR:
                            Task.Run(() =>
                            {
                                Console.Beep(1000, (int)(m_DataRegisters[(m_Instruction & 0xF00) >> 8] * (1.0 / CPU_CONSTANTS.TIMER_FREQUENCY) * 1000));
                            });
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.ADD_ADDR_VAR:
                            m_AdrressRegister += m_DataRegisters[(m_Instruction & 0xF00) >> 8];
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.SET_ADDR_SPRITE:
                            m_AdrressRegister = (m_DataRegisters[(m_Instruction & 0xF00) >> 8] * 5);
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.BCD_VAR:
                            m_Memory[m_AdrressRegister] = (byte)(m_DataRegisters[(m_Instruction & 0xF00) >> 8] / 100);
                            m_Memory[m_AdrressRegister + 1] = (byte)((m_DataRegisters[(m_Instruction & 0xF00) >> 8] - m_Memory[m_AdrressRegister] * 100) / 10);
                            m_Memory[m_AdrressRegister + 2] = (byte)(m_DataRegisters[(m_Instruction & 0xF00) >> 8] - m_Memory[m_AdrressRegister] * 100 - m_Memory[m_AdrressRegister + 1] * 10);
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.REG_DUMP:
                            for (int i = 0; i <= ((m_Instruction & 0xF00) >> 8); i++)
                                m_Memory[m_AdrressRegister + i] = m_DataRegisters[i];
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.REG_LOAD:
                            for (int i = 0; i <= ((m_Instruction & 0xF00) >> 8); i++)
                                m_DataRegisters[i] = m_Memory[m_AdrressRegister + i];
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private void ProcessInstructionDebug()
        {
            m_DebugStopWatch.Restart();

            ProcessInstruction();

            double elapsed = m_DebugStopWatch.ElapsedTicks;

            m_DebugStream.WriteLine("ADDR: 0x" + (m_ExecutionPointer - 2).ToString("X4") + " INSTR: 0x" + m_Instruction.ToString("X4"));

            switch ((CHIP_8_INSTRUCTIONS)(m_Instruction >> 12))
            {
                case CHIP_8_INSTRUCTIONS.FUNC1:

                    switch ((MISC_SUB_INSTRUCTIONS)(m_Instruction & 0xFFF))
                    {
                        case MISC_SUB_INSTRUCTIONS.CLS:
                            m_DebugStream.WriteLine("> Clear display");
                            break;
                        case MISC_SUB_INSTRUCTIONS.RET:
                            m_DebugStream.WriteLine("> Return.");
                            break;
                        default:
                            break;
                    }
                    break;
                case CHIP_8_INSTRUCTIONS.GOTO:
                    m_DebugStream.WriteLine("> Go to 0x" + (m_Instruction & 0xFFF).ToString("X4"));
                    break;
                case CHIP_8_INSTRUCTIONS.CALL:
                    m_DebugStream.WriteLine("> Call 0x" + (m_Instruction & 0xFFF).ToString("X4"));
                    break;
                case CHIP_8_INSTRUCTIONS.EQUALS_CTE:
                    m_DebugStream.WriteLine("> Skip next if V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " equals 0x" + (m_Instruction & 0xFF).ToString("X2"));
                    break;
                case CHIP_8_INSTRUCTIONS.NOT_EQUALS_CTE:
                    m_DebugStream.WriteLine("> Skip next if V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " does not equal 0x" + (m_Instruction & 0xFF).ToString("X2"));
                    break;
                case CHIP_8_INSTRUCTIONS.EQUALS_VAR:
                    switch (m_Instruction & 0xF)
                    {
                        case 0x0:
                            m_DebugStream.WriteLine("> Skip next if V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " equals V" + ((m_Instruction & 0x0F0) >> 4).ToString("X"));
                            break;
                        default:
                            break;
                    }
                    break;
                case CHIP_8_INSTRUCTIONS.SET_CTE:
                    m_DebugStream.WriteLine("> Set V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " to 0x" + (m_Instruction & 0xFF).ToString("X2"));
                    break;
                case CHIP_8_INSTRUCTIONS.ADD_CTE:
                    m_DebugStream.WriteLine("> Add 0x" + (m_Instruction & 0xFF).ToString("X2") + " to V" + ((m_Instruction & 0xF00) >> 8).ToString("X"));
                    break;
                case CHIP_8_INSTRUCTIONS.MANIP_VAR:
                    switch ((MANIP_VAR_SUB_INSTRUCTIONS)(m_Instruction & 0x000F))
                    {
                        case MANIP_VAR_SUB_INSTRUCTIONS.SET_VAR:
                            m_DebugStream.WriteLine("> Set V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " to V" + ((m_Instruction & 0xF0) >> 4).ToString("X"));
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.OR_VAR:
                            m_DebugStream.WriteLine("> OR V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " with V" + ((m_Instruction & 0xF0) >> 4).ToString("X"));
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.AND_VAR:
                            m_DebugStream.WriteLine("> AND V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " with V" + ((m_Instruction & 0xF0) >> 4).ToString("X"));
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.XOR_VAR:
                            m_DebugStream.WriteLine("> XOR V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " with V" + ((m_Instruction & 0xF0) >> 4).ToString("X"));
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.ADD_VAR:
                            m_DebugStream.WriteLine("> Add V" + ((m_Instruction & 0xF0) >> 4).ToString("X") + " to V" + ((m_Instruction & 0xF00) >> 8).ToString("X"));
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.SUBTRACT_VAR:
                            m_DebugStream.WriteLine("> Subtract V" + ((m_Instruction & 0xF0) >> 4).ToString("X2") + " from V" + ((m_Instruction & 0xF00) >> 8).ToString("X"));
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.RIGHT_SHIFT:
                            m_DebugStream.WriteLine("> Right shift V" + ((m_Instruction & 0xF00) >> 8).ToString("X"));
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.VAR_SUBTRACT:
                            m_DebugStream.WriteLine("> Subtract V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " from V" + ((m_Instruction & 0xF0) >> 4).ToString("X2") + " and store in V" + ((m_Instruction & 0xF00) >> 8).ToString("X"));
                            break;
                        case MANIP_VAR_SUB_INSTRUCTIONS.LEFT_SHIFT:
                            m_DebugStream.WriteLine("> Left shift V" + ((m_Instruction & 0xF00) >> 8).ToString("X"));
                            break;
                        default:
                            break;
                    }
                    break;
                case CHIP_8_INSTRUCTIONS.NOT_EQUALS_VAR:
                    switch (m_Instruction & 0xF)
                    {
                        case 0x0:
                            m_DebugStream.WriteLine("> Skip next if V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " does not equal V" + ((m_Instruction & 0x0F0) >> 4).ToString("X2"));
                            break;
                        default:
                            break;
                    }
                    break;
                case CHIP_8_INSTRUCTIONS.SET_ADDR:
                    m_DebugStream.WriteLine("> Set I to 0x" + (m_Instruction & 0xFFF).ToString("X4"));
                    break;
                case CHIP_8_INSTRUCTIONS.GOTO_PLUS:
                    m_DebugStream.WriteLine("> Go to 0x" + (m_Instruction & 0xFFF).ToString("X4") + " + V0");
                    break;
                case CHIP_8_INSTRUCTIONS.RAND:
                    m_DebugStream.WriteLine("> Set V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " to rand(0x0,0x" + (m_Instruction & 0xFF).ToString("X2"));
                    break;
                case CHIP_8_INSTRUCTIONS.DISP:
                    m_DebugStream.WriteLine("> Disp " + (m_Instruction & 0xF).ToString("X") + " rows at (" + m_DataRegisters[(m_Instruction & 0xF00) >> 8].ToString("X2") + "," + m_DataRegisters[(m_Instruction & 0xF0) >> 4].ToString("X2") + ")");
                    break;
                case CHIP_8_INSTRUCTIONS.KEY_SKIP:
                    switch ((KEY_SKIP_SUB_INSTRUCTIONS)(m_Instruction & 0x0FF))
                    {
                        case KEY_SKIP_SUB_INSTRUCTIONS.PRESSED:
                            m_DebugStream.WriteLine("> Skip next if key " + m_DataRegisters[(m_Instruction & 0xF00) >> 8].ToString("X2") + " is pressed");
                            break;
                        case KEY_SKIP_SUB_INSTRUCTIONS.NOT_PRESSED:
                            m_DebugStream.WriteLine("> Skip next if key " + m_DataRegisters[(m_Instruction & 0xF00) >> 8].ToString("X2") + " is not pressed");
                            break;
                        default:
                            break;
                    }
                    break;
                case CHIP_8_INSTRUCTIONS.FUNC2:
                    switch ((FUNCTION_SUB_INSTRUCTIONS)(m_Instruction & 0xFF))
                    {
                        case FUNCTION_SUB_INSTRUCTIONS.SET_VAR_DELAY_TIMER:
                            m_DebugStream.WriteLine("> Set V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " to delay timer.");
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.AWAIT_KEYPRESS:
                            m_DebugStream.WriteLine("> Await key " + m_DataRegisters[(m_Instruction & 0xF00) >> 8].ToString("X2"));
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.SET_DELAY_TIMER_VAR:
                            m_DebugStream.WriteLine("> Set delay timer to V" + ((m_Instruction & 0xF00) >> 8).ToString("X"));
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.SET_SOUND_TIMER_VAR:
                            m_DebugStream.WriteLine("> Set sound timer to V" + ((m_Instruction & 0xF00) >> 8).ToString("X"));
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.ADD_ADDR_VAR:
                            m_DebugStream.WriteLine("> Add V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " to I");
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.SET_ADDR_SPRITE:
                            m_DebugStream.WriteLine("> Set I to font sprite V" + (m_Instruction & 0xF00).ToString("X"));
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.BCD_VAR:
                            m_DebugStream.WriteLine("> Set I to BCD of V" + ((m_Instruction & 0xF00) >> 8).ToString("X"));
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.REG_DUMP:
                            m_DebugStream.WriteLine("> Dump V0 to V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " to I");
                            break;
                        case FUNCTION_SUB_INSTRUCTIONS.REG_LOAD:
                            m_DebugStream.WriteLine("> Load V0 to V" + ((m_Instruction & 0xF00) >> 8).ToString("X") + " from I");
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            m_DebugStream.WriteLine("PTR: " + m_ExecutionPointer.ToString("X4"));

            for (int i = 0; i < m_DataRegisters.Length; i++)
            {
                m_DebugStream.WriteLine("V" + i.ToString("X1") + " = 0x" + m_DataRegisters[i].ToString("X2"));
            }

            m_DebugStream.WriteLine("I = 0x" + m_AdrressRegister.ToString("X4"));

            m_DebugStream.WriteLine("STACK:");
            foreach (int addr in m_Stack)
            {
                m_DebugStream.WriteLine("0x" + addr.ToString("X4"));
            }

            if (((m_Instruction & 0xF000) >> 12) == 0xD)
            {
                m_DebugStream.WriteLine("DISP:");
                for (int i = 0; i < m_Display.GetLength(1); i++)
                {
                    for (int j = 0; j < m_Display.GetLength(0); j++)
                    {
                        if (m_Display[j, i])
                            m_DebugStream.Write('0');
                        else
                            m_DebugStream.Write('.');
                    }

                    m_DebugStream.WriteLine();
                }
            }

            m_DebugStream.WriteLine( (elapsed / Stopwatch.Frequency) * 1000 + " ms");
            m_DebugStream.WriteLine("****************************************************************");
            m_DebugStream.Flush();
        }

        private void DelayTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (m_DelayTimerRegister != 0)
                m_DelayTimerRegister--;
        }

        public Processor(string romPath)
        {
            BinaryReader romReader = new BinaryReader(new FileStream(romPath, FileMode.Open));
            byte[] romData = romReader.ReadBytes((int)romReader.BaseStream.Length);
            romReader.Close();

            m_RomFile = new FileInfo(romPath);

            if (romData.Length > (CPU_CONSTANTS.MEMORY_BYTE_LEN - CPU_CONSTANTS.RESET_VECTOR))
            {
                throw new Exception("This doesn't seem to be a CHIP8 rom... ¯\\_(ツ)_/¯");
            }

            for (int i = 0; i < CPU_CONSTANTS.NUM_DATA_REGISTERS; i++)
                m_DataRegisters[i] = 0;

            for (int i = 0; i < CPU_CONSTANTS.MEMORY_BYTE_LEN; i++)
                m_Memory[i] = 0;

            ClearDisplay();

            int index = 0;
            Array.Copy(CPU_CONSTANTS.FONT_0, 0, m_Memory, index, CPU_CONSTANTS.FONT_0.Length);
            index += CPU_CONSTANTS.FONT_0.Length;
            Array.Copy(CPU_CONSTANTS.FONT_1, 0, m_Memory, index, CPU_CONSTANTS.FONT_1.Length);
            index += CPU_CONSTANTS.FONT_1.Length;
            Array.Copy(CPU_CONSTANTS.FONT_2, 0, m_Memory, index, CPU_CONSTANTS.FONT_2.Length);
            index += CPU_CONSTANTS.FONT_2.Length;
            Array.Copy(CPU_CONSTANTS.FONT_3, 0, m_Memory, index, CPU_CONSTANTS.FONT_3.Length);
            index += CPU_CONSTANTS.FONT_3.Length;
            Array.Copy(CPU_CONSTANTS.FONT_4, 0, m_Memory, index, CPU_CONSTANTS.FONT_4.Length);
            index += CPU_CONSTANTS.FONT_4.Length;
            Array.Copy(CPU_CONSTANTS.FONT_5, 0, m_Memory, index, CPU_CONSTANTS.FONT_5.Length);
            index += CPU_CONSTANTS.FONT_5.Length;
            Array.Copy(CPU_CONSTANTS.FONT_6, 0, m_Memory, index, CPU_CONSTANTS.FONT_6.Length);
            index += CPU_CONSTANTS.FONT_6.Length;
            Array.Copy(CPU_CONSTANTS.FONT_7, 0, m_Memory, index, CPU_CONSTANTS.FONT_7.Length);
            index += CPU_CONSTANTS.FONT_7.Length;
            Array.Copy(CPU_CONSTANTS.FONT_8, 0, m_Memory, index, CPU_CONSTANTS.FONT_8.Length);
            index += CPU_CONSTANTS.FONT_8.Length;
            Array.Copy(CPU_CONSTANTS.FONT_9, 0, m_Memory, index, CPU_CONSTANTS.FONT_9.Length);
            index += CPU_CONSTANTS.FONT_9.Length;
            Array.Copy(CPU_CONSTANTS.FONT_A, 0, m_Memory, index, CPU_CONSTANTS.FONT_A.Length);
            index += CPU_CONSTANTS.FONT_A.Length;
            Array.Copy(CPU_CONSTANTS.FONT_B, 0, m_Memory, index, CPU_CONSTANTS.FONT_B.Length);
            index += CPU_CONSTANTS.FONT_B.Length;
            Array.Copy(CPU_CONSTANTS.FONT_C, 0, m_Memory, index, CPU_CONSTANTS.FONT_C.Length);
            index += CPU_CONSTANTS.FONT_C.Length;
            Array.Copy(CPU_CONSTANTS.FONT_D, 0, m_Memory, index, CPU_CONSTANTS.FONT_D.Length);
            index += CPU_CONSTANTS.FONT_D.Length;
            Array.Copy(CPU_CONSTANTS.FONT_E, 0, m_Memory, index, CPU_CONSTANTS.FONT_E.Length);
            index += CPU_CONSTANTS.FONT_E.Length;
            Array.Copy(CPU_CONSTANTS.FONT_F, 0, m_Memory, index, CPU_CONSTANTS.FONT_F.Length);
            index += CPU_CONSTANTS.FONT_F.Length;

            Array.Copy(romData, 0, m_Memory, CPU_CONSTANTS.RESET_VECTOR, romData.Length);

            m_DelayTimer.Interval = (1.0 / CPU_CONSTANTS.TIMER_FREQUENCY) * 1000;
            m_DelayTimer.Elapsed += DelayTimerElapsed;
            m_DelayTimer.Start();

            m_KeyPressResetTimer.Interval = (1.0 / (CPU_CONSTANTS.TIMER_FREQUENCY/10)) * 1000;
            m_KeyPressResetTimer.Elapsed += KeyPressResetTimerElapsed;
        }

        private void KeyPressResetTimerElapsed(object sender, ElapsedEventArgs e)
        {
            m_KeyPressResetTimer.Stop();
            m_KeyPressed = Chip8Keys.UNKNOWN;
        }

        private void ClearDisplay()
        {
            for (int i = 0; i < m_Display.GetLength(0); i++)
                for (int j = 0; j < m_Display.GetLength(1); j++)
                    m_Display[i, j] = false;

            DisplayEvent?.Invoke(this, new DisplayEventArgs(m_Display));
        }

        private void WaitCycle(double seconds)
        {
            while (m_ProcessorCycleStopWatch.ElapsedTicks < (seconds * Stopwatch.Frequency))
            {

            };
        }

        private void Block(double seconds)
        {
            m_BlockStopWatch.Restart();

            while (m_BlockStopWatch.ElapsedTicks < (seconds * Stopwatch.Frequency))
            {

            };
        }
    }
}
