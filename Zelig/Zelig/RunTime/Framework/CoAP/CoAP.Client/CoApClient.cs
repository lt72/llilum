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
    using CoAP.Common.Diagnostics;

    public class CoAPClient : IDisposable
    {
        //
        // State
        // 

        private readonly AsyncMessaging m_messaging;
        private readonly Statistics     m_stats;
        private          IPEndPoint     m_remoteEndPoint;
        private          MessageBuilder m_messageBuilder;
        private          bool           m_disposed;

        //--//

        //
        // Constructors
        //

        public CoAPClient( AsyncMessaging messaging, Statistics stas )
        {
            m_messaging = messaging;
            m_stats     = stas;
            m_disposed  = false;
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

        public MessageBuilder Connect( IPEndPoint intermediary, ServerCoAPUri uri )
        {
            if(m_remoteEndPoint == null)
            {
                m_messageBuilder = MessageBuilder.Create( intermediary, uri ); 

                m_messaging.OnMessage += this.Messaging_IncomingMessageHandler;
                m_messaging.OnError   += this.Messaging_ErrorMessageHandler;

                m_messaging.Start( );

                m_remoteEndPoint = m_messageBuilder.Destination;
            }

            return (MessageBuilder)m_messageBuilder.Clone( ); 
        }

        public void Disconnect( )
        {
            m_remoteEndPoint = null;

            m_messaging.Stop( ); 

            m_messaging.OnMessage -= this.Messaging_IncomingMessageHandler; 
            m_messaging.OnError   -= this.Messaging_ErrorMessageHandler;
        }

        public CoAPMessage SendReceive( CoAPMessageRaw request )
        {
            //
            // Try and not create an inflated message
            //
            CoAPMessageRaw msg = request; // CoAPMessage.FromBuffer( request.Buffer );
            
            var messageCtx = new MessageContext( msg );

            messageCtx.Source = m_remoteEndPoint;

            msg.Context = messageCtx;

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
                    int retries = TransmissionParameters.MAX_RETRANSMIT;
                    int timeout = TransmissionParameters.InitialTimeout;

                    CoAPMessage response = null;
                    do
                    {
                        wrHolder.Timeout = timeout;
                        
                        Logger.Instance.Log( $"<==(C)== Sending message to {msg.Context.Source}: '{msg}'" ); 

                        m_messaging.SendMessageAsync( msg, msg.Context );

                        response = wrHolder.Response;

                        if(response != null)
                        {
                            break;
                        }

                        if(TransmissionParameters.ShouldRetry( ref retries, ref timeout ))
                        {
                            m_stats.RequestsRetransmissions++;

                            continue;
                        }

                        break;

                    } while(true);
                    
                    if(response != null && response.IsConfirmable)
                    {
                        var ack = m_messageBuilder.CreateAck( response, messageCtx ).BuildAndReset( );
                        ack.Context = messageCtx;

                        Logger.Instance.Log( $"<==(C)== Sending ACK with Message ID '{ack.MessageId}' for Message ID '{response.MessageId}' to {msg.Context.Source}" );

                        m_stats.AcksSent++;

                        m_messaging.SendMessageAsync( ack, messageCtx );
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
        public Statistics Statistics
        {
            get
            {
                return m_stats;
            }
        }

        //--//

        private void Messaging_IncomingMessageHandler( object sender, CoAPMessageEventArgs args )
        {
            var msg = args.MessageContext.MessageInflated;

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
                    Logger.Instance.Log( $"==(C)==> Received ACK for Message ID '{msg.MessageId}' from {args.MessageContext.Source}, resetting timeout..." );

                    //
                    // TODO: Should a client reset the timeout when receiving an ACK for a delayed response?
                    // https://github.com/lt72/CoAP-pr/issues/3
                    // 
                    wr.ResetTimeout( );

                    m_stats.AcksReceived++;
                }
                else
                {
                    Logger.Instance.Log( $"==(C)==> Received RESPONSE from {args.MessageContext.Source}: '{msg}'" );

                    if(msg.IsDelayedResponse)
                    {
                        m_stats.DelayedResposesesReceived++;
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
                Logger.Instance.LogError( $"***(C)*** Message {msg.MessageId} is not tracked! Sending RESET" );
                
                SendReset( args.MessageContext ); 
            }
        }

        private void Messaging_ErrorMessageHandler( object sender, CoAPMessageEventArgs args )
        {
            m_stats.Errors++;

            var messageCtx = args.MessageContext;
            var msg        = messageCtx.MessageInflated;

            Logger.Instance.LogError( $"***(C)*** Message {msg.MessageId} is in error!" );

            //
            // TODO: process error
            //
            if(msg.HasBadOptions)
            {
                //
                // Unrecognized options of class "critical" that occur in a Confirmable 
                // response, or piggybacked in an Acknowledgement, MUST cause the response 
                // to be rejected (Section 4.2).
                // 

                SendReset( messageCtx );
            }
        }

        private void SendReset( MessageContext context )
        {
            var reset = m_messageBuilder.CreateResetResponse( context ).Build( );

            reset.Context = context;

            Logger.Instance.Log( $"<==(C)== Sending RESET to {context.Source}: '{reset}'" );

            m_messaging.SendMessageAsync( reset, context );

            m_stats.ResetsSent++;
        }
    }
}
