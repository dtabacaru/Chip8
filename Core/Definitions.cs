namespace Chip8.Core
{
    public static class CPU_CONSTANTS
    {
        public const int INSTRUCTION_BYTE_LEN = 0x2;
        public const int MEMORY_BYTE_LEN      = 0x1000;
        public const int NUM_DATA_REGISTERS   = 0x10;
        public const int RESET_VECTOR         = 0x200;
        public const int DISPLAY_WIDTH        = 0x40;
        public const int DISPLAY_HEIGHT       = 0x20;
        public const int TIMER_FREQUENCY      = 0x3C;
        public const int SPRITE_LEN           = 0x08;
        public static readonly byte[] FONT_0  = { 0xF0, 0x90, 0x90, 0x90, 0xF0 };
        public static readonly byte[] FONT_1  = { 0x20, 0x60, 0x20, 0x20, 0x70 };
        public static readonly byte[] FONT_2  = { 0xF0, 0x10, 0xF0, 0x80, 0xF0 };
        public static readonly byte[] FONT_3  = { 0xF0, 0x10, 0xF0, 0x10, 0xF0 };
        public static readonly byte[] FONT_4  = { 0x90, 0x90, 0xF0, 0x10, 0x10 };
        public static readonly byte[] FONT_5  = { 0xF0, 0x80, 0xF0, 0x10, 0xF0 };
        public static readonly byte[] FONT_6  = { 0xF0, 0x80, 0xF0, 0x90, 0xF0 };
        public static readonly byte[] FONT_7  = { 0xF0, 0x10, 0x20, 0x40, 0x40 };
        public static readonly byte[] FONT_8  = { 0xF0, 0x90, 0xF0, 0x90, 0xF0 };
        public static readonly byte[] FONT_9  = { 0xF0, 0x90, 0xF0, 0x10, 0xF0 };
        public static readonly byte[] FONT_A  = { 0xF0, 0x90, 0xF0, 0x90, 0x90 };
        public static readonly byte[] FONT_B  = { 0xE0, 0x90, 0xE0, 0x90, 0xE0 };
        public static readonly byte[] FONT_C  = { 0xF0, 0x80, 0x80, 0x80, 0xF0 };
        public static readonly byte[] FONT_D  = { 0xE0, 0x90, 0x90, 0x90, 0xE0 };
        public static readonly byte[] FONT_E  = { 0xF0, 0x80, 0xF0, 0x80, 0xF0 };
        public static readonly byte[] FONT_F  = { 0xF0, 0x80, 0xF0, 0x80, 0x80 };
    }

    public enum CHIP_8_INSTRUCTIONS
    {
        FUNC1          = 0x00,
        GOTO           = 0x01,
        CALL           = 0x02,
        EQUALS_CTE     = 0x03,
        NOT_EQUALS_CTE = 0x04,
        EQUALS_VAR     = 0x05,
        SET_CTE        = 0x06,
        ADD_CTE        = 0x07,
        MANIP_VAR      = 0x08,
        NOT_EQUALS_VAR = 0x09,
        SET_ADDR       = 0x0A,
        GOTO_PLUS      = 0x0B,
        RAND           = 0x0C,
        DISP           = 0x0D,
        KEY_SKIP       = 0x0E,
        FUNC2          = 0x0F
    };

    public enum MISC_SUB_INSTRUCTIONS
    {
        CLS = 0x0E0,
        RET = 0x0EE
    }

    public enum MANIP_VAR_SUB_INSTRUCTIONS
    {
        SET_VAR      = 0x00,
        OR_VAR       = 0x01,
        AND_VAR      = 0x02,
        XOR_VAR      = 0x03,
        ADD_VAR      = 0x04,
        SUBTRACT_VAR = 0x05,
        RIGHT_SHIFT  = 0x06,
        VAR_SUBTRACT = 0x07,
        LEFT_SHIFT   = 0x0E,
    }

    public enum KEY_SKIP_SUB_INSTRUCTIONS
    {
        PRESSED     = 0x9E,
        NOT_PRESSED = 0xA1
    }

    public enum FUNCTION_SUB_INSTRUCTIONS
    {
        SET_VAR_DELAY_TIMER = 0x07,
        AWAIT_KEYPRESS      = 0x0A,
        SET_DELAY_TIMER_VAR = 0x15,
        SET_SOUND_TIMER_VAR = 0x18,
        ADD_ADDR_VAR        = 0x1E,
        SET_ADDR_SPRITE     = 0x29,
        BCD_VAR             = 0x33,
        REG_DUMP            = 0x55,
        REG_LOAD            = 0x65
    }

    enum Chip8Keys
    {
        KEY_0,
        KEY_1,
        KEY_2,
        KEY_3,
        KEY_4,
        KEY_5,
        KEY_6,
        KEY_7,
        KEY_8,
        KEY_9,
        KEY_A,
        KEY_B,
        KEY_C,
        KEY_D,
        KEY_E,
        KEY_F,
        UNKNOWN
    }
}
