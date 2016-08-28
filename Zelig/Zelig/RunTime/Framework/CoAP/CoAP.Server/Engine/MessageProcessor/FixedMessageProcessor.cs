//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Server
{
    using System;
    using CoAP.Stack;

    internal class FixedMessageProcessor
    {
        internal abstract class ProcessingState
        {
            public enum State
            {
                MessageReceived,
                DelayedProcessing,
                ImmediateResponseAvailable,
                DelayedResponseAvailable,
                SendMessageAndTrackExchangeLifetime,
                RetransmitDelayedResponse,
                ReplayResponse,
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

            private MessageProcessor m_processor;

            //--//

            protected ProcessingState( )
            {
            }

            internal static ProcessingState Create( State state )
            {
                return Create( state, null );
            }

            internal static ProcessingState Create( State state, object context )
            {
                switch(state)
                {
                    case State.MessageReceived:
                        return MessageProcessor.ProcessingState_MessageReceived.Get( );
                    case State.ImmediateResponseAvailable:
                        return MessageProcessor.ProcessingState_ImmediateResponseAvailable.Get( );
                    case State.DelayedProcessing:
                        return MessageProcessor.ProcessingState_DelayedProcessing.Get( );
                    case State.DelayedResponseAvailable:
                        return MessageProcessor.ProcessingState_DelayedResponseAvailable.Get( );
                    case State.RetransmitDelayedResponse:
                        return MessageProcessor.ProcessingState_RetransmitDelayedResponse.Get( );
                    case State.SendMessageAndTrackExchangeLifetime:
                        return MessageProcessor.ProcessingState_SendMessageAndTrackExchangeLifetime.Get( );
                    case State.SendReset:
                        return MessageProcessor.ProcessingState_SendReset.Get( );
                    case State.ResetReceived:
                        return MessageProcessor.ProcessingState_ResetReceived.Get( );
                    case State.AwaitingAck:
                        return MessageProcessor.ProcessingState_AwaitingAck.Get( );
                    case State.AckReceived:
                        return MessageProcessor.ProcessingState__AckReceived.Get( );
                    case State.Archive:
                        return MessageProcessor.ProcessingState_Archive.Get( );
                    case State.BadOptions:
                        return MessageProcessor.MessageProcessor__BadOptions.Get( );
                    case State.Error:
                        return MessageProcessor.ProcessingState_Error.Get( );
                    case State.ReplayResponse:
                        return MessageProcessor.ProcessingState__ReplayResponse.Get( context );
                    default:
                        throw new InvalidOperationException( );
                }
            }

            //
            // Helper methods
            // 

            internal abstract void Process( );

            public void SetProcessor( MessageProcessor processor )
            {
                m_processor = processor;
            }

            protected void Advance( ProcessingState.State stateFlag )
            {
                var state = ProcessingState.Create( stateFlag );

                state.SetProcessor( m_processor );

                m_processor.State = state;

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

        private   readonly MessageContext   m_messageCtx;
        private   readonly MessageEngine    m_owner;
        protected          ProcessingState  m_state;

        //--//

        //
        // Constructors 
        // 

        internal FixedMessageProcessor( MessageContext messageCtx, ProcessingState state, MessageEngine owner )
        {
            m_messageCtx = messageCtx;
            m_owner      = owner;
            m_state      = state;
        }

        internal static AsyncMessageProcessor CreateReplayResponseProcessor( MessageContext ctx, CoAPMessageRaw response, MessageEngine owner )
        {
            var state     = ProcessingState.Create( ProcessingState.State.ReplayResponse, response );
            var processor = new AsyncMessageProcessor( ctx, state, owner );

            state.SetProcessor( processor );

            return processor;
        }

        //
        // Helper methods
        // 

        internal virtual void Process( )
        {
            m_state.Process( );
        }


        //
        // Access methods
        //

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

        protected MessageEngine MessageEngine
        {
            get
            {
                return m_owner;
            }
        }

        protected MessageContext MessageContext
        {
            get
            {
                return m_messageCtx;
            }
        }
    }
}
