//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    [DisplayName("ARM emulator")]
    [HardwareModel(typeof(Emulation.ArmProcessor.ARMv4Simulator), HardwareModelAttribute.Kind.Engine)]
    public sealed class ArmEmulator : EngineCategory
    {
        public override object Instantiate(InstructionSetARM iset)
        {
            if(iset is InstructionSetARMv4)
            {
                return new Emulation.ArmProcessor.ARMv4Simulator( (InstructionSetARMv4)iset );
            }

            throw new NotSupportedException( ); 
        }
    }
}
