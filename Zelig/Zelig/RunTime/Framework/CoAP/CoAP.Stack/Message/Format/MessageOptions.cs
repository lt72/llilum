//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Collections.Generic;
    using System.Text;


    public class MessageOptions : IEncodable
    {
        internal static readonly MessageOptions EmptyOptions = new MessageOptions( );

        //
        // State
        //

        private readonly LinkedList<MessageOption> m_allOptions;
        private readonly List<string>              m_queries;

        //--//

        //
        // Contructors
        //

        public MessageOptions( )
        {
            m_allOptions    = new LinkedList<MessageOption>( );
            m_queries       = new List<string>             ( );
        }

        //
        // Helper Methods
        //

        internal void InsertInOrder( MessageOption option )
        {
            //
            // Insert in order and preserve order of insertion
            // 
            LinkedListNode<MessageOption> node = m_allOptions.First;

            if(node == null)
            {
                m_allOptions.AddFirst( option );
            }
            else
            {
                for(; node != m_allOptions.Last && node.Value.Number <= option.Number; node = node.Next) ;

                if(node.Value.Number <= option.Number)
                {
                    if(node.Value.Number == option.Number)
                    {
                        //
                        // If there is already such option, make sure it is a repeatable one or it has the same value...
                        //
                        if(option.IsRepeatable                     == false &&
                           option.Value.Equals( node.Value.Value ) == false  )
                        {
                            throw new CoAP_MessageFormatException( );
                        }
                    }

                    m_allOptions.AddAfter( node, option );
                }
                else
                {
                    m_allOptions.AddBefore( node, option );
                }
            }
        }

        internal void AppendToBack( MessageOption option )
        {
            if(m_allOptions.Last != null)
            {
                if(m_allOptions.Last.Value.Number > option.Number)
                {
                    throw new CoAP_MessageFormatException( );
                }
            }

            m_allOptions.AddLast( option );

            if(option.IsQuery)
            {
                m_queries.Add( (string)option.Value );
            }
        }

        public void Encode( NetworkOrderBinaryStream stream )
        {
            if(m_allOptions.Count > 0)
            {
                var first = m_allOptions.First;

                //
                // initialize delta
                //
                first.Value.Delta = (int)first.Value.Number;

                for(var node = m_allOptions.First; node != null;)
                {
                    node.Value.Encode( stream );

                    var current = node;

                    node = node.Next;

                    //
                    // update delta
                    //
                    if(node != null)
                    {
                        node.Value.Delta = ((int)node.Value.Number - current.Value.Delta);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

#if DESKTOP
        public override string ToString( )
        {
            var options = m_allOptions;

            var sb = new StringBuilder(); 

            for(var node = m_allOptions.First; node != null; node = node.Next)
            {
                sb.Append( node.Value ); 

                if(node != m_allOptions.Last)
                {
                    sb.Append( " + " ); 
                }
            }
            return $"[{sb}]";
        }
#endif
      
        //
        // Access Methods
        //
        
        public LinkedList<MessageOption> Options
        {
            get
            {
                return m_allOptions;
            }
        }
        
        public List<string> Queries
        {
            get
            {
                return m_queries;
            }
        }

        public int Size
        {
            get
            {
                return ComputeOptionsLength( );
            }
        }

        //--//

        internal void UpdateDeltas( )
        {
            if(m_allOptions.Count > 0)
            {
                var first = m_allOptions.First;

                //
                // initialize delta
                //
                first.Value.Delta = (int)first.Value.Number;

                for(var node = m_allOptions.First; node != null;)
                {
                    var current = node;

                    node = node.Next;

                    //
                    // update delta
                    //
                    if(node != null)
                    {
                        node.Value.Delta = ((int)node.Value.Number - current.Value.Delta);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        internal void Reset( )
        {
            m_allOptions.Clear( );
        }

        private int ComputeOptionsLength( )
        {
            if(m_allOptions.Count == 0)
            {
                return 0;
            }

            int totalLength = m_allOptions.Count;

            for(var node = m_allOptions.First; node != null; node = node.Next)
            {
                var option = node.Value;
                var delta  = (int)option.Delta;

                int valueLength = option.ValueLength;

                totalLength += valueLength;
                
                if(delta > 12)
                {
                    //
                    // we need one or two additional bytes
                    //
                    int delta1 = delta - 13;

                    totalLength += (delta1 <= 255) ? 1 : 2;
                }
                
                if(valueLength > 12)
                {
                    //
                    // we need one or two additional bytes
                    //
                    int valueLength1 = valueLength - 13;

                    totalLength += (valueLength1 <= 255) ? 1 : 2;
                }
            }

            return totalLength;
        }
    }
}

