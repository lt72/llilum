using System;

namespace CoAP.Samples.Server
{
    internal class TemperatureSensor
    {
        //
        // State 
        //
        
        private readonly Random m_random;
        private readonly int    m_floor;

        //
        //
        //
        internal TemperatureSensor( int floor )
        {
            m_random = new Random( ); 
            m_floor  = floor;
        }

        public float Temperature
        {
            get
            {
                return (float)((m_random.NextDouble( ) * 100) + m_floor); 
            }
        }
    }
}
