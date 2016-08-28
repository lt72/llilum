//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Test.ClientServerUtils
{
    using System.Net;
    using CoAP.Common;
    using CoAP.Stack;


    public static class TestConstants
    {
        public static readonly int ClientPort             = 8081;
        public static readonly int LocalOriginServerPort  = 8080;
        public static readonly int RemoteOriginServerPort = 9999;

        public static readonly IPEndPoint    Client__LocalOriginEndPoint__8081  = new IPEndPoint( IPAddress.Loopback                      , ClientPort );
        public static readonly IPEndPoint    Server__LocalOriginEndPoint__8080  = new IPEndPoint( Utils.AddressFromHostName( "localhost" ), LocalOriginServerPort );
        public static readonly IPEndPoint    Server__LocalOriginEndPoint__8089  = new IPEndPoint( Utils.AddressFromHostName( "localhost" ), 8089 );
        public static readonly IPEndPoint    Server__RemoteOriginEndPoint__9999 = new IPEndPoint( Utils.AddressFromHostName( "localhost" ), RemoteOriginServerPort );

        public static readonly IPEndPoint[] AllRootLocalOriginEndPoints                  = Utils.EndPointsFromHostName( "localhost", LocalOriginServerPort );
        public static readonly IPEndPoint[] AllRootRemoteOriginEndPoints                 = Utils.EndPointsFromHostName( "localhost", 9999 );

        public static readonly CoAPServerUri Resource__CoAPPing                          = new CoAPServerUri( Server__LocalOriginEndPoint__8080, ""                                 );
        public static readonly CoAPServerUri Resource__PingForAck_Immediate              = new CoAPServerUri( Server__LocalOriginEndPoint__8080, "res/ping-send-back-ack-immediate" );
        public static readonly CoAPServerUri Resource__EchoQuery_Immediate               = new CoAPServerUri( Server__LocalOriginEndPoint__8080, "res/echo-query-immediate"         );
        public static readonly CoAPServerUri Resource__EchoQuery_Immediate_OtherPort     = new CoAPServerUri( Server__LocalOriginEndPoint__8089, "res/echo-query-immediate"         );
        public static readonly CoAPServerUri Resource__EchoQuery_Delayed                 = new CoAPServerUri( Server__LocalOriginEndPoint__8080, "res/echo-query-delayed"           );
        public static readonly CoAPServerUri Resource__NotFound_Immediate                = new CoAPServerUri( Server__LocalOriginEndPoint__8080, "res/not-found-immediate"          );
        public static readonly CoAPServerUri Resource__NotFound_Delayed                  = new CoAPServerUri( Server__LocalOriginEndPoint__8080, "res/not-found-delayed"            );

        public static readonly CoAPServerUri Resource__Origin__EchoQuery_Immediate       = new CoAPServerUri( Server__RemoteOriginEndPoint__9999, "res/echo-query-immediate"       );
        public static readonly CoAPServerUri Resource__Origin__EchoQuery_Delayed         = new CoAPServerUri( Server__RemoteOriginEndPoint__9999, "res/echo-query-delayed"         );
        public static readonly CoAPServerUri Resource__ProxyMoniker__EchoQuery_Immediate = new CoAPServerUri( Server__LocalOriginEndPoint__8080 , "proxy/res/echo-query-immediate" );
        public static readonly CoAPServerUri Resource__ProxyMoniker__EchoQuery_Delayed   = new CoAPServerUri( Server__LocalOriginEndPoint__8080 , "proxy/res/echo-query-delayed"   );

        public static readonly MessageOption.OptionNumber UnsupportedOption__Elective_UnSafe_NoCacheKey = (MessageOption.OptionNumber)(((byte)(MessageOption.OptionNumber.Size1 + 1) | 0x02) & ~0x01);
        public static readonly MessageOption.OptionNumber UnsupportedOption__Critical_UnSafe_NoCacheKey = (MessageOption.OptionNumber)(0x01 | (byte)UnsupportedOption__Elective_UnSafe_NoCacheKey);        
    }
}
