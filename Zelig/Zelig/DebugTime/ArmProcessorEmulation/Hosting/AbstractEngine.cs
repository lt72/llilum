﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Zelig.Emulation.Hosting
{
    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public abstract class AbstractEngine : AbstractHost
    {
        //
        // State
        //

        protected readonly InstructionSetARM m_instructionSet;
        
        //
        // Constructor Methods
        //

        public AbstractEngine(InstructionSetARMv4 iset)
        {
            m_instructionSet = iset;
        }

        public bool CanDecode()
        {
            return m_instructionSet != null;
        }

        public InstructionSetARM InstructionSet
        {
            get
            {
                return m_instructionSet;
            }
        }
    }
}
