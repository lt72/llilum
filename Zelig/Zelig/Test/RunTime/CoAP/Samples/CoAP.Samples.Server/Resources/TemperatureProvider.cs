using System;
using CoAP.Stack.Abstractions;
using CoAP.Common;
using CoAP.Stack;
using CoAP.Server;

namespace CoAP.Samples.Server
{
    internal sealed class TemperatureProvider : ResourceProvider
    {
        //
        // State
        //

        private readonly TemperatureSensor m_sensor; 

        //
        // Contructors
        //
        internal TemperatureProvider( int id ) : base( true, false )
        {
            m_sensor = new TemperatureSensor( id ); 
        }

        public override bool CanFetchImmediateResponse( CoAPMessage request )
        {
            return true;
        }

        //--//

        protected override uint GET( string path, string[ ] query, out MessagePayload payload )
        {
            payload = MessagePayload_Int.New( (int)m_sensor.Temperature );

            return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Content );
        }
    }
}
