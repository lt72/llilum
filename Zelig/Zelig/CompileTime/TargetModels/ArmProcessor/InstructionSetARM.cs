//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.TargetModel.ArmProcessor
{

    using EncDef = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    
    public abstract class InstructionSetARM
    {
        public abstract class Opcode
        {
            //
            // State
            //

            public uint ConditionCodes;

            //--//

            protected void Prepare( uint ConditionCodes )
            {
                this.ConditionCodes = ConditionCodes;
            }

            //--//

            public virtual void Decode( uint op )
            {
                this.ConditionCodes = s_Encoding.get_ConditionCodes( op );
            }

            public virtual uint Encode( )
            {
                return s_Encoding.set_ConditionCodes( this.ConditionCodes );
            }

            public abstract void Print(     InstructionSetARM         owner,
                                            System.Text.StringBuilder str,
                                            uint                      opcodeAddress,
                                        ref uint                      target,
                                        ref bool                      targetIsCode );

            //--//

            protected void PrintMnemonic( System.Text.StringBuilder str,
                                                 string format,
                                          params object[ ] args )
            {
                int start = str.Length;

                str.AppendFormat( format, args );

                int len = str.Length - start;

                if(len < 9)
                {
                    str.Append( new string( ' ', 9 - len ) );
                }
            }

            //--//

            protected string DumpCondition( )
            {
                switch(this.ConditionCodes)
                {
                    case EncDef.c_cond_EQ: return "EQ";
                    case EncDef.c_cond_NE: return "NE";
                    case EncDef.c_cond_CS: return "CS";
                    case EncDef.c_cond_CC: return "CC";
                    case EncDef.c_cond_MI: return "MI";
                    case EncDef.c_cond_PL: return "PL";
                    case EncDef.c_cond_VS: return "VS";
                    case EncDef.c_cond_VC: return "VC";
                    case EncDef.c_cond_HI: return "HI";
                    case EncDef.c_cond_LS: return "LS";
                    case EncDef.c_cond_GE: return "GE";
                    case EncDef.c_cond_LT: return "LT";
                    case EncDef.c_cond_GT: return "GT";
                    case EncDef.c_cond_LE: return "LE";
                    case EncDef.c_cond_AL: return "";
                }

                return "??";
            }

            //--//

            public static string DumpRegister( uint reg )
            {
                switch(reg)
                {
                    case EncDef.c_register_r0: return "r0";
                    case EncDef.c_register_r1: return "r1";
                    case EncDef.c_register_r2: return "r2";
                    case EncDef.c_register_r3: return "r3";
                    case EncDef.c_register_r4: return "r4";
                    case EncDef.c_register_r5: return "r5";
                    case EncDef.c_register_r6: return "r6";
                    case EncDef.c_register_r7: return "r7";
                    case EncDef.c_register_r8: return "r8";
                    case EncDef.c_register_r9: return "r9";
                    case EncDef.c_register_r10: return "r10";
                    case EncDef.c_register_r11: return "r11";
                    case EncDef.c_register_r12: return "r12";
                    case EncDef.c_register_sp: return "sp";
                    case EncDef.c_register_lr: return "lr";
                    case EncDef.c_register_pc: return "pc";
                }

                return "??";
            }

            //--//

            static protected string DumpShiftType( uint stype )
            {
                switch(stype)
                {
                    case EncDef.c_shift_LSL: return "LSL";
                    case EncDef.c_shift_LSR: return "LSR";
                    case EncDef.c_shift_ASR: return "ASR";
                    case EncDef.c_shift_ROR: return "ROR";
                    case EncDef.c_shift_RRX: return "RRX";
                }

                return "???";
            }

            static protected string DumpHalfWordKind( uint kind )
            {
                switch(kind)
                {
                    case EncDef.c_halfwordkind_SWP: return "SWP";
                    case EncDef.c_halfwordkind_U2 : return "H";
                    case EncDef.c_halfwordkind_I1 : return "SB";
                    case EncDef.c_halfwordkind_I2 : return "SH";
                }

                return "??";
            }
        }

        //--//

        protected static EncodingDefinition s_Encoding = CurrentInstructionSetEncoding.GetEncoding();
        
        //
        // State
        //
        private readonly InstructionSetVersion m_version;

        //
        // Constructor Methods
        //

        protected InstructionSetARM( InstructionSetVersion version )
        {
            m_version = version;
        }

        //
        // Helper methods
        // 

        public abstract Opcode PrepareForMRS                    { get; }
        public abstract Opcode PrepareForMSR_1                  { get; }
        public abstract Opcode PrepareForMSR_2                  { get; }
        public abstract Opcode PrepareForDataProcessing_1       { get; }

        public abstract Opcode PrepareForDataProcessing_2       { get; }

        public abstract Opcode PrepareForDataProcessing_3       { get; }
        public abstract Opcode PrepareForMultiply               { get; }
        public abstract Opcode PrepareForMultiplyLong           { get; }
        public abstract Opcode PrepareForSingleDataSwap         { get; }
        public abstract Opcode PrepareForBranchAndExchange      { get; }
        public abstract Opcode PrepareForHalfwordDataTransfer_1 { get; }
        public abstract Opcode PrepareForHalfwordDataTransfer_2 { get; }
        public abstract Opcode PrepareForSingleDataTransfer_1   { get; }
        public abstract Opcode PrepareForSingleDataTransfer_2   { get; }
        public abstract Opcode PrepareForSingleDataTransfer_3   { get; }
        public abstract Opcode PrepareForBlockDataTransfer      { get; }
        public abstract Opcode PrepareForBranch                 { get; }
        public abstract Opcode PrepareForCoprocDataTransfer     { get; }
        public abstract Opcode PrepareForCoprocDataOperation    { get; }
        public abstract Opcode PrepareForCoprocRegisterTransfer { get; }
        public abstract Opcode PrepareForSoftwareInterrupt      { get; }
        public abstract Opcode PrepareForBreakpoint             { get; }

        public override bool Equals(object obj)
        {
            var match = obj as InstructionSetARM;

            if(match != null)
            {
                return m_version == match.Version;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //
        // Access methods
        //

        public InstructionSetVersion Version
        {
            get
            {
                return m_version;
            }
        }

        //--//

        public abstract Opcode Decode( uint op );

        public abstract string DecodeAndPrint( uint address, uint op, out uint target, out bool targetIsCode );
    }
}
