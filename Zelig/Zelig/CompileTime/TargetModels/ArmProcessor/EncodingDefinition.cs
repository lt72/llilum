//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.TargetModel.ArmProcessor
{
    abstract public class EncodingDefinition
    {
        public const int c_PC_offset       = 8;

        //--//

        /////////////////////////////////////////

        //--//

        /////////////////////////////////////////
        public const uint c_register_r0  =  0; //
        public const uint c_register_r1  =  1; //
        public const uint c_register_r2  =  2; //
        public const uint c_register_r3  =  3; //
        public const uint c_register_r4  =  4; //
        public const uint c_register_r5  =  5; //
        public const uint c_register_r6  =  6; //
        public const uint c_register_r7  =  7; //
        public const uint c_register_r8  =  8; //
        public const uint c_register_r9  =  9; //
        public const uint c_register_r10 = 10; //
        public const uint c_register_r11 = 11; //
        public const uint c_register_r12 = 12; //
        public const uint c_register_r13 = 13; //
        public const uint c_register_r14 = 14; //
        public const uint c_register_r15 = 15; //
        public const uint c_register_sp  = 13; //
        public const uint c_register_lr  = 14; //
        public const uint c_register_pc  = 15; //
        /////////////////////////////////////////

        //////////////////////////////////////////
        public const uint c_register_lst_r0  = 1U << (int)c_register_r0 ;
        public const uint c_register_lst_r1  = 1U << (int)c_register_r1 ;
        public const uint c_register_lst_r2  = 1U << (int)c_register_r2 ;
        public const uint c_register_lst_r3  = 1U << (int)c_register_r3 ;
        public const uint c_register_lst_r4  = 1U << (int)c_register_r4 ;
        public const uint c_register_lst_r5  = 1U << (int)c_register_r5 ;
        public const uint c_register_lst_r6  = 1U << (int)c_register_r6 ;
        public const uint c_register_lst_r7  = 1U << (int)c_register_r7 ;
        public const uint c_register_lst_r8  = 1U << (int)c_register_r8 ;
        public const uint c_register_lst_r9  = 1U << (int)c_register_r9 ;
        public const uint c_register_lst_r10 = 1U << (int)c_register_r10;
        public const uint c_register_lst_r11 = 1U << (int)c_register_r11;
        public const uint c_register_lst_r12 = 1U << (int)c_register_r12;
        public const uint c_register_lst_r13 = 1U << (int)c_register_r13;
        public const uint c_register_lst_r14 = 1U << (int)c_register_r14;
        public const uint c_register_lst_r15 = 1U << (int)c_register_r15;
        public const uint c_register_lst_sp  = 1U << (int)c_register_sp ;
        public const uint c_register_lst_lr  = 1U << (int)c_register_lr ;
        public const uint c_register_lst_pc  = 1U << (int)c_register_pc ;
        ///////////////////////////////////////////

        //--//

        ///////////////////////////////////////////
        public const uint c_operation_AND = 0x0; // operand1 AND operand2
        public const uint c_operation_EOR = 0x1; // operand1 EOR operand2
        public const uint c_operation_SUB = 0x2; // operand1 - operand2
        public const uint c_operation_RSB = 0x3; // operand2 - operand1
        public const uint c_operation_ADD = 0x4; // operand1 + operand2
        public const uint c_operation_ADC = 0x5; // operand1 + operand2 + carry
        public const uint c_operation_SBC = 0x6; // operand1 - operand2 + carry - 1
        public const uint c_operation_RSC = 0x7; // operand2 - operand1 + carry - 1
        public const uint c_operation_TST = 0x8; // as AND, but result is not written
        public const uint c_operation_TEQ = 0x9; // as EOR, but result is not written
        public const uint c_operation_CMP = 0xA; // as SUB, but result is not written
        public const uint c_operation_CMN = 0xB; // as ADD, but result is not written
        public const uint c_operation_ORR = 0xC; // operand1 OR operand2
        public const uint c_operation_MOV = 0xD; // operand2(operand1 is ignored)
        public const uint c_operation_BIC = 0xE; // operand1 AND NOT operand2(Bit clear)
        public const uint c_operation_MVN = 0xF; // NOT operand2(operand1 is ignored)
        ///////////////////////////////////////////

        //--//

        ///////////////////////////////////////
        public const uint c_shift_LSL = 0x0; // logical shift left
        public const uint c_shift_LSR = 0x1; // logical shift right
        public const uint c_shift_ASR = 0x2; // arithmetic shift right
        public const uint c_shift_ROR = 0x3; // rotate right
        public const uint c_shift_RRX = 0x4; // rotate right with extend
        ///////////////////////////////////////

        //--//

        //////////////////////////////////////////////
        public const uint c_halfwordkind_SWP = 0x0; //
        public const uint c_halfwordkind_U2  = 0x1; //
        public const uint c_halfwordkind_I1  = 0x2; //
        public const uint c_halfwordkind_I2  = 0x3; //
        //////////////////////////////////////////////

        //--//

        /////////////////////////////////////////
        public const uint c_cond_EQ     = 0x0; //  Z set                                equal
        public const uint c_cond_NE     = 0x1; //  Z clear                          not equal
        public const uint c_cond_CS     = 0x2; //  C set                   unsigned     higher or same
        public const uint c_cond_CC     = 0x3; //  C clear                 unsigned     lower
        public const uint c_cond_MI     = 0x4; //  N set                                negative
        public const uint c_cond_PL     = 0x5; //  N clear                              positive or zero
        public const uint c_cond_VS     = 0x6; //  V set                                overflow
        public const uint c_cond_VC     = 0x7; //  V clear                           no overflow
        public const uint c_cond_HI     = 0x8; //  C set and Z clear       unsigned     higher
        public const uint c_cond_LS     = 0x9; //  C clear or Z set        unsigned     lower or same
        public const uint c_cond_GE     = 0xA; //  N equals V                           greater or equal
        public const uint c_cond_LT     = 0xB; //  N not equal to V                     less than
        public const uint c_cond_GT     = 0xC; //  Z clear AND (N equals V)             greater than
        public const uint c_cond_LE     = 0xD; //  Z set OR (N not equal to V)          less than or equal
        public const uint c_cond_AL     = 0xE; //  (ignored) always
        public const uint c_cond_UNUSED = 0xF; //
        /////////////////////////////////////////
        public const uint c_cond_NUM    = 0x10;
        
        public enum Format
        {
            MRS                    ,
            MSR_1                  ,
            MSR_2                  ,
            DataProcessing_1       ,
            DataProcessing_2       ,
            DataProcessing_3       ,
            Multiply               ,
            MultiplyLong           ,
            SingleDataSwap         ,
            BranchAndExchange      ,
            HalfwordDataTransfer_1 ,
            HalfwordDataTransfer_2 ,
            SingleDataTransfer_1   ,
            SingleDataTransfer_2   ,
            SingleDataTransfer_3   ,
            Undefined              ,
            BlockDataTransfer      ,
            Branch                 ,
            CoprocDataTransfer     ,
            CoprocDataOperation    ,
            CoprocRegisterTransfer ,
            SoftwareInterrupt      ,

            FIRST_FORMAT   = MRS,
            LAST_FORMAT    = SoftwareInterrupt,
            NUM_OF_FORMATS = (SoftwareInterrupt - MRS + 1)
        };

        //--//

        //
        // State
        //

        private readonly InstructionSetVersion m_version;

        public EncodingDefinition( InstructionSetVersion isv )
        {
            m_version = isv;
        }

        public override bool Equals( object obj )
        {
            var match = obj as EncodingDefinition;

            if(match != null)
            {
                return m_version == match.Version;
            }

            return false;
        }

        public override int GetHashCode( )
        {
            return base.GetHashCode( );
        }

        public InstructionSetVersion Version
        {
            get
            {
                return m_version;
            }
        }

        //--//

        [System.Diagnostics.Conditional( "DEBUG" )]
        static void OPCODE_VERIFY_INSERT_FIELD( int val    ,
                                                int bitLen )
        {
            int valHigh = val >> bitLen;

            if(valHigh !=  0 &&
               valHigh != -1  )
            {
                throw new System.ArgumentException( string.Format( "Found value outside bounds of field [{0}:{1}]: {2}", -(1 << (bitLen-1)), (1 << (bitLen-1)), val ) );
            }
        }

        [System.Diagnostics.Conditional( "DEBUG" )]
        static void OPCODE_VERIFY_INSERT_FIELD( uint val    ,
                                                int  bitLen )
        {
            uint valHigh = val >> bitLen;

            if(valHigh != 0)
            {
                throw new System.ArgumentException( string.Format( "Found value outside bounds of field [{0}:{1}]: {2}", 0, (1 << bitLen) - 1, val ) );
            }
        }

        [System.Diagnostics.Conditional( "DEBUG" )]
        static void OPCODE_VERIFY_INSERT_FIELD( uint val    ,
                                                int  valPos ,
                                                int  valLen ,
                                                int  bitLen )
        {
            uint valHigh = val >> valLen;

            if(valHigh != 0)
            {
                throw new System.ArgumentException( string.Format( "Found value outside bounds of field [{0}:{1}]: {2}", 0, (1 << bitLen) - 1, val ) );
            }
        }

        //--//

        static public uint OPCODE_DECODE_MASK( int len )
        {
            return (1U << len) - 1U;
        }

        static public uint OPCODE_DECODE_INSERTFIELD( int val    ,
                                                      int bitPos ,
                                                      int bitLen )
        {
            OPCODE_VERIFY_INSERT_FIELD( val, bitLen );

            return ((uint)val & OPCODE_DECODE_MASK( bitLen )) << bitPos;
        }

        static public uint OPCODE_DECODE_INSERTFIELD( uint val    ,
                                                      int  bitPos ,
                                                      int  bitLen )
        {
            OPCODE_VERIFY_INSERT_FIELD( val, bitLen );

            return (val & OPCODE_DECODE_MASK( bitLen )) << bitPos;
        }

        static public uint OPCODE_DECODE_INSERTFIELD( uint val    ,
                                                      int  valPos ,
                                                      int  valLen ,
                                                      int  bitPos ,
                                                      int  bitLen )
        {
            OPCODE_VERIFY_INSERT_FIELD( val, valPos, valLen, bitLen );

            return ((val >> valPos) & EncodingDefinition_ARMv4.OPCODE_DECODE_MASK( bitLen )) << bitPos;
        }

        static public uint OPCODE_DECODE_EXTRACTFIELD( uint op     ,
                                                       int  bitPos ,
                                                       int  bitLen )
        {
            return (op >> bitPos) & OPCODE_DECODE_MASK( bitLen );
        }

        static public uint OPCODE_DECODE_EXTRACTFIELD( uint op     ,
                                                       int  valPos ,
                                                       int  valLen ,
                                                       int  bitPos ,
                                                       int  bitLen )
        {
            return ((op >> bitPos) & EncodingDefinition_ARMv4.OPCODE_DECODE_MASK( bitLen )) << valPos;
        }

        //--//

        static public uint OPCODE_DECODE_SETFLAG( bool val    ,
                                                  int  bitPos )
        {
            return val ? (1U << bitPos) : 0U;
        }

        static public bool OPCODE_DECODE_CHECKFLAG( uint op     ,
                                                    int  bitPos )
        {
            return (op & (1U << bitPos)) != 0;
        }

        //--//

        abstract public uint get_ConditionCodes     ( uint op  );
        abstract public uint set_ConditionCodes     ( uint val );

        abstract public bool get_ShouldSetConditions( uint op  );
        abstract public uint set_ShouldSetConditions( bool val );

        //--//

        abstract public uint get_Register1( uint op  );
        abstract public uint set_Register1( uint val );

        abstract public uint get_Register2( uint op  );
        abstract public uint set_Register2( uint val );

        abstract public uint get_Register3( uint op  );
        abstract public uint set_Register3( uint val );

        abstract public uint get_Register4( uint op  );
        abstract public uint set_Register4( uint val );

        //--//

        abstract public bool get_Multiply_IsAccumulate( uint op  );
        abstract public uint set_Multiply_IsAccumulate( bool val );

        abstract public bool get_Multiply_IsSigned    ( uint op  );
        abstract public uint set_Multiply_IsSigned    ( bool val );

        //--//

        abstract public bool get_StatusRegister_IsSPSR( uint op  );
        abstract public uint set_StatusRegister_IsSPSR( bool val );

        abstract public uint get_StatusRegister_Fields( uint op  );
        abstract public uint set_StatusRegister_Fields( uint val );

        //--//

        abstract public uint get_Shift_Type     ( uint op  );
        abstract public uint set_Shift_Type     ( uint val );

        abstract public uint get_Shift_Immediate( uint op  );
        abstract public uint set_Shift_Immediate( uint val );

        abstract public uint get_Shift_Register ( uint op  );
        abstract public uint set_Shift_Register ( uint val );

        //--//

        abstract public uint get_DataProcessing_Operation( uint op  );
        abstract public uint set_DataProcessing_Operation( uint val );

        abstract public uint get_DataProcessing_ImmediateSeed    ( uint op  );
        abstract public uint set_DataProcessing_ImmediateSeed    ( uint val );

        abstract public uint get_DataProcessing_ImmediateRotation( uint op  );
        abstract public uint set_DataProcessing_ImmediateRotation( uint val );

        abstract public uint get_DataProcessing_ImmediateValue( uint op );

        abstract public uint get_DataProcessing_ImmediateValue( uint imm, uint rot );

        abstract public bool check_DataProcessing_ImmediateValue( uint val, out uint immRes, out uint rotRes );

        //--//

        abstract public bool get_DataTransfer_IsLoad         ( uint op  );
        abstract public uint set_DataTransfer_IsLoad         ( bool val );

        abstract public bool get_DataTransfer_ShouldWriteBack( uint op  );
        abstract public uint set_DataTransfer_ShouldWriteBack( bool val );

        abstract public bool get_DataTransfer_IsByteTransfer ( uint op  );
        abstract public uint set_DataTransfer_IsByteTransfer ( bool val );

        abstract public bool get_DataTransfer_IsUp           ( uint op  );
        abstract public uint set_DataTransfer_IsUp           ( bool val );

        abstract public bool get_DataTransfer_IsPreIndexing  ( uint op  );
        abstract public uint set_DataTransfer_IsPreIndexing  ( bool val );

        abstract public uint get_DataTransfer_Offset         ( uint op  );
        abstract public uint set_DataTransfer_Offset         ( uint val );

        //--//

        abstract public uint get_HalfWordDataTransfer_Kind  ( uint op  );
        abstract public uint set_HalfWordDataTransfer_Kind  ( uint val );
        abstract public uint get_HalfWordDataTransfer_Offset( uint op  );
        abstract public uint set_HalfWordDataTransfer_Offset( uint val );

        //--//

        abstract public bool get_BlockDataTransfer_LoadPSR     ( uint op  );
        abstract public uint set_BlockDataTransfer_LoadPSR     ( bool val );

        abstract public uint get_BlockDataTransfer_RegisterList( uint op  );
        abstract public uint set_BlockDataTransfer_RegisterList( uint val );

        //--//

        abstract public bool get_Branch_IsLink( uint op  );
        abstract public uint set_Branch_IsLink( bool val );

        abstract public int  get_Branch_Offset( uint op  );
        abstract public uint set_Branch_Offset( int  val );

        //--//

        abstract public uint get_Coproc_CpNum( uint op  );
        abstract public uint set_Coproc_CpNum( uint val );
                                                        
        //--//

        abstract public bool get_CoprocRegisterTransfer_IsMRC( uint op  );
        abstract public uint set_CoprocRegisterTransfer_IsMRC( bool val );

        abstract public uint get_CoprocRegisterTransfer_Op1  ( uint op  );
        abstract public uint set_CoprocRegisterTransfer_Op1  ( uint val );

        abstract public uint get_CoprocRegisterTransfer_Op2  ( uint op  );
        abstract public uint set_CoprocRegisterTransfer_Op2  ( uint val );

        abstract public uint get_CoprocRegisterTransfer_CRn  ( uint op  );
        abstract public uint set_CoprocRegisterTransfer_CRn  ( uint val );

        abstract public uint get_CoprocRegisterTransfer_CRm  ( uint op  );
        abstract public uint set_CoprocRegisterTransfer_CRm  ( uint val );

        abstract public uint get_CoprocRegisterTransfer_Rd   ( uint op  );
        abstract public uint set_CoprocRegisterTransfer_Rd   ( uint val );

        //--//

        abstract public bool get_CoprocDataTransfer_IsLoad         ( uint op  );
        abstract public uint set_CoprocDataTransfer_IsLoad         ( bool val );

        abstract public bool get_CoprocDataTransfer_ShouldWriteBack( uint op  );
        abstract public uint set_CoprocDataTransfer_ShouldWriteBack( bool val );
                                                             
        abstract public bool get_CoprocDataTransfer_IsWide         ( uint op  );
        abstract public uint set_CoprocDataTransfer_IsWide         ( bool val );

        abstract public bool get_CoprocDataTransfer_IsUp           ( uint op  );
        abstract public uint set_CoprocDataTransfer_IsUp           ( bool val );

        abstract public bool get_CoprocDataTransfer_IsPreIndexing  ( uint op  );
        abstract public uint set_CoprocDataTransfer_IsPreIndexing  ( bool val );
                                                        
        abstract public uint get_CoprocDataTransfer_Rn             ( uint op  );
        abstract public uint set_CoprocDataTransfer_Rn             ( uint val );
                                                        
        abstract public uint get_CoprocDataTransfer_CRd            ( uint op  );
        abstract public uint set_CoprocDataTransfer_CRd            ( uint val );
                                                        
        abstract public uint get_CoprocDataTransfer_Offset         ( uint op  );
        abstract public uint set_CoprocDataTransfer_Offset         ( uint val );

        //--//

        abstract public uint get_CoprocDataOperation_Op1  ( uint op  );
        abstract public uint set_CoprocDataOperation_Op1  ( uint val );

        abstract public uint get_CoprocDataOperation_Op2  ( uint op  );
        abstract public uint set_CoprocDataOperation_Op2  ( uint val );

        abstract public uint get_CoprocDataOperation_CRn  ( uint op  );
        abstract public uint set_CoprocDataOperation_CRn  ( uint val );

        abstract public uint get_CoprocDataOperation_CRm  ( uint op  );
        abstract public uint set_CoprocDataOperation_CRm  ( uint val );

        abstract public uint get_CoprocDataOperation_CRd  ( uint op  );
        abstract public uint set_CoprocDataOperation_CRd  ( uint val );

        //--//

        abstract public uint get_SoftwareInterrupt_Immediate( uint op  );
        abstract public uint set_SoftwareInterrupt_Immediate( uint val );

        //--//

        abstract public uint get_Breakpoint_Immediate( uint op  );
        abstract public uint set_Breakpoint_Immediate( uint val );
    }
}
