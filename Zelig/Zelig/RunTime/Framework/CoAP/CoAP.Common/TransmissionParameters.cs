//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Common
{
    public static class TransmissionParameters
    {

        //  +-------------------+---------------+
        //  | name              | default value |
        //  +-------------------+---------------+
        //  | MAX_TRANSMIT_SPAN |          45 s |
        //  | MAX_TRANSMIT_WAIT |          93 s |
        //  | MAX_LATENCY       |         100 s |
        //  | PROCESSING_DELAY  |           2 s |
        //  | MAX_RTT           |         202 s |
        //  | EXCHANGE_LIFETIME |         247 s |
        //  | NON_LIFETIME      |         145 s |
        //  +-------------------+---------------+

        public const  int    default_ACK_TIMEOUT       =  2 * 1000; // milliseconds.  !!! making this value lower than 1 second is a violation of recommendations in RFC5405 !!!
        public static int            ACK_TIMEOUT       = default_ACK_TIMEOUT;
        public const  double default_ACK_RANDOM_FACTOR = 1.5; // count
        public static double         ACK_RANDOM_FACTOR = default_ACK_RANDOM_FACTOR;
        public const  int    default_MAX_RETRANSMIT    = 4; // count
        public static int            MAX_RETRANSMIT    = default_MAX_RETRANSMIT;
        public const  int    default_NSTART            = 1; // count
        public static int            NSTART            = default_NSTART;
        public const  int    default_DEFAULT_LEISURE   = 5000; // millisenconds
        public const  int            DEFAULT_LEISURE   = default_DEFAULT_LEISURE;
        public const  int    default_PROBING_RATE      = 1; // byte / second
        public const  int            PROBING_RATE      = default_PROBING_RATE;

        //--//

        /// <summary>
        /// Maximum time from the first transmission of a Confirmable message to its last retransmission.
        /// (ACK_TIMEOUT * ((2 ** MAX_RETRANSMIT) - 1) * ACK_RANDOM_FACTOR)
        /// </summary>
        public  const  int default_MAX_TRANSMIT_SPAN = 45 * 1000;                   // milliseconds
        
        public static int MAX_TRANSMIT_SPAN
        {
            get
            {
                //
                // Use pre-computed value when possible 
                //
                if( MAX_RETRANSMIT    == default_MAX_RETRANSMIT     &&
                    ACK_TIMEOUT       == default_ACK_TIMEOUT        && 
                    ACK_RANDOM_FACTOR == default_ACK_RANDOM_FACTOR   )
                {
                    return default_MAX_TRANSMIT_SPAN;
                }

                int res = 0;

                for(int i = 0; i < MAX_RETRANSMIT; ++i)
                {
                    res += ACK_TIMEOUT << i;
                }

                return (int)(res * ACK_RANDOM_FACTOR); 
            }
        }

        /// <summary>
        /// Maximum time from the first transmission of a Confirmable message to the time when the sender gives up on receiving an acknowledgement or reset.
        /// (ACK_TIMEOUT * ((2 ** (MAX_RETRANSMIT + 1)) - 1) * ACK_RANDOM_FACTOR)
        /// </summary>
        public const int default_MAX_TRANSMIT_WAIT = 93 * 1000; // milliseconds 

        public static int MAX_TRANSMIT_WAIT
        {
            get
            {
                //
                // Use pre-computed value when possible 
                //
                if( MAX_RETRANSMIT    == default_MAX_RETRANSMIT     &&
                    ACK_TIMEOUT       == default_ACK_TIMEOUT        && 
                    ACK_RANDOM_FACTOR == default_ACK_RANDOM_FACTOR   )
                {
                    return default_MAX_TRANSMIT_WAIT;
                }

                int res = 0;

                for(int i = 0; i < MAX_RETRANSMIT + 1; ++i)
                {
                    res += ACK_TIMEOUT << i;
                }

                return (int)(res * ACK_RANDOM_FACTOR);
            }
        }

        /// <summary>
        /// maximum time a datagram is expected to take from the start of its transmission to the completion of its reception. 
        /// Arbitrarily defined to be 100 seconds. Such values allows the message ID lifetime timer to be contained in 8 bits, 
        /// when represented in seconds.
        /// </summary>
        public const int default_MAX_LATENCY = 100 * 1000; // milliseconds

        /// <summary>
        /// Time a node takes to turn around a Confirmable message into an acknowledgement. 
        /// Per RFC7252, we assume the node will attempt to send an ACK before having the sender time out, so as a conservative assumption 
        /// we set it equal to ACK_TIMEOUT. 
        /// </summary>
        public const int default_PROCESSING_DELAY = default_ACK_TIMEOUT; // milliseconds

        /// <summary>
        /// The maximum round-trip time. 
        /// (2 * MAX_LATENCY) + PROCESSING_DELAY
        /// </summary>
        public const int default_MAX_RTT = 202 * 1000; // milliseconds
        

        /// <summary>
        /// The time from starting to send a Confirmable message to the time when an acknowledgement is no longer expected, e.g. the time at which 
        /// message-layer information about the message exchange can be purged. 
        /// EXCHANGE_LIFETIME includes a MAX_TRANSMIT_SPAN, a MAX_LATENCY forward, PROCESSING_DELAY, and a MAX_LATENCY for the way back. 
        /// Note that there is no need to consider MAX_TRANSMIT_WAIT if the configuration is chosen such that the last waiting period 
        /// (ACK_TIMEOUT* (2 ** MAX_RETRANSMIT) or the difference between MAX_TRANSMIT_SPAN and MAX_TRANSMIT_WAIT) is less than MAX_LATENCY -- 
        /// which is a likely choice, as MAX_LATENCY is a worst-case value unlikely to be met in the real world.  In this case, EXCHANGE_LIFETIME 
        /// simplifies to:
        ///  MAX_TRANSMIT_SPAN + (2 * MAX_LATENCY) + PROCESSING_DELAY
        /// </summary>
        public const int default_EXCHANGE_LIFETIME = 247 * 1000; // milliseconds

        public static int EXCHANGE_LIFETIME
        {
            get
            {
                return MAX_TRANSMIT_SPAN + (2 * default_MAX_LATENCY) + default_PROCESSING_DELAY; 
            }
        }

        /// <summary>
        /// Time from sending a Non-confirmable message to the time its Message ID can be safely reused. 
        /// If multiple transmission of a NON message is not used, its value is MAX_LATENCY, or 100 seconds.However, a CoAP sender might send a 
        /// NON message multiple times, in particular for multicast applications.While the period of reuse is not bounded by the  specification, 
        /// an expectation of reliable detection of duplication at the receiver is on the timescales of MAX_TRANSMIT_SPAN. 
        /// Therefore, for this purpose, it is safer to use the value:
        /// MAX_TRANSMIT_SPAN + MAX_LATENCY
        /// </summary>
        public const int default_NON_LIFETIME = 145 * 1000; // milliseconds

        public static int NonLifeTime
        {
            get
            {
                return MAX_TRANSMIT_SPAN + default_MAX_LATENCY; 
            }
        }

        //--//

        public static int InitialTimeout
        {
            get
            {
                return (int)(ACK_TIMEOUT * ACK_RANDOM_FACTOR) + 1;
            }
        }

        public static bool ShouldRetry( ref int retries, ref int timeout )
        {
            //
            // A message is retransmitted when the current re-transmission counter is less than MAX_RETRANSMIT.
            // Timeout is doubled at each retry. 
            // 
            if(--retries > 0)
            {
                timeout *= 2;

                return true;
            }

            return false;
        }
    }
}
