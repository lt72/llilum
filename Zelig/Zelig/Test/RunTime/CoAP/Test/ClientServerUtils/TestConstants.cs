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
        public static readonly IPEndPoint    EndPoint__8080                          = new IPEndPoint( Utils.AddressFromHostName( "localhost" ) , 8080 );
        public static readonly IPEndPoint    EndPoint__8089                          = new IPEndPoint( Utils.AddressFromHostName( "localhost" ) , 8089 );

        public static readonly ServerCoAPUri Resource__PingForAck_Immediate          = new ServerCoAPUri( EndPoint__8080, "ping-send-back-ack-immediate" );
        public static readonly ServerCoAPUri Resource__EchoQuery_Immediate           = new ServerCoAPUri( EndPoint__8080, "echo-query-immediate"         );
        public static readonly ServerCoAPUri Resource__EchoQuery_Immediate_OtherPort = new ServerCoAPUri( EndPoint__8089, "echo-query-immediate"         );
        public static readonly ServerCoAPUri Resource__EchoQuery_Delayed             = new ServerCoAPUri( EndPoint__8080, "echo-query-delayed"           );
        public static readonly ServerCoAPUri Resource__NotFound_Immediate            = new ServerCoAPUri( EndPoint__8080, "not-found-immediate"          );
        public static readonly ServerCoAPUri Resource__NotFound_Delayed              = new ServerCoAPUri( EndPoint__8080, "not-found-delayed"            );

        public static readonly MessageOption.OptionNumber UnsupportedOption__Elective_UnSafe_NoCacheKey = (MessageOption.OptionNumber)(((byte)(MessageOption.OptionNumber.Size1 + 1) | 0x02) & ~0x01);
        public static readonly MessageOption.OptionNumber UnsupportedOption__Critical_UnSafe_NoCacheKey = (MessageOption.OptionNumber)(0x01 | (byte)UnsupportedOption__Elective_UnSafe_NoCacheKey);
    }
}
