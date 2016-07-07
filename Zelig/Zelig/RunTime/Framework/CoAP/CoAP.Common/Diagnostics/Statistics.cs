//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Common.Diagnostics
{
    using System;
    using System.Collections.Generic;


    public class Statistics
    {
        private readonly List<Func<int>> m_funcs;

        /// <summary>
        /// Number of ACKs received 
        /// </summary>
        public int AcksReceived { get; set; }
        /// <summary>
        /// Number of ACKs sent
        /// </summary>
        public int AcksSent { get; set; }
        /// <summary>
        /// Number of requests received
        /// </summary>
        public int RequestsReceived { get; set; }
        /// <summary>
        /// Number of requests sent
        /// </summary>
        public int RequestsSent { get; set; }
        /// <summary>
        /// Number of retransmissions for requests, not inclusive of the very first instance of transmission
        /// </summary>
        public int RequestsRetransmissions { get; set; }
        /// <summary>
        /// Number of reset messages received
        /// </summary>
        public int ResetsReceived { get; set; }
        /// <summary>
        /// Number of reset messages sent
        /// </summary>
        public int ResetsSent { get; set; }
        /// <summary>
        /// Number of immediate responses received, not inclusive of resets
        /// </summary>
        public int ImmediateResposesReceived { get; set; }
        /// <summary>
        /// Number of immediate responses sent, not inclusive of resets
        /// </summary>
        public int ImmediateResposesSent { get; set; }
        /// <summary>
        /// Number of delayed responses received, not inclusive of resets
        /// </summary>
        public int DelayedResposesesReceived { get; set; }
        /// <summary>
        /// Number of delayed responses sent, not inclusive of resets
        /// </summary>
        public int DelayedResponsesSent { get; set; }
        /// <summary>
        /// Number of retransmissions for delayed responses, inot inclusive of the very first instance of transmission
        /// </summary>
        public int DelayedResposesRetransmissions { get; set; }
        /// <summary>
        /// Number of errors, both in the stack and its usage
        /// </summary>
        public int Errors { get; set; }

        //--//

        public Statistics( )
        {
            m_funcs = new List<Func<int>>( );

            m_funcs.Add( new Func<int>( ( ) => this.AcksReceived                   ) );
            m_funcs.Add( new Func<int>( ( ) => this.AcksSent                       ) );
            m_funcs.Add( new Func<int>( ( ) => this.RequestsReceived               ) );
            m_funcs.Add( new Func<int>( ( ) => this.RequestsSent                   ) );
            m_funcs.Add( new Func<int>( ( ) => this.ResetsReceived                 ) );
            m_funcs.Add( new Func<int>( ( ) => this.ResetsSent                     ) );
            m_funcs.Add( new Func<int>( ( ) => this.RequestsRetransmissions        ) );
            m_funcs.Add( new Func<int>( ( ) => this.ImmediateResposesReceived      ) );
            m_funcs.Add( new Func<int>( ( ) => this.ImmediateResposesSent          ) );
            m_funcs.Add( new Func<int>( ( ) => this.DelayedResposesesReceived      ) );
            m_funcs.Add( new Func<int>( ( ) => this.DelayedResponsesSent            ) );
            m_funcs.Add( new Func<int>( ( ) => this.DelayedResposesRetransmissions ) );
            m_funcs.Add( new Func<int>( ( ) => this.Errors                         ) );

            Clear( );
        }

        //public Statistics(
        //    int acksReceived, 
        //    int acksSent,
        //    int requestsReceived,
        //    int requestsSent,
        //    int resetsReceived,
        //    int resetsSent,
        //    int requestsTransmissions,
        //    int immediateResposesReceived,
        //    int immediateResposesSent,
        //    int delayedResposesesReceived,
        //    int delayedResposesSent,
        //    int delayedResposesTransmissions,
        //    int errors ) : this( )
        //{
        //    this.AcksReceived                    = acksReceived;
        //    this.AcksSent                        = acksSent;
        //    this.RequestsReceived                = requestsReceived;
        //    this.RequestsSent                    = requestsSent;
        //    this.ResetsReceived                  = resetsReceived;
        //    this.ResetsSent                      = resetsSent;
        //    this.RequestsTransmissions           = requestsTransmissions;
        //    this.ImmediateResposesReceived       = immediateResposesReceived;
        //    this.ImmediateResposesSent           = immediateResposesSent;
        //    this.DelayedResposesesReceived       = delayedResposesesReceived;
        //    this.DelayedResposesSent             = delayedResposesSent;
        //    this.DelayedResposesTransmissions    = delayedResposesTransmissions;
        //    this.Errors                          = errors;
        //}

        public void Clear( )
        {
            AcksReceived                    = 0;
            AcksSent                        = 0;
            RequestsReceived                = 0;
            RequestsSent                    = 0;
            ResetsReceived                  = 0;
            ResetsSent                      = 0;
            RequestsRetransmissions         = 0;
            ImmediateResposesReceived       = 0;
            ImmediateResposesSent           = 0;
            DelayedResposesesReceived       = 0;
            DelayedResponsesSent             = 0;
            DelayedResposesRetransmissions  = 0;
            Errors                          = 0;
        }

        public Func<int>[] ToArray( )
        {
            return m_funcs.ToArray( ); 
        }
    }
}
