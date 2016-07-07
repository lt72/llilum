//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using System;
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
            var msg = CoAPMessage.FromBuffer( request.Buffer );

            using(var parser = MessageParser.CheckOutParser( ))
            {
                parser.Inflate( msg );
            }

            Assert( msg.MessageId == response.MessageId ); 
        }

        public static void Assert_NotSameMessageId( CoAPMessageRaw request, CoAPMessage response )
        {
            var msg = CoAPMessage.FromBuffer( request.Buffer );

            using(var parser = MessageParser.CheckOutParser( ))
            {
                parser.Inflate( msg );
            }

            Assert( msg.MessageId != response.MessageId );
        }

        public static void Assert_SameToken( CoAPMessageRaw request, CoAPMessage response )
        {
            var msg = CoAPMessage.FromBuffer( request.Buffer );

            using(var parser = MessageParser.CheckOutParser( ))
            {
                parser.Inflate( msg );
            }

            Assert( msg.Token.Equals( response.Token ) ); 
        }
        
        public static void Assert_Payload( byte[ ] payload, CoAPMessage response )
        {
            Assert( Utils.ByteArrayCompare( response.Payload.Payload, payload ) ); 
        }

        #endregion

        #region Statistics

        public static void Assert_Statistics( Statistics target, Statistics comparand, int timeout )
        {
            bool fOk = true;

            var t = target   .ToArray();
            var c = comparand.ToArray();

            for(int i = 0; i < c.Length; ++i)
            {
                bool fChecks = VerifyStat( c[ i ](), timeout, t[ i ] );

                if(fChecks == false)
                {
                    Logger.Instance.LogError( $"FAILURE: stats do not match" );
                    fOk = false;
                }
            }

            CoApTestAsserts.Assert( fOk ); 
        }

        public static void Assert_AcksSent( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.AcksSent ) ) );
        }

        public static void Assert_AcksReceived( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.AcksReceived ) ) );
        }

        public static void Assert_ResetsSent( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.ResetsSent ) ) );
        }

        public static void Assert_RequestsReceived( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.RequestsReceived ) ) );
        }

        public static void Assert_RequestsSent( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.RequestsSent ) ) ); 
        }

        public static void Assert_ResetsReceived( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.ResetsReceived ) ) );
        }

        public static void Assert_RequestTransmissions( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.RequestsRetransmissions ) ) );
        }

        public static void Assert_ImmediateResposesSent( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.ImmediateResposesSent ) ) );
        }

        public static void Assert_ImmediateResposesReceived( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.ImmediateResposesReceived ) ) );
        }

        public static void Assert_DelayedResposesSent( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.DelayedResponsesSent ) ) );
        }

        public static void Assert_DelayedResposesesReceived( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.DelayedResposesesReceived ) ) );
        }

        public static void Assert_DelayedResposesTransmissions( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.DelayedResposesRetransmissions ) ) );
        }

        public static void Assert_Errors( Statistics stats, int count, int timeout )
        {
            CoApTestAsserts.Assert( VerifyStat( count, timeout, new Func<int>( ( ) => stats.Errors ) ) );
        }

        #endregion

        private static bool VerifyStat( int count, int timeout, Func<int> targetStat )
        {
            int loops = timeout / 100;

            int loop = 0;
            bool fOk = true;
            while(targetStat( ) < count && loop < loops)
            {
                ++loop;

                Logger.Instance.LogWarning( $"Stats do not match, wait..." );

                Thread.Sleep( 100 );
            }

            if(targetStat( ) != count)
            {
                fOk = false;
            }

            return fOk;
        }
    }
}
