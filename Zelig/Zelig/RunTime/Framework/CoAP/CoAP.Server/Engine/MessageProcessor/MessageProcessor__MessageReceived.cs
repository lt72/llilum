//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Diagnostics;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Common.Diagnostics;
    using System;

    internal partial class MessageProcessor
    {
        internal sealed class ProcessingState_MessageReceived : ProcessingState
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

            internal override void Process( )
            {
                var processor = this.Processor;

                var engine     = processor.MessageEngine;
                var stats      = engine.Owner.Statistics;
                var messageCtx = processor.MessageContext;
                var msg        = messageCtx.MessageInflated;
                var proc       = processor;

                ProcessingState.State state = State.ImmediateResponseAvailable;

                Logger.Instance.Log( string.Format( $"==[S({engine.LocalEndPoint})]==> Rx ID={msg.MessageId} from {messageCtx.Source}: '{msg}'" ) );

                //
                // Check if this is a query or anything else
                //
                string path = msg.Options.Path;

                if(String.IsNullOrEmpty( path ))
                {
                    //
                    // Not a query...
                    // 

                    if(msg.IsReset)
                    {
                        //
                        // Client sent a reset...
                        // 

                        Logger.Instance.LogWarning( $"==[S({engine.LocalEndPoint})]==> Rx RESET ID={msg.MessageId}..." );

                        state = State.ResetReceived;
                    }
                    else if(msg.IsAck)
                    {
                        //
                        // Client sent a ACK...
                        // 

                        Logger.Instance.Log( $"==[S({engine.LocalEndPoint})]==> Rx ACK ID={msg.MessageId}" );

                        state = State.AckReceived;
                    }
                    else if(msg.IsEmpty)
                    {
                        //
                        // CoAP ping, sending reset
                        // 

                        Logger.Instance.LogWarning( $"==[S({engine.LocalEndPoint})]==> Rx ID={msg.MessageId} (CoAP Ping), sending a RESET..." );

                        state = State.SendReset;
                    }
                    else
                    {
                        //
                        // Bogus message, sending reset
                        // 

                        Logger.Instance.LogWarning( $"==[S({engine.LocalEndPoint})]==> ID={msg.MessageId} bad message, sending a RESET..." );

                        state = State.SendReset;
                    }
                }
                else
                {
                    stats.RequestsReceived++;
                    
                    //
                    // Find a provider, whether a local one or a proxy...
                    //

                    IResourceProvider provider = this.Processor.MessageEngine.Owner.QueryProvider( path );

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
                        // check if provider only supports GET, which is by far the most common case
                        //
                        if(provider.IsReadOnly && msg.IsGET == false)
                        {
                            //
                            // Unsupported request method
                            // 

                            stats.Errors++;

                            Logger.Instance.LogWarning( $"==[S({this.Processor.MessageEngine.LocalEndPoint})]==> Server received unsupported method request." );

                            messageCtx.ResponseCode = CoAPMessage.RequestError_WithDetail( CoAPMessage.Detail_RequestError.MethodNotAllowed );

                            state = State.ImmediateResponseAvailable;
                        }
                        else
                        {
                            //
                            // Check if we can send the response as a Piggybacked response or if we need to Ack: a local provider 
                            // can be 'immediate' or 'delayed', a proxied provider will be treated as 'delayed' if there is no 
                            // fresh and valid represensation in the cache. Since the proxy provider handles the cache, there is no 
                            // difference between proxy and local providers at this level. 
                            //
                            if(provider.CanFetchImmediateResponse( msg ))
                            {
                                MessagePayload payload = null;
                                MessageOptions options = new MessageOptions();

                                //
                                // There is a resource associated with this query and it is immediately available 
                                // 
                                messageCtx.ResponseCode = provider.ExecuteMethod( msg, ref payload, ref options );

                                if(msg.IsGET && messageCtx.ResponseCode != CoAPMessageRaw.Success_WithDetail( CoAPMessageRaw.Detail_Success.Valid ))
                                {
                                    messageCtx.ResponsePayload = payload;
                                }
                                else
                                {
                                    //
                                    // Get-Valid (2.03) or any non-idempotent methods should not generate a payload
                                    //
                                    if(payload != null)
                                    {
                                        Debug.Assert( false, "Responses for non-idempotent methods or GET with a Valid (2.03) response should not carry a payload." );
                                    }
                                }

                                messageCtx.ResponseOptions.Add( options );

                                state = State.ImmediateResponseAvailable;
                            }
                            else
                            {
                                //
                                // There is a resource associated with this query but it is not immediately available.
                                // 
                                processor.ResourceHandler = new ResourceHandler( provider );

                                state = State.DelayedProcessing;
                            }
                        }
                    }
                }

                Advance( state );
            }

            //--//
        }
    }
}
