//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.TargetModel.ArmProcessor
{

    using EncDef = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARMv7M__32Bits;
    
    public class InstructionSetARMv7M : InstructionSetARMv4
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        public InstructionSetARMv7M( InstructionSetVersion version ) : base( version )
        {
        }

        //--//
        public override Opcode Decode( uint op )
        {
            Opcode opcode = null;

            if     ((op & EncDef.opmask_Breakpoint            ) == EncDef.op_Breakpoint            ) opcode = m_Breakpoint            ;
            else if((op & EncDef.opmask_MRS                   ) == EncDef.op_MRS                   ) opcode = m_MRS                   ;
            else if((op & EncDef.opmask_MSR_1                 ) == EncDef.op_MSR_1                 ) opcode = m_MSR_1                 ;
            else if((op & EncDef.opmask_MSR_2                 ) == EncDef.op_MSR_2                 ) opcode = m_MSR_2                 ;
            else if((op & EncDef.opmask_DataProcessing_1      ) == EncDef.op_DataProcessing_1      ) opcode = m_DataProcessing_1      ;
            else if((op & EncDef.opmask_DataProcessing_2      ) == EncDef.op_DataProcessing_2      ) opcode = m_DataProcessing_2      ;
            else if((op & EncDef.opmask_DataProcessing_3      ) == EncDef.op_DataProcessing_3      ) opcode = m_DataProcessing_3      ;
            else if((op & EncDef.opmask_Multiply              ) == EncDef.op_Multiply              ) opcode = m_Multiply              ;
            else if((op & EncDef.opmask_MultiplyLong          ) == EncDef.op_MultiplyLong          ) opcode = m_MultiplyLong          ;
            else if((op & EncDef.opmask_SingleDataSwap        ) == EncDef.op_SingleDataSwap        ) opcode = m_SingleDataSwap        ;
            else if((op & EncDef.opmask_BranchAndExchange     ) == EncDef.op_BranchAndExchange     ) opcode = m_BranchAndExchange     ;
            else if((op & EncDef.opmask_HalfwordDataTransfer_1) == EncDef.op_HalfwordDataTransfer_1) opcode = m_HalfwordDataTransfer_1;
            else if((op & EncDef.opmask_HalfwordDataTransfer_2) == EncDef.op_HalfwordDataTransfer_2) opcode = m_HalfwordDataTransfer_2;
            else if((op & EncDef.opmask_SingleDataTransfer_1  ) == EncDef.op_SingleDataTransfer_1  ) opcode = m_SingleDataTransfer_1  ;
            else if((op & EncDef.opmask_SingleDataTransfer_2  ) == EncDef.op_SingleDataTransfer_2  ) opcode = m_SingleDataTransfer_2  ;
            else if((op & EncDef.opmask_SingleDataTransfer_3  ) == EncDef.op_SingleDataTransfer_3  ) opcode = m_SingleDataTransfer_3  ;
            else if((op & EncDef.opmask_Undefined             ) == EncDef.op_Undefined             ) opcode = null                    ;
            else if((op & EncDef.opmask_BlockDataTransfer     ) == EncDef.op_BlockDataTransfer     ) opcode = m_BlockDataTransfer     ;
            else if((op & EncDef.opmask_Branch                ) == EncDef.op_Branch                ) opcode = m_Branch                ;
            else if((op & EncDef.opmask_CoprocDataTransfer    ) == EncDef.op_CoprocDataTransfer    ) opcode = m_CoprocDataTransfer    ;
            else if((op & EncDef.opmask_CoprocDataOperation   ) == EncDef.op_CoprocDataOperation   ) opcode = m_CoprocDataOperation   ;
            else if((op & EncDef.opmask_CoprocRegisterTransfer) == EncDef.op_CoprocRegisterTransfer) opcode = m_CoprocRegisterTransfer;
            else if((op & EncDef.opmask_SoftwareInterrupt     ) == EncDef.op_SoftwareInterrupt     ) opcode = m_SoftwareInterrupt     ;

            if(opcode != null)
            {
                opcode.Decode( op );
            }

            return opcode;
        }
        
    }
}
