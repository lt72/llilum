//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;


    public class WaitingRecordHolder : IDisposable
    {

        public class WaitingRecord
        {
            private readonly AutoResetEvent m_responseAvailable;
            private readonly CoAPMessageRaw m_request;
            private          CoAPMessage    m_response;
            private          int            m_timeout;
            private          bool           m_waiting;
            private          uint           m_error;

            //--//

            //
            // Contructors 
            //

            public WaitingRecord( CoAPMessageRaw request )
            {
                m_responseAvailable = new AutoResetEvent( false );
                m_request           = request;
                m_waiting           = true;
            }

            //
            // Helper Methods
            // 

            public void ResetTimeout( )
            {
                m_responseAvailable.Set( ); 
            }

            //
            // Access Methods
            // 

            public CoAPMessage Response
            {
                get
                {
                    Debug.Assert( m_timeout > 0 );

                    while(m_waiting)
                    {
                        if(m_responseAvailable.WaitOne( m_timeout, false ) == false)
                        {
                            //
                            // leave the waiting loop if timeout elapses
                            // 

                            break;
                        }
                    }

                    return m_response;
                }
                set
                {
                    m_response = value;
                    m_waiting  = false;

                    m_responseAvailable.Set( );
                }
            }

            public MessageToken Token
            {
                get
                {
                    return m_request.Token;
                }
            }

            public ushort MessageId
            {
                get
                {
                    return m_request.MessageId;
                }
            }

            public int Timeout
            {
                get
                {
                    return m_timeout;
                }
                internal set
                {
                    m_timeout = value;
                }
            }

            public bool Waiting
            {
                get
                {
                    return m_waiting;
                }
            }

            public uint Error
            {
                get
                {
                    return m_error;
                }
                set
                {
                    m_error = value;
                }
            }
        }

        //--//

        //
        // State 
        //
        
        private static readonly Dictionary<MessageToken, WaitingRecord> s_waitTable;
        private static readonly object                                  s_sync;
        //--//
        private        readonly WaitingRecord m_wr;

        //--//

        static WaitingRecordHolder()
        { 
            s_waitTable = new Dictionary<MessageToken, WaitingRecord>( new MessageToken.TokenComparer() );
            s_sync      = new object( ); 
        }

        private WaitingRecordHolder( WaitingRecord wr )
        {
            m_wr = wr;
        }

        public static WaitingRecordHolder WaitResponse( CoAPMessageRaw request)
        {
            var wr = new WaitingRecordHolder( new WaitingRecord( request ) ); 

            lock(s_sync)
            {
                s_waitTable.Add( request.Token, wr.m_wr ); 
            }

            return wr;
        }

        public static WaitingRecord Get( CoAPMessage msg, bool fCheckMessageId )
        {
            WaitingRecord wr = null;

            lock(s_sync)
            {
                if(s_waitTable.TryGetValue( msg.Token, out wr ))
                {
                    if(fCheckMessageId && wr.MessageId != msg.MessageId)
                    {
                        return null;
                    }
                }
            }

            return wr;
        }

        public void Dispose( )
        {
            lock(s_sync)
            {
                s_waitTable.Remove( m_wr.Token );
            }
        }

        //
        // Helper methods 
        //

        public CoAPMessage Response
        {
            get
            {
                return m_wr.Response;
            }
        }

        public int Timeout
        {
            get
            {
                return m_wr.Timeout;
            }
            set
            {
                m_wr.Timeout = value;
            }
        }

        public bool Waiting
        {
            get
            {
                return m_wr.Waiting;
            }
        }

        public uint Error
        {
            get
            {
                return m_wr.Error;
            }
        }
    }
}
