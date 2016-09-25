//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.CortexM4
{
    using Microsoft.DeviceModels.Chipset.CortexM;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;

    public abstract class Processor : ChipsetModel.ProcessorARMv7MForZelig
    {

        public abstract new class Context : ChipsetModel.ProcessorARMv7MForZelig.Context
        {
            //
            // Constructor Methods
            //

            //
            // Helper Methods
            //

            //
            // Access Methods
            //
        }


        //
        // Helper Methods
        //

        public override void InitializeProcessor()
        {
            base.InitializeProcessor();
        }
    }
}
