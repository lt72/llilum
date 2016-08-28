//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using CoAP.Common.Diagnostics;
    using System.Threading;
    using CoAP.Common;
    using CoAP.Stack;

    public static class CoApTestAsserts
    {
        public static void Assert( bool condition )
        {
            if(condition == false)
            {
                Debug.Assert( false ); 
            }
        }
        
        #region Message Format 

        public static void Assert_Version( CoAPMessage response, CoAPMessage.ProtocolVersion version )
        {
            Assert( response.Version == version );
        }

        public static void Assert_Type( CoAPMessage response, params CoAPMessage.MessageType[] types )
        {
            bool fOk = false;
            foreach(var t in types)
            {
                if(response.Type == t)
                {
                    fOk = true;
                    break;
                }
            }
            Assert( fOk );
        }

        public static void Assert_Code( CoAPMessage response, CoAPMessage.Class cls, CoAPMessage.Detail_Request detail )
        {
            Assert( response.ClassCode          == cls );
            Assert( response.DetailCode_Request == detail );
        }

        public static void Assert_Code( CoAPMessage response, CoAPMessage.Class cls, CoAPMessage.Detail_Success detail )
        {
            Assert( response.ClassCode          == cls );
            Assert( response.DetailCode_Success == detail );
        }

        public static void Assert_Code( CoAPMessage response, CoAPMessage.Class cls, CoAPMessage.Detail_RequestError detail )
        {
            Assert( response.ClassCode               == cls );
            Assert( response.DetailCode_RequestError == detail );
        }

        public static void Assert_Code( CoAPMessage response, CoAPMessage.Class cls, CoAPMessage.Detail_ServerError detail )
        {
            Assert( response.ClassCode          == cls );
            Assert( response.DetailCode_ServerError == detail );
        }

        public static void Assert_SameMessageId( CoAPMessageRaw request, CoAPMessage response )
        {
            Assert( request.MessageId == response.MessageId );
        }

        public static void Assert_NotSameMessageId( CoAPMessageRaw request, CoAPMessage response )
        {
            Assert( request.MessageId != response.MessageId );
        }

        public static void Assert_SameToken( CoAPMessageRaw request, CoAPMessage response )
        {
            Assert( request.Token.Equals( response.Token ) );
        }

        public static void Assert_EmptyPayload( MessagePayload payload )
        {
            Assert( Utils.ByteArrayCompare( (byte[ ])payload.Value, (byte[ ])MessagePayload.EmptyPayload.Value ) );
        }

        public static void Assert_Payload( MessagePayload payload, MessagePayload response )
        {
            Assert( Utils.ByteArrayCompare( (byte[ ])payload.Value, (byte[ ])response.Value ) );
        }

        public static void Assert_Payload( byte[ ] payload, MessagePayload response )
        {
            Assert( Utils.ByteArrayCompare( payload, (byte[ ])response.Value ) );
        }

        #endregion

        #region Statistics

        public static void Assert_Statistics( Statistics resultStats, Statistics expectedStats, int timeout )
        {
            bool fOk = true;

            var result   = resultStats  .ValuesToArray();
            var expected = expectedStats.ValuesToArray();

            var statsNames = resultStats.NamesToArray();
            var failureMessages = new List<string>();

            for(int i = 0; i < expected.Length; ++i)
            {
                var res   = result  [ i ]  ; 
                var xpctd = expected[ i ]();
                
                bool fChecks = VerifyStat( statsNames[ i ], xpctd, timeout, res );

                if(fChecks == false)
                {
                    failureMessages.Add( $"FAILURE: stats for '{statsNames[ i ]}' do not match! Got {res()}, expected {xpctd}" );
                    
                    fOk = false;
                }
            }

            if(fOk == false)
            {
                foreach(var msg in failureMessages)
                {
                    Logger.Instance.LogError( msg );
                }

                CoApTestAsserts.Assert( false );
            }
        }

        public static void Assert_AcksSent( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "AcksSent", count, timeout, new Func<int>( ( ) => stats.AcksSent ) ) );
        }

        public static void Assert_AcksReceived( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "AcksReceived", count, timeout, new Func<int>( ( ) => stats.AcksReceived ) ) );
        }

        public static void Assert_ResetsSent( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "ResetsSent", count, timeout, new Func<int>( ( ) => stats.ResetsSent ) ) );
        }

        public static void Assert_RequestsReceived( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "RequestsReceived", count, timeout, new Func<int>( ( ) => stats.RequestsReceived ) ) );
        }

        public static void Assert_RequestsSent( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "RequestsSent", count, timeout, new Func<int>( ( ) => stats.RequestsSent ) ) ); 
        }

        public static void Assert_ResetsReceived( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "ResetsReceived", count, timeout, new Func<int>( ( ) => stats.ResetsReceived ) ) );
        }

        public static void Assert_RequestTransmissions( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "RequestsRetransmissions", count, timeout, new Func<int>( ( ) => stats.RequestsRetransmissions ) ) );
        }

        public static void Assert_ImmediateResposesSent( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "ImmediateResposesSent", count, timeout, new Func<int>( ( ) => stats.ImmediateResposesSent ) ) );
        }

        public static void Assert_ImmediateResposesReceived( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "ImmediateResposesReceived", count, timeout, new Func<int>( ( ) => stats.ImmediateResposesReceived ) ) );
        }

        public static void Assert_DelayedResposesSent( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "DelayedResponsesSent", count, timeout, new Func<int>( ( ) => stats.DelayedResponsesSent ) ) );
        }

        public static void Assert_DelayedResposesesReceived( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "DelayedResponsesReceived", count, timeout, new Func<int>( ( ) => stats.DelayedResponsesReceived ) ) );
        }

        public static void Assert_DelayedResposesTransmissions( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "DelayedResposesRetransmissions", count, timeout, new Func<int>( ( ) => stats.DelayedResposesRetransmissions ) ) );
        }

        public static void Assert_CacheHits( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "CacheHits", count, timeout, new Func<int>( ( ) => stats.CacheHits ) ) );
        }

        public static void Assert_CacheMisses( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "CacheMisses", count, timeout, new Func<int>( ( ) => stats.CacheMisses ) ) );
        }

        public static void Assert_Errors( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( "Errors", count, timeout, new Func<int>( ( ) => stats.Errors ) ) );
        }

        #endregion

        private static bool VerifyStat( string name, int count, int timeout, Func<int> targetStat )
        {
            int loops = timeout / 100;

            int loop = 0;
            bool fOk = true;

            var res = targetStat(); 
            while(res < count && loop < loops)
            {
                ++loop;

                Logger.Instance.LogWarning( $"Stat '{name}' do not match, got {res} but expected {count}, wait..." );

                Thread.Sleep( 100 );

                res = targetStat( ); 
            }

            if(targetStat( ) != count)
            {
                fOk = false;
            }

            return fOk;
        }
    }
}
