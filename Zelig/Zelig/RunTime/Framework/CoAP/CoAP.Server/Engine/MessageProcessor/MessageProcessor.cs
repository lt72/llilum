//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using CoAP.Common;
    using CoAP.Common.Diagnostics;
    using CoAP.Stack;
    using CoAP.Stack.Abstractions;


    public abstract partial class MessageProcessor
    {
        public abstract class ProcessingState
        {
            public enum State
            {
                MessageReceived,
                DelayedProcessing,
                ImmediateResponseAvailable,
                DelayedResponseAvailable,
                RetransmitDelayedResponse,
                SendReset,
                ResetReceived,
                AwaitingAck,
                AckReceived,
                BadOptions,
                Error,
                Archive,
            }

            //
            // State
            //

            protected MessageProcessor m_processor; 

            //--//

            protected ProcessingState( )
            {
            }

            internal static ProcessingState Create( State state )
            {
                // TODO: pool or pre-allocate states
                switch(state)
                {
                    case State.MessageReceived:
                        return ProcessingState_MessageReceived.Get( );
                    case State.ImmediateResponseAvailable:
                        return ProcessingState_ImmediateResponseAvailable.Get( );
                    case State.DelayedProcessing:
                        return ProcessingState_DelayedProcessing.Get( );
                    case State.DelayedResponseAvailable:
                        return ProcessingState_DelayedResponseAvailable.Get( );
                    case State.RetransmitDelayedResponse:
                        return ProcessingState_RetransmitDelayedResponse.Get( );
                    case State.SendReset:
                        return ProcessingState_SendReset.Get( );
                    case State.ResetReceived:
                        return ProcessingState_ResetReceived.Get( );
                    case State.AwaitingAck:
                        return ProcessingState_AwaitingAck.Get( );
                    case State.AckReceived:
                        return ProcessingState__AckReceived.Get( );
                    case State.Archive:
                        return ProcessingState_Archive.Get( );
                    case State.BadOptions:
                        return MessageProcessor__BadOptions.Get( ); 
                    case State.Error:
                        return ProcessingState_Error.Get( );
                    default:
                        throw new InvalidOperationException( );
                }
            }

            //
            // Helper methods
            // 

            public virtual void Process( )
            {
            }

            public void SetProcessor( MessageProcessor processor )
            {
                m_processor = processor;
            }
            
            protected void Advance( ProcessingState.State stateFlag )
            {
                var state = ProcessingState.Create( stateFlag );

                state.SetProcessor( m_processor );

                m_processor.State =  state;

                m_processor.Process( );
            }

            //
            // Access methods
            // 

            protected MessageProcessor Processor
            {
                get
                {
                    return m_processor;
                }
            }
        }

        //--//

        //
        // State
        // 

        private readonly MessageContext   m_messageCtx;
        protected        ProcessingState  m_state;
        private          Timer            m_ackTimer;
        private          int              m_ackRetries;
        private readonly MessageBuilder   m_messageBuilder;
        private readonly MessageEngine    m_owner;

        //--//

        //
        // Constructors 
        // 

        public MessageProcessor( MessageContext messageCtx, ProcessingState state, MessageEngine owner )
        {
            m_messageCtx     = messageCtx;
            m_owner          = owner;
            m_state          = state;
            m_messageBuilder = MessageBuilder.Create( owner.Owner.EndPoints[ 0 ] );
        }

        //
        // Helper methods
        // 

        public abstract void Process( );
        
        public ProcessingState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        public MessageEngine Engine
        {
            get
            {
                return m_owner;
            }
        }

        public MessageBuilder MessageBuilder
        {
            get
            {
                return m_messageBuilder;
            }
        }

        public MessageContext MessageContext
        {
            get
            {
                return m_messageCtx;
            }
        }
                
        public void StartTrackingAck( int timeout )
        {
            //
            // Do not track twice!!!
            //
            if(m_ackTimer == null)
            {
                lock (this)
                {
                    if(m_ackTimer != null)
                    {
                        Logger.Instance.LogWarning( $"***(S)*** ACK timer already exists." );
                        return;
                    }

                    //
                    // Create timer object
                    // 
                    var ackTimer = new Timer( ( obj ) =>
                    {
                        var proc       = this;
                        var messageCtx = proc.MessageContext;
                        var id         = messageCtx.Response.MessageId;

                        //Debug.Assert( proc                 == this );
                        //Debug.Assert( messageCtx.Processor == this );

                        //
                        // Timer may have been in fight before it got disabled, so check and bail out in case.
                        // 
                        if(m_owner.IsAckPending( id ) == false)
                        {
                            m_ackTimer.Dispose( );

                            Logger.Instance.Log( $"***(S)*** ACK for response with message ID '{id}' expired after reponse was not tracked." );

                            return;
                        }

                        if(Interlocked.Decrement( ref m_ackRetries ) <= 0)
                        {
                            //
                            // If timer exceeded number of retries, move to error state 
                            //

                            m_ackTimer.Dispose( );

                            Logger.Instance.LogError( $"***(S)*** ACK for response with message ID '{id}' was not received." );

                            messageCtx.Error = CoAPMessage.Error.Processing__AckNotReceived;

                            proc.State = MessageProcessor.ProcessingState.Create( MessageProcessor.ProcessingState.State.Error );

                            proc.Process( );
                        }
                        else
                        {
                            //
                            // If timer timed out, re-send the message, please note that we need to 
                            // perform this action on the original MessageProcessor instance
                            //
                            Logger.Instance.LogWarning( $"***(S)*** ACK for response with message ID '{id}' timed out, retrying..." );

                            proc.State = MessageProcessor.ProcessingState.Create( MessageProcessor.ProcessingState.State.RetransmitDelayedResponse );

                            proc.Process( );
                        }

                    }, this, Timeout.Infinite, Timeout.Infinite );

                    //
                    // Assign state and start timer
                    // 
                    m_ackRetries = TransmissionParameters.MAX_RETRANSMIT;
                    m_ackTimer   = ackTimer;

                    m_ackTimer.Change( timeout, timeout );

                    Logger.Instance.Log( $"***(S)*** Started tracking ACK for response with message ID '{m_messageCtx.Response.MessageId}'." );
                }
            }
        }

        public void StopTrackingAck( MessageContext messageCtx )
        {
            //
            // Find the MessageContext associated with the token the ACK was sent for and 
            // Dispose the timer associated with this ACK. Please note that it could not 
            // have been created yet. The Message ID of the current incoming message should
            // match the ID of the message whose ACK that is being awaited. 
            //
            var id = messageCtx.Message.MessageId;

            //
            // Find original MessageProcessor whose ACK is beign tracked and stop the re-transmission timer
            //
            MessageProcessor proc = null;
            if(m_owner.TryRemoveAckPending( id, out proc ))
            {
                Logger.Instance.Log( $"***(S)*** Stop tracking ACK for response with message ID '{id}'." );

                //Debug.Assert( messageCtx != null );
                
                proc.m_ackTimer.Dispose( );

                m_state = MessageProcessor.ProcessingState.Create( MessageProcessor.ProcessingState.State.Archive );

                Process( );
            }
            else
            {
                Logger.Instance.LogWarning( $"***(S)*** Bad attempt at stop tracking ACK for response with message ID '{id}'." );
            }
        }

        //
        // Access methods
        //
        
        internal Timer AckTimer
        {
            get
            {
                return m_ackTimer;
            }
        }
    }
}
