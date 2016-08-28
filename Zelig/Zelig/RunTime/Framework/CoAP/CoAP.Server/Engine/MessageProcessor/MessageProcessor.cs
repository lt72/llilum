//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System.Threading;
    using CoAP.Common;
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;


    internal partial class MessageProcessor : FixedMessageProcessor
    {
        //
        // State
        // 

        private readonly MessageBuilder   m_messageBuilder;
        private          Timer            m_ackAndLifeTimeTrackingTimer;
        private          int              m_ackRetries;
        private          ResourceHandler  m_resourceHandler;

        //--//

        //
        // Constructors 
        // 

        internal MessageProcessor( MessageContext messageCtx, ProcessingState state, MessageEngine owner ) : base( messageCtx, state, owner )
        {
            m_messageBuilder = MessageBuilder.Create( owner.OriginEndPoints[ 0 ] );
        }

        //
        // Helper methods
        // 

        internal void StartExchangeLifeTimeTrackingTimer( int timeout )
        {
            //
            // Do not track twice!!!
            //
            if(m_ackAndLifeTimeTrackingTimer == null)
            {
                lock(this)
                {
                    if(m_ackAndLifeTimeTrackingTimer != null)
                    {
                        Logger.Instance.LogWarning( $"***[S({this.MessageEngine.LocalEndPoint})]*** EXCHANGE_LIFETIME timer already exists." );
                        return;
                    }

                    //
                    // Create timer object
                    // 
                    var lifeTimeTimer = new Timer( ( obj ) =>
                    {
                        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        // Capture MessageProcessor instance that will process the ACK: it is NOT the instance 
                        // attached to the context at the time we receive the ACK!
                        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                        var proc       = this;
                        var messageCtx = proc.MessageContext;
                        var id         = messageCtx.ResponseAwaitingAck.MessageId;
                        
                        //
                        // If timer timed out, archive...
                        //
                        Logger.Instance.LogWarning( $"***[S({this.MessageEngine.LocalEndPoint})]*** EXCHANGE_LIFETIME for response with message ID '{id}' timed out, archiving..." );

                        var state = MessageProcessor.ProcessingState.Create( MessageProcessor.ProcessingState.State.Archive );

                        proc.State = state;

                        state.SetProcessor( proc );

                        proc.Process( );

                    }, this, Timeout.Infinite, Timeout.Infinite );

                    //
                    // Assign state and start timer
                    // 
                    m_ackAndLifeTimeTrackingTimer = lifeTimeTimer;

                    m_ackAndLifeTimeTrackingTimer.Change( timeout, timeout );

                    Logger.Instance.Log( $"***[S({this.MessageEngine.LocalEndPoint})]*** Started tracking EXCHANGE_LIFETIME for response with message ID '{this.MessageContext.ResponseAwaitingAck.MessageId}'." );
                }
            }
        }


        internal void StartAckTrackingTimer( int timeout )
        {
            //
            // Do not track twice!!!
            //
            if(m_ackAndLifeTimeTrackingTimer == null)
            {
                lock(this)
                {
                    if(m_ackAndLifeTimeTrackingTimer != null)
                    {
                        Logger.Instance.LogWarning( $"***[S({this.MessageEngine.LocalEndPoint})]*** ACK timer already exists." );
                        return;
                    }

                    //
                    // Create timer object
                    // 
                    var ackTimer = new Timer( ( obj ) =>
                    {
                        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        // Capture MessageProcessor instance that will process the ACK: it is NOT the instance 
                        // attached to the context at the time we receive the ACK!
                        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                        var proc       = this;
                        var messageCtx = proc.MessageContext;
                        var id         = messageCtx.ResponseAwaitingAck.MessageId;

                        //Debug.Assert( proc                 == this );
                        //Debug.Assert( messageCtx.Processor == this );

                        //
                        // Timer may have been in fight before it got disabled, so check and bail out in case.
                        // 
                        if(this.MessageEngine.IsAckPending( messageCtx.ResponseAwaitingAck.Context ) == false)
                        {
                            m_ackAndLifeTimeTrackingTimer.Dispose( );

                            Logger.Instance.Log( $"***[S({this.MessageEngine.LocalEndPoint})]*** ACK for response with message ID '{id}' expired after reponse was not tracked." );

                            return;
                        }

                        if(Interlocked.Decrement( ref m_ackRetries ) <= 0)
                        {
                            //
                            // If timer exceeded number of retries, move to error state 
                            //

                            m_ackAndLifeTimeTrackingTimer.Dispose( );

                            Logger.Instance.LogError( $"***[S({this.MessageEngine.LocalEndPoint})]*** ACK for response with message ID '{id}' was not received." );

                            messageCtx.ProtocolError = CoAPMessage.Error.Processing__AckNotReceived;

                            var state = MessageProcessor.ProcessingState.Create( MessageProcessor.ProcessingState.State.Error );

                            proc.State = state;

                            state.SetProcessor( proc );

                            proc.Process( );
                        }
                        else
                        {
                            //
                            // If timer timed out, re-send the message, please note that we need to 
                            // perform this action on the original MessageProcessor instance
                            //
                            Logger.Instance.LogWarning( $"***[S({this.MessageEngine.LocalEndPoint})]*** ACK for response with message ID '{id}' timed out, retrying..." );

                            var state = MessageProcessor.ProcessingState.Create( MessageProcessor.ProcessingState.State.RetransmitDelayedResponse );

                            proc.State = state;

                            state.SetProcessor( proc );

                            proc.Process( );
                        }

                    }, this, Timeout.Infinite, Timeout.Infinite );

                    //
                    // Assign state and start timer
                    // 
                    m_ackRetries                  = TransmissionParameters.MAX_RETRANSMIT;
                    m_ackAndLifeTimeTrackingTimer = ackTimer;

                    m_ackAndLifeTimeTrackingTimer.Change( timeout, Timeout.Infinite );

                    Logger.Instance.Log( $"***[S({this.MessageEngine.LocalEndPoint})]*** Started tracking ACK for response with message ID '{this.MessageContext.ResponseAwaitingAck.MessageId}'." );
                }
            }
            else
            {
                m_ackAndLifeTimeTrackingTimer.Change( timeout * ((TransmissionParameters.MAX_RETRANSMIT - m_ackRetries) * 2), Timeout.Infinite );

                Logger.Instance.Log( $"***[S({this.MessageEngine.LocalEndPoint})]*** Started tracking ACK for response with message ID '{this.MessageContext.ResponseAwaitingAck.MessageId}'." );
            }
        }

        internal void StopAckTrackingTimer( MessageContext messageCtx )
        {
            //
            // Find the MessageContext associated with the token the ACK was sent for and 
            // Dispose the timer associated with this ACK. Please note that it could not 
            // have been created yet. The Message ID of the current incoming message should
            // match the ID of the message whose ACK that is being awaited. 
            //
            var id = messageCtx.Message.MessageId;

            //
            // Find original MessageProcessor whose ACK is being tracked and stop the re-transmission timer
            //
            MessageProcessor proc = null;
            if(this.MessageEngine.TryRemoveAckPending( messageCtx.Message.Context, out proc ))
            {
                Logger.Instance.Log( $"***[S({this.MessageEngine.LocalEndPoint})]*** Stop tracking ACK for response with message ID '{id}'." );
                
                proc.m_ackAndLifeTimeTrackingTimer.Dispose( );
            }
            else
            {
                Logger.Instance.LogError( $"***[S({this.MessageEngine.LocalEndPoint})]*** Bad attempt at stop tracking ACK for response with message ID '{id}'." );
            }
        }

        //
        // Access methods
        //

        internal MessageBuilder MessageBuilder
        {
            get
            {
                return m_messageBuilder;
            }
        }

        protected ResourceHandler ResourceHandler
        {
            get
            {
                return m_resourceHandler;
            }
            set
            {
                m_resourceHandler = value;
            }
        }
        
        //--//

        internal Timer AckTimer
        {
            get
            {
                return m_ackAndLifeTimeTrackingTimer;
            }
        }
    }
}
