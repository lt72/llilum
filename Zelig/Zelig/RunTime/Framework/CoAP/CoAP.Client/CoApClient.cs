//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Client
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using CoAP.Common;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;
    using CoAP.Stack.Abstractions.Messaging;
    using CoAP.Common.Diagnostics;

    public class CoAPClient : IDisposable
    {
        //
        // State
        // 

        private readonly AsyncMessaging m_messaging;
        private readonly bool           m_shared;
        private readonly int            m_timeoutSeconds;
        private readonly int            m_retries;
        private readonly Statistics     m_stats;
        private          IPEndPoint     m_remoteEndPoint;
        private          MessageBuilder m_messageBuilder;
        private          bool           m_disposed;

        //--//

        //
        // Constructors
        //

        public CoAPClient( AsyncMessaging messaging, bool sharedMessaging, int timeoutSeconds, int retries, Statistics stas )
        {
            m_messaging      = messaging;
            m_shared         = sharedMessaging;
            m_timeoutSeconds = timeoutSeconds;
            m_retries        = retries;
            m_stats          = stas;
            m_disposed       = false;
        }

        public CoAPClient( AsyncMessaging messaging, bool sharedMessaging, Statistics stats )
            : this( messaging, sharedMessaging, TransmissionParameters.InitialTimeout, TransmissionParameters.MAX_RETRANSMIT, stats )
        {
        }

        public CoAPClient( AsyncMessaging messaging, Statistics stats )
            : this( messaging, false, TransmissionParameters.InitialTimeout, TransmissionParameters.MAX_RETRANSMIT, stats )
        {
        }


        #region IDisposable Support

        ~CoAPClient( )
        {
            Dispose( false );
        }

        protected virtual void Dispose( bool disposing )
        {
            if(!m_disposed)
            {
                if(disposing)
                {
                    if(m_shared == false)
                    {
                        this.Stop( );
                    }

                    this.Disconnect( );
                }

                m_disposed = true;
            }
        }
        
        public void Dispose( )
        {
            Dispose( true );

            GC.SuppressFinalize(this);
        }

        #endregion

        //
        // Helper methods
        //

        public MessageBuilder Connect( IPEndPoint intermediary, CoAPServerUri uri )
        {
            ThrowIfDisposed( );

            if(m_remoteEndPoint == null)
            {
                m_messageBuilder = MessageBuilder.Create( intermediary, uri ); 

                m_messaging.OnMessage += this.Messaging_IncomingMessageHandler;
                m_messaging.OnError   += this.Messaging_ErrorMessageHandler;

                if(intermediary == null)
                {
                    m_remoteEndPoint = m_messageBuilder.Destination;
                }
                else
                {
                    m_remoteEndPoint = intermediary;
                }
            }

            return (MessageBuilder)m_messageBuilder.Clone( );
        }

        public void Start( )
        {
            m_messaging.Start( );
        }

        public void Stop( )
        {
            m_messaging.Stop( );
        }

        public void Disconnect( )
        {
            m_remoteEndPoint = null;
            
            m_messaging.OnMessage -= this.Messaging_IncomingMessageHandler; 
            m_messaging.OnError   -= this.Messaging_ErrorMessageHandler;
        }

        public CoAPMessage SendReceive( CoAPMessageRaw request )
        {
            ThrowIfDisposed( );

            //
            // Try and not create an inflated message
            //
            CoAPMessageRaw msg = request; // CoAPMessage.FromBuffer( request.Buffer );
            
            var messageCtx = MessageContext.WrapWithContext( msg );

            messageCtx.Source = m_remoteEndPoint;
            
            //
            // Inflate
            //            
            //new MessageParser( ).Parse( msg, m_remoteEndPoint );
            
            //
            // Issue request
            //
            using(var wrHolder = WaitingRecordHolder.WaitResponse( msg ))
            {
                m_stats.RequestsSent++;

                try
                {
                    int timeout = m_timeoutSeconds;
                    int retries = m_retries;

                    CoAPMessage response = null;
                    do
                    {
                        wrHolder.Timeout = timeout;
                        
                        Logger.Instance.Log( $"<==[C({m_messaging.LocalEndPoint})]== Tx message (ID={msg.MessageId},TIMEOUT={timeout},RETRIES={retries}) to {msg.Context.Source}: '{msg}'" ); 

                        m_messaging.SendMessageAsync( msg );

                        response = wrHolder.Response;

                        if(response != null)
                        {
                            break;
                        }

                        if(wrHolder.Waiting && TransmissionParameters.ShouldRetry( ref retries, ref timeout ))
                        {
                            m_stats.RequestsRetransmissions++;

                            continue;
                        }

                        request.Context.ResponseCode = CoAPMessage.ServerError_WithDetail( CoAPMessageRaw.Detail_ServerError.GatewayTimeout );
                        
                        break;

                    } while(true);
                    
                    if(response != null && response.IsConfirmable)
                    {
                        var ack = m_messageBuilder.CreateAck( response, response.Context ).Build( );

                        ack.Context = messageCtx;

                        Logger.Instance.Log( $"<==[C({m_messaging.LocalEndPoint})]== Tx ACK with ID={ack.MessageId} for ID={response.MessageId} to {msg.Context.Source}" );

                        m_stats.AcksSent++;

                        m_messaging.SendMessageAsync( ack );
                    }
                    
                    return response;
                }
                catch
                {
                    Debug.Assert( false );
                    // TODO: what logging?
                }
            }

            return null;
        }

        //
        // Access methods
        //

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return m_remoteEndPoint;
            }
        }

        public Statistics Statistics
        {
            get
            {
                return m_stats;
            }
        }

        //--//

        private void Messaging_IncomingMessageHandler( object sender, HandlerRole role, CoAPMessageEventArgs args )
        {
            var messageCtx = args.MessageContext;

            //
            // Discard all messages that are not generated by the remote endpoint.
            // 
            if(messageCtx.Source.Equals( m_remoteEndPoint ))
            {
                var msg = messageCtx.MessageInflated;

                //
                // Message matching MessageId , token
                // 
                // From rfc 7252, section 5.3.2.Request/Response Matching Rules: 
                //
                //  The exact rules for matching a response to a request are as follows:
                //  1. The source endpoint of the response MUST be the same as the destination 
                //     endpoint of the original request.
                //  2. In a piggybacked response, the Message ID of the Confirmable request and 
                //     the Acknowledgement MUST match, and the tokens of the response and original 
                //     request MUST match.In a separate response, just the tokens of the response 
                //     and original request MUST match.
                //

                var wr = WaitingRecordHolder.Get( msg, msg.IsPiggyBackedResponse );

                if(wr != null)
                {
                    if(msg.IsEmptyAck)
                    {
                        Logger.Instance.Log( $"==[C({m_messaging.LocalEndPoint})]==> Rx ACK ID={msg.MessageId} from {messageCtx.Source}, resetting timeout..." );

                        //
                        // TODO: Should a client reset the timeout when receiving an ACK for a delayed response?
                        // https://github.com/lt72/CoAP-pr/issues/3
                        // 
                        wr.ResetTimeout( );

                        m_stats.AcksReceived++;
                    }
                    else
                    {
                        Logger.Instance.Log( $"==[C({m_messaging.LocalEndPoint})]==> Rx RESPONSE ID={msg.MessageId} from {messageCtx.Source}: '{msg}'" );

                        if(msg.IsDelayedResponse)
                        {
                            m_stats.DelayedResponsesReceived++;
                        }
                        else if(msg.IsReset)
                        {
                            m_stats.ResetsReceived++;
                        }
                        else
                        {
                            m_stats.ImmediateResposesReceived++;
                        }

                        wr.Response = msg;
                    }
                }
                else
                {
                    Logger.Instance.LogError( $"***(C)*** ID={msg.MessageId} is not tracked! Sending RESET" );

                    SendReset( msg, messageCtx );
                }
            }
        }

        private void Messaging_ErrorMessageHandler( object sender, HandlerRole role, CoAPMessageEventArgs args )
        {
            //
            // Discard all messages that are not generated by the remote endpoint.
            // 
            if(args.MessageContext.Source.Equals( m_remoteEndPoint ))
            {
                Logger.Instance.LogError( $"***[C({m_messaging.LocalEndPoint})]*** Received message with error!" );

                m_stats.Errors++;

                var messageCtx = args.MessageContext;
                var error      = args.MessageContext.ProtocolError;

                if(error == CoAPMessageRaw.Error.Parsing__OptionError)
                {
                    //
                    // We can safely grab the message to inspect some basic header properties
                    //
                    var msg = messageCtx.Message;

                    Logger.Instance.LogError( $"***[C({m_messaging.LocalEndPoint})]*** Message ID={msg.MessageId} has bad options!" );

                    // From RFC7252, section 4.2 (Messages Transmitted Reliably): 
                    //
                    // Unrecognized options of class "critical" that occur in a Confirmable 
                    // response, or piggybacked in an Acknowledgement, MUST cause the response 
                    // to be rejected.
                    // 

                    SendReset( msg, messageCtx );

                    //
                    // Alert waiting client
                    //

                    var wr = WaitingRecordHolder.Get( (CoAPMessage)msg, msg.IsPiggyBackedResponse );

                    if(wr != null)
                    {
                        wr.Error    = CoAPMessageRaw.ServerError_WithDetail( CoAPMessageRaw.Detail_ServerError.BadGateway );
                        wr.Response = null;
                    }
                }
            }
        }

        private void SendReset( CoAPMessageRaw msg, MessageContext messageCtx )
        {
            //
            // !!! CAUTION: the message could not be entirely valid, just use the header portion !!!
            //
            if(messageCtx.ProtocolError != CoAPMessageRaw.Error.Parsing__Malformed_NoHeader)
            {
                var reset = m_messageBuilder.CreateResetResponse( msg, messageCtx ).Build( );

                reset.Context = messageCtx;

                Logger.Instance.LogWarning( $"<==[C({m_messaging.LocalEndPoint})]== Sending RESET to {messageCtx.Source}: '{reset}'." );

                m_messaging.SendMessageAsync( reset );
            }
            else
            {
                Logger.Instance.LogError( $"<==[C({m_messaging.LocalEndPoint})]== Should send RESET to {messageCtx.Source} but cannot parse message header." );
            }

            m_stats.ResetsSent++;
        }

        private void ThrowIfDisposed( )
        {
            if(m_disposed)
            {
                throw new ObjectDisposedException( "CoAP.Client.CoAPClient" ); 
            }
        }
    }
}
