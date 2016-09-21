﻿//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures
{
    using Microsoft.Zelig.Runtime.TypeSystem;
    using Microsoft.Zelig.TargetModel.ArmProcessor;
    using System;
    using System.Collections.Generic;
    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;

    public sealed class LlvmForArmV7M_VFP : ArmPlatform
    {
        private const Capabilities c_ProcessorCapabilities = Capabilities.ARMv7M | Capabilities.VFPv2;

        //
        // State
        //

        //
        // Constructor Methods
        //

        public LlvmForArmV7M_VFP( ZeligIR.TypeSystemForCodeTransformation typeSystem, MemoryMapCategory memoryMap )
            : base( typeSystem, memoryMap, c_ProcessorCapabilities )
        {
        }

        public override void RegisterForNotifications( ZeligIR.TypeSystemForCodeTransformation ts,
                                                       ZeligIR.CompilationSteps.DelegationCache cache )
        {
            base.RegisterForNotifications( ts, cache );

            cache.Register( new ZeligIR.CompilationSteps.Handlers.SoftwareFloatingPoint() );

            cache.Register( new ArmV4.Optimizations( this ) );
        }

        public override ZeligIR.ImageBuilders.CompilationState CreateCompilationState( ZeligIR.ImageBuilders.Core core,
                                                                               ZeligIR.ControlFlowGraphStateForCodeTransformation cfg )
        {
            return new LlvmForArmV7MCompilationState( core, cfg );
        }

        //--//

        //
        // Access Methods
        //

        public override string CodeGenerator
        {
            get
            {
                return InstructionSetVersion.CodeGenerator_LLVM;
            }
        }

        public override uint PlatformFamily
        {
            get
            {
                return InstructionSetVersion.Platform_Family__Cortex;
            }
        }

        public override uint PlatformVersion
        {
            get
            {
                return InstructionSetVersion.Platform_Version__ARMv7M;
            }
        }

        public override uint PlatformVFP
        {
            get
            {
                return InstructionSetVersion.Platform_VFP__HardVFP;
            }
        }

        public override bool PlatformBigEndian
        {
            get { return false; }
        }        

        //--//

        public override TypeRepresentation GetMethodWrapperType( )
        {
            return m_typeSystem.GetWellKnownType( "Microsoft_Zelig_ARMv7ForLlvm_MethodWrapper" );
        }

        //
        // Not implememted, and used only during machine code emission
        //

        public override bool HasRegisterContextArgument( MethodRepresentation md )
        {
            return false;
        }

        public override void ComputeSetOfRegistersToSave( ZeligIR.Abstractions.CallingConvention cc,
                                                                   ZeligIR.ControlFlowGraphStateForCodeTransformation cfg,
                                                                   BitVector modifiedRegisters,
                                                               out BitVector registersToSave,
                                                               out Runtime.HardwareException he )
        {
            throw new Exception( "ComputeSetOfRegistersToSave not implemented" );
        }

        //--//

        protected override void ComputeRegisterFlushFixup( BitVector registersToSave )
        {
            throw new Exception( "ComputeRegisterFlushFixup not implemented" );
        }
    }
}