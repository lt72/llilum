//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Diagnostics;
    using CoAP.Common;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;
    using System;

    public partial class MessageProcessor
    {
        internal class ProcessingState_MessageReceived : ProcessingState
        {
            private ProcessingState_MessageReceived( )
            {
            }

            internal static ProcessingState Get( )
            {
                return new ProcessingState_MessageReceived( );
            }

            //
            // Helper methods
            // 

            public override void Process( )
            {
                var processor = this.Processor;

                var stats = processor.Engine.Owner.Statistics;

                var messageCtx = processor.MessageContext;
                var msg        = messageCtx.MessageInflated;                
                var queries    = msg.Options.Queries;
                var proc       = (MessageProcessor)processor;

                ProcessingState.State state = State.ImmediateResponseAvailable;

                Logger.Instance.Log( string.Format( $"==(S)==> Received message from {messageCtx.Source}: '{msg}'" ) ); 

                if(queries.Count > 0)
                {
                    stats.RequestsReceived++;

                    if(queries.Count == 1)
                    {
                        var server = processor.Engine.Owner;

                        IResourceProvider provider = server.QueryProvider( queries[0] );

                        if(provider == null)
                        {
                            stats.Errors++;

                            //
                            // There is no resource associated with this query => send 404 'Not Found'
                            // 
                            messageCtx.ResponseCode = CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.NotFound );

                            state = State.ImmediateResponseAvailable;
                        }
                        else
                        {
                            //
                            // check if provider only supports GET
                            //
                            if(provider.IsReadOnly && msg.IsGET == false)
                            {
                                //
                                // Unsupported request method
                                // 

                                stats.Errors++;

                                Logger.Instance.LogWarning( "==(S)==> Server received unsupported method request." );

                                messageCtx.ResponseCode = CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );

                                state = State.ImmediateResponseAvailable;
                            }
                            else
                            {
                                if(provider.IsImmediate)
                                {
                                    //
                                    // There is a resource associated with this query and it is immediately available 
                                    // 
                                    object res = null;
                                    uint responseCode = CoAPMessage.RequestError_WithDetail(CoAPMessage.Detail_RequestError.MethodNotAllowed );

                                    messageCtx.ResponseCode = provider.ExecuteMethod( msg.DetailCode_Request, queries[ 0 ], out res );
                                    if(res != null)
                                    {
                                        messageCtx.ResponsePayload = Defaults.Encoding.GetBytes( res.ToString( ) );
                                    }
                                    
                                    state = State.ImmediateResponseAvailable;
                                }
                                else
                                {
                                    //
                                    // There is a resource associated with this query but it is not immediately available => send 000 'Ack'
                                    // 
                                    messageCtx.ResourceHandler = proc.Engine.Owner.CreateResourceHandler( provider );

                                    state = State.DelayedProcessing;
                                }
                            }
                        }
                    }
                    else
                    {
                        //
                        // Multiple queries, Not supported => Send BadRequest
                        // 

                        stats.Errors++;

                        Logger.Instance.LogWarning( "==(S)==> Server received unsupported multiple query or no query..." );

                        messageCtx.ResponseCode = CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.BadRequest );

                        state = State.ImmediateResponseAvailable;
                    }
                }
                else
                {
                    //
                    // Not a query...
                    // 
                    if(msg.IsReset)
                    {
                        //
                        // Client sent a reset...
                        // 

                        Logger.Instance.LogWarning( "==(S)==> Server received a RESET..." );

                        state = State.ResetReceived;
                    }
                    else if(msg.IsAck)
                    {
                        //
                        // Client sent a ACK...
                        // 

                        Logger.Instance.Log( $"==(S)==> Server received a ACK for Message ID '{msg.MessageId}'" );

                        state = State.AckReceived;
                    }
                    else if(msg.IsEmpty)
                    {
                        //
                        // CoAP ping, sending reset
                        // 

                        Logger.Instance.LogWarning( "==(S)==> Server received CoAP Ping, sending a RESET..." );

                        state = State.SendReset;
                    }
                    else
                    {
                        //
                        // Bogus query, sending reset
                        // 

                        Logger.Instance.LogWarning( "==(S)==> Server received bad query, sending a RESET..." );

                        state = State.SendReset;
                    }
                }

                Advance( state );
            }

            //--//
        }
    }
}
