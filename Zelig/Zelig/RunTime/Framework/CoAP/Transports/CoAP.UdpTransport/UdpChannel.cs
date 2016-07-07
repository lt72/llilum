//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.UdpTransport
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;


    public class UdpChannel : ICoAPChannel
    {
        //
        // State
        //
        
        private readonly Socket m_udpSocket;

        //--//

        //
        // Contructors
        //

        public UdpChannel( IPEndPoint localEndPoint )
        {
            m_udpSocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
        }

        //
        // Helper methods
        //

        public void Open( IPEndPoint localEndPoint )
        {
            if(m_udpSocket.IsBound)
            {
                return;
            }
            m_udpSocket.Bind( localEndPoint );
        }

        public void Close( )
        {
            m_udpSocket.Close( ); 
        }
        

        public void Bind( EndPoint localEndPoint )
        {
            m_udpSocket.Bind( localEndPoint ); 
        }
        
        public int Send( byte[] buffer, int offset, int count, EndPoint receiver )
        {
            return m_udpSocket.SendTo( buffer, offset, count, SocketFlags.None, receiver );
        }

        public int Receive( byte[] buffer, int offset, int count, ref EndPoint sender )
        {   
            int received = m_udpSocket.ReceiveFrom( buffer, offset, count, SocketFlags.None, ref sender ); 
                
            Buffer.BlockCopy( buffer, 0, buffer, offset, received ); 

            return received; 
        }

        //
        // Access methods
        //
        
        public int Available
        {
            get
            {
                return m_udpSocket.Available;
            }
        }

        public IPEndPoint LocalEndpoint
        {
            get
            {
                return (IPEndPoint)m_udpSocket.LocalEndPoint;
            }
        }

        public int SendTimeout
        {
            get
            {
                throw new NotImplementedException( );
            }

            set
            {
                throw new NotImplementedException( );
            }
        }

        public int ReceiveTimeout
        {
            get
            {
                throw new NotImplementedException( );
            }

            set
            {
                throw new NotImplementedException( );
            }
        }

        public IPEndPoint LocalEndPoint
        {
            get
            {
                return (IPEndPoint)m_udpSocket.LocalEndPoint;
            }
        }
    }
}
