//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using CoAP.Stack.Abstractions;
    using CoAP.Common;
    using CoAP.Common.Diagnostics;


    public abstract class AsyncMessaging
    {
        //
        // State 
        // 

        private   readonly IChannelFactory    m_channelFactory;
        private   readonly IPEndPoint         m_localEndPoint;
        protected          CoAPMessageHandler m_messageHandler;
        protected          CoAPMessageHandler m_errorHandler;

        //--//

        //
        // Constructors
        // 

        public AsyncMessaging( IChannelFactory channelFactory, IPEndPoint localEndPoint )
        {
            m_channelFactory = channelFactory;
            m_localEndPoint  = localEndPoint;
        }

        //
        // Helper methods
        // 

        public event CoAPMessageHandler OnMessage
        {
            add
            {
                m_messageHandler += value;
            }
            remove
            {
                m_messageHandler -= value;
            }
        }

        public event CoAPMessageHandler OnError
        {
            add
            {
                m_errorHandler += value;
            }
            remove
            {
                m_errorHandler -= value;
            }
        }

        public abstract void SendMessageAsync( CoAPMessageRaw msg );
        
        public abstract void Start( );

        public abstract void Stop( );

        //
        // Access methods
        //

        //--//

        public IChannelFactory ChannelFactory
        {
            get
            {
                return m_channelFactory;
            }
        }

        public IPEndPoint LocalEndPoint
        {
            get
            {
                return m_localEndPoint;
            }
        }
    }
}
