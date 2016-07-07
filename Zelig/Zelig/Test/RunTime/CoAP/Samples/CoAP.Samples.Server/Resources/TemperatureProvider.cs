using System;
using CoAP.Common;
using CoAP.Stack;

namespace CoAP.Samples.Server
{
    internal class TemperatureProvider : StandardResourceProvider
    {
        //
        // State
        //

        private readonly TemperatureSensor m_sensor; 

        //
        // Contructors
        //
        internal TemperatureProvider( int id )
        {
            m_sensor = new TemperatureSensor( id ); 
        }

        public override bool IsImmediate
        {
            get
            {
                return false;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        //--//

        protected override uint GET( string query, out object result )
        {
            result = m_sensor.Temperature;

            return CoAPMessage.Success_WithDetail( CoAPMessage.Detail_Success.Content );
        }
    }
}
