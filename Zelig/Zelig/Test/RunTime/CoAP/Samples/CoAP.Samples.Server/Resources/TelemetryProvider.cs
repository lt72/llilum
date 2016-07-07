namespace CoAP.Samples.Server
{
    class TelemetryProvider : CoAP.Stack.Abstractions.AbstractTelemetryProvider
    {
        //
        // State
        // 

        private readonly BasicUdpServer m_server;

        //--//

        //
        // Contructors
        // 

        internal TelemetryProvider( BasicUdpServer server )
        {
            m_server = server; 
        }

        //
        // Helper Methods
        // 

        public override long GetValue( string tag )
        {
            switch(tag)
            {
                case CoAP.Stack.Abstractions.AbstractTelemetryProvider.CumulativeReadings_Tag:
                    return CumulativeReadings;
                default:
                    throw new CoAP.Stack.CoAP_TelemetryTagNotSupportException( ); 
            }
        }
    }
}
